// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "MMIScene.h"
#include "Access/ServiceAccess.h"
#include <concurrent_unordered_map.h>
#include "AvatarContent.h"

using namespace std;
using namespace MMIStandard;

namespace MMIStandard {
	class SessionContent
	{
		/*
			Class which contains the content which is related to the specific session
		*/
	public:
		//	The corresponding scene access
		unique_ptr<MMIScene> sceneBuffer;

		//	The corresponding service access
		unique_ptr<ServiceAccess> serviceAccess;

		//	The avatar contents of the specific session
		mutable Concurrency::concurrent_unordered_map<std::string, unique_ptr<AvatarContent>> avatarContent;

		//	The id of the session
		string sessionID;

		// The last time the session was used 
		time_t lastAccess;

	public:

		// Basic constructor
		SessionContent(string sessionId);
		SessionContent(const SessionContent&) = delete;
		SessionContent& operator=(const SessionContent&) = delete;
		SessionContent(SessionContent&&) = delete;
		SessionContent& operator=(SessionContent&&) = delete;

		//	Getter for the service access
		ServiceAccess & GetServiceAccess() const;

		//	Getter for the scene Access
		MMIScene & GetScene()const;

		//  Returns the avatar content based on the sessionID
		const AvatarContent & GetAvatarContentBySessionID(string sessionID) const;

		//  Returns the avatar content based on the avatarID
		const AvatarContent & GetAvatarContentByAvatarID(string avatarID) const;
	};
}

