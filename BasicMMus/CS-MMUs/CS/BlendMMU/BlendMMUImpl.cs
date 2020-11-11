// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace BlendMMU
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "BlendMJMU", "Blend", "", "MMU for transition modeling.", "MMU for transition modeling.")]
    public class BlendMMUImpl:MMUBase
    {
        private float blendDuration;
        private float elapsed = 0;
        private MInstruction instruction;

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

        [MParameterAttribute("BlendDuration", "float", "The duration of the blending [s] that is performed.", false)]
        [MParameterAttribute("BlendType", "{ToInitial, ToCurrent}", "The blend type (by default to current).", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            base.AssignInstruction(instruction, simulationState);

            this.instruction = instruction;

            if (instruction.Properties.ContainsKey("BlendDuration"))
                this.blendDuration = float.Parse(instruction.Properties["BlendDuration"], System.Globalization.CultureInfo.InvariantCulture);


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


            elapsed += (float)time;

            float blendWeight = Math.Min(1, elapsed / blendDuration);

            //Perform the actual motion blending
            result.Posture = MMICSharp.Common.Tools.Blending.PerformBlend(this.SkeletonAccess as IntermediateSkeleton, simulationState.Initial, simulationState.Current, blendWeight, true);


            //Provide end event if finished
            if(elapsed >= this.blendDuration)
                result.Events.Add(new MSimulationEvent("Blend finished", mmiConstants.MSimulationEvent_End, this.instruction.ID));


            return result;
        }
    }
}
