// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "MAvatarPostureExtensions.h"
#include <vector>
#include<iostream>
#include "Extensions/MVector3Extensions.h"
#include "Extensions/MQuaternionExtensions.h"
#include <deque>

void MavatarPostureExtensions::GetPostureValues(MAvatarPostureValues &_return, const MAvatarPosture & avatarPosture)
{
	_return.__set_PostureData(vector<double>{});
	if (avatarPosture.AvatarID.empty()) 
	{
		_return.__set_AvatarID("default");
	}
	else
	{
		_return.__set_AvatarID(avatarPosture.AvatarID);
	}

	//Add root bone value
	_return.PostureData.emplace_back(avatarPosture.Joints[0].Position.X);
	_return.PostureData.emplace_back(avatarPosture.Joints[0].Position.Y);
	_return.PostureData.emplace_back(avatarPosture.Joints[0].Position.Z);

	_return.PostureData.emplace_back(avatarPosture.Joints[0].Rotation.X);
	_return.PostureData.emplace_back(avatarPosture.Joints[0].Rotation.Y);
	_return.PostureData.emplace_back(avatarPosture.Joints[0].Rotation.Z);
	_return.PostureData.emplace_back(avatarPosture.Joints[0].Rotation.W);

	//Add the other values

	for (size_t i = 1; i < avatarPosture.Joints.size();i++)
	{
		_return.PostureData.emplace_back(avatarPosture.Joints[i].Rotation.X);
		_return.PostureData.emplace_back(avatarPosture.Joints[i].Rotation.Y);
		_return.PostureData.emplace_back(avatarPosture.Joints[i].Rotation.Z);
		_return.PostureData.emplace_back(avatarPosture.Joints[i].Rotation.W);
	}
}

shared_ptr<MAvatarPostureValues> MavatarPostureExtensions::GetPostureValues(const MAvatarPosture & avatarPosture)
{
	auto _return = make_shared<MAvatarPostureValues>();
	GetPostureValues(*_return, avatarPosture);
	return _return;
}

void  MavatarPostureExtensions::AssignBoneTypes( MAvatarPosture & avatarPosture, const unordered_map<string, MJointType::type> &mapping)
{
	for ( auto transform : avatarPosture.Joints)
	{
		auto it = mapping.find(transform.ID);
		if (it != mapping.end())
		{
			transform.Type = it->second;
		}
	}
}

void MavatarPostureExtensions::AssignPostureValues(MAvatarPosture & _return, const MAvatarPostureValues & avatarPostureValues)
{
	if (avatarPostureValues.PostureData.size() != _return.Joints.size() * 4 + 3)
	{
		throw runtime_error("Value count does not fit to bone count of hierarchy");
	}

	if (avatarPostureValues.PostureData.size() >= 7)
	{
		_return.Joints[0].Position.__set_X(avatarPostureValues.PostureData[0]);
		_return.Joints[0].Position.__set_Y(avatarPostureValues.PostureData[1]);
		_return.Joints[0].Position.__set_Z(avatarPostureValues.PostureData[2]);

		_return.Joints[0].Rotation.__set_X(avatarPostureValues.PostureData[3]);
		_return.Joints[0].Rotation.__set_Y(avatarPostureValues.PostureData[4]);
		_return.Joints[0].Rotation.__set_Z(avatarPostureValues.PostureData[5]);
		_return.Joints[0].Rotation.__set_W(avatarPostureValues.PostureData[6]);

		int index = 1;
		for (size_t i = 7; i < avatarPostureValues.PostureData.size(); i += 4)
		{
			_return.Joints[index].Rotation.__set_X(avatarPostureValues.PostureData[i]);
			_return.Joints[index].Rotation.__set_Y(avatarPostureValues.PostureData[i+1]);
			_return.Joints[index].Rotation.__set_Z(avatarPostureValues.PostureData[i+2]);
			_return.Joints[index].Rotation.__set_W(avatarPostureValues.PostureData[i+3]);

			index++;
		}
	}
}

shared_ptr<MJoint> MavatarPostureExtensions::GetJoint(const MAvatarPosture& posture, const MJointType::type & type)
{
	for ( auto &joint : posture.Joints)
	{
		if (joint.Type == type)
			return make_shared<MJoint>(joint);
	}
	return nullptr;
}

shared_ptr<MJoint> MavatarPostureExtensions::GetJoint(const MAvatarPosture & posture, const string & name)
{
	for (auto &joint : posture.Joints)
	{
		if (joint.ID == name)
			return make_shared<MJoint>(joint);
	}

	return nullptr;
}

shared_ptr<MVector3> MavatarPostureExtensions::GetGlobalPosition(const MAvatarPosture & posture, const MJointType::type & boneType)
{
	

	//Get the specified bone by tpe
	shared_ptr<MJoint> boneResult = GetJoint(posture,boneType);
	const MJoint  *bone =&*boneResult;

	if (!bone)
	{
		throw new runtime_error("Bone with this type was notfound");
	}

	return CalculateHierarchyPosition(posture, *boneResult);	
}

