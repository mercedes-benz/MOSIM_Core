// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// This class stores and manages the Unreal Scene as it is transferred to the MOSIM framework.
// Stores pointers to all MMMISceneObjects and MMIAvatars in MMISceneObjectsByID and MMIAvatarByID.
// All other Classes in the Plugin access these Maps for getting information about the scene.
// The info about the scene, that is pushed to the Adapters is stored in
// UnrealSceneAccess::SceneUpdate
// Updates with ApplyManipulations() the MSceneObjects and MMISceneObjects, if changes happened
// during the MOSIM step

#include "UnrealSceneAccess.h"
#include "MMUAccess.h"
#include "MMISceneObject.h"
#include "MMIAvatar.h"
#include "Utils/Logger.h"
#include "Extensions/MVector3Extensions.h"
#include "Extensions/MQuaternionExtensions.h"

#include "RemoteSceneAccessServer.h"
//#include "RemoteSceneManipulationServer.h"

// define static fields
static int currentSceneObjectID = 1;
static int currentAvatarID = 1;
// mutex for accessing scene related elements
static mutex sceneMutex;
static mutex sceneObjectIDMutex;
static mutex avatarIDMutex;

UnrealSceneAccess::UnrealSceneAccess()
    : FrameID( 0 ), HistoryBufferSize( 20 ), settings( new MMISettings() )
{
}

UnrealSceneAccess::~UnrealSceneAccess()
{
}

//// are initialized in the setup method of the SimulationController
void UnrealSceneAccess::InitializeServers()
{
    this->UESceneAccessServerCore = shared_ptr<SceneAccessServer::UnrealSceneAccessServer>(
        new SceneAccessServer::UnrealSceneAccessServer( this ) );
    this->UESceneAccessServerCore->InitializeServers();
}

string UnrealSceneAccess::CreateUUID()
{
    return MMUAccess::GetNewGuid();
}

// static method
string UnrealSceneAccess::CreateSceneObjectID()
{
    // return type
    string id;
    // lock the mutex
    sceneObjectIDMutex.lock();
    // create new ID by incrementing the current ID
    id = to_string( currentSceneObjectID++ );
    // release the mutex
    sceneObjectIDMutex.unlock();
    return id;
}

// static method
string UnrealSceneAccess::CreateAvatarID()
{
    // return type
    string id;
    // lock the mutex
    avatarIDMutex.lock();
    // create new ID by incrementing the current ID
    id = to_string( currentAvatarID++ );
    // release the mutex
    avatarIDMutex.unlock();
    return id;
}

void UnrealSceneAccess::ApplyUpdates( MBoolResponse& _return, const MSceneUpdate& sceneUpdates )
{
    // Increment the frame id
    this->FrameID++;

    // Stores the history
    SceneHistory.push( make_tuple( this->FrameID, sceneUpdates ) );

    // Only allow the max buffer size
    while( SceneHistory.size() > this->HistoryBufferSize )
        this->SceneHistory.pop();

    // Check if there are avatars to be added
    if( sceneUpdates.AddedAvatars.size() > 0 )
        this->AddAvatars( sceneUpdates.AddedAvatars );

    // Check if there are new scene objects which should be added
    if( sceneUpdates.AddedSceneObjects.size() > 0 )
        this->AddSceneObjects( sceneUpdates.AddedSceneObjects );

    // Check if there are changed avatars that need to be retransmitted
    if( sceneUpdates.ChangedAvatars.size() > 0 )
        this->UpdateAvatars( sceneUpdates.ChangedAvatars );

    // Check if there are changed sceneObjects that need to be retransmitted
    if( sceneUpdates.ChangedSceneObjects.size() > 0 )
        this->UpdateSceneObjects( sceneUpdates.ChangedSceneObjects );

    // Check if there are avatars that need to be removed
    if( sceneUpdates.RemovedAvatars.size() > 0 )
        this->RemoveAvatars( sceneUpdates.RemovedAvatars );

    // Check if there are scene objects that need to be removed
    if( sceneUpdates.RemovedSceneObjects.size() > 0 )
        this->RemoveSceneObjects( sceneUpdates.RemovedSceneObjects );

    _return.__set_Successful( true );
}

