// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "thrift/server/TNonblockingServer.h"

using namespace apache::thrift::server;
class ThriftNonBlockingServer
{
private: 
	TNonblockingServer* server;
public:
	ThriftNonBlockingServer();
	~ThriftNonBlockingServer();

	void Start(int port);
};

