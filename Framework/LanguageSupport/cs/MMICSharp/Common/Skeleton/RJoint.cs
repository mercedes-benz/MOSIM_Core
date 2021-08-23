// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger

using MMIStandard;
using System.Collections.Generic;
using System;


namespace MMICSharp.Common
{
    public class RJoint : Joint
    {

        public RJoint(MJoint j) : base(j)
        {
        }

        public new static RJoint Initialize(List<MJoint> joints)
        {
            RJoint root = new RJoint(joints[0]);
            for (int i = 1; i < joints.Count; i++)
            {
                RJoint parent = (RJoint) root.GetChild(joints[i].Parent);
                RJoint j = new RJoint(joints[i]);
                if (parent != null)
                    parent.children.Add(j);
                j.parentJoint = parent;
            }

            // initialize zero posture
            root.SetAvatarPostureValues(null);
            return root;
        }



        // Retargeting functionality:

        //Matrix4x4 targetOffset;
        //Matrix4x4 isOffset;
        MVector3 targetOffset = new MVector3(0,0,0);
        MVector3 isOffset = new MVector3(0,0,0);

        MVector3 targetBP = new MVector3(0, 0, 0);
        MVector3 isBP = new MVector3(0, 0, 0);


        MQuaternion isGBR = new MQuaternion(0, 0, 0, 1);
        MQuaternion invIsGBR = new MQuaternion(0, 0, 0, 1);
        MQuaternion targetGBR = new MQuaternion(0, 0, 0, 1);
        MQuaternion invTargetGBR = new MQuaternion(0, 0, 0, 1);

        MQuaternion inverseOffsetRotation = new MQuaternion(0, 0, 0, 1);

        double boneLength = 0.0f;

        bool isMapped = false;


        public void SetBaseReference(MAvatarPosture posture)
        {
            foreach(MJoint j in posture.Joints)
            {
                if(j.Type != MJointType.Undefined)
                {
                    RJoint rj = (RJoint)this.GetChild(j.Type);
                    rj.SetBaseReference(j.Rotation, j.Position);
                }
            }
        } 

        private void SetBaseReference(MQuaternion baseGlobalRot, MVector3 baseGlobalPos)
        {
            this.isMapped = true;
            this.targetBP = baseGlobalPos.Clone();
            this.targetGBR = baseGlobalRot.Clone();

            this.isBP = this.GetGlobalPosition();
            this.isGBR = this.GetGlobalRotation();

            this.invTargetGBR = MQuaternionExtensions.Inverse(this.targetGBR);
            this.invIsGBR = MQuaternionExtensions.Inverse(this.isGBR);

            this.isOffset = this.invIsGBR.Multiply(this.targetBP.Subtract(this.isBP));
            this.targetOffset = this.invTargetGBR.Multiply(this.isBP.Subtract(this.targetBP));

            this.inverseOffsetRotation = MQuaternionExtensions.Inverse(this.GetOffsetRotation());
        }
 


        public MQuaternion RetargetRotationToIS(MQuaternion globalRot)
        {
            MQuaternion rot = globalRot.Multiply(this.invTargetGBR).Multiply(this.isGBR);
            this.globalRot = rot;
            return rot;
        }

        public MQuaternion RetargetRotationToTarget()
        {
            MQuaternion globalRot = this.GetGlobalRotation();
            MQuaternion rot = globalRot.Multiply(this.invIsGBR).Multiply(this.targetGBR);
            return rot;
        }

        public MVector3 RetargetPositionToIS(MVector3 globalPos, MQuaternion globalRot)
        {
            MVector3 pos = globalPos.Add(globalRot.Multiply(this.targetOffset));
            this.globalPos = pos;
            return pos;
        }

        public MVector3 RetargetPositionToTarget()
        {
            return this.GetGlobalPosition().Add(this.GetGlobalRotation().Multiply(this.isOffset));
        }




        private static MVector3 getJointPosition(MAvatarPosture globalTarget, string joint)
        {
            foreach(MJoint j in globalTarget.Joints)
            {
                if(j.ID == joint)
                {
                    return new MVector3(j.Position.X, j.Position.Y, j.Position.Z);
                }
            }
            return new MVector3(0, 0, 0);
        }

        private static double GetJointDistance(MAvatarPosture globalTarget, string joint1, string joint2)
        {
            return getJointPosition(globalTarget, joint1).Subtract(getJointPosition(globalTarget, joint2)).Magnitude();
        }

        private double GetShoulderHeight()
        {
            double ret = 0.0f;
            ret = this.GetOffsetPositions().Y;
            if (this.parentJoint != null)
            {
                ret += ((RJoint)this.parentJoint).GetShoulderHeight();
            }
            return ret;
        }

        private static MVector3 GetRelativePositionToParent(MAvatarPosture globalTarget, string joint, string parent)
        {
            foreach (MJoint j in globalTarget.Joints)
            {
                if (j.ID == joint)
                {
                    foreach (MJoint jP in globalTarget.Joints)
                    {
                        if (jP.ID == parent)
                        {
                            return new MTransform("tmp", jP.Position, jP.Rotation).InverseTransformPoint(j.Position);
                        }
                    }

                }
            }
            return new MVector3(0, 0, 0);
        }

