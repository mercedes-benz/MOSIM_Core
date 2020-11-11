// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace MoveMMU
{
    [Serializable]
    public class HandContainer
    {
        /// <summary>
        /// Indicates whether a both handed move is performed
        /// </summary>
        public bool BothHanded = false;

        /// <summary>
        /// The joint type of the hand
        /// </summary>
        public MJointType Type = MJointType.LeftWrist;

        /// <summary>
        /// The optional trajectory of the hand
        /// </summary>
        public List<MTransform> Trajectory = null;

        /// <summary>
        /// The present trajectory index
        /// </summary>
        public int TrajectoryIndex = 0;

        /// <summary>
        /// The corresponding instruction
        /// </summary>
        public MInstruction Instruction;

        /// <summary>
        /// Indicates whether the instruction execution is currently active
        /// </summary>
        public bool IsActive = false;

        /// <summary>
        /// The velocity of the move operation
        /// </summary>
        public float Velocity = 0.5f;

        /// <summary>
        /// The angular velocity of the move operation
        /// By default it is 0  (not considered)
        /// </summary>
        public float AngularVelocity = 0f;

        /// <summary>
        /// The acceleration
        /// </summary>
        public float Acceleration = 1.0f;

        /// <summary>
        /// The offset of the hand
        /// </summary>
        public MTransform Offset;

        /// <summary>
        /// Optionally defined hold time (waiting n seconds until the end event is provided)
        /// </summary>
        public double HoldTime = 0;

        /// <summary>
        /// The currently elapsed hold time
        /// </summary>
        public double ElapsedHoldTime = 0;

        /// <summary>
        /// Flag indicates whether the hand container is properly initialized
        /// </summary>
        public bool Initialized = false;

        /// <summary>
        /// Flag indicates whether collision avoidance is enabled
        /// </summary>
        public bool CollisionAvoidance = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public HandContainer()
        {
            this.Initialized = false;
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instruction"></param>
        /// <param name="isActive"></param>
        /// <param name="bothHanded"></param>
        public HandContainer(MJointType type, MInstruction instruction, bool isActive, bool bothHanded = false):this()
        {
            this.Type = type;
            this.Instruction = instruction;
            this.IsActive = isActive;
            this.BothHanded = bothHanded;
        }
    }
}
