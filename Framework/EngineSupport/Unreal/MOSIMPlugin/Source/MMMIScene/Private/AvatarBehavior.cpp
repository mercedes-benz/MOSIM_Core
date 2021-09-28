// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Helper class for executing hard coded behaviors (series of MInstructions) without applying Ajan
// Only for testing purposes, unrequired as soon as Ajan is working

#include "AvatarBehavior.h"
#include "MOSIM.h"
#include "SimulationController.h"
#include "UnrealSceneAccess.h"
#include "MMISceneObject.h"
#include "MMIAvatar.h"
#include "gen-cpp/mmi_constants.h"

#include <algorithm>

#include "MMUAccess.h"
#include "PropertiesCreator.h"

#include "Kismet/GameplayStatics.h"
#include "Engine.h"

// Sets default values
AAvatarBehavior::AAvatarBehavior()
    : WalkTargetName( "" ),
      WalkTargetNameStr( "" ),
      ReachTargetName( "" ),
      ReachTargetNameStr( "" ),
      CarryToTargetName( "" ),
      CarryToTargetNameStr( "" ),
      WalkTargetCarryToTargetName( "" ),
      WalkTargetCarryToTargetNameStr( "" ),
      AvatarName( "" ),
      gameHasStarted( false ),
      scene( nullptr ),
      currentAvatarID( "" ),
      MMIConstants( new mmiConstants() ),
      SimContr( nullptr ),
      issetSimContr( false )
{
    // Set this actor to call Tick() every frame.  You can turn this off to improve performance if
    // you don't need it.
    PrimaryActorTick.bCanEverTick = true;
}

AAvatarBehavior::~AAvatarBehavior()
{
    if( this->MMIConstants )
        delete this->MMIConstants;
}

// Called when the game starts or when spawned
void AAvatarBehavior::BeginPlay()
{
    Super::BeginPlay();
}

// Called every frame
void AAvatarBehavior::Tick( float DeltaTime )
{
    Super::Tick( DeltaTime );
}

void AAvatarBehavior::AssignIdleInstruction()
{
    if( this->currentAvatarID.empty() )
    {
        UE_LOG(
            LogMOSIM, Warning,
            TEXT( "No Avatar is assigned. Assign Avatar first before assigning instructions." ) );
        return;
    }

    MInstruction* Instruction = new MInstruction();
    Instruction->__set_ID( MMUAccess::GetNewGuid() );
    Instruction->__set_Name( "Idle" );
    Instruction->__set_MotionType( "Pose/Idle" );
    // save the instruction for the current avatar
    this->InstructionMap[this->currentAvatarID] = vector<MInstruction>{*Instruction};

    FString message = "AssignIdleInstruction: Assigned Idle Instruction.";
    UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
    GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                      FColor::Green, message );

    if( !this->gameHasStarted )
    {
        FString msg = "Assign instruction failed. Please start simulation and try again.";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *msg );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, msg );
        return;
    }

    if( !this->issetSimContr )
    {
        for( TActorIterator<ASimulationController> SimContrItr( this->scene ); SimContrItr;
             ++SimContrItr )
        {
            this->SimContr = *SimContrItr;
            this->issetSimContr = true;
            // Execute the instructions
            this->SimContr->LoadAndExecuteInstructions();
            break;
        }
    }
    else
    {
        // Execute the instructions
        this->SimContr->LoadAndExecuteInstructions();
    }
}

