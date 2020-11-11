#include "AdapterController.h"
#include <filesystem>
#include <thread> 
#include "SessionData.h"
#include "boost/exception/diagnostic_information.hpp"
#include "ThriftClient/ThriftClient.cpp"
#include "src/MMIRegisterService.h"
#include "Utils/Logger.h"
#include "ThriftServer/ThriftNonBlockingServer.h"
#include "ThriftServer/ThriftServer.h"

CPPMMUInstantiator AdapterController::instantiator;

AdapterController::AdapterController(const MIPAddress & aAddress, const MIPAddress &rAddress, const string &mmuPath, int workerCount, const CPPMMUInstantiator &instantiator, const std::vector<std::string> & languages, const MAdapterDescription &adapterDescription) :adapterAddress(aAddress), registerAddress(rAddress), mmuPath(mmuPath), workerCount{ workerCount }, languages{ languages }
{
	this->isRegistered = false;
	AdapterController::instantiator = instantiator;

	//init SessionData
	SessionData::adapterDescription = adapterDescription;
	SessionData::registerAddress = rAddress;
}

const CPPMMUInstantiator & AdapterController::GetMMUInstantiator()
{
	return instantiator;
}

AdapterController::~AdapterController()
{
	ThriftClient<MMIRegisterServiceClient> client{ this->registerAddress.Address,this->registerAddress.Port};
	client.Start();
	MBoolResponse response{};
	client.access->UnregisterAdapter(response, SessionData::adapterDescription);

	//this->thriftServer.~AdapterServer();
}

void AdapterController::Start()
{
	SessionData::startTime = time(0);
	//new thread for registering at the MMIRegister
	thread registerThread(&AdapterController::RegisterAdapter, this);

	//new thread for checking for loadable MMUs
	FileWatcher watcher{mmuPath,languages };
	thread fileWatcherThread(&FileWatcher::Start, watcher);

	////new thread for adapter server
	thread serverThread(&AdapterController::StartAdapterServer, this);
	//
	////block "main thread" until all threads finished
	registerThread.join();
	fileWatcherThread.join();
	serverThread.join();
}

void AdapterController::RegisterAdapter()
{
	while (this->isRegistered!=true)
	{
		try
		{
			ThriftClient<MMIRegisterServiceClient> client{ this->registerAddress.Address,this->registerAddress.Port };
			client.Start();
			MBoolResponse response{};
			client.access->RegisterAdapter(response, SessionData::adapterDescription);
			this->isRegistered = response.Successful;
			if (this->isRegistered)
				Logger::printLog(L_INFO, "Successfully registered at MMIRegister");
		}
		catch (...)
		{
			Logger::printLog(L_ERROR, "Failed to register the adapter at MMIRegister");
			this_thread::sleep_for(1s);
		}	
	}
}

void AdapterController::StartAdapterServer()
{
	//ThriftNonBlockingServer server {};
	ThriftServer server{};
	Logger::printLog(L_INFO, "Starting adapter server at: " + this->adapterAddress.Address + ":" + to_string (this->adapterAddress.Port));
	try 
	{
		server.Start(this->adapterAddress.Port,this->workerCount);
		//server.Start(this->adapterAddress.Port);
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}
}


