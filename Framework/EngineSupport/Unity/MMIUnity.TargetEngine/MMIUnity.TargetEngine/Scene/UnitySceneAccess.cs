// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Andreas Kaiser

using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MMIUnity.TargetEngine.Scene
{
    /// <summary>
    /// Class which provides a basic scene access for the unity scene. 
    /// This is fundamental component for the synchronization with he MMI framework.
    /// It needs to be added to a root GameObject. All MMISceneObject which should be considered must be below this object in hierarchy.
    /// </summary>
    [RequireComponent(typeof(MMISettings))]
    public class UnitySceneAccess : MonoBehaviour, MSceneAccess.Iface, MSynchronizableScene.Iface
    {
        /// <summary>
        /// Indixer for accessing scene object by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MSceneObject this[string name]
        {
            get
            {
                return this.GetSceneObjectByName(name);
            }
        }

     
        #region public properties

        /// <summary>
        /// The current frame id
        /// </summary>
        [HideInInspector]
        public int FrameID = 0;

        /// <summary>
        /// Store the last n frames
        /// </summary>
        [HideInInspector]
        public int HistoryBufferSize = 20;

        /// <summary>
        /// The current simulation time
        /// </summary>
        public float SimulationTime;

        /// <summary>
        /// A queue which contains the history of the last n applied scnee manipulations
        /// </summary>
        public Queue<Tuple<int, MSceneUpdate>> SceneHistory = new Queue<Tuple<int, MSceneUpdate>>();


        /// <summary>
        /// A queue holding the remote scene manipulation requests that might have been inserted asynchronously (the server runs on a separate thread)
        /// </summary>
        public Queue<RemoteSceneManipulation> RemoteSceneManipulations = new Queue<RemoteSceneManipulation>();


        /// <summary>
        /// The singleton instance
        /// </summary>
        public static UnitySceneAccess Instance
        {
            get;
            private set;
        }

        #endregion

        #region private variables

        /// <summary>
        /// Mutex for accessing scene related elements
        /// </summary>
        private static Mutex sceneMutex = new Mutex();
        private static Mutex sceneObjectIDMutex = new Mutex();
        private static Mutex avatarIDMutex = new Mutex();

        private static int currentSceneObjectID = 1;
        private static int currentAvatarID = 1;

        #endregion

        #region protected fields for storing the data

        /// <summary>
        /// Mapping between the name of a scene object and a unique id
        /// </summary>
        protected Dictionary<string, List<string>> nameIdMappingSceneObjects = new Dictionary<string, List<string>>();

        /// <summary>
        /// Dictionary containing all scene objects structured by the specific id
        /// </summary>
        protected Dictionary<string, MSceneObject> sceneObjectsByID = new Dictionary<string, MSceneObject>();

        /// <summary>
        /// Dictionary which allows the access of different avatar by the corresponding id
        /// </summary>
        protected Dictionary<string, MAvatar> avatarsByID = new Dictionary<string, MAvatar>();

        /// <summary>
        /// Dictionary that provides a fast access to the name id mapping of the avatars
        /// </summary>
        protected Dictionary<string, List<string>> nameIdMappingAvatars = new Dictionary<string, List<string>>();

        /// <summary>
        /// The last processed scene update
        /// </summary>
        protected MSceneUpdate SceneUpdate = new MSceneUpdate();

        /// <summary>
        /// The list of all scene events
        /// </summary>
        protected List<MSceneUpdate> sceneChanges = new List<MSceneUpdate>();


        /// <summary>
        /// Server for the remote scene access
        /// </summary>
        protected RemoteSceneAccessServer remoteSceneAccessServer;

        /// <summary>
        /// Server for the remote scene manipulations
        /// </summary>
        protected RemoteSceneManipulationServer remoteSceneManipulationServer;

        #endregion




        /// <summary>
        /// Basic awake routine
        /// </summary>
        private void Awake()
        {
            //Set the singleton instance
            Instance = this;
        }

        /// <summary>
        /// Basic start routine executed by unity
        /// </summary>
        private void Start()
        {
            MMISettings settings = this.GetComponent<MMISettings>();
            if (settings.AllowRemoteSceneConnections)
            {
                MIPAddress registerAddress = new MIPAddress(settings.MMIRegisterAddress, settings.MMIRegisterPort);

                //Create the server for remote scene access
                this.remoteSceneAccessServer = new RemoteSceneAccessServer(new MIPAddress(settings.RemoteSceneAccessAddress, settings.RemoteSceneAccessPort), registerAddress, this);

                //Create server for remote scene manipulations
                this.remoteSceneManipulationServer = new RemoteSceneManipulationServer(new MIPAddress(settings.RemoteSceneWriteAddress, settings.RemoteSceneWritePort), registerAddress, new RemoteSceneManipulationRequest(this));

                //Start both servers
                this.remoteSceneAccessServer.Start();
                this.remoteSceneManipulationServer.Start();
            }
        }

        /// <summary>
        /// Method is executed if the application is terminated
        /// </summary>
        private void OnApplicationQuit()
        {
            //Dipose the servers if running
            if (this.remoteSceneAccessServer != null)
                this.remoteSceneAccessServer.Dispose();
            if (this.remoteSceneManipulationServer != null)
                this.remoteSceneManipulationServer.Dispose();
        }


        /// <summary>
        /// Creates a new UUI for arbitrary objects within the scene 
        /// </summary>
        /// <returns></returns>
        public static string CreateUUID()
        {
            return Guid.NewGuid().ToString();
        }


        /// <summary>
        /// Creates a seperate scene object id by just incrementing the number. 
        /// Provides unique ids in the scope of the session ID.
        /// Warning this does not create unique UUIDs in a global scope
        /// </summary>
        /// <returns></returns>
        public static string CreateSceneObjectID()
        {
            string id;

            //Wait for the mutex
            sceneObjectIDMutex.WaitOne();

            //Create a new id by incrementing the current id
            id = (currentSceneObjectID++).ToString();

            //Release the mutex
            sceneObjectIDMutex.ReleaseMutex();

            //Return the id
            return id;
        }


        /// <summary>
        /// Creates a seperate scene avatar id by just incrementing the number. 
        /// Provides unique ids in the scope of the session ID.
        /// Warning this does not create unique UUIDs in a global scope
        /// </summary>
        /// <returns></returns>
        public static string CreateAvatarID()
        {
            string id;

            //Wait for the mutex
            avatarIDMutex.WaitOne();

            //Create a new id by incrementing the current id
            id = (currentAvatarID++).ToString();

            //Release the mutex
            avatarIDMutex.ReleaseMutex();

            //Return the id
            return id;
        }


        /// <summary>
        /// Applies the scene updates on the scene -> Sychronization of the scene
        /// </summary>
        /// <param name="sceneUpdates">The scene manipulations to be considered</param>
        /// <param name="deepCopy">Specifies whether the scene manipulations are directly applied or a deep copy is performed</param>
        public virtual MBoolResponse ApplyUpdates(MSceneUpdate sceneUpdate)
        {
            //Create a new bool response
            MBoolResponse result = new MBoolResponse(true);

            //Increment the frame id
            this.FrameID++;

            //Stores the history
            SceneHistory.Enqueue(new Tuple<int, MSceneUpdate>(FrameID, sceneUpdate));

            //Only allow the max buffer size
            while (SceneHistory.Count > this.HistoryBufferSize)
                this.SceneHistory.Dequeue();


            //Check if there are avatars to be added
            if (sceneUpdate.AddedAvatars?.Count > 0)
                this.AddAvatars(sceneUpdate.AddedAvatars);

            //Check if there are new scene objects which should be added
            if (sceneUpdate.AddedSceneObjects?.Count > 0)
                this.AddSceneObjects(sceneUpdate.AddedSceneObjects);

            //Check if there are changed avatars that need to be retransmitted
            if (sceneUpdate.ChangedAvatars?.Count > 0)
                this.UpdateAvatars(sceneUpdate.ChangedAvatars);

            //Check if there are changed sceneObjects that need to be retransmitted
            if (sceneUpdate.ChangedSceneObjects?.Count > 0)
                this.UpdateSceneObjects(sceneUpdate.ChangedSceneObjects);

            //Check if there are avatars that need to be removed
            if (sceneUpdate.RemovedAvatars?.Count > 0)
                this.RemoveAvatars(sceneUpdate.RemovedAvatars);

            //Check if there are scene objects that need to be removed
            if (sceneUpdate.RemovedSceneObjects?.Count > 0)
                this.RemoveSceneObjects(sceneUpdate.RemovedSceneObjects);


            return result;
        }


        /// <summary>
        /// Applies manipulations 
        /// </summary>
        /// <param name="sceneManipulations"></param>
        /// <returns></returns>
        public virtual MBoolResponse ApplyManipulations(List<MSceneManipulation> sceneManipulations)
        {
            //Acquire the mutex
            sceneMutex.WaitOne();

            //Handle each scene manipulation
            foreach (MSceneManipulation sceneManipulation in sceneManipulations)
            {
                //Incorporate the transform updates
                if (sceneManipulation.Transforms != null)
                {
                    //Handle each transform manioulation
                    foreach (MTransformManipulation transformUpdate in sceneManipulation.Transforms)
                    {
                        //Find the corresponding unity match: To do -> performance can be optimized in future (e.g. dictionary)
                        MMISceneObject match = this.transform.GetComponentsInChildren<MMISceneObject>().First(s => s.MSceneObject.Transform.ID == transformUpdate.Target);

                        //Do not update phsysics within the frame, since the transform is actively manipultaed
                        match.UpdatePhysicsCurrentFrame = false;

                        //Update the position (if changed)
                        if (transformUpdate.Position != null)
                            match.transform.position = transformUpdate.Position.ToVector3();

                        //Update the rotation (if changed)
                        if (transformUpdate.Rotation != null)
                            match.transform.rotation = transformUpdate.Rotation.ToQuaternion();

                        //Update the parent (if changed)
                        if (transformUpdate.Parent != null)
                        {
                            MMISceneObject parentMatch = GameObject.FindObjectsOfType<MMISceneObject>().ToList().Find(s => s.MSceneObject.Name == transformUpdate.Parent || s.MSceneObject.Transform.ID == transformUpdate.Parent);

                            if (parentMatch != null)
                                match.transform.SetParent(parentMatch.transform, true);
                        }
                    }
                }

                //Incorporate the phsysics interactions
                if (sceneManipulation.PhysicsInteractions != null)
                {
                    //Handle each physicsInteraction
                    foreach (MPhysicsInteraction physicsInteraction in sceneManipulation.PhysicsInteractions)
                    {
                        //Find the matching object : To do -> performance can be optimized in future (e.g. dictionary)
                        MMISceneObject match = this.transform.GetComponentsInChildren<MMISceneObject>().First(s => s.MSceneObject.Transform.ID == physicsInteraction.Target);

                        if (match != null)
                        {
                            //Activate the rigid body
                            if (match.GetComponent<Rigidbody>() != null)
                            {
                                match.GetComponent<Rigidbody>().detectCollisions = true;
                                match.GetComponent<Rigidbody>().isKinematic = false;


                                //Apply the manipulation
                                switch (physicsInteraction.Type)
                                {
                                    case MPhysicsInteractionType.AddForce:
                                        match.GetComponent<Rigidbody>().AddForce(physicsInteraction.Values.ToVector3(), ForceMode.Impulse);
                                        break;

                                    case MPhysicsInteractionType.AddTorque:
                                        match.GetComponent<Rigidbody>().AddTorque(physicsInteraction.Values.ToVector3(), ForceMode.Impulse);
                                        break;

                                    case MPhysicsInteractionType.ChangeAngularVelocity:
                                        match.GetComponent<Rigidbody>().angularVelocity = physicsInteraction.Values.ToVector3();
                                        match.MSceneObject.PhysicsProperties.AngularVelocity = physicsInteraction.Values;
                                        break;

                                    case MPhysicsInteractionType.ChangeCenterOfMass:
                                        match.GetComponent<Rigidbody>().centerOfMass = physicsInteraction.Values.ToVector3();
                                        match.MSceneObject.PhysicsProperties.CenterOfMass = physicsInteraction.Values;

                                        break;

                                    case MPhysicsInteractionType.ChangeInertia:
                                        match.GetComponent<Rigidbody>().inertiaTensor = physicsInteraction.Values.ToVector3();
                                        match.MSceneObject.PhysicsProperties.Inertia = physicsInteraction.Values;

                                        break;

                                    case MPhysicsInteractionType.ChangeMass:
                                        match.GetComponent<Rigidbody>().mass = (float)physicsInteraction.Values[0];
                                        match.MSceneObject.PhysicsProperties.Mass = physicsInteraction.Values[0];

                                        break;

                                    case MPhysicsInteractionType.ChangeVelocity:
                                        match.GetComponent<Rigidbody>().velocity = physicsInteraction.Values.ToVector3();
                                        match.MSceneObject.PhysicsProperties.Velocity = physicsInteraction.Values;
                                        break;
                                }
                            }
                        }
                    }
                }

                //Incorporate the properties
                if (sceneManipulation.Properties != null)
                {
                    //Handle all manipulations of the properties
                    foreach (MPropertyManipulation propertyUpdate in sceneManipulation.Properties)
                    {
                        //Find the matching object : To do -> performance can be optimized in future (e.g. dictionary)
                        MMISceneObject match = this.transform.GetComponentsInChildren<MMISceneObject>().First(s => s.MSceneObject.Transform.ID == propertyUpdate.Target);

                        //Apply the changes if a scene object can be found
                        if (match != null)
                        {
                            if (match.MSceneObject.Properties == null)
                                match.MSceneObject.Properties = new Dictionary<string, string>();

                            if (!match.MSceneObject.Properties.ContainsKey(propertyUpdate.Key))
                                match.MSceneObject.Properties[propertyUpdate.Key] = propertyUpdate.Value;
                            else
                                match.MSceneObject.Properties.Add(propertyUpdate.Key, propertyUpdate.Value);
                        }
                    }
                }
            }

            //Release the mutex
            sceneMutex.ReleaseMutex();

            return new MBoolResponse(true);
        }


        #region methods for updating the scene from unity -> results are not written back to the UnityScene -> just for synchronization

        /// <summary>
        /// Method to add a scene object
        /// </summary>
        /// <param name="sceneObject"></param>
        public static void AddAvatar(MAvatar avatar)
        {
            //Acquire the mutex
            sceneMutex.WaitOne();

            if (Instance.SceneUpdate.AddedAvatars == null)
                Instance.SceneUpdate.AddedAvatars = new List<MAvatar>();

            Instance.SceneUpdate.AddedAvatars.Add(avatar);


            //Add the avatar to id dictionary
            if (!Instance.avatarsByID.ContainsKey(avatar.ID))
            {
                //Add either a clone or the original one
                Instance.avatarsByID.Add(avatar.ID, avatar.Clone());
            }

            //Add name <-> id mapping
            if (!Instance.nameIdMappingAvatars.ContainsKey(avatar.Name))
                Instance.nameIdMappingAvatars.Add(avatar.Name, new List<string>() { avatar.ID });

            else
            {
                if (!Instance.nameIdMappingAvatars.ContainsKey(avatar.ID))
                    Instance.nameIdMappingAvatars.Add(avatar.ID, new List<string>());   

                Instance.nameIdMappingAvatars[avatar.ID].Add(avatar.ID);
            }

            sceneMutex.ReleaseMutex();


        }


        /// <summary>
        /// Method to add a scene object
        /// </summary>
        /// <param name="sceneObject"></param>
        public static void AddSceneObject(MSceneObject sceneObject)
        {
            //Acquire the mutex
            sceneMutex.WaitOne();

            if (Instance.SceneUpdate.AddedSceneObjects == null)
                Instance.SceneUpdate.AddedSceneObjects = new List<MSceneObject>();

            Instance.SceneUpdate.AddedSceneObjects.Add(sceneObject);

            //Add the scene object to id dictionary
            if (!Instance.sceneObjectsByID.ContainsKey(sceneObject.ID))
            {
                //Add either a clone or the original one
                Instance.sceneObjectsByID.Add(sceneObject.ID, sceneObject);
            }

            //Add name <-> id mapping
            if (!Instance.nameIdMappingSceneObjects.ContainsKey(sceneObject.Name))
                Instance.nameIdMappingSceneObjects.Add(sceneObject.Name, new List<string>() { sceneObject.ID });

            else
            {
                //To do check if list already contains the id
                Instance.nameIdMappingSceneObjects[sceneObject.Name].Add(sceneObject.ID);
            }

            sceneMutex.ReleaseMutex();
        }


        /// <summary>
        /// Method to signalize changed physics properties
        /// </summary>
        /// <param name="sceneObject"></param>
        /// <param name="physicsProperties"></param>
        public static void PhysicsPropertiesChanged(MSceneObject sceneObject, MPhysicsProperties physicsProperties)
        {
            if (Instance.SceneUpdate.ChangedSceneObjects == null)
                Instance.SceneUpdate.ChangedSceneObjects = new List<MSceneObjectUpdate>();

            Instance.SceneUpdate.ChangedSceneObjects.Add(new MSceneObjectUpdate()
            {
                ID = sceneObject.ID,
                PhysicsProperties = physicsProperties
            });

        }

        /// <summary>
        /// Method to signalize transformation cahnges of a scene object
        /// </summary>
        /// <param name="sceneObject"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        public static void TransformationChanged(MSceneObject sceneObject, MVector3 position, MQuaternion rotation, string parent)
        {
            if (Instance.SceneUpdate.ChangedSceneObjects == null)
                Instance.SceneUpdate.ChangedSceneObjects = new List<MSceneObjectUpdate>();

            //To do check for contradicting transforms in one frame

            //To do optimize data size
            Instance.SceneUpdate.ChangedSceneObjects.Add(new MSceneObjectUpdate()
            {
                ID = sceneObject.ID,
                Transform = new MTransformUpdate()
                {
                    Parent = parent,
                    Position = position.GetValues(),
                    Rotation = rotation.GetValues()
                }
            });

        }

        public static void PostureValuesChanged(MAvatar avatar, MAvatarPostureValues postureValues)
        {
            if (Instance.SceneUpdate.ChangedAvatars == null)
                Instance.SceneUpdate.ChangedAvatars = new List<MAvatarUpdate>();

            //To do optimize data size in future
            MAvatarUpdate avatarUpdate = Instance.SceneUpdate.ChangedAvatars.Find(s => s.ID == avatar.ID);

            //Check if already available (if not add the avatar update9
            if (avatarUpdate == null)
            {
                Instance.SceneUpdate.ChangedAvatars.Add(new MAvatarUpdate()
                {
                    ID = avatar.ID,
                    PostureValues = postureValues
                });
            }
            //Otherweise update the posture values
            else
                avatarUpdate.PostureValues = postureValues;
        }

        public static void AvatarChanged(MAvatar avatar)
        {
            if (Instance.SceneUpdate.ChangedAvatars == null)
                Instance.SceneUpdate.ChangedAvatars = new List<MAvatarUpdate>();


            //Find the MAvatarUpdate if already available
            MAvatarUpdate match = Instance.SceneUpdate.ChangedAvatars.Find(s => s.ID == avatar.ID);

            if (match == null)
            {
                //To do optimize data size
                Instance.SceneUpdate.ChangedAvatars.Add(new MAvatarUpdate()
                {
                    ID = avatar.ID,
                    PostureValues = avatar.PostureValues,
                    Description = avatar.Description,
                    SceneObjects = avatar.SceneObjects,
                    Properties = new List<MPropertyUpdate>()
                });
            }

            else
            {
                match.PostureValues = avatar.PostureValues;
                match.Description = avatar.Description;
                match.SceneObjects = avatar.SceneObjects;
                match.Properties = new List<MPropertyUpdate>();
            }

        }


        public static void SceneObjectChanged(MSceneObject sceneObject)
        {
            if (Instance.SceneUpdate.ChangedSceneObjects == null)
                Instance.SceneUpdate.ChangedSceneObjects = new List<MSceneObjectUpdate>();

            //To do optimize data size
            Instance.SceneUpdate.ChangedSceneObjects.Add(new MSceneObjectUpdate()
            {
                ID = sceneObject.ID,
                Transform = new MTransformUpdate()
                {
                    Parent = sceneObject.Transform.Parent,
                    Position = sceneObject.Transform.Position.GetValues(),
                    Rotation = sceneObject.Transform.Rotation.GetValues()
                },
                Collider = sceneObject.Collider,
                Mesh = sceneObject.Mesh,
                PhysicsProperties = sceneObject.PhysicsProperties,
                //To do -> Incorporate only the deltas
                Properties = new List<MPropertyUpdate>()
            });
        }

        /// <summary>
        /// Method to remove a scene object
        /// </summary>
        /// <param name="sceneObject"></param>
        public static void RemoveSceneObject(MSceneObject sceneObject)
        {
            if (Instance.SceneUpdate.RemovedSceneObjects == null)
                Instance.SceneUpdate.RemovedSceneObjects = new List<string>();

            Instance.SceneUpdate.RemovedSceneObjects.Add(sceneObject.ID);
        }

        /// <summary>
        /// Clears all scene changes that are currently buffered
        /// </summary>
        public static void ClearChanges()
        {
            Instance.SceneUpdate = new MSceneUpdate();
        }

        #endregion




        #region basic interface methods


        /// <summary>
        /// Returns the changes / scene manipulations of the last frame
        /// </summary>
        /// <returns></returns>
        public virtual MSceneUpdate GetSceneChanges()
        {
            return this.SceneUpdate;
        }


        /// <summary>
        /// Returns the full scene in form of a list of scene manipulations
        /// </summary>
        /// <returns></returns>
        public virtual MSceneUpdate GetFullScene()
        {
            MSceneUpdate sceneUpdate = new MSceneUpdate()
            {
                AddedSceneObjects = this.GetSceneObjects(),
                AddedAvatars = this.GetAvatars()
            };

            return sceneUpdate;
        }


        /// <summary>
        /// Returns all colliders
        /// </summary>
        /// <returns></returns>
        public virtual List<MCollider> GetColliders()
        {
            return this.sceneObjectsByID.Values.Select(s => s.Collider).ToList();
        }


        /// <summary>
        /// Returns the scene update of a particular frame
        /// </summary>
        /// <param name="frameID"></param>
        /// <returns></returns>
        public virtual MSceneUpdate GetSceneUpdate(int frameID)
        {
            Tuple<int,MSceneUpdate> match = this.SceneHistory.ToList().Find(s => s.Item1 == frameID);

            //Return the matching element if available
            if (match != null)
                return match.Item2;

            //By default return an empty scene update
            return new MSceneUpdate();
        }


        /// <summary>
        /// Returns the mesh by a given name (if available)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual MMesh GetMesh(string name)
        {
            //Gather the first id
            string id = this.nameIdMappingSceneObjects[name][0];

            return this.sceneObjectsByID[id].Mesh;
        }


        /// <summary>
        /// Returns the scene object by a given name (if available)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual MSceneObject GetSceneObject(string name)
        {
            //Gather the first id
            string id = this.nameIdMappingSceneObjects[name][0];

            return this.sceneObjectsByID[id];
        }


        /// <summary>
        /// Returns the scene object based on the id (if available)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual MSceneObject GetSceneObjectByID(string id)
        {
            return this.sceneObjectsByID[id];
        }


        /// <summary>
        /// Returns a list of all scene objects
        /// </summary>
        /// <returns></returns>
        public virtual List<MSceneObject> GetSceneObjects()
        {
            return this.sceneObjectsByID.Values.ToList();
        }


        /// <summary>
        /// Returns the transform by name (if available)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual MTransform GetTransform(string name)
        {
            //Gather the first id
            string id = this.nameIdMappingSceneObjects[name][0];

            return this.sceneObjectsByID[id].Transform;
        }

        
        /// <summary>
        /// Returns all transforms in the scene
        /// </summary>
        /// <returns></returns>
        public virtual List<MTransform> GetTransforms()
        {
            return this.sceneObjectsByID.Values.Select(s => s.Transform).ToList();
        }

        /// <summary>
        /// Returns a scene object by name (if available)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MSceneObject GetSceneObjectByName(string name)
        {
            return this.GetSceneObjectByID(this.nameIdMappingSceneObjects[name][0]);
        }


        /// <summary>
        /// Returns all scene objects in a particular range
        /// </summary>
        /// <param name="position"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<MSceneObject> GetSceneObjectsInRange(MVector3 position, double range)
        {
            List<MSceneObject> resultList = new List<MSceneObject>();

            foreach(MSceneObject sceneObject in this.GetSceneObjects())
            {
                //Check if in range
                if(sceneObject.Transform.Position.Subtract(position).Magnitude() <= range)
                    resultList.Add(sceneObject);
            }

            return resultList;
        }

        /// <summary>
        /// Returns the collider based on a given id (if available)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MCollider GetColliderById(string id)
        {
            if(this.sceneObjectsByID.ContainsKey(id))
                return this.sceneObjectsByID[id].Collider;
            return null;
        }

        /// <summary>
        /// Returns all colliders in range
        /// </summary>
        /// <param name="position"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<MCollider> GetCollidersInRange(MVector3 position, double range)
        {
            List<MCollider> result = new List<MCollider>();
            foreach (MSceneObject sceneObject in this.GetSceneObjectsInRange(position, range))
            {
                if (sceneObject.Collider != null)
                    result.Add(sceneObject.Collider);
            }

            return result;
        }

        /// <summary>
        /// Returns all meshes
        /// </summary>
        /// <returns></returns>
        public List<MMesh> GetMeshes()
        {
            return this.sceneObjectsByID.Values.Select(s => s.Mesh).ToList();
        }

        /// <summary>
        /// Returns a mesh defined by the id (if available)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MMesh GetMeshByID(string id)
        {
            if(this.sceneObjectsByID.ContainsKey(id))
                return this.sceneObjectsByID[id].Mesh;
            return null;
        }

        /// <summary>
        /// Returns the transform by id (if available)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MTransform GetTransformByID(string id)
        {
            if (this.sceneObjectsByID.ContainsKey(id))
                return this.sceneObjectsByID[id].Transform;

            return null;
        }

        /// <summary>
        /// Returns all avatars in the scene
        /// </summary>
        /// <returns></returns>
        public List<MAvatar> GetAvatars()
        {
            return this.avatarsByID.Values.ToList();
        }

        /// <summary>
        /// Returns an avatar by id (if available)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MAvatar GetAvatarByID(string id)
        {
            if(this.avatarsByID.ContainsKey(id))
                return this.avatarsByID[id];
            return null;
        }

        /// <summary>
        /// Returns an avatar by a given name (if available)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MAvatar GetAvatarByName(string name)
        {
            if (nameIdMappingAvatars.ContainsKey(name))
            {
                List<string> ids = nameIdMappingAvatars[name];

                if(ids.Count >0)
                    return this.GetAvatarByID(ids[0]);
            }

            return null;
        }

        /// <summary>
        /// Returns the avatars in range of the specified position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public List<MAvatar> GetAvatarsInRange(MVector3 position, double distance)
        {
            List<MAvatar> result = new List<MAvatar>();

            foreach (MAvatar avatar in this.GetAvatars())
            {
                MVector3 avatarPosition = new MVector3(avatar.PostureValues.PostureData[0], avatar.PostureValues.PostureData[1], avatar.PostureValues.PostureData[2]);

                if (avatarPosition.Subtract(position).Magnitude() <= distance)
                    result.Add(avatar);
            }
            return result;
        }


        /// <summary>
        /// Returns the current simulation time
        /// </summary>
        /// <returns></returns>
        public double GetSimulationTime()
        {
            return this.SimulationTime;
        }

        /// <summary>
        /// Disposes the service
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public MBoolResponse Dispose(Dictionary<string, string> properties)
        {
            this.remoteSceneManipulationServer?.Dispose();
            this.remoteSceneAccessServer?.Dispose();
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Restarts the scene service -> Nothing to do in here
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public MBoolResponse Restart(Dictionary<string, string> properties)
        {
            return new MBoolResponse(false);
        }


        /// <summary>
        /// Returns a navigation mesh of the scene
        /// </summary>
        /// <returns></returns>
        public MNavigationMesh GetNavigationMesh()
        {
            Debug.LogError("GetNavigationMesh executed altough being not implemented yet");

            //Currently not implemented -> to do
            return new MNavigationMesh();
        }

        /// <summary>
        /// Returns the scene in a specific format (E.g. gltf) -> To do
        /// </summary>
        /// <param name="fileFormat"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public byte[] GetData(string fileFormat, string selection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the present status
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetStatus()
        {
            return new Dictionary<string, string>()
            {
                { "Running", "True"}
            };
        }

        /// <summary>
        /// Provides the description of the scene service
        /// </summary>
        /// <returns></returns>
        public MServiceDescription GetDescription()
        {
            return new MServiceDescription()
            {
                ID = System.Guid.NewGuid().ToString(),
                Name = "RemoteSceneAccess",
                Language = "Unity/C#"
            };
        }


        /// <summary>
        /// Returns all attachements
        /// </summary>
        /// <returns></returns>
        public List<MAttachment> GetAttachments()
        {
            List<MAttachment> attachments = new List<MAttachment>();

            foreach (MSceneObject sceneObject in this.GetSceneObjects())
            {
                attachments.AddRange(sceneObject.Attachments);
            }

            return attachments;
        }


        /// <summary>
        /// Returns the attachments by the given id (if available)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<MAttachment> GetAttachmentsByID(string id)
        {
            if (this.sceneObjectsByID.ContainsKey(id))
            {
                return this.GetSceneObjectByID(id).Attachments;
            }

            return new List<MAttachment>();
        }


        /// <summary>
        /// Returns the attachmetns by the given name (if available)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<MAttachment> GetAttachmentsByName(string name)
        {
            if (this.nameIdMappingSceneObjects.ContainsKey(name))
            {
                return this.GetSceneObjectByName(name).Attachments;
            }
            return new List<MAttachment>();
        }


        /// <summary>
        ///  Returns all attachments of the children (recursively) including the one of the scene object specified by the id).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<MAttachment> GetAttachmentsChildrenRecursive(string id)
        {
            if (!this.sceneObjectsByID.ContainsKey(id))
                return new List<MAttachment>();

            List<MAttachment> attachments = new List<MAttachment>();


            MSceneObject current = this.GetSceneObjectByID(id);


            while (current != null)
            {
                if (current.Attachments != null && current.Attachments.Count > 0)
                {
                    attachments.AddRange(current.Attachments);
                }

                current = this.GetChild(current);
            }

            return attachments;
        }


        /// <summary>
        /// Returns all attachments of the parents (recursively) including the one of the scene object specified by the id).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<MAttachment> GetAttachmentsParentsRecursive(string id)
        {
            if (!this.sceneObjectsByID.ContainsKey(id))
                return new List<MAttachment>();

            List<MAttachment> attachments = new List<MAttachment>();

            MSceneObject current = this.GetSceneObjectByID(id);

            while (current.Transform.Parent != null && current.Transform.Parent != " ")
            {
                if (current.Attachments != null && current.Attachments.Count > 0)
                {
                    attachments.AddRange(current.Attachments);
                }

                current = this.GetSceneObjectByID(current.Transform.Parent);
            }

            return attachments;

        }

        /// <summary>
        /// Setup function defined in the MMIServiceBase interface
        /// </summary>
        /// <returns></returns>
        public MBoolResponse Setup(MAvatarDescription avatar, Dictionary<string, string> properties)
        {
            //Nothing to do in here
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Consume function defined in the MMIServiceBase interface
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Dictionary<string, string> Consume(Dictionary<string, string> properties)
        {
            //Nothing to do in here
            return new Dictionary<string, string>();
        }


        #endregion


        #region private methods


        /// <summary>
        /// Returns the child of the scene object (if available=
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private MSceneObject GetChild(MSceneObject sceneObject)
        {
            return this.GetSceneObjects().Find(s => s.Transform != null && s.Transform.Parent == sceneObject.ID);
        }


        /// <summary>
        /// Adds avatars to the internal scene representation
        /// </summary>
        /// <param name="avatars"></param>
        /// <param name="deepCopy"></param>
        private MBoolResponse AddAvatars(List<MAvatar> avatars)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each avatar
            foreach (MAvatar avatar in avatars)
            {
                if (this.avatarsByID.ContainsKey(avatar.ID))
                {
                    if (result.LogData == null)
                        result.LogData = new List<string>();

                    result.LogData.Add($"Cannot add avatar {avatar.Name}, object is already registered");
                    continue;
                }

                //Add the scene object to id dictionary
                if (!this.avatarsByID.ContainsKey(avatar.ID))
                {
                    //Add either a clone or the original one
                    this.avatarsByID.Add(avatar.ID, avatar.Clone());
                }

                //Add name <-> id mapping
                if (!this.nameIdMappingAvatars.ContainsKey(avatar.Name))
                    this.nameIdMappingAvatars.Add(avatar.Name, new List<string>() { avatar.ID });

                else
                {
                    //To do check if list already contains the id
                    this.nameIdMappingAvatars[avatar.ID].Add(avatar.ID);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds scene obnjects to the internal scene representation
        /// </summary>
        /// <param name="sceneObjects"></param>
        /// <param name="deepCopy"></param>
        /// <returns></returns>
        private MBoolResponse AddSceneObjects(List<MSceneObject> sceneObjects)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each scene object
            foreach (MSceneObject sceneObject in sceneObjects)
            {
                if (this.sceneObjectsByID.ContainsKey(sceneObject.ID))
                {
                    if (result.LogData == null)
                        result.LogData = new List<string>();

                    result.LogData.Add($"Cannot add scene object {sceneObject.Name}, object is already registered");

                    continue;
                }


                //Add the scene object to id dictionary
                if (!sceneObjectsByID.ContainsKey(sceneObject.ID))
                {
                    //Add either a clone or the original one
                    this.sceneObjectsByID.Add(sceneObject.ID, sceneObject.Clone());
                }

                //Add name <-> id mapping
                if (!this.nameIdMappingSceneObjects.ContainsKey(sceneObject.Name))
                    this.nameIdMappingSceneObjects.Add(sceneObject.Name, new List<string>() { sceneObject.ID });

                else
                {
                    //To do check if list already contains the id
                    this.nameIdMappingSceneObjects[sceneObject.ID].Add(sceneObject.ID);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds scene obnjects to the internal scene representation
        /// </summary>
        /// <param name="sceneObjects"></param>
        /// <param name="deepCopy"></param>
        /// <returns></returns>
        private MBoolResponse UpdateSceneObjects(List<MSceneObjectUpdate> sceneObjects)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each scene object
            foreach (MSceneObjectUpdate sceneObjectUpdate in sceneObjects)
            {
                //Add the scene object to id dictionary
                if (!sceneObjectsByID.ContainsKey(sceneObjectUpdate.ID))
                {
                    //To do print Problem
                }

                if (this.sceneObjectsByID.ContainsKey(sceneObjectUpdate.ID))
                {

                    MSceneObject sceneObject = this.sceneObjectsByID[sceneObjectUpdate.ID];


                    //Update the transform
                    if (sceneObjectUpdate.Transform != null)
                    {
                        MTransformUpdate transformUpdate = sceneObjectUpdate.Transform;

                        if (transformUpdate.Position != null)
                            sceneObject.Transform.Position = transformUpdate.Position.ToMVector3();

                        if (transformUpdate.Rotation != null)
                            sceneObject.Transform.Rotation = transformUpdate.Rotation.ToMQuaternion();

                        if (transformUpdate.Parent != null)
                            sceneObject.Transform.Parent = transformUpdate.Parent;
                    }

                    if (sceneObjectUpdate.Mesh != null)
                        this.sceneObjectsByID[sceneObjectUpdate.ID].Mesh = sceneObjectUpdate.Mesh;

                    if (sceneObjectUpdate.Collider != null)
                        sceneObject.Collider = sceneObjectUpdate.Collider;

                    if (sceneObjectUpdate.PhysicsProperties != null)
                        sceneObject.PhysicsProperties = sceneObjectUpdate.PhysicsProperties;
                }
            }

            return result;
        }


        /// <summary>
        /// Adds avatars to the internal scene representation
        /// </summary>
        /// <param name="avatars"></param>
        /// <param name="deepCopy"></param>
        private MBoolResponse UpdateAvatars(List<MAvatarUpdate> avatars)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each avatar
            foreach (MAvatarUpdate avatarUpdate in avatars)
            {
                //Add the scene object to id dictionary
                if (!this.avatarsByID.ContainsKey(avatarUpdate.ID))
                {
                    //Print Problem
                }

                if (this.avatarsByID.ContainsKey(avatarUpdate.ID))
                {
                    if (avatarUpdate.Description != null)
                        this.avatarsByID[avatarUpdate.ID].Description = avatarUpdate.Description;

                    if (avatarUpdate.PostureValues != null)
                        this.avatarsByID[avatarUpdate.ID].PostureValues = avatarUpdate.PostureValues;

                    if (avatarUpdate.SceneObjects != null)
                        this.avatarsByID[avatarUpdate.ID].SceneObjects = avatarUpdate.SceneObjects;

                    //To do update the name <-> id mapping
                }

            }

            return result;
        }


        /// <summary>
        /// Removes avatars from the scene by given ids
        /// </summary>
        /// <param name="avatarIDs"></param>
        /// <returns></returns>
        private MBoolResponse RemoveAvatars(List<string> avatarIDs)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each scene object
            foreach (string id in avatarIDs)
            {
                //Find the object
                if (avatarsByID.ContainsKey(id))
                {
                    string avatarName = avatarsByID[id].Name;

                    //Remove the mapping
                    if (nameIdMappingAvatars.ContainsKey(avatarName))
                    {
                        nameIdMappingAvatars[avatarName].Remove(id);
                    }

                    //Remove the scene object from the dictionary
                    avatarsByID.Remove(id);
                }
            }
            return result;
        }


        /// <summary>
        /// Removes scene objects from the scene by given ids
        /// </summary>
        /// <param name="sceneObjectIDs"></param>
        /// <returns></returns>
        private MBoolResponse RemoveSceneObjects(List<string> sceneObjectIDs)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each scene object
            foreach (string id in sceneObjectIDs)
            {
                //Find the object
                if (sceneObjectsByID.ContainsKey(id))
                {
                    string sceneObjectName = sceneObjectsByID[id].Name;

                    //Remove the mapping
                    if (nameIdMappingSceneObjects.ContainsKey(sceneObjectName))
                    {
                        nameIdMappingSceneObjects[sceneObjectName].Remove(id);
                    }

                    //Remove the scene object from the dictionary
                    sceneObjectsByID.Remove(id);
                }
            }
            return result;
        }





        #endregion

    }
}
