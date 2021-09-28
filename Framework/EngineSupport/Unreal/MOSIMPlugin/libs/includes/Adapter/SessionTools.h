// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include<string>
#include <vector>

using namespace std;
namespace MMIStandard {
	class SessionTools
	{
		/*
			Helper class for sessions
		*/
	public:

		// returns the scene id and the avatar id based on the session id 
		static vector<std::string> GetSplittedIds(const string & sessionID);
	};
}

