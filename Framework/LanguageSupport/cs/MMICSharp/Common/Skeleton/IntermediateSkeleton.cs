// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger

using MMIStandard;
using System.Collections.Generic;

namespace MMICSharp.Common
{
    /// <summary>
    /// Class for accessing the MOSIM intermediate skeleton
    /// </summary>
    public class IntermediateSkeleton : MSkeletonAccess.Iface
    {

        /// <summary>
        /// Dictionary holding the descriptions for all managed avatars for each avatar id
        /// </summary>
        private Dictionary<string, MAvatarDescription> avatarDescriptions = new Dictionary<string, MAvatarDescription>();
        
        /// <summary>
        /// Dictionary holding the last set posture values (set over SetChannelData) for each avatar id
        /// </summary>
        private Dictionary<string, MAvatarPostureValues> lastPostureValues = new Dictionary<string, MAvatarPostureValues>();
        
        /// <summary>
        /// Dictionary storing the skeleton hierarchies as a RJoint tree for each avatar id
        /// </summary>
        private Dictionary<string, RJoint> hierarchies = new Dictionary<string, RJoint>();

        /// <summary>
        /// Dictionary storing the animated joints for each avatar, if set globally (via SetAnimatedJoints)
        /// </summary>
        private Dictionary<string, List<MJointType>> animatedJoints = new Dictionary<string, List<MJointType>>();


        /// <summary>
        /// The description of the service
        /// </summary>
        private readonly MServiceDescription description = new MServiceDescription()
        {
            ID = System.Guid.NewGuid().ToString(),
            Name = "remoteSkeletonAccess"
        };


        public IntermediateSkeleton()
        {
        }

        /// <summary>
        /// Initializes the anthropometry from a given description. 
        /// Has to be performed prior to all other interactions with the 
        /// intermediate skeleton (e.g. query for joint positions, etc.).
        /// </summary>
        /// <param name="description"></param>
        public void InitializeAnthropometry(MAvatarDescription description)
        {
            this.avatarDescriptions[description.AvatarID] = description;

            //update joint offsets
            this.hierarchies[description.AvatarID] = RJoint.Initialize(description.ZeroPosture.Joints);

            List<double> zero_rotations = new List<double>();
            this.hierarchies[description.AvatarID].CreateZeroVector(zero_rotations);
            MAvatarPostureValues values = new MAvatarPostureValues(description.AvatarID, zero_rotations);
            this.lastPostureValues[description.AvatarID] = values;

            List<MJointType> animatedJoints = new List<MJointType>();
            foreach(MJoint j in description.ZeroPosture.Joints)
            {
                animatedJoints.Add(j.Type);
            }
            this.SetAnimatedJoints(description.AvatarID, animatedJoints);
        }

        /// <summary>
        /// Returns the avatar description given by the id (null if not available)
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MAvatarDescription GetAvatarDescription(string avatarID)
        {
            if(this.avatarDescriptions.ContainsKey(avatarID))
                return avatarDescriptions[avatarID];

            return null;
        }

        /// <summary>
        /// Sets the animated joints for a given avatar id.
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="joints"></param>
        public void SetAnimatedJoints(string avatarID, List<MJointType> joints)
        {
            this.animatedJoints[avatarID] = joints;
        }

        /// <summary>
        /// Sets the posture values. The AvatarID is contained within the values. Additionally, this function stores the provided posture values as the last posture values. 
        /// </summary>
        /// <param name="values"></param>
        public void SetChannelData(MAvatarPostureValues values)
        {
            this.hierarchies[values.AvatarID].SetAvatarPostureValues(values, this.animatedJoints[values.AvatarID]);
            this.lastPostureValues[values.AvatarID] = values;
        }

        /// <summary>
        /// Returns the last set avatar posture values. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MAvatarPostureValues GetLastPostureValues(string avatarID)
        {
            return this.lastPostureValues[avatarID];
        }


        /// <summary>
        /// Returns the current posture of the avatarID in a global MAvatarPosture representation. 
        /// (Each joint contains translation and rotation in global coordinate system)
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MAvatarPosture GetCurrentGlobalPosture(string avatarID)
        {
            MAvatarPosture globalPosture = new MAvatarPosture
            {
                AvatarID = avatarID,
                Joints = new List<MJoint>()
            };

            if (this.hierarchies.ContainsKey(avatarID))
            {
                MAvatarPosture zeroPosture = this.avatarDescriptions[avatarID].ZeroPosture;
                foreach (MJoint j in zeroPosture.Joints)
                {
                    Joint refJoint = this.hierarchies[avatarID].GetChild(j.Type);
                    MJoint globalJoint = new MJoint(j.ID, j.Type, refJoint.GetGlobalPosition(), refJoint.GetGlobalRotation());
                    globalPosture.Joints.Add(globalJoint);
                }
            }
            return globalPosture;
        }

