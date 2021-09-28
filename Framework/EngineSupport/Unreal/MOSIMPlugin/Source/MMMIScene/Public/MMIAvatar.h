// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam, Janis Sprenger

// This class is the representation of the MOSIM Avatar in the Unreal Engine. It is derivated from
// APawn.
// APawn was favored compared to ACharacter, as APawn allows a simpler C++ based access to the
// skeleton.
// Defines methods for accessing the Avatars skeleton, including coordinate transformations.
// Connects via MMUAccess to the MOSIM framework, loads and initializes the CoSimulationMMU
// Connects to the required services (SkeletonAccess and Retargeting)

#pragma once

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/avatar_types.h"
#include "gen-cpp/MCoSimulationAccess.h"
#include "gen-cpp/MSkeletonAccess.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "Components/PoseableMeshComponent.h"

// load JSON:
#include "CoreMinimal.h"
#include "Misc/FileHelper.h"
#include "Dom/JsonObject.h"
#include "Dom/JsonValue.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"

#include "MMIAvatar.generated.h"

// forward declarations
class ASimulationController;
class UnrealSceneAccess;
class MMUAccess;
class RetargetingAccess;
class SkeletonAccess;

using namespace std;
using namespace MMIStandard;

UCLASS( HideCategories = ( Physics, Collision, LOD, Cooking, Activation ) )
class MMISCENE_API AMMIAvatar : public APawn
{
    GENERATED_BODY()

public:
    // Sets default values for this character's properties (constructor)
    AMMIAvatar();

    // destructor
    ~AMMIAvatar();

    // Poseable Mesh Component containing the rigged avatar.
    UPROPERTY( VisibleAnywhere, Category = "MOSIM Properties" )
    UPoseableMeshComponent* Mesh;

    // Relative path of the reference posture file for MOSIM retargeting service.
    UPROPERTY( EditAnywhere, Category = "MOSIM Properties", Meta = ( FilePathFilter = "mos" ) )
    FFilePath ReferencePostureFile;

    // add bones of the avatar as scene objects
    bool AddBoneSceneObjects;

    // timeout for establishing the connection to the MMI framework and the remote MMUs in seconds
    int Timeout;

    // static storage of the Port Number
    // is incremented by 1 for every MMIAvatar instance and passed to the CoSimulationMMU
    // each CoSimulationMMU starts a remote scene access server that listen on the respective port
    // port is passed to Ajan and Ajan registers at the respective port at the
    // RemoteCoSimulationAccess
    static int RemoteCoSimulationAccessPortIncremented;

    // the port for the external write access
    int RemoteCoSimulationAccessPort;

    // the address for the external write access
    string RemoteCoSimulationAccessAddress;

    // ID of the avatar, ideally automatically generated with a UUID.
    string AvatarID;

    // ID of the session
    string sessionID;

    // basename of the avatar
    string baseName;
    void UpdateBaseName();

    string statusText;

    // Representation of the MAvatar
    MAvatar MAvatar;

    // class providing the access to the MMUs
    MMUAccess* MMUAccessPtr;

    // pointer to the unreal scene access
    UnrealSceneAccess* SceneAccess;

    // access to the simulation controller
    ASimulationController* SimController;

    // the remote skeleton access
    SkeletonAccess* skeletonAccessPtr;

    // Function to read out the current MAvatarPostureValues of the Avatar. Utilizes the Retargeting
    // Service.
    MAvatarPostureValues ReadCurrentPosture();

    // current helper variable to check, if everything is set up and tick can be used.
    bool running;

    // Function to apply Posture Values to the Avatar. Utilizes the Retargeting Service.
    void ApplyPostureValues( MAvatarPostureValues vals );

    // setup method for the avatar, called by the simulation controller at begin play
    bool Setup( MIPAddress registerAddress, string _sessionID, UnrealSceneAccess* _SceneAccess,
                ASimulationController* _SimContr, mutex& mtx, mutex& mtxReta );

    // Called every frame
    virtual void Tick( float DeltaTime ) override;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // test functions

    // test loading MMUs
    UFUNCTION( BlueprintCallable, Category = "MOSIM|Functions" )
    FString BP_LoadingCallback();

    // Test loading the avatar configuration
    UFUNCTION( BlueprintCallable, Category = "MOSIM|Functions" )
    FString BP_LoadAvatarConfig( FString filePath );

    // sending the MInstruction to Idle
    UFUNCTION( BlueprintCallable, Category = "MOSIM|Functions" )
    FString BP_Idle();

    // Example Update function just with the Idle MMU
    UFUNCTION( BlueprintCallable, Category = "MOSIM|Functions" )
    void BP_Update( float dt );

    ////////////////////////////////////////////////////////////////////////////////////////////////

protected:
    // Called when the game starts or when spawned
    virtual void BeginPlay() override;

    // the retrageting access
    RetargetingAccess* retargetingAccessPtr;

    // Loads Avatar Posture from File
    MAvatarPosture LoadAvatarPosture( FString filePath );
    // Reference Posture for Retargeting
    MAvatarPosture GlobalReferencePosture;

private:
    bool isInitialized;
};
