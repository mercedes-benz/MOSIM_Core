// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Thrift server that provides the remote access to the Unreal scene. Required by Ajan.
// The methods answerign the client requests are defined in the UnrealSceneAccessServer class,
// which redirects them to the UnrealSceneAccess.

#pragma once

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/core_types.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include "Windows\AllowWindowsPlatformTypes.h"
#include <thrift/protocol/TCompactProtocol.h>
//#include <thrift/protocol/TBinaryProtocol.h>
#include <thrift/transport/TTransportUtils.h>
#include <thrift/transport/TSocket.h>
//#include <thrift/server/TSimpleServer.h> //needed for simpleServer
#include <thrift/server/TThreadPoolServer.h>
#include <thrift/concurrency/ThreadManager.h>
#include <thrift/concurrency/ThreadFactory.h>
#include "Windows\HideWindowsPlatformTypes.h"

using namespace apache::thrift;
using namespace apache::thrift::protocol;
using namespace apache::thrift::transport;
using namespace apache::thrift::server;
using namespace apache::thrift::concurrency;

using namespace MMIStandard;
using namespace std;

namespace SceneAccessServer
{
class UnrealSceneAccessServer;

class RemoteSceneAccessServer
{
public:
    // constructor
    RemoteSceneAccessServer( MIPAddress* address, MIPAddress* registerAddress,
                             shared_ptr<UnrealSceneAccessServer> implementation );

    // destructor
    ~RemoteSceneAccessServer();

    // start the adapter server
    void Start();

    // stop the server
    void Stop();

    MServiceDescription* description;

private:
    shared_ptr<UnrealSceneAccessServer> handler;

    MIPAddress* listenAddress;

    // TSimpleServer* server;
    TThreadPoolServer* server;

    bool isRunning;
};
}