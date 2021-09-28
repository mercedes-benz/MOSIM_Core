// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// This class is the representation of the MSceneObject in the Unreal Engine.
// Allows the user to mark AActors as MSceneObject.
// Makes the required properties of the parent AActor (Root Transform, Colliders, Physics) known
// to the MOSIM framework and transforms them to the format required by MOSIM.

#include "MMISceneObject.h"
#include "MOSIM.h"
#include "SimulationController.h"
#include "UnrealSceneAccess.h"
#include "MMUAccess.h"
#include <regex>
#include <cmath>
#include <algorithm>
#include "Extensions/MVector3Extensions.h"
#include "Components/CapsuleComponent.h"
#include "Components/BoxComponent.h"
#include "Components/SphereComponent.h"

#include "Engine/StaticMeshActor.h"
#include "Templates/Casts.h"
#include "Utils/Logger.h"
#include "EngineUtils.h"

// Sets default values for this component's properties
UMMISceneObject::UMMISceneObject()
    : UpdatePhysicsCurrentFrame( false ),
      taskEditorID( 0 ),
      localID( 0 ),
      ParentActor( nullptr ),
      rootComp( nullptr ),
      SceneAccess( nullptr ),
      BodyInstance( nullptr ),
      RootPrimitive( nullptr ),
      simController( nullptr ),
      scene( nullptr ),
      mass( 0.f ),
      colliderSet( false ),
      lastGlobalPosition( FVector() ),
      lastGlobalRotation( FQuat() ),
      rootThreshold( 0.1f ),
      physicsUpdateCounter( 0 ),
      lastVelocity( FVector() ),
      lastAngularVelocity( FVector() ),
      lastCenterOfMass( FVector() ),
      lastInertia( FVector() ),
      lastMass( 0.f ),
      physicsThreshold( 0.1f ),
      isInitialized( false ),
      parentHasStaticMesh( false ),
      parentHasSkeletalMesh( false ),
      parentHasPrimitiveComponent( false ),
      hasParentMMISceneObject( false ),
      rootIsPrimitive( false )
{
    // Set this component to be initialized when the game starts, and to be ticked every frame.  You
    // can turn these features
    // off to improve performance if you don't need them.
    PrimaryComponentTick.bCanEverTick = true;

    // update the component during every tick
    PrimaryComponentTick.SetTickFunctionEnable( true );

    // get the root component of the top level actor
    ParentActor = GetAttachmentRootActor();
    if( ParentActor != nullptr )
    {
        // use the parents root component for updating the physics, colliders, etc.
        rootComp = ParentActor->GetRootComponent();
        if( rootComp == nullptr )
        {
            UE_LOG( LogMOSIM, Warning, TEXT( "Owner has no root component." ) );
        }
        // check, if parent has a static mesh
        ParentActor->GetComponents<UStaticMeshComponent>( StaticMeshComponents );
        if( StaticMeshComponents.Num() > 0 )
        {
            parentHasStaticMesh = true;
            parentHasMesh = true;

            // check if the parent has a skeletal mesh
            ParentActor->GetComponents<USkeletalMeshComponent>( SkeletalMeshComponents );
            if( SkeletalMeshComponents.Num() > 0 )
            {
                parentHasSkeletalMesh = true;
            }
        }
        else
        {
            // check if the parent has a skeletal mesh
            ParentActor->GetComponents<USkeletalMeshComponent>( SkeletalMeshComponents );
            if( SkeletalMeshComponents.Num() > 0 )
            {
                parentHasSkeletalMesh = true;
                parentHasMesh = true;
            }
        }
        // check, whether the owner has a primitive component
        ParentActor->GetComponents<UPrimitiveComponent>( PrimitiveComponents );
        if( PrimitiveComponents.Num() > 0 )
        {
            parentHasPrimitiveComponent = true;
        }
        // TODO: refering to the first found primitive component as the primitive component of the
        // root component is prone to errors, however, it seems to work so far
        // TODO: attempt to exclude by refering to the wrong primitive component as the root
        // components primitive component by throwing an error if the AActor has more than one
        // primitive component
        // if (PrimitiveComponents.Num() > 1) { UE_LOG(LogMOSIM, Warning, TEXT("Marked SceneObject
        // has more than one primitive component. The UE-MOSIM plugin is currently not able to
        // handle this.")); }
    }
    else
    {
        UE_LOG( LogMOSIM, Warning, TEXT( "MMISceneObject component is not set, has no owner." ) );
        // runtime_error("Component is not set, has no owner.");
    }

    // TODO: call to the BodyInstance how it should be done, however, it does not work, see line 110
    // get the body instance
    // if (parentHasPrimitiveComponent) {
    //	BodyInstance = PrimitiveComponents[0]->GetBodyInstance();
    //}

    UPrimitiveComponent* PrimitiveComponent = Cast<UPrimitiveComponent>( this->rootComp );
    if( PrimitiveComponent )
    {
        rootIsPrimitive = true;
        // TODO: actually, the BodyInstance should be derived from the Primitive Component from the
        // following line and not from PrimitiveComponents[0], but unfortunately, when queries are
        // made to the BodyInstance, only nonsense is returned e.g. for the mass, and the other
        // properties.
        // FName ComponentTypeName = PrimitiveComponent->GetFName();
        // RootPrimitive = PrimitiveComponent;
        // BodyInstance = PrimitiveComponent->GetBodyInstance();
        // UE_LOG(LogMOSIM, Warning, TEXT("Result Cast %s"), *ComponentTypeName.ToString());
        RootPrimitive = PrimitiveComponents[0];
        BodyInstance = PrimitiveComponents[0]->GetBodyInstance();
        // ComponentTypeName = PrimitiveComponents[0]->GetFName();
        // UE_LOG(LogMOSIM, Warning, TEXT("Ergebnis Array %s"), *ComponentTypeName.ToString());
    }
    else
        UE_LOG( LogMOSIM, Warning,
                TEXT( "Root Component of the MMISceneObject has to be a UPrimitiveComponent." ) );

    // make the static mesh movable
    if( this->parentHasStaticMesh )
    {
        for( UStaticMeshComponent* statMeshComp : StaticMeshComponents )
        {
            statMeshComp->SetMobility( EComponentMobility::Movable );
            statMeshComp->SetSimulatePhysics( true );
        }
    }

    // set the collison for the whole actor
    if( ParentActor != nullptr )
    {
        ParentActor->SetActorEnableCollision( true );
    }

    // set the MMISceneObject Name (has to be unique, therfore actor Name)
    // and the MSceneObject Name (not necessarily unique), has to be equal to the actor label
    if( ParentActor != nullptr )
    {
        this->SetupBase();
    }

    // Create a new UUID for the MSceneObject
    this->MSceneObject.__set_ID( MMUAccess::GetNewGuid() );
}

