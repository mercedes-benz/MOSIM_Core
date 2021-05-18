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
        public Transform reference { get; private set; }


        // Base rotations of human bones with respect to the reference bone. 
        private Quaternion humanBoneBaseRotation { get; set; } = Quaternion.identity;
        private Quaternion inverseBaseRotation = Quaternion.identity;

        // Game object for visualization. 
        private GameObject gameJoint { get; set; }

        //private Animator anim;

        private List<ISVisualizationJoint> children = new List<ISVisualizationJoint>();
        public ISVisualizationJoint parent { get; private set; }
        private MMICSharp.Common.RJoint j;


        public ISVisualizationJoint(MMICSharp.Common.RJoint jBase, Transform RootBone, Dictionary<MJointType, string> bonenameMap)
        {
            if (bonenameMap.ContainsKey(jBase.GetMJoint().Type))
            {
                string name = bonenameMap[jBase.GetMJoint().Type];
                this.reference = RootBone.GetChildRecursiveByName(name);
            }
            else
            {
                this.reference = null;
            }
            this.j = jBase;

            for (int i = 0; i < jBase.children.Count; i++)
            {
                MMICSharp.Common.RJoint child = (RJoint)jBase.children[i];
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
                        values.Add(pos.x);
                        break;
                    case MChannel.YOffset:
                        values.Add(pos.y);
                        break;
                    case MChannel.ZOffset:
                        values.Add(pos.z);
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

        public Matrix4x4 GetGlobalMatrix()
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