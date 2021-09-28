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

#pragma once

#include "MMISettings.h"
#include "UnrealSceneAccessServer.h"

#include "Windows\AllowWindowsPlatformTypes.h"
#include "gen-cpp/MSceneAccess.h"
#include "gen-cpp/MSynchronizableScene.h"
#include "gen-cpp/scene_types.h"
#include "Windows\HideWindowsPlatformTypes.h"

#include <unordered_map>
#include <queue>
#include <tuple>

using namespace MMIStandard;
using namespace std;

// forward declarations
class AMMIAvatar;
class UMMISceneObject;

class UnrealSceneAccess : virtual public MSceneAccessIf, virtual public MSynchronizableSceneIf
{
public:
    // constructor
    UnrealSceneAccess();

    // destructor
    ~UnrealSceneAccess();

    // create UUI
    string CreateUUID();

    // create seperate scene object id by incrementing the number
    // does not provide a unique UUID in a global scope
    static string CreateSceneObjectID();

    // create seperate scene avatar id by incrementing the number
    // does not provide a unique UUID in a global scope
    static string CreateAvatarID();

    // pointer to the UnrealSceneAccess server core
    shared_ptr<SceneAccessServer::UnrealSceneAccessServer> UESceneAccessServerCore;

    // initialize and start the remote scene access servers
    void InitializeServers();

    // Inherited from MSceneAccessIf
    // Returns a list of all scene objects
    virtual void GetSceneObjects( vector<MSceneObject>& _return ) override;
    // Returns the scene object based on the id (if available)
    virtual void GetSceneObjectByID( MSceneObject& _return, const string& id ) override;
    // Returns a scene object by name (if available)
    virtual void GetSceneObjectByName( MSceneObject& _return, const string& name ) override;
    // Returns all scene objects in a particular range
    virtual void GetSceneObjectsInRange( vector<MSceneObject>& _return, const MVector3& position,
                                         const double range ) override;
    // Returns all colliders
    virtual void GetColliders( vector<MCollider>& _return ) override;
    // Returns the collider based on a given id (if available)
    virtual void GetColliderById( MCollider& _return, const string& id ) override;
    // Returns all colliders in range
    virtual void GetCollidersInRange( vector<MCollider>& _return, const MVector3& position,
                                      const double range ) override;
    // Returns all meshes
    virtual void GetMeshes( vector<MMesh>& _return ) override;
    // Returns a mesh defined by the id (if available)
    virtual void GetMeshByID( MMesh& _return, const string& id ) override;
    // Returns all transforms in the scene
    virtual void GetTransforms( vector<MTransform>& _return ) override;
    // Returns the transform by id (if available)
    virtual void GetTransformByID( MTransform& _return, const string& id ) override;
    // Returns all avatars in the scene
    virtual void GetAvatars( vector<MAvatar>& _return ) override;
    // Returns an avatar by id (if available)
    virtual void GetAvatarByID( MAvatar& _return, const string& id ) override;
    // Returns an avatar by a given name (if available)
    virtual void GetAvatarByName( MAvatar& _return, const string& name ) override;
    // Returns the avatars in range of the specified position
    virtual void GetAvatarsInRange( vector<MAvatar>& _return, const MVector3& position,
                                    const double distance ) override;
    // Returns the current simulation time
    virtual double GetSimulationTime() override;
    // Returns the changes / scene manipulations of the last frame
    virtual void GetSceneChanges( MSceneUpdate& _return ) override;
    // Returns the full scene in form of a list of scene manipulations
    virtual void GetFullScene( MSceneUpdate& _return ) override;
    // Returns a navigation mesh of the scene
    virtual void GetNavigationMesh( MNavigationMesh& _return ) override;
    // Returns the scene in a specific format (E.g. gltf) -> To do
    virtual void GetData( string& _return, const string& fileFormat,
                          const string& selection ) override;
    // Returns all attachements
    virtual void GetAttachments( vector<MAttachment>& _return ) override;
    // Returns the attachments by the given id (if available)
    virtual void GetAttachmentsByID( vector<MAttachment>& _return, const string& id ) override;
    // Returns the attachmetns by the given name (if available)
    virtual void GetAttachmentsByName( vector<MAttachment>& _return, const string& name ) override;
    // Returns all attachments of the children (recursively) including the one of the scene object
    // specified by the id).
    virtual void GetAttachmentsChildrenRecursive( vector<MAttachment>& _return,
                                                  const string& id ) override;
    // Returns all attachments of the parents (recursively) including the one of the scene object
    // specified by the id).
    virtual void GetAttachmentsParentsRecursive( vector<MAttachment>& _return,
                                                 const string& id ) override;

    // inherited from MMIServiceBaseIf
    // Returns the present status
    virtual void GetStatus( map<string, string>& _return ) override;
    // Provides the description of the scene service
    virtual void GetDescription( MServiceDescription& _return ) override;
    // Setup function defined in the MMIServiceBase interface
    virtual void Setup( MBoolResponse& _return, const MAvatarDescription& avatar,
                        const map<string, string>& properties ) override;
    // Consume function defined in the MMIServiceBase interface
    virtual void Consume( map<string, string>& _return,
                          const map<string, string>& properties ) override;
    // Disposes the service
    virtual void Dispose( MBoolResponse& _return, const map<string, string>& properties ) override;
    // Restarts the scene service
    virtual void Restart( MBoolResponse& _return, const map<string, string>& properties ) override;

