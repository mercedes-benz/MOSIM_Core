// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "Extensions/MVector3Extensions.h"
#include <math.h>

void MVector3Extensions::ToMVector3(MVector3 & _return, const vector<double>& values)
{
	if (values.size() < 3)
	{
		throw runtime_error("Can not create MVector3: input has less than 3 values");
	}
	_return.__set_X(values[0]);
	_return.__set_Y(values[1]);
	_return.__set_Z(values[2]);
}

void MVector3Extensions::ToDoubleVector(vector<double> & _return, const MVector3 & values)
{	
	if (_return.size() != 3)
	{
		throw runtime_error("Can not update double vector: Input needs to have exactly 3 values");
	}
	_return[0] = values.X;
	_return[1] = values.Y;
	_return[2] = values.Z;	
}

float MVector3Extensions::EuclideanDistance(const MVector3 &vector1, const MVector3 &vector2)
{
	MVector3 _result = MVector3{};
	Subtract(_result, vector1, vector2);
	return Magnitude(_result);
}

void MVector3Extensions::Subtract(MVector3 & _return, const MVector3 &vector1, const MVector3 &vector2)
{
	_return.__set_X(vector1.X-vector2.X);
	_return.__set_Y(vector1.Y - vector2.Y);
	_return.__set_Z(vector1.Z - vector2.Z);
}

shared_ptr<MVector3> MVector3Extensions::Subtract(const MVector3 & vector1, const MVector3 & vector2)
{
	auto _return = make_shared<MVector3>();
	Subtract(*_return, vector1, vector2);
	return _return;
}

float MVector3Extensions::Magnitude(const MVector3 & vector)
{
	return (float)sqrt(pow(vector.X,2)+pow(vector.Y,2)+pow(vector.Z,2));
}

void MVector3Extensions::Add(MVector3 & _return, const MVector3 & vector1, const MVector3 & vector2)
{
	_return.__set_X(vector1.X + vector2.X);
	_return.__set_Y(vector1.Y + vector2.Y);
	_return.__set_Z(vector1.Z + vector2.Z);
}

shared_ptr<MVector3> MVector3Extensions::Add(const MVector3 & vector1, const MVector3 & vector2)
{
	auto _return = make_shared<MVector3>();
	Add(*_return, vector1, vector2);
	return _return;
}

void MVector3Extensions::Multiply(MVector3 & _return, const MVector3 & vector, double scalar)
{
	_return.__set_X(vector.X * scalar);
	_return.__set_Y(vector.Y * scalar);
	_return.__set_Z(vector.Z * scalar);
}

shared_ptr<MVector3> MVector3Extensions::Multiply(const MVector3 & vector, double scalar)
{
	auto _return = make_shared<MVector3>();
	Multiply(*_return, vector, scalar);
	return _return;
}
