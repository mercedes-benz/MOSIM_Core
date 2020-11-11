// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger

using MMIStandard;
using System.Collections.Generic;

namespace MMICSharp.Common
{

    public class Joint
    {
        protected Joint parentJoint;
        public List<Joint> children = new List<Joint>();

        protected MVector3 globalPos = null;
        protected MQuaternion globalRot = null;
        protected MJoint joint;

        protected MQuaternion currentRotationValues = new MQuaternion(0, 0, 0, 1);
        protected MVector3 translation = new MVector3(0, 0, 0);



        public Joint(MJoint j)
        {
            this.joint = j;
        }

        public MJoint GetMJoint()
        {
            return this.joint;
        }

        public List<MChannel> GetChannels()
        {
            return this.joint.Channels;
        }

        public static Joint Initialize(List<MJoint> joints)
        {
            Joint root = new Joint(joints[0]);
            for (int i = 1; i < joints.Count; i++)
            {
                Joint parent = root.GetChild(joints[i].Parent);
                Joint j = new Joint(joints[i]);
                if (parent != null)
                    parent.children.Add(j);
                j.parentJoint = parent;
            }

            // initialize zero posture
            root.SetAvatarPostureValues(null);
            return root;
        }

        public MVector3 GetLocalTranslation()
        {
            return this.translation.Clone();
        }
        
        public MQuaternion GetLocalRotation()
        {
            return this.currentRotationValues.Clone();
        }

        public void SetLocalRotation(MQuaternion rotation)
        {
            this.currentRotationValues = rotation;
        }

        public void SetLocalTranslation(MVector3 translation)
        {
            for (int i = 0; i < this.joint.Channels.Count; i++)
            {
                switch (this.joint.Channels[i])
                {
                    case MChannel.XOffset:
                        this.translation.X = translation.X;
                        break;
                    case MChannel.YOffset:
                        this.translation.Y = translation.Y;
                        break;
                    case MChannel.ZOffset:
                        this.translation.Z = translation.Z;
                        break;
                }
            }
            //this.translation = translation;
        }

        public MVector3 GetGlobalPosManually()
        {
            return this.globalPos.Clone();
        }

        public MQuaternion GetGlobalRotManually()
        {
            return this.globalRot.Clone();
        }

        public void SetGlobalPosManually(MVector3 pos)
        {
            this.globalPos = pos;
        }

        public void SetGlobalRotManually(MQuaternion rot)
        {
            this.globalRot = rot;
        }

        /// <summary>
        /// Generates avatar posture values for all joints. 
        /// </summary>
        /// <returns>list containing the posture values</returns>
        public List<double> GetAvatarPostureValues()
        {
            List<double> values = new List<double>();
            this.GetAvatarPostureValues(values, null);
            return values;
        }

        /// <summary>
        /// Generates partial Avatar Posture Values for the specified jointList
        /// </summary>
        /// <param name="jointList">unordered list of joints</param>
        /// <returns>List containing the partial joint values</returns>
        public List<double> GetAvatarPostureValuesPartial(List<MJointType> jointList)
        {
            List<double> values = new List<double>();
            this.GetAvatarPostureValues(values, jointList);
            return values;
        }

        /// <summary>
        /// Generates List for the MAvatarPostures. If jointList is null, all Joints are considered. 
        /// In any other case, only the values for the specified joints are utilized. 
        /// </summary>
        /// <param name="values">list to store the parameters in</param>
        /// <param name="jointList">unordered list of joints</param>
        private void GetAvatarPostureValues(List<double> values, List<MJointType> jointList)
        {
            MVector3 translation = this.translation;
            if (jointList == null || jointList.Contains(this.joint.Type))
            {
                for (int i = 0; i < this.joint.Channels.Count; i++)
                {
                    switch (this.joint.Channels[i])
                    {
                        case MChannel.WRotation:
                            values.Add(this.currentRotationValues.W);
                            break;
                        case MChannel.XRotation:
                            values.Add(this.currentRotationValues.X);
                            break;
                        case MChannel.YRotation:
                            values.Add(this.currentRotationValues.Y);
                            break;
                        case MChannel.ZRotation:
                            values.Add(this.currentRotationValues.Z);
                            break;
                        case MChannel.XOffset:
                            values.Add(translation.X);
                            break;
                        case MChannel.YOffset:
                            values.Add(translation.Y);
                            break;
                        case MChannel.ZOffset:
                            values.Add(translation.Z);
                            break;
                    }
                }
            }
            foreach (var child in this.children)
            {
                child.GetAvatarPostureValues(values, jointList);
            }
        }

        public void SetAvatarPostureValues(MAvatarPostureValues values, List<MJointType> animatedJoints = null)
        {
            this.SetAvatarPostureValues(values, 0, animatedJoints);
        }

