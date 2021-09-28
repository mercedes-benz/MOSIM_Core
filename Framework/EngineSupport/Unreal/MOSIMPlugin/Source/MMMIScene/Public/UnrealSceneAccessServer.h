// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Stephan Adam

// Wrapper class for connecting the RemoteSceneAccessServer with the UnrealSceneAccess.
// RemoteSceneAccess requires a shared_ptr of the class, what conflicts with the garbage collector
// of Unreal, if an Unreal class has a pointer to the class. This makes this class necessary.

#pragma once

#include "MMISettings.h"

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
class UnrealSceneAccess;

namespace SceneAccessServer
{
class RemoteSceneAccessServer;

class UnrealSceneAccessServer : virtual public MSceneAccessIf,
                                virtual public MSynchronizableSceneIf,
                                public enable_shared_from_this<UnrealSceneAccessServer>
{
public:
    // constructor
    UnrealSceneAccessServer( UnrealSceneAccess* _SceneAccess );

    // destructor
    ~UnrealSceneAccessServer();

    UnrealSceneAccess* SceneAccess;

    // initialize and start the remote scene access servers. They are initialized in the setup
    // method of the SimulationController
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

protected:
    // server for the remote scene access
    RemoteSceneAccessServer* remoteSceneAccessServer;
    // server for the remote scene manipulation
    // RemoteSceneManipulationServer* remoteSceneManipulationServer;

private:
    // settings (e.g. ports, IPs)
    MMISettings* settings;

    // server thread
    thread* remoteSceneAccessServerThread;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // method constructing thrift client for terminating thrift servers running in seperate threads

public:
    // static void TerminationThriftClient(string const & address, int const & port);
    void TerminationThriftClient();
};
}
