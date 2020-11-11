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
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "ReachMMU", "Pose/Reach", "", "A reach MMU", "A reach MMU realized using inverse kinematics.")]
    public class ReachMMUImpl:MMUBase
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
        /// The desired hand joint
        /// </summary>
        private MJointType handJoint;

        /// <summary>
        /// A helper class to manage the constraint list
        /// </summary>
        private ConstraintManager constraintManager;

        /// <summary>
        /// Flag specifies whether the IK is used once and blending is carried out
        /// </summary>
        private bool singleShotIK = false;


        /// <summary>
        /// The maximum velocity of the reach motion
        /// </summary>
        private float velocity = 1.0f;

        /// <summary>
        /// The maximum angular velocity of the reach motion.
        /// By default 0 -> Position weight is used
        /// </summary>
        private float angularVelocity = 0;

        /// <summary>
        /// The target posture used for the single shot ik
        /// </summary>
        private MAvatarPostureValues singleShotIKTargetPosture;

        /// <summary>
        /// The used trajectory for the reach motion
        /// </summary>
        private List<MTransform> trajectory;

        /// <summary>
        /// The current trajectory index of reach
        /// </summary>
        private int trajectoryIndex;

        /// <summary>
        /// The threshold for translation and rotation
        /// </summary>
        private readonly float translationThreshold = 0.01f;
        private readonly float rotationThreshold = 2f;

        /// <summary>
        /// Basic constructor
        /// </summary>
        public ReachMMUImpl()
        {
            this.Name = "ReachMMU";
            this.MotionType = "reach";
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

            //Setuo the skeleton access
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            //Create a new constraint manager
            this.constraintManager = new ConstraintManager(this.SceneAccess);

            return response;
        }


        /// <summary>
        /// Method to assign an actual instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the target location (object) or MGeometryConstraint", true)]
        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the reach motion", true)]
        [MParameterAttribute("SingleShotIK", "bool", "Specifies if the ik is used once for the initial computation and blending is carried out afterwards.", false)]
        [MParameterAttribute("Velocity", "float", "Specifies the velocity of the reaching.", false)]
        [MParameterAttribute("AngularVelocity", "float", "Specifies the angular velocity of the reaching.", false)]
        [MParameterAttribute("Trajectory", "MPathConstraint", "Optionally defined trajectory for reaching.", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Setup the ik
            this.ServiceAccess.IKService.Setup(this.AvatarDescription, new Dictionary<string,string>());

            //Reset all flags/states
            this.constraintManager = new ConstraintManager(this.SceneAccess);
            this.singleShotIK = false;
            this.singleShotIKTargetPosture = null;

            //Assign the instruction
            this.instruction = instruction;

            //Parse the parameters
            MBoolResponse result = this.ParseParameters(instruction);

            if (!result.Successful)
                return result;
        
        
            //Compute the target posture
            if (this.singleShotIK)
            {
                List<MConstraint> tempConstraints = new List<MConstraint>();
                constraintManager.SetConstraints(ref tempConstraints);

                //Set the ik constraints
                constraintManager.SetEndeffectorConstraint(this.handJoint, targetTransform.Position, targetTransform.Rotation);

                //Compute the posture

                MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(simulationState.Current, constraintManager.GetJointConstraints(), new Dictionary<string, string>());

                this.singleShotIKTargetPosture = ikResult.Posture.Copy();


                //Clear the constraints in the constraint manager
                tempConstraints.Clear();
            }

            //Return true/success
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
                Events = new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),  
                SceneManipulations = new List<MSceneManipulation>(),
                Posture = simulationState.Current
            };

            //Compute the target transform at the beginning of each frame
            this.targetTransform = this.ComputeTargetTransform();

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


            //The next pose
            MTransform nextPose = null;

            //The current velocity used for path planning
            float currentVelocity = this.velocity;// + this.ComputeRootVelocity(time, simulationState);

            //Use the trajectory if defined
            if (this.trajectory != null)
            {
                //If a trajectory is used -> The target transform is the last point of the trajectory
                this.targetTransform = this.trajectory.Last();

                //Compute the next pose
                nextPose = this.DoLocalMotionPlanning(currentVelocity, this.angularVelocity, TimeSpan.FromSeconds(time), currentHandPosition, currentHandRotation, this.trajectory[trajectoryIndex].Position, this.trajectory[trajectoryIndex].Rotation);

                //Check if close to current target -> move to next target -> To do consider rotation
                if ((nextPose.Position.Subtract(trajectory[trajectoryIndex].Position)).Magnitude() < this.translationThreshold  && MQuaternionExtensions.Angle(nextPose.Rotation, trajectory[trajectoryIndex].Rotation)< this.rotationThreshold && trajectoryIndex < trajectory.Count - 1)
                {
                    trajectoryIndex++;
                }
            }


            else
            {
                //Compute the next pose
                nextPose = this.DoLocalMotionPlanning(currentVelocity, this.angularVelocity, TimeSpan.FromSeconds(time), currentHandPosition, currentHandRotation, this.targetTransform.Position, this.targetTransform.Rotation);
            }


            //Get the current distance
            float currentDistance = (nextPose.Position.Subtract(targetTransform.Position)).Magnitude();
            float currentAngularDistance = (float)MQuaternionExtensions.Angle(nextPose.Rotation, targetTransform.Rotation);


            //Check if the ik is only computed once and blending is performed subsequently
            if (this.singleShotIK)
            {
                //Estimate the weight for blending
                float weight = (float)Math.Min(1, (currentVelocity * time) / currentDistance);

                //To check -> Why is a deep copy required?
                result.Posture = Blending.PerformBlend((IntermediateSkeleton)this.SkeletonAccess, simulationState.Initial, this.singleShotIKTargetPosture.Copy(), weight, false);


                if (weight >= 1 - 1e-3)
                {
                    result.Events.Add(new MSimulationEvent(this.instruction.Name, mmiConstants.MSimulationEvent_End, this.instruction.ID));

                    constraintManager.SetEndeffectorConstraint(new MJointConstraint(this.handJoint)
                    {
                        GeometryConstraint = new MGeometryConstraint()
                        {
                            ParentObjectID = "",
                            ParentToConstraint = new MTransform(System.Guid.NewGuid().ToString(), targetTransform.Position, targetTransform.Rotation)
                        }
                    });

                }
            }

            //Default scenario -> IK is computed for each frame
            else
            {
                if (currentDistance <= this.translationThreshold && currentAngularDistance <= this.rotationThreshold)
                {
                    //Set the target
                    nextPose.Position = targetTransform.Position;
                    nextPose.Rotation = targetTransform.Rotation;

                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, "Reach finished");
                    result.Events.Add(new MSimulationEvent(this.instruction.Name, mmiConstants.MSimulationEvent_End, this.instruction.ID));
                }

                //Set the desired endeffector constraints
                constraintManager.SetEndeffectorConstraint(this.handJoint, nextPose.Position, nextPose.Rotation);                 
            }


            //Create a list with the specific constraints for the reach MMU -> Only get the specific ones that must be solved (local constraints)
            List<MConstraint> ikConstraints = constraintManager.GetJointConstraints();

            //Only solve if at least one constraint is defined
            if (ikConstraints.Count > 0)
            {
                int ikIterations = 1;

                MIKServiceResult ikResult = null;

                //Use the ik to compute a posture fulfilling the requested constraints
                //To do -> Try with different initial postures / compute suitability of the generated posture
                for (int i = 0; i < ikIterations; i++)
                {
                    //Compute twice
                    ikResult = this.ServiceAccess.IKService.CalculateIKPosture(result.Posture, ikConstraints, new Dictionary<string, string>());
                    result.Posture = ikResult.Posture;
                }
            }

            //Update the constraint manager to operate on the global constraints
            constraintManager.SetConstraints(ref globalConstraints);

            //Integrate the newly defined constraints in the global ones
            constraintManager.Combine(localConstraints);

            //Just for better understanding -> Assign the previous constraints + integrated ones to the result (this is not neccessary since the constraint manager is operating on the reference)
            result.Constraints = globalConstraints;
            
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
            //Extract the single shot ik parameter if defined
            if (instruction.Properties.ContainsKey("SingleShotIK"))
                bool.TryParse(instruction.Properties["SingleShotIK"], out singleShotIK);
            
            //Extract the velocity if defined
            if (instruction.Properties.ContainsKey("Velocity"))
            {
                this.velocity = float.Parse(instruction.Properties["Velocity"], System.Globalization.CultureInfo.InvariantCulture);
            }

            //Extract the angular velocity
            if (instruction.Properties.ContainsKey("AngularVelocity"))
            {
                this.angularVelocity = float.Parse(instruction.Properties["AngularVelocity"], System.Globalization.CultureInfo.InvariantCulture);
            }


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

                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, $"Assigned hand trajectory. Number elements: {trajectory.Count}, hand: {this.handJoint}");
                }
                else
                {
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, $"Cannot assign trajectory of hand: {this.handJoint}. No suitable MPathConstraint available.");

                }
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
