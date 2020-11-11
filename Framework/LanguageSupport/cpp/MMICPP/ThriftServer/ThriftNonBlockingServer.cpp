#include "ThriftNonBlockingServer.h"
#include <thread>
#include "thrift/transport/TTransport.h"
#include "thrift/transport/TNonblockingServerSocket.h"
#include <thrift/concurrency/ThreadManager.h>
#include <thrift/concurrency/ThreadFactory.h>
#include "Adapter/ThriftAdapterImplementation.h"
#include "thrift/protocol/TCompactProtocol.h"

using namespace std;
using namespace apache::thrift::protocol;
using namespace apache::thrift::transport;
using namespace apache::thrift::server;
using namespace apache::thrift::concurrency;

ThriftNonBlockingServer::ThriftNonBlockingServer()
{
}


ThriftNonBlockingServer::~ThriftNonBlockingServer()
{
	try
	{
		this->server->stop();
		delete this->server;
	}
	catch (...)
	{
	}
}

void ThriftNonBlockingServer::Start(int port)
{
	int hw_threads = thread::hardware_concurrency();
	int io_threads = hw_threads / 2;
	int worker_threads = hw_threads * 1.5 + 1;
	auto transport = make_shared<TNonblockingServerSocket>(port);

	shared_ptr<MMIAdapterProcessorFactory> processor = make_shared<MMIAdapterProcessorFactory>(make_shared<MMIAdapterIfSingletonFactory>(make_shared<ThriftAdapterImplementation>()));
	auto protoc_fac = make_shared<TCompactProtocolFactoryT<TMemoryBuffer>>();

	//threadmanager for reusing threads
	std::shared_ptr<ThreadManager> threadManager = ThreadManager::newSimpleThreadManager(worker_threads);
	threadManager->threadFactory(std::make_shared<ThreadFactory>());
	threadManager->start();
	


	this->server = new TNonblockingServer(processor,protoc_fac,transport,threadManager);
	this->server->setNumIOThreads(io_threads);
	this->server->serve();

	

}
