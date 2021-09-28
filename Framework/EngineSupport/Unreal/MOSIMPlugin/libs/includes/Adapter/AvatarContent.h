// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include <string>
#include <unordered_map>
#include "MotionModelUnitBaseIf.h"
#include "gen-cpp/scene_types.h"

using namespace std;
namespace MMIStandard {
	class AvatarContent
	{
		/*
			Class which specifies the content of an avatar
		*/
	public:
		//	the id of the avatar
		string avatarID;

		// The list of MMUs of the session
		mutable unordered_map<string, unique_ptr<MotionModelUnitBaseIf>> MMUs;

		//	The posture of the reference avatar
		MAvatarPosture referencePosture;

	public:
		//	Basic constructor
		AvatarContent(string avatarId);

		AvatarContent(const AvatarContent&) = delete;
		AvatarContent& operator=(const AvatarContent&) = delete;
		AvatarContent(AvatarContent&&) = delete;
		AvatarContent& operator=(AvatarContent&&) = delete;

		//	Returns the MMU based on the id
		MotionModelUnitBaseIf &GetMMUbyId(const string &mmuId) const;
	};
}

