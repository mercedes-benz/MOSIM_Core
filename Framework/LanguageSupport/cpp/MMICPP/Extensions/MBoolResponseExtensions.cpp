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