        /// <summary>
        /// Returns the current posture values for the avatar by recomputing the values based on the internal hierarchy. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MAvatarPostureValues GetCurrentPostureValues(string avatarID)
        {
            RJoint root = this.hierarchies[avatarID];
            List<double> postureValues = root.GetAvatarPostureValues();
            MAvatarPostureValues values = new MAvatarPostureValues(avatarID, postureValues);
            return values;
            //return this.lastPostureValues[avatarID];
        }

        /// <summary>
        /// Returns partial MAvatarPostureValues for the defined joint list of the lastPostureValues.
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="joints"></param>
        /// <returns></returns>
        public MAvatarPostureValues GetCurrentPostureValuesPartial(string avatarID, List<MJointType> joints)
        {
            RJoint root = this.hierarchies[avatarID];
            List<double> postureValues = root.GetAvatarPostureValuesPartial(joints);
            MAvatarPostureValues values = new MAvatarPostureValues(avatarID, postureValues);
            return values;
        }



        /// <summary>
        /// Recomputes the present posture values for the given avatar id. 
        /// This function has to be called after any set-function call in order
        /// to get the MAvatarPosture for the current configuration. 
        /// If the posture is to be set permantently, SetChannelData has to be called
        /// with the generated MAvatarPostureValues
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MAvatarPostureValues RecomputeCurrentPostureValues(string avatarID)
        {
            MAvatarPostureValues ret = this.GetCurrentPostureValues(avatarID);
            ret.PostureData = this.hierarchies[avatarID].GetAvatarPostureValues();
            return ret;
        }


        /// <summary>
        /// Returns a list of positions for all joints denoting the current world positions 
        /// based on the current hierarchy. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public List<MVector3> GetCurrentJointPositions(string avatarID)
        {
            return this.hierarchies[avatarID].GetCurrentJointPositions();
        }

        /// <summary>
        /// Returns the root position of the avatar in world space. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MVector3 GetRootPosition(string avatarID)
        {
            return this.hierarchies[avatarID].GetGlobalPosition();
        }

        /// <summary>
        /// Returns the root rotation of the avatar in world space. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MQuaternion GetRootRotation(string avatarID)
        {
            return this.hierarchies[avatarID].GetGlobalRotation();
        }

        /// <summary>
        /// Returns the joint position in world space for a specific joint and avatar.
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="boneType">joint</param>
        /// <returns></returns>
        public MVector3 GetGlobalJointPosition(string avatarId, MJointType boneType)
        {
            return this.hierarchies[avatarId].GetChild(boneType).GetGlobalPosition();
        }

        /// <summary>
        /// Returns the joint rotation in world space for a specific joint and avatar.
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="boneType">joint</param>
        /// <returns></returns>
        public MQuaternion GetGlobalJointRotation(string avatarId, MJointType boneType)
        {
            return this.hierarchies[avatarId].GetChild(boneType).GetGlobalRotation();
        }


        /// <summary>
        /// Sets the local joint rotation of a specific joint in a specific avatar. 
        /// The rotation is to be set in a joint local space, meaning the space of the parent joint 
        /// after the offset translation and rotations are applied. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="boneType">joint</param>
        /// <param name="rotation"></param>
        public void SetLocalJointRotation(string avatarID, MJointType boneType, MQuaternion rotation)
        {
            this.hierarchies[avatarID].GetChild(boneType).SetLocalRotation(rotation);
        }

        /// <summary>
        /// Gets the local joint rotation of a specific joint in a specific avatar. 
        /// The rotation is in a joint local space, meaning the space of the parent joint 
        /// after the offset translation and rotations are applied. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="boneType">joint</param>
        /// <returns></returns>
        public MQuaternion GetLocalJointRotation(string avatarID, MJointType boneType)
        {
            return this.hierarchies[avatarID].GetChild(boneType).GetCurrentRotationValues();
        }

