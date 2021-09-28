// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// This class provides the required methods for connecting to the Skeleton Access service
// and defines the methods for utilizing the Skeleton Access service.

#pragma once

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/MSkeletonAccess.h"
#include "ThriftClient/ThriftClient.cpp"

#include "gen-cpp/avatar_constants.h"
#include "gen-cpp/avatar_types.h"

#include "gen-cpp/register_types.h"
#include "gen-cpp/MMIRegisterService.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include <iostream>
#include "Utils/Logger.h"

using namespace std;
using namespace MMIStandard;

class SkeletonAccess
{
public:
    SkeletonAccess( string sessionId, const MIPAddress& mmiRegisterAddress, const int maxTime,
                    const string& _avatarID );

    ~SkeletonAccess();

    MIPAddress* skeletonServiceAddr;

    void InitializeAnthropometry( const MAvatarDescription& description );
    MAvatarDescription GetAvatarDescription( const string& avatarID );
    void SetAnimatedJoints( const string& avatarID, const vector<MJointType::type>& joints );
    void SetChannelData( const MAvatarPostureValues& values );
    MAvatarPosture GetCurrentGlobalPosture( const string& avatarID );
    MAvatarPosture GetCurrentLocalPosture( const string& avatarID );
    MAvatarPostureValues GetCurrentPostureValues( MAvatarPostureValues& _return,
                                                  const string& avatarID );
    MAvatarPostureValues GetCurrentPostureValuesPartial( const string& avatarID,
                                                         const vector<MJointType::type>& joints );
    vector<MVector3> GetCurrentJointPositions( const string& avatarID );
    MVector3 GetRootPosition( const string& avatarID );
    MQuaternion GetRootRotation( const string& avatarID );
    MVector3 GetGlobalJointPosition( const string& avatarId, const MJointType::type joint );
    MQuaternion GetGlobalJointRotation( const string& avatarId, const MJointType::type joint );
    MVector3 GetLocalJointPosition( const string& avatarId, const MJointType::type joint );
    MQuaternion GetLocalJointRotation( const string& avatarId, const MJointType::type joint );
    void SetRootPosition( const string& avatarId, const MVector3& position );
    void SetRootRotation( const string& avatarId, const MQuaternion& rotation );
    void SetGlobalJointPosition( const string& avatarId, const MJointType::type joint,
                                 const MVector3& position );
    void SetGlobalJointRotation( const string& avatarId, const MJointType::type joint,
                                 const MQuaternion& rotation );
    void SetLocalJointPosition( const string& avatarId, const MJointType::type joint,
                                const MVector3& position );
    void SetLocalJointRotation( const string& avatarId, const MJointType::type joint,
                                const MQuaternion& rotation );
    MAvatarPostureValues RecomputeCurrentPostureValues( const string& avatarId );

    /*
    Thrift Interface:
    void InitializeAnthropometry(1: avatar.MAvatarDescription description),
    avatar.MAvatarDescription GetAvatarDescription(1: string avatarID),
    void SetAnimatedJoints(1: string avatarID, 2: list<avatar.MJointType> joints),
    void SetChannelData(1: avatar.MAvatarPostureValues values),
    avatar.MAvatarPosture GetCurrentGlobalPosture(1: string avatarID),
    avatar.MAvatarPosture GetCurrentLocalPosture(1: string avatarID),
    avatar.MAvatarPostureValues GetCurrentPostureValues(1: string avatarID),
    avatar.MAvatarPostureValues GetCurrentPostureValuesPartial(1: string avatarID,
    2:list<avatar.MJointType> joints),
    list<math.MVector3> GetCurrentJointPositions(1: string avatarID),
    math.MVector3 GetRootPosition(1: string avatarID),
    math.MQuaternion GetRootRotation(1: string avatarID),
    math.MVector3 GetGlobalJointPosition(1: string avatarId, 2:avatar.MJointType joint),
    math.MQuaternion GetGlobalJointRotation(1: string avatarId, 2:avatar.MJointType joint),
    math.MVector3 GetLocalJointPosition(1: string avatarId, 2: avatar.MJointType joint),
    math.MQuaternion GetLocalJointRotation(1: string avatarId, 2: avatar.MJointType joint),
    void SetRootPosition(1: string avatarId, 2:math.MVector3 position),
    void SetRootRotation(1: string avatarId, 2:math.MQuaternion rotation),
    void SetGlobalJointPosition(1: string avatarId, 2: avatar.MJointType joint, 3:math.MVector3
    position),
    void SetGlobalJointRotation(1: string avatarId, 2: avatar.MJointType joint, 3:math.MQuaternion
    rotation),
    void SetLocalJointPosition(1: string avatarId, 2:avatar.MJointType joint, 3:math.MVector3
    position),
    void SetLocalJointRotation(1: string avatarId, 2:avatar.MJointType joint, 3:math.MQuaternion
    rotation),
    avatar.MAvatarPostureValues RecomputeCurrentPostureValues(1: string avatarId),
    */

private:
    string _avatarID;
    string _sessionID;
    MIPAddress _mmiRegisterAddress;
    int _maxTime;

    // Currently, the retargeting Service is identified by name in the registry.
    string RETARGETING_SERVICE_NAME = "Standalone Skeleton Access";

    ThriftClient<MSkeletonAccessClient>* _skeletonClient;
    bool Connect();
};