// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once

#include "gen-cpp/core_types.h"
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

