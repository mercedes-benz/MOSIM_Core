// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// This class provides the methods for handling the connection to the Adapters.

#pragma once

#include "IAdapterAccess.h"

#include "Windows\AllowWindowsPlatformTypes.h"
#include "ThriftClient/ThriftClient.h"
#include "gen-cpp/MMIAdapter.h"
#include "Windows\HideWindowsPlatformTypes.h"

using namespace MMIStandard;
using namespace std;

// Forward declarations
class MMUAccess;

class RemoteAdapterAccess : public IAdapter
{
private:
    MMUAccess* mmuAccess;

protected:
    MAdapterDescription Description;
    vector<MMUDescription> MMUDescriptions;

public:
    ThriftClient<MMIAdapterClient> thriftClient;
    string Address;
    int Port;
    bool Initialized;
    bool Aborted;
    bool SceneSynchronized;
    bool Loaded;

    // constructor
    RemoteAdapterAccess( string address, int port, MAdapterDescription adapterDescription,
                         MMUAccess* mmuAccess );
    // destructor
    ~RemoteAdapterAccess();

    // getters and setters
    const MAdapterDescription* GetDescription();
    void SetDescription( const MAdapterDescription& _description );
    string GetAdapterName();

    // getter
    vector<MMUDescription> GetLoadableMMUs();
    // getter that calls the thrift client
    vector<MMUDescription> GetLoadableMMUsClient();

    // inherited from IAdapter
    virtual void Start() override;
    virtual vector<MotionModelUnitAccess*> CreateMMUConnections(
        string sessionId, vector<MMUDescription> mmuDescriptions ) override;
    virtual MBoolResponse PushScene( MSceneUpdate sceneUpdates, string sessionId ) override;
    virtual vector<MSceneObject> GetScene( string sessionId ) override;
    virtual MSceneUpdate GetSceneChanges( string sessionId ) override;
    virtual MBoolResponse CloseConnection() override;
    virtual MBoolResponse CreateSession( string sessionId,
                                         MAvatarDescription referenceAvatar ) override;
    virtual MBoolResponse CloseSession( string sessionId ) override;
    virtual MBoolResponse LoadMMUs( vector<string> ids, string sessionId ) override;
    virtual map<string, string> GetStatus() override;
    virtual string CreateCheckpoint( string mmuID, string checkpointId ) override;
    virtual MBoolResponse RestoreCheckpoint( string mmuID, string checkpointID,
                                             string checkPointData ) override;
};
