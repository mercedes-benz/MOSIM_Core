// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// This class provides the methods for handling the connection to the Adapters.

#include "RemoteAdapterAccess.h"
#include "MMUAccess.h"
#include "MotionModelUnitAccess.h"
#include <iostream>
#include <thread>
#include <chrono>
#include "Utils/Logger.h"

#include "Windows\AllowWindowsPlatformTypes.h"
#include "ThriftClient/ThriftClient.cpp"
#include "Windows\HideWindowsPlatformTypes.h"

// constructor
RemoteAdapterAccess::RemoteAdapterAccess( string address, int port,
                                          MAdapterDescription adapterDescription,
                                          MMUAccess* mmuAccess )
    : mmuAccess( mmuAccess ),
      Description( adapterDescription ),
      thriftClient( address, port ),
      Address( address ),
      Port( port ),
      Initialized( false ),
      Aborted( false ),
      SceneSynchronized( false ),
      Loaded( false )
{
}

// destructor
RemoteAdapterAccess::~RemoteAdapterAccess()
{
    // TODO_IMPORTANT: are adpaters correctly disposed?
    // MBoolResponse _return = this->CloseConnection();

    // close the thrift connection again
    // unrequired, as the thriftClient is stored in the classes stack and thus delted anyway when
    // the scope of the class ends
    // would have been necessary if a pointer is applied
    // this->thriftClient.~ThriftClient();
}

void RemoteAdapterAccess::Start()
{
    // Create new adapter client
    this->thriftClient = ThriftClient<MMIAdapterClient>{this->Address, this->Port};
    this->thriftClient.Start();

    // try to connect the adapter until status is available
    while( !this->Initialized && !this->Aborted )
    {
        try
        {
            map<string, string> status = this->GetStatus();
            this->Initialized = true;
        }
        catch( exception e )
        {
            Logger::printLog( L_ERROR, " Failed to start remote adapter." );
            std::cout << "Failed to start remote adapter, message: " << e.what() << endl;
        }
    }
}

// Returns all mmus which are available at the assigned adapter and for the given session Id
vector<MotionModelUnitAccess*> RemoteAdapterAccess::CreateMMUConnections(
    string sessionId, vector<MMUDescription> mmuDescriptions )
{
    // get the available MMUs
    vector<MMUDescription> availableMMUs;
    this->thriftClient.access->GetMMus( availableMMUs, sessionId );

    if( availableMMUs.empty() )
        throw runtime_error( "Tcp server not available" );

    vector<MotionModelUnitAccess*> result( availableMMUs.size() );
    for( int i = 0; i < availableMMUs.size(); i++ )
    {
        // Create a new MMU connection Instance
        result[i] = new MotionModelUnitAccess( this->mmuAccess, this, this->mmuAccess->SessionId,
                                               availableMMUs[i] );
    }
    return result;
}

MBoolResponse RemoteAdapterAccess::PushScene( MSceneUpdate sceneUpdates, string sessionId )
{
    MBoolResponse _return;
    this->thriftClient.access->PushScene( _return, sceneUpdates, sessionId );
    return _return;
}

vector<MSceneObject> RemoteAdapterAccess::GetScene( string sessionId )
{
    vector<MSceneObject> _return;
    this->thriftClient.access->GetScene( _return, sessionId );
    return _return;
}

MSceneUpdate RemoteAdapterAccess::GetSceneChanges( string sessionId )
{
    MSceneUpdate _return;
    this->thriftClient.access->GetSceneChanges( _return, sessionId );
    return _return;
}

MBoolResponse RemoteAdapterAccess::CloseConnection()
{
    MBoolResponse _return;
    try
    {
        this->thriftClient.~ThriftClient();
        _return.__set_Successful( true );
    }
    catch( ... )
    {
        _return.__set_Successful( false );
    }
    return _return;
}

MBoolResponse RemoteAdapterAccess::CreateSession( string sessionId,
                                                  MAvatarDescription referenceAvatar )
{
    MBoolResponse _return;
    this->thriftClient.access->CreateSession( _return, sessionId );
    return _return;
}

MBoolResponse RemoteAdapterAccess::CloseSession( string sessionId )
{
    MBoolResponse _return;
    this->thriftClient.access->CloseSession( _return, sessionId );
    return _return;
}

MBoolResponse RemoteAdapterAccess::LoadMMUs( vector<string> ids, string sessionId )
{
    map<string, string> response;
    this->thriftClient.access->LoadMMUs( response, ids, sessionId );
    MBoolResponse _return;
    if( response.size() == ids.size() )
    {
        this->Loaded = true;
        _return.__set_Successful( true );
        return _return;
    }
    else
    {
        _return.__set_Successful( false );
        return _return;
    }
}

map<string, string> RemoteAdapterAccess::GetStatus()
{
    map<string, string> _return;
    this->thriftClient.access->GetStatus( _return );
    return _return;
}

string RemoteAdapterAccess::CreateCheckpoint( string mmuID, string checkpointId )
{
    string _return;
    this->thriftClient.access->CreateCheckpoint( _return, mmuID, checkpointId );
    return _return;
}

MBoolResponse RemoteAdapterAccess::RestoreCheckpoint( string mmuID, string checkpointID,
                                                      string checkPointData )
{
    MBoolResponse _return;
    this->thriftClient.access->RestoreCheckpoint( _return, mmuID, this->mmuAccess->SessionId,
                                                  checkPointData );
    return _return;
}

// getters and setters

const MAdapterDescription* RemoteAdapterAccess::GetDescription()
{
    return &this->Description;
}

void RemoteAdapterAccess::SetDescription( const MAdapterDescription& _description )
{
    this->Description = _description;
}

string RemoteAdapterAccess::GetAdapterName()
{
    return this->Description.Name;
}

vector<MMUDescription> RemoteAdapterAccess::GetLoadableMMUs()
{
    return this->MMUDescriptions;
}

vector<MMUDescription> RemoteAdapterAccess::GetLoadableMMUsClient()
{
    this->thriftClient.access->GetLoadableMMUs( this->MMUDescriptions );
    return this->MMUDescriptions;
}