        private int SetAvatarPostureValues(MAvatarPostureValues values, int id, List<MJointType> animatedJoints)
        {
            if (animatedJoints == null || animatedJoints.Contains(this.joint.Type))
            {
                translation = new MVector3(0, 0, 0);

                this.currentRotationValues = new MQuaternion(0, 0, 0, 1);

                if (values != null)
                {
                    MVector3 translation = new MVector3(0, 0, 0);
                    for (int i = 0; i < this.joint.Channels.Count; i++)
                    {
                        switch (this.joint.Channels[i])
                        {
                            case MChannel.WRotation:
                                this.currentRotationValues.W = values.PostureData[id];
                                break;
                            case MChannel.XRotation:
                                this.currentRotationValues.X = values.PostureData[id];
                                break;
                            case MChannel.YRotation:
                                this.currentRotationValues.Y = values.PostureData[id];
                                break;
                            case MChannel.ZRotation:
                                this.currentRotationValues.Z = values.PostureData[id];
                                break;
                            case MChannel.XOffset:
                                translation.X = (float)values.PostureData[id];
                                break;
                            case MChannel.YOffset:
                                translation.Y = (float)values.PostureData[id];
                                break;
                            case MChannel.ZOffset:
                                translation.Z = (float)values.PostureData[id];
                                break;
                        }
                        id += 1;
                    }

                    this.translation = translation;
                }
            }



            MQuaternion parentRotation = new MQuaternion(0, 0, 0, 1);
            MVector3 parentPosition = new MVector3(0, 0, 0);
            if (this.parentJoint != null)
            {
                parentRotation = this.parentJoint.globalRot;
                parentPosition = this.parentJoint.globalPos;
            }


            MVector3 rotatedOffset = parentRotation.Multiply(this.joint.Position);
            MVector3 rotatedTranslation = parentRotation.Multiply(this.joint.Rotation).Multiply(this.translation);
            this.globalPos = rotatedTranslation.Add(rotatedOffset).Add(parentPosition);

            this.globalRot = parentRotation.Multiply(this.joint.Rotation).Multiply(this.currentRotationValues);


            foreach (Joint c in this.children)
            {
                id = c.SetAvatarPostureValues(values, id, animatedJoints);
            }
            return id;
        }


        public MVector3 GetTranslation()
        {
            return translation;
        }

        public MQuaternion GetCurrentRotationValues()
        {
            return this.currentRotationValues;
        }


        public Joint GetChild(string id)
        {
            if (this.joint.ID == id)
                return this;

            foreach (Joint c in this.children)
            {
                Joint j = c.GetChild(id);
                if (j != null)
                    return j;
            }
            return null;
        }

        public Joint GetChild(MJointType type)
        {
            if (this.joint.Type == type)
                return this;

            foreach (Joint c in this.children)
            {
                Joint j = c.GetChild(type);
                if (j != null)
                    return j;
            }
            return null;

        }

        public void CreateZeroVector(List<double> rotation_data)
        {
            foreach (MChannel c in this.joint.Channels)
            {
                if (c == MChannel.WRotation)
                {
                    rotation_data.Add(1.0);
                }
                else
                {
                    rotation_data.Add(0.0);
                }
            }

            foreach (Joint child in this.children)
            {
                child.CreateZeroVector(rotation_data);
            }
        }

        public List<MVector3> GetCurrentJointPositions()
        {
            List<MVector3> list = new List<MVector3>();
            this.GetCurrentJointPositions(list);
            return list;
        }

        private void GetCurrentJointPositions(List<MVector3> list)
        {
            list.Add(this.GetGlobalPosition());

            foreach (Joint child in this.children)
            {
                child.GetCurrentJointPositions(list);
            }
        }

        public MVector3 GetGlobalPosition()
        {
            if(this.globalPos == null)
            {
                MQuaternion parentRotation = new MQuaternion(0, 0, 0, 1);
                MVector3 parentPosition = new MVector3(0, 0, 0);
                if (this.parentJoint != null)
                {
                    parentRotation = this.parentJoint.globalRot;
                    parentPosition = this.parentJoint.globalPos;
                }


                MVector3 rotatedOffset = parentRotation.Multiply(this.joint.Position);
                MVector3 rotatedTranslation = parentRotation.Multiply(this.joint.Rotation).Multiply(this.translation);

                this.globalPos = rotatedTranslation.Add(rotatedOffset).Add(parentPosition);
            }
            return this.globalPos;
        }

        public MVector3 GetOffsetPositions()
        {
            return this.joint.Position;
        }
        public MQuaternion GetOffsetRotation()
        {
            return this.joint.Rotation;
        }


        public MQuaternion GetGlobalRotation()
        {
            if (this.globalRot == null)
            {
                MQuaternion parentRotation = new MQuaternion(0, 0, 0, 1);
                MVector3 parentPosition = new MVector3(0, 0, 0);
                if (this.parentJoint != null)
                {
                    parentRotation = this.parentJoint.globalRot;
                    parentPosition = this.parentJoint.globalPos;
                }

                this.globalRot = parentRotation.Multiply(this.joint.Rotation).Multiply(this.currentRotationValues);
            }
            

            return this.globalRot;
        }
    }
}
