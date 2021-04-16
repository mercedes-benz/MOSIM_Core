// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;


namespace AlignBodyMMU
{
    /// <summary>
    /// Implementation of the AlignBodyMMU that transfers the body from a given start posture to a desired one.
    /// </summary>
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "AlignBodyMMU", "Pose/AlignBody", "", "Implementation of a MMU that transfers from a start body posture to a desired one.", "Implementation of a MMU that transfers from a start body posture to a desired one using motion blending and inverse.")]
    public class AlignBodyMMUImpl : MMUBase
    {
        private MPostureConstraint postureConstraint;
        private TimeSpan elapsed = TimeSpan.Zero;
        private TimeSpan duration = TimeSpan.FromSeconds(0.5f);
        private MInstruction instruction;
        private bool considerRootTransform = false;

        private OperatingMode Mode = OperatingMode.Default;

        /// <summary>
        /// Enum for defining the opration mode of the align body mmu -> in future further modes could be implemented
        /// </summary>
        private enum OperatingMode
        {
            Default,
            BlendToGoalPosture
        };

        /// <summary>
        /// Default constructor
        /// </summary>
        public AlignBodyMMUImpl()
        {
            //Nothing to do
        }

        /// <summary>
        /// Basic initialize method
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            this.SkeletonAccess = new IntermediateSkeleton();

            //First setup the skeleton access
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            //Call the base class method
            return base.Initialize(avatarDescription, properties);
        }

        /// <summary>
        /// Basic assign instruction method
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("Mode", "{Default, BlendToGoalPosture}", "The desired posture", false)]
        [MParameterAttribute("GoalPosture", "MPostureConstraint", "The desired posture", false)]
        [MParameterAttribute("Duration", "float", "The duration until the body alignment is finished", false)]
        [MParameterAttribute("ConsiderRootTransform", "bool", "Flag specifies whether the root transform is considered for aligning the body (if false, only the local joint rotations are used). By default the value is false.", false)]

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Reset all parameters
            this.duration = TimeSpan.FromSeconds(0.5f);
            this.elapsed = TimeSpan.Zero;
            this.instruction = instruction;
            this.considerRootTransform = false;


            //Parse the properties
            if(instruction.Properties != null)
            {
                if (instruction.Properties.ContainsKey("Mode"))
                {
                    if(!Enum.TryParse<OperatingMode>(instruction.Properties["Mode"], out this.Mode))
                    {
                        //Using default mode if parsing fails
                        this.Mode = OperatingMode.Default;
                    }

                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_DEBUG, "Operation mode set: " + this.Mode);

                }

                if (instruction.Properties.ContainsKey("ConsiderRootTransform"))
                    bool.TryParse(instruction.Properties["ConsiderRootTransform"], out this.considerRootTransform);

                //Parse the desired duration
                if (instruction.Properties.ContainsKey("Duration"))
                    this.duration = TimeSpan.FromSeconds(float.Parse(instruction.Properties["Duration"], System.Globalization.CultureInfo.InvariantCulture));

                //Parse the goal posture if defined
                if (instruction.Properties.ContainsKey("GoalPosture"))
                {
                    string constraintID = instruction.Properties["GoalPosture"];

                    try
                    {
                        this.postureConstraint = instruction.Constraints.Find(s => s.ID == constraintID).PostureConstraint;
                        MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_DEBUG, "Using defined goal posture");

                    }
                    catch (Exception)
                    {
                        MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Problem receiving the posture constraint");
                        return new MBoolResponse(false);
                    }
                }
            }


            //Call the base class method
            return base.AssignInstruction(instruction, simulationState);
        }


        /// <summary>
        /// Basic do step routine executed for each frame
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Create a new simulation result
            MSimulationResult result = new MSimulationResult()
            {
                Events = new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),
                SceneManipulations = new List<MSceneManipulation>(),
                Posture = simulationState.Current
            };

            //Increment the elapsed time
            this.elapsed += TimeSpan.FromSeconds(time);

            //Get the current posture
            MAvatarPostureValues currentPosture = simulationState.Initial;

            //Create variable for storing the goal posture
            MAvatarPostureValues goalPosture = null;


            //Set the goal posture based on the operating mode
            switch (this.Mode)
            {
                case OperatingMode.Default:
                    goalPosture = simulationState.Current.Copy();
                    break;

                case OperatingMode.BlendToGoalPosture:
                    goalPosture = postureConstraint.Posture.Copy();
                    break;
            }

                

            //Determine the weight for blending
            float weight = (float)(this.elapsed.TotalSeconds / this.duration.TotalSeconds);


            //To do -> Use ik to move larger distances (especially for feet)


            //Workaround to ignore root transform
            if (!this.considerRootTransform)
            {
                for (int i = 0; i < 7; i++)
                    goalPosture.PostureData[i] = currentPosture.PostureData[i];
            }

            //Perform a simple blending
            result.Posture = MMICSharp.Common.Tools.Blending.PerformBlend(this.SkeletonAccess as IntermediateSkeleton, currentPosture, goalPosture, weight, this.considerRootTransform);


            //Provide finished event
            if(this.elapsed >= this.duration)
                result.Events.Add(new MSimulationEvent("Posture aligned", mmiConstants.MSimulationEvent_End, this.instruction.ID));

            return result;
        }

    }
}
