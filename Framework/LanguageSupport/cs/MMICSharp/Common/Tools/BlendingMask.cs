// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMICSharp.Common.Tools
{
    /// <summary>
    /// Class contains a set of default blend masks
    /// </summary>
    public class BlendingMask
    {

        /// <summary>
        /// Returns a blend mask for considering the rotations of the left arm + fingers
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> LeftArmRotations
        {
            get
            {
                Dictionary<MJointType, BlendProperty> mask = BlendingMask.None;
                mask[MJointType.LeftShoulder] = new BlendProperty(0, 1f);
                mask[MJointType.LeftElbow] = new BlendProperty(0, 1f);
                mask[MJointType.LeftWrist] = new BlendProperty(0, 1f);

                mask[MJointType.LeftThumbMid] = new BlendProperty(0, 1f);
                mask[MJointType.LeftThumbMeta] = new BlendProperty(0, 1f);
                mask[MJointType.LeftThumbCarpal] = new BlendProperty(0, 1f);

                mask[MJointType.LeftIndexMeta] = new BlendProperty(0, 1f);
                mask[MJointType.LeftIndexProximal] = new BlendProperty(0, 1f);
                mask[MJointType.LeftIndexDistal] = new BlendProperty(0, 1f);

                mask[MJointType.LeftMiddleMeta] = new BlendProperty(0, 1f);
                mask[MJointType.LeftMiddleProximal] = new BlendProperty(0, 1f);
                mask[MJointType.LeftMiddleDistal] = new BlendProperty(0, 1f);

                mask[MJointType.LeftRingMeta] = new BlendProperty(0, 1f);
                mask[MJointType.LeftRingProximal] = new BlendProperty(0, 1f);
                mask[MJointType.LeftRingDistal] = new BlendProperty(0, 1f);

                mask[MJointType.LeftLittleMeta] = new BlendProperty(0, 1f);
                mask[MJointType.LeftLittleProximal] = new BlendProperty(0, 1f);
                mask[MJointType.LeftLittleDistal] = new BlendProperty(0, 1f);
                return mask;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the rotations of the right arm + fingers
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> RightArmRotations
        {
            get
            {
                Dictionary<MJointType, BlendProperty> mask = BlendingMask.None;
                mask[MJointType.RightShoulder] = new BlendProperty(0, 1f);
                mask[MJointType.RightElbow] = new BlendProperty(0, 1f);
                mask[MJointType.RightWrist] = new BlendProperty(0, 1f);

                mask[MJointType.RightThumbMid] = new BlendProperty(0, 1f);
                mask[MJointType.RightThumbMeta] = new BlendProperty(0, 1f);
                mask[MJointType.RightThumbCarpal] = new BlendProperty(0, 1f);

                mask[MJointType.RightIndexMeta] = new BlendProperty(0, 1f);
                mask[MJointType.RightIndexProximal] = new BlendProperty(0, 1f);
                mask[MJointType.RightIndexDistal] = new BlendProperty(0, 1f);

                mask[MJointType.RightMiddleMeta] = new BlendProperty(0, 1f);
                mask[MJointType.RightMiddleProximal] = new BlendProperty(0, 1f);
                mask[MJointType.RightMiddleDistal] = new BlendProperty(0, 1f);

                mask[MJointType.RightRingMeta] = new BlendProperty(0, 1f);
                mask[MJointType.RightRingProximal] = new BlendProperty(0, 1f);
                mask[MJointType.RightRingDistal] = new BlendProperty(0, 1f);

                mask[MJointType.RightLittleMeta] = new BlendProperty(0, 1f);
                mask[MJointType.RightLittleProximal] = new BlendProperty(0, 1f);
                mask[MJointType.RightLittleDistal] = new BlendProperty(0, 1f);

                return mask;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the rotations of the upper body (without hips)
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> UpperBodyRotations
        {
            get
            {
                Dictionary<MJointType, BlendProperty> mask = BlendingMask.None;
 

                foreach (var entry in BlendingMask.LeftArmRotations)
                {
                    mask[entry.Key] = entry.Value;
                }

                foreach (var entry in BlendingMask.RightArmRotations)
                {
                    mask[entry.Key] = entry.Value;
                }

                return mask;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the rotations of the lower body (without hips)
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> LowerBodyRotations
        {
            get
            {
                Dictionary<MJointType, BlendProperty> mask = BlendingMask.None;

                foreach (var entry in BlendingMask.LeftLegRotations)
                {
                    mask[entry.Key] = entry.Value;
                }

                foreach (var entry in BlendingMask.RightLegRotations)
                {
                    mask[entry.Key] = entry.Value;
                }

                return mask;
            }
        }

        /// <summary>
        /// Returns a blend mask for considering the rotations of the left leg
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> LeftLegRotations
        {
            get
            {
                Dictionary<MJointType, BlendProperty> mask = BlendingMask.None;
                mask[MJointType.LeftHip] = new BlendProperty(0, 1f);
                mask[MJointType.LeftKnee] = new BlendProperty(0, 1f);
                mask[MJointType.LeftAnkle] = new BlendProperty(0, 1f);
                mask[MJointType.LeftBall] = new BlendProperty(0, 1f);

                return mask;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the rotations of the right leg
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> RightLegRotations
        {
            get
            {
                Dictionary<MJointType, BlendProperty> mask = BlendingMask.None;
                mask[MJointType.RightHip] = new BlendProperty(0, 1f);
                mask[MJointType.RightKnee] = new BlendProperty(0, 1f);
                mask[MJointType.RightAnkle] = new BlendProperty(0, 1f);
                mask[MJointType.RightBall] = new BlendProperty(0, 1f);

                return mask;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the rotation only
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> RotationsOnly
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = new Dictionary<MJointType, BlendProperty>();
                foreach (MJointType type in Enum.GetValues(typeof(MJointType)))
                {
                    dict.Add(type, new BlendProperty(0, 1.0f));
                }

                return dict;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the positions only
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> PositionsOnly
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = new Dictionary<MJointType, BlendProperty>();
                foreach (MJointType type in Enum.GetValues(typeof(MJointType)))
                {
                    dict.Add(type, new BlendProperty(1.0f, 0f));
                }

                return dict;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the root transform + all rotations
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> RootTransformAndRotations
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = RotationsOnly;
                dict[MJointType.PelvisCentre].PositionWeight = 1.0f;
                return dict;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the local rotation only
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> LocalRotationsOnly
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = RotationsOnly;
                dict[MJointType.PelvisCentre].RotationWeight = 0.0f;
                return dict;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the local psotions only
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> LocalPositionsOnly
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = BlendingMask.PositionsOnly;
                dict[MJointType.PelvisCentre].PositionWeight = 0.0f;
                return dict;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering the rotation only
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> None
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = new Dictionary<MJointType, BlendProperty>();
                foreach (MJointType type in Enum.GetValues(typeof(MJointType)))
                {
                    dict.Add(type, new BlendProperty(0, 0));
                }

                return dict;
            }
        }


        /// <summary>
        /// Returns a blend mask for considering all bone types with postion and rotation (default behavior)
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> All
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = new Dictionary<MJointType, BlendProperty>();
                foreach (MJointType type in Enum.GetValues(typeof(MJointType)))
                {
                    dict.Add(type, new BlendProperty(1, 1));
                }

                return dict;
            }
        }

        /// <summary>
        /// Returns a blend mask for considering the rotation only
        /// </summary>
        public static Dictionary<MJointType, BlendProperty> RootTransformOnly
        {
            get
            {
                Dictionary<MJointType, BlendProperty> dict = None;
                dict[MJointType.PelvisCentre].PositionWeight = 1.0f;
                dict[MJointType.PelvisCentre].RotationWeight = 1.0f;
                return dict;
            }
        }

    }

    /// <summary>
    /// Class which represents properties to be used within a blending mask
    /// </summary>
    public class BlendProperty
    {
        /// <summary>
        /// The position weight used for blending [0,1]
        /// </summary>
        public float PositionWeight;

        /// <summary>
        /// The rotation weight used for blending [0,1]
        /// </summary>
        public float RotationWeight;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="positionWeight"></param>
        /// <param name="rotationWeight"></param>
        public BlendProperty(float positionWeight, float rotationWeight)
        {
            this.PositionWeight = positionWeight;
            this.RotationWeight = rotationWeight;
        }
    }
}