UMMISceneObject::~UMMISceneObject()
{
    // unregister the SceneObject at the UnrealSceneAccess
    if( this->SceneAccess )
    {
        this->SceneAccess->RemoveSceneObject_SceneUpdate( this->MSceneObject );
        this->SceneAccess->RemoveMMISceneObject( this->MSceneObject.ID );
    }
}

// Called when the game starts
void UMMISceneObject::BeginPlay()
{
    Super::BeginPlay();
}

void UMMISceneObject::Setup( UnrealSceneAccess* _SceneAccess )
{
    // do the base settings for the setup
    this->SetupBase();

    switch( this->SceneObjectType )
    {
        case SceneObjectType::Tool:
            // switch out unreal physics interactions
            for( UPrimitiveComponent* primiComp : this->PrimitiveComponents )
            {
                primiComp->SetSimulatePhysics( true );
                primiComp->SetEnableGravity( false );
            }
            break;
        case SceneObjectType::WalkTarget:
            break;
        default:
            this->SetupCollider();
            break;
    }

    // get access to the UnrealSceneAccess
    this->SceneAccess = _SceneAccess;

    // Add the scene object to the scene access
    this->SceneAccess->AddSceneObject_SceneUpdate( this->MSceneObject );

    this->isInitialized = true;
}

void UMMISceneObject::SetupBase()
{
    // set the transform of the MSceneObject
    this->SetupTransform();

    // update the names of the object
    this->updateNames();

    // set the physics, update the root transform history and the physics history
    if( this->ParentActor )
    {
        this->SetupPhysics();
        this->UpdateRootHistory();
        this->UpdatePhysicsHistory();
    }
}

// Called every frame
void UMMISceneObject::TickComponent( float DeltaTime, ELevelTick TickType,
                                     FActorComponentTickFunction* ThisTickFunction )
{
    if( !this->isInitialized )
        return;

    Super::TickComponent( DeltaTime, TickType, ThisTickFunction );

    if( this->HasRootChanged() )
    {
        // sequence of the root comp update
        // UnrealSceneAccess writes into the MSceneObject.Transform and passes the values to the
        // root component of the owner AActor
        // via calling MMISceneObject::UpdateRootComponent()
        // Here, the MSceneObject.Transform is updated according to the owner AActors root
        // component, when something has changed.
        // No matter, whether the engine or MOSIM changed the owners root transform

        // update the MSceneObject
        this->UpdateMSceneObjTransform( this->MSceneObject.Transform );

        // notify unreal scene access about the changed transformation
        if( this->SceneAccess )
            this->SceneAccess->TransformationChanged_SceneUpdate(
                this->MSceneObject, this->MSceneObject.Transform.Position,
                this->MSceneObject.Transform.Rotation, this->MSceneObject.Transform.Parent );
        else
            UE_LOG( LogMOSIM, Warning,
                    TEXT( "Scene Access in %s is not initialized. Simulation is not executable." ),
                    UTF8_TO_TCHAR( this->Name.c_str() ) );

        // update the root component history
        this->UpdateRootHistory();
    }

    // sequence of the physics update
    // UnrealSceneAccess writes into the MSceneObject.PhysicsProperties and passes the values to the
    // owner AActor via calling MMISceneObject::UpdatePhysics(physicsInteraction)
    // Here, the MSceneObject.PhysicsProperties is updated acording to the owner AActors physics,
    // when something has changed
    // No matter, whether the engine or MOSIM changed the owners physics

    if( this->HasPhysicsChanged() )
    {
        // update the MSceneObjects physics according to the owner AActors physics
        this->SetupPhysics();

        // update the physics history
        this->UpdatePhysicsHistory();

        // notify unreal scene access about the changed transformation
        this->SceneAccess->PhysicsPropertiesChanged_SceneUpdate(
            this->MSceneObject, this->MSceneObject.PhysicsProperties );
    }
}