void AAvatarBehavior::AssignWalkInstruction()
{
    if( this->currentAvatarID.empty() )
    {
        UE_LOG(
            LogMOSIM, Warning,
            TEXT( "No Avatar is assigned. Assign Avatar first before assigning instructions." ) );
        return;
    }

    // Walk instruction to walk to a specific target using an MGeometryConstraint
    MInstruction* Instruction = new MInstruction();
    Instruction->__set_ID( MMUAccess::GetNewGuid() );
    Instruction->__set_MotionType( "Locomotion/Walk" );
    Instruction->__set_Name( "Walking to target location" );

    this->WalkTargetNameStr = string( TCHAR_TO_UTF8( *this->WalkTargetName ) );

    // update the local registries of the Avatars and Objects
    this->GetAvatarsAndObjects();

    // find the ID of the walk target
    vector<UMMISceneObject*>::iterator it = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->WalkTargetNameStr;
        } );

    if( it != this->SceneObjects.end() )
    {
        vector<string> propertiesVector = {"TargetID", ( *it )->MSceneObject.ID,
                                           "UseTargetOrientation", "false"};
        map<string, string> properties = PropertiesCreator::Create( propertiesVector );
        Instruction->__set_Properties( properties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID] = vector<MInstruction>{*Instruction};

        FString message = FString::Printf( TEXT( "AssignWalkInstruction: Assigned Walk Instruction "
                                                 "to Target Object with the name %s!" ),
                                           *this->WalkTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, message );

        if( !this->gameHasStarted )
        {
            FString msg = "Assign instruction failed. Please start simulation and try again.";
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *msg );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Green, msg );
            return;
        }

        if( !this->issetSimContr )
        {
            for( TActorIterator<ASimulationController> SimContrItr( this->scene ); SimContrItr;
                 ++SimContrItr )
            {
                this->SimContr = *SimContrItr;
                this->issetSimContr = true;
                // Execute the instructions
                this->SimContr->LoadAndExecuteInstructions();
                break;
            }
        }
        else
        {
            // Execute the instructions
            this->SimContr->LoadAndExecuteInstructions();
        }
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignWalkInstruction: No Target Object with the name %s was found!" ),
            *this->WalkTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }
}

void AAvatarBehavior::AssignWalkAndIdleInstruction()
{
    if( this->currentAvatarID.empty() )
    {
        UE_LOG(
            LogMOSIM, Warning,
            TEXT( "No Avatar is assigned. Assign Avatar first before assigning instructions." ) );
        return;
    }

    // Walk instruction to walk to a specific target using an MGeometryConstraint
    MInstruction* WalkInstruction = new MInstruction();
    WalkInstruction->__set_ID( MMUAccess::GetNewGuid() );
    WalkInstruction->__set_MotionType( "Locomotion/Walk" );
    WalkInstruction->__set_Name( "Walking to target location" );

    this->WalkTargetNameStr = string( TCHAR_TO_UTF8( *this->WalkTargetName ) );

    // update the local registries of the Avatars and Objects
    this->GetAvatarsAndObjects();

    // find the ID of the walk target
    vector<UMMISceneObject*>::iterator it = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->WalkTargetNameStr;
        } );

    if( it != this->SceneObjects.end() )
    {
        vector<string> propertiesVector = {"TargetID", ( *it )->MSceneObject.ID,
                                           "UseTargetOrientation", "false"};
        map<string, string> properties = PropertiesCreator::Create( propertiesVector );
        WalkInstruction->__set_Properties( properties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID] = vector<MInstruction>{*WalkInstruction};
        UE_LOG( LogMOSIM, Display, TEXT( "AssignWalkandIdleInstruction: Assigned Walk Instruction "
                                         "to Target Object with the name %s!" ),
                *this->WalkTargetName );
    }
    else
    {
        UE_LOG(
            LogMOSIM, Warning,
            TEXT( "AssignWalkandIdleInstruction: No Target Object with the name %s was found!" ),
            *this->WalkTargetName );
    }

    // Idle instruction
    MInstruction* IdleInstruction = new MInstruction();
    IdleInstruction->__set_ID( MMUAccess::GetNewGuid() );
    IdleInstruction->__set_Name( "Idle" );
    IdleInstruction->__set_MotionType( "Pose/Idle" );
    // set the idle properties
    IdleInstruction->__set_StartCondition( string(
        WalkInstruction->ID + ":" + this->MMIConstants->MSimulationEvent_End + " + 0.01" ) );
    // save the instruction for the current avatar
    this->InstructionMap[this->currentAvatarID].push_back( *IdleInstruction );

    if( !this->gameHasStarted )
    {
        FString msg = "Assign instruction failed. Please start simulation and try again.";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *msg );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, msg );
        return;
    }

    if( !this->issetSimContr )
    {
        for( TActorIterator<ASimulationController> SimContrItr( this->scene ); SimContrItr;
             ++SimContrItr )
        {
            this->SimContr = *SimContrItr;
            this->issetSimContr = true;
            // Execute the instructions
            this->SimContr->LoadAndExecuteInstructions();
            break;
        }
    }
    else
    {
        // Execute the instructions
        this->SimContr->LoadAndExecuteInstructions();
    }
}

