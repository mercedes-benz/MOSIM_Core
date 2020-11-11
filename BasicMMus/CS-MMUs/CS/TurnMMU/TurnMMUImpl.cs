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

namespace TurnMMU
{
    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(TurnMMUImpl)))
            {
                Console.ReadLine();
            }
        }
    }


    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "TurnMMU", "Object/Turn", "", "A turn MMU which models both, one handed and turning object motions.", "The turn MMU is realized using inverse kinematics.")]
    public class TurnMMUImpl : MMUBase
    {


        #region private members

        private ConstraintManager constraintManager;
        private MSimulationState simulationState;
        private MAvatarDescription avatarDescription;

        private List<HandContainer> ActiveHands = new List<HandContainer>();

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        public TurnMMUImpl()
        {
            this.Name = "MoveMMU";
            this.MotionType = "move";
        }




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

            this.avatarDescription = avatarDescription;
            this.ActiveHands = new List<HandContainer>();

            // Added new intermediate skeleton representation. 
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);

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
        [MParameterAttribute("SubjectID", "ID", "The id of the object that should be turned", false)]
        [MParameterAttribute("Hand", "{Left,Right}", "The hand of the carry motion", true)]
        [MParameterAttribute("MaxVelocity", "float", "An optionally defined velocity.", false)]
        [MParameterAttribute("Repetitions", "int", "How many times the turning should be done.", false)]
        [MParameterAttribute("Angle", "float", "The turning angle", false)]
        [MParameterAttribute("Axis", "ID", "The turning axis", false)]
        [MParameterAttribute("MinAngle", "float", "The start angle", false)]
        [MParameterAttribute("MaxAngle", "float", "The end angle", false)]
        //[MParameterAttribute("fixFingerTransformations", "bool", "Specifies whether the finger locations should be fixed", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            this.simulationState = simulationState;

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
            foreach (HandContainer hand in this.ActiveHands)
            {
                if (hand.Trajectory == null && !hand.Instruction.Properties.ContainsKey("TargetID") && hand.TurningAxis == null)
                {
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "At least one of both trajectory or target must be set. Aborting Move MMU");
                    return new MBoolResponse(false);
                }

                //hand.Instruction.Properties.Contains("Repetitions")

                MVector3 handPosition = GetGlobalPosition(simulationState.Initial, hand.JointType);
                MQuaternion handRotation = GetGlobalRotation(simulationState.Initial, hand.JointType);


                //Get the transform of the scene object to be moved
                MTransform sceneObjectTransform = this.SceneAccess.GetTransformByID(hand.Instruction.Properties["SubjectID"]);

                //Compute the relative transform of the hand
                hand.Offset = new MTransform("", sceneObjectTransform.InverseTransformPoint(handPosition), sceneObjectTransform.InverseTransformRotation(handRotation));

                hand.Initialized = true;


                hand.Trajectory = new List<MTransform>();


                if (hand.TurningAxis != null)
                {
                    //Get the initial transform of the subject
                    MTransform initialTransform = hand.Subject.Transform.Clone();

                    //Determine the final transform
                    MTransform finalTransform = null;
                    if (hand.AngleIntervalDefined)
                    {
                        MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, $"Called Turning MMU with turning axis defined. Min angle {hand.MinAngle}, max angle {hand.MaxAngle}.");

                        finalTransform = RotateAround(initialTransform, hand.TurningAxis.Position, hand.TurningAxis.Rotation.Multiply(new MVector3(0, 0, 1)), hand.MinAngle);
                        finalTransform = RotateAround(initialTransform, hand.TurningAxis.Position, hand.TurningAxis.Rotation.Multiply(new MVector3(0, 0, 1)), hand.MaxAngle);
                    }

                    else
                    {
                        MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, $"Called Turning MMU with turning axis defined. Turning angle {hand.TurningAngle}.");

                        finalTransform = RotateAround(initialTransform, hand.TurningAxis.Position, hand.TurningAxis.Rotation.Multiply(new MVector3(0, 0, 1)), hand.TurningAngle);
                    }

                    for (int i = 0; i < hand.Repetitions; i++)
                    {
                        hand.Trajectory.Add(finalTransform);
                        hand.Trajectory.Add(initialTransform);
                    }
                }

                else
                {
                    for (int i = 0; i < hand.Repetitions; i++)
                    {
                        hand.Trajectory.Add(hand.Target.Transform.Clone());
                        hand.Trajectory.Add(hand.Subject.Transform.Clone());
                    }
                }

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
                Events = this.simulationState.Events ?? new List<MSimulationEvent>(),
                DrawingCalls = new List<MDrawingCall>(),
                SceneManipulations = this.simulationState.SceneManipulations ?? new List<MSceneManipulation>(),
                Posture = this.simulationState.Current,
                Constraints = this.simulationState.Constraints ?? new List<MConstraint>()
            };

            List<MConstraint> constraints = result.Constraints;

            //Setup the constraint manager
            this.constraintManager.SetConstraints(ref constraints);

            //Set the simulation sate
            this.simulationState = simulationState;


            //Handle each active hand
            for (int i = this.ActiveHands.Count - 1; i >= 0; i--)
            {
                //Get the current hand
                HandContainer hand = this.ActiveHands[i];

                if (!hand.Initialized)
                    continue;

                //Get the transform of the object to be positioned
                MTransform currentObjectTransform = hand.Subject.Transform;

                //Get the transform of the target
                MTransform targetObjectTransform = null;

                //Determine the next location of the object (at the end of the frame)
                MTransform nextObjectTransform = null;

                //Check if trajectory is defined
                if (hand.Trajectory != null)
                {
                    //The last point is the target transform
                    targetObjectTransform = hand.Trajectory.Last();


                    //Estimate the next transfom based on local motion planning
                    nextObjectTransform = this.DoLocalMotionPlanning(hand.Velocity, hand.AngularVelocity, TimeSpan.FromSeconds(time), currentObjectTransform.Position, currentObjectTransform.Rotation, hand.Trajectory[hand.TrajectoryIndex].Position, hand.Trajectory[hand.TrajectoryIndex].Rotation);


                    float translationDistance = (nextObjectTransform.Position.Subtract(hand.Trajectory[hand.TrajectoryIndex].Position)).Magnitude();
                    double angularDistance = MQuaternionExtensions.Angle(nextObjectTransform.Rotation, hand.Trajectory[hand.TrajectoryIndex].Rotation);


                    //Check if close to current target -> move to next target
                    if (translationDistance < 0.01f && angularDistance < 0.5f && hand.TrajectoryIndex < hand.Trajectory.Count - 1)
                    {
                        hand.TrajectoryIndex++;
                    }
                }

                //Default behavior if no trajectory is specified
                else
                {
                    targetObjectTransform = hand.Target.Transform;

                    //Estimate the next pose of the scene object
                    nextObjectTransform = this.DoLocalMotionPlanning(hand.Velocity, hand.AngularVelocity, TimeSpan.FromSeconds(time), currentObjectTransform.Position, currentObjectTransform.Rotation, targetObjectTransform.Position, targetObjectTransform.Rotation);
                }


                //Set the pose of the object to the next estimated pose
                result.SceneManipulations.Add(new MSceneManipulation()
                {
                    Transforms = new List<MTransformManipulation>()
                     {
                         new MTransformManipulation()
                         {
                             Target = hand.Subject.ID,
                             Position = nextObjectTransform.Position,
                             Rotation = nextObjectTransform.Rotation
                         }
                     }
                });


                //Compute the next handpose based on the offset
                MTransform nextHandTransform = new MTransform("", nextObjectTransform.TransformPoint(hand.Offset.Position), nextObjectTransform.TransformRotation(hand.Offset.Rotation));


                //Set the ik constraints
                constraintManager.SetEndeffectorConstraint(new MJointConstraint(hand.JointType)
                {
                    GeometryConstraint = new MGeometryConstraint("")
                    {
                        ParentToConstraint = new MTransform(Guid.NewGuid().ToString(), nextHandTransform.Position, nextHandTransform.Rotation)
                    }
                });
                    
                   
                //To do add constraints  
                float distance = (nextObjectTransform.Position.Subtract(targetObjectTransform.Position)).Magnitude();
                double angularDist = MQuaternionExtensions.Angle(nextObjectTransform.Rotation, targetObjectTransform.Rotation);

                if (hand.Trajectory != null && hand.TrajectoryIndex < hand.Trajectory.Count - 2)
                {
                    //Do nothing
                }
                else
                {
                    if (distance < 0.01f && angularDist < 0.5f)
                    {
                        //Increment the time
                        hand.ElapsedHoldTime += time;

                        if (hand.ElapsedHoldTime < hand.HoldTime)
                            continue;

                        this.ActiveHands.RemoveAt(i);

                        //Add new finished event
                        if (hand.BothHanded)
                        {
                            if (ActiveHands.Count == 0)
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
            }





            //Use the ik service
            if (result.Constraints.Count > 0)
            {
                MIKServiceResult ikResult = ServiceAccess.IKService.CalculateIKPosture(this.simulationState.Current, result.Constraints, new Dictionary<string, string>());

                result.Posture = ikResult.Posture;

            }

            return result;
        }


        #region private methods


        /// <summary>
        /// Sets up the respective hand
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instruction"></param>
        private void SetupHand(MJointType type, MInstruction instruction)
        {
            HandContainer hand = this.ActiveHands.Find(s => s.JointType == type);

            if (hand != null)
                this.ActiveHands.Remove(hand);


            //Create a new hand
            hand = new HandContainer(type, instruction, true);

            //First extract all parameters
            if (!instruction.Properties.GetValue(out hand.Velocity, "Velocity"))
                hand.Velocity = 1.0f;

            //First extract all parameters
            if (!instruction.Properties.GetValue(out hand.AngularVelocity, "AngularVelocity"))
                hand.AngularVelocity = 30f;

            if (!instruction.Properties.GetValue(out hand.Acceleration, "Acceleration"))
                hand.Acceleration = 1.0f;

            if (!instruction.Properties.GetValue(out hand.Repetitions, "Repetitions"))
                hand.Repetitions = 1;

            if (!instruction.Properties.GetValue(out hand.TurningAngle, "Angle"))
                hand.TurningAngle = 45f;

            if (instruction.Properties.GetValue(out hand.MinAngle, "MinAngle"))
                hand.AngleIntervalDefined = true;

            if (instruction.Properties.GetValue(out hand.MaxAngle, "MaxAngle"))
                hand.AngleIntervalDefined = true;


            instruction.Properties.GetValue(out hand.FixFingerTransformations, "FixFingerTransformations");

            if (instruction.Properties.ContainsKey("Axis"))
                hand.TurningAxis = SceneAccess.GetTransformByID(instruction.Properties["Axis"]);

            if (instruction.Properties.ContainsKey("SubjectID"))
                hand.Subject = this.SceneAccess.GetSceneObjectByID(instruction.Properties["SubjectID"]);

            if(instruction.Properties.ContainsKey("TargetID"))
                hand.Target = this.SceneAccess.GetSceneObjectByID(instruction.Properties["TargetID"]);


            this.ActiveHands.Add(hand);
        }

        /// <summary>
        /// Performs the actual motion planning
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="angularVelocity"></param>
        /// <param name="time"></param>
        /// <param name="currentPosition"></param>
        /// <param name="currentRotation"></param>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <returns></returns>
        private MTransform DoLocalMotionPlanning(float velocity, float angularVelocity, TimeSpan time, MVector3 currentPosition, MQuaternion currentRotation, MVector3 targetPosition, MQuaternion targetRotation)
        {
            MTransform result = new MTransform();

            MVector3 delta = targetPosition.Subtract(currentPosition);
            double angle = Math.Abs(MQuaternionExtensions.Angle(currentRotation, targetRotation));

            double maxTranslationDelta = velocity * time.TotalSeconds;

            if (delta.Magnitude() >= maxTranslationDelta)
            {
                delta = delta.Normalize();
                delta = delta.Multiply(maxTranslationDelta);
            }

            //To do consider self collision



            double maxAngle = angularVelocity * time.TotalSeconds;

            if (angle < maxAngle)
            {
                angle = maxAngle;
            }

            double weight = Math.Min(1, maxAngle / angle);

            result.Position = currentPosition.Add(delta);
            result.Rotation = MQuaternionExtensions.Slerp(currentRotation, targetRotation, (float)weight);
            //result.Time = time;


            return result;
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
            return this.SkeletonAccess.GetGlobalJointPosition(this.avatarDescription.AvatarID, jointType);
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
            return this.SkeletonAccess.GetGlobalJointRotation(this.avatarDescription.AvatarID, jointType);
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
                Position = this.SkeletonAccess.GetGlobalJointPosition(this.avatarDescription.AvatarID, jointType),
                Rotation = this.SkeletonAccess.GetGlobalJointRotation(this.avatarDescription.AvatarID, jointType)
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
            return this.SkeletonAccess.GetRootRotation(this.avatarDescription.AvatarID);
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
            return this.SkeletonAccess.GetRootPosition(this.avatarDescription.AvatarID);
        }


        #endregion


        /// <summary>
        /// Rotates the current transform around the specific point and axis
        /// </summary>
        /// <param name="center">The rotation center</param>
        /// <param name="axis">The rotation axis</param>
        /// <param name="angle">The angle to rotate</param>
        private static MTransform RotateAround(MTransform transform, MVector3 center, MVector3 axis, float angle)
        {
            MTransform res = new MTransform()
            {
                ID = System.Guid.NewGuid().ToString()
            };

            MVector3 pos = transform.Position;

            MQuaternion rot = MQuaternionExtensions.FromEuler(axis.Multiply(angle)); // get the desired rotation
            MVector3 dir = pos.Subtract(center); // find current direction relative to center
            dir = rot.Multiply(dir); // rotate the direction

            res.Position = center.Add(dir); // define new position                                                   
            MQuaternion myRot = transform.Rotation;
            res.Rotation = transform.Rotation.Multiply(MQuaternionExtensions.Inverse(myRot).Multiply(rot).Multiply(myRot));


            return res;
        }


    }
}
