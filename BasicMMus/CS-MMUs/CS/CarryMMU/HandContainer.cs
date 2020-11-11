// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace CarryMMU
{
    [Serializable]
    public class HandContainer
    {

        public string ConstraintID;


        /// <summary>
        /// The current carry state of the respective hand
        /// </summary>
        public CarryState State = CarryState.None;

        /// <summary>
        /// The hand type of the hand either left or right
        /// </summary>
        public HandType Type = HandType.Left;

        public MEndeffectorType IKEndeffectorType
        {
            get
            {
                switch (this.Type)
                {
                    case HandType.Left:
                        return MEndeffectorType.LeftHand;
                    case HandType.Right:
                        return MEndeffectorType.RightHand;
                }

                return MEndeffectorType.Root;
            }
        }

        public MJointType JointType
        {
            get
            {
                switch (this.Type)
                {
                    case HandType.Left:
                        return MJointType.LeftWrist;
                    case HandType.Right:
                        return MJointType.RightWrist;
                }

                return MJointType.Undefined;
            }
        }

        /// <summary>
        /// The assigned instruction
        /// </summary>
        public MInstruction Instruction;

        public bool IsActive = false;

        /// <summary>
        /// The assigned velocity of the hand
        /// </summary>
        public float Velocity = 1.0f;

        /// <summary>
        /// The offset of the hand relative to the object
        /// </summary>
        public MTransform HandOffset;

        /// <summary>
        /// The offset of the object relative to the hand
        /// </summary>
        public MTransform ObjectOffset;

        /// <summary>
        /// The assigned hand pose
        /// </summary>
        public MTransform HandPose;

        /// <summary>
        /// The name of the carry target (if defined)
        /// </summary>
        public string CarryTargetName;

        /// <summary>
        /// The elapsed time of the blend
        /// </summary>
        public double ElapsedBlendTime = 0;

        /// <summary>
        /// The specified blend duration
        /// </summary>
        public double BlendDuration = 1.0f;

        /// <summary>
        /// The start posture for blending
        /// </summary>
        public MAvatarPostureValues BlendStartPosture;


        public HandContainer()
        {

        }

        public HandContainer(HandType type, MInstruction instruction, bool isActive)
        {
            this.Type = type;
            this.Instruction = instruction;
            this.IsActive = isActive;
            this.ConstraintID = "Carry:"+type+":"+System.Guid.NewGuid().ToString();
        }

        //public HandContainer(SerializableHand data)
        //{
        //    this.Instruction = data.Instruction;
        //    this.ObjectOffset = new MTransform();
        //    this.ObjectOffset.FromArray(data.InverseOffset);
        //    this.IsActive = data.IsActive;
        //    this.HandOffset = new Pose();
        //    this.HandOffset.FromArray(data.Offset);
        //    this.Type = data.Type;
        //    this.Velocity = data.Velocity;
        //    //Tbd
        //}
    }

    #region serialization

    [Serializable]
    public class SerializableState
    {
        public List<SerializableHand> ActiveHands = new List<SerializableHand>();

        public MSimulationState AvatarState;
    }



    [Serializable]
    public class SerializableHand
    {
        public bool BothHanded = false;

        public HandType Type = HandType.Left;

        public MInstruction Instruction;

        public bool IsActive = false;

        public float Velocity = 1.5f;

        public float[] Offset;

        public float[] InverseOffset;

        public SerializableHand()
        {

        }

        //public SerializableHand(HandContainer hand)
        //{
        //    this.Instruction = hand.Instruction;
        //    this.InverseOffset = hand.ObjectOffset.ToArray();
        //    this.IsActive = hand.IsActive;
        //    this.Offset = hand.HandOffset.ToArray();
        //    this.Type = hand.Type;
        //    this.Velocity = hand.Velocity;
        //}

    }

    #endregion
}