void AAvatarBehavior::AssignReachInstruction()
{
    if( this->currentAvatarID.empty() )
    {
        UE_LOG(
            LogMOSIM, Warning,
            TEXT( "No Avatar is assigned. Assign Avatar first before assigning instructions." ) );
        return;
    }

    // Walk instruction to walk to a specific target using an MGeometryConstraint
    MInstruction* WalkInstruction = new MInstruction();
    WalkInstruction->__set_ID( MMUAccess::GetNewGuid() );
    WalkInstruction->__set_MotionType( "Locomotion/Walk" );
    WalkInstruction->__set_Name( "Walking to target location" );

    this->WalkTargetNameStr = string( TCHAR_TO_UTF8( *this->WalkTargetName ) );

    // Reach Instruction
    MInstruction* ReachInstruction = new MInstruction();
    ReachInstruction->__set_ID( MMUAccess::GetNewGuid() );
    ReachInstruction->__set_MotionType( "Pose/Reach" );
    ReachInstruction->__set_Name( "Reaching for target object." );
    ReachInstruction->__set_StartCondition(
        string( WalkInstruction->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );

    this->ReachTargetNameStr = string( TCHAR_TO_UTF8( *this->ReachTargetName ) );

    // update the local registries of the Avatars and Objects
    this->GetAvatarsAndObjects();

    // find the ID of the walk target
    vector<UMMISceneObject*>::iterator itWalk = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->WalkTargetNameStr;
        } );

    // find the ID of the reach target
    vector<UMMISceneObject*>::iterator itReach = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->ReachTargetNameStr;
        } );

    // create the porperties
    if( itWalk != this->SceneObjects.end() )
    {
        // set the walk properties
        vector<string> walkPropertiesVector = {"TargetID", ( *itWalk )->MSceneObject.ID,
                                               "UseTargetOrientation", "false"};
        map<string, string> walkProperties = PropertiesCreator::Create( walkPropertiesVector );
        WalkInstruction->__set_Properties( walkProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID] = vector<MInstruction>{*WalkInstruction};

        FString message = FString::Printf( TEXT( "AssignReachInstruction: Assigned Walk "
                                                 "Instruction to Target Object with the name %s!" ),
                                           *this->WalkTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, message );
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignReachInstruction: No Target Object with the name %s was found!" ),
            *this->ReachTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }

    // create the properties
    if( itReach != this->SceneObjects.end() )
    {
        // set the reach properties
        vector<string> reachPropertiesVector = {"TargetID", ( *itReach )->MSceneObject.ID, "Hand",
                                                "Right"};
        map<string, string> reachProperties = PropertiesCreator::Create( reachPropertiesVector );
        ReachInstruction->__set_Properties( reachProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID].push_back( *ReachInstruction );

        if( !this->gameHasStarted )
        {
            FString msg = "Assign instruction failed. Please start simulation and try again.";
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *msg );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Green, msg );
            return;
        }

        if( !this->issetSimContr )
        {
            for( TActorIterator<ASimulationController> SimContrItr( this->scene ); SimContrItr;
                 ++SimContrItr )
            {
                this->SimContr = *SimContrItr;
                this->issetSimContr = true;
                // Execute the instructions
                this->SimContr->LoadAndExecuteInstructions();
                break;
            }
        }
        else
        {
            // Execute the instructions
            this->SimContr->LoadAndExecuteInstructions();
        }
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignReachInstruction: No Target Object with the name %s was found!" ),
            *this->ReachTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }
}

