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

