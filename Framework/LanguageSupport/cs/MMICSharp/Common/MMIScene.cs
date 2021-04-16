// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MMICSharp.Common
{
    /// <summary>
    /// Class represents a (hyptothetical) scene which can be specifically set up by the developer
    /// </summary>
    public class MMIScene:  MSceneAccess.Iface
    {

        #region protected fields for storing the data

        /// <summary>
        /// Mapping between the name of a scene object and a unique id
        /// </summary>
        protected ConcurrentDictionary<string, List<string>> nameIdMappingSceneObjects = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Dictionary containing all scene objects structured by the specific id
        /// </summary>
        protected ConcurrentDictionary<string, MSceneObject> sceneObjectsByID = new ConcurrentDictionary<string, MSceneObject>();


        protected ConcurrentDictionary<string, MAvatar> avatarsByID = new ConcurrentDictionary<string, MAvatar>();


        protected ConcurrentDictionary<string, List<string>> nameIdMappingAvatars = new ConcurrentDictionary<string, List<string>>();


        /// <summary>
        /// The list of all scene events
        /// </summary>
        protected MSceneUpdate SceneUpdate = new MSceneUpdate();

        #endregion

        #region public fields

        /// <summary>
        /// A queue which contains the history of the last n applied scnee manipulations
        /// </summary>
        public Queue<Tuple<int, MSceneUpdate>> SceneHistory = new Queue<Tuple<int, MSceneUpdate>>();

        /// <summary>
        /// The current frame id
        /// </summary>
        public int FrameID = 0;

        /// <summary>
        /// Store the last n frames
        /// </summary>
        public int HistoryBufferSize = 20;

        /// <summary>
        /// The current simulation time
        /// </summary>
        public double SimulationTime = 0;

        #endregion


        /// <summary>
        /// Copies the full scene of the reference scene access
        /// </summary>
        /// <param name="original"></param>
        public virtual void CopyScene(MSceneAccess.Iface original)
        {
            this.Apply(original.GetFullScene(), true);
        }


        /// <summary>
        /// Clears the whole scene
        /// </summary>
        public virtual void Clear()
        {
            this.SceneHistory = new Queue<Tuple<int, MSceneUpdate>>();
            this.SceneUpdate = new MSceneUpdate();
            this.sceneObjectsByID = new ConcurrentDictionary<string, MSceneObject>();
            this.avatarsByID = new ConcurrentDictionary<string, MAvatar>();
            this.FrameID = 0;
            this.nameIdMappingAvatars = new ConcurrentDictionary<string, List<string>>();
            this.nameIdMappingSceneObjects = new ConcurrentDictionary<string, List<string>>();
        }


        /// <summary>
        /// Applies the scene manipulation on the scene
        /// </summary>
        /// <param name="sceneUpdates">The scene manipulations to be considered</param>
        /// <param name="deepCopy">Specifies whether the scene manipulations are directly applied or a deep copy is performed</param>
        public virtual MBoolResponse Apply(MSceneUpdate sceneUpdate, bool deepCopy = false)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Increment the frame id
            this.FrameID++;

            //Stores the history
            SceneHistory.Enqueue(new Tuple<int, MSceneUpdate>(FrameID, sceneUpdate));

            //Only allow the max buffer size
            while (SceneHistory.Count > this.HistoryBufferSize)
                this.SceneHistory.Dequeue();


            //Set the scene changes to the input of the present frame
            this.SceneUpdate = sceneUpdate;


            //Check if there are avatars to be added
            if(sceneUpdate.AddedAvatars?.Count>0)
                this.AddAvatars(sceneUpdate.AddedAvatars, deepCopy);             

            //Check if there are new scene objects which should be added
            if (sceneUpdate.AddedSceneObjects?.Count > 0)
                this.AddSceneObjects(sceneUpdate.AddedSceneObjects, deepCopy);
                
            //Check if there are changed avatars that need to be retransmitted
            if (sceneUpdate.ChangedAvatars?.Count > 0)
                this.UpdateAvatars(sceneUpdate.ChangedAvatars, deepCopy);

            //Check if there are changed sceneObjects that need to be retransmitted
            if (sceneUpdate.ChangedSceneObjects?.Count > 0)
                this.UpdateSceneObjects(sceneUpdate.ChangedSceneObjects, deepCopy);           

            //Check if there are avatars that need to be removed
            if (sceneUpdate.RemovedAvatars?.Count > 0)
                this.RemoveAvatars(sceneUpdate.RemovedAvatars, deepCopy);

            //Check if there are scene objects that need to be removed
            if (sceneUpdate.RemovedSceneObjects?.Count > 0)
                this.RemoveSceneObjects(sceneUpdate.RemovedSceneObjects,deepCopy);         
            

            return result;
        }




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
            return this.GetSceneObjects().Select(s => s.Collider).ToList();
        }


        /// <summary>
        /// Returns the scene update for the given frame
        /// </summary>
        /// <param name="frameID"></param>
        /// <returns></returns>
        public virtual MSceneUpdate GetSceneUpdate(int frameID)
        {
            var match = this.SceneHistory.ToList().Find(s => s.Item1 == frameID);

            if (match != null)
                return match.Item2;
            

            return new MSceneUpdate();
        }

        /// <summary>
        /// Returns the mesh given the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual MMesh GetMesh(string name)
        {
            if (this.nameIdMappingSceneObjects.ContainsKey(name))
            {
                //Gather the first id
                string id = this.nameIdMappingSceneObjects[name][0];

                return this.sceneObjectsByID[id].Mesh;
            }

            return null;
        }

        /// <summary>
        /// Returns the scene object given the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual MSceneObject GetSceneObject(string name)
        {
            if (this.nameIdMappingSceneObjects.ContainsKey(name))
            {
                //Gather the first id
                string id = this.nameIdMappingSceneObjects[name][0];

                return this.sceneObjectsByID[id];
            }

            return null;
        }

        /// <summary>
        /// Returns the scene object based on the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual MSceneObject GetSceneObjectByID(string id)
        {
            if(this.sceneObjectsByID.ContainsKey(id))
                return this.sceneObjectsByID[id];

            return null;
        }

        /// <summary>
        /// Returns all scene objects
        /// </summary>
        /// <returns></returns>
        public virtual List<MSceneObject> GetSceneObjects()
        {
            return this.sceneObjectsByID.Values.ToList();
        }

        /// <summary>
        /// Returns a transform by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual MTransform GetTransform(string name)
        {
            if (this.nameIdMappingSceneObjects.ContainsKey(name))
            {
                //Gather the first id
                string id = this.nameIdMappingSceneObjects[name][0];

                return this.sceneObjectsByID[id].Transform;
            }
            return null;
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
        /// Returns a scene object using the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MSceneObject GetSceneObjectByName(string name)
        {
            if(this.nameIdMappingSceneObjects.ContainsKey(name))
                return this.GetSceneObjectByID(this.nameIdMappingSceneObjects[name][0]);
            return null;
        }

        /// <summary>
        /// Returns all scene objects within the specified range
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public List<MSceneObject> GetSceneObjectsInRange(MVector3 position, double distance)
        {
            List<MSceneObject> result = new List<MSceneObject>();

            foreach (MSceneObject sceneObject in this.GetSceneObjects())
            {
                if (EuclideanDistance(sceneObject.Transform.Position, position) <= distance)
                    result.Add(sceneObject);
            }
            return result;
        }

        /// <summary>
        /// Returns the collider by the given id
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
        /// Returns all colliders within the specified range (if available)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<MCollider> GetCollidersInRange(MVector3 position, double range)
        {
            List<MCollider> result = new List<MCollider>();
            foreach(MSceneObject sceneObject in this.GetSceneObjectsInRange(position, range))
            {
                if (sceneObject.Collider != null)
                {
                    result.Add(sceneObject.Collider);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all meshes 
        /// </summary>
        /// <returns></returns>
        public List<MMesh> GetMeshes()
        {
            return this.GetSceneObjects().Select(s=>s.Mesh).ToList();
        }

        /// <summary>
        /// Returns the mesh by the given id
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
        /// Returns a transform by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MTransform GetTransformByID(string id)
        {
            if(this.sceneObjectsByID.ContainsKey(id))
                return this.sceneObjectsByID[id].Transform;

            return null;
        }

        /// <summary>
        /// Returns all avatars
        /// </summary>
        /// <returns></returns>
        public List<MAvatar> GetAvatars()
        {
            return this.avatarsByID.Values.ToList();
        }

        /// <summary>
        /// Returns an avatar based on the defined id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MAvatar GetAvatarByID(string id)
        {
            if(this.avatarsByID.ContainsKey(id))
                return this.avatarsByID[id];
            return null;
        }


        public MAvatar GetAvatarByName(string name)
        {
            if(this.nameIdMappingAvatars.ContainsKey(name))
                return this.GetAvatarByID(this.nameIdMappingAvatars[name][0]);
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

            foreach(MAvatar avatar in this.GetAvatars())
            {
                MVector3 avatarPosition = new MVector3(avatar.PostureValues.PostureData[0], avatar.PostureValues.PostureData[1], avatar.PostureValues.PostureData[2]);

                if (avatarPosition.Subtract(position).Magnitude() <= distance)
                    result.Add(avatar);
            }
            return result;
        }

        public virtual double GetSimulationTime()
        {
            return this.SimulationTime;
        }

        #region attachments

        public List<MAttachment> GetAttachments()
        {
            List<MAttachment> attachments = new List<MAttachment>();

            foreach(MSceneObject sceneObject in this.GetSceneObjects())
            {
                attachments.AddRange(sceneObject.Attachments);
            }

            return attachments;
        }

        public List<MAttachment> GetAttachmentsByID(string id)
        {
            if (this.sceneObjectsByID.ContainsKey(id))
                return this.GetSceneObjectByID(id).Attachments;

            return new List<MAttachment>();
        }

        public List<MAttachment> GetAttachmentsByName(string name)
        {
            if (this.nameIdMappingSceneObjects.ContainsKey(name))
                return this.GetSceneObjectByName(name).Attachments;

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

            while(current.Transform.Parent != null && current.Transform.Parent != " ")
            {
                if(current.Attachments!=null && current.Attachments.Count > 0)
                {
                    attachments.AddRange(current.Attachments);
                }

                current = this.GetSceneObjectByID(current.Transform.Parent);
            }

            return attachments;

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
        private MBoolResponse AddAvatars(List<MAvatar> avatars, bool deepCopy)
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
                    this.avatarsByID.TryAdd(avatar.ID, deepCopy ? avatar.Clone() : avatar);
                }

                //Add name <-> id mapping
                if (!this.nameIdMappingAvatars.ContainsKey(avatar.Name))
                    this.nameIdMappingAvatars.TryAdd(avatar.Name, new List<string>() { avatar.ID });

                else
                {
                    if (!this.nameIdMappingAvatars.ContainsKey(avatar.ID))
                        this.nameIdMappingAvatars.TryAdd(avatar.ID, new List<string>());

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
        private MBoolResponse AddSceneObjects(List<MSceneObject> sceneObjects, bool deepCopy)
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
                    this.sceneObjectsByID.TryAdd(sceneObject.ID, deepCopy ? sceneObject.Clone() : sceneObject);
                }

                //Add name <-> id mapping
                if (!this.nameIdMappingSceneObjects.ContainsKey(sceneObject.Name))
                    this.nameIdMappingSceneObjects.TryAdd(sceneObject.Name, new List<string>() { sceneObject.ID });

                else
                {
                    //To do check if list already contains the id
                    this.nameIdMappingSceneObjects[sceneObject.Name].Add(sceneObject.ID);
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
        private MBoolResponse UpdateSceneObjects(List<MSceneObjectUpdate> sceneObjects, bool deepCopy)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each scene object
            foreach (MSceneObjectUpdate sceneObjectUpdate in sceneObjects)
            {
                //Add the scene object to id dictionary
                if (!sceneObjectsByID.ContainsKey(sceneObjectUpdate.ID))
                {
                    if (result.LogData == null)
                        result.LogData = new List<string>();

                    result.LogData.Add($"Cannot update scene object {sceneObjectUpdate.ID}, object is not available");
                    continue;
                }

                if (this.sceneObjectsByID.ContainsKey(sceneObjectUpdate.ID))
                {

                    MSceneObject sceneObject = this.sceneObjectsByID[sceneObjectUpdate.ID];


                    //Update the transform
                    if (sceneObjectUpdate.Transform != null)
                    {
                        MTransformUpdate transformUpdate = sceneObjectUpdate.Transform;

                        if(transformUpdate.Position != null)
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
        private MBoolResponse UpdateAvatars(List<MAvatarUpdate> avatars, bool deepCopy)
        {
            MBoolResponse result = new MBoolResponse(true);

            //Iterate over each avatar
            foreach (MAvatarUpdate avatarUpdate in avatars)
            {
                //Add the scene object to id dictionary
                if (!this.avatarsByID.ContainsKey(avatarUpdate.ID))
                {
                    result.LogData.Add($"Cannot update avatar {avatarUpdate.ID}, object is not available");
                }

                if (this.avatarsByID.ContainsKey(avatarUpdate.ID))
                {
                    if(avatarUpdate.Description != null)
                        this.avatarsByID[avatarUpdate.ID].Description = avatarUpdate.Description;

                    if(avatarUpdate.PostureValues !=null)
                        this.avatarsByID[avatarUpdate.ID].PostureValues = avatarUpdate.PostureValues;

                    if (avatarUpdate.SceneObjects != null)
                        this.avatarsByID[avatarUpdate.ID].SceneObjects = avatarUpdate.SceneObjects;

                    //To do update the name <-> id mapping
                }

            }

            return result;
        }



        private MBoolResponse RemoveAvatars(List<string> avatarIDs, bool deepCopy)
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
                    avatarsByID.TryRemove(id, out MAvatar avatar);
                }
            }
            return result;
        }


        private MBoolResponse RemoveSceneObjects(List<string> sceneObjectIDs, bool deepCopy)
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
                    sceneObjectsByID.TryRemove(id, out MSceneObject sceneObject);
                }
            }
            return result;
        }







        /// <summary>
        /// Basic euclidean distance function for float arrays
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        private static float EuclideanDistance(float[] vector1, float[] vector2)
        {
            float sum = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                sum += (vector1[i] - vector2[i]) * (vector1[i] - vector2[i]);
            }

            return (float)Math.Sqrt(sum);
        }



        /// <summary>
        /// Basic euclidean distance function for float arrays
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        private static float EuclideanDistance(MVector3 vector1, MVector3 vector2)
        {
            MVector3 diff = vector1.Subtract(vector2);

            return diff.Magnitude();
        }

        private static float EuclideanDistance(List<double> vector1, float[] vector2)
        {
            float sum = 0;

            for (int i = 0; i < vector1.Count; i++)
            {
                sum += (float)((vector1[i] - vector2[i]) * (vector1[i] - vector2[i]));
            }

            return (float)Math.Sqrt(sum);
        }

        public virtual MNavigationMesh GetNavigationMesh()
        {
            throw new NotImplementedException();
        }

        public byte[] GetData(string fileFormat, string selection)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetStatus()
        {
            return new Dictionary<string, string>()
            {
                {"Running","true" }
            };
        }

        public MServiceDescription GetDescription()
        {
            return new MServiceDescription()
            {
                ID = System.Guid.NewGuid().ToString(),
                Name = "MMISceneAccess"
            };
        }

        public MBoolResponse Setup(MAvatarDescription avatar, Dictionary<string, string> properties)
        {
            return new MBoolResponse(true);
        }

        public Dictionary<string, string> Consume(Dictionary<string, string> properties)
        {
            throw new NotImplementedException();
        }

        public MBoolResponse Dispose(Dictionary<string, string> properties)
        {
            return new MBoolResponse(true);
        }

        public MBoolResponse Restart(Dictionary<string, string> properties)
        {
            throw new NotImplementedException();
        }



        #endregion

    }
}
