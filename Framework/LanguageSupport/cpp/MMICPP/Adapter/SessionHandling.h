// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include <string>
#include "SessionContent.h"

using namespace std;
namespace MMIStandard {

	class SessionHandling
	{
		/*
			Helper class for session handling
		*/

		//only ThriftAdapterImplementation can access the functions
		friend class ThriftAdapterImplementation;

	private:
		//	 Returns the session content based on the session id
		static const SessionContent & GetSessionContentBySessionID(const string &sessionID);

		//	 Returns the session content based on the scene id
		static const SessionContent & GetSessionContentBySceneID(const string &sceneID);

		//	Creates a new session content
		static const SessionContent & CreateSessionContent(const string &sessionID);

		//	Deletes a session content based on the session id
		static void RemoveSessionContent(const string &sessionId);

		//	get the MMU based on the SessionID and the mmuID
		static MotionModelUnitBaseIf &GetMMUbyId(const string &sessionID,const string &mmuId);

		//	get the Avatarcontent bsed on the sessionID and the mmuID
		static const  AvatarContent & GetAvatarContentBySessionID(string sessionID);
	};
}

