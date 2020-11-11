#pragma once

#include "src/core_types.h"
#include <string>

using namespace MMIStandard;
using namespace std;

namespace MMIStandard {
	class MBoolResponseExtensions
	{
		/*
			Class which extends MBoolResponse
		*/
	public:
		MBoolResponseExtensions() = delete;
		// Method which updates the LogData
		static void Update(MBoolResponse &_return, string &Message, bool isSucessfull);
	};
}