shared_ptr<MVector3> MavatarPostureExtensions::GetGlobalPosition(const MAvatarPosture & posture, const string & boneName)
{
	//Create empty list which represents the hierarchy
	deque<MJoint> hierarchy{};

	//Get the specified bone by tpe
	shared_ptr<MJoint> boneResult = GetJoint(posture, boneName);
	
	if (!boneResult)
	{
		throw new runtime_error("Bone with this type was notfound");
	}

	return CalculateHierarchyPosition(posture, *boneResult);	
}

shared_ptr<MVector3> MavatarPostureExtensions::CalculateHierarchyPosition(const MAvatarPosture & posture,const MJoint & bone)
{
	//Create empty list which represents the hierarchy
	deque<MJoint> hierarchy{};
	const MJoint *tempBone = &bone;
	hierarchy.emplace_back(*tempBone);
	while (!tempBone->Parent.empty() || tempBone->Parent != "")
	{
		//Assign the parent
		for (const auto joint : posture.Joints)
		{
			if (joint.ID == tempBone->Parent)
			{
				tempBone = &joint;
				hierarchy.emplace_front(joint);
			}
			else
			{
				break;
			}
		}
	}
	auto position = make_shared<MVector3>(hierarchy[0].Position);
	//MQuaternion rotation = hierarchy[0].Rotation;

	for (size_t i = 0; i < hierarchy.size() - 1; i++)
	{
		//Compute the new position
		MVector3Extensions::Add(*position, *position, *(MQuaternionExtensions::MultiplyToMVector3(hierarchy[0].Rotation, hierarchy[i + 1].Position)));
		//MQuaternionExtensions::Multiply(rotation, hierarchy[i + 1].Rotation);
	}
	return position;
}

shared_ptr<MQuaternion> MavatarPostureExtensions::GetGlobalRotation(const MAvatarPosture & posture, const MJointType::type & boneType)
{
	//Create empty list which represents the hierarchy
	deque<MJoint> hierarchy{};

	//Get the specified bone by tpe
	shared_ptr<MJoint> boneResult = GetJoint(posture, boneType);

	if (!boneResult)
	{
		throw new runtime_error("Bone with this type was notfound");
	}
	return CalculateHierarchyRotation(posture, *boneResult);
}

shared_ptr<MQuaternion> MavatarPostureExtensions::GetGlobalRotation(const MAvatarPosture & posture, const string & boneName)
{
	//Create empty list which represents the hierarchy
	deque<MJoint> hierarchy{};

	//Get the specified bone by tpe
	shared_ptr<MJoint> boneResult = GetJoint(posture, boneName);

	if (!boneResult)
	{
		throw new runtime_error("Bone with this type was notfound");
	}

	return CalculateHierarchyRotation(posture, *boneResult);
}

shared_ptr<MQuaternion> MavatarPostureExtensions::CalculateHierarchyRotation(const MAvatarPosture & posture, const MJoint & bone)
{
	//Create empty list which represents the hierarchy
	deque<MJoint> hierarchy{};
	const MJoint *tempBone = &bone;
	hierarchy.emplace_back(*tempBone);
	while (!tempBone->Parent.empty() || tempBone->Parent != "")
	{
		//Assign the parent
		for (const auto joint : posture.Joints)
		{
			if (joint.ID == tempBone->Parent)
			{
				tempBone = &joint;
				hierarchy.emplace_front(joint);
			}
			else
			{
				break;
			}
		}
	}
	//auto position = make_shared<MVector3>(hierarchy[0].Position);
	auto rotation = make_shared<MQuaternion>(hierarchy[0].Rotation);

	for (size_t i = 0; i < hierarchy.size() - 1; i++)
	{
		//Compute the new rotation
		//MVector3Extensions::Add(*position, *position, *(MQuaternionExtensions::MultiplyToMVector3(hierarchy[0].Rotation, hierarchy[i + 1].Position)));
		MQuaternionExtensions::Multiply(*rotation,*rotation, hierarchy[i + 1].Rotation);
	}
	return rotation;
}

void MavatarPostureExtensions::SetBoneLengths(MAvatarPosture & posture, const unordered_map<MJointType::type, float>& boneLengths)
{
	if(posture.Joints.empty())
		throw new runtime_error("Can not scale empty avatar posture!");

	for (const auto & element : boneLengths)
	{
		auto bone = GetJoint(posture,element.first);
		if (!bone)
		{
			bone->Position = *(MVector3Extensions::Multiply(bone->Position, element.second / MVector3Extensions::Magnitude(bone->Position)));
		}
	}
}

void MavatarPostureExtensions::SetLocalPosition(MAvatarPosture & posture, const MAvatarPosture & data)
{
	for (const auto &transform : data.Joints)
	{
		for (auto joints : posture.Joints)
		{
			if (joints.ID == transform.ID)
			{
				joints.__set_Position(transform.Position);
			}
		}
	}
}

