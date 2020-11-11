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

