// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Central instance for exectuing the simulations. Collects all MAvatars and MScene Objects
// and passes them to the UnrealSceneAccess. Initializes all MAvatars and MSceneObjects during
// BeginPlay() and executes the Simulation in MOSIM during Tick()

#include "SimulationController.h"
#include "MOSIM.h"
#include "UnrealSceneAccess.h"
#include "MMISettings.h"
#include "MMUAccess.h"
#include "MMIAvatar.h"
#include "MMISceneObject.h"
#include "AvatarBehavior.h"
#include "SkeletonAccess.h"
#include "AjanAgent.h"
#include "EngineUtils.h"
#include "Engine.h"
#include "Utils/Logger.h"

#include <thread>
#include <Concurrent_vector.h>

#include "Kismet/GameplayStatics.h"

// Sets default values
ASimulationController::ASimulationController()
    : AutoCreateSessionID( true ),
      RealTime( true ),
      FixedStepTime( 0.01f ),
      PhysicsUpdatesPerFrame( true ),
      AutoStart( true ),
      UESceneAccess( nullptr ),
      Settings( nullptr ),
      Behavior( nullptr ),
      initialized( false ),
      frameNumber( 0 ),
      currentUpdateFPS( 30.f ),
      currentInstructionID( string( "" ) ),
      walkTargetObjectID( string( "" ) ),
      currentAvatar( nullptr )
{
    // Set this actor to call Tick() every frame.  You can turn this off to improve performance if
    // you don't need it.
    PrimaryActorTick.bCanEverTick = true;

    // Instance of MMISettings
    Settings = new MMISettings{};

    // Instance of the UnrealSceneAccess
    UESceneAccess = new UnrealSceneAccess();
}

ASimulationController::~ASimulationController()
{
    // set the SceneAccess pointer in all registered avatars and mmisceneobjects to nullptr
    for( AMMIAvatar* avatarInUEAccess : this->UESceneAccess->GetMMIAvatarsVector() )
    {
        avatarInUEAccess->SceneAccess = nullptr;
        avatarInUEAccess->SimController = nullptr;
    }
    // set the Simulation Controller pointer in all registered avatars to nullptr
    for( UMMISceneObject* objectInUEAccess : this->UESceneAccess->GetMMISceneObjectsVector() )
    {
        objectInUEAccess->SceneAccess = nullptr;
    }

    if( this->Settings )
        delete this->Settings;
    if( this->UESceneAccess )
        delete this->UESceneAccess;
}

// Called when the game starts or when spawned
void ASimulationController::BeginPlay()
{
    Super::BeginPlay();

    if( this->AutoStart == true )
        try
        {
            this->Setup();
        }
        catch( exception e )
        {
            UE_LOG( LogMOSIM, Error, TEXT( "Setup failed. Perhaps the MOSIM framework is not "
                                           "started. Simulation will be finished." ) );
            FString message(
                "Setup failed. Perhaps the MOSIM framework is not started. Simulation will be "
                "finished." );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, message );
            APlayerController* PlayerController = GetWorld()->GetFirstPlayerController();
            PlayerController->ConsoleCommand( "quit" );

            this->initialized = false;
            return;
        }
}

