// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

// CppAdapter.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include "CppAdapter.h"

int main(int ac, char* av[])
{
	//create a logger instance
	Logger LoggerInst;
	//log everything
	LoggerInst.logLevel = L_DEBUG;

	std::cout << R"(
   ______              ___       __            __           
  / ____/__    __     /   | ____/ /___ _____  / /____  _____
 / /  __/ /___/ /_   / /| |/ __  / __ `/ __ \/ __/ _ \/ ___/
/ /__/_  __/_  __/  / ___ / /_/ / /_/ / /_/ / /_/  __/ /    
\____//_/   /_/    /_/  |_\__,_/\__,_/ .___/\__/\___/_/   
                                    /_/	)" << endl;
	//parsing and check the command line args 
	string adapterAddress;
	string registerAddress;
	string mmuPath;
	int workerCount = 4;
	int logLevel=2;


	try {
		po::options_description desc("Allowed options");
		desc.add_options()
			//("help", "produce help message") 	does not work because of following required parameters
			("address,a", po::value<string>(&adapterAddress)->required(), "The address of the adapters tcp server")
			("raddress,r", po::value<string>(&registerAddress)->required(), "The address of the register which holds the central information.")
			("mmupath,m", po::value<string>(&mmuPath)->required(), "The path of the mmu folder.")
			("threads,t", po::value<int>(&workerCount), "The Number of worker threads for the server");
			("debug,d", po::value<int>(&logLevel), "The log level 0:SILENT, 1: ERROR, 2: INFO, 3: DEBUG");

		po::variables_map vm;
		po::store(po::parse_command_line(ac, av, desc), vm);
		po::notify(vm);

		////does not work because of required 
		//if (vm.count("help")) {
		//	std::cout << desc << "\n";
		//	return 0;
		//}
	}
	catch (exception& e)
	{
		cout << e.what() << endl;
		return 0;
	}

	/*switch (logLevel) {
		case 0: Logger::logLevel = L_SILENT;
			break;
		case 1: Logger::logLevel = L_ERROR;
			break;
		case 2: Logger::logLevel = L_INFO;
			break;
		case 3: Logger::logLevel = L_DEBUG;
			break;
	}*/
	
	vector<string> adapterAddressSplit;
	vector<string> registerAddressSplit;
	boost::split(adapterAddressSplit, adapterAddress, boost::is_any_of(":"));
	boost::split(registerAddressSplit, registerAddress, boost::is_any_of(":"));

	MIPAddress adapterMIPAddress{};
	adapterMIPAddress.__set_Address(adapterAddressSplit[0]);
	adapterMIPAddress.__set_Port(std::stoi(adapterAddressSplit[1]));

	MIPAddress registerMIPAddress{};
	registerMIPAddress.__set_Address(registerAddressSplit[0]);
	registerMIPAddress.__set_Port(std::stoi(registerAddressSplit[1]));

	//print connection info
	cout << ("_________________________________________________________________") << std::endl;
	cout << ("Adapter is reachable at: ") << adapterMIPAddress.Address << (":") << adapterMIPAddress.Port << std::endl;
	cout << ("Register is reachable at: ") << registerMIPAddress.Address << (":") << registerMIPAddress.Port << std::endl;
	cout << ("MMUs will be loaded from: ") << mmuPath << std::endl;
	cout << ("_________________________________________________________________") << std::endl;

	MAdapterDescription adapterDescription = MAdapterDescription{};
	adapterDescription.__set_Name("C++Adapter");
	adapterDescription.__set_ID("7999d37f-1337-45da-9015-f4adde70a44d");
	adapterDescription.__set_Language("C++");
	adapterDescription.__set_Addresses(vector<MIPAddress>{adapterMIPAddress});
	map<string, string> propertiesMap;
	adapterDescription.__set_Properties(propertiesMap);

	SessionData SessionDataInst = SessionData{};

	
	//start the adapter controller
	CPPMMUInstantiator Instantiator = CPPMMUInstantiator{};
	AdapterController adapterController{ adapterMIPAddress,registerMIPAddress,mmuPath, workerCount,Instantiator, vector<string>{"C++"}, adapterDescription};
	adapterController.Start();

	return 0;
}

