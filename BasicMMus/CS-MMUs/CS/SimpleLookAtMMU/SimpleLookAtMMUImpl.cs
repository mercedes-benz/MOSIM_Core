// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using System;
using System.Collections.Generic;
using MMIStandard;
using MMICSharp.Common.Tools;
using MMICSharp.Common.Attributes;
using System.Linq;

namespace ReachMMU
{
    /// <summary>
    /// Implementation of a simple reach MMU
    /// </summary>
    [MMUDescriptionAttribute("Janis Sprenger", "1.0", "SimpleLookAtMMU", "Pose/LookAt", "", "A LookAt MMU", "A LookAt MMU realized using pure mathematics.")]
    public class SimpleLookAtMMU:MMUBase
    {
        #region private fields

        /// <summary>
        /// The stored instruction
        /// </summary>
        private MInstruction instruction;

        /// <summary>
        /// The target transform of the hand 
        /// </summary>
        private MTransform targetTransform;

        /// <summary>
        /// Basic constructor
        /// </summary>
        public SimpleLookAtMMU()
        {
            this.Name = "SimpleLookAtMMU";
            this.MotionType = "Pose/LookAt";
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
            MBoolResponse response = base.Initialize(avatarDescription, properties);

            //Setup the skeleton access
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            return response;
        }


        /// <summary>
        /// Method to assign an actual instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the target location (object) or MGeometryConstraint", true)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Assign the instruction
            this.instruction = instruction;

            //Parse the parameters
            MBoolResponse result = this.ParseParameters(instruction);

            if (!result.Successful)
                return result;
        

            //Return true/success
            return new MBoolResponse(true);
        }


        private void EyeLookAt(string avatarID, MJointType eye)
        {
            // Compute Global look-at rotation
            MQuaternion rotation;
            MVector3 eyePos = this.SkeletonAccess.GetGlobalJointPosition(avatarID, eye);
            MQuaternion eyeRot = this.SkeletonAccess.GetGlobalJointRotation(avatarID, eye);
            MVector3 targetPos = this.targetTransform.Position;
            MVector3 gazeDir = targetPos.Subtract(eyePos);
            MVector3 gaze = new MVector3(1, 0, 0);
            gaze = eyeRot.Multiply(gaze);
            MQuaternion rot = MVector3Extensions.FromToRotation(gaze, gazeDir);
            rotation = rot.Multiply(eyeRot);

            // Set Global Look At Rotation
            this.SkeletonAccess.SetGlobalJointRotation(avatarID, eye, rotation);
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
                SceneManipulations = new List<MSceneManipulation>(),
                Posture = simulationState.Current
            };

            //Compute the target transform at the beginning of each frame
            this.targetTransform = this.ComputeTargetTransform();

            //Set the channel data to the approved state of the last frame (all MMUs were executed including the low prio grasp/positioning)
            this.SkeletonAccess.SetChannelData(simulationState.Current);

            EyeLookAt(simulationState.Initial.AvatarID, MJointType.LeftEye);
            EyeLookAt(simulationState.Initial.AvatarID, MJointType.RightEye);

            result.Posture = this.SkeletonAccess.RecomputeCurrentPostureValues(simulationState.Current.AvatarID);

            //Return the result
            return result;
        }


        /// <summary>
        /// Methods parses the parameters defined in the MInstruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private MBoolResponse ParseParameters(MInstruction instruction)
        { 
            //Extract the information with regard to the target
            if (instruction.Properties.ContainsKey("TargetID"))
            {
                //Use the constraint (if defined)
                if (instruction.Constraints != null && instruction.Constraints.Exists(s => s.ID == instruction.Properties["TargetID"]))
                {

                    MConstraint match = instruction.Constraints.Find(s => s.ID == instruction.Properties["TargetID"]);
                    this.targetTransform = new MTransform()
                    {
                        ID = "target",
                        Position = match.GeometryConstraint.GetGlobalPosition(this.SceneAccess),
                        Rotation = match.GeometryConstraint.GetGlobalRotation(this.SceneAccess)
                    };

                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_DEBUG, "Using MGeometryConstraint for target definition");
                }

                //Gather from the scene
                else
                {
                    this.targetTransform = this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);
                }
            }
            //Error id not available
            else
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Required parameter Target ID not defined" }
                };
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Computes the root velocity of the avatar
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private float ComputeRootVelocity(double time, MSimulationState simulationState)
        {
            //Get the root position
            this.SkeletonAccess.SetChannelData(simulationState.Initial);
            var currentRootPosition = this.SkeletonAccess.GetRootPosition(this.AvatarDescription.AvatarID);

            this.SkeletonAccess.SetChannelData(simulationState.Current);
            var previousRootPosition = this.SkeletonAccess.GetRootPosition(this.AvatarDescription.AvatarID);


            previousRootPosition.Y = 0;
            currentRootPosition.Y = 0;

            //Estimate the root velocity
            return previousRootPosition.Subtract(currentRootPosition).Magnitude() / (float)time;
        }


        /// <summary>
        /// Performs local motion planning to reach the defined point
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="time"></param>
        /// <param name="currentPosition"></param>
        /// <param name="currentRotation"></param>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <returns></returns>
        private MTransform DoLocalMotionPlanning(double velocity, double angularVelocity, TimeSpan time, MVector3 currentPosition, MQuaternion currentRotation, MVector3 targetPosition, MQuaternion targetRotation)
        {
            //Create a new transform representing the result
            MTransform result = new MTransform();

            //Estimate the vector to reach the goal
            MVector3 delta = targetPosition.Subtract(currentPosition);
            float distance = delta.Magnitude();

            //Determine the angular distance
            double angle = Math.Abs(MQuaternionExtensions.Angle(currentRotation, targetRotation));


            //Determine the max translation delta and max angle
            double maxTranslationDelta = velocity * time.TotalSeconds;
            double maxAngle = angularVelocity * time.TotalSeconds;

            //Compute the translation weight
            float translationWeight = (float)Math.Min(1, maxTranslationDelta / delta.Magnitude());

            //Compute the rotation weight
            float rotationWeight = (float)Math.Min(1, maxAngle / angle);

            //Limit the translation
            if (delta.Magnitude() >= maxTranslationDelta)
            {
                delta = delta.Normalize();
                delta = delta.Multiply(maxTranslationDelta);
            }

 
            //Compute the new position
            result.Position = currentPosition.Add(delta);

            if(angularVelocity == 0)
                result.Rotation = MQuaternionExtensions.Slerp(currentRotation, targetRotation, translationWeight);

            else
                result.Rotation = MQuaternionExtensions.Slerp(currentRotation, targetRotation, rotationWeight);

 
            return result;
        }


        /// <summary>
        /// Dynamically estimates the target transform
        /// </summary>
        /// <returns></returns>
        private MTransform ComputeTargetTransform()
        {
            MTransform target = new MTransform()
            {
                ID = "target"
            };

            //Use the constraint (if defined)
            if (instruction.Constraints != null && instruction.Constraints.Exists(s => s.ID == instruction.Properties["TargetID"]))
            {
                MConstraint match = instruction.Constraints.Find(s => s.ID == instruction.Properties["TargetID"]);

                //Compute the global position and rotation of the geometry constraint
                target.Position = match.GeometryConstraint.GetGlobalPosition(this.SceneAccess);
                target.Rotation = match.GeometryConstraint.GetGlobalRotation(this.SceneAccess);
            }

            //Gather from the scene
            else
                target = this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);

            return target;
        }
    }
}
