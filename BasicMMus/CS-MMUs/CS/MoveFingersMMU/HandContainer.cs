// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections.Generic;

namespace MoveFingersMMU
{
    public class HandContainer
    {
        public MJointType Type = MJointType.LeftWrist;

        /// <summary>
        /// The final posture that should be established
        /// </summary>
        public MPostureConstraint FinalPosture;

        /// <summary>
        /// The assigned instruction
        /// </summary>
        public MInstruction Instruction;

        /// <summary>
        /// The angular velocity used for blending
        /// </summary>
        public float AngularVelocity = 90f;

        /// <summary>
        /// The time the blend required (if defined)
        /// </summary>
        public float Duration = 1f;

        /// <summary>
        /// The elapsed time
        /// </summary>
        public float Elapsed = 0;

        /// <summary>
        /// Specifies whether the duration/time is used
        /// </summary>
        public bool HasDuration = false;

        /// <summary>
        /// Indicates whether a release should be performed
        /// </summary>
        public bool Release = false;

        /// <summary>
        /// Flag which indicates whether the positioning is finished
        /// </summary>
        public bool Positioned = false;


        /// <summary>
        /// The current finger rotations
        /// </summary>
        public Dictionary<MJointType, MQuaternion> CurrentFingerRotations = new Dictionary<MJointType, MQuaternion>();

    }
}