void ASimulationController::Setup()
{
    // get the world
    scene = this->GetWorld();

    // collect all avatars and objects
    this->RegisterAllAvatarsAndObjects();

    // start the scene access server
    this->UESceneAccess->InitializeServers();

    // perform the final initialzation here,
    // because the TargetObject Name is required,
    // Object type of the Target Object Name is set to Walk::Target
    // --> Collider is not transfered to the MOSIM framework
    this->FinalInitializationMMISceneObjects();

    // assign the instructions for the avatars
    this->Avatars = this->UESceneAccess->GetMMIAvatarsVector();

    // register, whether all avatars are ajan agents
    concurrent_vector<AMMIAvatar*> AreAjanAgents = concurrent_vector<AMMIAvatar*>();
    concurrent_vector<AMMIAvatar*> AreNotAjanAgents = concurrent_vector<AMMIAvatar*>();

    // generate mutex for controlling the write access to the vectors above
    // generate mutex for controlling the skeleton access
    mutex mtx;
    mutex mtxReta;
    mutex mtxSetup;

    int ajanClPortCounter = 8083;

    bool avatarLoadSuccess = false;
    for( AMMIAvatar* avatar : this->Avatars )
    {
        UE_LOG( LogMOSIM, Display, TEXT( "AjanCLPort: %i" ), ajanClPortCounter );

        // Create a new session id
        string currentSessionId = MMUAccess::GetNewGuid();
        this->SessionId.push_back( currentSessionId );

        // setup the avatar
        mtxSetup.lock();
        avatarLoadSuccess = avatar->Setup( Settings->MIPRegisterAddress, currentSessionId,
                                           this->UESceneAccess, this, mtx, mtxReta );
        mtxSetup.unlock();

        if( !avatarLoadSuccess )
        {
            FString messageFStr = "Avatar setup failed.";
            UE_LOG( LogMOSIM, Error, TEXT( "%s" ), *messageFStr );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, messageFStr );
            this->initialized = false;
            return;
        }

        // check, whether Ajan is running
        TArray<UAjanAgent*> AjanComponents;
        avatar->GetComponents<UAjanAgent>( AjanComponents );
        if( AjanComponents.Num() > 0 && AjanComponents[0]->isAjanRunning )
        {
            AjanComponents[0]->SetClPort( ajanClPortCounter );
            avatar->running = true;
            AreAjanAgents.push_back( avatar );
        }
        else
        {
            AreNotAjanAgents.push_back( avatar );
        }

        // each Ajan agent requires 3 subsequent ports
        ajanClPortCounter += 3;
    }

    // look for instructions in AAvatarBehavior Class in case Avatars are found that are not
    // controlled by Ajan
    if( !AreNotAjanAgents.empty() )
    {
        // get the avatarBehavior
        for( TActorIterator<AAvatarBehavior> BehaviorItr( scene ); BehaviorItr; ++BehaviorItr )
        {
            UE_LOG( LogMOSIM, Display,
                    TEXT( "ASimulationController(): AAvatarBehavior class found." ) );
            this->Behavior = *BehaviorItr;
            this->Behavior->gameHasStarted = true;
        }

        // continue, in case no AvatarBehavior class is registerd
        if( !this->Behavior )
        {
            UE_LOG( LogMOSIM, Warning,
                    TEXT( "No AAvatarBehavior class found. All Avatars not marked as AjanAgents "
                          "will not execute any instructions." ) );
            FString message =
                FString::Printf( TEXT( "No AAvatarBehavior class found. All Avatars not marked as "
                                       "AjanAgents will not execute any instructions." ) );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, message );
            return;
        }

        for( AMMIAvatar* avatar : AreNotAjanAgents )  // could be done in threads
        {
            this->currentAvatar = avatar;

            if( this->Behavior->InstructionMap.find( avatar->MAvatar.ID ) ==
                    this->Behavior->InstructionMap.end() ||
                this->Behavior->InstructionMap.empty() )
            {
                FString message = FString::Printf(
                    TEXT( "ASimulationController(): No instructions found. Assign instructions "
                          "first to the current Avatar (%s) before executing the simulation." ),
                    UTF8_TO_TCHAR( avatar->baseName.c_str() ) );
                UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *message );
                GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                                  FColor::Green, message );

                // assign avatar in the AvatarBehavior Class
                this->Behavior->AssignAvatarString( avatar->baseName );
            }
            else
            {
                this->LoadAndExecuteInstructions();
            }
        }
    }

    this->initialized = true;
}

void ASimulationController::LoadAndExecuteInstructions()
{
    vector<MInstruction> InstructionVector =
        this->Behavior->InstructionMap[this->currentAvatar->MAvatar.ID];
    this->ExecuteInstructions( InstructionVector );
}

void ASimulationController::ExecuteInstructions( const vector<MInstruction>& _InstructionVector )
{
    MBoolResponse retBool;

    MSimulationState simstate = MSimulationState();
    simstate.Current = this->currentAvatar->ReadCurrentPosture();
    simstate.Initial = this->currentAvatar->ReadCurrentPosture();

    for( MInstruction instruction : _InstructionVector )
    {
        for( int i = 0; i < this->currentAvatar->MMUAccessPtr->MotionModelUnits.size(); i++ )
        {
            try
            {
                this->currentAvatar->MMUAccessPtr->MotionModelUnits[i]->AssignInstruction(
                    retBool, instruction, simstate );
                this->currentAvatar->running = true;
                this->currentInstructionID = instruction.ID;
            }
            catch( const exception& e )
            {
                string message = string( e.what() );
                Logger::printLog( L_ERROR, message );
                FString messageFStr = FString::Printf(
                    TEXT( "%s Please restart MOSIM framework and the simulation." ),
                    UTF8_TO_TCHAR( message.c_str() ) );
                UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *messageFStr );
                GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                                  FColor::Red, messageFStr );
            }
        }
    }
}

void ASimulationController::StopCurrentInstruction()
{
    MBoolResponse retBool;
    for( int i = 0; i < this->currentAvatar->MMUAccessPtr->MotionModelUnits.size(); i++ )
    {
        this->currentAvatar->MMUAccessPtr->MotionModelUnits[i]->Abort( retBool,
                                                                       this->currentInstructionID );
        if( !retBool.Successful )
        {
            string message = "Error while aborting instruction!";
            Logger::printLog( L_ERROR, message );
            FString messageFStr =
                FString::Printf( TEXT( "%s Please restart MOSIM framework and the simulation." ),
                                 UTF8_TO_TCHAR( message.c_str() ) );
            UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *messageFStr );
            GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                              FColor::Red, messageFStr );
        }
    }
}

