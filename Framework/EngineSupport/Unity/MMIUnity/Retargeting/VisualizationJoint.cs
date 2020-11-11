// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger


using System.Collections.Generic;
using UnityEngine;
using MMICSharp.Common;
using MMIStandard;

namespace MMIUnity.Retargeting
{

    public class ISVisualizationJoint
    {

        // Hard reference to human bones if possible. In case of the spine, this will be null. 
        //private HumanBodyBones referenceBone { get; }
        private Transform reference;


        // Base rotations of human bones with respect to the reference bone. 
        private Quaternion humanBoneBaseRotation { get; set; } = Quaternion.identity;
        private Quaternion inverseBaseRotation = Quaternion.identity;

        // Game object for visualization. 
        private GameObject gameJoint { get; set; }

        //private Animator anim;

        private List<ISVisualizationJoint> children = new List<ISVisualizationJoint>();
        private ISVisualizationJoint parent;
        private MMICSharp.Common.RJoint j;


        public ISVisualizationJoint(MMICSharp.Common.RJoint jBase, Transform RootBone, Dictionary<MJointType, string> bonenameMap)
        {
            if (bonenameMap.ContainsKey(jBase.GetMJoint().Type))
            {
                string name = bonenameMap[jBase.GetMJoint().Type];
                this.reference = RootBone.GetChildRecursiveByName(name);
            } else
            {
                this.reference = null;
            }
            /*
            if (bonemap.ContainsKey(jBase.GetMJoint().ID))
            {
                this.referenceBone = bonemap[jBase.GetMJoint().ID];
            }
            else
            {
                //Debug.LogWarning("Retargeting: there is a bone without a reference: " + jBase.GetMJoint().ID);
                this.referenceBone = HumanBodyBones.LastBone;
            }*/
            //this.anim = anim;
            this.j = jBase;

            for (int i = 0; i < jBase.children.Count; i++)
            {
                MMICSharp.Common.RJoint child = (RJoint) jBase.children[i];
                ISVisualizationJoint retjoint = new ISVisualizationJoint(child, RootBone, bonenameMap);
                retjoint.parent = this;
                this.children.Add(retjoint);
            }
        }

        public void Destroy()
        {
            GameObject.Destroy(this.gameJoint);
        }


        public List<double> GetManualPostureValues()
        {
            List<double> vals = new List<double>();
            this.GetManualPostureValues(vals);
            return vals;
        }

        private void GetManualPostureValues(List<double> values)
        {
            Quaternion q = (this.inverseBaseRotation * this.gameJoint.transform.localRotation);
            //Vector3 pos = this.gameJoint.transform.localPosition - this.j.GetMJoint().Position.ToVector3(); //this.GetRelativeWorldPos
            Vector3 pos = this.GetPositionAnimation();
            for (int i = 0; i < this.j.GetChannels().Count; i++)
            {
                switch (this.j.GetChannels()[i])
                {
                    case MChannel.WRotation:
                        values.Add(q.w);
                        break;
                    case MChannel.XRotation:
                        values.Add(q.x);
                        break;
                    case MChannel.YRotation:
                        values.Add(q.y);
                        break;
                    case MChannel.ZRotation:
                        values.Add(q.z);
                        break;
                    case MChannel.XOffset:
                        values.Add(pos.x);// - this.j.GetOffsetPositions().X);
                        break;
                    case MChannel.YOffset:
                        values.Add(pos.y);// - this.j.GetOffsetPositions().Y);
                        break;
                    case MChannel.ZOffset:
                        values.Add(pos.z);// - this.j.GetOffsetPositions().Z);
                        break;
                }
            }

            foreach (var child in this.children)
            {
                child.GetManualPostureValues(values);
            }

        }


        /// <summary>
        /// Updates the animation data based on the current intermediate skeleton configuration. This function has to be used in case of manual animation of the intermediate skeleton. 
        /// </summary>
        public void ApplyPostureValues()
        {
            this.gameJoint.transform.rotation = this.j.GetGlobalRotation().ToQuaternion();

            if (this.j.GetChannels().Contains(MChannel.XOffset))
            {
                this.gameJoint.transform.position = this.j.GetGlobalPosition().ToVector3();
            }

            foreach (ISVisualizationJoint child in this.children)
            {
                child.ApplyPostureValues();
            }
        }

        public Vector3 GetPositionAnimation()
        {
            Vector3 dir = this.gameJoint.transform.localPosition;
            dir -= this.j.GetOffsetPositions().ToVector3();
            dir = Quaternion.Inverse(this.j.GetOffsetRotation().ToQuaternion()) * dir;

            return dir;
        }

