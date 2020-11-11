// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Janis Sprenger

using System.Collections.Generic;
using UnityEngine;
using MMIStandard;
using MMICSharp.Common;
using MMIUnity.Retargeting;


namespace MMIUnity
{

    /// <summary>
    /// Base class for a Unity based character which simplifies the overall integration in the MMI Framework
    /// </summary>
    public class UnityAvatarBase : MonoBehaviour
    {
        #region public fields

        /// <summary>
        /// The joints of the avatar
        /// </summary>
        public List<Transform> Joints = new List<Transform>();

        /// <summary>
        /// The root transform
        /// </summary>
        public Transform RootTransform;

        /// <summary>
        /// The root bone
        /// </summary>
        public Transform Pelvis;

        /// <summary>
        /// Class for visualizing the intermediate skeleton
        /// </summary>
        [HideInInspector]
        public SkeletonVisualization skelVis;

        [HideInInspector]
        public bool UseVirtualRoot = true;

        /// <summary>
        /// Flag specifies whether the skeleton visualization is provided
        /// </summary>
        public bool UseSkeletonVisualization = false;

        /// <summary>
        /// Prefab for visualizing the individual joints
        /// </summary>
        public GameObject gameJointPrefab;

        /// <summary>
        /// The assigned animator
        /// </summary>
        [HideInInspector]        
        public Animator animatorReference;

        /// <summary>
        /// The path of the configuration file
        /// </summary>
        public string ConfigurationFilePath = "configurations/avatar.mos";

        #region bone mapping

        public Dictionary<string, MJointType> bonenameMap;

        public static Dictionary<MJointType, HumanBodyBones> humanbonemap = new Dictionary<MJointType, HumanBodyBones>
        {

            {MJointType.PelvisCentre, HumanBodyBones.Hips },
            {MJointType.RightHip, HumanBodyBones.RightUpperLeg },
            {MJointType.RightKnee, HumanBodyBones.RightLowerLeg},
            {MJointType.RightAnkle, HumanBodyBones.RightFoot },
            {MJointType.RightBall, HumanBodyBones.RightToes },

            {MJointType.RightShoulder, HumanBodyBones.RightUpperArm },
            {MJointType.RightElbow, HumanBodyBones.RightLowerArm },
            {MJointType.RightWrist, HumanBodyBones.RightHand },

            {MJointType.LeftHip, HumanBodyBones.LeftUpperLeg },
            {MJointType.LeftKnee, HumanBodyBones.LeftLowerLeg},
            {MJointType.LeftAnkle, HumanBodyBones.LeftFoot },
            {MJointType.LeftBall, HumanBodyBones.LeftToes },

            {MJointType.LeftShoulder, HumanBodyBones.LeftUpperArm },
            {MJointType.LeftElbow, HumanBodyBones.LeftLowerArm },
            {MJointType.LeftWrist, HumanBodyBones.LeftHand },

                // Interpolation of spine joints was not bi-directional stable. Hence, reference joints are set manually. 
                // This has to be updated in the future, as it will not generalize to different target avatars. 
            {MJointType.S1L5Joint, HumanBodyBones.Spine },
            {MJointType.T12L1Joint, HumanBodyBones.Chest },
            {MJointType.T1T2Joint,  HumanBodyBones.UpperChest},
            {MJointType.C4C5Joint, HumanBodyBones.Neck},
            {MJointType.HeadJoint, HumanBodyBones.Head },

             //-----------------------------
             //finger joints

            {MJointType.RightIndexProximal, HumanBodyBones.RightIndexProximal },
            {MJointType.RightIndexMeta, HumanBodyBones.RightIndexIntermediate },
            {MJointType.RightIndexDistal, HumanBodyBones.RightIndexDistal },


            {MJointType.RightMiddleProximal, HumanBodyBones.RightMiddleProximal },
            {MJointType.RightMiddleMeta, HumanBodyBones.RightMiddleIntermediate },
            {MJointType.RightMiddleDistal, HumanBodyBones.RightMiddleDistal },

            {MJointType.RightRingProximal, HumanBodyBones.RightRingProximal },
            {MJointType.RightRingMeta, HumanBodyBones.RightRingIntermediate },
            {MJointType.RightRingDistal, HumanBodyBones.RightRingDistal },

            {MJointType.RightLittleProximal, HumanBodyBones.RightLittleProximal },
            {MJointType.RightLittleMeta, HumanBodyBones.RightLittleIntermediate },
            {MJointType.RightLittleDistal, HumanBodyBones.RightLittleDistal },

            {MJointType.RightThumbMid, HumanBodyBones.RightThumbProximal },
            {MJointType.RightThumbMeta, HumanBodyBones.RightThumbIntermediate },
            {MJointType.RightThumbCarpal, HumanBodyBones.RightThumbDistal },



            {MJointType.LeftIndexProximal, HumanBodyBones.LeftIndexProximal },
            {MJointType.LeftIndexMeta, HumanBodyBones.LeftIndexIntermediate },
            {MJointType.LeftIndexDistal, HumanBodyBones.LeftIndexDistal },

            {MJointType.LeftMiddleProximal, HumanBodyBones.LeftMiddleProximal },
            {MJointType.LeftMiddleMeta, HumanBodyBones.LeftMiddleIntermediate},
            {MJointType.LeftMiddleDistal, HumanBodyBones.LeftMiddleDistal },

            {MJointType.LeftRingProximal, HumanBodyBones.LeftRingProximal },
            {MJointType.LeftRingMeta, HumanBodyBones.LeftRingIntermediate },
            {MJointType.LeftRingDistal, HumanBodyBones.LeftRingDistal },

            {MJointType.LeftLittleProximal, HumanBodyBones.LeftLittleProximal },
            {MJointType.LeftLittleMeta, HumanBodyBones.LeftLittleIntermediate },
            {MJointType.LeftLittleDistal, HumanBodyBones.LeftLittleDistal },


            {MJointType.LeftThumbMid, HumanBodyBones.LeftThumbProximal },
            {MJointType.LeftThumbMeta, HumanBodyBones.LeftThumbIntermediate },
            {MJointType.LeftThumbCarpal, HumanBodyBones.LeftThumbDistal }
        };

