// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity.TargetEngine.Scene
{
    /// <summary>
    /// Enum for representing a hand type
    /// </summary>
    public enum HandType
    {
        Left,
        Right
    }

    /// <summary>
    /// Class represent a hand pose within the unity engine
    /// </summary>
    [ExecuteInEditMode]
    public class UnityHandPose : MMISceneObject
    {
        /// <summary>
        /// The hand type 
        /// </summary>
        public HandType HandType = HandType.Left;

        /// <summary>
        /// Returns an interaction pose representing the current hand transform
        /// To do return root Bone as global coordinates
        /// </summary>
        /// <returns></returns>
        public MHandPose GetPose()
        {
            //Create a new hand pose
            MHandPose handPose = new MHandPose
            {
                Joints = new List<MJoint>()
            };

            //Get all bones in hierarchy
            UnityBone[] bones = this.GetComponentsInChildren<UnityBone>();



            int depth = 0;
            foreach (UnityBone bone in bones)
            {
                //Add the first bone in global space
                if (depth == 0)
                    handPose.Joints.Add(new MJoint(bone.ID, bone.Type, bone.transform.position.ToMVector3(), bone.transform.rotation.ToMQuaternion()));


                //Add all other using the local space
                else
                {
                    MJoint mjoint = new MJoint(bone.ID, bone.Type, bone.transform.localPosition.ToMVector3(), bone.transform.localRotation.ToMQuaternion());


                    if (bone.transform.parent != null)
                    {
                        var parentBone = bone.transform.parent.GetComponent<UnityBone>();

                        if (parentBone != null)
                            mjoint.Parent = parentBone.ID;
                    }

                    handPose.Joints.Add(mjoint);
                }

                depth++;
            }

            //Use the handpose directly
            if (bones.Length == 0)
                handPose.Joints.Add(new MJoint(Guid.NewGuid().ToString(), this.HandType == HandType.Left ? MJointType.LeftWrist : MJointType.RightWrist, this.transform.GetLocalPositionScaleIndependent().ToMVector3(), this.transform.localRotation.ToMQuaternion()));

            //To do
            return handPose;
        }

        /// <summary>
        /// Returns a joint constraint representing the hand posture
        /// </summary>
        /// <returns></returns>
        public List<MConstraint> GetJointConstraints()
        {
            List<MConstraint> constraints = new List<MConstraint>();

            foreach(MJoint joint in this.GetPose().Joints)
            {
                //Create a new constraint
                MConstraint constraint = new MConstraint(Guid.NewGuid().ToString())
                {
                    JointConstraint = new MJointConstraint(joint.Type)
                    {
                        GeometryConstraint = new MGeometryConstraint()
                        {
                            ParentObjectID = "",
                            ParentToConstraint = new MTransform()
                            {
                                ID = Guid.NewGuid().ToString(),
                                Position = joint.Position,
                                Rotation = joint.Rotation
                            }
                        }
                    }
                };

                constraints.Add(constraint);
            }

            return constraints;
        }


        /// <summary>
        /// Creates a posture constraint given the UnityHandPose 
        /// </summary>
        /// <returns></returns>
        public MPostureConstraint GetPostureConstraint()
        {
            MPostureConstraint postureConstraint = new MPostureConstraint()
            {
                JointConstraints = new List<MJointConstraint>(),
                Posture = new MAvatarPostureValues("", new List<double>())
            };

            foreach (MJoint joint in this.GetPose().Joints)
            {
                MJointConstraint jointConstraint = new MJointConstraint(joint.Type)
                {
                    GeometryConstraint = new MGeometryConstraint()
                    {
                        ParentObjectID = "",
                        ParentToConstraint = new MTransform()
                        {
                            ID = Guid.NewGuid().ToString(),
                            Position = joint.Position,
                            Rotation = joint.Rotation
                        }
                    }
                };

                postureConstraint.JointConstraints.Add(jointConstraint);
            }

            return postureConstraint;
        }


        /// <summary>
        /// Editor visualization
        /// </summary>
        protected override void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.up * 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.right * 0.05f);
        }

        void OnEnable()
        {
            if (Constraints == null)
                Constraints = new List<MConstraint>();
            MConstraint mconst = new MConstraint("Posture constraint");
            mconst.PostureConstraint = GetPostureConstraint();
            Constraints.Add(mconst);
            SaveConstraints();
        }
    }
}
