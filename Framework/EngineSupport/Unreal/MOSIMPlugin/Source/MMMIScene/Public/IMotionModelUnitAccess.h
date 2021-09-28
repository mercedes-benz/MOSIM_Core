// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Interface defining the required methods for classes providing access to the mmus

#pragma once

#include <string>

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/MotionModelUnit.h"
#include "gen-cpp/register_types.h"
#include "Windows\HideWindowsPlatformTypes.h"

using namespace std;
using namespace MMIStandard;

class IMotionModelUnitAccess : MotionModelUnitIf
{
protected:
    string ID;
    string name;
    MMUDescription Description;
};
