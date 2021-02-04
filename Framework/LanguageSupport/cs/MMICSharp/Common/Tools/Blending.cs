// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Janis Sprenger

using MMIStandard;
using System.Collections.Generic;



namespace MMICSharp.Common.Tools
{
    /// <summary>
    /// Class contains blending functionality for the MMI framework
    /// </summary>
    public static class Blending
    {
        /// <summary>
        /// Performs a blending based on the from posture and the to posture. In particular a blending weight and an additional blending mask is utilized. If the blending mask is set to null, all bones with position + rotation will be used for blending.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="weight"></param>
        /// <param name="blendingMask"></param>
        /// <returns></returns>
        public static MAvatarPostureValues PerformBlend(IntermediateSkeleton skeleton, MAvatarPostureValues from, MAvatarPostureValues to, float weight, Dictionary<MJointType, BlendProperty> blendingMask = null)
        {
            //MAvatarPosture result = from.Clone();
            MAvatarPosture zero = skeleton.GetAvatarDescription(from.AvatarID).ZeroPosture;
            skeleton.SetChannelData(from);
            List<MQuaternion> fromRot = skeleton.GetLocalJointRotations(from.AvatarID);
            skeleton.SetChannelData(to);
            List<MQuaternion> toRot = skeleton.GetLocalJointRotations(to.AvatarID);


            for (int i = 0; i < zero.Joints.Count; i++)
            {
                //By default belnd both position and rotation
                BlendProperty blendProperty = new BlendProperty(1.0f, 1.0f);
                MJointType joint = zero.Joints[i].Type;

                if (blendingMask != null && blendingMask.ContainsKey(joint))
                {
                    //Get the bone weight
                    blendingMask.TryGetValue(joint, out blendProperty);
                }

                //Perform a linear interpolation of the position
                // Does not correspond to intermediate skeleton representation. 
                // result.Joints[i].Position = result.Joints[i].Position.Lerp(to.Joints[i].Position, weight * blendProperty.PositionWeight);

                //Perform a slerp of the rotation
                skeleton.SetLocalJointRotation(to.AvatarID, joint, fromRot[i].Slerp(toRot[i], weight * blendProperty.RotationWeight));
            }

            return skeleton.RecomputeCurrentPostureValues(to.AvatarID);
        }


        /// <summary>
        /// Performs a blending based on the from posture and the to posture.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="weight"></param>
        /// <param name="rootTransform">Specifies whether the root transform is blended as well</param>
        /// <returns></returns>
        public static MAvatarPostureValues PerformBlend(IntermediateSkeleton skeleton, MAvatarPostureValues from, MAvatarPostureValues to, float weight, bool rootTransform = true)
        {
            MAvatarPosture zero = skeleton.GetAvatarDescription(from.AvatarID).ZeroPosture;
            skeleton.SetChannelData(from);
            List<MQuaternion> fromRot = skeleton.GetLocalJointRotations(from.AvatarID);
            skeleton.SetChannelData(to);
            List<MQuaternion> toRot = skeleton.GetLocalJointRotations(to.AvatarID);


            //Blend the rotation of each joint
            for (int i = 0; i < zero.Joints.Count; i++)
            {
                //Perform a slerp of the rotation
                skeleton.SetLocalJointRotation(to.AvatarID, zero.Joints[i].Type, fromRot[i].Slerp(toRot[i], weight));
            }



            //Recompute the result posture values
            MAvatarPostureValues result = skeleton.RecomputeCurrentPostureValues(to.AvatarID);


            //Blend the root transform if specified
            if (rootTransform)
            {
                if (from.PostureData.Count >= 3 || to.PostureData.Count >= 3)
                {
                    //Gather the root position of the from posture values
                    MVector3 rootPosFrom = new MVector3(from.PostureData[0], from.PostureData[1], from.PostureData[2]);

                    //Gather the root position of the to posture values
                    MVector3 rootPosTo = new MVector3(to.PostureData[0], to.PostureData[1], to.PostureData[2]);

                    //Perform an interpolation to determine new blended position
                    MVector3 newGlobalPosition = MVector3Extensions.Lerp(rootPosFrom, rootPosTo, weight);

                    result.PostureData[0] = newGlobalPosition.X;
                    result.PostureData[1] = newGlobalPosition.Y;
                    result.PostureData[2] = newGlobalPosition.Z;
                }
            }

            return result;
        }
    }

}
