// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Thrift server that provides the remote access to the Unreal scene. Required by Ajan.
// The methods answerign the client requests are defined in the UnrealSceneAccessServer class,
// which redirects them to the UnrealSceneAccess.

#include "RemoteSceneAccessServer.h"
#include "MMUAccess.h"
#include "UnrealSceneAccessServer.h"

using namespace MMIStandard;

namespace SceneAccessServer
{
RemoteSceneAccessServer::RemoteSceneAccessServer(
    MIPAddress* address, MIPAddress* registerAddress,
    shared_ptr<UnrealSceneAccessServer> implementation )
    : description( nullptr ), listenAddress( nullptr )
{
    description = new MServiceDescription();
    description->__set_ID( MMUAccess::GetNewGuid() );
    description->__set_Name( "remoteSceneAccessUnreal" );
    description->__set_Language( "C++" );
    description->__set_Addresses( vector<MIPAddress>{*address} );

    listenAddress = address;
    handler = implementation;
    isRunning = false;
}

RemoteSceneAccessServer::~RemoteSceneAccessServer()
{
    if( this->description )
        delete this->description;

    try
    {
        if( this->isRunning )
            this->server->stop();
        delete this->server;
    }
    catch( ... )
    {
    }
}

void RemoteSceneAccessServer::Start()
{
    auto proc = make_shared<MSceneAccessProcessor>( this->handler );
    auto trans_svr = make_shared<TServerSocket>( this->listenAddress->Port );
    auto trans_fac = make_shared<TBufferedTransportFactory>();
    auto proto_fac = make_shared<TCompactProtocolFactory>();
    // auto proto_fac = make_shared<TBinaryProtocolFactory>();

    // threadmanager for reusing threads
    std::shared_ptr<ThreadManager> threadManager = ThreadManager::newSimpleThreadManager();
    threadManager->threadFactory( std::make_shared<ThreadFactory>() );
    threadManager->start();

    // thread pool server
    this->server = new TThreadPoolServer( proc, trans_svr, trans_fac, proto_fac, threadManager );
    // simple server -> for debugging
    // this->server = new TSimpleServer(proc, trans_svr, trans_fac, proto_fac);

    this->server->serve();

    this->isRunning = true;
}

void RemoteSceneAccessServer::Stop()
{
    try
    {
        if( this->server )
        {
            this->server->stop();
            this->isRunning = false;
        }
    }
    catch( ... )
    {
    }
}
}