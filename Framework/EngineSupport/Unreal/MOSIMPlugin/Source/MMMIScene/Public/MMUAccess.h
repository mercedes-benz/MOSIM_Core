// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam, Janis Sprenger

// This class provides the methods for connecting to the Register and
// loading and initializing MMUs. Summarizes the methods from
// RemoteAdapterAccess and MotionModelUnitAccess.

#pragma once

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/MSkeletonAccess.h"
#include "gen-cpp/register_types.h"
#include "gen-cpp/MMIRegisterService.h"
#include "gen-cpp/MSceneAccess.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include "MotionModelUnitAccess.h"

#include <string>
#include <vector>
#include <thread>

using namespace std;
using namespace MMIStandard;

class RemoteAdapterAccess;

class MMUAccess
{
    // private fields
protected:
    // vector<IAdapter> Adapters;

    bool isInitialized;

    void setIsInitialized( bool val )
    {
        isInitialized = val;
    };

    vector<MMUDescription> MMUDescriptions;

public:
    string SessionId;
    string AvatarID;
    vector<RemoteAdapterAccess*> Adapters;
    vector<MotionModelUnitAccess*> MotionModelUnits;
    MSceneAccessIf* SceneAccess;

    vector<MAdapterDescription> adapterDescriptions;

    // constructors
    MMUAccess();
    MMUAccess( string sessionId );

    // destructor
    ~MMUAccess();

    // getters
    bool GetIsInitialized()
    {
        return this->isInitialized;
    }
    vector<MMUDescription> GetMMUDescriptions()
    {
        return MMUDescriptions;
    }
    vector<const MAdapterDescription*> GetAdapterDescriptions();

    bool IsLoaded();

    // connect the given adapters
    bool Connect( const MIPAddress& mmiRegisterAddress, const int maxTime, const string& _avatarID,
                  MAvatarDescription description );

    // returns all loadable MMUs in form of their description
    vector<MMUDescription> GetLoadableMMUsClient();

    // loads the defined MMUs
    bool LoadAllMMUs( vector<MMUDescription> mmuList, int timeout );

    // load specific MMU
    bool LoadSpecificMMU( const MMUDescription& mmuDesc, int timeout );

    // initialize the mmus
    // TODO: change int argument (time until the MMUs have to be initialized in an std::chrono
    // expression)
    bool InitializeAllMMUs( int timeout, const MAvatarDescription& _AvatarID );

    // initialize the mmus
    bool InitializeSpecificMMU( int timeout, const MAvatarDescription& AvatarDescription,
                                MotionModelUnitAccess* mmu,
                                const map<string, string>& initializationProperties );

    // synchronise the scene
    void PushScene( bool transmitFullScene = false );

    // create checkpoints
    map<string, string> CreateCheckpoint( vector<string> mmuIDs );

    // restore checkpoints
    void RestoreCheckpoint( map<string, string> checkpointData );

    // fetch the scene information from the adapter
    vector<MSceneObject> FetchScene();

    // fetch the scene information from the adapter
    MSceneUpdate FetchSceneChanges();

    // generates new guid and returns it as a string
    static string GetNewGuid();

    // controls threads
    static void StopThreads( vector<thread*>& threadVector );
};

// generate one mutex for each adapter to ensure no parallel accesses to the same adapter by the
// MMIAvatars
struct AdapterAccessMutexesStruct
{
    mutex cSharpMtx;
    mutex unityMtx;
    mutex coSimMtx;
};