void AAvatarBehavior::AssignCarryToTargetInstruction()
{
    if( this->currentAvatarID.empty() )
    {
        UE_LOG(
            LogMOSIM, Warning,
            TEXT( "No Avatar is assigned. Assign Avatar first before assigning instructions." ) );
        return;
    }

    ////////////////////////////////

    // walk to target

    // reach for target

    // grasp

    // carry

    // walk to other place

    // reach for traget location to place the object

    // release

    // cancel carry if necessary (by action)

    /////////////////////////

    // get all the walk/carry target strings
    this->WalkTargetNameStr = string( TCHAR_TO_UTF8( *this->WalkTargetName ) );
    this->ReachTargetNameStr = string( TCHAR_TO_UTF8( *this->ReachTargetName ) );
    this->WalkTargetCarryToTargetNameStr =
        string( TCHAR_TO_UTF8( *this->WalkTargetCarryToTargetName ) );
    this->CarryToTargetNameStr = string( TCHAR_TO_UTF8( *this->CarryToTargetName ) );

    // First Walk instruction to walk to a specific target using an MGeometryConstraint
    MInstruction* WalkInstruction1 = new MInstruction();
    WalkInstruction1->__set_ID( MMUAccess::GetNewGuid() );
    WalkInstruction1->__set_MotionType( "Locomotion/Walk" );
    WalkInstruction1->__set_Name( "Walking to target location" );

    // Reach Instruction
    MInstruction* ReachInstruction = new MInstruction();
    ReachInstruction->__set_ID( MMUAccess::GetNewGuid() );
    ReachInstruction->__set_MotionType( "Pose/Reach" );
    ReachInstruction->__set_Name( "Reaching for target object." );
    ReachInstruction->__set_StartCondition(
        string( WalkInstruction1->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );

    // grasp instruction
    MInstruction* GraspInstruction = new MInstruction();
    GraspInstruction->__set_ID( MMUAccess::GetNewGuid() );
    GraspInstruction->__set_MotionType( "Pose/Grasp" );
    GraspInstruction->__set_Name( "Grasping for target object." );
    GraspInstruction->__set_StartCondition(
        string( ReachInstruction->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );

    // First Walk instruction to walk to a specific target using an MGeometryConstraint
    MInstruction* WalkInstruction2 = new MInstruction();
    WalkInstruction2->__set_ID( MMUAccess::GetNewGuid() );
    WalkInstruction2->__set_MotionType( "Locomotion/Walk" );
    WalkInstruction2->__set_Name( "Walking to target location" );
    // WalkInstruction2->__set_StartCondition(string(GraspInstruction->ID + ":" +
    // this->MMIConstants->MSimulationEvent_End + " + 0.01"));
    WalkInstruction2->__set_StartCondition(
        string( ReachInstruction->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );

    // move instruction --> immer wenn object bewegt wird
    MInstruction* MoveInstruction = new MInstruction();
    MoveInstruction->__set_ID( MMUAccess::GetNewGuid() );
    MoveInstruction->__set_MotionType( "Object/Move" );
    MoveInstruction->__set_Name( "Move Object" );
    MoveInstruction->__set_StartCondition(
        string( ReachInstruction->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );
    // MoveInstruction->__set_StartCondition(string(WalkInstruction2->ID + ":" +
    // this->MMIConstants->MSimulationEvent_End));

    // Reach Instruction
    MInstruction* ReachInstruction2 = new MInstruction();
    ReachInstruction2->__set_ID( MMUAccess::GetNewGuid() );
    ReachInstruction2->__set_MotionType( "Pose/Reach" );
    ReachInstruction2->__set_Name( "Reaching for target object." );
    ReachInstruction2->__set_StartCondition(
        string( WalkInstruction2->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );
    // ReachInstruction2->__set_EndCondition(string(WalkInstruction2->ID + ":" +
    // this->MMIConstants->MSimulationEvent_End));

    // Carry Instruction
    MInstruction* CarryInstruction = new MInstruction();
    CarryInstruction->__set_ID( MMUAccess::GetNewGuid() );
    CarryInstruction->__set_MotionType( "Object/Carry" );
    CarryInstruction->__set_Name( "Carrying target object." );
    CarryInstruction->__set_StartCondition(
        string( ReachInstruction->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );
    CarryInstruction->__set_EndCondition(
        string( ReachInstruction2->ID + ":" + this->MMIConstants->MSimulationEvent_End + " + 2" ) );

    // release instruction
    MInstruction* ReleaseInstruction = new MInstruction();
    ReleaseInstruction->__set_ID( MMUAccess::GetNewGuid() );
    ReleaseInstruction->__set_MotionType( "Object/Release" );
    ReleaseInstruction->__set_Name( "Release Object" );
    ReleaseInstruction->__set_StartCondition(
        string( ReachInstruction2->ID + ":" + this->MMIConstants->MSimulationEvent_End ) );

    //// Idle instruction
    // MInstruction* IdleInstruction = new MInstruction();
    // IdleInstruction->__set_ID(MMUAccess::GetNewGuid());
    // IdleInstruction->__set_Name("Idle");
    // IdleInstruction->__set_MotionType("Pose/Idle");
    // IdleInstruction->__set_StartCondition(string(WalkInstruction2->ID + ":" +
    // this->MMIConstants->MSimulationEvent_End + " + 0.01"));
    //// save the instruction for the current avatar
    ////this->InstructionMap[this->currentAvatarID].push_back(*IdleInstruction);

    // update the local registries of the Avatars and Objects
    this->GetAvatarsAndObjects();

    // find the ID of the walk target
    vector<UMMISceneObject*>::iterator itWalk = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->WalkTargetNameStr;
        } );

    // find the ID of the reach target
    vector<UMMISceneObject*>::iterator itReach = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->ReachTargetNameStr;
        } );

    // find the ID of the walk target
    vector<UMMISceneObject*>::iterator itCarryWalk = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->WalkTargetCarryToTargetNameStr;
        } );

    // find the ID of the walk target
    vector<UMMISceneObject*>::iterator itCarry = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->CarryToTargetNameStr;
        } );

    // create the porperties
    if( itWalk != this->SceneObjects.end() )
    {
        // set the walk properties
        vector<string> walkPropertiesVector = {"TargetID", ( *itWalk )->MSceneObject.ID,
                                               "UseTargetOrientation", "false"};
        map<string, string> walkProperties = PropertiesCreator::Create( walkPropertiesVector );
        WalkInstruction1->__set_Properties( walkProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID] = vector<MInstruction>{*WalkInstruction1};

        FString message = FString::Printf( TEXT( "AssignCarryInstruction: Assigned Walk "
                                                 "Instruction to Target Object with the name %s!" ),
                                           *this->WalkTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, message );
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignCarryInstruction: No Target Object with the name %s was found!" ),
            *this->ReachTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }

    // create the properties
    if( itReach != this->SceneObjects.end() )
    {
        // set the reach properties
        vector<string> reachPropertiesVector = {"TargetID", ( *itReach )->MSceneObject.ID, "Hand",
                                                "Right"};
        map<string, string> reachProperties = PropertiesCreator::Create( reachPropertiesVector );
        ReachInstruction->__set_Properties( reachProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID].push_back( *ReachInstruction );

        // set the reach properties
        vector<string> carryPropertiesVector = {"TargetID", ( *itReach )->MSceneObject.ID, "Hand",
                                                "Right"};
        map<string, string> carryProperties = PropertiesCreator::Create( carryPropertiesVector );
        CarryInstruction->__set_Properties( carryProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID].push_back( *CarryInstruction );

        //// set the grasp properties
        // vector<string> graspPropertiesVector = { "TargetID", (*itReach)->MSceneObject.ID, "Hand",
        // "Right" };
        // map<string, string> graspProperties = PropertiesCreator::Create(graspPropertiesVector);
        // GraspInstruction->__set_Properties(graspProperties);
        //// save the instruction for the current avatar
        ////this->InstructionMap[this->currentAvatarID].push_back(*GraspInstruction);
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignCarryInstruction: No Target Object with the name %s was found!" ),
            *this->ReachTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }

    // create the properties and execute the instructions
    if( itCarryWalk != this->SceneObjects.end() )
    {
        // set the walk properties
        vector<string> walkPropertiesVector = {"TargetID", ( *itCarryWalk )->MSceneObject.ID,
                                               "UseTargetOrientation", "false"};
        map<string, string> walkProperties = PropertiesCreator::Create( walkPropertiesVector );
        WalkInstruction2->__set_Properties( walkProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID].push_back( *WalkInstruction2 );

        //// set the release properties
        // vector<string> ReleasePropertiesVector = { "Hand", "Right", "OnStart",
        // string(CarryInstruction->ID + ":" + "EndInstruction") };
        // map<string, string> ReleaseProperties =
        // PropertiesCreator::Create(ReleasePropertiesVector);
        // ReleaseInstruction->__set_Properties(ReleaseProperties);
        //// save the instruction for the current avatar
        // this->InstructionMap[this->currentAvatarID].push_back(*ReleaseInstruction);
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignCarryInstruction: No Target Object with the name %s was found!" ),
            *this->WalkTargetCarryToTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }

    if( itCarry != this->SceneObjects.end() )
    {
        // set the reach properties
        vector<string> reach_2PropertiesVector = {"TargetID", ( *itCarry )->MSceneObject.ID, "Hand",
                                                  "Right"};
        map<string, string> reach_2Properties =
            PropertiesCreator::Create( reach_2PropertiesVector );
        ReachInstruction2->__set_Properties( reach_2Properties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID].push_back( *ReachInstruction2 );

        vector<string> movePropertiesVector = {"TargetID",  ( *itCarry )->MSceneObject.ID,
                                               "SubjectID", ( *itReach )->MSceneObject.ID,
                                               "Hand",      "Right"};
        map<string, string> moveProperties = PropertiesCreator::Create( reach_2PropertiesVector );
        MoveInstruction->__set_Properties( moveProperties );
        // save the instruction for the current avatar
        // this->InstructionMap[this->currentAvatarID].push_back(*MoveInstruction);

        // set the release properties
        // vector<string> releasePropertiesVector = { "Hand", "Right", "OnStart",
        // ReachInstruction2->ID+":EndInstruction", "OnStart", CarryInstruction->ID +
        // ":EndInstruction" };
        vector<string> releasePropertiesVector = {"Hand", "Right", "OnStart",
                                                  MoveInstruction->ID + ":EndInstruction"};
        map<string, string> releaseProperties =
            PropertiesCreator::Create( releasePropertiesVector );
        ReleaseInstruction->__set_Properties( releaseProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID].push_back( *ReleaseInstruction );

        if( !this->gameHasStarted )
        {
            FString msg = "Assign instruction failed. Please start simulation and try again.";
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *msg );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Green, msg );
            return;
        }

        if( !this->issetSimContr )
        {
            for( TActorIterator<ASimulationController> SimContrItr( this->scene ); SimContrItr;
                 ++SimContrItr )
            {
                this->SimContr = *SimContrItr;
                this->issetSimContr = true;
                // Execute the instructions
                this->SimContr->LoadAndExecuteInstructions();
                break;
            }
        }
        else
        {
            // Execute the instructions
            this->SimContr->LoadAndExecuteInstructions();
        }
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignCarryInstruction: No Target Object with the name %s was found!" ),
            *this->CarryToTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }
}

