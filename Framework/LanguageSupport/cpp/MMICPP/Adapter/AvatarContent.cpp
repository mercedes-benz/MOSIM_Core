#include "AvatarContent.h"

AvatarContent::AvatarContent(string avatarId) :avatarID{ avatarId }
{
}

 MotionModelUnitBaseIf & AvatarContent::GetMMUbyId(const string &mmuId) const
{
	auto iter = this->MMUs.find(mmuId);
	if (iter != this->MMUs.end()) // mmu found!
	{
		return *iter->second;
	}
	else
	{
		throw runtime_error("MMU with id: " + mmuId + " not found in avatar content");
	}
}

