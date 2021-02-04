// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "SessionHandling.h"
#include "SessionData.h"
#include "SessionTools.h"
#include "SessionData.h"
#include "Utils/Logger.h"


const SessionContent &SessionHandling::GetSessionContentBySessionID(const string & sessionID)
{
	return GetSessionContentBySceneID(SessionTools::GetSplittedIds(sessionID)[0]);
}

const SessionContent & MMIStandard::SessionHandling::GetSessionContentBySceneID(const string & sceneID)
{
	auto it = SessionData::SessionContents.find(sceneID);
	if (it != SessionData::SessionContents.end())
	{
		return *it->second;
	}
	else
	{
		throw runtime_error("Can not find session content with ID: " + sceneID);
	}
}

const SessionContent & SessionHandling::CreateSessionContent(const string & sessionId)
{
		std::vector<string> splittedIds = SessionTools::GetSplittedIds(sessionId);
		std::string sceneId = splittedIds[0];
		std::string avatarId = splittedIds[1];

		//check if session content is already available 
		auto it = SessionData::SessionContents.find(sceneId);
		if (it != SessionData::SessionContents.end())
		{
			//check if existing session content has already the avatar content
			auto avatarContentIt = it->second->avatarContent.find(avatarId);
			if (avatarContentIt == it->second->avatarContent.end()) //avatar content not available
			{

				it->second->avatarContent[avatarId] = make_unique<AvatarContent>(avatarId);
				return *SessionData::SessionContents[sceneId];
			}
			else
			{
				throw runtime_error("Unable to create session: session and avatar content already available");
			}	
		}
		else
		{
			Logger::printLog(L_INFO, "Create new session: " + sessionId);
			unique_ptr <AvatarContent> avatarContent = make_unique<AvatarContent>(avatarId);
			unique_ptr <SessionContent> sessionContent = make_unique<SessionContent>(sessionId);
			sessionContent->avatarContent[avatarId] = move(avatarContent);
			SessionData::SessionContents[sceneId] = move(sessionContent);

			//TODO check
			return *sessionContent;
		}
}

void SessionHandling::RemoveSessionContent(const string & sessionID)
{
	auto it = SessionData::SessionContents.find(SessionTools::GetSplittedIds(sessionID)[0]);
	if (it != SessionData::SessionContents.end())
	{
		SessionData::SessionContents.unsafe_erase(it);
	}
	else
	{
		throw runtime_error("Can not find Session content with ID: " + sessionID);		
	}	
}

MotionModelUnitBaseIf & MMIStandard::SessionHandling::GetMMUbyId(const string & sessionID, const string & mmuId)
{
	return GetAvatarContentBySessionID(sessionID).GetMMUbyId(mmuId);
}

const AvatarContent & MMIStandard::SessionHandling::GetAvatarContentBySessionID(string sessionID)
{
	vector<string> splitted = SessionTools::GetSplittedIds(sessionID);
	return GetSessionContentBySceneID(splitted[0]).GetAvatarContentByAvatarID(splitted[1]);
}