void ASimulationController::FinalInitializationMMISceneObjects()
{
    for( UMMISceneObject* mmiSceneObject : this->UESceneAccess->GetMMISceneObjectsVector() )
    {
        if( this->Behavior )
        {
            // look for the traget Object and check, whether it is a walk target
            string sceneObjectName = mmiSceneObject->MSceneObject.Name;
            int compareResult = sceneObjectName.compare( this->Behavior->WalkTargetNameStr );
            if( compareResult == 0 )
            {
                // save the walk target
                this->walkTargetObjectID = mmiSceneObject->MSceneObject.ID;
                // target object should not have a collider
                if( mmiSceneObject->SceneObjectType != SceneObjectType::WalkTarget )
                {
                    mmiSceneObject->SceneObjectType = SceneObjectType::WalkTarget;
                    // Output error message
                    FString message = FString::Printf(
                        TEXT( "Target Object %s has to be of type Walk Target.\nModified the "
                              "Object Type to Walk Target." ),
                        UTF8_TO_TCHAR( mmiSceneObject->Name.c_str() ) );
                    UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *message );
                    GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ),
                                                      5.0f, FColor::Green, message );
                }
            }

            // check, whether it is a reach target
            compareResult = sceneObjectName.compare( this->Behavior->ReachTargetNameStr );
            if( compareResult == 0 )
            {
                // save the walk target
                this->walkTargetObjectID = mmiSceneObject->MSceneObject.ID;
                // target object should not have a collider
                if( mmiSceneObject->SceneObjectType != SceneObjectType::Tool )
                {
                    mmiSceneObject->SceneObjectType = SceneObjectType::Tool;
                    // Output error message
                    FString message =
                        FString::Printf( TEXT( "Target Object %s has to be of type Tool.\nModified "
                                               "the Object Type to Tool." ),
                                         UTF8_TO_TCHAR( mmiSceneObject->Name.c_str() ) );
                    UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *message );
                    GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ),
                                                      5.0f, FColor::Green, message );
                }
            }

            // check, whether it is a carry target
            compareResult = sceneObjectName.compare( this->Behavior->CarryToTargetNameStr );
            if( compareResult == 0 )
            {
                // save the walk target
                this->walkTargetObjectID = mmiSceneObject->MSceneObject.ID;
                // target object should not have a collider
                if( mmiSceneObject->SceneObjectType != SceneObjectType::WalkTarget )
                {
                    mmiSceneObject->SceneObjectType = SceneObjectType::WalkTarget;
                    // Output error message
                    FString message = FString::Printf(
                        TEXT( "Target Object %s has to be of type Walk Target.\nModified the "
                              "Object Type to Walk Target." ),
                        UTF8_TO_TCHAR( mmiSceneObject->Name.c_str() ) );
                    UE_LOG( LogMOSIM, Warning, TEXT( "%s" ), *message );
                    GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ),
                                                      5.0f, FColor::Green, message );
                }
            }
        }

        // start the setup of the MMISceneObject
        mmiSceneObject->Setup( this->UESceneAccess );
    }
}

// Called every frame
void ASimulationController::Tick( float DeltaTime )
{
    Super::Tick( DeltaTime );

    // estimate updates in FPS
    this->currentUpdateFPS = 1.0f / DeltaTime;

    // Get the desired delta time for the current frame
    this->frameDeltaTime = this->RealTime ? DeltaTime : this->FixedStepTime;

    // skip rest, if not initialized
    if( !this->initialized )
    {
        FString message(
            "Setup failed. Perhaps the MOSIM framework is not started. Simulation will be "
            "terminated." );
        UE_LOG( LogMOSIM, Error, TEXT( "%s" ), *message );
        GEngine->AddOnScreenDebugMessage( static_cast<uint64>( this->GetUniqueID() ), 5.0f,
                                          FColor::Red, message );
        APlayerController* PlayerController = GetWorld()->GetFirstPlayerController();
        PlayerController->ConsoleCommand( "quit" );
        return;
    }

    this->DoStep( this->frameDeltaTime );

    // count the frames
    this->frameNumber++;

    // ...
}