// update root component (location of the Actor/SceneObject)
void UMMISceneObject::UpdateRootComponent()
{
    FVector loc = FVector();
    FQuat rot = FQuat();

    MTransform j = this->MSceneObject.Transform;

    // Due to the coordinate system proble, the virtual root has to be considered separately.
    loc.X = -j.Position.X;
    loc.Y = j.Position.Z;
    loc.Z = j.Position.Y;
    loc = loc * 100.f;

    rot.X = -j.Rotation.X;
    rot.Y = j.Rotation.Z;
    rot.Z = j.Rotation.Y;
    rot.W = j.Rotation.W;

    // the virtual root mapps to the actor location / rotation.
    this->ParentActor->SetActorLocation( loc );
    this->ParentActor->SetActorRotation( rot.Rotator() );
}

// update the physics of the object
void UMMISceneObject::UpdatePhysics( const MPhysicsInteraction& physicsInteraction )
{
    if( !this->rootIsPrimitive || !this->BodyInstance )
        return;

    this->UpdatePhysicsCurrentFrame = true;

    // Apply the manipulation
    switch( physicsInteraction.Type )
    {
        case MPhysicsInteractionType::AddForce:
        {
            FVector forceFVectMOSIM = FVector();
            UMMISceneObject::DoubleVectToFVect( forceFVectMOSIM,
                                                this->MSceneObject.PhysicsProperties.NetForce );
            FVector forceFVect = FVector();
            UMMISceneObject::ConvertFVectMOSIMToUnrealCoord( forceFVect, forceFVectMOSIM );
            this->BodyInstance->AddForce( forceFVect, true, true );
            break;
        }
        case MPhysicsInteractionType::AddTorque:
        {
            FVector torqueFVectMOSIM = FVector();
            UMMISceneObject::DoubleVectToFVect( torqueFVectMOSIM,
                                                this->MSceneObject.PhysicsProperties.NetTorque );
            FVector torqueFVect = FVector();
            UMMISceneObject::ConvertFVectMOSIMToUnrealCoord( torqueFVect, torqueFVectMOSIM );
            this->BodyInstance->AddTorqueInRadians( torqueFVect, true, true );
            break;
        }
        case MPhysicsInteractionType::ChangeAngularVelocity:
        {
            FVector angularVelFVectMOSIM = FVector();
            UMMISceneObject::DoubleVectToFVect(
                angularVelFVectMOSIM, this->MSceneObject.PhysicsProperties.AngularVelocity );
            FVector angularVelFVect = FVector();
            UMMISceneObject::ConvertFVectMOSIMToUnrealCoord( angularVelFVect,
                                                             angularVelFVectMOSIM );
            this->BodyInstance->AddTorqueInRadians( angularVelFVect, true, true );
            break;
        }
        case MPhysicsInteractionType::ChangeCenterOfMass:
        {
            // convert meters to cm
            vector<double> comDouble = this->MSceneObject.PhysicsProperties.CenterOfMass;
            transform( comDouble.begin(), comDouble.end(), comDouble.begin(),
                       []( double& value ) { return value * 100.f; } );
            // convert coordinate systems
            FVector comNewMOSIM = FVector();
            UMMISceneObject::DoubleVectToFVect( comNewMOSIM, comDouble );
            FVector comNew = FVector();
            UMMISceneObject::ConvertFVectMOSIMToUnrealCoord( comNew, comNewMOSIM );
            // calculate the center of mass offset
            FVector comOld = this->BodyInstance->GetCOMPosition();
            FVector offset = UMMISceneObject::SubtractFVectors( comOld, comNew );
            this->RootPrimitive->SetCenterOfMass( offset, FName() );
            break;
        }
        case MPhysicsInteractionType::ChangeInertia:
        {
            // change of the inertia tensor is actually unnecessary
            // should be done either by the bodys shape or by the mass distribution, not directly
            FVector inertiaFVectMOSIM = FVector();
            UMMISceneObject::DoubleVectToFVect( inertiaFVectMOSIM,
                                                this->MSceneObject.PhysicsProperties.Inertia );
            FVector inertiaFVect = FVector();
            UMMISceneObject::ConvertFVectMOSIMToUnrealCoord( inertiaFVect, inertiaFVectMOSIM );
            UE_LOG( LogMOSIM, Warning,
                    TEXT( "Change of the inertia tensor is not implemented so far." ) );
            break;
        }
        case MPhysicsInteractionType::ChangeMass:
        {
            // TODO: check, if the commented lines are necessary
            // this->BodyInstance->bOverrideMass = true;
            this->BodyInstance->SetMassOverride( this->MSceneObject.PhysicsProperties.Mass );
            // this->BodyInstance->UpdateMassProperties();
            break;
        }
        case MPhysicsInteractionType::ChangeVelocity:
        {
            FVector velFVectMOSIM = FVector();
            UMMISceneObject::DoubleVectToFVect( velFVectMOSIM,
                                                this->MSceneObject.PhysicsProperties.Velocity );
            FVector velFVect = FVector();
            UMMISceneObject::ConvertFVectMOSIMToUnrealCoord( velFVect, velFVectMOSIM );
            this->RootPrimitive->AddImpulse( velFVect, FName(), true );
            break;
        }
    }
}

