// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "gen-cpp/scene_types.h"

using namespace std;
using namespace MMIStandard;

namespace MMIStandard {
	class MQuaternionExtensions
	{
		/*
			Class which extends MQuaternion
		*/

	public:
		MQuaternionExtensions() = delete;
		//	Method which converts double values to MQaternion
		static void ToMQuaternion(MQuaternion &_return, const vector<double> &values);
		static shared_ptr<MQuaternion> ToMQuaternion(const vector<double> &values);

		//	Method which multiplies a MQuaternion with a MVector3
		static void MultiplyToMVector3(MVector3 &_return, const MQuaternion &quat, const MVector3 &vec);
		static shared_ptr<MVector3> MultiplyToMVector3(const MQuaternion &quat, const MVector3 &vec);

		//	Metod which multiplies two Quaternions
		static void Multiply(MQuaternion &_return, const MQuaternion &left, const MQuaternion &right);
		static shared_ptr<MQuaternion> Multiply(const MQuaternion &left, const MQuaternion &right);
	};
}

