// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam, Janis Sprenger

// This class provides the methods for connecting to the Register and
// loading and initializing MMUs. Summarizes the methods from
// RemoteAdapterAccess and MotionModelUnitAccess.

#include "MMUAccess.h"
#include "MOSIM.h"
#include <Combaseapi.h>
#include "RemoteAdapterAccess.h"
#include <iostream>
#include <concurrent_unordered_map.h>
#include <chrono>
#include "Utils/Logger.h"
#include <algorithm>

MMUAccess::MMUAccess()
{
    // create the session ID
    this->SessionId = MMUAccess::GetNewGuid();
}

MMUAccess::MMUAccess( std::string sessionId )
{
    this->SessionId = sessionId;
}

// destructor
MMUAccess::~MMUAccess()
{
    // Dispose every mmu
    int count = 0;
    // Dispose every MMU and delete MMU iface
    vector<MBoolResponse> boolResponses;
    for( MotionModelUnitAccess* mmu : this->MotionModelUnits )
    {
        MBoolResponse boolResponse;
        map<string, string> properties;
        // avoid access to nullptr
        if( mmu )
        {
            // TODO_IMPORTANT: call does not work at the moment --> perhaps issue in the MOSIM
            // framework --> discuss disposing the CoSimulationMMU
            // mmu->Dispose(boolResponse, properties);
            // boolResponses.push_back(boolResponse);
            // delete the MotionModelUnitAccess;
            delete mmu;
            count++;
            // if all MotionModelUnitAccesses are deleted, delete the vector
            if( this->MotionModelUnits.size() == count )
                this->MotionModelUnits.clear();
        }
    }

    // dispose every adapter
    count = 0;
    for( RemoteAdapterAccess* adapter : this->Adapters )
    {
        // avoid access to nullptr
        if( adapter )
        {
            // close the connection
            adapter->CloseSession( this->SessionId );
            // delete the RemoteAdapterAccess
            delete adapter;
            count++;
            // if all RemoteAdapterAccesses are deleted, delete the vector
            if( this->Adapters.size() == count )
                this->Adapters.clear();
        }
    }
}

vector<const MAdapterDescription*> MMUAccess::GetAdapterDescriptions()
{
    vector<const MAdapterDescription*> AdapterDescriptions;
    for( int i = 0; i < Adapters.size(); i++ )
    {
        AdapterDescriptions.push_back( Adapters[i]->GetDescription() );
    }
    return AdapterDescriptions;
}

bool MMUAccess::IsLoaded()
{
    if( this->Adapters.empty() )
        return false;
    else
        return all_of( this->Adapters.begin(), this->Adapters.end(),
                       []( RemoteAdapterAccess* adapter ) { return adapter->GetInitialized(); } );
}

