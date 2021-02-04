// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "MBoolResponseExtensions.h"


void MBoolResponseExtensions::Update(MBoolResponse & _return, string & message,bool isSucessfull)
{
	if (_return.__isset.LogData)
	{
		_return.LogData.emplace_back(message);
	}
	else
	{
		_return.__set_LogData(vector<string>{message});
	}
	_return.__set_Successful(isSucessfull);
}
