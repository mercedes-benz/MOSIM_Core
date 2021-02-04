// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "MotionModelUnitBaseIf.h"

using namespace MMIStandard;
using namespace std;

namespace MMIStandard {
	class CPPMMUInstantiator
	{
		/*
			 Class which instantiates basic CPP MMUs
		*/
	public:
		//	Returns the instantiated MMU based on the path
		unique_ptr<MotionModelUnitBaseIf> InstantiateMMU(const string &mmuPath);
	};
}

