// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Helper class for creating the properties of the MInstructions

#include "PropertiesCreator.h"

map<string, string> PropertiesCreator::Create( const vector<string>& properties )
{
    // check, if the properties vector is of even size
    if( properties.size() % 2 != 0 )
        throw runtime_error( "Properties vector has to be of even length!" );

    map<string, string> _return = map<string, string>();
    for( int i = 0; i < properties.size(); i += 2 )
    {
        _return.insert( pair<string, string>{properties[i], properties[i + 1]} );
    }
    return _return;
}