void UMMISceneObject::UpdateMSceneObjTransform( MTransform& transform )
{
    FVector loc;
    FQuat rot;

    // Virtual root does not exist and is defined to transfer the Actor location and rotation to.
    loc = this->ParentActor->GetActorLocation();
    rot = this->ParentActor->GetActorRotation().Quaternion();

    // there has to be a scaling factor of 100 to transfer to MOSIM.
    loc = loc / 100.f;

    // This is the standard transfer between UE4 -> MOSIM coordinate system.
    // UE4: (Left, Forward, Up), MOSIM: (Right, Up, Forward), both Left-Handed Coordinate Systems.
    transform.Position.X = -loc.X;
    transform.Position.Y = loc.Z;
    transform.Position.Z = loc.Y;

    transform.Rotation.X = -rot.X;
    transform.Rotation.Y = rot.Z;
    transform.Rotation.Z = rot.Y;
    transform.Rotation.W = rot.W;
}

void UMMISceneObject::SetupTransform()
{
    // Create MTransform stub based on the global reference posture
    MTransform transform = MTransform();
    transform.ID = this->MSceneObject.ID;
    transform.Position = MVector3();
    transform.Rotation = MQuaternion();

    // set the transform according to the current position
    this->UpdateMSceneObjTransform( transform );
    this->MSceneObject.__set_Transform( transform );

    // set the parent, if available
    this->SetParent();
}

void UMMISceneObject::SetParent()
{
    // set the parent if available
    USceneComponent* ParentComponent = this->GetAttachParent();
    int counter = 0;

    while( ParentComponent != nullptr )
    {
        if( counter > 0 )
            ParentComponent = ParentComponent->GetAttachParent();

        UMMISceneObject* parentMMISceneObject = Cast<UMMISceneObject>( ParentComponent );
        if( parentMMISceneObject != nullptr )
        {
            this->MSceneObject.Transform.__set_Parent( parentMMISceneObject->MSceneObject.ID );
            this->MSceneObject.Transform.__isset.Parent = true;
            this->hasParentMMISceneObject = true;
            break;
        }
        else
        {
            counter++;
            continue;
        }
    }
}

void UMMISceneObject::ChangeParent( string newParentID )
{
    vector<UMMISceneObject*> mmiSceneObjects = this->SceneAccess->GetMMISceneObjectsVector();
    bool found = false;
    for( UMMISceneObject* mmiSceneObject : mmiSceneObjects )
    {
        if( mmiSceneObject->MSceneObject.ID == newParentID )
        {
            this->MSceneObject.Transform.__set_Parent( newParentID );
            found = true;
        }
    }
    if( !found )
    {
        string message = "New Parent UMMISceneObject with name " + newParentID + " not found.";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
        Logger::printLog( L_ERROR, message );
    }
}

