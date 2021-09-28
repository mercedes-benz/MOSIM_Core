// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Wrapper class for connecting the RemoteSceneAccessServer with the UnrealSceneAccess.
// RemoteSceneAccess requires a shared_ptr of the class, what conflicts with the garbage collector
// of Unreal, if an Unreal class has a pointer to the class. This makes this class necessary.

#include "UnrealSceneAccessServer.h"
#include "UnrealSceneAccess.h"
#include "MMUAccess.h"
#include "Utils/Logger.h"

#include "RemoteSceneAccessServer.h"

// necessary includes for the termination client
#include "Windows\AllowWindowsPlatformTypes.h"
#include <thrift/transport/TTransportUtils.h>
#include <thrift/protocol/TCompactProtocol.h>
#include <thrift/transport/TSocket.h>
#include "Windows\HideWindowsPlatformTypes.h"

// required namespaces for the thrift client
using namespace apache::thrift;
using namespace apache::thrift::transport;
using namespace apache::thrift::protocol;

namespace SceneAccessServer
{
UnrealSceneAccessServer::UnrealSceneAccessServer( UnrealSceneAccess* _SceneAccess )
    : SceneAccess( _SceneAccess ),
      remoteSceneAccessServer( nullptr ),
      settings( new MMISettings() ),
      remoteSceneAccessServerThread( nullptr ) /*,
         remoteSceneManipulationServer(nullptr)*/
{
}

UnrealSceneAccessServer::~UnrealSceneAccessServer()
{
    if( this->remoteSceneAccessServerThread )
    {
        this->TerminationThriftClient();

        if( this->remoteSceneAccessServerThread->joinable() )
        {
            this->remoteSceneAccessServerThread->join();
            delete this->remoteSceneAccessServerThread;
        }
    }
    // if (this->remoteSceneManipulationServer)
    //	delete this->remoteSceneManipulationServer;
}

void UnrealSceneAccessServer::InitializeServers()
{
    auto accessLambda = [this]() {
        this->remoteSceneAccessServer = new RemoteSceneAccessServer{
            &this->SceneAccess->settings->MIPRemoteSceneAccessAddress,
            &this->SceneAccess->settings->MIPRegisterAddress, shared_from_this()};
        this->remoteSceneAccessServer->Start();
    };

    this->remoteSceneAccessServerThread = new thread( accessLambda );
}

void UnrealSceneAccessServer::ApplyUpdates( MBoolResponse& _return,
                                            const MSceneUpdate& sceneUpdates )
{
    this->SceneAccess->ApplyUpdates( _return, sceneUpdates );
}

void UnrealSceneAccessServer::ApplyManipulations(
    MBoolResponse& _return, const vector<MSceneManipulation>& sceneManipulations )
{
    this->SceneAccess->ApplyManipulations( _return, sceneManipulations );
}

void UnrealSceneAccessServer::GetSceneObjects( vector<MSceneObject>& _return )
{
    this->SceneAccess->GetSceneObjects( _return );
}

void UnrealSceneAccessServer::GetSceneObjectByID( MSceneObject& _return, const string& id )
{
    this->SceneAccess->GetSceneObjectByID( _return, id );
}

void UnrealSceneAccessServer::GetSceneObjectByName( MSceneObject& _return, const string& name )
{
    this->SceneAccess->GetSceneObjectByName( _return, name );
}

void UnrealSceneAccessServer::GetSceneObjectsInRange( vector<MSceneObject>& _return,
                                                      const MVector3& position, const double range )
{
    this->SceneAccess->GetSceneObjectsInRange( _return, position, range );
}

void UnrealSceneAccessServer::GetColliders( vector<MCollider>& _return )
{
    this->SceneAccess->GetColliders( _return );
}

void UnrealSceneAccessServer::GetColliderById( MCollider& _return, const string& id )
{
    this->SceneAccess->GetColliderById( _return, id );
}

void UnrealSceneAccessServer::GetCollidersInRange( vector<MCollider>& _return,
                                                   const MVector3& position, const double range )
{
    this->SceneAccess->GetCollidersInRange( _return, position, range );
}

void UnrealSceneAccessServer::GetMeshes( vector<MMesh>& _return )
{
    this->SceneAccess->GetMeshes( _return );
}

void UnrealSceneAccessServer::GetMeshByID( MMesh& _return, const string& id )
{
    this->SceneAccess->GetMeshByID( _return, id );
}

void UnrealSceneAccessServer::GetTransforms( vector<MTransform>& _return )
{
    this->SceneAccess->GetTransforms( _return );
}

void UnrealSceneAccessServer::GetTransformByID( MTransform& _return, const string& id )
{
    this->SceneAccess->GetTransformByID( _return, id );
}

void UnrealSceneAccessServer::GetAvatars( vector<MAvatar>& _return )
{
    this->SceneAccess->GetAvatars( _return );
}

void UnrealSceneAccessServer::GetAvatarByID( MAvatar& _return, const string& id )
{
    this->SceneAccess->GetAvatarByID( _return, id );
}

void UnrealSceneAccessServer::GetAvatarByName( MAvatar& _return, const string& name )
{
    this->SceneAccess->GetAvatarByName( _return, name );
}

void UnrealSceneAccessServer::GetAvatarsInRange( vector<MAvatar>& _return, const MVector3& position,
                                                 const double distance )
{
    this->SceneAccess->GetAvatarsInRange( _return, position, distance );
}

double UnrealSceneAccessServer::GetSimulationTime()
{
    return this->SceneAccess->GetSimulationTime();
}

void UnrealSceneAccessServer::GetNavigationMesh( MNavigationMesh& _return )
{
    Logger::printLog( L_ERROR, " GetNavigationMesh is not implemented so far." );
    runtime_error( "GetNavigationMesh is not implemented so far." );
}

void UnrealSceneAccessServer::GetData( string& _return, const string& fileFormat,
                                       const string& selection )
{
    Logger::printLog( L_ERROR, " GetData is not implemented so far." );
    runtime_error( "GetData is not implemented so far." );
}

void UnrealSceneAccessServer::GetAttachments( vector<MAttachment>& _return )
{
    this->SceneAccess->GetAttachments( _return );
}

void UnrealSceneAccessServer::GetAttachmentsByID( vector<MAttachment>& _return, const string& id )
{
    this->SceneAccess->GetAttachmentsByID( _return, id );
}

void UnrealSceneAccessServer::GetAttachmentsByName( vector<MAttachment>& _return,
                                                    const string& name )
{
    this->SceneAccess->GetAttachmentsByName( _return, name );
}

void UnrealSceneAccessServer::GetAttachmentsChildrenRecursive( vector<MAttachment>& _return,
                                                               const string& id )
{
    Logger::printLog( L_ERROR, " GetAttachmentsChildrenRecursive is not implemented so far." );
    runtime_error( "GetAttachmentsChildrenRecursive is not implemented so far." );
}

void UnrealSceneAccessServer::GetAttachmentsParentsRecursive( vector<MAttachment>& _return,
                                                              const string& id )
{
    Logger::printLog( L_ERROR, " GetAttachmentsParentsRecursive is not implemented so far." );
    runtime_error( "GetAttachmentsParentsRecursive is not implemented so far." );
}

void UnrealSceneAccessServer::GetStatus( map<string, string>& _return )
{
    _return.insert( pair<string, string>{"Running", "True"} );
}

void UnrealSceneAccessServer::GetDescription( MServiceDescription& _return )
{
    if( this->remoteSceneAccessServer )
    {
        _return = *this->remoteSceneAccessServer->description;
    }
}

void UnrealSceneAccessServer::Setup( MBoolResponse& _return, const MAvatarDescription& avatar,
                                     const map<string, string>& properties )
{
    Logger::printLog( L_ERROR, " Setup is not implemented so far." );
    runtime_error( "Setup is not implemented so far." );
}

void UnrealSceneAccessServer::Consume( map<string, string>& _return,
                                       const map<string, string>& properties )
{
    Logger::printLog( L_ERROR, " Consume is not implemented so far." );
    runtime_error( "Consume is not implemented so far." );
}

void UnrealSceneAccessServer::Dispose( MBoolResponse& _return,
                                       const map<string, string>& properties )
{
    try
    {
        if( this->remoteSceneAccessServer != nullptr )
        {
            this->remoteSceneAccessServer->Stop();
        }
        // if (this->remoteSceneManipulationServer != nullptr)
        //{
        //	this->remoteSceneManipulationServer->Stop();
        //}
        _return.__set_Successful( true );
    }
    catch( ... )
    {
        // catch strange exceptions;
    }
}

void UnrealSceneAccessServer::Restart( MBoolResponse& _return,
                                       const map<string, string>& properties )
{
    Logger::printLog( L_ERROR, " Restart is not implemented so far." );
    runtime_error( "Restart is not implemented so far." );
}

void UnrealSceneAccessServer::GetSceneChanges( MSceneUpdate& _return )
{
    _return = this->SceneAccess->SceneUpdate;
}

void UnrealSceneAccessServer::GetFullScene( MSceneUpdate& _return )
{
    this->SceneAccess->GetFullScene( _return );
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// method constructing thrift client for terminating thrift servers running in seperate threads

void UnrealSceneAccessServer::TerminationThriftClient()
{
    shared_ptr<TTransport> socket(
        new TSocket( this->settings->MIPRemoteSceneAccessAddress.Address,
                     this->settings->MIPRemoteSceneAccessAddress.Port ) );
    shared_ptr<TTransport> transport = shared_ptr<TTransport>( new TBufferedTransport( socket ) );
    shared_ptr<TProtocol> protocol( new TCompactProtocol{transport} );
    shared_ptr<MSceneAccessClient> access =
        shared_ptr<MSceneAccessClient>( new MSceneAccessClient{protocol} );

    // start the thrift client
    try
    {
        if( !transport->isOpen() )
            transport->open();
    }
    catch( exception e )
    {
        string errorMessage = e.what();
        Logger::printLog( L_ERROR, errorMessage );  // TODO: define error message
    }

    // dispose the client
    map<string, string> properties = map<string, string>();
    MBoolResponse returnBool = MBoolResponse();
    access->Dispose( returnBool, properties );

    // close the thrift connection again
    transport->close();
}
}