        /// <summary>
        /// Gets the local joint rotation of all joints in a specific avatar. 
        /// The rotation is in a joint local space, meaning the space of the parent joint 
        /// after the offset translation and rotations are applied. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public List<MQuaternion> GetLocalJointRotations(string avatarID)
        {
            List<MQuaternion> ret = new List<MQuaternion>();
            foreach(var joint in this.GetAvatarDescription(avatarID).ZeroPosture.Joints)
            {
                ret.Add(this.GetLocalJointRotation(avatarID, joint.Type));
            }
            return ret;
        }

        /// <summary>
        /// Returns the root joint object of a specific avatar. This is now native MSkeletonAccess function. 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public Joint GetRoot(string avatarID)
        {
            return this.hierarchies[avatarID];
        }


        /// <summary>
        /// Sets the joint position of a specific joint for an avatar in world space. 
        /// This function has to be used with caution, as it may break the skeleton.
        /// This function is not protected, meaning it sets the global position as well for joints,
        /// where there is not translation channel. 
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="joint"></param>
        /// <param name="position"></param>
        public void SetGlobalJointPosition(string avatarId, MJointType joint, MVector3 position)
        {
            this.hierarchies[avatarId].GetChild(joint).SetGlobalPosManually(position);
            this.RecomputeCurrentPostureValues(avatarId);
        }