void UnrealSceneAccess::ApplyManipulations( MBoolResponse& _return,
                                            const vector<MSceneManipulation>& sceneManipulations )
{
    // Acquire the mutex
    sceneMutex.lock();

    // Handle each scene manipulation
    for( MSceneManipulation sceneManipulation : sceneManipulations )
    {
        // Incorporate the transform updates
        // Handle each transform manipulation
        for( MTransformManipulation transformUpdate : sceneManipulation.Transforms )
        {
            // find the corresponding objects in the Unreal scene, loop over entries in map
            try
            {
                // Do not update physics within the frame, since the transform is actively
                // manipulated
                MMISceneObjectsByID.at( transformUpdate.Target )->UpdatePhysicsCurrentFrame = false;

                // supervise, if updates happened
                bool updateTookPlace = false;

                // Update the position (if changed)
                if( transformUpdate.__isset.Position )
                {
                    MMISceneObjectsByID.at( transformUpdate.Target )
                        ->MSceneObject.Transform.Position = transformUpdate.Position;
                    updateTookPlace = true;
                }

                // Update the rotation (if changed)
                if( transformUpdate.__isset.Rotation )
                {
                    MMISceneObjectsByID.at( transformUpdate.Target )
                        ->MSceneObject.Transform.Rotation = transformUpdate.Rotation;
                    updateTookPlace = true;
                }

                // Update the parent (if changed)
                if( transformUpdate.__isset.Parent && !transformUpdate.Parent.empty() )
                {
                    MMISceneObjectsByID.at( transformUpdate.Target )
                        ->ChangeParent( transformUpdate.Parent );
                    updateTookPlace = true;
                }

                // Update the root component of the MMISceneObject owner
                if( updateTookPlace )
                    MMISceneObjectsByID.at( transformUpdate.Target )->UpdateRootComponent();
            }
            catch( exception e )
            {
                string loggerMsg( e.what() );
                runtime_error(
                    "Matching MMISceneObject for applying the scene manipulations was not found. "
                    "Message: " +
                    loggerMsg );
                Logger::printLog( L_ERROR,
                                  " Matching MMISceneObject for applying the scene manipulations "
                                  "was not found: " +
                                      loggerMsg );
            }
        }

        // Incorporate the phsysics interactions
        // Handle each physicsInteraction
        for( MPhysicsInteraction physicsInteraction : sceneManipulation.PhysicsInteractions )
        {
            // Find the matching object
            try
            {
                // look if the parent of the MMIsceneObject component has a parent with a static
                // mesh
                if( MMISceneObjectsByID.at( physicsInteraction.Target )->rootIsPrimitive == true )
                {
                    // supervise, if updates happened
                    bool updateTookPlace = false;

                    // Apply the manipulation
                    switch( physicsInteraction.Type )
                    {
                        case MPhysicsInteractionType::AddForce:
                            MMISceneObjectsByID.at( physicsInteraction.Target )
                                ->MSceneObject.PhysicsProperties.__set_NetForce(
                                    physicsInteraction.Values );
                            updateTookPlace = true;
                            break;

                        case MPhysicsInteractionType::AddTorque:
                            MMISceneObjectsByID.at( physicsInteraction.Target )
                                ->MSceneObject.PhysicsProperties.__set_NetTorque(
                                    physicsInteraction.Values );
                            updateTookPlace = true;
                            break;

                        case MPhysicsInteractionType::ChangeAngularVelocity:
                            MMISceneObjectsByID.at( physicsInteraction.Target )
                                ->MSceneObject.PhysicsProperties.__set_AngularVelocity(
                                    physicsInteraction.Values );
                            updateTookPlace = true;
                            break;

                        case MPhysicsInteractionType::ChangeCenterOfMass:
                            MMISceneObjectsByID.at( physicsInteraction.Target )
                                ->MSceneObject.PhysicsProperties.__set_CenterOfMass(
                                    physicsInteraction.Values );
                            updateTookPlace = true;
                            break;

                        case MPhysicsInteractionType::ChangeInertia:
                            MMISceneObjectsByID.at( physicsInteraction.Target )
                                ->MSceneObject.PhysicsProperties.__set_Inertia(
                                    physicsInteraction.Values );
                            updateTookPlace = true;
                            break;

                        case MPhysicsInteractionType::ChangeMass:
                            MMISceneObjectsByID.at( physicsInteraction.Target )
                                ->MSceneObject.PhysicsProperties.__set_Mass(
                                    physicsInteraction.Values[0] );
                            updateTookPlace = true;
                            break;

                        case MPhysicsInteractionType::ChangeVelocity:
                            MMISceneObjectsByID.at( physicsInteraction.Target )
                                ->MSceneObject.PhysicsProperties.__set_Velocity(
                                    physicsInteraction.Values );
                            updateTookPlace = true;
                            break;
                    }

                    if( updateTookPlace )
                        MMISceneObjectsByID.at( physicsInteraction.Target )
                            ->UpdatePhysics( physicsInteraction );
                }
            }
            catch( exception e )
            {
                string loggerMsg( e.what() );
                runtime_error(
                    "Matching MMISceneObject for applying the physics manipulations was not found. "
                    "Message: " +
                    loggerMsg );
                Logger::printLog( L_ERROR,
                                  " Matching MMISceneObject for applying the physics manipulations "
                                  "was not found: " +
                                      loggerMsg );
            }
        }

        // Incorporate the properties
        // Handle all manipulations of the properties
        for( MPropertyManipulation propertyUpdate : sceneManipulation.Properties )
        {
            // Apply the changes if a scene object can be found
            try
            {
                if( MMISceneObjectsByID.at( propertyUpdate.Target )
                        ->MSceneObject.Properties.find( propertyUpdate.Key ) !=
                    MMISceneObjectsByID.at( propertyUpdate.Target )->MSceneObject.Properties.end() )
                    MMISceneObjectsByID.at( propertyUpdate.Target )
                        ->MSceneObject.Properties[propertyUpdate.Key] = propertyUpdate.Value;
                else
                    MMISceneObjectsByID.at( propertyUpdate.Target )
                        ->MSceneObject.Properties.insert(
                            pair<string, string>{propertyUpdate.Key, propertyUpdate.Value} );
            }
            catch( exception e )
            {
                string loggerMsg( e.what() );
                runtime_error(
                    "Matching MMISceneObject for applying the properties was not found. Message: " +
                    loggerMsg );
                Logger::printLog(
                    L_ERROR,
                    " Matching MMISceneObject for applying the properties was not found: " +
                        loggerMsg );
            }
        }
    }

    // Release the mutex
    sceneMutex.unlock();

    _return.__set_Successful( true );
}