// connect the given adapters
bool MMUAccess::Connect( const MIPAddress& mmiRegisterAddress, const int maxTime,
                         const string& _avatarID, MAvatarDescription description )
{
    // create a list of adapter descriptions
    // local copy of the thrift client is delted when the methods scope ends -> thrift connection is
    // then terminated
    ThriftClient<MMIRegisterServiceClient> client( mmiRegisterAddress.Address,
                                                   mmiRegisterAddress.Port, true );

    bool adapterDescriptionReceived = false;
    for( int i = 0; i <= maxTime; i++ )
    {
        try
        {
            // get the adapter names
            client.access->GetRegisteredAdapters( this->adapterDescriptions, this->SessionId );
            adapterDescriptionReceived = true;
            break;
        }
        catch( exception e )
        {
            string message = "Cannot connect to mmi register";
            Logger::printLog( L_ERROR, message );
            std::cout << "Cannot connect to mmi register, message: " << e.what() << endl;
            this_thread::sleep_for( chrono::seconds( 1 ) );
            if( i == maxTime )
            {
                adapterDescriptionReceived = false;
                break;
            }
        }
    }

    if( !adapterDescriptionReceived )
        return false;

    // access the adapters
    bool adapterAlreadyAvail;
    for( auto adaptDesc : adapterDescriptions )
    {
        // skip the adapters that are already available
        adapterAlreadyAvail = false;
        for( auto loadedAdaptDesc : this->Adapters )
        {
            if( *loadedAdaptDesc->GetDescription() == adaptDesc )
            {
                cout << "Adapter " << adaptDesc.Name << "is already available" << endl;
                adapterAlreadyAvail = true;
                break;
            }
        }
        if( !adapterAlreadyAvail )
        {
            string adapterAddress = adaptDesc.Addresses[0].Address;
            RemoteAdapterAccess* remoteAdapterAccess = new RemoteAdapterAccess(
                adapterAddress, adaptDesc.Addresses[0].Port, adaptDesc, this );
            this->Adapters.push_back( remoteAdapterAccess );
        }
    }

    Concurrency::concurrent_unordered_map<IAdapter*, bool> connectedAdapters;

    vector<thread*> threads;
    for( RemoteAdapterAccess* adapter : this->Adapters )
    {
        const MAdapterDescription* adaptDescPtr = adapter->GetDescription();
        string adaptName = adaptDescPtr->Name;
        threads.push_back( new thread(
            [this, &connectedAdapters, description]( IAdapter* adapter ) {
                try
                {
                    adapter->Start();
                    adapter->CreateSession( this->SessionId, description );
                    connectedAdapters.insert( pair<IAdapter*, bool>( adapter, true ) );
                }
                catch( exception e )
                {
                    Logger::printLog( L_ERROR, " Cannot connect to adapters" );
                    std::cout << "Cannot connect to adapters, message: " << e.what() << endl;
                }
            },
            adapter ) );
    }
    // control threads
    MMUAccess::StopThreads( threads );

    // Remove all adapters not being connected (therfore iterate over all added adapters)
    for( int i = this->Adapters.size() - 1; i >= 0; i-- )
    {
        // Remove the adapter if not connected
        if( connectedAdapters.find( this->Adapters[i] ) == connectedAdapters.end() )
        {
            // Close the connection
            this->Adapters[i]->CloseConnection();
        }
    }

    // delete entries in vector
    for( auto it = this->Adapters.begin(); it != this->Adapters.end(); )
    {
        if( connectedAdapters.find( *it ) == connectedAdapters.end() )
        {
            // delete the adapter
            delete *it;
            // remove the adapter pointer from the vector
            it = this->Adapters.erase( it );
        }
        else
            ++it;
    }

    // Return true if at least one adapter is connected
    if( this->Adapters.size() > 0 )
        return true;
    else
        return false;
}

vector<MMUDescription> MMUAccess::GetLoadableMMUsClient()
{
    // get the loadable MMUs from the adapters
    for( RemoteAdapterAccess* adapter : this->Adapters )
    {
        vector<MMUDescription> mmuDescriptions = adapter->GetLoadableMMUsClient();
        // add to the list
        for( auto mmuDesc : mmuDescriptions )
        {
            // loadableMMUs.push_back(mmuDesc);
            this->MMUDescriptions.push_back( mmuDesc );
        }
    }
    return this->MMUDescriptions;
}

