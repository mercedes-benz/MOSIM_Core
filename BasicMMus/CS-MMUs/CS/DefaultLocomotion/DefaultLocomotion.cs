// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger

using MMICSharp.Common;
using System;
using System.Collections.Generic;
using MMIStandard;
using MMICSharp.Common.Attributes;

namespace ReachMMU
{
    /// <summary>
    /// Implementation of a simple reach MMU
    /// </summary>
    [MMUDescriptionAttribute("Janis Sprenger", "1.0", "DefaultLocomotion", "Locomotion","", "A fallback locomotion MMU.", "A fallback MMU allowing arbritrary global repositioning in space without animations.")]
    public class DefaultLocomotionImpl:MMUBase
    {
        #region private fields
  
        /// <summary>
        /// The stored instruction
        /// </summary>
        private MInstruction instruction;

        /// <summary>
        /// The target transform 
        /// </summary>
        private MTransform targetTransform;

        /// <summary>
        /// The maximum velocity of the reach motion
        /// </summary>
        private float velocity = -1.0f;

        /// <summary>
        /// Basic constructor
        /// </summary>
        public DefaultLocomotionImpl()
        {
            this.Name = "DefaultLocomotion";
            this.MotionType = "locomotion";
        }

        #endregion

        /// <summary>
        /// Initialization method -> just call the base class
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            MBoolResponse res = base.Initialize(avatarDescription, properties);

            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            return res;
        }

        /// <summary>
        /// Method to assign an actual instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        //[MParameterAttribute("TargetID", "ID", "The id of the target location (object)", true)]
        [MParameterAttribute("Velocity", "float", "Specifies the velocity of the character for linear interpolation.", false)]

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        { 
            //Assign the instruction
            this.instruction = instruction;

            MBoolResponse response = new MBoolResponse(true);



            if (instruction.Constraints.Count > 0 && instruction.Constraints[0].GeometryConstraint != null)
            {
                MSceneObject parent = this.SceneAccess.GetSceneObjectByID(instruction.Constraints[0].GeometryConstraint.ParentObjectID);

                if (instruction.Constraints[0].GeometryConstraint.ParentToConstraint != null)
                {
                    MTransform ptc = instruction.Constraints[0].GeometryConstraint.ParentToConstraint;
                    String gtp = ptc.Parent;
                    if (gtp != null && this.SceneAccess.GetSceneObjectByID(gtp) != null)
                    {
                        // transform parent takes precedent. 
                        this.targetTransform = ptc.LocalToGlobal(this.SceneAccess);
                    }
                    else if (parent != null)
                    {
                        // parent to constraint has not valid parent, thus the geometry constraint parent is 
                        this.targetTransform = ptc.Multiply(parent.Transform.LocalToGlobal(this.SceneAccess));
                    }
                    else
                    {
                        this.targetTransform = ptc;
                    }
                }
                else
                {
                    MVector3 pos = new MVector3(0,0,0); 
                    if (instruction.Constraints[0].GeometryConstraint.TranslationConstraint != null)
                    {
                        MTranslationConstraint trlCstr = instruction.Constraints[0].GeometryConstraint.TranslationConstraint;
                        if (parent != null)
                        {
                            pos = parent.Transform.Position.Add(trlCstr.GetVector3());
                        } else
                        {
                            pos = trlCstr.GetVector3();
                        }
                    }
                    MQuaternion rot = new MQuaternion(0,0,0,1);
                    if(instruction.Constraints[0].GeometryConstraint.RotationConstraint != null)
                    {
                        MRotationConstraint rtCstr = instruction.Constraints[0].GeometryConstraint.RotationConstraint;
                        if (parent != null)
                        {
                            rot = rtCstr.GetQuaternion().Multiply(parent.Transform.Rotation);
                        }
                        else
                        {
                            rot = rtCstr.GetQuaternion();
                        }
                    }
                    this.targetTransform = new MTransform("", pos, rot);
                }
                
            } else
            {
                response = new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Required target constraint (MGeometryConstraint) not defined" }
                };
            }



            //Extract the velocity if defined
            if (instruction.Properties.ContainsKey("Velocity"))
            {
                Console.WriteLine("vel: " + instruction.Properties["Velocity"]);
                float.TryParse(instruction.Properties["Velocity"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out velocity);
            } else
            {
                velocity = -1.0f;
            }

            /*
            //Get the target id
            if (instruction.Properties.ContainsKey("TargetID"))
                this.targetTransform = this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);
            //Error id not available
            else
            {
                return new MBoolResponse(false)
                {
                };
            }*/

            //Return true/success
            return response;
        }


        /// <summary>
        /// Do step routine in which the actual simulation result is generated
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
                SceneManipulations = new List<MSceneManipulation>()         
            };

            this.SkeletonAccess.SetChannelData(simulationState.Current.Copy());

            // Target position we want to transform to
            MVector3 targetPos = this.targetTransform.Position;

            // Fully body posture. Only Pelvis Center (first joint) has to be manipulated. 
            List<double> posture = simulationState.Current.PostureData;

            // Current position and distance to target. 
            MVector3 currentPos = this.SkeletonAccess.GetRootPosition(simulationState.Initial.AvatarID);
            MVector3 distance = targetPos.Subtract(currentPos);

            // Current rotation and rotational diff to target
            MQuaternion currentRot = this.SkeletonAccess.GetRootRotation(simulationState.Initial.AvatarID);
            MQuaternion targetRot = this.targetTransform.Rotation;



            MVector3 newPos;

            MVector3 deltaDistance = distance.Clone();


            if (this.velocity > 0)
            {
                deltaDistance = distance.Normalize().Multiply(this.velocity * time);
                Console.WriteLine("Delta v: " + deltaDistance.Magnitude() + " " + time + " " + this.velocity);
            }



            // If no velocity set or distance very close, directly morph to target position and rotation. 
            if (this.velocity <= 0 || distance.Magnitude() < deltaDistance.Magnitude()) {
                newPos = targetPos;

                // Set rotation
                //posture[3] = this.targetTransform.Rotation.W;
                //posture[4] = this.targetTransform.Rotation.X;
                //posture[5] = this.targetTransform.Rotation.Y;
                //posture[6] = this.targetTransform.Rotation.Z;

                // Add end event. 
                Console.WriteLine("Finished with vel " + this.velocity + " at " + distance.Magnitude());
                result.Events.Add(new MSimulationEvent(this.instruction.Name, mmiConstants.MSimulationEvent_End, this.instruction.ID));

            }
            else // if velocity > 0 and distance sufficiently large, we should apply linear translation with the provided velocity. 
            {
                newPos = currentPos.Add(deltaDistance);
                Console.WriteLine("Target Location: " + this.targetTransform.Position + " " + currentPos + " " + distance + " " + deltaDistance + " " + newPos);

            }

            Console.WriteLine("newposrot: " + newPos + " " + targetRot);
            this.SkeletonAccess.SetRootPosition(simulationState.Current.AvatarID, newPos);
            this.SkeletonAccess.SetRootRotation(simulationState.Current.AvatarID, targetRot);

            result.Posture = this.SkeletonAccess.GetCurrentPostureValues(simulationState.Current.AvatarID);

            Console.WriteLine("Frame : " + result.Posture.PostureData[0] + " " + result.Posture.PostureData[1] + " " + result.Posture.PostureData[2] + " " + result.Posture.PostureData[3] + " " + result.Posture.PostureData[4] + " " + result.Posture.PostureData[5] + " " + result.Posture.PostureData[6] + " ");

            //Return the result
            return result;
        }
    }


}
