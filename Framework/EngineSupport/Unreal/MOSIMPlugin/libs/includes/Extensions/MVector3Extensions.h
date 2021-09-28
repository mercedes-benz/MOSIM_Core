// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include <vector>
#include "gen-cpp/scene_types.h"

using namespace std;
using namespace MMIStandard;

namespace MMIStandard {
	class MVector3Extensions
	{
		/*
			Class which extends MVector3
		*/
	public:
		MVector3Extensions() = delete;
		//	Method which converts double values to MVector3
		static void ToMVector3(MVector3 &_return, const vector<double> &values);

		//  Method to convert MVector3 to double vector
		static void ToDoubleVector(vector<double> & _return, const MVector3 & values);

		//	Method calculates the euclidean distance between the two vectors
		static float EuclideanDistance(const  MVector3 &vector1, const MVector3 &vector2);

		// Method subtracts two given vectors
		static void Subtract(MVector3 &_return, const MVector3 &vector1, const  MVector3 &vector2);

		static shared_ptr<MVector3>Subtract(const MVector3 &vector1, const  MVector3 &vector2);

		//	Method calculates the magnitude of a vector
		static float Magnitude(const MVector3 & vector);

		//	Method adds two given vectors
		static void Add(MVector3 &_return, const MVector3 &vector1, const  MVector3 &vector2);
		static shared_ptr<MVector3> Add(const MVector3 &vector1, const MVector3 &vector2);

		//	Methods multiplies two given vectors
		static void Multiply(MVector3 &_return, const MVector3 &vector, double scalar);
		static shared_ptr<MVector3> Multiply(const MVector3 &vector, double scalar);
	};
}

