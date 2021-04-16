// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Adapter;
using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using ReachMMU;
using System;
using System.Collections.Generic;

namespace ReachMMUConcurrent
{

    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(ReachMMUConcurrentImpl)))
            {
                Console.ReadLine();
            }
        }

    }


    /// <summary>
    /// MMU implements a concurrent reach motion for both hands. Internally the MMU builds upon the single handed reach MMU (ReachMMUImpl).
    /// </summary>
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "ReachMMUConcurrent", "Pose/Reach", "", "A reach MMU which allows concurrent states.", "A reach MMU realized using inverse kinematics. The MMU allows the execution of left and right hand reaches simultaniously.")]
    public class ReachMMUConcurrentImpl:MMUBase
    {
        /// <summary>
        /// Respective instance for the left/right hand
        /// </summaryReachMMUImpl
        private Dictionary<MInstruction, ReachMMUImpl> mmuInstances = new Dictionary<MInstruction, ReachMMUImpl>();

        /// <summary>
        /// The assigned instructions
        /// </summary>
        private List<MInstruction> instructions = new List<MInstruction>();

        /// <summary>
        /// The minmum reach distance after which the reach can be started
        /// </summary>
        private float minReachDistance = 1.2f;

        private readonly float minDistanceDefault = 1.2f;

        /// <summary>
        /// Flag specifies whether debug outputs should be generated
        /// </summary>
        private bool debug = true;


        /// <summary>
        /// Default initialize method
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            if (this.SkeletonAccess == null)
            {
                //Create the skeleton access
                this.SkeletonAccess = new IntermediateSkeleton();
                this.SkeletonAccess.InitializeAnthropometry(avatarDescription);
            }


            return base.Initialize(avatarDescription, properties);
        }


        /// <summary>
        /// Basic assign instruction method
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the target location (object) or MGeometryConstraint", true)]
        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the reach motion", true)]
        [MParameterAttribute("SingleShotIK", "bool", "Specifies if the ik is used once for the initial computation and blending is carried out afterwards.", false)]
        [MParameterAttribute("Velocity", "float", "Specifies the velocity of the reaching.", false)]
        [MParameterAttribute("AngularVelocity", "float", "Specifies the angular velocity of the reaching.", false)]
        [MParameterAttribute("MinDistance", "float", "Specifies the minmum distance at which the reaching can be started (used for check prerequisites).", false)]
        [MParameterAttribute("Debug", "bool", "Specifies wheather debug output should be displayed.", false)]
        [MParameterAttribute("Trajectory", "MPathConstraint", "Optionally defined trajectory for reaching.", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //To do -> Check whether the execution is allowed
            ReachMMUImpl instance = new ReachMMUImpl
            {
                SceneAccess = this.SceneAccess,
                ServiceAccess = this.ServiceAccess,
                SkeletonAccess = this.SkeletonAccess
            };

            //Get the min distance parameter
            if (instruction.Properties != null)
            {
                instruction.Properties.GetValue(out minReachDistance, "MinDistance");
                instruction.Properties.GetValue(out debug, "Debug");
            }

            //Call the instance responsible for the left/right arm
            instance.Initialize(this.AvatarDescription, new Dictionary<string, string>());
            instance.AssignInstruction(instruction, simulationState);


            //Add the instructions and the mmu instance
            instructions.Add(instruction);
            mmuInstances.Add(instruction, instance);


            return new MBoolResponse(true);
        }


        /// <summary>
        /// Basic do step routine that is executed for each frame to compute the actual motion
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //The simulation result which is provided as overall result
            MSimulationResult result = new MSimulationResult()
            {
                Posture = simulationState.Current,
                Events = new List<MSimulationEvent>(),
            };

            //Handle each active MMU (each instruction coressponds to one MMU)
            for(int i= instructions.Count-1;i>=0;i--)
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
                result.DrawingCalls ?.AddRange(localResult.DrawingCalls);

                //Add the events
                if (localResult.Events != null && localResult.Events.Count > 0)
                    result.Events.AddRange(localResult.Events);

                if (localResult.Events.Exists(s=>s.Type == mmiConstants.MSimulationEvent_End && s.Reference == instructions[i].ID))
                {
                    //Remove the respective MMU
                    mmuInstances.Remove(instructions[i]);

                    //Remove from the list
                    instructions.RemoveAt(i);
                }

            }

            return result;
        }


        /// <summary>
        /// Method aborts the specified instruction
        /// </summary>
        /// <param name="instructionID"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Implementation of the check prerequisites method which is used by the co-simulation to determine whether the MMU can be started
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public override MBoolResponse CheckPrerequisites(MInstruction instruction)
        {

            //Get the min distance parameter
            if (instruction.Properties != null && instruction.Properties.ContainsKey("MinDistance"))
                instruction.Properties.GetValue(out minReachDistance, "MinDistance");
            else
                minReachDistance = minDistanceDefault;

            if (instruction.Properties.ContainsKey("TargetID"))
            {
                MSceneObject sceneObject = this.SceneAccess.GetSceneObjectByID(instruction.Properties["TargetID"]);

                MAvatar avatar = this.SceneAccess.GetAvatarByID(this.AvatarDescription.AvatarID);

                if(sceneObject !=null && avatar != null)
                {
                    this.SkeletonAccess.SetChannelData(avatar.PostureValues);

                    //Get the root position
                    MVector3 rootPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.PelvisCentre);
                    rootPosition.Y = 0;

                    //Get the object position
                    MVector3 objectPosition = new MVector3(sceneObject.Transform.Position.X, 0, sceneObject.Transform.Position.Z);

                    //Compute the distance between root and object
                    float distance = rootPosition.Subtract(objectPosition).Magnitude();

                    //Check if below distance
                    if(distance < this.minReachDistance)
                    {
                        if(this.debug)
                            Logger.Log(Log_level.L_DEBUG, $"Check prerequisites of reach successfull! Distance: {distance}/{minReachDistance}");

                        return new MBoolResponse(true);
                    }
                    else
                    {
                        if (this.debug)
                            Logger.Log(Log_level.L_DEBUG, $"Check prerequisites of reach failed! Distance: {distance}/{minReachDistance}");

                        return new MBoolResponse(false);

                    }

                }
            }

            return new MBoolResponse(true);
        }

    }


}
