// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace DefaultMMU
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "DefaultMMU", "default", "", "A default fallback MMU", "A MMU visualizing text as a fallback mechanism.")]
    public class DefaultMMUImpl:MMUBase
    {
        private const string durationKey = "duration";
        private const string textKey = "text";



        private TimeSpan duration;
        private TimeSpan elapsed;
        private string text = "";
        private MInstruction instruction;


        [MParameterAttribute(textKey, "string", "The fallback text which should be visualized.", false)]
        [MParameterAttribute(durationKey, "double[ms]", "The duration the MMU takes until the end event is provided.", false)]

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Set elapsed to zero
            this.elapsed = TimeSpan.Zero;
            this.instruction = instruction;

            //Parse the parameters
            if (instruction.Properties != null)
            {
                if (instruction.Properties.ContainsKey(durationKey))
                {
                    this.duration = TimeSpan.FromSeconds(double.Parse(instruction.Properties[durationKey]));
                }

                if (instruction.Properties.ContainsKey(textKey))
                {
                    this.text = instruction.Properties[textKey];
                }
            }
            
            return base.AssignInstruction(instruction, simulationState);
        }


        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            MSimulationResult result = new MSimulationResult()
            {
                Constraints = simulationState.Constraints,
                Events = simulationState.Events?? new List<MSimulationEvent>(),
                Posture = simulationState.Current,
                SceneManipulations = simulationState.SceneManipulations,
                DrawingCalls = new List<MDrawingCall>()
            };


            this.elapsed += TimeSpan.FromSeconds(time);

            //Add a drwaing call
            result.DrawingCalls.Add(new MDrawingCall(MDrawingCallType.DrawText)
            {
                 Properties = new Dictionary<string, string>()
                 {
                     {"text", this.text + this.elapsed.TotalSeconds + " s" }
                 }
            });


            //Create the finished event
            if(this.elapsed > this.duration)
            {
                result.Events.Add(new MSimulationEvent("Finished", mmiConstants.MSimulationEvent_End, this.instruction.ID));
            }


            return result;
        }

    }
}