MBoolResponse UnrealSceneAccess::AddAvatars( vector<MAvatar> avatars )
{
    MBoolResponse result;
    result.__set_Successful( true );

    // Iterate over each avatar
    // TODO: check, if ID of MAvatar is the same as the ID used for the MMIAvatars in the map
    for( MAvatar& avatar : avatars )
    {
        if( this->MMIAvatarsByID.find( avatar.ID ) != this->MMIAvatarsByID.end() )
        {
            result.LogData.push_back(
                "Cannot add avatar {avatar.Name}, avatar is already registered" );
            continue;
        }

        // Add the scene object to id dictionary
        if( this->MMIAvatarsByID.find( avatar.ID ) == this->MMIAvatarsByID.end() )
        {
            // Add the new MAvatar in a AMMIAvatar
            // TODO --> does not work so far --> clarifiy what should be done here --> spawning of a
            // new avatar seems to be required
            // this->MMIAvatarsByID.insert(pair<string, shared_ptr<AMMIAvatar>> {avatar.ID,
            // make_shared<AMMIAvatar>(AMMIAvatar{})});
            // MMIAvatarsByID[avatar.ID]->MAvatarPtr = &avatar;
        }

        // Add name <-> id mapping
        if( this->nameIdMappingAvatars.find( avatar.Name ) == this->nameIdMappingAvatars.end() )
            this->nameIdMappingAvatars.insert(
                pair<string, vector<string>>{avatar.Name, vector<string>{avatar.ID}} );
        else
        {
            // To do check if list already contains the id
            this->nameIdMappingAvatars[avatar.ID].push_back( avatar.ID );
        }
    }

    return result;
}

// TODO: does not work so far --> Objects would have to be spawned in the scene
MBoolResponse UnrealSceneAccess::AddSceneObjects( vector<MSceneObject> sceneObjects )
{
    MBoolResponse result;
    result.__set_Successful( true );

    // Iterate over each scene object
    for( MSceneObject& sceneObject : sceneObjects )
    {
        if( this->MMISceneObjectsByID.find( sceneObject.ID ) != this->MMISceneObjectsByID.end() )
        {
            result.LogData.push_back(
                "Cannot add avatar {sceneObject.Name}, object is already registered" );
            continue;
        }

        // Add the scene object to id dictionary
        if( this->MMISceneObjectsByID.find( sceneObject.ID ) == this->MMISceneObjectsByID.end() )
        {
            // Add the original one
            // TODO --> does not work so far --> clarifiy what should be done here
            // this->MMISceneObjectsByID.insert(pair<string, shared_ptr<UMMISceneObject>>
            // {sceneObject.ID, make_shared<UMMISceneObject>(UMMISceneObject{})});
            // this->MMISceneObjectsByID[sceneObject.ID]->MSceneObject = sceneObject;
        }

        // Add name <-> id mapping
        if( this->nameIdMappingSceneObjects.find( sceneObject.Name ) ==
            this->nameIdMappingSceneObjects.end() )
            this->nameIdMappingSceneObjects.insert(
                pair<string, vector<string>>{sceneObject.Name, vector<string>{sceneObject.ID}} );
        else
        {
            // To do check if list already contains the id
            this->nameIdMappingSceneObjects[sceneObject.ID].push_back( sceneObject.ID );
        }
    }

    return result;
}

MBoolResponse UnrealSceneAccess::UpdateAvatars( vector<MAvatarUpdate> avatars )
{
    MBoolResponse result;
    result.__set_Successful( true );

    // Iterate over each avatar
    for( MAvatarUpdate avatarUpdate : avatars )
    {
        try
        {
            this->MMIAvatarsByID.at( avatarUpdate.ID )->MAvatar.Description =
                avatarUpdate.Description;

            this->MMIAvatarsByID.at( avatarUpdate.ID )->MAvatar.PostureValues =
                avatarUpdate.PostureValues;

            if( !avatarUpdate.SceneObjects.empty() )
                this->MMIAvatarsByID.at( avatarUpdate.ID )->MAvatar.SceneObjects =
                    avatarUpdate.SceneObjects;
        }
        catch( exception e )
        {
            string loggerMsg( e.what() );
            runtime_error( "Avatar ID is not known, exception message: " + loggerMsg );
            Logger::printLog( L_ERROR, " Avatar ID is not known, exception message: " + loggerMsg );
        }
    }
    return result;
}

