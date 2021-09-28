// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "ThriftServer.h"
#include <thrift/protocol/TCompactProtocol.h>
#include <thrift/transport/TTransportUtils.h>
#include <iostream>
#include "Adapter/ThriftAdapterImplementation.h"
#include <thrift/transport/TSocket.h>
//#include <thrift/server/TSimpleServer.h> //needed for simpleServer
#include <thrift/server/TThreadPoolServer.h>
#include <thrift/concurrency/ThreadManager.h>
#include <thrift/concurrency/ThreadFactory.h>

using namespace std;
using namespace apache::thrift;
using namespace apache::thrift::protocol;
using namespace apache::thrift::transport;
using namespace apache::thrift::server;
using namespace apache::thrift::concurrency;


class MMIAdapterCloneFactory : virtual public MMIAdapterIfFactory
{
	// Inherited via MMIAdapterIfFactory
	virtual MMIAdapterIf * getHandler(const::apache::thrift::TConnectionInfo & connInfo) override
	{
		std::shared_ptr<TSocket> sock = std::dynamic_pointer_cast<TSocket>(connInfo.transport);
		//TODO for debugging maybe include in debugging
		/*cout << "Incoming connection\n";
		cout << "\tSocketInfo: " << sock->getSocketInfo() << "\n";
		cout << "\tPeerHost: " << sock->getPeerHost() << "\n";
		cout << "\tPeerAddress: " << sock->getPeerAddress() << "\n";
		cout << "\tPeerPort: " << sock->getPeerPort() << "\n";*/
		return new ThriftAdapterImplementation{};
	}

	virtual void releaseHandler(MMIAdapterIf * handler) override
	{
		delete handler;
	}
};

ThriftServer::~ThriftServer()
{
	try
	{
		this->server->stop();
		delete this->server ;
	}
	catch (...)
	{
	}

}

void ThriftServer::Start(int port,int workerCount)
{
	//Simple server only uses one thread maybe usefull for debugging
	/*TSimpleServer server(
		std::make_shared<MMIAdapterProcessor>(std::make_shared<ThriftAdapterImplementation>()),
		std::make_shared<TServerSocket>(port),
		std::make_shared<TBufferedTransportFactory>(),
		std::make_shared<TCompactProtocolFactory>());
	*/

	//TODO set with console argument at start
	//number of simultaneously working server threads

	//threadmanager for reusing threads
	std::shared_ptr<ThreadManager> threadManager =ThreadManager::newSimpleThreadManager(workerCount);
	threadManager->threadFactory(std::make_shared<ThreadFactory>());
	threadManager->start();

	//This server allows "workerCount" connection at a time, and reuses threads

	this->server = new TThreadPoolServer(std::make_shared<MMIAdapterProcessorFactory>(std::make_shared<MMIAdapterCloneFactory>()),
		std::make_shared<TServerSocket>(port),
		std::make_shared<TBufferedTransportFactory>(),
		std::make_shared<TCompactProtocolFactory>(),
		threadManager);

	this->server->serve();
};



