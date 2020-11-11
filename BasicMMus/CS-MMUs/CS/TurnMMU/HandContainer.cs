// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace TurnMMU
{
    [Serializable]
    public class HandContainer
    {
        public bool BothHanded = false;

        public MSceneObject Subject;
        public MSceneObject Target;


        public MJointType JointType;

        public List<MTransform> Trajectory = null;

        public int TrajectoryIndex = 0;

        public MInstruction Instruction;

        public bool IsActive = false;

        public float Velocity = 0.5f;

        public float Acceleration = 1.0f;

        public MTransform Offset;

        public double HoldTime = 0;

        public double ElapsedHoldTime = 0;

        public bool Initialized = false;

        public int Repetitions = 1;

        public float AngularVelocity = 60f;

        public float TurningAngle;

        public bool AngleIntervalDefined = false;
        public float MinAngle = 0;
        public float MaxAngle = 0;

        public bool FixFingerTransformations = false;

        public MTransform TurningAxis;

        public HandContainer()
        {

        }

        public HandContainer(MJointType type, MInstruction instruction, bool isActive, bool bothHanded = false)
        {
            this.JointType = type;
            this.Instruction = instruction;
            this.IsActive = isActive;
            this.BothHanded = bothHanded;
            this.Initialized = false;
        }
    }

    public enum HandType
    {
        Left,
        Right
    }
}