MBoolResponse UnrealSceneAccess::UpdateSceneObjects( vector<MSceneObjectUpdate> sceneObjects )
{
    MBoolResponse result;
    result.__set_Successful( true );

    // Iterate over each scene object
    for( MSceneObjectUpdate sceneObjectUpdate : sceneObjects )
    {
        if( this->MMISceneObjectsByID.find( sceneObjectUpdate.ID ) !=
            this->MMISceneObjectsByID.end() )
        {
            // Update the transform
            MTransformUpdate transformUpdate = sceneObjectUpdate.Transform;
            // convert vector to MVector3
            MVector3 mVect3;
            MVector3Extensions::ToMVector3( mVect3, transformUpdate.Position );
            this->MMISceneObjectsByID[sceneObjectUpdate.ID]->MSceneObject.Transform.Position =
                mVect3;
            // convert rotation to quarternion
            MQuaternion mQuat;
            MQuaternionExtensions::ToMQuaternion( mQuat, transformUpdate.Rotation );
            this->MMISceneObjectsByID[sceneObjectUpdate.ID]->MSceneObject.Transform.Rotation =
                mQuat;
            // update the remaining fields
            this->MMISceneObjectsByID[sceneObjectUpdate.ID]->MSceneObject.Transform.Parent =
                transformUpdate.Parent;
            this->MMISceneObjectsByID[sceneObjectUpdate.ID]->MSceneObject.Mesh =
                sceneObjectUpdate.Mesh;
            this->MMISceneObjectsByID[sceneObjectUpdate.ID]->MSceneObject.Collider =
                sceneObjectUpdate.Collider;
            this->MMISceneObjectsByID[sceneObjectUpdate.ID]->MSceneObject.PhysicsProperties =
                sceneObjectUpdate.PhysicsProperties;
        }
        else
        {
            runtime_error( "Object ID is not known." );
            Logger::printLog( L_ERROR, " Object ID is not known." );
        }
    }
    return result;
}

// Removes avatars from the scene by given ids
MBoolResponse UnrealSceneAccess::RemoveAvatars( vector<string> avatarIDs )
{
    MBoolResponse result;
    result.__set_Successful( true );

    // Iterate over each scene object
    for( string id : avatarIDs )
    {
        // Find the object
        if( MMIAvatarsByID.find( id ) != MMIAvatarsByID.end() )
        {
            string avatarName = MMIAvatarsByID[id]->MAvatar.Name;

            // Remove the mapping
            if( nameIdMappingAvatars.find( avatarName ) != nameIdMappingAvatars.end() )
            {
                nameIdMappingAvatars.erase( nameIdMappingAvatars.find( avatarName ) );
            }

            // Remove the scene object from the dictionary
            MMIAvatarsByID[id]->~AMMIAvatar();
            MMIAvatarsByID.erase( id );
        }
    }
    return result;
}

// Removes scene objects from the scene by given ids
MBoolResponse UnrealSceneAccess::RemoveSceneObjects( vector<string> sceneObjectIDs )
{
    MBoolResponse result;
    result.__set_Successful( true );

    // Iterate over each scene object
    for( string id : sceneObjectIDs )
    {
        // Find the object
        if( MMISceneObjectsByID.find( id ) != MMISceneObjectsByID.end() )
        {
            string objectName = MMISceneObjectsByID[id]->MSceneObject.Name;

            // Remove the mapping
            if( nameIdMappingSceneObjects.find( objectName ) != nameIdMappingSceneObjects.end() )
            {
                nameIdMappingSceneObjects.erase( nameIdMappingSceneObjects.find( objectName ) );
            }

            // Remove the scene object from the dictionary
            MMISceneObjectsByID[id]->~UMMISceneObject();
            MMISceneObjectsByID.erase( id );
        }
    }
    return result;
}

void UnrealSceneAccess::GetSceneObjects( vector<MSceneObject>& _return )
{
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        _return.push_back( it->second->MSceneObject );
    }
}

void UnrealSceneAccess::GetSceneObjectByID( MSceneObject& _return, const string& id )
{
    _return = MMISceneObjectsByID[id]->MSceneObject;
}

void UnrealSceneAccess::GetSceneObjectByName( MSceneObject& _return, const string& name )
{
    // TODO: returns only first found MSceneObject in the current implementation, what happens if
    // there is more than one MSceneObject with the same name
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        if( it->second->MSceneObject.Name == name )
            _return = it->second->MSceneObject;
    }
}

void UnrealSceneAccess::GetSceneObjectsInRange( vector<MSceneObject>& _return,
                                                const MVector3& position, const double range )
{
    vector<MSceneObject> allObjects;
    this->GetSceneObjects( allObjects );
    for( MSceneObject sceneObject : allObjects )
    {
        // Check if in range
        MVector3 posDiff;
        MVector3Extensions::Subtract( posDiff, sceneObject.Transform.Position, position );
        float magDiff = MVector3Extensions::Magnitude( posDiff );
        if( static_cast<double>( magDiff ) <= range )
            _return.push_back( sceneObject );
    }
}