void AAvatarBehavior::AssignCarryInstruction()
{
    if( this->currentAvatarID.empty() )
    {
        UE_LOG(
            LogMOSIM, Warning,
            TEXT( "No Avatar is assigned. Assign Avatar first before assigning instructions." ) );
        return;
    }

    // Carry Instruction
    MInstruction* CarryInstruction = new MInstruction();
    CarryInstruction->__set_ID( MMUAccess::GetNewGuid() );
    CarryInstruction->__set_MotionType( "Object/Carry" );
    CarryInstruction->__set_Name( "Carrying target object." );

    // update the local registries of the Avatars and Objects
    this->GetAvatarsAndObjects();

    // find the ID of the reach target
    vector<UMMISceneObject*>::iterator itReach = find_if(
        this->SceneObjects.begin(), this->SceneObjects.end(), [this]( UMMISceneObject* obj ) {
            return obj->MSceneObject.Name == this->ReachTargetNameStr;
        } );

    // create the properties
    if( itReach != this->SceneObjects.end() )
    {
        // set the reach properties
        vector<string> carryPropertiesVector = {"TargetID", ( *itReach )->MSceneObject.ID, "Hand",
                                                "Right"};
        map<string, string> carryProperties = PropertiesCreator::Create( carryPropertiesVector );
        CarryInstruction->__set_Properties( carryProperties );
        // save the instruction for the current avatar
        this->InstructionMap[this->currentAvatarID].push_back( *CarryInstruction );

        if( !this->gameHasStarted )
        {
            FString msg = "Assign instruction failed. Please start simulation and try again.";
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *msg );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Green, msg );
            return;
        }

        if( !this->issetSimContr )
        {
            for( TActorIterator<ASimulationController> SimContrItr( this->scene ); SimContrItr;
                 ++SimContrItr )
            {
                this->SimContr = *SimContrItr;
                this->issetSimContr = true;
                // Execute the instructions
                this->SimContr->LoadAndExecuteInstructions();
                break;
            }
        }
        else
        {
            // Execute the instructions
            this->SimContr->LoadAndExecuteInstructions();
        }
    }
    else
    {
        FString message = FString::Printf(
            TEXT( "AssignCarryInstruction: No Target Object with the name %s was found!" ),
            *this->ReachTargetName );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// stop instruction

void AAvatarBehavior::StopInstruction()
{
    if( this->issetSimContr )
    {
        // stop the current instructions
        this->SimContr->StopCurrentInstruction();
    }
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// methods for loading the scene objects and avatars

void AAvatarBehavior::GetAvatarsAndObjects()
{
    // find the simulation controller
    this->scene = this->GetWorld();
    // count the loop executions
    int loopCount = 0;
    for( TActorIterator<ASimulationController> SimContrItr( this->scene ); SimContrItr;
         ++SimContrItr )
    {
        UE_LOG( LogMOSIM, Display, TEXT( "Simulation Controller found!" ) );
        // Register Avatar at the Unreal Scene Access
        SimContrItr->RegisterAllAvatarsAndObjects();
        // get all Avatars from the Unreal Scene Access
        this->Avatars = SimContrItr->UESceneAccess->GetMMIAvatarsVector();
        // get all SceneObjects from the Unreal Scene Access
        this->SceneObjects = SimContrItr->UESceneAccess->GetMMISceneObjectsVector();

        // check, whether the vectors contain any entries
        if( this->Avatars.empty() || this->SceneObjects.empty() )
        {
            FString message =
                "Register contains no Scene Objects and Avatars.\nAvatars and Scene "
                "Objects have to be updated first.";
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *message );
            // Output error message in the engine
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, message );
        }

        // there should be only one simulation controller
        loopCount++;
        break;
    }

    if( loopCount == 0 )  // it will be always 1 or 0 depending whether there is or there isn't
                          // simulation controller
    {
        FString message = "No Simulation Controller found!";
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *message );
        // Output error message in the engine
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }

    if( !this->SceneObjects.empty() )
        for( int i = 0; i < this->SceneObjects.size(); i++ )
        {
            UE_LOG( LogMOSIM, Display, TEXT( "%s" ),
                    *FString( this->SceneObjects[i]->MSceneObject.Name.c_str() ) );
        }
}

// method for loading the avatar
void AAvatarBehavior::AssignAvatarString( string AvatarNameStr )
{
    // update the local registries of the Avatars and Objects
    this->GetAvatarsAndObjects();

    // find the ID of the assigned avatar
    vector<AMMIAvatar*>::iterator it =
        find_if( this->Avatars.begin(), this->Avatars.end(),
                 [&AvatarNameStr]( AMMIAvatar* obj ) { return obj->baseName == AvatarNameStr; } );

    if( it != this->Avatars.end() )
    {
        this->currentAvatarID = ( *it )->MAvatar.ID;

        FString message = FString::Printf( TEXT( "Avatar with the name %s assigned!" ),
                                           UTF8_TO_TCHAR( AvatarNameStr.c_str() ) );
        UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Green, message );
    }
    else
    {
        FString message =
            FString::Printf( TEXT( "AssignAvatar: No Avatar with the name %s was found!" ),
                             UTF8_TO_TCHAR( AvatarNameStr.c_str() ) );
        UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
    }
}

void AAvatarBehavior::AssignAvatar()
{
    string AvatarNameStr = string( TCHAR_TO_UTF8( *this->AvatarName ) );
    this->AssignAvatarString( AvatarNameStr );
}
