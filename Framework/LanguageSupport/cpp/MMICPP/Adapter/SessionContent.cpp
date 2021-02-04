// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "SessionContent.h"
#include "SessionData.h"
#include "Adapter/SessionTools.h"


SessionContent::SessionContent(string sessionId) :sessionID{ sessionId },lastAccess{0}
{
	this->serviceAccess = make_unique<ServiceAccess>(SessionData::GetRegisterAddress(),sessionID);
	this->sceneBuffer = make_unique<MMIScene>();
}

ServiceAccess  & SessionContent::GetServiceAccess() const
{
	return *(this->serviceAccess);
}

MMIScene & SessionContent::GetScene() const
{
	return *(this->sceneBuffer);
}

const AvatarContent & SessionContent::GetAvatarContentBySessionID(string sessionID) const
{
	return this->GetAvatarContentByAvatarID(SessionTools::GetSplittedIds(sessionID)[1]);
}

const AvatarContent & MMIStandard::SessionContent::GetAvatarContentByAvatarID(string avatarID) const
{
	auto avatarContentIt = avatarContent.find(avatarID);
	if (avatarContentIt != avatarContent.end()) //avatar content  available
	{
		return *avatarContentIt->second;
	}
	else
	{
		throw runtime_error("Can not find avatar content with id: " + avatarID);
	}
}