        /// <summary>
        /// Sets the rotation of a specific joint of an avatar in world space. Child
        /// joints are not affected by this rotation. 
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="joint"></param>
        /// <param name="rotation"></param>
        public void SetGlobalJointRotation(string avatarId, MJointType joint, MQuaternion rotation)
        {
            this.hierarchies[avatarId].GetChild(joint).SetGlobalRotManually(rotation);
            this.RecomputeCurrentPostureValues(avatarId);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public MAvatarPosture GetCurrentLocalPosture(string avatarID)
        {
            throw new System.NotImplementedException();
        }
        
        /// <summary>
        /// Returns the position of a joint in a joint local space. The joint local space
        /// is the parent space after the transformation by offset position and rotation. 
        /// If the joint has no translation channels, the return will be a zero vector. 
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="joint"></param>
        /// <returns></returns>
        public MVector3 GetLocalJointPosition(string avatarId, MJointType joint)
        {
            return this.hierarchies[avatarId].GetChild(joint).GetLocalTranslation();
        }

        /// <summary>
        /// Sets the root position of an avatar. In addition, this function re-aligns the pelvis
        /// and the root joint. Meaning, the root joint is set to the target position and 
        /// the translation animation of the pelvis is removed. 
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="position"></param>
        public void SetRootPosition(string avatarId, MVector3 position)
        {
            MAvatarPostureValues values = this.GetCurrentPostureValues(avatarId);
            position = position.Subtract(this.hierarchies[avatarId].GetMJoint().Position);
            // Set root position
            values.PostureData[0] = position.X;
            values.PostureData[1] = position.Y;
            values.PostureData[2] = position.Z;

            // reset pelvis position to be positioned above the root
            values.PostureData[7] = 0;
            values.PostureData[8] = 0;
            values.PostureData[9] = 0;

            this.hierarchies[values.AvatarID].SetAvatarPostureValues(values, this.animatedJoints[values.AvatarID]);

            // I avoid using the SetChannelData here, to not overwrite the last set channel values. 
            // this.SetChannelData(values);
        }

        /// <summary>
        /// Sets the root rotation of an avatar. 
        /// In this function, the root joint is set to the target rotation and the rotation animation 
        /// of the pelvis is removed. 
        /// 
        /// TODO: This has to be improved, as all animation of the pelvis is removed. As the roots forward direction (z-axis)
        /// should point to the front of the avatar, only this part of the rotation should be removed from the pelvis. 
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="rotation"></param>
        public void SetRootRotation(string avatarId, MQuaternion rotation)
        {
            MAvatarPostureValues values = this.GetCurrentPostureValues(avatarId);
            // set root rotation
            values.PostureData[3] = rotation.W;
            values.PostureData[4] = rotation.X;
            values.PostureData[5] = rotation.Y;
            values.PostureData[6] = rotation.Z;

            // reset pelvis rotation to be oriented along with the root. 
            values.PostureData[10] = 1;
            values.PostureData[11] = 0;
            values.PostureData[12] = 0;
            values.PostureData[13] = 0;

            this.hierarchies[values.AvatarID].SetAvatarPostureValues(values, this.animatedJoints[values.AvatarID]);

            // I avoid using the SetChannelData here, to not overwrite the last set channel values. 
            // this.SetChannelData(values);
        }

        /// <summary>
        /// Sets the local translation of a single joint. This function is protected, as it only allows for setting
        /// the translation, if the joint actually contains translation channels. If not, the translation information 
        /// is not applied. 
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="joint"></param>
        /// <param name="position"></param>
        public void SetLocalJointPosition(string avatarId, MJointType joint, MVector3 position)
        {
            this.hierarchies[avatarId].GetChild(joint).SetLocalTranslation(position);
        }


        /// <summary>
        /// Generates a default MAvatarPosture based on the ISDescription class. 
        /// This function is to be used to get an initial MAvatarPosture in 
        /// zero posture, which is fixed up to the scale.
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public static MAvatarDescription GenerateFromDescriptionFile(string avatarID)
        {
            MAvatarDescription desc = new MAvatarDescription
            {
                AvatarID = avatarID
            };

            List<MJoint> MJointList = ISDescription.GetDefaultJointList();

            desc.ZeroPosture = new MAvatarPosture(desc.AvatarID, MJointList);
            return desc;
        }

        // the following functions can be used to parse MAvatarDescriptions from a bvh-like file. These functions
        // are currently not maintained and might be outdated. 
        #region parsing of MAvatarPosture files
       
        private static MJoint ParseJoint(string[] mos, ref int line_counter, ref List<MJoint> mjointList)
        {
            // parse lines for current joint
            // Todo: Improve parser to consider empty lines and comments
            string name = mos[line_counter].Split(' ')[1];
            float[] off = parseFloatParameter(mos[line_counter + 2].Split(' '), 3);
            MVector3 offset = new MVector3(off[0], off[1], off[2]);
            float[] quat = parseFloatParameter(mos[line_counter + 3].Split(' '), 4);
            MQuaternion rotation = new MQuaternion(quat[1], quat[2], quat[3], quat[0]);
            string[] channels = mos[line_counter + 4].Replace("CHANNELS", "").Split(' ');
            List<MChannel> mchannels = MapChannels(channels);


            MJoint mjoint = new MJoint(name, MJointTypeMap[name], offset, rotation);
            mjoint.Channels = mchannels;
            mjointList.Add(mjoint);

            line_counter += 5;
            while(!mos[line_counter].Contains("}"))
            {
                MJoint child = ParseJoint(mos, ref line_counter, ref mjointList);
                child.Parent = mjoint.ID;
            }
            line_counter += 1;

            return mjoint;
        }

        /// <summary>
        /// Helper function to parse floats from strings. 
        /// </summary>
        /// <param name="floats"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static float[] parseFloatParameter(string[] floats, int end)
        {
            float[] flts = new float[end];
            for (int i = 1; i < end + 1; i++)
            {
                flts[i - 1] = float.Parse(floats[i], System.Globalization.CultureInfo.InvariantCulture);
            }
            return flts;
        }

        public static List<MChannel> MapChannels(string[] channels)
        {
            List<MChannel> mchannels = new List<MChannel>();

            foreach (string c in channels)
            {
                if (c == "XTranslation")
                {
                    mchannels.Add(MChannel.XOffset);
                }
                else if (c == "YTranslation")
                {
                    mchannels.Add(MChannel.YOffset);
                }
                else if (c == "ZTranslation")
                {
                    mchannels.Add(MChannel.ZOffset);
                }
                else if(c == "Wrotation")
                {
                    mchannels.Add(MChannel.WRotation);
                }
                else if (c == "Xrotation")
                {
                    mchannels.Add(MChannel.XRotation);
                }
                else if (c == "Yrotation")
                {
                    mchannels.Add(MChannel.YRotation);
                }
                else if (c == "Zrotation")
                {
                    mchannels.Add(MChannel.ZRotation);
                }
            }
            return mchannels;
        }
        #endregion




        #region MMIServiceBase functions

        public Dictionary<string, string> GetStatus()
        {
            return new Dictionary<string, string>()
            {
                { "Running", true.ToString() }
            };
        }

        public MServiceDescription GetDescription()
        {
            return this.description;
        }

        public virtual MBoolResponse Setup(MAvatarDescription avatar, Dictionary<string, string> properties)
        {
            return new MBoolResponse(true);
        }

        public virtual Dictionary<string, string> Consume(Dictionary<string, string> properties)
        {
            return new Dictionary<string, string>();
        }


        public MBoolResponse Dispose(Dictionary<string, string> properties)
        {
            return new MBoolResponse(true);
        }

        public MBoolResponse Restart(Dictionary<string, string> properties)
        {
            return new MBoolResponse(false);
        }

        #endregion

        private static Dictionary<string, MJointType> MJointTypeMap = new Dictionary<string, MJointType>()
    {
            {"Undefined", MJointType.Undefined},
            {"Root", MJointType.Root },
{"LeftBallTip", MJointType.LeftBallTip},
{"LeftBall", MJointType.LeftBall},
{"LeftAnkle", MJointType.LeftAnkle},
{"LeftKnee", MJointType.LeftKnee},
{"LeftHip", MJointType.LeftHip},


{"RightBallTip", MJointType.RightBallTip},
{"RightBall", MJointType.RightBall},
{"RightAnkle", MJointType.RightAnkle},
{"RightKnee", MJointType.RightKnee},
{"RightHip", MJointType.RightHip},


{"PelvisCentre", MJointType.PelvisCentre},
{"S1L5Joint", MJointType.S1L5Joint},
{"T12L1Joint", MJointType.T12L1Joint},
{"T1T2Joint", MJointType.T1T2Joint},
{"C4C5Joint", MJointType.C4C5Joint},


{"HeadJoint", MJointType.HeadJoint},
{"HeadTip", MJointType.HeadTip},
{"MidEye", MJointType.MidEye},
{"LeftShoulder", MJointType.LeftShoulder},
{"LeftElbow", MJointType.LeftElbow},
{"LeftWrist", MJointType.LeftWrist},
{"RightShoulder", MJointType.RightShoulder},
{"RightElbow", MJointType.RightElbow},
{"RightWrist", MJointType.RightWrist},


{"LeftThumbMid", MJointType.LeftThumbMid},
{"LeftThumbMeta", MJointType.LeftThumbMeta},
{"LeftThumbCarpal", MJointType.LeftThumbCarpal},
{"LeftThumbTip", MJointType.LeftThumbTip},

{"LeftIndexMeta", MJointType.LeftIndexMeta},
{"LeftIndexProximal", MJointType.LeftIndexProximal},
{"LeftIndexDistal", MJointType.LeftIndexDistal},
{"LeftIndexTip", MJointType.LeftIndexTip},

{"LeftMiddleMeta", MJointType.LeftMiddleMeta},
{"LeftMiddleProximal", MJointType.LeftMiddleProximal},
{"LeftMiddleDistal", MJointType.LeftMiddleDistal},
{"LeftMiddleTip", MJointType.LeftMiddleTip},

{"LeftRingMeta", MJointType.LeftRingMeta},
{"LeftRingProximal", MJointType.LeftRingProximal},
{"LeftRingDistal", MJointType.LeftRingDistal},
{"LeftRingTip", MJointType.LeftRingTip},

{"LeftLittleMeta", MJointType.LeftLittleMeta},
{"LeftLittleProximal", MJointType.LeftLittleProximal},
{"LeftLittleDistal", MJointType.LeftLittleDistal},
{"LeftLittleTip", MJointType.LeftLittleTip},


{"RightThumbMid", MJointType.RightThumbMid},
{"RightThumbMeta", MJointType.RightThumbMeta},
{"RightThumbCarpal", MJointType.RightThumbCarpal},
{"RightThumbTip", MJointType.RightThumbTip},

{"RightIndexMeta", MJointType.RightIndexMeta},
{"RightIndexProximal", MJointType.RightIndexProximal},
{"RightIndexDistal", MJointType.RightIndexDistal},
{"RightIndexTip", MJointType.RightIndexTip},

{"RightMiddleMeta", MJointType.RightMiddleMeta},
{"RightMiddleProximal", MJointType.RightMiddleProximal},
{"RightMiddleDistal", MJointType.RightMiddleDistal},
{"RightMiddleTip", MJointType.RightMiddleTip},

{"RightRingMeta", MJointType.RightRingMeta},
{"RightRingProximal", MJointType.RightRingProximal},
{"RightRingDistal", MJointType.RightRingDistal},
{"RightRingTip", MJointType.RightRingTip},

{"RightLittleMeta", MJointType.RightLittleMeta},
{"RightLittleProximal", MJointType.RightLittleProximal},
{"RightLittleDistal", MJointType.RightLittleDistal},
{"RightLittleTip", MJointType.RightLittleTip }
    };

    }
}
