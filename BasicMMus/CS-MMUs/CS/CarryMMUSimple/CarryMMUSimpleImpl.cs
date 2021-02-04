// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMICSharp.Common.Tools;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace CarryMMUSimple
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "CarryMMUSimple", "Object/Carry", "carrySingleHanded", "A carry MMU", "A single handed carry MMU.")]
    public class CarryMMUSimpleImpl:MMUBase
    {
        #region private fields

        /// <summary>
        /// The stored instruction
        /// </summary>
        private MInstruction instruction;

        /// <summary>
        /// The current transform of the object 
        /// </summary>
        private MTransform objectTransform;

        /// <summary>
        /// The desired hand joint
        /// </summary>
        private MJointType handJoint;

        /// <summary>
        /// Flag specifying whether an offset should be added to the hand in order to avoid collisions with the carried object
        /// </summary>
        private bool addOffset = true;

        /// <summary>
        /// The moving velocity of the hand
        /// </summary>
        private float velocity = 1.4f;

        /// <summary>
        /// The position offset of the object relative to the hand
        /// </summary>
        private MVector3 objectPositionOffset;

        /// <summary>
        /// The rotation offset of the object relative to the hand
        /// </summary>
        private MQuaternion objectRotationOffset;

        /// <summary>
        /// A helper class to manage the constraint list
        /// </summary>
        private ConstraintManager constraintManager;

        /// <summary>
        /// Flag indicating whether the positioning is finished
        /// </summary>
        private bool positioningFinished = false;


        /// <summary>
        /// The root position of the last frame
        /// </summary>
        private MVector3 rootPositionLastFrame;


        /// <summary>
        /// Threshold for the positioningFinishedEvent
        /// </summary>
        private float positioningFinishedThreshold = 0.01f;

        #endregion

        /// <summary>
        /// Basic initialize routine
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            MBoolResponse res = base.Initialize(avatarDescription, properties);
            // Added new intermediate skeleton representation. 
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            this.constraintManager = new ConstraintManager(this.SceneAccess);

            //Nothing to do in here
            return res;
        }

        /// <summary>
        /// Basic assign instruction method
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the object which should be carried", true)]
        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the carry motion", true)]
        [MParameterAttribute("AddOffset", "bool", "Specifies whether an offset is automatically added to the carry position considering the object dimensions", false)]
        //[MParameterAttribute("CarryTargetID", "ID", "Specifies an optional carry target. If defined, this is used instead of the underlying posture as target.", false)]
        [MParameterAttribute("PositioningFinishedThreshold", "float", "Threshold for the positioning finished event.", false)]
        [MParameterAttribute("Velocity", "float", "Specifies the velocity of the reaching.", false)]

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Assign the instruction
            this.instruction = instruction;

            //Reset the flags and states
            this.positioningFinished = false;
            this.rootPositionLastFrame = null;

            // Initialize IK Service
            this.ServiceAccess.IKService.Setup(this.AvatarDescription, new Dictionary<string, string>());

            //Get the target id
            if (instruction.Properties.ContainsKey("TargetID"))
                this.objectTransform = this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);
            //Error id not available
            else
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Required parameter TargetID not defined" }
                };
            }

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

            //Parse optional property
            if (instruction.Properties.ContainsKey("PositioningFinishedThreshold"))
                float.TryParse(instruction.Properties["PositioningFinishedThreshold"], out this.positioningFinishedThreshold);

            //Parse optional property
            if (instruction.Properties.ContainsKey("AddOffset"))
                bool.TryParse(instruction.Properties["AddOffset"], out addOffset);


            //Extract the velocity if defined
            if (instruction.Properties.ContainsKey("Velocity"))
                float.TryParse(instruction.Properties["Velocity"], out velocity);


            //Get the initial position and rotation
            this.SkeletonAccess.SetChannelData(simulationState.Initial);
            MVector3 currentHandPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, this.handJoint);
            MQuaternion currentHandRotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, this.handJoint);

            //Create a new transform representing the "virtual" transform of the hand
            MTransform handTransform = new MTransform("hand", currentHandPosition, currentHandRotation);

            //Compute the offsets between hand <-> object
            this.objectPositionOffset = handTransform.InverseTransformPoint(objectTransform.Position);
            this.objectRotationOffset = handTransform.InverseTransformRotation(objectTransform.Rotation);


            return new MBoolResponse(true);
        }



        /// <summary>
        /// Basic do step routine which computes thre resulting posture for each frame.
        /// The Simple carry just tries to position the object to the current location of the hand animation of the previous MMU in hiearchy
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MSimulationEventAttribute("PositioningFinished", "PositioningFinished")]
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Create a new simulation result
            MSimulationResult result = new MSimulationResult()
            {
                Events = simulationState.Events ?? new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),
                SceneManipulations = simulationState.SceneManipulations ?? new List<MSceneManipulation>()
            };

            //Assign the constraints to a temp varilable
            List<MConstraint> constraints = result.Constraints;

            //Use the constraint manager to manage the constraints
            constraintManager.SetConstraints(ref constraints);


            //Get the hand position and rotation of the last frame (approved result)
            this.SkeletonAccess.SetChannelData(simulationState.Initial);
            MVector3 currentHandPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, this.handJoint);
            MQuaternion currentHandRotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, this.handJoint);


            //Get the desired hand position (of the underlying motion e.g. idle)
            this.SkeletonAccess.SetChannelData(simulationState.Current);
            MVector3 targetHandPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, this.handJoint);
            MQuaternion targetHandRotation = this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, this.handJoint);


            //Add an offset on top of the position if desired
            if(this.addOffset)
                targetHandPosition = ComputeNewPositionWithOffset(targetHandPosition, simulationState.Current);

            //Move the hand from the current position to the target position
            MVector3 deltaPosition = targetHandPosition.Subtract(currentHandPosition);

            //Compute the distance of the hand to the target hand position
            float distanceToGoal = deltaPosition.Magnitude();


            //Create positioning finished event if not already created and distance below threshold
            if(distanceToGoal < this.positioningFinishedThreshold && !this.positioningFinished)
            {
                result.Events.Add(new MSimulationEvent("PositioningFinished", "PositioningFinished", this.instruction.ID));
                this.positioningFinished = true;
            }

            //Compute the current velocity based on the general max velocity and the velocity of the root motion
            float currentVelocity = this.velocity + this.ComputeRootVelocity(time, simulationState);

            //Compute the max distance which can be covered within the current frame
            float maxDistance = (float)(time * currentVelocity);

            //Compute the weight for slerping (weight increases with shrinking distance to target)
            float weight = Math.Max(0, 1 - distanceToGoal);

            //Create a new transform representing the next hand transform
            MTransform newHandTransform = new MTransform("", currentHandPosition.Clone(), currentHandRotation.Clone())
            {
                //Compute the new hand position (normalize delta position and multiply by max distance)
                Position = currentHandPosition.Add(deltaPosition.Normalize().Multiply(Math.Min(deltaPosition.Magnitude(), maxDistance))),

                //Just perform an interpolation to gather new hand rotation (weight is determined by the translation distance)
                Rotation = MQuaternionExtensions.Slerp(currentHandRotation, targetHandRotation, weight)
            };


            //Compute the corresponding positon/rotation of the object and
            //adjust the transformation of the object which should be moved
            result.SceneManipulations.Add(new MSceneManipulation()
            {
                Transforms = new List<MTransformManipulation>()
                 {
                      new MTransformManipulation()
                      {
                          Target = this.objectTransform.ID,
                          //Compute the new global position of the object
                           Position = newHandTransform.TransformPoint(this.objectPositionOffset),
                           //Compute the new global rotation of the object
                           Rotation = newHandTransform.TransformRotation(this.objectRotationOffset)
                        }
                 }
            });


            //Set the desired endeffector constraints
            constraintManager.SetEndeffectorConstraint(this.handJoint, newHandTransform.Position, newHandTransform.Rotation);

            //Generate a new posture using the ik solver and the specified constraints               
            MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(simulationState.Current, constraintManager.GetJointConstraints(), new  Dictionary<string, string>());
            result.Posture = ikResult.Posture;

            //Return the result
            return result;
        }


        /// <summary>
        /// Computes the new (desired) hand position considering the offset of the collider (to avoid self-collisions)
        /// </summary>
        /// <param name="targetHandPosition"></param>
        /// <param name="currentPosture"></param>
        /// <returns></returns>
        private MVector3 ComputeNewPositionWithOffset(MVector3 targetHandPosition, MAvatarPostureValues currentPosture)
        {
            //Optionally ensure that the object does not intersect the avatar
            MCollider collider = this.SceneAccess.GetColliderById(this.objectTransform.ID);

            //Determine the offset based on the respective collider
            float offset = 0;

            if(collider.SphereColliderProperties !=null)
                offset = (float)collider.SphereColliderProperties.Radius;

            if (collider.BoxColliderProperties != null)
                offset = (float)collider.BoxColliderProperties.Size.Magnitude();

            if (collider.CapsuleColliderProperties != null)
                offset = Math.Max((float)collider.CapsuleColliderProperties.Height, (float)collider.CapsuleColliderProperties.Radius);

            //The offset could be also dynamically determined (using the mesh intersection distance or using Physics Compute Pentration in unity)

            this.SkeletonAccess.SetChannelData(currentPosture);

            //Get the shoulder positions
            MVector3 leftShoulderPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID,MJointType.LeftShoulder);
            MVector3 rightShoulderPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID,MJointType.RightShoulder);

            //Compute the direction vector pointing from the avatar towards the respective hand 
            MVector3 dir = new MVector3(0, 0, 0);

            switch (this.handJoint)
            {
                case MJointType.LeftWrist:
                    dir = leftShoulderPosition.Subtract(rightShoulderPosition).Normalize();
                    break;

                case MJointType.RightWrist:
                    dir = rightShoulderPosition.Subtract(leftShoulderPosition).Normalize();
                    break;
            }

            //Add an offset on top of the position
            return targetHandPosition.Add(dir.Multiply(offset));
        }


        /// <summary>
        /// Computes the root velocity of the avatar given the initial and current state
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private float ComputeRootVelocity(double time, MSimulationState simulationState)
        {
            float velocity = 0;

            if(this.rootPositionLastFrame != null)
            {
                MVector3 currentRootPosition = new MVector3(simulationState.Initial.PostureData[0], 0, simulationState.Initial.PostureData[2]);

                //Estimate the root velocity
                velocity = (rootPositionLastFrame.Subtract(currentRootPosition)).Magnitude() / (float)time;
            }

            this.rootPositionLastFrame = new MVector3(simulationState.Initial.PostureData[0], 0, simulationState.Initial.PostureData[2]);

            return velocity;
        }

    }
}






