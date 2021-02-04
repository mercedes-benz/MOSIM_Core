// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "SessionData.h"

using namespace MMIStandard;

//initialize static members
MAdapterDescription SessionData::adapterDescription;
MIPAddress SessionData::registerAddress;
std::vector<MMUDescription> SessionData::mmuDescriptions;
std::unordered_map<std::string, std::string> SessionData::mmuPaths;
time_t SessionData::startTime;
time_t SessionData::lastAccess=0;
Concurrency::concurrent_unordered_map<std::string, unique_ptr<SessionContent>> SessionData::SessionContents;
 

const MMUDescription &SessionData::GetMMUDescription(string mmuId)
{
	for (const MMUDescription &desc : mmuDescriptions)
	{
		if (desc.ID.compare(mmuId)==0)
			return desc;
	}
	throw runtime_error("No MMUDescription with id:" + mmuId + " found");
}

const MIPAddress & SessionData::GetRegisterAddress()
{ 
	return  registerAddress;
}


