#pragma once
#include "src/core_types.h"
#include<string>
#include "FileWatcher.h"
#include "Adapter/CPPMMUInstantiator.h"
#include "src/mmu_types.h"
#include "Utils/Logger.h"

using namespace MMIStandard;
using namespace std;

namespace MMIStandard {
	class AdapterController
	{
		/**
			Basic class which:
				creates the adapter description
				initializes the adapter and the SessionData
				registers the adapter at the MMIRegister
				starts the FileWatcher
				starts the AdapterServer
		*/

	private:
		//	The address of the AdapterServer
		const  MIPAddress adapterAddress;

		//	The address of the MMiRegister
		const  MIPAddress registerAddress;

		//	The path of the mmus
		const string mmuPath;

		//	The supported languages
		vector<string> languages;

		//	Bool which shows, if the adapter is registered at the MMIRegister
		bool isRegistered;

		//	The number of server threads
		int workerCount;

		//	The helper class which instantiates the MMUs from file
		static CPPMMUInstantiator instantiator;

	private:
		//	Registers the adapter at the MMIRegister
		void RegisterAdapter();

		//	Creates and starts an new AdapterServer
		void StartAdapterServer();

	public:
		//	Basic constructor
		//	<param name="address">The address of the adapter</param>
		//	<param name="mmiRegisterAddress">The address of the register (where all services, adapters and mmus are registered)</param>
		//	<param name="mmuPath">The path where the MMUs are located</param>
		//	<param name="instantioator">the instantiator class which instantiates thte MMus from file</param>
		//	<param name="languages">the languages the adapter should support </param>
		//	<param name="adapterDescription">the description of the adapter</param>
		//	<param name="logLevel">the log level for the logger, default = L_Debug</param>
		AdapterController(const MIPAddress & aAddress, const MIPAddress &rAddress, const string &mmuPath, int workerCount, const CPPMMUInstantiator &instantiator, const std::vector<std::string> & languages, const MAdapterDescription &adapterDescription);
		//returns a const reference of the instantiator

		static const CPPMMUInstantiator & GetMMUInstantiator();
		//	Basic descructor
		//	unregisters the adapter at the MMIRegister
		~AdapterController();

		//	Starts a thread for registering the adapter, for the Filewatcher and for the AdapterServer
		void Start();
	};
}
