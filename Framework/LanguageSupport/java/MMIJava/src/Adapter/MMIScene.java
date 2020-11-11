package Adapter;

import Extensions.MQuaternionExtensions;
import Extensions.MVector3Extensions;
import MMIStandard.*;

import java.util.*;
import java.util.stream.Collectors;

public class MMIScene implements MSceneAccess.Iface {
    /*
            Class represents a (hypothetical) scene which can be specifically set up by the developer
    */
    //	Map containing all scene objects structured by the specific id
    private final Map<String, MSceneObject> sceneObjectsByID = new HashMap<>();
    //	Map containing all avatars structured by the specific id
    private final Map<String, MAvatar> avatarsByID = new HashMap<>();
    //	Mapping between the name of a scene object and a unique id
    private final Map<String, List<String>> nameIdMappingSceneObjects = new HashMap<>();
    //	Mapping between the name of a avatar and a unique id
    private final Map<String, List<String>> nameIdMappingAvatars = new HashMap<>();
    //	MSceneUpdate from the previous frame
    private MSceneUpdate sceneUpdate = new MSceneUpdate();
    //	ID of the frame
    private int frameID = 0;
    //	A list  which contains the history of the last n applied scene manipulations
    private Queue<Tuple<Integer, MSceneUpdate>> SceneHistory = new LinkedList<>();
    //	The size of the HistoryBuffer
    private final int HistoryBufferSize = 20;

    //	Clears the whole scene
    public void clear() {
        this.avatarsByID.clear();
        this.sceneObjectsByID.clear();
        this.nameIdMappingAvatars.clear();
        this.nameIdMappingSceneObjects.clear();
        this.SceneHistory.clear();
        this.sceneUpdate = new MSceneUpdate();
        this.frameID = 0;
    }

    //	Returns the scene objects
    @Override
    public List<MSceneObject> GetSceneObjects() {
        return new ArrayList<>(this.sceneObjectsByID.values());
    }

    //	Returns the scene object based on the id
    @Override
    public MSceneObject GetSceneObjectByID(String id) {
        return this.sceneObjectsByID.get(id);
    }

    //	Returns the scene object based on the name
    @Override
    public MSceneObject GetSceneObjectByName(String name) {
        return this.GetSceneObjectByID(this.nameIdMappingSceneObjects.get(name).get(0));
    }

    //	Returns the scene object based on range
    @Override
    public List<MSceneObject> GetSceneObjectsInRange(MVector3 position, double range) {
        return this.sceneObjectsByID.values().parallelStream().filter(s -> MVector3Extensions.euclideanDistance(s.Transform.getPosition(), position) <= range).collect(Collectors.toList());
    }

    //	Returns the collider of the scene objects
    @Override
    public List<MCollider> GetColliders() {
        return this.GetSceneObjects().stream().map(MSceneObject::getCollider).collect(Collectors.toList());
    }

    //	Returns the collider of the scene object based on the id
    @Override
    public MCollider GetColliderById(String id) {

        return this.sceneObjectsByID.get(id).Collider;
    }

    //	Returns the collider of the scene objects based on the range
    @Override
    public List<MCollider> GetCollidersInRange(MVector3 position, double range) {
        return this.GetSceneObjectsInRange(position, range).parallelStream().map(MSceneObject::getCollider).collect(Collectors.toList());
    }

    //	Returns the meshes of the scene objects
    @Override
    public List<MMesh> GetMeshes() {
        return this.GetSceneObjects().stream().map(MSceneObject::getMesh).collect(Collectors.toList());
    }

    //	Returns the meshes of the scene object based on the id
    @Override
    public MMesh GetMeshByID(String id) {
        return this.GetSceneObjectByID(id).getMesh();
    }

    //	Returns the transforms of the scene objects
    @Override
    public List<MTransform> GetTransforms() {
        return this.GetSceneObjects().stream().map(MSceneObject::getTransform).collect(Collectors.toList());
    }

    //	Returns the meshes of the scene object based on the id
    @Override
    public MTransform GetTransformByID(String id) {
        return this.GetSceneObjectByID(id).getTransform();
    }

    //	Returns the avatars
    @Override
    public List<MAvatar> GetAvatars() {
        return new ArrayList<>(this.avatarsByID.values());
    }

    //	Returns the avatar based on the id
    @Override
    public MAvatar GetAvatarByID(String id) {
        return this.avatarsByID.get(id);
    }

