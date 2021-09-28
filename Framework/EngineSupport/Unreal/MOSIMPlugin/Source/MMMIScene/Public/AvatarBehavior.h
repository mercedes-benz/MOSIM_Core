// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Helper class for executing hard coded behaviors (series of MInstructions) without applying Ajan
// Only for testing purposes, unrequired as soon as Ajan is working

#pragma once

#include <vector>
#include <map>

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/mmu_types.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include "Containers/Map.h"
#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "AvatarBehavior.generated.h"

class AMMIAvatar;
class UMMISceneObject;
class ASimulationController;

namespace MMIStandard
{
class mmiConstants;
}

using namespace MMIStandard;
using namespace std;

UCLASS()
class MMISCENE_API AAvatarBehavior : public AActor
{
    GENERATED_BODY()

public:
    // Sets default values for this actor's properties
    AAvatarBehavior();

    // destructor
    ~AAvatarBehavior();

    // storage for the list of instructions for each avatar
    map<string, vector<MInstruction>> InstructionMap = map<string, vector<MInstruction>>();

    //////////////////////////////////////////////////////////////////////
    // Methods for assigning instuctions

    // assignIdleInstruction
    UFUNCTION( CallInEditor, Category = "Instructions" )
    void AssignIdleInstruction();

    // assignWalkInstruction
    UFUNCTION( CallInEditor, Category = "Instructions" )
    void AssignWalkInstruction();
    UPROPERTY( EditAnywhere, Category = "Targets" )
    FString WalkTargetName;
    string WalkTargetNameStr;

    UFUNCTION( CallInEditor, Category = "Instructions" )
    void AssignWalkAndIdleInstruction();

    UFUNCTION( CallInEditor, Category = "Instructions" )
    void AssignReachInstruction();
    UPROPERTY( EditAnywhere, Category = "Targets" )
    FString ReachTargetName;
    string ReachTargetNameStr;

    UFUNCTION( CallInEditor, Category = "Instructions" )
    void AssignCarryInstruction();
    UFUNCTION( CallInEditor, Category = "Instructions" )
    void AssignCarryToTargetInstruction();
    UPROPERTY( EditAnywhere, Category = "Targets" )
    FString CarryToTargetName;
    string CarryToTargetNameStr;
    UPROPERTY( EditAnywhere, Category = "Targets" )
    FString WalkTargetCarryToTargetName;
    string WalkTargetCarryToTargetNameStr;

    UFUNCTION( CallInEditor, Category = "Instructions" )
    void StopInstruction();

    UFUNCTION( CallInEditor, Category = "Assign Avatar" )
    void AssignAvatar();
    UPROPERTY( EditAnywhere, Category = "Assign Avatar" )
    FString AvatarName;

    void AssignAvatarString( string AvatarNameStr );

    // load avatars and objects
    // reference all MMIAvatars and MMISceneObjects in the local Lists
    void GetAvatarsAndObjects();

    bool gameHasStarted;

protected:
    // Called when the game starts or when spawned
    virtual void BeginPlay() override;

private:
    // lists of all MMISceneObjects and MMIAvatars
    vector<AMMIAvatar*> Avatars;
    vector<UMMISceneObject*> SceneObjects;

    // access to the scene
    UWorld* scene;

    // ID of the currently selected avatar
    string currentAvatarID;

    // mmiconstants
    mmiConstants* MMIConstants;

    // simulation controller
    ASimulationController* SimContr;
    bool issetSimContr;

public:
    // Called every frame
    virtual void Tick( float DeltaTime ) override;
};