bool MMUAccess::LoadAllMMUs( vector<MMUDescription> mmuList, int timeout )
{
    typedef Concurrency::concurrent_unordered_map<IAdapter*, vector<MotionModelUnitAccess*>>
        AdapterMMUAccesses;
    AdapterMMUAccesses adapterMMUAccesses;
    try
    {
        for( RemoteAdapterAccess* adapter : this->Adapters )
        {
            // get the MMU IDs
            vector<string> mmuIds;
            vector<MMUDescription> loadableMMUs = adapter->GetLoadableMMUs();
            for( MMUDescription mmuDesc : loadableMMUs )
            {
                mmuIds.push_back( mmuDesc.ID );
            }
            // load MMUs based on their ID
            MBoolResponse boolResponse = adapter->LoadMMUs( mmuIds, this->SessionId );
            if( !boolResponse.Successful )
            {
                string msgs = "Problems loading MMUs in " + adapter->GetAdapterName() + "!";
                throw MOSIMException( msgs );
            }
            // add the loaded mmus to the concurrent map
            vector<MotionModelUnitAccess*> mmuAccesses =
                adapter->CreateMMUConnections( this->SessionId, this->MMUDescriptions );
            adapterMMUAccesses.insert(
                pair<IAdapter*, vector<MotionModelUnitAccess*>>( adapter, mmuAccesses ) );
        }

        // add the mmus to the global list (already implemented for parallel threading)
        // loop over the elements, create iterator pointing to the start of the map
        Concurrency::concurrent_unordered_map<IAdapter*, vector<MotionModelUnitAccess*>>::iterator
            it = adapterMMUAccesses.begin();
        while( it != adapterMMUAccesses.end() )
        {
            // add the entries in the map to the general vector
            for( MotionModelUnitAccess* mmuAccess : it->second )
            {
                this->MotionModelUnits.push_back( mmuAccess );
            }
            it++;
        }
        return true;
    }
    catch( exception& e )
    {
        cout << e.what() << endl;
        string loggerMsg( e.what() );
        Logger::printLog( L_ERROR, " " + loggerMsg );
        return false;
    }
}

bool MMUAccess::LoadSpecificMMU( const MMUDescription& mmuDesc, int timeout )
{
    try
    {
        // find the adapter hosting the MMU
        RemoteAdapterAccess* adapterPtr = nullptr;

        for( RemoteAdapterAccess* adapter : this->Adapters )
        {
            const vector<MMUDescription>& allLoadableMMUs = adapter->GetLoadableMMUs();
            if( find( allLoadableMMUs.begin(), allLoadableMMUs.end(), mmuDesc ) !=
                allLoadableMMUs.end() )
            {
                adapterPtr = adapter;
                break;
            }
        }
        if( adapterPtr == nullptr )
            return false;

        // load MMU based on the ID
        vector<string> mmuID = {mmuDesc.ID};
        MBoolResponse boolResponse = adapterPtr->LoadMMUs( mmuID, this->SessionId );
        if( !boolResponse.Successful )
        {
            string msgs = "Problems loading MMUs in " + adapterPtr->GetAdapterName() + "!";
            throw MOSIMException( msgs );
        }
        // create MMUAccess for specific MMU
        vector<MMUDescription> mmuDescVector = {mmuDesc};
        vector<MotionModelUnitAccess*> mmuAccess =
            adapterPtr->CreateMMUConnections( this->SessionId, mmuDescVector );
        this->MotionModelUnits.push_back( mmuAccess[0] );
        return true;
    }
    catch( exception& e )
    {
        cout << e.what() << endl;
        string loggerMsg( e.what() );
        Logger::printLog( L_ERROR, " " + loggerMsg );
        return false;
    }
}

bool MMUAccess::InitializeAllMMUs( int timeout, const MAvatarDescription& _AvatarDesc )
{
    if( !this->IsLoaded() )
        return false;

    map<string, string> initializationProperties;

    // TODO: check, if the required arguments in the loop below are needed in other functions
    bool success;
    try
    {
        // use several threads for initializing the MMUs
        vector<thread*> threads;
        // generate mutex for control write access to the initializationProperties
        mutex mtx;
        auto lambdExp = [this, &initializationProperties, _AvatarDesc,
                         &mtx]( MotionModelUnitAccess* mmu ) {
            MBoolResponse boolResponse;
            mtx.lock();
            mmu->Initialize( boolResponse, _AvatarDesc, initializationProperties );
            mtx.unlock();
        };
        for( MotionModelUnitAccess* mmu : this->MotionModelUnits )
        {
            threads.push_back( new thread( lambdExp, mmu ) );
        }
        MMUAccess::StopThreads( threads );
        success = true;
    }
    catch( ... )
    {
        success = false;
    }

    if( success )
    {
        this->isInitialized = true;
    }

    return success;
}

