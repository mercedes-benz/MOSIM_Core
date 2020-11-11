// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMICSharp.Common.Tools;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReleaseMMU
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.1", "ReleaseMMU", "Object/Release","", "A release MMU which models the release of an object.", "A release MMU realized using inverse kinematics. Within the final step of the approach, motion blending is used to ensure smoothness to the underlying posture.")]
    public class ReleaseMMUImpl:MMUBase
    {
        #region private members

        /// <summary>
        /// The stored instruction
        /// </summary>
        private MInstruction instruction;

        /// <summary>
        /// The desired hand joint
        /// </summary>
        private MJointType handJoint;

        /// <summary>
        /// The moving velocity of the hand
        /// </summary>
        private float velocity = 1f;

        /// <summary>
        /// The angular velocity of the reach
        /// </summary>
        private float angularVelocity = 0;

        /// <summary>
        /// Create a new constraint manager
        /// </summary>
        private ConstraintManager constraintManager;

        /// <summary>
        /// Flag specifies whether ik is used for modeling the release motion
        /// </summary>
        private bool useIK = true;


        private enum ReleaseMotionState
        {
            IK,
            Blending
        }

        private ReleaseMotionState state = ReleaseMotionState.IK;

        /// <summary>
        /// The time used for the end blend (transition modeling)
        /// </summary>
        private float endBlendDuration = 0.3f;
        private float elapsedBlendTime = 0f;


        private List<MTransform> trajectory = new List<MTransform>();
        private int trajectoryIndex = 0;
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

            //Initialize the skeleton access
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            return response;
        }

        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the release motion", true)]
        [MParameterAttribute("Velocity", "float", "The max velocity of the release motion", false)]
        [MParameterAttribute("Trajectory", "MPathConstraint", "An optional trajectory for the release motion", false)]

        [MParameterAttribute("AngularVelocity", "float", "The max angular velocity of the release motion", false)]
        [MParameterAttribute("EndBlendDuration", "float", "The duration of the blending that is performed after the ik reached the goal (smooth transition)", false)]
        [MParameterAttribute("UseBlending", "bool", "Specifies whether motion blending is used for modeling the release motion (by default false)", false)]

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState avatarState)
        {
            //Initialize the ik service
            this.ServiceAccess.IKService.Setup(this.AvatarDescription, new Dictionary<string, string>());

            //Create a new constraint manager
            this.constraintManager = new ConstraintManager(this.SceneAccess);

            //Assign the instruction
            this.instruction = instruction;

            //Set state to ik
            this.state = ReleaseMotionState.IK;
            this.trajectoryIndex = 0;

            //Parse the parameters
            MBoolResponse response = this.ParseParameters(instruction);

            if (!response.Successful)
                return response;


            return new MBoolResponse(true);
        }


        /// <summary>
        /// Basic do step routine that is executed for each frame and generates the actual motion.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            if (this.useIK)
                return this.DoStepIK(time,simulationState);
            else
                return this.DoStepBlending(time, simulationState);
        }


        /// <summary>
        /// Method parses the parameters
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private MBoolResponse ParseParameters(MInstruction instruction)
        {
            MBoolResponse response = new MBoolResponse(true);

            //Get the target hand
            if (instruction.Properties.ContainsKey("Hand"))
            {
                if (instruction.Properties["Hand"] == "Left")
                    this.handJoint = MJointType.LeftWrist;

                if (instruction.Properties["Hand"] == "Right")
                    this.handJoint = MJointType.RightWrist;
            }
            //Error target hand not specified
            else
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Required parameter hand not defined" }
                };
            }


            if (instruction.Properties.ContainsKey("Velocity"))
                this.velocity = float.Parse(instruction.Properties["Velocity"], System.Globalization.CultureInfo.InvariantCulture);

            if (instruction.Properties.ContainsKey("AngularVelocity"))
                this.angularVelocity = float.Parse(instruction.Properties["AngularVelocity"], System.Globalization.CultureInfo.InvariantCulture);

            if (instruction.Properties.ContainsKey("UseBlending"))
            {
                bool.TryParse("UseBlending", out bool useBlending);
                this.useIK = !useBlending;
            }

            if (instruction.Properties.ContainsKey("EndBlendDuration"))
                this.endBlendDuration = float.Parse(instruction.Properties["EndBlendDuration"], System.Globalization.CultureInfo.InvariantCulture);

            //First extract all parameters
            if (instruction.Properties.ContainsKey("Trajectory"))
            {
                string pathConstraintID = instruction.Properties["Trajectory"];


                if (instruction.Constraints != null || instruction.Constraints.Where(s => s.PathConstraint != null && s.ID == pathConstraintID).Count() == 0)
                {
                    //Get the path constraint
                    MPathConstraint pathConstraint = instruction.Constraints.Find(s => s.PathConstraint != null && s.ID == pathConstraintID).PathConstraint;

                    //Extract the trajectory
                    this.trajectory = pathConstraint.GetMTransformList();

                    //Add the target transform
                    this.trajectory.Add(new MTransform("targetTransform", new MVector3(0, 0, 0), new MQuaternion(0, 0, 0, 1)));

                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, $"Assigned hand trajectory. Number elements: {trajectory.Count}, hand: {this.handJoint}");
                }
                else
                {
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, $"Cannot assign trajectory of hand: {this.handJoint}. No suitable MPathConstraint available.");

                }
            }

            return response;
        }


        /// <summary>
        /// Basic do step routine that is executed for each frame and generates the actual motion.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        private MSimulationResult DoStepIK(double time, MSimulationState simulationState)
        {
            //Create a default result
            MSimulationResult result = new MSimulationResult()
            {
                Events = new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),
                SceneManipulations = new List<MSceneManipulation>(),
                Posture = simulationState.Current
            };

            //The presently active constraints
            List<MConstraint> globalConstraints = result.Constraints;

            //The local constraints defined within the MMU 
            List<MConstraint> localConstraints = new List<MConstraint>();

            //Use the constraint manager to manage the local constraints
            constraintManager.SetConstraints(ref localConstraints);

            //Set the channel data to the approved state of the last frame (all MMUs were executed including the low prio grasp/positioning)
            this.SkeletonAccess.SetChannelData(simulationState.Initial);

            //Get the current hand position and rotation
            MVector3 currentHandPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, this.handJoint);
            MQuaternion currentHandRotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, this.handJoint);

            //Set the skeleton acess data to the current
            this.SkeletonAccess.SetChannelData(simulationState.Current);



            //Determine the target hand position (either underlying MMU or given via boundary constraints)
            MTransform targetTransform = new MTransform()
            {
                Position = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, this.handJoint),
                Rotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, this.handJoint)
            };


            MTransform nextPose = null;
            float translationWeight = 0;


            //Use the trajectory if defined
            if (this.trajectory != null && this.trajectory.Count>0)
            {
                //Update the last element dynamically
                trajectory.Last().Position = targetTransform.Position;
                trajectory.Last().Rotation = targetTransform.Rotation;

                //Compute the next pose
                nextPose = this.DoLocalMotionPlanning(this.velocity, this.angularVelocity, TimeSpan.FromSeconds(time), currentHandPosition, currentHandRotation, this.trajectory[trajectoryIndex].Position, this.trajectory[trajectoryIndex].Rotation, out translationWeight);

                //Check if close to current target -> move to next target -> To do consider rotation
                if ((nextPose.Position.Subtract(trajectory[trajectoryIndex].Position)).Magnitude() < 0.1f && MQuaternionExtensions.Angle(nextPose.Rotation, trajectory[trajectoryIndex].Rotation) < 1f && trajectoryIndex < trajectory.Count - 1)
                {
                    trajectoryIndex++;
                }
            }


            else
            {
                //Compute the next pose
                nextPose = this.DoLocalMotionPlanning(this.velocity, this.angularVelocity, TimeSpan.FromSeconds(time), currentHandPosition, currentHandRotation, targetTransform.Position, targetTransform.Rotation, out translationWeight);
            }


            ////Determine the next pose using local motion planning
            //MTransform nextPose = this.DoLocalMotionPlanning(this.velocity, this.angularVelocity, TimeSpan.FromSeconds(time), currentHandPosition, currentHandRotation, targetHandPosition, targetHandRotation, out float translationWeight);

            //Perform a partial blend
            //result.Posture = this.PerformPartialBlend(this.handJoint, translationWeight, simulationState);



            //Get the current distance
            float currentDistance = (nextPose.Position.Subtract(targetTransform.Position)).Magnitude();
            float currentAngularDistance = (float)MQuaternionExtensions.Angle(nextPose.Rotation, targetTransform.Rotation);


            //Handle the present state (either in ik mode oder blending)
            switch (this.state)
            {
                case ReleaseMotionState.IK:
                    if (currentDistance < 0.02f && currentAngularDistance < 5f)
                    {
                        //Switch to blending to realize the final share
                        this.state = ReleaseMotionState.Blending;
                        this.elapsedBlendTime = 0;

                        //Set to global constraints
                        this.constraintManager.SetConstraints(ref globalConstraints);

                        //Remove all constraints for the respective hand
                        this.constraintManager.RemoveEndeffectorConstraints(this.handJoint);
                    }
                    else
                    {
                        //Update the constraint
                        this.constraintManager.SetEndeffectorConstraint(this.handJoint, nextPose.Position, nextPose.Rotation);
                    }
                    break;
            }


            //Use the local constraint to compute the ik
            this.constraintManager.SetConstraints(ref localConstraints);



            //React depending on the given state
            switch (this.state)
            {
                //In ik mode the ik solver must be called
                case ReleaseMotionState.IK:

                    //Create a list with the specific constraints for the reach MMU -> Only get the specific ones that must be solved (local constraints)
                    List<MConstraint> ikConstraints = constraintManager.GetJointConstraints();

                    //Only solve if at least one constraint is defined
                    if (ikConstraints.Count > 0)
                    {
                        //Compute twice
                        MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(result.Posture, ikConstraints, new Dictionary<string, string>());
                        result.Posture = ikResult.Posture;
                    }
                    break;

               //In blending mode, motion blending must be performed
                case ReleaseMotionState.Blending:

                    //Perform a blend 
                    elapsedBlendTime += (float)time;

                    float blendWeight = Math.Min(1, elapsedBlendTime / endBlendDuration);

                    result.Posture = MMICSharp.Common.Tools.Blending.PerformBlend(this.SkeletonAccess as IntermediateSkeleton, simulationState.Initial, simulationState.Current, blendWeight,true);
                    //Perform a partial blend
                    //result.Posture = this.PerformPartialBlend(this.handJoint, blendWeight, simulationState);

                    if (blendWeight >= 1f)
                    {
                        result.Events.Add(new MSimulationEvent()
                        {
                            Name = "Release Finished",
                            Reference = this.instruction.ID,
                            Type = mmiConstants.MSimulationEvent_End
                        });
                    }

                    break;
            }



            //Combine the constraints
            this.constraintManager.SetConstraints(ref globalConstraints);
            this.constraintManager.Combine(localConstraints);

            return result;
        }


        /// <summary>
        /// Basic do step routine that is executed for each frame and generates the actual motion.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        private MSimulationResult DoStepBlending(double time, MSimulationState simulationState)
        {
            //Create a new simulation result
            MSimulationResult result = new MSimulationResult()
            {
                Events = simulationState.Events ?? new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints,
                SceneManipulations = simulationState.SceneManipulations ?? new List<MSceneManipulation>(),
                Posture = simulationState.Current
            };


            //Directly operate on the global constraints -> since no ik is computed
            List<MConstraint> globalConstraints = result.Constraints;

            //Assign the global constraints to the constraint manager
            this.constraintManager.SetConstraints(ref globalConstraints);

            //Use the initial state (approved posture of last frame)
            this.SkeletonAccess.SetChannelData(simulationState.Initial);

            //Get the current hand position and rotation
            MVector3 currentHandPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, this.handJoint);
            MQuaternion currentHandRotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, this.handJoint);


            MJointConstraint jointConstraint = null;

            switch (this.handJoint)
            {
                case MJointType.LeftWrist:
                    jointConstraint = this.constraintManager.GetEndeffectorConstraint(MJointType.LeftWrist);
                    break;

                case MJointType.RightWrist:
                    jointConstraint = this.constraintManager.GetEndeffectorConstraint(MJointType.RightWrist);
                    break;
            }


            //Handle the joint constraint
            if (jointConstraint != null)
            {
                //Get the current hand positon based on the constraint
                currentHandPosition = jointConstraint.GeometryConstraint.GetGlobalPosition(this.SceneAccess);
                currentHandRotation = jointConstraint.GeometryConstraint.GetGlobalRotation(this.SceneAccess);
            }

            //Set the skeleton to the current state
            this.SkeletonAccess.SetChannelData(simulationState.Current);


            //Determine the target hand position (either underlying MMU or given via boundary constraints)
            MVector3 targetHandPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, this.handJoint);
            MQuaternion targetHandRotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, this.handJoint);

            //Move the hand from the current position to the target position
            MVector3 deltaPosition = targetHandPosition.Subtract(currentHandPosition);

            float angle = (float)MQuaternionExtensions.Angle(currentHandRotation, targetHandRotation);

            //Compute the distance of the hand to the target hand position
            float distanceToGoal = deltaPosition.Magnitude();

            //Compute the max distance which can be covered within the current frame
            float maxDistance = (float)(time * this.velocity);

            //Compute the max allowed angle
            float maxAngle = (float)(time * this.angularVelocity);

            //Compute the weight for slerping (weight increases with shrinking distance to target)
            float translationWeight = Math.Min(1.0f, maxDistance / distanceToGoal);

            //Compute the rotation weight
            float rotationWeight = Math.Min(1.0f, maxAngle / angle);

            //Blend from the current rotation to the target
            result.Posture = Blending.PerformBlend((IntermediateSkeleton)this.SkeletonAccess, simulationState.Initial, simulationState.Current, Math.Min(translationWeight,rotationWeight), true);

            this.SkeletonAccess.SetChannelData(result.Posture);

            if (distanceToGoal < 0.01f)
            {
                result.Events.Add(new MSimulationEvent()
                {
                    Name = "Release Finished",
                    Reference = this.instruction.ID,
                    Type = mmiConstants.MSimulationEvent_End
                });

                //Remove all constraints for the respective hand
                this.constraintManager.RemoveEndeffectorConstraints(this.handJoint);
            }
            else
            {
                //Remove the endeffector constraint
                this.constraintManager.RemoveEndeffectorConstraints(this.handJoint);

                //Update the constraint
                this.constraintManager.SetEndeffectorConstraint(this.handJoint, this.SkeletonAccess.GetGlobalJointPosition(result.Posture.AvatarID, this.handJoint), this.SkeletonAccess.GetGlobalJointRotation(result.Posture.AvatarID, this.handJoint));
            }



            return result;
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
        private MTransform DoLocalMotionPlanning(double velocity, double angularVelocity, TimeSpan time, MVector3 currentPosition, MQuaternion currentRotation, MVector3 targetPosition, MQuaternion targetRotation, out float translationWeight)
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
            translationWeight = (float)Math.Min(1, maxTranslationDelta / delta.Magnitude());

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


        private MAvatarPostureValues PerformPartialBlend(MJointType bodySide, float weight, MSimulationState simulationState)
        {
            List<MJointType> toConsider = new List<MJointType>();
            switch (bodySide)
            {
                case MJointType.LeftWrist:
                    toConsider.Add(MJointType.LeftShoulder);
                    toConsider.Add(MJointType.LeftElbow);
                    toConsider.Add(MJointType.LeftWrist);
                    break;

                case MJointType.RightWrist:
                    toConsider.Add(MJointType.RightShoulder);
                    toConsider.Add(MJointType.RightElbow);
                    toConsider.Add(MJointType.RightWrist);
                    break;
            }


            Dictionary<MJointType, MQuaternion> rotations = new Dictionary<MJointType, MQuaternion>();

            foreach(MJointType jt in toConsider)
            {

                this.SkeletonAccess.SetChannelData(simulationState.Initial);
                MQuaternion rot1 = this.SkeletonAccess.GetLocalJointRotation(this.AvatarDescription.AvatarID, jt);

                this.SkeletonAccess.SetChannelData(simulationState.Current);
                MQuaternion rot2 = this.SkeletonAccess.GetLocalJointRotation(this.AvatarDescription.AvatarID, jt);

                MQuaternion interpolatedRotation = MQuaternionExtensions.Slerp(rot1, rot2, weight);

                rotations.Add(jt, interpolatedRotation);

            }

            this.SkeletonAccess.SetChannelData(simulationState.Current);

            foreach (var entry in rotations)
            {
                this.SkeletonAccess.SetLocalJointRotation(this.AvatarDescription.AvatarID, entry.Key, entry.Value);
            }

            return this.SkeletonAccess.RecomputeCurrentPostureValues(this.AvatarDescription.AvatarID);
        }


        /// <summary>
        /// Returns the forward vector of the root transform
        /// </summary>
        /// <param name="posture"></param>
        /// <returns></returns>
        private MVector3 GetGlobalDirection(MAvatarPostureValues posture, MVector3 localDireciton)
        {
            MTransform rootTransform = this.GetRootTransform(posture);
            //Compute the forwad vector of the root transform
            MVector3 rootForward = rootTransform.Rotation.Multiply(localDireciton);
            rootForward.Y = 0;
            rootForward = rootForward.Normalize();

            return rootForward;
        }

        /// <summary>
        /// Returns the global transform of the specific joint type and posture
        /// </summary>
        /// <param name="posture"></param>
        /// <param name="jointType"></param>
        /// <returns></returns>
        private MTransform GetTransform(MAvatarPostureValues posture, MJointType jointType)
        {
            this.SkeletonAccess.SetChannelData(posture);

            return new MTransform()
            {
                ID = "",
                Position = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, jointType),
                Rotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, jointType)
            };
        }

        /// <summary>
        /// Returns the root transform of the specific posture
        /// </summary>
        /// <param name="posture"></param>
        /// <returns></returns>
        private MTransform GetRootTransform(MAvatarPostureValues posture)
        {
            this.SkeletonAccess.SetChannelData(posture);

            return new MTransform()
            {
                ID = "",
                Position = this.SkeletonAccess.GetRootPosition(this.AvatarDescription.AvatarID),
                Rotation = this.SkeletonAccess.GetRootRotation(this.AvatarDescription.AvatarID)
            };
        }

    }

}