    //	Returns the avatar based on the name
    @Override
    public MAvatar GetAvatarByName(String name) {
        //return this.avatarsByID.values().stream().filter(a -> a.Name.equals(name)).findFirst().orElse(null);
        return this.avatarsByID.get(this.nameIdMappingAvatars.get(name).get(0));
    }

    //	Returns the avatar based on the range
    @Override
    public List<MAvatar> GetAvatarsInRange(MVector3 position, double distance) {
        List<MAvatar> _result = new ArrayList<>();
        for (MAvatar avatar : this.avatarsByID.values()) {
            MVector3 avatarPosition = new MVector3();
            avatarPosition.setX(avatar.getPostureValues().getPostureData().get(0));
            avatarPosition.setY(avatar.getPostureValues().getPostureData().get(1));
            avatarPosition.setZ(avatar.getPostureValues().getPostureData().get(2));

            if (MVector3Extensions.euclideanDistance(avatarPosition, position) <= distance) {
                _result.add(avatar);
            }
        }
        return _result;
    }

    @Override
    public double GetSimulationTime() {
        return 0;
    }

    // Returns the changes from the previous frame
    @Override
    public MSceneUpdate GetSceneChanges() {
        return this.sceneUpdate;
    }

    // Returns all sceneobjects and avatars as MSceneUpdate
    @Override
    public MSceneUpdate GetFullScene() {
        // List<MSceneObject> sceneObjects = this.GetSceneObjects();
        MSceneUpdate sceneUpdate = new MSceneUpdate();
        sceneUpdate.setAddedSceneObjects(this.GetSceneObjects());
        sceneUpdate.setAddedAvatars(this.GetAvatars());
        return sceneUpdate;
    }

    @Override
    public MNavigationMesh GetNavigationMesh() {

        return null;
    }

    //Applies the scene manipulation on the scene
    // <param name="sceneUpdates">The scene manipulations to be considered</param>
    public MBoolResponse Apply(MSceneUpdate sceneUpdate) {
        MBoolResponse result = new MBoolResponse(true);
        //Increment the frame id
        this.frameID++;
        //Stores the history
        SceneHistory.offer(new Tuple<>(frameID, sceneUpdate));

        //Only allow the max buffer size
        while (SceneHistory.size() > this.HistoryBufferSize)
            this.SceneHistory.poll();

        //Set the scene changes to the input of the present frame
        this.sceneUpdate = sceneUpdate;

        if (sceneUpdate.isSetAddedAvatars())
            this.AddAvatars(result, sceneUpdate.AddedAvatars);

        //Check if there are new scene objects which should be added
        if (sceneUpdate.isSetAddedSceneObjects())
            this.AddSceneObjects(result, sceneUpdate.AddedSceneObjects);


        //Check if there are changed avatars that need to be retransmitted
        if (sceneUpdate.isSetChangedAvatars())
            this.UpdateAvatars(result, sceneUpdate.ChangedAvatars);

        //Check if there are changed sceneObjects that need to be retransmitted
        if (sceneUpdate.isSetChangedSceneObjects())
            this.UpdateSceneObjects(result, sceneUpdate.ChangedSceneObjects);

        //Check if there are avatars that need to be removed
        if (sceneUpdate.isSetRemovedAvatars())
            this.RemoveAvatars(result, sceneUpdate.RemovedAvatars);

        //Check if there are scene objects that need to be removed
        if (sceneUpdate.isSetRemovedSceneObjects())
            this.RemoveSceneObjects(result, sceneUpdate.RemovedSceneObjects);
        return result;
    }

    //	Removes all scene objects from the scene
    //	<param name="sceneObjectIDs">The IDs of the scene objects which should be removed</param>
    private void RemoveSceneObjects(MBoolResponse result, List<String> removedSceneObjects) {
        for (String id : removedSceneObjects) {
            MSceneObject removed = this.sceneObjectsByID.remove(id);
            if (removed != null) {
                this.nameIdMappingSceneObjects.remove(removed.getName());
            } else {
                result.addToLogData("could not remove scene object  with id: " + id + " not found");
                result.setSuccessful(false);
            }
        }
    }

    //	Removes all scene objects from the scene
    //	<param name="avatarIDs">The IDs of the avatars which should be removed</param>
    private void RemoveAvatars(MBoolResponse result, List<String> removedAvatars) {
        for (String id : removedAvatars) {
            MAvatar removed = this.avatarsByID.remove(id);
            if (removed != null) {
                this.nameIdMappingAvatars.remove(removed.getName());
            } else {
                result.addToLogData("could not remove avatar with id: " + id + " not found");
                result.setSuccessful(false);
            }
        }
    }

