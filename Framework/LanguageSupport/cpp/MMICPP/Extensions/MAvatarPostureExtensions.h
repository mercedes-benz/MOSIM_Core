// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "gen-cpp/scene_types.h"
#include <unordered_map>

using namespace MMIStandard;
using namespace std;

namespace MMIStandard {
	class MavatarPostureExtensions
	{
		/*
		Class which extends MVector3
		*/
  
	private:
		static shared_ptr<MQuaternion> CalculateHierarchyRotation(const MAvatarPosture &posture, const MJoint & bone);
		static shared_ptr<MVector3> CalculateHierarchyPosition(const MAvatarPosture &posture, const MJoint & bone);

	public:
		// assigns a MAvatarPosture to MAvatarPostureVAlues
		static void GetPostureValues(MAvatarPostureValues &_return, const MAvatarPosture &avatarPosture);
		static shared_ptr<MAvatarPostureValues> GetPostureValues(const MAvatarPosture &avatarPosture);

		//assigns new bone types
		static void AssignBoneTypes(MAvatarPosture & _return, const unordered_map<string, MJointType::type> &mapping);

		//assigns MAvatarPostureValues to a MAvatarPosture
		static void AssignPostureValues(MAvatarPosture & _return, const MAvatarPostureValues & avatarPostureValues);

		//returns a joint by type from a given MAvatarPosture
		static shared_ptr<MJoint> GetJoint(const MAvatarPosture &posture, const MJointType::type & type);

		//returns a joint by name from a given MAvatarPosture
		static shared_ptr<MJoint> GetJoint(const MAvatarPosture &posture, const string &name);

		//Returns the global position of the given bone by type
		static shared_ptr<MVector3> GetGlobalPosition(const MAvatarPosture &posture, const MJointType::type &boneType);

		//Returns the global position of the given bone by name
		static shared_ptr<MVector3> GetGlobalPosition(const MAvatarPosture &posture, const string &boneName);

		//Returns the global rotation of the given bone by type
		static shared_ptr<MQuaternion> GetGlobalRotation(const MAvatarPosture & posture, const MJointType::type &boneType);

		//Returns the global rotation of the given bone by name
		static shared_ptr<MQuaternion> GetGlobalRotation(const MAvatarPosture & posture, const string &boneName);

		//Scales the avatar posture based on the given bone lengths
		static void SetBoneLengths(MAvatarPosture &posture, const unordered_map<MJointType::type, float> &boneLengths);

		//Sets the local positions of the avatar posture based on an input posture
		static void SetLocalPosition(MAvatarPosture &posture, const MAvatarPosture &data);
	};
}