void UnrealSceneAccess::GetColliders( vector<MCollider>& _return )
{
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        _return.push_back( it->second->MSceneObject.Collider );
    }
}

void UnrealSceneAccess::GetColliderById( MCollider& _return, const string& id )
{
    if( this->MMISceneObjectsByID.find( id ) != this->MMISceneObjectsByID.end() )
        _return = this->MMISceneObjectsByID[id]->MSceneObject.Collider;
}

void UnrealSceneAccess::GetCollidersInRange( vector<MCollider>& _return, const MVector3& position,
                                             const double range )
{
    vector<MSceneObject> allObjects;
    this->GetSceneObjects( allObjects );
    for( MSceneObject sceneObject : allObjects )
    {
        // Check if in range
        MVector3 posDiff;
        MVector3Extensions::Subtract( posDiff, sceneObject.Transform.Position, position );
        float magDiff = MVector3Extensions::Magnitude( posDiff );
        if( static_cast<double>( magDiff ) <= range )
            _return.push_back( sceneObject.Collider );
    }
}

void UnrealSceneAccess::GetMeshes( vector<MMesh>& _return )
{
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        _return.push_back( it->second->MSceneObject.Mesh );
    }
}

void UnrealSceneAccess::GetMeshByID( MMesh& _return, const string& id )
{
    if( this->MMISceneObjectsByID.find( id ) != this->MMISceneObjectsByID.end() )
        _return = this->MMISceneObjectsByID[id]->MSceneObject.Mesh;
}

void UnrealSceneAccess::GetTransforms( vector<MTransform>& _return )
{
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        _return.push_back( it->second->MSceneObject.Transform );
    }
}

void UnrealSceneAccess::GetTransformByID( MTransform& _return, const string& id )
{
    if( this->MMISceneObjectsByID.find( id ) != this->MMISceneObjectsByID.end() )
        _return = this->MMISceneObjectsByID[id]->MSceneObject.Transform;
}

void UnrealSceneAccess::GetAvatars( vector<MAvatar>& _return )
{
    for( auto it = MMIAvatarsByID.begin(); it != MMIAvatarsByID.end(); it++ )
    {
        _return.push_back( it->second->MAvatar );
    }
}

void UnrealSceneAccess::GetAvatarByID( MAvatar& _return, const string& id )
{
    _return = MMIAvatarsByID[id]->MAvatar;
}

void UnrealSceneAccess::GetAvatarByName( MAvatar& _return, const string& name )
{
    // TODO: returns only first found avatar in the current implementation, what happens if there is
    // more than one avatar with the same name
    for( auto it = MMIAvatarsByID.begin(); it != MMIAvatarsByID.end(); it++ )
    {
        if( it->second->MAvatar.Name == name )
            _return = it->second->MAvatar;
    }
}

void UnrealSceneAccess::GetAvatarsInRange( vector<MAvatar>& _return, const MVector3& position,
                                           const double distance )
{
    vector<MAvatar> allAvatars;
    this->GetAvatars( allAvatars );
    for( MAvatar avatar : allAvatars )
    {
        // Check if in range
        MVector3 avatarPosition;
        avatarPosition.__set_X( avatar.PostureValues.PostureData[0] );
        avatarPosition.__set_Y( avatar.PostureValues.PostureData[1] );
        avatarPosition.__set_Z( avatar.PostureValues.PostureData[2] );
        MVector3 posDiff;
        MVector3Extensions::Subtract( posDiff, avatarPosition, position );
        float magDiff = MVector3Extensions::Magnitude( posDiff );
        if( static_cast<double>( magDiff ) <= distance )
            _return.push_back( avatar );
    }
}

double UnrealSceneAccess::GetSimulationTime()
{
    return this->SimulationTime;
}

void UnrealSceneAccess::GetNavigationMesh( MNavigationMesh& _return )
{
    Logger::printLog( L_ERROR, " GetNavigationMesh is not implemented so far." );
    runtime_error( "GetNavigationMesh is not implemented so far." );
}

void UnrealSceneAccess::GetData( string& _return, const string& fileFormat,
                                 const string& selection )
{
    Logger::printLog( L_ERROR, " GetData is not implemented so far." );
    runtime_error( "GetData is not implemented so far." );
}

void UnrealSceneAccess::GetAttachments( vector<MAttachment>& _return )
{
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        for( MAttachment attachment : it->second->MSceneObject.Attachments )
        {
            _return.push_back( attachment );
        }
    }
}

void UnrealSceneAccess::GetAttachmentsByID( vector<MAttachment>& _return, const string& id )
{
    for( MAttachment attachment : this->MMISceneObjectsByID[id]->MSceneObject.Attachments )
    {
        _return.push_back( attachment );
    }
}

void UnrealSceneAccess::GetAttachmentsByName( vector<MAttachment>& _return, const string& name )
{
    // TODO: returns only first found MSceneObject in the current implementation, what happens if
    // there is more than one MSceneObject with the same name
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        if( it->second->MSceneObject.Name == name )
        {
            for( MAttachment attachment : it->second->MSceneObject.Attachments )
            {
                _return.push_back( attachment );
            }
        }
    }
}

