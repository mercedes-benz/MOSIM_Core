// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger


using System.Collections.Generic;
using UnityEngine;
using MMICSharp.Common;
using MMIStandard;

namespace MMIUnity.Retargeting
{

    /// <summary>
    /// The Skeleton Retargeting class manages bi-directional retargeting between the intermediate skeleton and a mechanim-configured humanoid avatar. 
    /// It utilizes a reference posture to map both representations and interpolates the spine. 
    /// </summary>
    public class SkeletonVisualization
    {
        IntermediateSkeleton skeleton;
        //private Animator anim;

        public ISVisualizationJoint root;
        private string AvatarID;

        Dictionary<MJointType, string> bonenameMap;
        Transform RootBone;

        private RetargetingService retargetingService;

        // potential alignment addon. This functionality was moved to the skeleton configurator tool. 
        public IJointAlignment alignment;

        public SkeletonVisualization(IntermediateSkeleton skeleton, RetargetingService retargetingService, Transform RootBone, Dictionary<string, MJointType> bonenameMap, string AvatarID, GameObject gameJointPrefab)
        {
            //this.anim = anim;

            this.RootBone = RootBone;
            this.bonenameMap = bonenameMap.Invert();

            this.skeleton = skeleton;
            this.AvatarID = AvatarID;

            this.retargetingService = retargetingService;

            root = new ISVisualizationJoint((RJoint)skeleton.GetRoot(this.AvatarID), RootBone, this.bonenameMap);
            root.CreateGameObjSkel(gameJointPrefab);
        }

        public void AlignAvatar()
        {
            if (this.alignment != null)
            {
                this.alignment.AlignAvatar(this.root);
            }
        }

        public void AssignPostureValues()
        {
            this.root.ApplyPostureValues();
        }

        public void SetZeroPosture()
        {
            root.SetToZero(this.AvatarID);
        }

        public MAvatarPostureValues GetZeroPosture()
        {
            return root.GetZeroPosture(this.AvatarID);
        }

        public List<double> GetRetargetedPostureValues()
        {
            return this.root.GetManualPostureValues();
        }
    }
}