        public float GetBoneLength()
        {
            return (float)this.boneLength;
        }


        public void SetOffsets(MVector3 vec)
        {
            this.GetOffsetPositions().X = vec.X;
            this.GetOffsetPositions().Y = vec.Y;
            this.GetOffsetPositions().Z = vec.Z;
        }

        private MVector3 GetFingerPosition(MAvatarPosture globalTarget, Dictionary<MJointType, string> joint_map, double z_shift)
        {
            MVector3 wristPos = getJointPosition(globalTarget, joint_map[this.parentJoint.GetMJoint().Type]);//this.parentJoint.GetGlobalPosition();
            MVector3 thisPos = getJointPosition(globalTarget, joint_map[this.GetMJoint().Type]);
            thisPos.Y = wristPos.Y; // adjust height
            MVector3 newPos = new MTransform("tmp", wristPos, this.parentJoint.GetGlobalRotation()).InverseTransformPoint(thisPos);
            newPos.Z -= z_shift;
            return newPos;
        }

        public void ScaleSkeleton(MAvatarPosture globalTarget, Dictionary<MJointType, string> joint_map)
        {
            List<MJointType> spine = new List<MJointType>() { MJointType.S1L5Joint, MJointType.T12L1Joint, MJointType.T1T2Joint, MJointType.C4C5Joint, MJointType.HeadJoint };
            if (spine.Contains(this.GetMJoint().Type))
            {

                //float shoulder_height = getJointPosition(HumanBodyBones.LeftUpperArm, anim).y;
                double shoulder_height = getJointPosition(globalTarget, joint_map[MJointType.HeadJoint]).Y;
                double hip_height = getJointPosition(globalTarget, joint_map[MJointType.PelvisCentre]).Y;
                double spine_length = shoulder_height - hip_height;

                
                ((RJoint)this.parentJoint).boneLength = (this.GetOffsetPositions()).Multiply(spine_length).Magnitude();
                this.GetOffsetPositions().X = 0;
                this.GetOffsetPositions().Y = 1.0;
                this.GetOffsetPositions().Z = 0;
            }
            if(this.joint.Type == MJointType.Root)
            {
                //MVector3 rootPos = getJointPosition(globalTarget, joint_map[MJointType.Root]);
                //this.SetOffsets(rootPos);
            }
            else if (this.joint.Type == MJointType.PelvisCentre)
            {
                MVector3 hips = getJointPosition(globalTarget, joint_map[MJointType.PelvisCentre]).Clone();
                MVector3 shoulder = getJointPosition(globalTarget, joint_map[MJointType.LeftShoulder]);
                MVector3 leftupleg = getJointPosition(globalTarget, joint_map[MJointType.LeftHip]);

                // initialize Root at 0, 0, 0
                ((RJoint)this.parentJoint).SetOffsets(new MVector3(0,0,0));

                // find the average z axis position of shoulders and hips to find initial position of hip
                hips.Z = (shoulder.Z + leftupleg.Z) / 2;
                // raise hip to the hight of the target avatars hip height. 
                hips.Y = leftupleg.Y;
                //MVector3 root = new MVector3(hips.X, 0, hips.Z);
                hips.X = 0;
                hips.Z = 0;
                this.SetOffsets(hips);

            }
            else
            {
                double factor = 0.0d;
                if (this.parentJoint.children.Count == 3 && this.parentJoint.children[0] != this)
                {
                    // If there are two children (e.g. pelvis / upper spine), take the half distance between both children
                    factor = GetJointDistance(globalTarget, joint_map[this.parentJoint.children[1].GetMJoint().Type], joint_map[this.parentJoint.children[2].GetMJoint().Type]) / 2.0d;
                }
                else
                {
                    // If there is one child, take the parent bone length to scale target position
                    factor = ((RJoint)this.parentJoint).boneLength;
                }
                // scale offset to scale the skeleton to match bone lengths. 
                this.SetOffsets(this.GetOffsetPositions().Multiply(factor));


                // handle special cases for shoulders and hands
                string jointID = this.GetMJoint().ID;

                if (jointID.Contains("Shoulder"))
                {
                    // in case of shoulders, they have to be raised in up direction. 
                    double shoulder_height = getJointPosition(globalTarget, joint_map[this.GetMJoint().Type]).Y;
                    double current_Pos = this.GetShoulderHeight();
                    shoulder_height = shoulder_height - current_Pos;
                    this.GetOffsetPositions().Y = shoulder_height;

                }
            }
            if (this.children.Count > 0 && this.children[0].GetMJoint().ID.Contains("Tip"))
            {
                // Finger Tips
                MVector3 offsetVector = this.children[0].GetOffsetPositions();
                this.boneLength = offsetVector.Magnitude();
            }
            else
            {
                // if this is not the last joint in a sequence, scale depending on distance to child human bone
                if (joint_map.ContainsKey(this.children[0].GetMJoint().Type) && joint_map.ContainsKey(this.GetMJoint().Type))
                {
                    this.boneLength = GetJointDistance(globalTarget, joint_map[this.GetMJoint().Type], joint_map[this.children[0].GetMJoint().Type]);
                }
                else
                {
                    this.boneLength = 0.1;
                }
                
            }

            // recurse
            foreach (Joint child in this.children)
            {
                if (!child.GetMJoint().ID.Contains("Tip"))
                {
                    ((RJoint)(child)).ScaleSkeleton(globalTarget, joint_map);
                }
            }

            if(this.GetMJoint().ID.Contains("Wrist"))
            {
                MVector3 wristPos = this.GetGlobalPosition();//getJointPosition(globalTarget, joint_map[this.GetMJoint().Type]);
                MTransform Twrist = new MTransform("tmp", this.GetGlobalPosition(), this.GetGlobalRotation());
                Joint middle = this.children[0];

                Joint index = this.children[1];
                Joint ring = this.children[2];
                Joint little = this.children[3];
                Joint thumb = this.children[4];

                double middle_shift = 0.0;
                if (joint_map.ContainsKey(middle.GetMJoint().Type))
                {
                    MVector3 MiddlePos = ((RJoint)middle).GetFingerPosition(globalTarget, joint_map, 0);
                    middle_shift = MiddlePos.Z;
                    MiddlePos.Z = 0;
                    ((RJoint)this.children[0]).SetOffsets(MiddlePos);
                } else
                {
                    ((RJoint)this.children[0]).SetOffsets(new MVector3(0,0.1,0));
                }

                if (joint_map.ContainsKey(index.GetMJoint().Type))
                {
                    MVector3 IndexPos = ((RJoint)index).GetFingerPosition(globalTarget, joint_map, middle_shift);
                    ((RJoint)this.children[1]).SetOffsets(IndexPos);
                }
                else
                {
                    ((RJoint)this.children[1]).SetOffsets(new MVector3(0, 0.1, 0.02));
                }

                if (joint_map.ContainsKey(ring.GetMJoint().Type))
                {
                    MVector3 RingPos = ((RJoint)ring).GetFingerPosition(globalTarget, joint_map, middle_shift);
                    ((RJoint)this.children[2]).SetOffsets(RingPos);
                }
                else
                {
                    ((RJoint)this.children[2]).SetOffsets(new MVector3(0, 0.1, -0.02));
                }
                if (joint_map.ContainsKey(little.GetMJoint().Type))
                {
                    MVector3 LittlePos = ((RJoint)little).GetFingerPosition(globalTarget, joint_map, middle_shift);
                    ((RJoint)this.children[3]).SetOffsets(LittlePos);
                }
                else
                {
                    ((RJoint)this.children[3]).SetOffsets(new MVector3(0, 0.1, -0.04));
                }

                if (joint_map.ContainsKey(thumb.GetMJoint().Type))
                {
                    MVector3 ThumbPos = ((RJoint)thumb).GetFingerPosition(globalTarget, joint_map, middle_shift);
                    ((RJoint)this.children[4]).SetOffsets(ThumbPos);
                }
                else
                {
                    ((RJoint)this.children[4]).SetOffsets(new MVector3(0, 0.02, 0.02));
                }
            }
        }



