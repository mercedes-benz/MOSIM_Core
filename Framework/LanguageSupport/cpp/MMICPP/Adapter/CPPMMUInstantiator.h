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