        private Matrix4x4 GetGlobalMatrix()
        {
            var t = j.GetOffsetPositions().ToVector3();
            var q = j.GetOffsetRotation().ToQuaternion();
            var m = Matrix4x4.TRS(t, q, Vector3.one);
        
            if (parent != null)
            {
                var parentM = parent.GetGlobalMatrix();
                m = parentM * m;
            }
            
            return m;
        }


        private static Vector3 getJointPosition(Transform t)
        {
            if(t == null)
            {
                return Vector3.zero;
            } else
            {
                return new Vector3(t.position.x, t.position.y, t.position.z);
            }
            /*
            if (bone != HumanBodyBones.LastBone && anim.GetBoneTransform(bone) != null)
            {
                Vector3 v = anim.GetBoneTransform(bone).position;
                return new Vector3(v.x, v.y, v.z);
            }
            else
            {
                return Vector3.zero;
            }*/
        }

        /// <summary>
        /// Creates the Game Objects for the skeleton joints. The Game Objects are not only for visualization but provide access to Unity3D transformation classes as well. 
        /// </summary>
        /// <param name="jointPrefab"></param>
        public void CreateGameObjSkel(GameObject jointPrefab)
        {
            // Instantiate new object
            this.gameJoint = Object.Instantiate<GameObject>(jointPrefab);
            this.gameJoint.name = this.j.GetMJoint().ID;
            Vector3 parentPosition = new Vector3(0, 0, 0);

            // Set parent
            if (this.parent != null)
            {
                gameJoint.transform.parent = this.parent.gameJoint.transform;
            }
            else
            {
            }

            // Set transformations
            gameJoint.transform.localScale = new Vector3(1, 1, 1);
            gameJoint.transform.localRotation = new Quaternion((float)this.j.GetOffsetRotation().X, (float)this.j.GetOffsetRotation().Y, (float)this.j.GetOffsetRotation().Z, (float)this.j.GetOffsetRotation().W);
            gameJoint.transform.localPosition = new Vector3((float)this.j.GetOffsetPositions().X, (float)this.j.GetOffsetPositions().Y, (float)this.j.GetOffsetPositions().Z);



            // compute inverse rotations
            this.inverseBaseRotation = Quaternion.Inverse(gameJoint.transform.localRotation);

            // This is a hack. Changing the scale above messes up the child joints. Hence, additional Scaled joints as dummy objects are utilized. 
            GameObject jointScaled = Object.Instantiate<GameObject>(jointPrefab);
            jointScaled.name = this.gameJoint.name;
            jointScaled.transform.parent = gameJoint.transform;
            jointScaled.transform.localPosition = Vector3.zero;
            jointScaled.transform.localRotation = Quaternion.identity;
            float visWidth = 20;
            float visLength = 100 * (float)this.j.GetBoneLength();
            string jointID = this.j.GetMJoint().ID;
            if (jointID.Contains("Proximal") || jointID.Contains("Meta") || jointID.Contains("Distal") || jointID.Contains("Mid") || jointID.Contains("Carpal"))
            {
                visWidth = 5;
            }
            if (jointID.Contains("Tip"))
            {
                visWidth = 2;
                visLength = 0.001f;
            }
            jointScaled.transform.localScale = new Vector3(visWidth, visLength, visWidth);

            // Recurse
            foreach (ISVisualizationJoint child in this.children)
            {
                child.CreateGameObjSkel(jointPrefab);
            }
        }

        private void AlignJoint(ISVisualizationJoint start, ISVisualizationJoint end)
        {
            Vector3 source_dir = getJointPosition(end.reference) - getJointPosition(start.reference);
            Vector3 endPos = end.GetGlobalMatrix().GetColumn(3);//end.gameJoint.transform.position;
            Vector3 target_dir = endPos - getJointPosition(start.reference);
            Quaternion q = Quaternion.FromToRotation(source_dir, target_dir);
            
            start.reference.rotation = q * start.reference.rotation;

            //this.anim.GetBoneTransform (start.referenceBone).rotation = q * this.anim.GetBoneTransform(start.referenceBone).rotation;
        }
        
        private void SetFingerJoints(ISVisualizationJoint j)
        {

            //set index joint to target index joint position
            Vector3 targetPos = getJointPosition(j.reference);
            Vector3 newOffset = j.parent.GetGlobalMatrix().inverse.MultiplyPoint3x4(targetPos);

            j.reference.position = new Vector3(j.GetGlobalMatrix().GetColumn(3).x, j.GetGlobalMatrix().GetColumn(3).y, j.GetGlobalMatrix().GetColumn(3).z) ;            
        }

