// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// This class is the representation of the MSceneObject in the Unreal Engine.
// Allows the user to mark AActors as MSceneObject.
// Makes the required properties of the parent AActor (Root Transform, Colliders, Physics) known
// to the MOSIM framework and transforms them to the format required by MOSIM.

#pragma once

#include "CoreMinimal.h"
#include "Components/SceneComponent.h"
#include "GameFramework/Actor.h"
#include "PhysicsEngine/BodyInstance.h"

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/scene_types.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include "MMISceneObject.generated.h"

using namespace MMIStandard;
using namespace std;

// forward declarations
class ASimulationController;
class UnrealSceneAccess;

UENUM()
enum SceneObjectType
{
    SceneObject = 1,
    InitialLocation = 2,
    FinalLocation = 3,
    WalkTarget = 4,
    Area = 5,
    Part = 6,
    Tool = 7,
    Group = 8
};

UCLASS( ClassGroup = ( Custom ), meta = ( BlueprintSpawnableComponent ) )
class MMISCENE_API UMMISceneObject : public USceneComponent
{
    GENERATED_BODY()

public:
    // Sets default values for this component's properties
    UMMISceneObject();

    // destructor
    ~UMMISceneObject();

    bool UpdatePhysicsCurrentFrame;

    MSceneObject MSceneObject;

    // IDs task List Editor, will be handeled by Task List Editor
    unsigned long taskEditorID;  // can repeat
    unsigned long localID;       // should not repeat

    // Name of the MMISceneObject as string
    string Name;
    // Get the GetActorLabel that the user can see in the interface
    string getNameStr();
    // update the names
    void updateNames();

    // Update the MSceneObject Name and set the Name of the MMISceneObject accordingly
    void setMSceneObjectName( string _Name );

    // get the root component
    AActor* ParentActor;
    USceneComponent* rootComp;

    // pointer to the unreal scene access
    UnrealSceneAccess* SceneAccess;

    // static mesh components
    TArray<UStaticMeshComponent*> StaticMeshComponents;

    // skeletal mesh components
    TArray<USkeletalMeshComponent*> SkeletalMeshComponents;

    // primitive components
    TArray<UPrimitiveComponent*> PrimitiveComponents;

    // Class to modify the physics
    FBodyInstance* BodyInstance;

    // primitive component of the root component
    UPrimitiveComponent* RootPrimitive;

    // Triggered at the simulation start by the SimulationController
    void Setup( UnrealSceneAccess* _SceneAccess );

    // Base setup method, required for triggering the setup without the simulation controller
    void SetupBase();

    // set the parent MMISceneObject, if available
    void SetParent();

    // change the parent object to the new object by name
    void ChangeParent( string newParentName );

    // dropdown list for MMISceneObjectTypes
    UPROPERTY( EditAnywhere, BlueprintReadWrite, Category = SceneObjectType )
    TEnumAsByte<SceneObjectType> SceneObjectType = SceneObjectType::SceneObject;

protected:
    // Called when the game starts
    virtual void BeginPlay() override;

private:
    // access to the simulation controller
    ASimulationController* simController;

    // access to the scene
    UWorld* scene;

    // Sets up the transform of the MSceneObject
    void SetupTransform();

    // get the current position of the AActor owning the MMISceneObject component
    void UpdateMSceneObjTransform( MTransform& transform );

    // set up the colliders in the MSceneObject
    void SetupCollider();

    // set up the physics
    void SetupPhysics();

    // mass of the object
    float mass;

    // flag indicating whether a collider was found
    bool colliderSet;

    /////////////////////////////////////////////////////////////////////////////////////
    ///////////// fields to supervise changes of the root component /////////////////////

    // has the root component changed
    bool HasRootChanged();
    // Update the root component history
    void UpdateRootHistory();
    // history of the root component
    FVector lastGlobalPosition;
    FQuat lastGlobalRotation;

    // threshold value above which changes are recognized
    float rootThreshold;

    /////////////////////////////////////////////////////////////////////////////////////
    ///////////// fields to supervise changes of the physics ////////////////////////////

    int physicsUpdateCounter;
    // has the root component changed
    bool HasPhysicsChanged();
    // Update the root component history
    void UpdatePhysicsHistory();
    // history of the physics
    FVector lastVelocity;
    FVector lastAngularVelocity;
    FVector lastCenterOfMass;
    FVector lastInertia;
    float lastMass;

    // threshold value above which changes are recognized
    float physicsThreshold;

    bool isInitialized;

    ////////////////////////////////////////////////////////////////////////////////////

public:
    // Called every frame
    virtual void TickComponent( float DeltaTime, ELevelTick TickType,
                                FActorComponentTickFunction* ThisTickFunction ) override;

    // check, if parent has static mesh
    bool parentHasStaticMesh;

    // check, if the parent has a skeletal mesh
    bool parentHasSkeletalMesh;

    // check, if the parent has a mesh
    bool parentHasMesh;

    // check, if the parent has a primitive component
    bool parentHasPrimitiveComponent;

    // check, if the scene component has a parent MMISceneObject
    bool hasParentMMISceneObject;

    // check, whether the root component is a primitive component
    bool rootIsPrimitive;

    // update root component (location of the Actor/SceneObject)
    void UpdateRootComponent();

    // update the physics of the object
    void UpdatePhysics( const MPhysicsInteraction& physicsInteraction );

    ///////////////////////////////////////////////
    // static methods

    static void DoubleVectToFVect( FVector& fVect, const vector<double>& doubleVect );

    static void FVectToDoubleVect( vector<double>& doubleVect, const FVector& fVect );

    static void FVectToMVect3( MVector3& mVect, const FVector& fVect );

    static void ConvertMVectUnrealToMOSIMCoord( MVector3& mosimLoc, const MVector3& unrealLoc );

    static void ConvertFVectUnrealToMOSIMCoord( FVector& mosimLoc, const FVector& unrealLoc );

    static void ConvertFVectMOSIMToUnrealCoord( FVector& unrealLoc, const FVector& mosimLoc );

    static FVector SubtractFVectors( const FVector& fVect1, const FVector& fVect2 );
};