    // inherited from MSynchronizableSceneIf
    // Applies the scene updates on the scene -> Sychronization of the scene
    virtual void ApplyUpdates( MBoolResponse& _return, const MSceneUpdate& sceneUpdates ) override;
    // Applies manipulations
    virtual void ApplyManipulations(
        MBoolResponse& _return, const vector<MSceneManipulation>& sceneManipulations ) override;

    // The current frame id
    int FrameID;
    // Store the last n frames
    int HistoryBufferSize;
    // The current simulation time
    float SimulationTime;
    // A queue which contains the history of the last n applied scene manipulations
    queue<tuple<int, const MSceneUpdate&>> SceneHistory;
    //// A queue holding the remote scene manipulation requests that might have been inserted
    /// asynchronously (the server runs on a separate thread)
    // queue<RemoteSceneManipulation> RemoteSceneManipulations;

    // Add new MMISceneObject in the Dictionary containing all MMI scene objects structured by the
    // specific id
    void AddMMISceneObject( string id, UMMISceneObject* sceneObj );
    // Remove MMISceneObject in the Dictionary containing all MMI scene objects structured by the
    // specific id
    void RemoveMMISceneObject( string id );
    // return registered MMISceneObjects as vector
    vector<UMMISceneObject*> GetMMISceneObjectsVector();

    // Add new MMIAvatar in the Dictionary containing all MMI avatars structured by the specific id
    void AddMMIAvatar( string id, AMMIAvatar* avatar );
    // Remove MMIAvatar in the Dictionary containing all MMI avatars structured by the specific id
    void RemoveMMIAvatar( string id );
    // return registered MMISceneObjects as vector
    vector<AMMIAvatar*> GetMMIAvatarsVector();

    // The last processed scene update
    MSceneUpdate SceneUpdate;
    // The list of all scene events
    // TODO: is this really necessary? --> Never Used
    vector<MSceneUpdate> SceneChanges;

    //////////////////////////////////////////////////
    // Methods for updateig the SceneUpdate

    // add an avatar to the SceneUpdate
    void AddAvatar_SceneUpdate( MAvatar& avatar );

    // add a scene object to the SceneUpdate
    void AddSceneObject_SceneUpdate( MSceneObject& sceneObject );

    // Method to signalize changed physics properties
    void PhysicsPropertiesChanged_SceneUpdate( MSceneObject& sceneObject,
                                               MPhysicsProperties& physicsProperties );

    // Method to signalize transformation changes of a scene object
    void TransformationChanged_SceneUpdate( MSceneObject& sceneObject, MVector3& position,
                                            MQuaternion& rotation, string& parent );

    // Update the posture values in the SceneUpdate
    void PostureValuesChanged_SceneUpdate( MAvatar& avatar, MAvatarPostureValues& postureValues );

    // Update the Avatar in the SceneUpdate
    void AvatarChanged_SceneUpdate( MAvatar& avatar );

    // Update the SceneObject in the SceneUpdate
    void SceneObjectChanged_SceneUpdate( MSceneObject& sceneObject );

    // Method to remove a scene object in the scene update
    void RemoveSceneObject_SceneUpdate( MSceneObject& sceneObject );

    // Remove an avatar from the scene update
    void RemoveAvatar_SceneUpdate( MAvatar& avatar );

    // clear updated events
    void Clear_SceneUpdate();

protected:
    // Mapping between the name of a scene object and a unique id
    unordered_map<string, vector<string>> nameIdMappingSceneObjects;
    // Dictionary that provides a fast access to the name id mapping of the avatars
    unordered_map<string, vector<string>> nameIdMappingAvatars;

private:
    // settings (e.g. ports, IPs)
    MMISettings* settings;

    // Adds avatars to the internal scene representation
    MBoolResponse AddAvatars( vector<MAvatar> avatars );
    // Adds scene objects to the internal scene representation
    MBoolResponse AddSceneObjects( vector<MSceneObject> sceneObjects );
    // Updates avatars in the internal scene representation
    MBoolResponse UpdateAvatars( vector<MAvatarUpdate> avatars );
    // Updates scene objects in the internal scene representation
    MBoolResponse UpdateSceneObjects( vector<MSceneObjectUpdate> sceneObjects );
    // Removes avatars from the scene by given ids
    MBoolResponse RemoveAvatars( vector<string> avatarIDs );
    // Removes scene objects from the scene by given ids
    MBoolResponse RemoveSceneObjects( vector<string> sceneObjectIDs );

    // Dictionary containing all MMI scene objects structured by the specific id
    unordered_map<string, UMMISceneObject*> MMISceneObjectsByID;

    // Dictionary containing all MMI avatars structured by the specific id
    unordered_map<string, AMMIAvatar*> MMIAvatarsByID;

    friend class SceneAccessServer::UnrealSceneAccessServer;
};