void ASimulationController::DoStep( float time )
{
    // Scene synchronization of each MMU Access
    this->PushScene();

    // Do the Co Simulation for each Avatar
    // Create a map which contains the avatar states for each MMU

    for( AMMIAvatar* avatar : this->Avatars )  // could be parallelized in multiple threads
    {
        if( avatar->running )
        {
            MSimulationState simstate = MSimulationState();
            simstate.Current = avatar->ReadCurrentPosture();
            simstate.Initial = avatar->ReadCurrentPosture();

            MSimulationResult simRes = MSimulationResult();

            for( int i = 0; i < avatar->MMUAccessPtr->MotionModelUnits.size(); i++ )
            {
                avatar->MMUAccessPtr->MotionModelUnits[i]->DoStep( simRes, time, simstate );
                break;
            }

            // pass channel data to the remote skeleton access
            avatar->skeletonAccessPtr->SetChannelData( simstate.Current );

            // Add the results
            this->resultsMap.insert( pair<AMMIAvatar*, MSimulationResult>{avatar, simRes} );

            for( string st : simRes.LogData )
            {
                FString fstr( st.c_str() );
                UE_LOG( LogMOSIM, Display, TEXT( "%s" ), *fstr );
            }

            // apply the results
            this->ApplySceneUpdate();

            // clear all results, as this is no archive
            this->resultsMap.clear();
        }
    }
}

void ASimulationController::PushScene()
{
    // Synchronizes the scene in before each update

    auto lambdaExp = [this]( const int& i ) {
        if( this->frameNumber == 0 )
        {
            this->Avatars[i]->MMUAccessPtr->PushScene( true );
        }
        else
        {
            this->Avatars[i]->MMUAccessPtr->PushScene( false );
        }
    };

    vector<thread*> ThreadVector = vector<thread*>();

    for( int i = 0; i < this->Avatars.size(); i++ )
    {
        ThreadVector.push_back( new thread( lambdaExp, i ) );
    }
    MMUAccess::StopThreads( ThreadVector );

    // Clear the events since the current events have been already synchronized
    this->UESceneAccess->Clear_SceneUpdate();
}

void ASimulationController::ApplySceneUpdate()
{
    vector<MSceneManipulation> sceneManipulations = vector<MSceneManipulation>();

    for( pair<AMMIAvatar*, MSimulationResult> coSimulationResult : this->resultsMap )
    {
        // Assign the posture of the avatar
        coSimulationResult.first->ApplyPostureValues( coSimulationResult.second.Posture );

        // Add the scene manipulations
        for( MSceneManipulation sceneManipuation : coSimulationResult.second.SceneManipulations )
            sceneManipulations.push_back( sceneManipuation );
    }

    // Apply the manipulations of the scene
    MBoolResponse boolResp;
    this->UESceneAccess->ApplyManipulations( boolResp, sceneManipulations );
    if( !boolResp.Successful )
    {
        string message = "Problems assigning scene manipulations!";
        UE_LOG( LogMOSIM, Error, TEXT( "%s" ), *FString( message.c_str() ) );
        Logger::printLog( L_ERROR, message );
    }

    // TODO: Apply the remote scene updates (not implemented so far)
    // this->ApplyRemoteSceneUpdates();
}

void ASimulationController::RegisterAllAvatarsAndObjects()
{
    // collect all avatars
    scene = this->GetWorld();
    for( TActorIterator<AMMIAvatar> ActorItr( scene ); ActorItr; ++ActorItr )
    {
        UE_LOG( LogMOSIM, Display, TEXT( "AMMIAvatar with name %s found." ),
                *FString( ActorItr->MAvatar.Name.c_str() ) );
        // Register Avatar at the Unreal Scene Access
        this->UESceneAccess->AddMMIAvatar( ActorItr->MAvatar.ID, *ActorItr );
    }

    // collect all MMISceneObjects
    for( TActorIterator<AActor> USceneObjItr( scene ); USceneObjItr; ++USceneObjItr )
    {
        TArray<USceneComponent*> ComponentsArray;
        USceneObjItr->GetComponents<USceneComponent>( ComponentsArray );
        for( USceneComponent* sceneComponent : ComponentsArray )
        {
            UMMISceneObject* mmiSceneObject = Cast<UMMISceneObject>( sceneComponent );
            if( mmiSceneObject )
            {
                // update the names of the Object
                mmiSceneObject->SetupBase();
                UE_LOG( LogMOSIM, Display, TEXT( "UMMISceneObject with name %s found." ),
                        *FString( mmiSceneObject->Name.c_str() ) );
                // Register the scene object at the Unreal Scene Access
                this->UESceneAccess->AddMMISceneObject( mmiSceneObject->MSceneObject.ID,
                                                        mmiSceneObject );
            }
        }
    }
}

// necessary, as the thrift servers are othwerise not terminated at the end of the simulation
void ASimulationController::EndPlay( EEndPlayReason::Type EndPlayReason )
{
    if( this->UESceneAccess )
        this->UESceneAccess->UESceneAccessServerCore->~UnrealSceneAccessServer();

    // set back the port counter to the original value
    AMMIAvatar::RemoteCoSimulationAccessPortIncremented = 9011;
}