        private void AlignHand(string hand)
        {
            // under-aligning the fingers results in better visual quality
            AlignJoint(this.GetJointByName(hand + "ThumbMid"), this.GetJointByName(hand + "ThumbCarpal"));
            AlignJoint(this.GetJointByName(hand + "IndexProximal"), this.GetJointByName(hand + "IndexDistal"));
            AlignJoint(this.GetJointByName(hand + "MiddleProximal"), this.GetJointByName(hand + "MiddleDistal"));
            AlignJoint(this.GetJointByName(hand + "RingProximal"), this.GetJointByName(hand + "RingDistal"));
            AlignJoint(this.GetJointByName(hand + "LittleProximal"), this.GetJointByName(hand + "LittleDistal"));

            // equal numerical poses do not necessarily mean equal geometrical / meshed poses

            /*
            SetFingerJoints(this.GetJointByName(hand + "ThumbMid"));
            SetFingerJoints(this.GetJointByName(hand + "IndexProximal"));
            SetFingerJoints(this.GetJointByName(hand + "MiddleProximal"));
            SetFingerJoints(this.GetJointByName(hand + "RingProximal"));
            SetFingerJoints(this.GetJointByName(hand + "LittleProximal"));

            AlignJoint(this.GetJointByName(hand + "ThumbMid"), this.GetJointByName(hand + "ThumbMeta"));
            AlignJoint(this.GetJointByName(hand + "ThumbMeta"), this.GetJointByName(hand + "ThumbCarpal"));

            AlignJoint(this.GetJointByName(hand + "IndexProximal"), this.GetJointByName(hand + "IndexMeta"));
            AlignJoint(this.GetJointByName(hand + "IndexMeta"), this.GetJointByName(hand + "IndexDistal"));

            AlignJoint(this.GetJointByName(hand + "MiddleProximal"), this.GetJointByName(hand + "MiddleMeta"));
            AlignJoint(this.GetJointByName(hand + "MiddleMeta"), this.GetJointByName(hand + "MiddleDistal"));

            AlignJoint(this.GetJointByName(hand + "RingProximal"), this.GetJointByName(hand + "RingMeta"));
            AlignJoint(this.GetJointByName(hand + "RingMeta"), this.GetJointByName(hand + "RingDistal"));

            AlignJoint(this.GetJointByName(hand + "LittleProximal"), this.GetJointByName(hand + "LittleMeta"));
            AlignJoint(this.GetJointByName(hand + "LittleMeta"), this.GetJointByName(hand + "LittleDistal"));
            */
        }

        /// <summary>
        /// This function aligns the target avatar to the intermediate skeleton posture. It assumes, that both are in T-Pose and reduces the residual error due to small misalignments. 
        /// This function may have to be adapted to incorporate / load manually pre-aligend configurations
        /// </summary>
        /// <param name="anim"></param>
        public void AlignAvatar()
        {
            //this.anim = anim;
            AlignJoint(this.GetJointByName("LeftHip"), this.GetJointByName("LeftAnkle"));
            AlignJoint(this.GetJointByName("RightHip"), this.GetJointByName("RightAnkle"));
            AlignJoint(this.GetJointByName("LeftShoulder"), this.GetJointByName("LeftWrist"));
            AlignJoint(this.GetJointByName("RightShoulder"), this.GetJointByName("RightWrist"));

            AlignJoint(this.GetJointByName("LeftWrist"), this.GetJointByName("LeftMiddleProximal"));
            AlignJoint(this.GetJointByName("RightWrist"), this.GetJointByName("RightMiddleProximal"));


            AlignHand("Left");
            AlignHand("Right");
        }

        public MAvatarPostureValues GetZeroPosture(string AvatarID)
        {
            List<double> rotation_data = new List<double>();
            this.j.CreateZeroVector(rotation_data);
            return new MAvatarPostureValues(AvatarID, rotation_data);
        }

        public void SetToZero(string AvatarID)
        {

            MAvatarPostureValues zero = this.GetZeroPosture(AvatarID);
            this.j.SetAvatarPostureValues(zero);
        }


        public ISVisualizationJoint GetJointByName(string name)
        {
            if (this.j.GetMJoint().ID == name)
            {
                return this;
            }
            ISVisualizationJoint returnValue;
            foreach (ISVisualizationJoint children in this.children)
            {
                returnValue = children.GetJointByName(name);
                if (returnValue != null)
                {
                    return returnValue;
                }
            }
            return null;
        }
    }

}