    //	Updates all scene objects
    //	<param name="avatars>The scene objects which should be updated</param>
    private void UpdateSceneObjects(MBoolResponse result, List<MSceneObjectUpdate> changedSceneObjects) {
        for (MSceneObjectUpdate object : changedSceneObjects) {
            MSceneObject sceneObject = this.sceneObjectsByID.get(object.ID);
            if (sceneObject != null) {
                //Update the transform
                if (object.isSetTransform()) {
                    MTransformUpdate transformUpdate = object.Transform;
                    if (transformUpdate.isSetPosition())
                        sceneObject.Transform.Position = MVector3Extensions.toMVector3(transformUpdate.Position);

                    if (transformUpdate.isSetRotation())
                        sceneObject.Transform.Rotation = MQuaternionExtensions.toMQuaternion(transformUpdate.Rotation);

                    if (transformUpdate.isSetParent())
                        sceneObject.Transform.Parent = transformUpdate.Parent;
                }

                //update the mesh
                if (object.isSetMesh()) {
                    this.sceneObjectsByID.get(object.ID).setMesh(object.getMesh());
                }

                //update the collider
                if (object.isSetCollider()) {
                    this.sceneObjectsByID.get(object.ID).setCollider(object.getCollider());
                }

                //update the physics
                if (object.isSetPhysicsProperties()) {
                    this.sceneObjectsByID.get(object.ID).setPhysicsProperties(object.getPhysicsProperties());
                }
            } else {
                result.addToLogData("Could not update scene object: " + object.ID + " object not found");
                result.setSuccessful(false);
            }
        }

    }

    //	Updates all avatars
    //	<param name="avatars>The avatars which should be updated</param>
    private void UpdateAvatars(MBoolResponse result, List<MAvatarUpdate> changedAvatars) {
        for (MAvatarUpdate avatarUpdate : changedAvatars) {
            MAvatar avatar = this.avatarsByID.get(avatarUpdate.ID);
            if (avatar != null) {
                if (avatarUpdate.isSetDescription())
                    this.avatarsByID.get(avatarUpdate.getID()).Description = avatarUpdate.getDescription();

                if (avatarUpdate.isSetPostureValues())
                    this.avatarsByID.get(avatarUpdate.getID()).PostureValues = avatarUpdate.getPostureValues();


                if (avatarUpdate.isSetSceneObjects()) {
                    this.avatarsByID.get(avatarUpdate.getID()).SceneObjects = avatarUpdate.getSceneObjects();
                }
            } else {
                result.addToLogData("Could not update avatar : " + avatarUpdate.ID + " not found");
                result.setSuccessful(false);
            }
        }
    }

    //	Adds all scene objects to the scene
    //	<param name="sceneObjects>The scene objects which should be added</param>
    private void AddSceneObjects(MBoolResponse result, List<MSceneObject> addedSceneObjects) {
        for (MSceneObject object : addedSceneObjects) {
            if (this.sceneObjectsByID.putIfAbsent(object.ID, object) != null) {

                result.addToLogData("Could not add scene object: " + object.Name + " is already registered");
                result.setSuccessful(false);
                continue;
            }

            if (!this.nameIdMappingSceneObjects.containsKey(object.Name)) {
                this.nameIdMappingSceneObjects.putIfAbsent(object.Name, new ArrayList<>() {
                    {
                        add(object.ID);
                    }
                });
            } else {
                this.nameIdMappingSceneObjects.get(object.Name).add(object.ID);
            }
        }
    }

    //	Adds all avatars to the scene
    //	<param name="avatars">The avatars which should be added</param>
    private void AddAvatars(MBoolResponse result, List<MAvatar> addedAvatars) {
        for (MAvatar avatar : addedAvatars) {
            if (this.avatarsByID.putIfAbsent(avatar.ID, avatar) != null) {

                result.addToLogData("Could not add avatar: " + avatar.Name + " is already registered");
                result.setSuccessful(false);
                continue;
            }
            if (!this.nameIdMappingAvatars.containsKey(avatar.Name)) {
                this.nameIdMappingAvatars.putIfAbsent(avatar.Name, new ArrayList<>() {
                    {
                        add(avatar.ID);
                    }
                });
            } else {
                this.nameIdMappingSceneObjects.get(avatar.Name).add(avatar.ID);
            }
        }
    }
}
