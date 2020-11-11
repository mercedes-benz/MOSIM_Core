using DebugAdapter;
using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using ReleaseMMU;
using System;
using System.Collections.Generic;

namespace ReleaseMMUConcurrent
{

    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(ReleaseMMUConcurrentImpl)))
            {
                Console.ReadLine();
            }
        }

    }

    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "ReleaseMMU", "Object/Release", "", "A release MMU which models the release of an object.", "A release MMU realized using motion blending.")]
    public class ReleaseMMUConcurrentImpl: MMUBase
    {
        /// <summary>
        /// Respective instance for the left/right hand
        /// </summaryReachMMUImpl
        private Dictionary<MInstruction, ReleaseMMUImpl> mmuInstances = new Dictionary<MInstruction, ReleaseMMUImpl>();

        private List<MInstruction> instructions = new List<MInstruction>();


        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            return base.Initialize(avatarDescription, properties);
        }

        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the release motion", true)]
        [MParameterAttribute("Velocity", "float", "The max velocity of the release motion", false)]
        [MParameterAttribute("AngularVelocity", "float", "The max angular velocity of the release motion", false)]
        [MParameterAttribute("EndBlendDuration", "float", "The duration of the blending that is performed after the ik reached the goal (smooth transition)", false)]
        [MParameterAttribute("UseBlending", "bool", "Specifies whether motion blending is used for modeling the release motion (by default false)", false)]
        [MParameterAttribute("Trajectory", "MPathConstraint", "An optional velocity for the release motion", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //To do -> Check whether the execution is allowed
            ReleaseMMUImpl instance = new ReleaseMMUImpl
            {
                SceneAccess = this.SceneAccess,
                ServiceAccess = this.ServiceAccess,
                SkeletonAccess = this.SkeletonAccess
            };

            //Call the instance responsible for the left/right arm
            instance.Initialize(this.AvatarDescription, new Dictionary<string, string>());
            instance.AssignInstruction(instruction, simulationState);


            //Add the instructions and the mmu instance
            instructions.Add(instruction);
            mmuInstances.Add(instruction, instance);


            return new MBoolResponse(true);
        }


        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //The simulation result which is provided as overall result
            MSimulationResult result = new MSimulationResult()
            {
                Posture = simulationState.Current,
                Events = new List<MSimulationEvent>(),
            };

            //Handle each active MMU (each instruction coressponds to one MMU)
            for (int i = instructions.Count - 1; i >= 0; i--)
            {
                //Update the simulation state
                MSimulationResult localResult = mmuInstances[instructions[i]].DoStep(time, simulationState);

                //Update the simulation state
                //simulationState.Current = localResult.Posture;
                simulationState.Constraints = localResult.Constraints;

                //Write the result
                result.Constraints = localResult.Constraints;
                result.Posture = localResult.Posture;

                //Merge the scene manipulations
                result.SceneManipulations?.AddRange(localResult.SceneManipulations);

                //Merge the drawing calls
                result.DrawingCalls?.AddRange(localResult.DrawingCalls);

                //Add the events
                if (localResult.Events != null && localResult.Events.Count > 0)
                    result.Events.AddRange(localResult.Events);

                //Check if finished
                if (localResult.Events.Exists(s => s.Type == mmiConstants.MSimulationEvent_End && s.Reference == instructions[i].ID))
                {
                    //Remove the respective MMU
                    mmuInstances.Remove(instructions[i]);

                    //Remove from the list
                    instructions.RemoveAt(i);
                }

            }

            return result;
        }

        public override MBoolResponse Abort(string instructionID = null)
        {
            if (instructionID != null)
            {
                MInstruction instruction = this.instructions.Find(S => S.ID == instructionID);

                if (instruction != null)
                {
                    if (mmuInstances.ContainsKey(instruction))
                        mmuInstances.Remove(instruction);

                    if (instructions.Contains(instruction))
                        instructions.Remove(instruction);
                }
            }

            return base.Abort(instructionID);
        }
    }




}
