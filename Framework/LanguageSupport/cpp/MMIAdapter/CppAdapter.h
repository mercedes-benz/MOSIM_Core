// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "Adapter/AdapterController.h"
#include "Adapter/FileWatcher.h"
#include "boost/program_options.hpp"
#include "boost/algorithm/string.hpp"
#include <iostream>
#include "Adapter/SessionTools.h"
#include "Access/ServiceAccess.h"
#include "Utils/Logger.h"
#include "boost/exception/diagnostic_information.hpp"
#include "gen-cpp/mmu_types.h"
#include "Adapter/CPPMMUInstantiator.h"
#include "Adapter/SessionData.h"

using namespace MMIStandard;
namespace po = boost::program_options;
using namespace std;