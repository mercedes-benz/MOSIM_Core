// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger, Stephan Adam

// This class provides the required methods for connecting to the Retargeting service
// and defines the methods for utilizing the Retargeting service.

#pragma once

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/MRetargetingService.h"
#include "ThriftClient/ThriftClient.h"

#include "gen-cpp/avatar_constants.h"
#include "gen-cpp/avatar_types.h"

#include "gen-cpp/register_types.h"
#include "gen-cpp/MMIRegisterService.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include <iostream>
#include "Utils/Logger.h"

using namespace std;
using namespace MMIStandard;

class RetargetingAccess
{
public:
    RetargetingAccess( string sessionId, const MIPAddress& mmiRegisterAddress, const int maxTime,
                       const string& _avatarID );

    ~RetargetingAccess();

    MAvatarDescription SetupRetargeting( const MAvatarPosture& globalTarget );
    MAvatarPostureValues RetargetToIntermediate( const MAvatarPosture& globalTarget );
    MAvatarPosture RetargetFromIntermediate(
        const MAvatarPostureValues& intermediatePostureValues );

    /*
    Thrift Interface:
    avatar.MAvatarDescription SetupRetargeting(1:avatar.MAvatarPosture globalTarget),
    avatar.MAvatarPostureValues RetargetToIntermediate(1:avatar.MAvatarPosture globalTarget),
    avatar.MAvatarPosture RetargetToTarget(1:avatar.MAvatarPostureValues intermediatePostureValues),

    */

private:
    string _avatarID;
    string _sessionID;
    MIPAddress _mmiRegisterAddress;
    int _maxTime;

    // Currently, the retargeting Service is identified by name in the registry.
    string RETARGETING_SERVICE_NAME = "Retargeting";

    ThriftClient<MRetargetingServiceClient>* _retargetingClient;
    bool Connect();
};