void UnrealSceneAccess::GetAttachmentsChildrenRecursive( vector<MAttachment>& _return,
                                                         const string& id )
{
    Logger::printLog( L_ERROR, " GetAttachmentsChildrenRecursive is not implemented so far." );
    runtime_error( "GetAttachmentsChildrenRecursive is not implemented so far." );
}

void UnrealSceneAccess::GetAttachmentsParentsRecursive( vector<MAttachment>& _return,
                                                        const string& id )
{
    Logger::printLog( L_ERROR, " GetAttachmentsParentsRecursive is not implemented so far." );
    runtime_error( "GetAttachmentsParentsRecursive is not implemented so far." );
}

void UnrealSceneAccess::GetStatus( map<string, string>& _return )
{
    _return.insert( pair<string, string>{"Running", "True"} );
}

void UnrealSceneAccess::GetDescription( MServiceDescription& _return )
{
    Logger::printLog( L_ERROR, " GetDescription is not implemented in UnrealSceneAccessServer." );
    runtime_error( "GetDescription is not implemented in UnrealSceneAccessServer." );
}

void UnrealSceneAccess::Setup( MBoolResponse& _return, const MAvatarDescription& avatar,
                               const map<string, string>& properties )
{
    Logger::printLog( L_ERROR, " Setup is not implemented so far." );
    runtime_error( "Setup is not implemented so far." );
}

void UnrealSceneAccess::Consume( map<string, string>& _return,
                                 const map<string, string>& properties )
{
    Logger::printLog( L_ERROR, " Consume is not implemented so far." );
    runtime_error( "Consume is not implemented so far." );
}

void UnrealSceneAccess::Dispose( MBoolResponse& _return, const map<string, string>& properties )
{
    Logger::printLog( L_ERROR, " Function is implemented in UnrealSceneAcessServer." );
    runtime_error( "Function is implemented in UnrealSceneAcessServer." );
}

void UnrealSceneAccess::Restart( MBoolResponse& _return, const map<string, string>& properties )
{
    Logger::printLog( L_ERROR, " Restart is not implemented so far." );
    runtime_error( "Restart is not implemented so far." );
}

// Add new MMISceneObject in the Dictionary containing all MMI scene objects structured by the
// specific id
void UnrealSceneAccess::AddMMISceneObject( string id, UMMISceneObject* sceneObj )
{
    if( this->MMISceneObjectsByID.find( id ) == this->MMISceneObjectsByID.end() )
    {
        this->MMISceneObjectsByID.insert( pair<string, UMMISceneObject*>{id, sceneObj} );
    }
    else
    {
        Logger::printLog( L_ERROR, " Cannot add MMISceneObject, ID is already in use." );
        runtime_error( "Cannot add MMISceneObject, ID is already in use." );
    }
}

// Add new MMIAvatar in the Dictionary containing all MMI avatars structured by the specific id
void UnrealSceneAccess::AddMMIAvatar( string id, AMMIAvatar* avatar )
{
    if( this->MMIAvatarsByID.find( id ) == this->MMIAvatarsByID.end() )
    {
        this->MMIAvatarsByID.insert( pair<string, AMMIAvatar*>{id, avatar} );
    }
    else
    {
        Logger::printLog( L_ERROR, " Cannot add MMIAvatar, ID is already in use." );
        runtime_error( "Cannot add MMIAvatar, ID is already in use." );
    }
}

vector<UMMISceneObject*> UnrealSceneAccess::GetMMISceneObjectsVector()
{
    vector<UMMISceneObject*> _return;
    for( auto it = MMISceneObjectsByID.begin(); it != MMISceneObjectsByID.end(); it++ )
    {
        _return.push_back( it->second );
    }
    return _return;
}

vector<AMMIAvatar*> UnrealSceneAccess::GetMMIAvatarsVector()
{
    vector<AMMIAvatar*> _return;
    for( auto it = MMIAvatarsByID.begin(); it != MMIAvatarsByID.end(); it++ )
    {
        _return.push_back( it->second );
    }
    return _return;
}

void UnrealSceneAccess::RemoveMMISceneObject( string id )
{
    if( this->MMISceneObjectsByID.find( id ) == this->MMISceneObjectsByID.end() )
    {
        this->MMISceneObjectsByID.erase( MMISceneObjectsByID.find( id ) );
    }
    else
    {
        Logger::printLog( L_ERROR, " Cannot remove MMISceneObject, ID is not found." );
        runtime_error( "Cannot remove MMISceneObject, ID is not found." );
    }
}

void UnrealSceneAccess::RemoveMMIAvatar( string id )
{
    if( this->MMIAvatarsByID.find( id ) == this->MMIAvatarsByID.end() )
    {
        this->MMIAvatarsByID.erase( MMIAvatarsByID.find( id ) );
    }
    else
    {
        Logger::printLog( L_ERROR, " Cannot remove MMIAvatar, ID is not found." );
        runtime_error( "Cannot remove MMIAvatar, ID is not found." );
    }
}

