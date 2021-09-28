// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Helper class for creating the properties of the MInstructions

#pragma once

#include <string>
#include <map>
#include <vector>

using namespace std;

class PropertiesCreator
{
public:
    static map<string, string> Create( const vector<string>& properties );
};
