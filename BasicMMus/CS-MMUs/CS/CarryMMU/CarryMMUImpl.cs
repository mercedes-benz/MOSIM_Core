// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMICSharp.Common.Tools;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace CarryMMU
{


    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(CarryMMUImpl)))
            {
                Console.ReadLine();
            }
        }
    }


    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "CarryMMU", "Object/Carry", "", "Implementation of a comprehensive carry MMU", "A carry MMU that supports both, single handed and both handed.")]
    /// <summary>
    /// Implementation of a MMU which is able to carry objects either one or two handed
    /// </summary>
    public class CarryMMUImpl : MMUBase
    {

        #region private fields

        /// <summary>
        /// The joint that is used as reference for both handed carry
        /// </summary>
        private MJointType bothHandedCarryReferenceJoint = MJointType.PelvisCentre;

        /// <summary>
        /// The list of actively used hands by the MMU
        /// </summary>
        private List<HandContainer> ActiveHands = new List<HandContainer>();

        /// <summary>
        /// Specifies whether the object is carried with both hands
        /// </summary>
        private bool bothHandedCarry = false;

        /// <summary>
        /// The relative rotation of the object (relative to root)
        /// </summary>
        private MQuaternion relativeObjectRotation;

        /// <summary>
        /// The relative postion of the object (relative to root)
        /// </summary>
        private MVector3 relativeObjectPosition;

        /// <summary>
        /// The desired carry distance for both handed carry
        /// </summary>
        private float carryDistanceBothHanded = 0.65f;

        /// <summary>
        /// The desired carry height for both handed carry
        /// </summary>
        private float carryHeightBothHanded = 0.2f;

        /// <summary>
        /// The max velocity used for object positioning
        /// </summary>
        private float positionObjectVelocity = 1.0f;

        /// <summary>
        /// The assigned instruction
        /// </summary>
        private MInstruction instruction;

        /// <summary>
        /// The carry target (if defined)
        /// </summary>
        private string CarryTargetName;

        /// <summary>
        /// The internally specified carry transform (relative)
        /// </summary>
        private MTransform internalCarryTransform;

        /// <summary>
        /// Specifies whether the ik is utilized for carrying 
        /// (if disabled the object is fixed at the hand position)
        /// </summary>
        private bool UseCarryIK = false;

        /// <summary>
        /// Separate state for both handed carry
        /// </summary>
        private CarryState bothHandedState = CarryState.None;

        /// <summary>
        /// The avatar description provided by the MMI framework
        /// </summary>
        private MAvatarDescription avatarDescription;


        /// <summary>
        /// The constraint manager to handle the constraint
        /// </summary>
        private ConstraintManager constraintManager;


        /// <summary>
        /// The present simulation state
        /// </summary>
        private MSimulationState simulationState;

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


            this.avatarDescription = avatarDescription;
            this.ActiveHands = new List<HandContainer>();

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
        [MParameterAttribute("TargetID", "ID", "The id of the object which should be carried", true)]
        [MParameterAttribute("Hand", "{Left,Right,Both}", "The hand of the carry motion", true)]
        [MParameterAttribute("AddOffset", "bool", "Specifies whether an offset is automatically added to the carry position considering the object dimensions", false)]
        [MParameterAttribute("CarryTargetID", "ID", "Specifies an optional carry target. If defined, this is used instead of the underlying posture as target.", false)]
        [MParameterAttribute("Velocity", "float", "The max velocity of the hand motions contained in the carry motion.", false)]
        [MParameterAttribute("CarryDistance", "float", "Specifies an optional distance for both handed carrying without a carry target.", false)]
        [MParameterAttribute("CarryHeight", "float", "Specifies an optional height for both handed carrying without a carry target.", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Reset the properties
            this.UseCarryIK = false;
            this.bothHandedCarry = false;
            this.bothHandedState = CarryState.None;
            this.simulationState = simulationState;


            //Create a list which contains the hands which should be considered for carrying
            List<HandContainer> toPlan = new List<HandContainer>();

            if (instruction.Properties == null)
                throw new Exception($"{this.Name}: No properties defined!");

            //Extract the hand information
            if (instruction.Properties.ContainsKey("Hand"))
            {
                switch (instruction.Properties["Hand"])
                {
                    case "Left":
                        toPlan.Add(this.SetupHand(HandType.Left, instruction));
                        this.bothHandedCarry = false;
                        break;

                    case "Right":
                        toPlan.Add(this.SetupHand(HandType.Right, instruction));
                        this.bothHandedCarry = false;
                        break;

                    case "Both":
                        //Set flag for both handed carry
                        this.bothHandedCarry = true;
                        toPlan.Add(this.SetupHand(HandType.Left, instruction));
                        toPlan.Add(this.SetupHand(HandType.Right, instruction));
                        break;
                }
            }
            else
                toPlan.Add(this.SetupHand(HandType.Right, instruction));


            //Use the carry target if defined
            if (!instruction.Properties.GetValue(out this.CarryTargetName, "CarryTarget"))
                this.CarryTargetName = null;

            //Use the carry target if defined
            if (!instruction.Properties.GetValue(out this.UseCarryIK, "UseCarryIK"))
                UseCarryIK = false;

            //Use carry distance if defined
            if (!instruction.Properties.GetValue(out this.carryDistanceBothHanded, "CarryDistance"))
                carryDistanceBothHanded = 0.65f;

            //Use carry distance if defined
            if (!instruction.Properties.GetValue(out this.carryHeightBothHanded, "CarryHeight"))
                carryHeightBothHanded = 0.2f;

            //Use carry distance if defined
            if (!instruction.Properties.GetValue(out this.positionObjectVelocity, "Velocity"))
                this.positionObjectVelocity = 1.0f;


            //Compute and plan the relevant aspects of each hand
            foreach (HandContainer hand in toPlan)
            {
                //Get the (initial) hand transform
                MTransform handTransform = this.GetTransform(simulationState.Initial, hand.Type);

                //Get the current transform of the scene object
                MTransform sceneObjectTransform = this.SceneAccess.GetTransformByID(hand.Instruction.Properties["TargetID"]);

                //Get the hand pose
                try
                {
                    hand.HandPose = GetTransform(simulationState.Initial, hand.Type);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Problem estimating hand pose: " + e.Message + e.StackTrace);
                }

                //Compute the relative transform of the hand (hand relative to object)
                hand.HandOffset = new MTransform("", sceneObjectTransform.InverseTransformPoint(handTransform.Position), sceneObjectTransform.InverseTransformRotation(handTransform.Rotation));

                //Compute the inverse offset (object relative to hand)
                hand.ObjectOffset = new MTransform("", handTransform.InverseTransformPoint(sceneObjectTransform.Position), handTransform.InverseTransformRotation(sceneObjectTransform.Rotation));

                //Set state to positioning
                hand.State = CarryState.Positioning;
            }


            //Do additional computations for both handed carry
            if (bothHandedCarry)
            {
                //Set the state to positioning
                this.bothHandedState = CarryState.Positioning;

                //Assign the instruction
                this.instruction = instruction;

                //Get the current object transorm
                MTransform currentObjectTransform = this.SceneAccess.GetTransformByID(this.instruction.Properties["TargetID"]);

                //Get the current root transform -> to do
                MTransform rootTransform = GetTransform(this.simulationState.Initial, MJointType.PelvisCentre);

                //Compute the relative object transform
                this.relativeObjectRotation = rootTransform.InverseTransformRotation(currentObjectTransform.Rotation);
                this.relativeObjectPosition = rootTransform.InverseTransformPoint(currentObjectTransform.Position);

                //Manually specify a carry target
                if(this.CarryTargetName == null ||this.CarryTargetName.Length ==0)
                {
                    MTransform refTransform = GetTransform(this.simulationState.Initial, bothHandedCarryReferenceJoint);
                    MVector3 forward = GetRootForwad(this.simulationState.Initial);

                    //Determine the ref transform rotation just consider the y axis rotation
                    refTransform.Rotation = MQuaternionExtensions.FromEuler(new MVector3(0, Extensions.SignedAngle(new MVector3(0, 0, 1), forward,new MVector3(0,1,0)), 0));

                    //Compute the delta
                    //MVector3 delta = currentObjectTransform.Position.Subtract(refTransform.Position);
                    //MVector3 direction = new MVector3(delta.X, 0, delta.Z).Normalize();
                     
                    //The carry position i
                    MVector3 carryPosition = refTransform.Position.Add(forward.Multiply(this.carryDistanceBothHanded)).Add(new MVector3(0, carryHeightBothHanded, 0f));

                    //Forwad + offset
                    this.internalCarryTransform = new MTransform("CarryTarget", refTransform.InverseTransformPoint(carryPosition), refTransform.InverseTransformRotation(currentObjectTransform.Rotation));
                }

            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Do step method which computes the actual carry postures
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Set the current avatar state
            this.simulationState = simulationState;

            //Create a new result
            MSimulationResult result = new MSimulationResult()
            {
                Events = simulationState.Events ?? new List<MSimulationEvent>(),
                DrawingCalls = new List<MDrawingCall>(),
                SceneManipulations = simulationState.SceneManipulations ?? new List<MSceneManipulation>(),
                Posture = simulationState.Current,
                Constraints = simulationState.Constraints ?? new List<MConstraint>()
            };
            //Special case for both handed carry
            if (bothHandedCarry)
                this.DoStepBothHanded(result, time, simulationState);

            //Single handed carry ->Perform a single handed do step
            else
                result = this.DoStepSingleHanded(result, time, simulationState);
            

            //Return the computed result
            return result;
        }


        /// <summary>
        /// Performs the do step realted computations for the single hand mode
        /// </summary>
        /// <param name="result"></param>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        private MSimulationResult DoStepSingleHanded(MSimulationResult result, double time, MSimulationState simulationState)
        {
            //The presently active constraints
            List<MConstraint> globalConstraints = result.Constraints;

            //Operate on the local constraints
            this.constraintManager.SetConstraints(ref globalConstraints);


            //Handle each active hand
            for (int i = this.ActiveHands.Count - 1; i >= 0; i--)
            {
                //Get the current hand container
                HandContainer hand = this.ActiveHands[i];

                //Handle the state
                switch (hand.State)
                {
                    case CarryState.Positioning:
                        //Call positioning of single hand and add the resulting ik properties (e.g. hand position/Rotation)           
                        this.PositionObjectSingleHand(ref result, time, hand);
                        break;

                    case CarryState.Carry:
                        //Call the single handed carry and add the resulting ik properties (e.g. hand position/rotation)
                        this.SingleHandedCarry(ref result, time, hand);                   
                        break;
                }
            }


            //Get the joint constraints
            List<MConstraint> jointConstraints = this.constraintManager.GetJointConstraints();

            //Solve using ik if constraints are defined
            if (jointConstraints.Count > 0)
            {
                MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(result.Posture, jointConstraints, new Dictionary<string, string>());
                result.Posture = ikResult.Posture;
            }

            return result;
        }


        /// <summary>
        /// Performs the do step realted computations for both handed mode
        /// </summary>
        /// <param name="result"></param>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        private MSimulationResult DoStepBothHanded(MSimulationResult result, double time, MSimulationState simulationState)
        {
            //The presently active constraints
            List<MConstraint> globalConstraints = result.Constraints;

            //Operate on the local constraints
            this.constraintManager.SetConstraints(ref globalConstraints);


            switch (this.bothHandedState)
            {
                //First position the object in order to carry it properly
                case CarryState.Positioning:

                    //Perform both handed positioning
                    this.PositionObjectBothHanded(ref result, time);

                    //Solve using ik if constraints are defined
                    if (this.constraintManager.GetJointConstraints().Count > 0)
                    {
                        MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(result.Posture, this.constraintManager.GetJointConstraints(), new Dictionary<string, string>());
                        result.Posture = ikResult.Posture;
                    }

                    break;

                case CarryState.Carry:

                    //Perform a both handed carry (just preserve the relative position and rotation)
                    this.BothHandedCarry(ref result);

                    //Solve using ik if constraints are defined
                    if (this.constraintManager.GetJointConstraints().Count > 0)
                    {
                        MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(result.Posture, this.constraintManager.GetJointConstraints(), new Dictionary<string, string>());
                        result.Posture = ikResult.Posture;
                    }

                    break;
            }

            return result;

        }

        /// <summary>
        /// To do in future use instruction as argument
        /// Because it is not know which instruction should be aborted
        /// </summary>
        /// <returns></returns>
        public override MBoolResponse Abort(string instructionID)
        {
            //To do -> identify the correct hand which should be aborted

            HandContainer hand = this.ActiveHands.Find(s => s.Instruction.ID == instructionID);

            if (hand != null)
            {
                this.ActiveHands.Remove(hand);
            }

            else
            {
                Console.WriteLine("Carry aborted");
                this.ActiveHands.Clear();
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Checks the prerequisites for the carry motion -> Must be in reach
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public override MBoolResponse CheckPrerequisites(MInstruction instruction)
        {
            ////Fetch the location of the target object
            //if (instruction.Properties.ContainsKey("TargetID"))
            //{
            //    MTransform targetTransform = this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);

            //    MAvatar avatar = this.SceneAccess.GetAvatarByID(this.AvatarDescription.AvatarID);

            //    if (avatar != null && targetTransform != null)
            //    {
            //        MVector3 handPosition = new MVector3();

                   

            //        //To do -> Use the hand position
            //        if (instruction.Properties.ContainsKey("Hand"))
            //        {
            //            switch (instruction.Properties["Hand"])
            //            {
            //                case "Left":
            //                    handPosition = GetGlobalPosition(avatar.PostureValues, HandType.Left);
            //                    break;

            //                case "Right":
            //                    handPosition = GetGlobalPosition(avatar.PostureValues, HandType.Right);

            //                    break;

            //                case "Both":
            //                    //To do

            //                    //Check both
            //                    break;
            //            }
            //        }
            //        //Estimate the distance between object and hand
            //        double delta = Distance(handPosition, targetTransform.Position);

            //        Logger.Log(Log_level.L_INFO, "Distance: " + delta);

            //        if (delta < 0.1f)
            //        {
            //            Logger.Log(Log_level.L_INFO, "CheckPrerequisites: fulfilled");
            //            return new MBoolResponse(true);
            //        }
            //        else
            //        {
            //            Logger.Log(Log_level.L_INFO, "CheckPrerequisites not fulfilled");

            //            return new MBoolResponse(false);
            //        }
            //    }
            //}

            return new MBoolResponse(true);


        }


        #region private methods


        /// <summary>
        /// Method is responsible for modeling the positiong the object and hands which is the first part of the carry for both handed objects
        /// </summary>
        /// <param name="result"></param>
        /// <param name="time"></param>
        private void PositionObjectBothHanded(ref MSimulationResult result, double time)
        {
            double rootVelocity = this.ComputeRootVelocity(time);


            MAvatarPostureValues avatarPose = this.simulationState.Initial;
            MTransform currentObjectTransform = this.SceneAccess.GetTransformByID(this.instruction.Properties["TargetID"]);


            //Move the object to a central spot in front of the avatar
            //Create a new transform for the target object transform
            MTransform targetObjectTransform = new MTransform();


            if (this.CarryTargetName != null && this.CarryTargetName.Length >0)
            {
                MTransform targetTransform = SceneAccess.GetTransformByID(this.CarryTargetName);
                targetObjectTransform.Position = targetTransform.Position;
                targetObjectTransform.Rotation = targetTransform.Rotation;
            }

            else
            {
                MTransform refTransform = GetTransform(this.simulationState.Initial, bothHandedCarryReferenceJoint);
                MVector3 forward = GetRootForwad(this.simulationState.Initial);
                //Determine the ref transform rotation
                refTransform.Rotation = MQuaternionExtensions.FromEuler(new MVector3(0, Extensions.SignedAngle(new MVector3(0, 0, 1), forward, new MVector3(0, 1, 0)), 0));

                targetObjectTransform.Position = refTransform.TransformPoint(this.internalCarryTransform.Position);
                targetObjectTransform.Rotation = refTransform.TransformRotation(this.internalCarryTransform.Rotation);
            }

            MTransform nextObjectPose = this.DoLocalMotionPlanning(rootVelocity + positionObjectVelocity, TimeSpan.FromSeconds(time), currentObjectTransform.Position, currentObjectTransform.Rotation, targetObjectTransform.Position, targetObjectTransform.Rotation);
            MTransform nextObjectTransform = new MTransform("",nextObjectPose.Position, nextObjectPose.Rotation);


            //Update the position of the object
            result.SceneManipulations.Add(new MSceneManipulation()
            {

                Transforms = new List<MTransformManipulation>()
                {
                    new MTransformManipulation()
                    {
                            Target = instruction.Properties["TargetID"],
                            Position = nextObjectPose.Position,
                            Rotation = nextObjectPose.Rotation
                    }
                }
            });

            //Update the hands
            foreach (HandContainer hand in this.ActiveHands)
            {
                //Update the hands
                MTransform nextHandPose = new MTransform("",nextObjectTransform.TransformPoint(hand.HandOffset.Position), nextObjectTransform.TransformRotation(hand.HandOffset.Rotation));

                //Set a new endeffector constraint
                this.constraintManager.SetEndeffectorConstraint(hand.JointType, nextHandPose.Position, nextHandPose.Rotation, hand.ConstraintID);

                //Assign the hand pose to preserve finger rotations
                result.Posture = AssignHandPose(result.Posture,hand.HandPose,hand.Type);
            }



            //Check if position is finished
            if ((targetObjectTransform.Position.Subtract(nextObjectPose.Position)).Magnitude() < 0.01f && MQuaternionExtensions.Angle(targetObjectTransform.Rotation, nextObjectPose.Rotation) < 0.1f)
            {
                result.Events.Add(new MSimulationEvent("PositioningFinished", "PositioningFinished", instruction.ID));

                //Only consider the rotation around y axis
                double yRotation = this.GetRootRotation(this.simulationState.Initial).ToEuler().Y;
                    
                MTransform rootTransform = new MTransform("", this.GetRootPosition(this.simulationState.Initial), MQuaternionExtensions.FromEuler(new MVector3(0, yRotation, 0)));

                //Update the new relative coordinates
                this.relativeObjectRotation = rootTransform.InverseTransformRotation(nextObjectTransform.Rotation);
                this.relativeObjectPosition = rootTransform.InverseTransformPoint(nextObjectTransform.Position);

                this.bothHandedState = CarryState.Carry;

                //Get the joint constraints
                List<MConstraint> jointConstraints = this.constraintManager.GetJointConstraints();

                //Solve using ik if constraints are defined
                if (jointConstraints.Count > 0)
                {
                    MIKServiceResult ikResult = this.ServiceAccess.IKService.CalculateIKPosture(result.Posture, jointConstraints, new Dictionary<string, string>());
                    result.Posture = ikResult.Posture;
                }
            }
        }


        /// <summary>
        /// Performs the position step for a single hand
        /// </summary>
        /// <param name="result"></param>
        /// <param name="time"></param>
        /// <param name="hand"></param>
        /// <returns></returns>
        private void PositionObjectSingleHand(ref MSimulationResult result, double time, HandContainer hand)
        {
            //Compute the root velocity
            double rootVelocity = this.ComputeRootVelocity(time);

            //The current hand transform (the approved result of the last frame)
            MTransform currentHandTransform = GetTransform(simulationState.Initial, hand.Type);

            //The desired hand transform (of the underlying animation)
            MTransform targetHandTransform = GetTransform(simulationState.Current, hand.Type);


            //Check if for the hand a carry target is defined
            if (hand.CarryTargetName != null)
            {
                //Get the target transform if a carry target is defined
                MTransform carryTargetTransform = SceneAccess.GetTransformByID(this.CarryTargetName);

                //Compute the global position of the respective hand based on the object
                targetHandTransform.Position = carryTargetTransform.TransformPoint(hand.HandOffset.Position);
                targetHandTransform.Rotation = carryTargetTransform.TransformRotation(hand.HandOffset.Rotation);
            }


            rootVelocity = 0f;

            //Compute the new hand pose at the end of this frame
            MTransform nextHandTransform = this.DoLocalMotionPlanning(rootVelocity + hand.Velocity, TimeSpan.FromSeconds(time), currentHandTransform.Position, currentHandTransform.Rotation, targetHandTransform.Position, targetHandTransform.Rotation);


            //Compute the object transform
            result.SceneManipulations.Add(new MSceneManipulation()
            {
                Transforms = new List<MTransformManipulation>()
                {
                    new MTransformManipulation()
                    {
                        Target = hand.Instruction.Properties["TargetID"],
                        //Compute the object location with respect to the offset
                        Position = nextHandTransform.TransformPoint(hand.ObjectOffset.Position),
                        Rotation = nextHandTransform.TransformRotation(hand.ObjectOffset.Rotation)
                    }
                }
            });


            float translationDistance = targetHandTransform.Position.Subtract(nextHandTransform.Position).Magnitude();
            float angularDistance = (float)MQuaternionExtensions.Angle(nextHandTransform.Rotation, targetHandTransform.Rotation);

            //Check if goal reached -> change state
            if (translationDistance < 0.01f && angularDistance < 2)
            {
                result.Events.Add(new MSimulationEvent("PositioningFinished", "PositioningFinished", hand.Instruction.ID));

                //Finally in carry state
                hand.State = CarryState.Carry;

                //Set the constraint if the carry ik is enabled
                if (UseCarryIK ||hand.CarryTargetName !=null)
                {
                    //Remove the endeffector constraint no carry ik
                    this.constraintManager.SetEndeffectorConstraint(hand.JointType, nextHandTransform.Position, nextHandTransform.Rotation, hand.ConstraintID);
                }
                else
                {
                    //Remove the endeffector constraint no carry ik
                    this.constraintManager.RemoveEndeffectorConstraints(hand.JointType);
                }
            }

            //Not finished
            else
            {
                //Set the position and rotation parameters of the ik
                this.constraintManager.SetEndeffectorConstraint(hand.JointType, nextHandTransform.Position, nextHandTransform.Rotation, hand.ConstraintID);
            }
        }



        /// <summary>
        /// Method is repsonsible for modeling the actual carry for both handed objects
        /// </summary>
        /// <param name="result"></param>
        private void BothHandedCarry(ref MSimulationResult result)
        {
            //Create an empty transform representing the next object transform
            MTransform nextObjectTransform = new MTransform();


            //Update the desired object transform
            if (this.CarryTargetName != null && this.CarryTargetName.Length >0)
            {
                MTransform targetTransform = SceneAccess.GetTransformByID(this.CarryTargetName);
                nextObjectTransform.Position = targetTransform.Position;
                nextObjectTransform.Rotation = targetTransform.Rotation;
            }
            else
            {
                MTransform refTransform = GetTransform(this.simulationState.Initial, bothHandedCarryReferenceJoint);
                MVector3 forward = GetRootForwad(this.simulationState.Initial);
                //Determine the ref transform rotation
                refTransform.Rotation = MQuaternionExtensions.FromEuler(new MVector3(0, Extensions.SignedAngle(new MVector3(0, 0, 1), forward, new MVector3(0, 1, 0)), 0));

                nextObjectTransform.Position = refTransform.TransformPoint(this.internalCarryTransform.Position);
                nextObjectTransform.Rotation = refTransform.TransformRotation(this.internalCarryTransform.Rotation);
            }



            //Compute the object transform
            result.SceneManipulations.Add(new MSceneManipulation()
            {
                Transforms = new List<MTransformManipulation>()
                {
                    new MTransformManipulation()
                    {
                        Target = instruction.Properties["TargetID"],
                        Position = nextObjectTransform.Position,
                        Rotation = nextObjectTransform.Rotation
                    }
                }
            });


            List<MIKProperty> ikProperties = new List<MIKProperty>();


            //Update the hands
            foreach (HandContainer hand in this.ActiveHands)
            {
                //Update the hands
                MTransform nextHandPose = new MTransform("",nextObjectTransform.TransformPoint(hand.HandOffset.Position), nextObjectTransform.TransformRotation(hand.HandOffset.Rotation));

                this.constraintManager.SetEndeffectorConstraint(hand.JointType, nextHandPose.Position, nextHandPose.Rotation, hand.ConstraintID);
            }

  
        }


        /// <summary>
        /// Performs a single handed carry
        /// Just sets the object relative to the hand
        /// </summary>
        /// <param name="result"></param>
        /// <param name="hand"></param>
        /// <returns></returns>
        private List<MIKProperty> SingleHandedCarry(ref MSimulationResult result, double time, HandContainer hand)
        {
            List<MIKProperty> ikProperties = new List<MIKProperty>();

            //Check if a carry target is specified
            if (hand.CarryTargetName != null)
            {
                //Compute the root velocity
                double rootVelocity = this.ComputeRootVelocity(time);

                //Get the current transform of the carry target for the respective hand 
                MTransform targetTr = SceneAccess.GetTransformByID(this.CarryTargetName);

                //The transform of the carry target
                MTransform targetTransform = new MTransform("", targetTr.Position, targetTr.Rotation);

                //Compute the global position of the respective hand based on the object
                MVector3 targetHandPosition = targetTransform.TransformPoint(hand.HandOffset.Position);
                MQuaternion targetHandRotation = targetTransform.TransformRotation(hand.HandOffset.Rotation);

                //Get the current hand transform
                MTransform currentHandTransform = GetTransform(simulationState.Initial, hand.Type);

                //Compute the new hand pose
                MTransform nextHandPose = this.DoLocalMotionPlanning(rootVelocity + hand.Velocity, TimeSpan.FromSeconds(time), currentHandTransform.Position, currentHandTransform.Rotation, targetHandPosition, targetHandRotation);

                //Compute the object transform
                result.SceneManipulations.Add(new MSceneManipulation()
                {

                    Transforms = new List<MTransformManipulation>()
                        {
                        new MTransformManipulation()
                        {
                                Target = hand.Instruction.Properties["TargetID"],
                                Position = nextHandPose.TransformPoint(hand.ObjectOffset.Position),
                                Rotation = nextHandPose.TransformRotation(hand.ObjectOffset.Rotation)
                        }
                    }
                });

                //Set the position and rotation parameters of the ik
                this.constraintManager.SetEndeffectorConstraint(hand.JointType, nextHandPose.Position, nextHandPose.Rotation, hand.ConstraintID);
                
            }

            //Just set the object relative to the current hand 
            else
            {
                //Create a transform representing the hand transform for the planned frame
                MTransform handTransform = GetTransform(simulationState.Current, hand.Type);

                if (this.UseCarryIK)
                {
                    this.constraintManager.SetEndeffectorConstraint(hand.JointType, handTransform.Position, handTransform.Rotation, hand.ConstraintID);
                }



                //Compute the object transform
                result.SceneManipulations.Add(new MSceneManipulation()
                {
                    Transforms = new List<MTransformManipulation>()
                     {
                        new MTransformManipulation()
                        {
                              Target = hand.Instruction.Properties["TargetID"],
                              Position = handTransform.TransformPoint(hand.ObjectOffset.Position),
                              Rotation = handTransform.TransformRotation(hand.ObjectOffset.Rotation)
                        }
                    }
                });
            }


            //To do optionally consider self-collisions
            return ikProperties;
        }


        /// <summary>
        /// Computes the root velocity of the avatar
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private double ComputeRootVelocity(double time)
        {
            //Get the root position
            this.SkeletonAccess.SetChannelData(simulationState.Current);
            var currentRootPosition = this.SkeletonAccess.GetRootPosition(this.avatarDescription.AvatarID);

            this.SkeletonAccess.SetChannelData(simulationState.Current);
            var previousRootPosition = this.SkeletonAccess.GetRootPosition(this.avatarDescription.AvatarID);



            previousRootPosition.Y = 0;
            currentRootPosition.Y = 0;

            //Estimate the root velocity
            return previousRootPosition.Subtract(currentRootPosition).Magnitude() / time;
        }

        /// <summary>
        /// Returns the translation distance to move the hand from the initial state to the current state
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private float GetHandDistance(HandType type)
        {
            MVector3 targetHandPosition = this.GetGlobalPosition(simulationState.Current, type);

            MVector3 currentHandPosition = this.GetGlobalPosition(simulationState.Initial, type);

            return targetHandPosition.Subtract(currentHandPosition).Magnitude();
        }


        #region helper methods

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
                Position = this.SkeletonAccess.GetRootPosition(this.avatarDescription.AvatarID),
                Rotation = this.SkeletonAccess.GetRootRotation(this.avatarDescription.AvatarID)
            };
        }

        /// <summary>
        /// Returns the forward vector of the root transform
        /// </summary>
        /// <param name="posture"></param>
        /// <returns></returns>
        private MVector3 GetRootForwad(MAvatarPostureValues posture)
        {
            MTransform rootTransform = this.GetRootTransform(posture);
            //Compute the forwad vector of the root transform
            MVector3 rootForward = rootTransform.Rotation.Multiply(new MVector3(0, 0, 1));
            rootForward.Y = 0;
            rootForward = rootForward.Normalize();

            return rootForward;
        }


        private MVector3 GetGlobalPosition(MAvatarPostureValues posture, HandType type)
        {
            return GetGlobalPosition(posture, type == HandType.Left ? MJointType.LeftWrist : MJointType.RightWrist);
        }

        private MQuaternion GetGlobalRotation(MAvatarPostureValues posture, HandType type)
        {
            return GetGlobalRotation(posture, type == HandType.Left ? MJointType.LeftWrist : MJointType.RightWrist);
        }

        private MTransform GetTransform(MAvatarPostureValues posture, HandType type)
        {
            return GetTransform(posture, type == HandType.Left ? MJointType.LeftWrist : MJointType.RightWrist);
        }


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

        //To do
        private MAvatarPostureValues AssignBoneRotations(MAvatarPostureValues target, MAvatarPostureValues source, List<MJointType> joints)
        {
            //To do

            return target;
        }

        //To do
        private MAvatarPostureValues AssignHandPose(MAvatarPostureValues posture, MTransform handPose, HandType type)
        {
            //this.SkeletonAccess.SetChannelData(posture);
            return posture;

        }

        #endregion

        /// <summary>
        /// Setups the hand
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private HandContainer SetupHand(HandType type, MInstruction instruction)
        {
            HandContainer hand = this.ActiveHands.Find(s => s.Type == type);

            if (hand != null)
            {
                this.ActiveHands.Remove(hand);
            }


            //Create a new hand
            hand = new HandContainer(type, instruction, true)
            {
                BlendStartPosture = this.simulationState.Initial.Copy(),
                ElapsedBlendTime = 0,
                CarryTargetName = null
            };
            //Compute the blend time based on the distance and velocity

            hand.BlendDuration = GetHandDistance(hand.Type) / hand.Velocity;


            //First extract all parameters
            if (instruction.Properties.ContainsKey("Velocity"))
                hand.Velocity = float.Parse(instruction.Properties["Velocity"], System.Globalization.CultureInfo.InvariantCulture);

            //Use the carry target if defined
            if (instruction.Properties.ContainsKey("CarryTarget"))
                hand.CarryTargetName = instruction.Properties["CarryTarget"];


            this.ActiveHands.Add(hand);

            return hand;
        }


        /// <summary>
        /// Performs a local motion planning to estimate the next pose
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="time"></param>
        /// <param name="currentPosition"></param>
        /// <param name="currentRotation"></param>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <returns></returns>
        private MTransform DoLocalMotionPlanning(double velocity, TimeSpan time, MVector3 currentPosition, MQuaternion currentRotation, MVector3 targetPosition, MQuaternion targetRotation)
        {
            //Create a resulting transform
            MTransform result = new MTransform();


            //Estimate the delta
            MVector3 deltaPosition = targetPosition.Subtract(currentPosition);

            //Estimate the meximum allowed delta
            double maxTranslationDelta = velocity * time.TotalSeconds;

            //Limit the maximum 
            if (deltaPosition.Magnitude() >= maxTranslationDelta)
            {
                deltaPosition = deltaPosition.Normalize();
                deltaPosition = deltaPosition.Multiply(maxTranslationDelta);
            }


            float angularVelocityReach = 100f;
            double angle = Math.Abs(MQuaternionExtensions.Angle(currentRotation, targetRotation));

            double maxAngle = angularVelocityReach * time.TotalSeconds;

            //Estimate the blendweihgt for the oreitnation blending
            double weight = Math.Min(1, maxAngle / angle);

            result.Position = currentPosition.Add(deltaPosition);
            result.Rotation = MQuaternionExtensions.Slerp(currentRotation, targetRotation, (float)weight);
            //result.Time = time;


            return result;
        }


        private double Distance(MVector3 v1, MVector3 v2)
        {
            return v1.Subtract(v2).Magnitude();
        }




        #endregion
    }
}