        #endregion

        #endregion


        /// <summary>
        /// The employed retargeting service
        /// </summary>
        protected RetargetingService retargetingService = new RetargetingService(0);

        /// <summary>
        /// The assigned avatar id
        /// </summary>
        protected string AvatarID;

        /// <summary>
        /// The utilized skeleton access/intermediate skeleton
        /// </summary>
        private IntermediateSkeleton skeleton;

        /// <summary>
        /// Returns the instance of the retargeting service
        /// </summary>
        /// <returns></returns>
        public RetargetingService GetRetargetingService()
        {
            return retargetingService;
        }

        /// <summary>
        /// Returns the skeleton access
        /// </summary>
        /// <returns></returns>
        public MSkeletonAccess.Iface GetSkeletonAccess()
        {
            return this.skeleton;
        }

        /// <summary>
        /// Specifies whether the root transform is automatically computed
        /// </summary>
        // public bool AutoComputeRootTransform = true;


        /// <summary>
        /// Basic awake routine which can be overwritten
        /// </summary>
        protected virtual void Awake()
        {

        }

        /// <summary>
        /// Basic start routine which can be overwritten
        /// </summary>
        protected virtual void Start()
        {

        }


        /// <summary>
        /// Generates a global posture containing the list of joints with position and rotation in global space
        /// Required to pass on to the retargeting service. 
        /// </summary>
        /// <returns></returns>
        public virtual MAvatarPosture GenerateGlobalPosture()
        {
            List<MJoint> Joints = new List<MJoint>();
            
            if(this.UseVirtualRoot)
            {
                Joints = new List<MJoint>() { new MJoint("_VirtualRoot", MJointType.Root, this.RootTransform.position.ToMVector3(), this.RootTransform.rotation.ToMQuaternion()) };
            }
            //Create a new posture
            MAvatarPosture globalPosture = new MAvatarPosture
            {
                AvatarID = this.AvatarID,
                Joints = Joints
            };


            //Determine the joint values and write it into the posture
            this.Pelvis.GenerateGlobalJoints(this.bonenameMap, globalPosture.Joints);

            //Return the generated posture
            return globalPosture;
        }




        /// <summary>
        /// Applies the transform manipulations to be reflected in the intermediate skeleton
        /// </summary>
        public virtual void ApplyTransformManipulations()
        {
            //Update the intermediate skeleton with the new values

            //Get the global posture
            MAvatarPosture globalPosture = this.GenerateGlobalPosture();

            //Perform a retargeting to the intermediate skeleton
            MAvatarPostureValues intermediatePostureValues = retargetingService.RetargetToIntermediate(globalPosture);

            //Apply the posture values
            this.skeleton.SetChannelData(intermediatePostureValues);
        }


        /// <summary>
        /// Method sets up the retargeting of the specified avatar using the defined configuration file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual MAvatarPosture SetupRetargeting(string id)
        {
            MAvatarPosture p = null;
            if (System.IO.File.Exists(this.ConfigurationFilePath))
            {
                string s = System.IO.File.ReadAllText(this.ConfigurationFilePath);

                p = MMICSharp.Common.Communication.Serialization.FromJsonString<MAvatarPosture>(s); //JsonConvert.DeserializeObject<MAvatarPosture>(s);
                p.AvatarID = id;
            }

            return this.SetupRetargeting(id, p);
        }