        public void RecomputeLocalTransformations()
        {
            MQuaternion parentRotation = new MQuaternion(0, 0, 0, 1);
            MVector3 parentPosition = new MVector3(0, 0, 0);
            if (this.parentJoint != null)
            {
                parentRotation = this.parentJoint.GetGlobalRotation();
                parentPosition = this.parentJoint.GetGlobalPosition();
            }
            MQuaternion inverseParentRot = MQuaternionExtensions.Inverse(parentRotation);

            if (!isMapped && this.parentJoint == null)
            {
                this.currentRotationValues = new MQuaternion(0, 0, 0, 1);
                this.translation = this.globalPos;
            }
            else if (isMapped)
            {
                this.currentRotationValues = this.inverseOffsetRotation.Multiply(inverseParentRot).Multiply(this.globalRot);

                MVector3 rotatedOffset = parentRotation.Multiply(this.joint.Position);
                MVector3 rotatedTranslation = this.globalPos.Subtract(parentPosition).Subtract(rotatedOffset);
                this.translation = this.inverseOffsetRotation.Multiply(inverseParentRot).Multiply(rotatedTranslation);
            }
            else
            {
                this.currentRotationValues = new MQuaternion(0, 0, 0, 1);
                this.translation = new MVector3(0, 0, 0);
                this.globalPos = null;
                this.globalRot = null;
                this.GetGlobalPosition();
                this.GetGlobalRotation();
            }

            foreach (Joint c in this.children)
            {
                ((RJoint)c).RecomputeLocalTransformations();
            }

        }

    }
}