/////////////////////////////////////////////////////////////////////////////////////////////
// Methods for accessing the SceneUpdate

void UnrealSceneAccess::GetSceneChanges( MSceneUpdate& _return )
{
    _return = this->SceneUpdate;
}

void UnrealSceneAccess::GetFullScene( MSceneUpdate& _return )
{
    vector<MSceneObject> MSceneObjects;
    this->GetSceneObjects( MSceneObjects );
    _return.__set_AddedSceneObjects( MSceneObjects );
    _return.__isset.AddedSceneObjects = true;
    vector<MAvatar> MAvatars;
    this->GetAvatars( MAvatars );
    _return.__set_AddedAvatars( MAvatars );
    _return.__isset.AddedAvatars = true;
}

/////////////////////////////////////////////////////////////////////////////////////////////
// Methods for updating the SceneUpdate

// add an avatar to the SceneUpdate
void UnrealSceneAccess::AddAvatar_SceneUpdate( MAvatar& avatar )
{
    this->SceneUpdate.AddedAvatars.push_back( avatar );
    this->SceneUpdate.__isset.AddedAvatars = true;
}

// add a scene object to the SceneUpdate
void UnrealSceneAccess::AddSceneObject_SceneUpdate( MSceneObject& sceneObject )
{
    this->SceneUpdate.AddedSceneObjects.push_back( sceneObject );
    this->SceneUpdate.__isset.AddedSceneObjects = true;
}

// Method to signalize changed physics properties
void UnrealSceneAccess::PhysicsPropertiesChanged_SceneUpdate(
    MSceneObject& sceneObject, MPhysicsProperties& physicsProperties )
{
    MSceneObjectUpdate sceneObjUpdate;
    sceneObjUpdate.__set_ID( sceneObject.ID );
    sceneObjUpdate.__set_PhysicsProperties( physicsProperties );
    this->SceneUpdate.ChangedSceneObjects.push_back( sceneObjUpdate );
    this->SceneUpdate.__isset.ChangedSceneObjects = true;
}

// Method to signalize transformation changes of a scene object
void UnrealSceneAccess::TransformationChanged_SceneUpdate( MSceneObject& sceneObject,
                                                           MVector3& position,
                                                           MQuaternion& rotation, string& parent )
{
    if( any_of( this->SceneUpdate.ChangedSceneObjects.begin(),
                this->SceneUpdate.ChangedSceneObjects.end(),
                [sceneObject]( MSceneObjectUpdate& _sceneObjectUpdate ) {
                    return _sceneObjectUpdate.ID == sceneObject.ID;
                } ) )
    {
        // find the iterator and update the posture values
        for( auto it = this->SceneUpdate.ChangedSceneObjects.begin();
             it != this->SceneUpdate.ChangedSceneObjects.end(); )
        {
            if( it->ID != sceneObject.ID )
            {
                it++;
            }
            else
            {
                MTransformUpdate transform;
                transform.__set_Parent( parent );

                vector<double> posVect( 3 );
                MVector3Extensions::ToDoubleVector( posVect, position );
                transform.__set_Position( posVect );

                vector<double> rotVect( 4 );
                MQuaternionExtensions::ToDoubleVector( rotVect, rotation );
                transform.__set_Rotation( rotVect );

                it->__set_Transform( transform );
                break;
            }
        }
    }
    else
    {
        MSceneObjectUpdate sceneObjUpdate;
        sceneObjUpdate.__set_ID( sceneObject.ID );

        MTransformUpdate transform;
        transform.__set_Parent( parent );

        vector<double> posVect( 3 );
        MVector3Extensions::ToDoubleVector( posVect, position );
        transform.__set_Position( posVect );

        vector<double> rotVect( 4 );
        MQuaternionExtensions::ToDoubleVector( rotVect, rotation );
        transform.__set_Rotation( rotVect );

        sceneObjUpdate.__set_Transform( transform );

        this->SceneUpdate.ChangedSceneObjects.push_back( sceneObjUpdate );
        this->SceneUpdate.__isset.ChangedSceneObjects = true;
    }
}

// Update the posture values in the SceneUpdate
void UnrealSceneAccess::PostureValuesChanged_SceneUpdate( MAvatar& avatar,
                                                          MAvatarPostureValues& postureValues )
{
    // check if avatar already exists
    if( any_of(
            this->SceneUpdate.ChangedAvatars.begin(), this->SceneUpdate.ChangedAvatars.end(),
            [avatar]( MAvatarUpdate& _avatarUpdate ) { return _avatarUpdate.ID == avatar.ID; } ) )
    {
        // find the iterator and update the posture values
        for( auto it = this->SceneUpdate.ChangedAvatars.begin();
             it != this->SceneUpdate.ChangedAvatars.end(); )
        {
            if( it->ID != avatar.ID )
            {
                it++;
            }
            else
            {
                it->__set_PostureValues( postureValues );
                break;
            }
        }
    }
    else
    {
        // generate a new update
        MAvatarUpdate avatarUpdate;
        avatarUpdate.__set_ID( avatar.ID );
        avatarUpdate.__set_PostureValues( postureValues );
        this->SceneUpdate.ChangedAvatars.push_back( avatarUpdate );
        this->SceneUpdate.__isset.ChangedAvatars = true;
    }
}

