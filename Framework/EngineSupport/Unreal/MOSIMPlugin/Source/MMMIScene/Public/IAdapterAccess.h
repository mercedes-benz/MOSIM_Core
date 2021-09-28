// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Interface defining the required methods for classes providing access to the adapters

#pragma once

#include <unordered_map>

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/register_types.h"
#include "Windows\HideWindowsPlatformTypes.h"

using namespace std;
using namespace MMIStandard;

// forward declaration
class MotionModelUnitAccess;

class IAdapter
{
public:
    // public member methods
    virtual ~IAdapter()
    {
    }
    virtual void Start() = 0;
    virtual vector<MotionModelUnitAccess*> CreateMMUConnections(
        string sessionId, vector<MMUDescription> mmuDescriptions ) = 0;
    virtual MBoolResponse PushScene( MSceneUpdate sceneUpdates, string sessionId ) = 0;
    virtual vector<MSceneObject> GetScene( string sessionId ) = 0;
    virtual MSceneUpdate GetSceneChanges( string sessionId ) = 0;
    virtual MBoolResponse CloseConnection() = 0;
    virtual MBoolResponse CreateSession( string sessionId, MAvatarDescription referenceAvatar ) = 0;
    virtual MBoolResponse CloseSession( string sessionId ) = 0;
    virtual MBoolResponse LoadMMUs( vector<string> ids, string sessionId ) = 0;
    virtual map<string, string> GetStatus() = 0;
    virtual string CreateCheckpoint( string mmuID, string checkpointId ) = 0;
    virtual MBoolResponse RestoreCheckpoint( string mmuID, string checkpointID,
                                             string checkPointData ) = 0;

    const bool& GetInitialized()
    {
        return this->Initialized;
    }
    const bool& GetSceneSynchronized()
    {
        return this->SceneSynchronized;
    }
    void SetInitialized( bool _Initialized )
    {
        this->Initialized = _Initialized;
    }
    void SetSceneSynchronized( bool _SceneSynchronized )
    {
        this->SceneSynchronized = _SceneSynchronized;
    }

protected:
    // member types
    bool Initialized;
    bool SceneSynchronized;
    bool loaded;
};