bool MMUAccess::InitializeSpecificMMU( int timeout, const MAvatarDescription& AvatarDescription,
                                       MotionModelUnitAccess* mmu,
                                       const map<string, string>& initializationProperties )
{
    bool success;
    try
    {
        MBoolResponse boolResponse;
        mmu->Initialize( boolResponse, AvatarDescription, initializationProperties );
        success = true;
    }
    catch( const exception& e )
    {
        success = false;
        throw e;
    }

    return success;
}

void MMUAccess::PushScene( bool transmitFullScene )
{
    MSceneUpdate sceneUpdates;

    if( !transmitFullScene )
        this->SceneAccess->GetSceneChanges( sceneUpdates );
    else
        this->SceneAccess->GetFullScene( sceneUpdates );

    auto lambdaExp = [this, sceneUpdates]( RemoteAdapterAccess* adapter ) {
        adapter->SetSceneSynchronized( false );
        adapter->PushScene( sceneUpdates, this->SessionId );
        adapter->SetSceneSynchronized( true );
    };

    vector<thread*> threads;

    for( RemoteAdapterAccess* adapter : this->Adapters )
    {
        threads.push_back( new thread( lambdaExp, adapter ) );
    }
    MMUAccess::StopThreads( threads );
}

// create checkpoints
map<string, string> MMUAccess::CreateCheckpoint( vector<string> mmuIDs )
{
    map<string, string> checkpointData;
    for( MotionModelUnitAccess* mmu : this->MotionModelUnits )
    {
        if( find( mmuIDs.begin(), mmuIDs.end(), mmu->getID() ) != mmuIDs.end() )
        {
            string newCheckpointStr;
            mmu->CreateCheckpoint( newCheckpointStr );
            checkpointData.insert( pair<string, string>( mmu->getID(), newCheckpointStr ) );
        }
    }
    return checkpointData;
}

// restore checkpoints
void MMUAccess::RestoreCheckpoint( map<string, string> checkpointData )
{
    vector<MBoolResponse> boolResponses;
    MBoolResponse boolResponse;
    for( MotionModelUnitAccess* mmu : this->MotionModelUnits )
    {
        string mmuID = mmu->getID();

        if( checkpointData.find( mmuID ) != checkpointData.end() )
        {
            mmu->RestoreCheckpoint( boolResponse, checkpointData[mmuID] );
            boolResponses.push_back( boolResponse );
        }
    }
}

vector<MSceneObject> MMUAccess::FetchScene()
{
    vector<MSceneObject> sceneObjects;

    if( !this->Adapters.empty() )
    {
        sceneObjects = this->Adapters[0]->GetScene( this->SessionId );
    }

    return sceneObjects;
}

MSceneUpdate MMUAccess::FetchSceneChanges()
{
    MSceneUpdate SceneUpdate;

    if( !this->Adapters.empty() )
    {
        SceneUpdate = this->Adapters[0]->GetSceneChanges( this->SessionId );
    }

    return SceneUpdate;
}

// static Methods

string MMUAccess::GetNewGuid()
{
    UUID uuid = {0};
    string guid;

    // Create uuid or load from a string by UuidFromString() function
    ::UuidCreate( &uuid );

    // If you want to convert uuid to string, use UuidToString() function
    RPC_CSTR szUuid = NULL;
    if(::UuidToStringA( &uuid, &szUuid ) == RPC_S_OK )
    {
        guid = (char*)szUuid;
        ::RpcStringFreeA( &szUuid );
    }

    return guid;
}

void MMUAccess::StopThreads( vector<thread*>& threadVector )
{
    for( thread* threadInstance : threadVector )
    {
        try
        {
            threadInstance->join();
            delete threadInstance;
        }
        catch( exception e )
        {
            // TODO: implement functionality
        }
    }
}
