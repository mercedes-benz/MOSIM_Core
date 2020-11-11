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


            for (int i = 0; i < zero.Joints.Count; i++)
            {
                //By default belnd both position and rotation
                MJointType joint = zero.Joints[i].Type;

                //Perform a linear interpolation of the position
                // Does not correspond to intermediate skeleton representation. 
                // result.Joints[i].Position = result.Joints[i].Position.Lerp(to.Joints[i].Position, weight * blendProperty.PositionWeight);

                //Perform a slerp of the rotation
                skeleton.SetLocalJointRotation(to.AvatarID, joint, fromRot[i].Slerp(toRot[i], weight));
            }

            return skeleton.RecomputeCurrentPostureValues(to.AvatarID);            
            /*
            MAvatarPosture result = from.Clone();

            for (int i = 0; i < result.Joints.Count; i++)
            {
                //Skip if root transform should be ignored
                if (i == 0 && rootTransform)
                    result.Joints[i].Position = result.Joints[i].Position.Lerp(to.Joints[i].Position, weight);

                //Perform a slerp of the rotation
                result.Joints[i].Rotation = result.Joints[i].Rotation.Slerp(to.Joints[i].Rotation, weight);
            }

            return result;
            */
        }
    }

}
