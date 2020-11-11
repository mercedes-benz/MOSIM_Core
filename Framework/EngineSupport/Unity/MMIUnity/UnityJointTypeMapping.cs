// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity
{
    /// <summary>
    /// Default joint type mapping from the UnityMecanim types to the MJointType of the MMI framework
    /// </summary>
    public class UnityJointTypeMapping
    {
        //To do -> Check if required 
        public static Dictionary<MJointType, HumanBodyBones> ToHumanBodyBones = new Dictionary<MJointType, HumanBodyBones>()
        {
            { MJointType.PelvisCentre, HumanBodyBones.Hips},

            //To check
            { MJointType.S1L5Joint, HumanBodyBones.Spine},

            //Tbd LowerThoracicSpine
            //{ MJointType.T12L12Joint, HumanBodyBones.Spine},

            { MJointType.T1T2Joint, HumanBodyBones.Chest},


            { MJointType.C4C5Joint, HumanBodyBones.Neck},
            { MJointType.HeadJoint, HumanBodyBones.Head},
            { MJointType.MidEye, HumanBodyBones.LeftEye},


            { MJointType.LeftShoulder, HumanBodyBones.LeftUpperArm},
            { MJointType.LeftElbow, HumanBodyBones.LeftLowerArm},
            { MJointType.LeftWrist, HumanBodyBones.LeftHand},


            { MJointType.LeftIndexDistal, HumanBodyBones.LeftIndexDistal},
            { MJointType.LeftIndexProximal, HumanBodyBones.LeftIndexProximal},
            { MJointType.LeftIndexMeta, HumanBodyBones.LeftIndexIntermediate},
            //{ MJointType.LeftIndexMidCarpalJoint, HumanBodyBones.LeftIndexIntermediate},

            { MJointType.LeftLittleDistal, HumanBodyBones.LeftLittleDistal},
            { MJointType.LeftLittleProximal, HumanBodyBones.LeftLittleProximal},
            { MJointType.LeftLittleMeta, HumanBodyBones.LeftLittleIntermediate},
            //{ MJointType.LeftLittleMidCarpalJoint, HumanBodyBones.LeftLittleIntermediate},

            { MJointType.LeftMiddleDistal, HumanBodyBones.LeftMiddleDistal},
            { MJointType.LeftMiddleProximal, HumanBodyBones.LeftMiddleProximal},
            { MJointType.LeftMiddleMeta, HumanBodyBones.LeftMiddleIntermediate},
            //{ MJointType.LeftMiddleMidCarpalJoint, HumanBodyBones.LeftMiddleIntermediate},

            { MJointType.LeftRingDistal, HumanBodyBones.LeftRingDistal},
            { MJointType.LeftRingProximal, HumanBodyBones.LeftRingProximal},
            { MJointType.LeftRingMeta, HumanBodyBones.LeftRingIntermediate},
            //{ MJointType.LeftRingMidCarpalJoint, HumanBodyBones.LeftRingIntermediate},

            { MJointType.LeftThumbCarpal, HumanBodyBones.LeftThumbDistal},
            { MJointType.LeftThumbTip, HumanBodyBones.LeftThumbProximal},
            { MJointType.LeftThumbMid, HumanBodyBones.LeftThumbIntermediate},


            { MJointType.RightIndexDistal, HumanBodyBones.RightIndexDistal},
            { MJointType.RightIndexProximal, HumanBodyBones.RightIndexProximal},
            { MJointType.RightIndexMeta, HumanBodyBones.RightIndexIntermediate},
            //{ MJointType.RightIndexMidCarpalJoint, HumanBodyBones.RightIndexIntermediate},

            { MJointType.RightLittleDistal, HumanBodyBones.RightLittleDistal},
            { MJointType.RightLittleProximal, HumanBodyBones.RightLittleProximal},
            { MJointType.RightLittleMeta, HumanBodyBones.RightLittleIntermediate},
            //{ MJointType.RightLittleMidCarpalJoint, HumanBodyBones.RightLittleIntermediate},

            { MJointType.RightMiddleDistal, HumanBodyBones.RightMiddleDistal},
            { MJointType.RightMiddleProximal, HumanBodyBones.RightMiddleProximal},
            { MJointType.RightMiddleMeta, HumanBodyBones.RightMiddleIntermediate},
            //{ MJointType.RightMiddleMidCarpalJoint, HumanBodyBones.RightMiddleIntermediate},

            { MJointType.RightRingDistal, HumanBodyBones.RightRingDistal},
            { MJointType.RightRingProximal, HumanBodyBones.RightRingProximal},
            { MJointType.RightRingMeta, HumanBodyBones.RightRingIntermediate},
            //{ MJointType.RightRingMidCarpalJoint, HumanBodyBones.RightRingIntermediate},

            { MJointType.RightThumbCarpal, HumanBodyBones.RightThumbDistal},
            { MJointType.RightThumbTip, HumanBodyBones.RightThumbProximal},
            { MJointType.RightThumbMid, HumanBodyBones.RightThumbIntermediate},



            { MJointType.RightShoulder, HumanBodyBones.RightUpperArm},
            { MJointType.RightElbow, HumanBodyBones.RightLowerArm},
            { MJointType.RightWrist, HumanBodyBones.RightHand},

            { MJointType.LeftHip, HumanBodyBones.LeftUpperLeg},
            { MJointType.LeftKnee, HumanBodyBones.LeftLowerLeg},
            { MJointType.LeftAnkle, HumanBodyBones.LeftFoot},
            { MJointType.LeftBall, HumanBodyBones.LeftToes},

            { MJointType.RightHip, HumanBodyBones.RightUpperLeg},
            { MJointType.RightKnee, HumanBodyBones.RightLowerLeg},
            { MJointType.RightAnkle, HumanBodyBones.RightFoot},
            { MJointType.RightBall, HumanBodyBones.RightToes},
        };


        //To check -> is this required in future?
        public static Dictionary<HumanBodyBones, MJointType> ToJointType = new Dictionary<HumanBodyBones, MJointType>()
        {

            { HumanBodyBones.Hips, MJointType.PelvisCentre},

            //To check
            { HumanBodyBones.Spine,MJointType.S1L5Joint},

            //Tbd LowerThoracicSpine
            //{ MJointType.T12L12Joint, HumanBodyBones.Spine},

            { HumanBodyBones.Chest, MJointType.T1T2Joint},


            { HumanBodyBones.Neck, MJointType.C4C5Joint},
            { HumanBodyBones.Head, MJointType.HeadJoint},
            { HumanBodyBones.LeftEye, MJointType.MidEye},
            { HumanBodyBones.RightEye, MJointType.MidEye},


            { HumanBodyBones.LeftUpperArm, MJointType.LeftShoulder},
            { HumanBodyBones.LeftLowerArm, MJointType.LeftElbow},
            { HumanBodyBones.LeftHand, MJointType.LeftWrist},


            { HumanBodyBones.LeftIndexDistal, MJointType.LeftIndexDistal},
            { HumanBodyBones.LeftIndexProximal, MJointType.LeftIndexProximal},
            { HumanBodyBones.LeftIndexIntermediate, MJointType.LeftIndexMeta},

            { HumanBodyBones.LeftLittleDistal, MJointType.LeftLittleDistal},
            { HumanBodyBones.LeftLittleProximal, MJointType.LeftLittleProximal},
            { HumanBodyBones.LeftLittleIntermediate, MJointType.LeftLittleMeta},

            { HumanBodyBones.LeftMiddleDistal, MJointType.LeftMiddleDistal},
            { HumanBodyBones.LeftMiddleProximal, MJointType.LeftMiddleProximal},
            { HumanBodyBones.LeftMiddleIntermediate,MJointType.LeftMiddleMeta},

            { HumanBodyBones.LeftRingDistal, MJointType.LeftRingDistal},
            { HumanBodyBones.LeftRingProximal,MJointType.LeftRingProximal},
            { HumanBodyBones.LeftRingIntermediate, MJointType.LeftRingMeta},

            { HumanBodyBones.LeftThumbDistal, MJointType.LeftThumbCarpal},
            { HumanBodyBones.LeftThumbProximal, MJointType.LeftThumbTip},
            { HumanBodyBones.LeftThumbIntermediate, MJointType.LeftThumbMid},

            { HumanBodyBones.RightIndexDistal, MJointType.RightIndexDistal},
            { HumanBodyBones.RightIndexProximal, MJointType.RightIndexProximal},
            { HumanBodyBones.RightIndexIntermediate, MJointType.RightIndexMeta},

            { HumanBodyBones.RightLittleDistal, MJointType.RightLittleDistal},
            { HumanBodyBones.RightLittleProximal, MJointType.RightLittleProximal},
            { HumanBodyBones.RightLittleIntermediate, MJointType.RightLittleMeta},

            { HumanBodyBones.RightMiddleDistal, MJointType.RightMiddleDistal},
            { HumanBodyBones.RightMiddleProximal, MJointType.RightMiddleProximal},
            { HumanBodyBones.RightMiddleIntermediate,MJointType.RightMiddleMeta},

            { HumanBodyBones.RightRingDistal, MJointType.RightRingDistal},
            { HumanBodyBones.RightRingProximal,MJointType.RightRingProximal},
            { HumanBodyBones.RightRingIntermediate, MJointType.RightRingMeta},

            { HumanBodyBones.RightThumbDistal, MJointType.RightThumbCarpal},
            { HumanBodyBones.RightThumbProximal, MJointType.RightThumbTip},
            { HumanBodyBones.RightThumbIntermediate, MJointType.RightThumbMid},

            { HumanBodyBones.RightUpperArm,  MJointType.RightShoulder},
            { HumanBodyBones.RightLowerArm, MJointType.RightElbow},
            { HumanBodyBones.RightHand, MJointType.RightWrist},

            { HumanBodyBones.LeftUpperLeg, MJointType.LeftHip},
            { HumanBodyBones.LeftLowerLeg, MJointType.LeftKnee},
            { HumanBodyBones.LeftFoot, MJointType.LeftAnkle},
            { HumanBodyBones.LeftToes, MJointType.LeftBall},

            { HumanBodyBones.RightUpperLeg, MJointType.RightHip},
            { HumanBodyBones.RightLowerLeg, MJointType.RightKnee},
            { HumanBodyBones.RightFoot, MJointType.RightAnkle},
            { HumanBodyBones.RightToes, MJointType.RightBall},
        };
    }
}
