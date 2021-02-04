// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMICSharp.Common.Tools;
using MMIStandard;
using System;
using System.Collections.Generic;


namespace MoveMMU
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "MoveMMUSimple", "Object/Move","moveOnHanded", "A move MMU", "A move MMU realized using inverse kinematics.")]
    public class MoveMMUSimpleImpl:MMUBase
    {
        #region private fields

        /// <summary>
        /// The stored instruction
        /// </summary>
        private MInstruction instruction;

        /// <summary>
        /// The target transform of the object 
        /// </summary>
        private MTransform targetObjectTransform;

        /// <summary>
        /// The transform of the object which should be moved ariund
        /// </summary>
        private MTransform subjectTransform;

        /// <summary>
        /// The position offset of the hand relative to the subject
        /// </summary>
        private MVector3 handPositionOffset;

        /// <summary>
        /// The rotation offset of the hand relative to the subject
        /// </summary>
        private MQuaternion handRotationOffset;

        /// <summary>
        /// The desired hand joint
        /// </summary>
        private MJointType handJoint;

        /// <summary>
        /// A helper class to manage the constraint list
        /// </summary>
        private ConstraintManager constraintManager;

        /// <summary>
        /// The (optionally) assigned trajectory constraint
        /// </summary>
        private MPathConstraint trajectory;

        /// <summary>
        /// Specifies whether the MMU has a specified trajectory
        /// </summary>
        private bool hasTrajectory = false;

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
            // Added new intermediate skeleton representation. 
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            this.constraintManager = new ConstraintManager(this.SceneAccess);

            return res;

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

            //Reset all values
            this.hasTrajectory = false;

            // Initialize IK Service
            this.ServiceAccess.IKService.Setup(this.AvatarDescription, new Dictionary<string, string>());

            //Assign the instruction
            this.instruction = instruction;

            bool hasTarget = false;


            String targetID;
            //Get the target id using all synonyms
            if(instruction.Properties.GetValue(out targetID, "targetID","TargetID", "objectID"))
            {
                this.targetObjectTransform = this.SceneAccess.GetTransformByID(targetID);
                hasTarget = true;
            }


            //Error id not available
            if (!hasTarget)
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Required parameter Target ID not defined" }
                };
            }

            bool hasSubject = false;

            String subjectID;
            //Get the subject id using all synonyms
            if (instruction.Properties.GetValue(out subjectID, "subjectID", "SubjectID"))
            {
                this.subjectTransform = this.SceneAccess.GetTransformByID(subjectID);
                hasSubject = true;
            }

            //Error id not available
            if(!hasSubject)
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Required parameter Target ID not defined" }
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

            //Handle the trajectory constraint (if defined)
            if (instruction.Properties.ContainsKey("trajectory"))
            {
                string trajectoryID = instruction.Properties["trajectory"];

                if (instruction.Constraints != null)
                {
                    MConstraint constraint = instruction.Constraints.Find(s => s.ID == trajectoryID);

                    if(constraint.PathConstraint != null)
                    {
                        this.trajectory = constraint.PathConstraint;
                        this.hasTrajectory = true;
                    }
                }
            }



            //Compute the relative position and rotation
            this.SkeletonAccess.SetChannelData(simulationState.Initial);
            MVector3 currentHandPosition = this.SkeletonAccess.GetGlobalJointPosition(simulationState.Current.AvatarID, this.handJoint);
            MQuaternion currentHandRotation = this.SkeletonAccess.GetGlobalJointRotation(simulationState.Current.AvatarID, this.handJoint);

            /* Old system could access joint positions from MAvatarPosture directly, now we have to use the intermediate skeleton. 
            MVector3 currentHandPosition = simulationState.Initial.GetGlobalPosition(this.handJoint);
            MQuaternion currentHandRotation = simulationState.Initial.GetGlobalRotation(this.handJoint);
            */

            //Compute the offsets between hand <-> object
            this.handPositionOffset = this.subjectTransform.InverseTransformPoint(currentHandPosition);
            this.handRotationOffset = this.subjectTransform.InverseTransformRotation(currentHandRotation);


            return new MBoolResponse(true);
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
                Events = simulationState.Events != null ? simulationState.Events : new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints,
                SceneManipulations = simulationState.SceneManipulations!=null ? simulationState.SceneManipulations : new List<MSceneManipulation>()
            };


            //Create variables representing the next object position/rotation
            MTransform nextObjectTransform = subjectTransform.Clone();

            //Use the constraint manager to manage the constraints
            List<MConstraint> tmpConstraints = result.Constraints;

            //Set the constraints 
            constraintManager.SetConstraints(ref tmpConstraints);


            //Compute the new hand position and rotation
            MVector3 deltaPosition = this.targetObjectTransform.Position.Subtract(subjectTransform.Position);
            float distanceToGoal = deltaPosition.Magnitude();

            //Get the current object position
            float maxDistance = (float)time * 1.0f;

            //Check the current distance to goal
            if (distanceToGoal < 0.01f)
            {
                result.Events.Add(new MSimulationEvent(this.instruction.Name, mmiConstants.MSimulationEvent_End, this.instruction.ID));
            }
            else
            {
                //Compute the new hand position (normalize delta position and multiply by max distance)
                nextObjectTransform.Position = this.subjectTransform.Position.Add(deltaPosition.Normalize().Multiply(Math.Min(distanceToGoal, maxDistance)));

                //Compute the weight for slerping (weight increases with shrinking distance to target)
                float weight = Math.Max(0, 1 - distanceToGoal);

                //Just perform an interpolation to gather new hand rotation (weight is determined by the translation distance)
                nextObjectTransform.Rotation = MQuaternionExtensions.Slerp(this.subjectTransform.Rotation, this.targetObjectTransform.Rotation, weight);
            }

            //Adjust the transformation of the object which should be moved
            result.SceneManipulations.Add(new MSceneManipulation()
            {
                Transforms = new List<MTransformManipulation>()
                 {
                      new MTransformManipulation()
                      {
                          Target = this.subjectTransform.ID,
                           Position = nextObjectTransform.Position,
                           Rotation = nextObjectTransform.Rotation
                      }
                 }
            });

            //Get the current hand position in global space
            MVector3 globalHandPosition = nextObjectTransform.TransformPoint(this.handPositionOffset);
            MQuaternion globalHandRotation = nextObjectTransform.TransformRotation(this.handRotationOffset);
    
            //Set the desired endeffector constraints
            constraintManager.SetEndeffectorConstraint(this.handJoint, globalHandPosition, globalHandRotation);



            MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(simulationState.Current, constraintManager.GetJointConstraints(), new Dictionary<string, string>());

            //Generate a new posture using the ik solver and the specified constraints
            result.Posture = ikResult.Posture;

            //Return the result
            return result;
        }
    }
}
