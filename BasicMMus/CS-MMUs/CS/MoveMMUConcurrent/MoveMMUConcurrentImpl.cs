// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using MoveMMU;
using System.Collections.Generic;

namespace MoveMMUConcurrent
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "MoveMMUConcurrent", "Object/Move", "moveConcurrent", "A move MMU which allows concurrent instructions.", "A move MMU realized using inverse kinematics which allows concurrent instruction handling.")]
    public class MoveMMUConcurrentImpl : MMUBase
    {
        /// <summary>
        /// Respective instance for the left/right hand
        /// </summaryReachMMUImpl
        private Dictionary<MInstruction, MoveMMUImpl> mmuInstances = new Dictionary<MInstruction, MoveMMUImpl>();

        private List<MInstruction> instructions = new List<MInstruction>();


        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            return base.Initialize(avatarDescription, properties);
        }


        /// <summary>
        /// Method to assign an actual instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("objectID", "ID", "The id of the target location (object)", false)]
        [MParameterAttribute("subjectID", "ID", "The id of the target location (object)", false)]
        [MParameterAttribute("hand", "{Left,Right}", "The hand of the carry motion", true)]
        [MParameterAttribute("trajectory", "ID of the MTrajectory constraint", "An optionally specified trajectory.", false)]
        [MParameterAttribute("maxVelocity", "float", "An optionall defined velocity.", false)]
        ///Lecacy
        [MParameterAttribute("TargetID", "ID", "The id of the target location (object)", false)]
        [MParameterAttribute("SubjectID", "ID", "The id of the object which should be moved", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //To do -> Check whether the execution is allowed
            MoveMMUImpl instance = new MoveMMUImpl
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


            return new MBoolResponse();
        }


        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //The simulation result which is provided as overall result
            MSimulationResult result = new MSimulationResult()
            {
                Posture = simulationState.Current,
                Events = new List<MSimulationEvent>(),
                SceneManipulations = new List<MSceneManipulation>()
            };

            //Handle each active MMU (each instruction coressponds to one MMU)
            for (int i = instructions.Count - 1; i >= 0; i--)
            {
                //Update the simulation state
                MSimulationResult localResult = mmuInstances[instructions[i]].DoStep(time, simulationState);

                //Update the simulation state
                simulationState.Current = localResult.Posture;
                simulationState.Constraints = localResult.Constraints;

                //Write the result
                result.Constraints = localResult.Constraints;
                result.Posture = localResult.Posture;

                //Merge the scene manipulations
                result.SceneManipulations?.AddRange(localResult.SceneManipulations);

                //Merge the drawing calls
                result.DrawingCalls?.AddRange(localResult.DrawingCalls);

                //Add the events
                if(localResult.Events!=null && localResult.Events.Count >0)
                    result.Events.AddRange(localResult.Events);

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
