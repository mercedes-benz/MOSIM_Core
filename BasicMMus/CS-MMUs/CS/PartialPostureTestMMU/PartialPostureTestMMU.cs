// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace PartialPosture
{
    [MMUDescriptionAttribute("Janis Sprenger", "1.0", "PartialPostureTestMMU", "PartialPostureTest", "", "MMU for testing the partial posture constraints.", "MMU for testing the partial posture constraints.")]
    public class PartialPostureTestMMU:MMUBase
    {
        private float blendDuration;
        private float elapsed = 0;
        private MInstruction instruction;
        private int frames = 0;

        /// <summary>
        /// Initialization method -> just call the base class
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            MBoolResponse response = base.Initialize(avatarDescription, properties);

            //Initialize the skeleton access
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            return response;
        }

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            base.AssignInstruction(instruction, simulationState);
            this.instruction = instruction;
            this.frames = 0;
            return new MBoolResponse(true);
        }


        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {

            //Create a default result
            MSimulationResult result = new MSimulationResult()
            {
                Events = new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),
                SceneManipulations = new List<MSceneManipulation>(),
                Posture = simulationState.Current
            };
            if(this.frames == 0)
            {
                var data = new List<double>() { 0.707, 0.0, 0.0, 0.707 };
                MAvatarPostureValues constraintPosture = new MAvatarPostureValues(result.Posture.AvatarID, data);
                constraintPosture.PartialJointList = new List<MJointType>() { MJointType.LeftKnee };
                MConstraint c = new MConstraint("C_Posture_");
                c.PostureConstraint = new MPostureConstraint(constraintPosture);

                result.Constraints.Add(c);
            } else
            {
                result.Events.Add(new MSimulationEvent("EndTest", mmiConstants.MSimulationEvent_End, this.instruction.ID));
            }
            this.frames++;
            return result;
        }
    }
}
