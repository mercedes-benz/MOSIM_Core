// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "CPPMMUInstantiator.h"
#include <iostream>

typedef MotionModelUnitBaseIf* (__cdecl *MMU_factory)();

using namespace std;

std::unique_ptr<MotionModelUnitBaseIf> CPPMMUInstantiator::InstantiateMMU(const string &mmuPath)
{
	DWORD dwError = 0;
	// Load the DLL
	HINSTANCE dll_handle = LoadLibraryA(mmuPath.c_str());
	if (!dll_handle) {
		throw  std::invalid_argument("Unable to load the DLL, wrong path?");
	}

	// Get the function from the DLL
	MMU_factory MMU_func = reinterpret_cast<MMU_factory>(::GetProcAddress(dll_handle, "instantiate"));
	if (!MMU_func) {
		::FreeLibrary(dll_handle);
		throw std::runtime_error("Unable to create the MMU, check the DLL");
	}

	return std::unique_ptr<MotionModelUnitBaseIf> { MMU_func() };
}