void UMMISceneObject::SetupCollider()
{
    MCollider collider{};
    collider.__set_ID( this->MSceneObject.ID );

    // look for a box component
    TArray<UBoxComponent*> BoxComponents;
    this->ParentActor->GetComponents<UBoxComponent>( BoxComponents );
    if( BoxComponents.Num() > 0 && !this->colliderSet )
    {
        // set simulate physics to true
        BoxComponents[0]->SetSimulatePhysics( true );

        // set the collider type
        collider.__set_Type( MColliderType::Box );
        // get the colliders properties
        FVector boxExtentFVect = BoxComponents[0]->GetUnscaledBoxExtent();
        // calculate the box size
        vector<double> boxExtent( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( boxExtent, boxExtentFVect );
        transform( boxExtent.begin(), boxExtent.end(), boxExtent.begin(),
                   []( double& value ) { return value * 0.02f; } );

        // convert to MVector3
        MVector3 boxExtentMVect;
        UMMISceneObject::FVectToMVect3( boxExtentMVect, boxExtentFVect );
        // Convert to the MOSIM coordinate System
        MVector3 boxExtentMVectMOSIM;
        boxExtentMVectMOSIM.__set_X( boxExtentMVect.X );
        boxExtentMVectMOSIM.__set_Y( boxExtentMVect.Z );
        boxExtentMVectMOSIM.__set_Z( boxExtentMVect.Y );

        MBoxColliderProperties boxCollProps{};
        boxCollProps.__set_Size( boxExtentMVectMOSIM );
        collider.__set_BoxColliderProperties( boxCollProps );
        collider.__isset.BoxColliderProperties = true;

        this->MSceneObject.__set_Collider( collider );
        this->MSceneObject.__isset.Collider = true;

        // set offset to zero by default
        MVector3 offset = MVector3{};
        offset.__set_X( 0.f );
        offset.__set_Y( 0.f );
        offset.__set_Z( 0.f );
        collider.__set_PositionOffset( offset );
        collider.__isset.PositionOffset = true;

        this->colliderSet = true;
    }

    // look for a sphere component
    TArray<USphereComponent*> SphereComponents;
    this->ParentActor->GetComponents<USphereComponent>( SphereComponents );
    if( SphereComponents.Num() > 0 && !this->colliderSet )
    {
        // set simulate physics to true
        SphereComponents[0]->SetSimulatePhysics( true );

        // set the collider type
        collider.__set_Type( MColliderType::Sphere );
        // get the colliders properties
        float radius = SphereComponents[0]->GetUnscaledSphereRadius();
        radius = radius * 0.01f;

        MSphereColliderProperties sphereCollProps = MSphereColliderProperties{};
        sphereCollProps.__set_Radius( static_cast<double>( radius ) );
        collider.__set_SphereColliderProperties( sphereCollProps );
        collider.__isset.SphereColliderProperties = true;

        this->MSceneObject.__set_Collider( collider );
        this->MSceneObject.__isset.Collider = true;

        // set offset to zero by default
        MVector3 offset = MVector3{};
        offset.__set_X( 0.f );
        offset.__set_Y( 0.f );
        offset.__set_Z( 0.f );
        collider.__set_PositionOffset( offset );
        collider.__isset.PositionOffset = true;

        this->colliderSet = true;
    }

    // look for a capsule component
    TArray<UCapsuleComponent*> CapsuleComponents;
    this->ParentActor->GetComponents<UCapsuleComponent>( CapsuleComponents );
    if( CapsuleComponents.Num() > 0 && !this->colliderSet )
    {
        // set simulate physics to true
        CapsuleComponents[0]->SetSimulatePhysics( true );

        // set the collider type
        collider.__set_Type( MColliderType::Capsule );
        // get the colliders properties
        float height = CapsuleComponents[0]->GetUnscaledCapsuleHalfHeight() * 0.02f;
        float radius = CapsuleComponents[0]->GetUnscaledCapsuleRadius() * 0.01f;

        MCapsuleColliderProperties capsuleCollProps = MCapsuleColliderProperties{};
        capsuleCollProps.__set_Radius( static_cast<double>( height ) );
        capsuleCollProps.__set_Height( static_cast<double>( radius ) );

        // TODO: check, if there is a main axis
        // capsuleCollProps.__set_MainAxis(boxExtentMVect);
        collider.__set_CapsuleColliderProperties( capsuleCollProps );
        collider.__isset.CapsuleColliderProperties = true;

        this->MSceneObject.__set_Collider( collider );
        this->MSceneObject.__isset.Collider = true;

        // set offset to zero by default
        MVector3 offset = MVector3{};
        offset.__set_X( 0.f );
        offset.__set_Y( 0.f );
        offset.__set_Z( 0.f );
        collider.__set_PositionOffset( offset );
        collider.__isset.PositionOffset = true;

        this->colliderSet = true;
    }

    // default: BoundingBox
    if( this->parentHasStaticMesh && !this->colliderSet )
    {
        // global coordinates
        const FTransform WorldTransform = this->StaticMeshComponents[0]->GetComponentTransform();
        FBoxSphereBounds WorldBounds = this->StaticMeshComponents[0]->CalcBounds( WorldTransform );
        // set the collider type
        collider.__set_Type( MColliderType::Box );
        // get the colliders size and the offset
        // first the size
        // calculate the box size
        vector<double> boxExtent( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( boxExtent, WorldBounds.BoxExtent );
        transform( boxExtent.begin(), boxExtent.end(), boxExtent.begin(),
                   []( double& value ) { return value * 0.02f; } );
        // Convert to MVector3
        MVector3 boxExtentMVect = MVector3{};
        MVector3Extensions::ToMVector3( boxExtentMVect, boxExtent );

        // Transformation to the MOSIM coordinate system
        MVector3 boxExtentMVectMOSIM = MVector3{};
        // transfer the coordinates
        boxExtentMVectMOSIM.__set_X( boxExtentMVect.X );
        boxExtentMVectMOSIM.__set_Y( boxExtentMVect.Z );
        boxExtentMVectMOSIM.__set_Z( boxExtentMVect.Y );

        MBoxColliderProperties boxCollProps = MBoxColliderProperties{};
        boxCollProps.__set_Size( boxExtentMVectMOSIM );
        collider.__set_BoxColliderProperties( boxCollProps );
        collider.__isset.BoxColliderProperties = true;

        // then the offset
        vector<double> origin( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( origin, WorldBounds.Origin );
        transform( origin.begin(), origin.end(), origin.begin(),
                   []( double& value ) { return value * 0.01f; } );
        MVector3 originMVect = MVector3{};
        MVector3Extensions::ToMVector3( originMVect, origin );
        MVector3 originMVectMOSIM = MVector3{};
        UMMISceneObject::ConvertMVectUnrealToMOSIMCoord( originMVectMOSIM, originMVect );
        MVector3 offset = MVector3{};
        MVector3Extensions::Subtract( offset, this->MSceneObject.Transform.Position,
                                      originMVectMOSIM );

        collider.__set_PositionOffset( offset );
        collider.__isset.PositionOffset = true;

        this->MSceneObject.__set_Collider( collider );
        this->MSceneObject.__isset.Collider = true;

        this->colliderSet = true;
    }
    else if( this->parentHasSkeletalMesh && !this->colliderSet )
    {
        // global coordinates
        const FTransform WorldTransform = this->SkeletalMeshComponents[0]->GetComponentTransform();
        FBoxSphereBounds WorldBounds =
            this->SkeletalMeshComponents[0]->CalcBounds( WorldTransform );
        // set the collider type
        collider.__set_Type( MColliderType::Box );
        // get the colliders size and the offset
        // first the size
        // calculate the box size
        vector<double> boxExtent( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( boxExtent, WorldBounds.BoxExtent );
        transform( boxExtent.begin(), boxExtent.end(), boxExtent.begin(),
                   []( double& value ) { return value * 0.02f; } );

        // Convert to MVector3
        MVector3 boxExtentMVect = MVector3{};
        MVector3Extensions::ToMVector3( boxExtentMVect, boxExtent );

        // Transformation to the MOSIM coordinate system
        MVector3 boxExtentMVectMOSIM = MVector3{};
        // transfer the coordinates
        boxExtentMVectMOSIM.__set_X( boxExtentMVect.X );
        boxExtentMVectMOSIM.__set_Y( boxExtentMVect.Z );
        boxExtentMVectMOSIM.__set_Z( boxExtentMVect.Y );

        MBoxColliderProperties boxCollProps = MBoxColliderProperties{};
        boxCollProps.__set_Size( boxExtentMVectMOSIM );
        collider.__set_BoxColliderProperties( boxCollProps );
        collider.__isset.BoxColliderProperties = true;

        // then the offset
        vector<double> origin( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( origin, WorldBounds.Origin );
        transform( origin.begin(), origin.end(), origin.begin(),
                   []( double& value ) { return value * 0.01f; } );
        MVector3 originMVect = MVector3{};
        MVector3Extensions::ToMVector3( originMVect, origin );
        MVector3 offset = MVector3{};
        MVector3Extensions::Subtract( offset, this->MSceneObject.Transform.Position, originMVect );
        collider.__set_PositionOffset( offset );
        collider.__isset.PositionOffset = true;

        this->MSceneObject.__set_Collider( collider );
        this->MSceneObject.__isset.Collider = true;

        this->colliderSet = true;
    }
}

void UMMISceneObject::SetupPhysics()
{
    MPhysicsProperties physProps = MPhysicsProperties();

    if( this->rootIsPrimitive && this->BodyInstance )
    {
        // calculate the overall mass of the object
        this->mass = this->BodyInstance->GetBodyMass();

        // set the massoverride according to the total mass in the first call of the method
        if( this->physicsUpdateCounter == 0 )
            this->BodyInstance->SetMassOverride( mass, true );

        // set the mass of the object
        physProps.__set_Mass( this->mass );

        // get the center of mass
        FVector massCenterFVect = this->BodyInstance->GetCOMPosition();

        // set the center of mass
        FVector massCenterFVectMOSIM = FVector();
        UMMISceneObject::ConvertFVectUnrealToMOSIMCoord( massCenterFVectMOSIM, massCenterFVect );
        vector<double> massCenter( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( massCenter, massCenterFVectMOSIM );
        transform( massCenter.begin(), massCenter.end(), massCenter.begin(),
                   []( double& value ) { return value * 0.01f; } );
        physProps.__set_CenterOfMass( massCenter );

        // get the inertia tensor
        // TODO: is in local mass space --> whatever that is, clarify, what MOSIM expects
        FVector inertiaTensorF = this->BodyInstance->GetBodyInertiaTensor();
        FVector inertiaTensorFMOSIM = FVector();
        UMMISceneObject::ConvertFVectUnrealToMOSIMCoord( inertiaTensorFMOSIM, inertiaTensorF );
        vector<double> inertiaTensor( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( inertiaTensor, inertiaTensorFMOSIM );
        // convert kgcm^-2 to kgm^-2
        transform( inertiaTensor.begin(), inertiaTensor.end(), inertiaTensor.begin(),
                   []( double& value ) { return value * 1e4f; } );
        physProps.__set_Inertia( inertiaTensor );

        // get the angular velocity
        // TODO: is in global coordinates, clarify, what MOSIM expects
        FVector angularVelF = this->BodyInstance->GetUnrealWorldAngularVelocityInRadians();
        FVector angularVelFMOSIM = FVector();
        UMMISceneObject::ConvertFVectUnrealToMOSIMCoord( angularVelFMOSIM, angularVelF );
        vector<double> angularVel( 3, 0.f );
        UMMISceneObject::FVectToDoubleVect( angularVel, angularVelFMOSIM );
        physProps.__set_AngularVelocity( angularVel );
    }

    // TODO: unclear, if this code is really neccessary, returns currently the same value as above
    if( this->parentHasSkeletalMesh )
    {
        // get the center of mass
        FVector massCenterFVect = this->SkeletalMeshComponents[0]->GetSkeletalCenterOfMass();
        // set the center of mass
        FVector massCenterFVectMOSIM = FVector();
        UMMISceneObject::ConvertFVectUnrealToMOSIMCoord( massCenterFVectMOSIM, massCenterFVect );
        vector<double> massCenter( 3 );
        UMMISceneObject::FVectToDoubleVect( massCenter, massCenterFVectMOSIM );
        transform( massCenter.begin(), massCenter.end(), massCenter.begin(),
                   []( double& value ) { return value * 0.01f; } );
        physProps.__set_CenterOfMass( massCenter );
    }

    // set the velocity
    FVector velFVect = this->ParentActor->GetVelocity();
    FVector velFVectMOSIM = FVector();
    UMMISceneObject::ConvertFVectUnrealToMOSIMCoord( velFVectMOSIM, velFVect );
    vector<double> vel( 3 );
    UMMISceneObject::FVectToDoubleVect( vel, velFVectMOSIM );
    // convert cm/s to m/s
    double convFact = 0.01f;
    transform( vel.begin(), vel.end(), vel.begin(),
               [convFact]( double& c ) { return c * convFact; } );
    physProps.__set_Velocity( vel );

    // add the physics properties to the MSceneObject
    this->MSceneObject.__set_PhysicsProperties( physProps );
    this->MSceneObject.__isset.PhysicsProperties = true;
}

// Update the MSceneObject Name and set the Name of the MMISceneObject accordingly
void UMMISceneObject::setMSceneObjectName( string _Name )
{
    this->MSceneObject.__set_Name( _Name );

    // convert the name to an FString
    FString NameFStr( _Name.c_str() );
    FString NameFStrAdapted( "" );
    FString NameFStrAdaptedUnderscore( "" );
    bool nameAdapted = false;

    // find a unique name
    this->scene = this->GetWorld();

    // get a list of all names of actors in the scene
    vector<FString> actorNames = vector<FString>();
    for( TActorIterator<AActor> AActorItr( this->scene ); AActorItr; ++AActorItr )
    {
        actorNames.push_back( AActorItr->GetName() );
    }

    // check, whether the base name is already unique
    if( find( actorNames.begin(), actorNames.end(), NameFStr ) != actorNames.end() )
    {
        int index = 2;
        for( int i = 0; actorNames.size(); i++ )
        {
            NameFStrAdapted = NameFStr + FString::FromInt( index );
            NameFStrAdaptedUnderscore = NameFStr + "_" + FString::FromInt( index );
            if( find( actorNames.begin(), actorNames.end(), NameFStrAdapted ) != actorNames.end() ||
                find( actorNames.begin(), actorNames.end(), NameFStrAdaptedUnderscore ) !=
                    actorNames.end() )
                index++;
            else
                break;
        }
        this->ParentActor->Rename( *NameFStrAdapted );
        this->ParentActor->SetActorLabel( *NameFStr );  // defines label presented in the engine
    }
    else
    {
        this->ParentActor->Rename( *NameFStr );
        this->ParentActor->SetActorLabel( *NameFStr );
    }

    // set the name field according to the FString name of the object (to get the unique name)
    FString NameFStrReturn = ParentActor->GetName();
    // set the string version of the name according to the name of the parent object
    this->Name = string( TCHAR_TO_UTF8( *NameFStrReturn ) );
}

string UMMISceneObject::getNameStr()
{
    // if the MMISceneObject is the topmost object in the hierarchy,
    // give it the parents component name
    // otherwise give it the SceneComponents name
    if( !this->hasParentMMISceneObject )
    {
        // set the Name of the MMISceneObject according to the name of the parent object
        // TODO: ActorLabel returns strange values
        // FString LabelNameFStr = ParentActor->GetActorLabel();
        FString NameFStr = ParentActor->GetName();
        // set the string version of the name according to the name of the parent object
        // string _LabelName = string(TCHAR_TO_UTF8(*LabelNameFStr));
        string _Name = string( TCHAR_TO_UTF8( *NameFStr ) );
        return _Name;
    }
    else
    {
        FString NameFStr = this->GetName();
        string _Name = string( TCHAR_TO_UTF8( *NameFStr ) );
        return _Name;
    }
}

void UMMISceneObject::updateNames()
{
    // first set the MMISceneObject name
    this->Name = this->getNameStr();
    // set the name according to the Parents Name and the MMISceneObject Name (without the digits at
    // the end identifying copies of actors in Unreal)
    // check if the name contains an underscore followed by digits

    if( regex_match( this->Name, regex( "^(.+_)[0-9]*$" ) ) )
    {
        regex regexp( "_[0-9]*$" );
        string mSceneObjName = regex_replace( this->Name, regexp, "" );
        this->MSceneObject.__set_Name( mSceneObjName );
    }
    else
        this->MSceneObject.__set_Name( this->Name );
}

//////////////////////////////////////////////////////////////////////////////////////////////
// static methods for conversions

void UMMISceneObject::DoubleVectToFVect( FVector& fVect, const vector<double>& doubleVect )
{
    if( doubleVect.size() != 3 )
    {
        throw runtime_error( "Can not update double vector: Input needs to have exactly 3 values" );
    }
    fVect.X = doubleVect[0];
    fVect.Y = doubleVect[1];
    fVect.Z = doubleVect[2];
}

void UMMISceneObject::FVectToDoubleVect( vector<double>& doubleVect, const FVector& fVect )
{
    if( doubleVect.size() != 3 )
    {
        throw runtime_error( "Can not update double vector: Input needs to have exactly 3 values" );
    }
    doubleVect[0] = fVect.X;
    doubleVect[1] = fVect.Y;
    doubleVect[2] = fVect.Z;
}

void UMMISceneObject::FVectToMVect3( MVector3& mVect, const FVector& fVect )
{
    mVect.__set_X( fVect.X );
    mVect.__set_Y( fVect.Y );
    mVect.__set_Z( fVect.Z );
}

void UMMISceneObject::ConvertMVectUnrealToMOSIMCoord( MVector3& mosimLoc,
                                                      const MVector3& unrealLoc )
{
    mosimLoc.X = -unrealLoc.X;
    mosimLoc.Y = unrealLoc.Z;
    mosimLoc.Z = unrealLoc.Y;
}

void UMMISceneObject::ConvertFVectUnrealToMOSIMCoord( FVector& mosimLoc, const FVector& unrealLoc )
{
    mosimLoc.X = -unrealLoc.X;
    mosimLoc.Y = unrealLoc.Z;
    mosimLoc.Z = unrealLoc.Y;
}

void UMMISceneObject::ConvertFVectMOSIMToUnrealCoord( FVector& unrealLoc, const FVector& mosimLoc )
{
    unrealLoc.X = -mosimLoc.X;
    unrealLoc.Y = mosimLoc.Z;
    unrealLoc.Z = mosimLoc.Y;
}

FVector UMMISceneObject::SubtractFVectors( const FVector& fVect1, const FVector& fVect2 )
{
    FVector _return = FVector();
    _return.X = fVect1.X - fVect2.X;
    _return.Y = fVect1.Y - fVect2.Y;
    _return.Z = fVect1.Z - fVect2.Z;
    return _return;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////
// methods to track the transform of the root component

// Indicates whether changes occured since the last frame
bool UMMISceneObject::HasRootChanged()
{
    bool hasChanged = false;

    ////Check if the position has changed
    if( abs( FVector::Dist( this->lastGlobalPosition, this->ParentActor->GetActorLocation() ) ) >
        this->rootThreshold )
        hasChanged = true;

    ////Check if the rotation has changed
    FQuat globalRotation = this->ParentActor->GetActorRotation().Quaternion();
    if( abs( ( this->lastGlobalRotation.GetAngle() - globalRotation.GetAngle() ) ) >
        this->rootThreshold )
        hasChanged = true;

    // check, if the parent has changed
    if( this->hasParentMMISceneObject )
    {
        vector<UMMISceneObject*> MMISceneObjects = this->SceneAccess->GetMMISceneObjectsVector();
        vector<UMMISceneObject*>::iterator it = find_if(
            MMISceneObjects.begin(), MMISceneObjects.end(), [this]( UMMISceneObject* object ) {
                return object->MSceneObject.ID == this->MSceneObject.Transform.Parent;
            } );
        if( it != MMISceneObjects.end() )
        {
            UMMISceneObject* parentMMISceneObject = *it;

            ////Check if the position has changed
            if( abs( FVector::Dist( parentMMISceneObject->lastGlobalPosition,
                                    parentMMISceneObject->ParentActor->GetActorLocation() ) ) >
                this->rootThreshold )
                hasChanged = true;

            ////Check if the rotation has changed
            FQuat globalParentRotation =
                parentMMISceneObject->ParentActor->GetActorRotation().Quaternion();
            if( abs( ( parentMMISceneObject->lastGlobalRotation.GetAngle() -
                       globalParentRotation.GetAngle() ) ) > this->rootThreshold )
                hasChanged = true;
        }
        else
        {
            string message =
                "Could not find Parent MMISceneObject of Object " + this->MSceneObject.Name + "!";
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *FString( message.c_str() ) );
            Logger::printLog( L_INFO, message );
        }
    }

    return hasChanged;
}

// Updates the internal state according to the specified transform
void UMMISceneObject::UpdateRootHistory()
{
    // Update the transform in each frame
    this->lastGlobalPosition = this->ParentActor->GetActorLocation();
    this->lastGlobalRotation = this->ParentActor->GetActorRotation().Quaternion();
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////
// methods to track the changes of the physical properties

// updates the physics history
void UMMISceneObject::UpdatePhysicsHistory()
{
    if( this->rootIsPrimitive && this->BodyInstance )
    {
        this->lastAngularVelocity = this->BodyInstance->GetUnrealWorldAngularVelocityInRadians();
        this->lastCenterOfMass = this->BodyInstance->GetCOMPosition();
        this->lastInertia = this->BodyInstance->GetBodyInertiaTensor();

        if( this->physicsUpdateCounter == 0 )
        {
            this->lastMass = this->BodyInstance->GetBodyMass();
            float massCompare = this->BodyInstance->GetMassOverride();
        }
        else
        {
            // TODO: check, which mass is the one, that gets actually changed
            this->lastMass = this->BodyInstance->GetMassOverride();
            // this->lastMass = this->BodyInstance->GetBodyMass();
        }
    }

    this->lastVelocity = this->ParentActor->GetVelocity();

    this->physicsUpdateCounter++;
}

// check, whether the physics has changed
bool UMMISceneObject::HasPhysicsChanged()
{
    bool hasChanged = false;

    if( this->rootIsPrimitive && this->BodyInstance )
    {
        if( abs( FVector::Dist( this->lastAngularVelocity,
                                this->BodyInstance->GetUnrealWorldAngularVelocityInRadians() ) ) >
            this->physicsThreshold )
            hasChanged = true;

        if( abs( FVector::Dist( this->lastCenterOfMass, this->BodyInstance->GetCOMPosition() ) ) >
            this->physicsThreshold )
            hasChanged = true;

        if( abs( FVector::Dist( this->lastInertia, this->BodyInstance->GetBodyInertiaTensor() ) ) >
            this->physicsThreshold )
            hasChanged = true;

        if( this->physicsUpdateCounter == 0 )
        {
            if( abs( this->lastMass - this->BodyInstance->GetBodyMass() ) > this->physicsThreshold )
                hasChanged = true;
        }
        else
        {
            float newMass = this->BodyInstance->GetMassOverride();
            float diffMass = abs( this->lastMass - this->BodyInstance->GetBodyMass() );
            if( diffMass > this->physicsThreshold )
                hasChanged = true;
        }
    }

    FVector velocityNew = this->ParentActor->GetVelocity();
    float diffVector = abs( FVector::Dist( this->lastVelocity, this->ParentActor->GetVelocity() ) );
    if( diffVector > this->physicsThreshold )
        hasChanged = true;

    return hasChanged;
}