// Update the Avatar in the SceneUpdate
void UnrealSceneAccess::AvatarChanged_SceneUpdate( MAvatar& avatar )
{
    // check if avatar already exists
    if( any_of(
            this->SceneUpdate.ChangedAvatars.begin(), this->SceneUpdate.ChangedAvatars.end(),
            [avatar]( MAvatarUpdate& _avatarUpdate ) { return _avatarUpdate.ID == avatar.ID; } ) )
    {
        // find the iterator and update the posture values
        for( auto it = this->SceneUpdate.ChangedAvatars.begin();
             it != this->SceneUpdate.ChangedAvatars.end(); )
        {
            if( it->ID != avatar.ID )
            {
                it++;
            }
            else
            {
                it->__set_PostureValues( avatar.PostureValues );
                it->__set_Description( avatar.Description );
                it->__set_SceneObjects( avatar.SceneObjects );
                break;
            }
        }
    }
    else
    {
        // generate a new update
        MAvatarUpdate avatarUpdate;
        avatarUpdate.__set_ID( avatar.ID );
        avatarUpdate.__set_PostureValues( avatar.PostureValues );
        avatarUpdate.__set_Description( avatar.Description );
        avatarUpdate.__set_SceneObjects( avatar.SceneObjects );
        this->SceneUpdate.ChangedAvatars.push_back( avatarUpdate );
        this->SceneUpdate.__isset.ChangedAvatars = true;
    }
}

// Update the SceneObject in the SceneUpdate
void UnrealSceneAccess::SceneObjectChanged_SceneUpdate( MSceneObject& sceneObject )
{
    // check if avatar already exists
    if( any_of( this->SceneUpdate.ChangedSceneObjects.begin(),
                this->SceneUpdate.ChangedSceneObjects.end(),
                [sceneObject]( MSceneObjectUpdate& _sceneObjUpdate ) {
                    return _sceneObjUpdate.ID == sceneObject.ID;
                } ) )
    {
        // find the iterator and update the posture values
        for( auto it = this->SceneUpdate.ChangedSceneObjects.begin();
             it != this->SceneUpdate.ChangedSceneObjects.end(); )
        {
            if( it->ID != sceneObject.ID )
            {
                it++;
            }
            else
            {
                // update the transform
                MTransformUpdate transform;
                transform.__set_Parent( sceneObject.Transform.Parent );
                vector<double> posVect( 3 );
                MVector3Extensions::ToDoubleVector( posVect, sceneObject.Transform.Position );
                transform.__set_Position( posVect );
                vector<double> rotVect( 4 );
                MQuaternionExtensions::ToDoubleVector( rotVect, sceneObject.Transform.Rotation );
                transform.__set_Rotation( rotVect );
                it->__set_Transform( transform );
                // now the rest
                it->__set_Collider( sceneObject.Collider );
                it->__set_Mesh( sceneObject.Mesh );
                it->__set_PhysicsProperties( sceneObject.PhysicsProperties );
                break;
            }
        }
    }
    else
    {
        // generate a new update
        MSceneObjectUpdate sceneObjUpdate;
        sceneObjUpdate.__set_ID( sceneObject.ID );
        // update the transform
        MTransformUpdate transform;
        transform.__set_Parent( sceneObject.Transform.Parent );
        vector<double> posVect( 3 );
        MVector3Extensions::ToDoubleVector( posVect, sceneObject.Transform.Position );
        transform.__set_Position( posVect );
        vector<double> rotVect( 4 );
        MQuaternionExtensions::ToDoubleVector( rotVect, sceneObject.Transform.Rotation );
        transform.__set_Rotation( rotVect );
        sceneObjUpdate.__set_Transform( transform );
        // now the rest
        sceneObjUpdate.__set_Collider( sceneObject.Collider );
        sceneObjUpdate.__set_Mesh( sceneObject.Mesh );
        sceneObjUpdate.__set_PhysicsProperties( sceneObject.PhysicsProperties );
        // add to the list of Scene Object Updates
        this->SceneUpdate.ChangedSceneObjects.push_back( sceneObjUpdate );
        this->SceneUpdate.__isset.ChangedSceneObjects = true;
    }
}

// Method to remove a scene object from the scene update
void UnrealSceneAccess::RemoveSceneObject_SceneUpdate( MSceneObject& sceneObject )
{
    this->SceneUpdate.RemovedSceneObjects.push_back( sceneObject.ID );
    this->SceneUpdate.__isset.RemovedSceneObjects = true;
}

// Method to remove an avatar from the scene update
void UnrealSceneAccess::RemoveAvatar_SceneUpdate( MAvatar& avatar )
{
    this->SceneUpdate.RemovedAvatars.push_back( avatar.ID );
    this->SceneUpdate.__isset.RemovedAvatars = true;
}

// clear updated events
void UnrealSceneAccess::Clear_SceneUpdate()
{
    this->SceneUpdate = MSceneUpdate{};
}
