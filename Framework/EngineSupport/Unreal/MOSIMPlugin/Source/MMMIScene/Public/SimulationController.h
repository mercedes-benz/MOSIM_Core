// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Central instance for exectuing the simulations. Collects all MAvatars and MScene Objects
// and passes them to the UnrealSceneAccess. Initializes all MAvatars and MSceneObjects during
// BeginPlay() and executes the Simulation in MOSIM during Tick()

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Engine/World.h"
#include "Engine/EngineTypes.h"
#include <string>
#include <map>
#include <vector>
#include <concurrent_unordered_map.h>

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/mmu_types.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include "SimulationController.generated.h"

using namespace std;
using namespace MMIStandard;
using namespace Concurrency;

// forward declaration
class AMMIAvatar;
class UnrealSceneAccess;
class MMISettings;
class UMMISceneObject;
class AAvatarBehavior;

UCLASS( HideCategories = ( Input, Actor, LOD ) )
class MMISCENE_API ASimulationController : public AActor
{
    GENERATED_BODY()

public:
    // Sets default values for this actor's properties
    ASimulationController();
    // destructor
    ~ASimulationController();
    // Flag specified whether a new unique session ID is created at the beginning of the execution
    bool AutoCreateSessionID;
    //// The unique session id
    vector<string> SessionId;
    // Specifies whether the co simulator works in realtime
    bool RealTime;
    // Specifies the fixed frame time (if non realtime mode)
    float FixedStepTime;
    // The amount of physics updates within each frame
    int PhysicsUpdatesPerFrame;
    // All assigned avatars
    vector<AMMIAvatar*> Avatars;

    // start the simulation controller automatically
    bool AutoStart;

    // Instance of the UnrealSceneAccess
    UnrealSceneAccess* UESceneAccess;

    // Instance of MMISettings
    MMISettings* Settings;

    // Access to the Instructions, in case ajan is not coupled
    AAvatarBehavior* Behavior;

    // abort the current instruction during the game
    void StopCurrentInstruction();

protected:
    // Called when the game starts or when spawned
    virtual void BeginPlay() override;

    // Flag which inicates whether the Co Simulator is initialized
    bool initialized;
    // The delta time for the current frame
    float frameDeltaTime;
    // The serialized co-simulation checkpoints -> to be extended inn future
    vector<string> coSimulationCheckpoints;
    // The checkoints of the scenes
    map<string, string> sceneCheckpoints;
    // The present frame number
    int frameNumber;
    // The current fps
    float currentUpdateFPS;

    // Performs a simulation cycle for a single frame
    void DoStep( float time );

    // Pushes the scene to each adapter/MMU
    void PushScene();
    // apply the updates of the scene
    void ApplySceneUpdate();

public:
    // Called every frame
    virtual void Tick( float DeltaTime ) override;

    // setup the simulation controller
    void Setup();

    // update all MMIAvatars and MMISceneObjects in the scene
    void RegisterAllAvatarsAndObjects();

    // exectuted at the end of the game
    virtual void EndPlay( EEndPlayReason::Type EndPlayReason ) override;

    // load and execute the instructions
    void LoadAndExecuteInstructions();

    // current instruction ID
    string currentInstructionID;

private:
    // access to the scene
    UWorld* scene;

    // the walk target object ID
    string walkTargetObjectID;

    // results of the simulation step
    concurrent_unordered_map<AMMIAvatar*, MSimulationResult> resultsMap;

    // execute the instructions
    void ExecuteInstructions( const vector<MInstruction>& _InstructionVector );

    // current avatar in setup loop
    AMMIAvatar* currentAvatar;

    // Final initialization the MMISceneObjects
    void FinalInitializationMMISceneObjects();
};