        /// <summary>
        /// Method sets up the retargeting of the specified avaatar using an initial posture
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public virtual MAvatarPosture SetupRetargeting(string id, MAvatarPosture reference)
        {

            this.AvatarID = id;
            Animator anim = this.GetComponent<Animator>();
            this.animatorReference = anim;
            
            if(reference != null)
            {
                this.bonenameMap = new Dictionary<string, MJointType>();
                foreach(MJoint j in reference.Joints)
                {
                    if(! this.bonenameMap.ContainsKey(j.ID))
                        this.bonenameMap.Add(j.ID, j.Type);
                }
            }
            if (this.bonenameMap == null)
            {
                this.bonenameMap = new Dictionary<string, MJointType>();

                foreach (MJointType b in humanbonemap.Keys)
                {
                    bonenameMap.Add(anim.GetBoneTransform(humanbonemap[b]).name, b);
                }
            }

            MAvatarPosture p;
            if (reference != null)
            {
                retargetingService.SetupRetargeting(reference);
                skeleton = retargetingService.GetSkeleton();
                p = reference;
            } else {
                p = this.GenerateGlobalPosture();
                retargetingService.SetupRetargeting(p);
                skeleton = retargetingService.GetSkeleton();
            }
            
            


            //Create an empty joint object if not defined
            if (this.gameJointPrefab == null)
                this.gameJointPrefab = new GameObject("emptyJoint");

            if (UseSkeletonVisualization)
            {
                skelVis = new SkeletonVisualization(skeleton, retargetingService, this.Pelvis, this.bonenameMap, this.AvatarID, gameJointPrefab);
            }

            this.AssignPostureValues(this.GetPosture());
           
            return p;
    }


        /// <summary>
        /// Assigns the pose to the avatar.
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="assignLocalPositions">Specifies whether the local positions are assigned as well</param>
        public virtual void AssignPostureValues(MAvatarPostureValues pose, bool assignLocalPositions = true)
        {
            //Perform a retargeting to the specific skeleton using the retargeting service
            MAvatarPosture p = this.retargetingService.RetargetToTarget(pose);

            //Update the values of the skeletal representation
            skeleton.SetChannelData(pose);


            //Compute the root transformation if defined
            /*
            if (this.AutoComputeRootTransform)
            {
                MVector3 globalRootBonePosition = skeleton.GetRootPosition(this.AvatarID);
                MQuaternion globalRootBoneRotation = skeleton.GetRootRotation(this.AvatarID);


                //Compute the root transform
                this.RootTransform.position = new Vector3((float)globalRootBonePosition.X, this.RootTransform.position.y, (float)globalRootBonePosition.Z);
                //this.RootBone.transform.position = globalRootBonePosition.ToVector3();


                Vector3 currentEulerRoot = this.RootTransform.eulerAngles;
                this.RootTransform.rotation = Quaternion.Euler(currentEulerRoot.x, globalRootBoneRotation.ToQuaternion().eulerAngles.y - 90.0f, currentEulerRoot.z);
                //this.RootBone.transform.rotation = globalRootBoneRotation.ToQuaternion();

                MTransform CharTransform = new MTransform("tmp", this.RootTransform.position.ToMVector3(), this.RootTransform.rotation.ToMQuaternion());
            }
            */

            // Update root transform
            this.RootTransform.transform.position = p.Joints[0].Position.ToVector3();
            this.RootTransform.transform.rotation = p.Joints[0].Rotation.ToQuaternion();


            //Update the transforms/visualization by applying the global transformations
            this.Pelvis.ApplyGlobalJoints(p.Joints);

            //Update the skeleton visualization if enabled
            if(this.skelVis != null)
                this.skelVis.root.ApplyPostureValues();
            
        }


        /// <summary>
        /// Returns the pose of the avatar 
        /// Currently requires between 0.5ms and 1ms -> way too much
        /// </summary>
        /// <param name="setRootTransform"></param>
        /// <returns></returns>
        public virtual MAvatarPostureValues GetPosture()
        {
            MAvatarPostureValues vals = skeleton.GetCurrentPostureValues(this.AvatarID);
            return vals;
        }


        /// <summary>
        /// Returns the zero posture of the underlying avatar
        /// </summary>
        /// <returns></returns>
        public MAvatarPosture GetZeroPosture()
        {
            return this.retargetingService.GetSkeleton().GetAvatarDescription(this.AvatarID).ZeroPosture;
        }


        /// <summary>
        /// Returns the retargeted posture represented in the intermediate skeleton used in the MMI framework
        /// </summary>
        /// <returns></returns>
        public MAvatarPostureValues GetRetargetedPosture()
        {
            // Return retargeted posture from target Avatar to intermediate skeleton. 
            MAvatarPosture p = this.GenerateGlobalPosture();

            MAvatarPostureValues vals = this.retargetingService.RetargetToIntermediate(p);

            if (this.skelVis != null)
            {
                this.skelVis.AssignPostureValues();
            }

            return vals;
        }

    }
}
