// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "MQuaternionExtensions.h"

void MQuaternionExtensions::ToMQuaternion(MQuaternion & _return, const vector<double>& values)
{
	if (values.size() < 4)
	{
		throw runtime_error("Can not create MQuaternion: input has less than 3 values");
	}
	_return.__set_X(values[0]);
	_return.__set_Y(values[1]);
	_return.__set_Z(values[2]);
	_return.__set_W(values[3]);
}

shared_ptr<MQuaternion> MQuaternionExtensions::ToMQuaternion(const vector<double>& values)
{
	auto _return = make_shared<MQuaternion>();
	ToMQuaternion(*_return, values);
	return _return;
}

void MQuaternionExtensions::MultiplyToMVector3(MVector3 & _return, const MQuaternion & quat, const MVector3 & vec)
{
	double num = quat.X * 2.0;
	double num2 = quat.Y * 2.0;
	double num3 = quat.Z * 2.0;
	double num4 = quat.X * num;
	double num5 = quat.Y * num2;
	double num6 = quat.Z * num3;
	double num7 = quat.X * num2;
	double num8 = quat.X * num3;
	double num9 = quat.Y * num3;
	double num10 = quat.W * num;
	double num11 = quat.W * num2;
	double num12 = quat.W * num3;

	
	_return.__set_X((1 - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z);
	_return.__set_Y((num7 + num12) * vec.X + (1 - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z);
	_return.__set_Z((num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1 - (num4 + num5)) * vec.Z);
}

shared_ptr<MVector3> MQuaternionExtensions::MultiplyToMVector3(const MQuaternion & quat, const MVector3 & vec)
{
	auto _return = make_shared<MVector3>();
	MultiplyToMVector3(*_return, quat, vec);
	return _return;
}

void MQuaternionExtensions::Multiply(MQuaternion & _return, const MQuaternion & left, const MQuaternion & right)
{
	double x = left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y;
	double y = left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z;
	double z = left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X;
	double w = left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z;

}

shared_ptr<MQuaternion> MQuaternionExtensions::Multiply(const MQuaternion & left, const MQuaternion & right)
{
	auto _return = make_shared<MQuaternion>();
	Multiply(*_return, left, right);
	return _return;
}

