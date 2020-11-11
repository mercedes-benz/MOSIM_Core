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

namespace MoveMMU
{

    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(MoveMMUImpl)))
            {
                Console.ReadLine();
            }
        }
    }


    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "MoveMMU", "Object/Move", "moveBothHanded", "A move MMU which models both, one handed and both handed motions.", "A move MMU realized using inverse kinematics. It moreover supports concurrent motions.")]
    public class MoveMMUImpl : MMUBase
    {
        #region private members

        private ConstraintManager constraintManager;

        /// <summary>
        /// The presently active hands that are used for moving objects
        /// </summary>
        private List<HandContainer> activeHands = new List<HandContainer>();

        /// <summary>
        /// The translation and rotation threshold for finishing the motion
        /// </summary>
        private readonly float translationThreshold = 0.01f;
        private readonly float rotationThreshold = 2f;

        private bool collisionAvoidance = false;
        #endregion


        /// <summary>
        /// Basic initialization function
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            //Call the base class
            base.Initialize(avatarDescription, properties);

            //Create a new list for the active hands
            this.activeHands = new List<HandContainer>();

            // Added new intermediate skeleton representation. 
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

            //Create a new constraint manager
            this.constraintManager = new ConstraintManager(this.SceneAccess);

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Method to assign an actual instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the target location (object)", false)]
        [MParameterAttribute("SubjectID", "ID", "The id of the object hat should be moved.", true)]
        [MParameterAttribute("Hand", "{Left,Right}", "The hand used for moving the object.", true)]
        [MParameterAttribute("Trajectory", "ID of the MPathConstraint constraint", "An optionally specified trajectory.", false)]
        [MParameterAttribute("Velocity", "float", "An optionally defined velocity.", false)]
        [MParameterAttribute("AngularVelocity", "float", "An optionally defined angular velocity.", false)]
        [MParameterAttribute("HoldDuration", "float", "An optional time, the move is holded.", false)]
        [MParameterAttribute("CollisionAvoidance", "bool", "Flag defines whether local collision avoidance using steering behavior is used.", false)]

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {

            //Extract the hand information
            if (instruction.Properties.ContainsKey("Hand"))
            {
                switch (instruction.Properties["Hand"])
                {
                    case "Left":
                        this.SetupHand(MJointType.LeftWrist, instruction);
                        break;

                    case "Right":
                        this.SetupHand(MJointType.RightWrist, instruction);
                        break;

                    case "Both":
                        this.SetupHand(MJointType.LeftWrist, instruction);
                        this.SetupHand(MJointType.RightWrist, instruction);
                        break;
                }
            }
            else
            {
                this.SetupHand(MJointType.RightWrist, instruction);
            }

         
            //To fix -> might overwrite already active hands
            foreach (HandContainer hand in this.activeHands)
            {
                if(hand.Trajectory == null && !hand.Instruction.Properties.ContainsKey("TargetID") && !hand.Instruction.Properties.ContainsKey("targetID"))
                {
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "At least one of both trajectory or target must be set. Aborting Move MMU");
                    return new MBoolResponse(false);
                }

                //Get the global hand position and rotation
                MVector3 handPosition = GetGlobalPosition(simulationState.Initial, hand.Type);
                MQuaternion handRotation = GetGlobalRotation(simulationState.Initial, hand.Type);

                //Get the transform of the scene object to be moved
                MTransform sceneObjectTransform = this.SceneAccess.GetTransformByID(hand.Instruction.Properties.GetValue("SubjectID","subjectID"));

                //Compute the relative transform of the hand
                hand.Offset = new MTransform("", sceneObjectTransform.InverseTransformPoint(handPosition), sceneObjectTransform.InverseTransformRotation(handRotation));


                //bool addPseudoPoint = true;
                //if(hand.Trajectory == null && addPseudoPoint)
                //{
                //    //Compute artifical trajectory

                //    hand.Trajectory = new List<MTransform>() { sceneObjectTransform.Clone(), this.ComputeTargetTransform(hand) };           
                    
                //    //Insert intermediate point that prevents from intersection               
                //}

                //Set hand to initialized
                hand.Initialized = true;
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Basic to step routine which computes the result of the current frame
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Create a new result
            MSimulationResult result = new MSimulationResult()
            {
                Events = simulationState.Events ?? new List<MSimulationEvent>(),
                DrawingCalls = new List<MDrawingCall>(),
                SceneManipulations = simulationState.SceneManipulations ?? new List<MSceneManipulation>(),
                Posture = simulationState.Current,
                Constraints = simulationState.Constraints ?? new List<MConstraint>()
            };

            //The presently active constraints
            List<MConstraint> globalConstraints = new List<MConstraint>(result.Constraints);

            //The local constraints defined within the MMU 
            List<MConstraint> localConstraints = new List<MConstraint>();

            //Setup the constraint manager and use the local constraints
            this.constraintManager.SetConstraints(ref localConstraints);


            //Handle each active hand
            for (int i = this.activeHands.Count - 1; i >= 0; i--)
            {
                //Get the current hand
                HandContainer hand = this.activeHands[i];

                //Skip if hand is not initialized
                if (!hand.Initialized)
                    continue;

                //Get the transform of the object to be positioned
                MTransform currentObjectTransform = this.SceneAccess.GetTransformByID(hand.Instruction.Properties["SubjectID"]);

                //Get the transform of the target
                MTransform targetObjectTransform = null; 

                //Determine the next location of the object (at the end of the frame)
                MTransform nextObjectTransform = null;

                //Check if trajectory is defined
                if (hand.Trajectory != null)
                {
                    //The last point is the target transform
                    targetObjectTransform = hand.Trajectory.Last();

                    //The current rajectory point
                    MTransform currentTrajectoryPoint = hand.Trajectory[hand.TrajectoryIndex];

                    //Estimate the next transfom based on local motion planning
                    nextObjectTransform = this.DoLocalMotionPlanning(hand.Velocity,hand.AngularVelocity, TimeSpan.FromSeconds(time), currentObjectTransform.Position, currentObjectTransform.Rotation, currentTrajectoryPoint.Position, currentTrajectoryPoint.Rotation, hand.CollisionAvoidance);

                    //Get the current distance
                    float currentDistance = nextObjectTransform.Position.Subtract(hand.Trajectory[hand.TrajectoryIndex].Position).Magnitude();
                    float currentAngularDistance = (float)MQuaternionExtensions.Angle(nextObjectTransform.Rotation, hand.Trajectory[hand.TrajectoryIndex].Rotation);

                    //Check if close to current target -> move to next target
                    if (currentDistance < this.translationThreshold && currentAngularDistance < this.rotationThreshold && hand.TrajectoryIndex < hand.Trajectory.Count - 1)
                    {
                        hand.TrajectoryIndex++;
                    }
                }

                //Default behavior if no trajectory is specified
                else
                {
                    targetObjectTransform = this.ComputeTargetTransform(hand);                      

                    //Estimate the next pose of the scene object
                    nextObjectTransform = this.DoLocalMotionPlanning(hand.Velocity, hand.AngularVelocity, TimeSpan.FromSeconds(time), currentObjectTransform.Position, currentObjectTransform.Rotation, targetObjectTransform.Position, targetObjectTransform.Rotation, hand.CollisionAvoidance);
                }


                //Set the pose of the object to the next estimated pose
                result.SceneManipulations.Add(new MSceneManipulation()
                {
                    Transforms = new List<MTransformManipulation>()
                     {
                         new MTransformManipulation()
                         {
                             Target = hand.Instruction.Properties.GetValue("SubjectID", "subjectID"),
                             Position = nextObjectTransform.Position,
                             Rotation = nextObjectTransform.Rotation
                         }
                     }
                });


                //Compute the next handpose based on the offset
                MTransform nextHandTransform = new MTransform("",nextObjectTransform.TransformPoint(hand.Offset.Position), nextObjectTransform.TransformRotation(hand.Offset.Rotation));

                //Set the ik constraints
                constraintManager.SetEndeffectorConstraint(hand.Type, nextHandTransform.Position, nextHandTransform.Rotation);

                //To do add constraints  
                float distance = (nextObjectTransform.Position.Subtract(targetObjectTransform.Position)).Magnitude();
                float angularDistance = (float)MQuaternionExtensions.Angle(nextObjectTransform.Rotation, targetObjectTransform.Rotation);


                //Check if goal criteria fulfilled
                if (distance < this.translationThreshold && angularDistance < this.rotationThreshold)
                {
                    //Increment the time
                    hand.ElapsedHoldTime += time;

                    if (hand.ElapsedHoldTime < hand.HoldTime)
                        continue;

                    this.activeHands.RemoveAt(i);

                    //Add new finished event
                    if (hand.BothHanded)
                    {
                        if (activeHands.Count == 0)
                        {
                            result.Events.Add(new MSimulationEvent(hand.Instruction.Name, mmiConstants.MSimulationEvent_End, hand.Instruction.ID));
                        }
                    }

                    //Single handed grasp
                    else
                    {
                        result.Events.Add(new MSimulationEvent(hand.Instruction.Name, mmiConstants.MSimulationEvent_End, hand.Instruction.ID));
                    }
                }
            }



            //Get the properties from the constraint manager
            List<MConstraint> jointConstraints = this.constraintManager.GetJointConstraints();


            //Use the ik service if at least one constraint must be solved
            if (jointConstraints.Count > 0)
            {
                MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(simulationState.Current, jointConstraints, new Dictionary<string, string>());
                result.Posture = ikResult.Posture;
            }

            //Configure the constraint manager to operate on the global constraints
            constraintManager.SetConstraints(ref globalConstraints);

            //Combine the global with the local ones
            constraintManager.Combine(localConstraints);

            //Provide the combined constraints as result
            result.Constraints = globalConstraints;

            //Return the simulation result
            return result;
        }


        #region private methods


        /// <summary>
        /// Method is used to setup the parameters of one individual hand
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instruction"></param>
        private void SetupHand(MJointType type, MInstruction instruction)
        {
            HandContainer hand = this.activeHands.Find(s => s.Type == type);

            if (hand != null)
            {
                this.activeHands.Remove(hand);
            }


            //Create a new hand
            hand = new HandContainer(type, instruction, true);

            if (instruction.Properties.ContainsKey("Velocity"))
                hand.Velocity = float.Parse(instruction.Properties["Velocity"], System.Globalization.CultureInfo.InvariantCulture);

            if (instruction.Properties.ContainsKey("AngularVelocity"))
                hand.AngularVelocity = float.Parse(instruction.Properties["AngularVelocity"], System.Globalization.CultureInfo.InvariantCulture);

            if (instruction.Properties.ContainsKey("Acceleration"))
                hand.Acceleration = float.Parse(instruction.Properties["Acceleration"], System.Globalization.CultureInfo.InvariantCulture);

            if (instruction.Properties.ContainsKey("HoldDuration"))
                hand.HoldTime = float.Parse(instruction.Properties["HoldDuration"], System.Globalization.CultureInfo.InvariantCulture);

            if (instruction.Properties.ContainsKey("CollisionAvoidance"))
            {
                hand.CollisionAvoidance = bool.Parse(instruction.Properties["CollisionAvoidance"]);

                if(hand.CollisionAvoidance)
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, $"Using local collision avoidance, hand: {hand.Type}");

            }


            //First extract all parameters
            if (instruction.Properties.ContainsKey("Trajectory"))
            {
                string pathConstraintID= instruction.Properties["Trajectory"];
                
                if (instruction.Constraints != null || instruction.Constraints.Where(s => s.PathConstraint != null && s.ID == pathConstraintID).Count() ==0)
                {
                    //Get the path constraint
                    MPathConstraint pathConstraint = instruction.Constraints.Find(s => s.PathConstraint != null && s.ID == pathConstraintID).PathConstraint;

                    //Extract the trajectory
                    hand.Trajectory = pathConstraint.GetMTransformList();

                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, $"Assigned hand trajectory. Number elements: {hand.Trajectory.Count}, hand: {hand.Type}");
                }
                else
                {
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, $"Cannot assign trajectory of hand: {hand.Type}. No suitable MPathConstraint available.");

                }
            }


            this.activeHands.Add(hand);
        }


        /// <summary>
        /// Method performs a local motion planning and tries to reach the specified goal position and rotation using the given velocity,angular velocity and time.
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="angularVelocity"></param>
        /// <param name="time"></param>
        /// <param name="currentPosition"></param>
        /// <param name="currentRotation"></param>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <returns></returns>
        private MTransform DoLocalMotionPlanning(double velocity, double angularVelocity, TimeSpan time, MVector3 currentPosition, MQuaternion currentRotation, MVector3 targetPosition, MQuaternion targetRotation, bool collisionAvoidance)
        {
            //Create a new transform representing the result
            MTransform result = new MTransform();

            //Estimate the delta
            MVector3 delta = targetPosition.Subtract(currentPosition);

            //Determine the current delta angle
            double angle = Math.Abs(MQuaternionExtensions.Angle(currentRotation, targetRotation));

            //Determine the max translation delta and max angle in the current frame
            double maxTranslationDelta = velocity * time.TotalSeconds;
            double maxAngle = angularVelocity * time.TotalSeconds;

            //Estimate the blend weight for the rotation and position
            float rotationWeight = (float)Math.Min(1, maxAngle / angle);
            float positionWeight = (float)Math.Min(1, maxTranslationDelta / delta.Magnitude());

            //Limit the max translation
            if (delta.Magnitude() >= maxTranslationDelta)
            {
                delta = delta.Normalize();
                delta = delta.Multiply(maxTranslationDelta);            
            }


            if (collisionAvoidance)
            {
                MVector3 collisionAvoidanceForce = this.ComputCollisionAvoidance(currentPosition, delta);

                //if (collisionAvoidanceForce.Magnitude() > 0)
                //    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, "Collision avoidance force: " + collisionAvoidanceForce.Magnitude());

                //Add the collision avoidance force on top
                delta = delta.Add(collisionAvoidanceForce);

                //Limit the max translation
                if (delta.Magnitude() >= maxTranslationDelta)
                {
                    delta = delta.Normalize();
                    delta = delta.Multiply(maxTranslationDelta);
                }
            }

            //Compute the new position
            result.Position = currentPosition.Add(delta);


            //Compute the new rotation by interpolating towards the target rotation
            if (angularVelocity >0)
                result.Rotation = MQuaternionExtensions.Slerp(currentRotation, targetRotation, rotationWeight);

            //Use the rotation weight
            else
                result.Rotation = MQuaternionExtensions.Slerp(currentRotation, targetRotation, positionWeight);


            //Return the simulation result
            return result;
        }



        private class Obstacle
        {
            public MVector3 Center;
            public float Radius;
        }
        /// <summary>
        /// Implementation based on https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-collision-avoidance--gamedev-7777
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        private MVector3 ComputCollisionAvoidance(MVector3 position, MVector3 velocity)
        {         
            MVector3 normalizedVelocity = velocity.Normalize();
            float MAX_SEE_AHEAD = 0.4f;
            float MAX_AVOID_FORCE = 5f;

            //ahead = position + normalize(velocity) * MAX_SEE_AHEAD
            MVector3 ahead = position.Add(normalizedVelocity.Multiply(MAX_SEE_AHEAD));

            MVector3 ahead2 = position.Add(normalizedVelocity.Multiply(MAX_SEE_AHEAD * 0.5f));

            ///The obstacles describing the body
            List<Obstacle> obstacles = new List<Obstacle>();

            MVector3 pelvisPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.PelvisCentre);
            MVector3 headPosition = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.HeadJoint);


            obstacles.Add(new Obstacle()
            {
                Center = pelvisPosition,
                Radius = 0.45f
            });

            obstacles.Add(new Obstacle()
            {
                Center = headPosition,
                Radius = 0.3f
            });
            Obstacle mostThreatening  = findMostThreateningObstacle(position, ahead, ahead2,obstacles);

            MVector3 avoidance = new MVector3(0, 0, 0);

            if (mostThreatening != null)
            {
                avoidance.X = ahead.X - mostThreatening.Center.X;
                avoidance.Y = ahead.Y - mostThreatening.Center.Y;
                avoidance.Z = ahead.Z - mostThreatening.Center.Z;

                avoidance = avoidance.Normalize();
                avoidance = avoidance.Multiply(MAX_AVOID_FORCE);
            }
            else
                avoidance = avoidance.Multiply(0);

            return avoidance;
        }


        private bool LineIntersectsCircle(MVector3 ahead, MVector3 ahead2, Obstacle obstacle)
        {
            return MVector3Extensions.Distance(obstacle.Center, ahead) < obstacle.Radius || MVector3Extensions.Distance(obstacle.Center, ahead2) < obstacle.Radius;
        }


        private Obstacle findMostThreateningObstacle(MVector3 position, MVector3 ahead, MVector3 ahead2, List<Obstacle> obstacles)
        {
            Obstacle mostThreatening = null;

            foreach (Obstacle obstacle in obstacles)
            {
                bool collision = LineIntersectsCircle(ahead, ahead2, obstacle);

                // "position" is the character's current position
                if (collision && (mostThreatening == null || MVector3Extensions.Distance(position, obstacle.Center) < MVector3Extensions.Distance(position, mostThreatening.Center)))
                    mostThreatening = obstacle;
            }

            return mostThreatening;
        }


    #endregion

    #region utils


    /// <summary>
    /// Returns the global position of the specific joint type and posture
    /// </summary>
    /// <param name="posture"></param>
    /// <param name="jointType"></param>
    /// <returns></returns>
    private MVector3 GetGlobalPosition(MAvatarPostureValues posture, MJointType jointType)
        {
            this.SkeletonAccess.SetChannelData(posture);
            return this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, jointType);
        }

        /// <summary>
        /// Returns the global rotation of the specific joint type and posture
        /// </summary>
        /// <param name="posture"></param>
        /// <param name="jointType"></param>
        /// <returns></returns>
        private MQuaternion GetGlobalRotation(MAvatarPostureValues posture, MJointType jointType)
        {
            this.SkeletonAccess.SetChannelData(posture);
            return this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, jointType);
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
        /// Returns the global rotation of the specific joint type and posture
        /// </summary>
        /// <param name="posture"></param>
        /// <param name="jointType"></param>
        /// <returns></returns>
        private MQuaternion GetRootRotation(MAvatarPostureValues posture)
        {
            this.SkeletonAccess.SetChannelData(posture);
            return this.SkeletonAccess.GetRootRotation(this.AvatarDescription.AvatarID);
        }

        /// <summary>
        /// Returns the global position of the specific joint type and posture
        /// </summary>
        /// <param name="posture"></param>
        /// <param name="jointType"></param>
        /// <returns></returns>
        private MVector3 GetRootPosition(MAvatarPostureValues posture)
        {
            this.SkeletonAccess.SetChannelData(posture);
            return this.SkeletonAccess.GetRootPosition(this.AvatarDescription.AvatarID);
        }



        /// <summary>
        /// Dynamically estimates the target transform
        /// </summary>
        /// <returns></returns>
        private MTransform ComputeTargetTransform(HandContainer hand)
        {
            MTransform target = new MTransform()
            {
                ID = "target"
            };

            //Use the constraint (if defined)
            if (hand.Instruction.Constraints != null && hand.Instruction.Constraints.Exists(s => s.ID == hand.Instruction.Properties["TargetID"]))
            {
                MConstraint match = hand.Instruction.Constraints.Find(s => s.ID == hand.Instruction.Properties["TargetID"]);

                //Compute the global position and rotation of the geometry constraint
                target.Position = match.GeometryConstraint.GetGlobalPosition(this.SceneAccess);
                target.Rotation = match.GeometryConstraint.GetGlobalRotation(this.SceneAccess);
            }

            //Gather from the scene
            else
                target = this.SceneAccess.GetTransformByID(hand.Instruction.Properties["TargetID"]);

            return target;
        }


        #endregion



    }

}
