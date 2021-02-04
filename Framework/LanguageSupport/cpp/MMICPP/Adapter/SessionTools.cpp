// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "SessionTools.h"
#include <boost/algorithm/string.hpp>
 
using namespace MMIStandard;
 std::vector<string> SessionTools::GetSplittedIds(const string &sessionID)
{
	if (sessionID.empty()|| sessionID.compare("")==0)
	{
		 throw std::runtime_error("SessionId is empty");
	}
	std::vector<std::string>splitted;
	boost::split(splitted, sessionID, boost::is_any_of(":"));

	if (splitted.size() == 2 && !splitted[0].empty() && !splitted[1].empty())
	{
		return splitted;
	}
	else
	{
		return std::vector<std::string>{sessionID, "0"};
	}
	
}
