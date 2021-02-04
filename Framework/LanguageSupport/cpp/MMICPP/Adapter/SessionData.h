// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "gen-cpp/mmu_types.h"
#include "gen-cpp/MMIAdapter.h"
#include <concurrent_unordered_map.h>
#include "SessionContent.h"

using namespace MMIStandard;
using namespace std;

namespace MMIStandard {
	class SessionData
	{
		/*
			Class which contains the data of the sessions and MMUs
		*/

		// only this classes can access the private static members 
		friend class AdapterController;
		friend class ThriftAdapterImplementation;
		friend class FileWatcher;
		friend class SessionHandling;
		friend class SessionCleaner;
	private:

		//	The description of the adapter
		static MAdapterDescription adapterDescription;

		//	The address of the MMIRegister
		static MIPAddress registerAddress;

		//	The last time the adapter was used 
		static time_t lastAccess;

		//	The time when the adapter was started
		static time_t startTime;

		//	Descriptions of the loadable MMUs
		static std::vector<MMUDescription> mmuDescriptions;

		//	The paths to the Assemblies
		static std::unordered_map<std::string, std::string> mmuPaths; //id,MMUpath

		//	Map which contains all sessions
		static Concurrency::concurrent_unordered_map<std::string, unique_ptr <SessionContent>> SessionContents;

	public:

		//Getter for MMU description based on the id
		static const MMUDescription &GetMMUDescription(string mmuId);

		//Getter for the register address
		static const MIPAddress &GetRegisterAddress();

		SessionData(const SessionData&) = delete;
		SessionData& operator=(const SessionData&) = delete;
		SessionData(SessionData&&) = delete;
		SessionData& operator=(SessionData&&) = delete;
	};
}

