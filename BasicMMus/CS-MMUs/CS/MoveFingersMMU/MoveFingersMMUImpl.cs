// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;


namespace MoveFingersMMU
{

    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(MoveFingersMMUImpl)))
            {
                Console.ReadLine();
            }
        }
    }

    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "MoveFingersMMU", "Pose/MoveFingers", "", "MMU allows to manipulate the finger joints by means of motion blending.", "MMU allows to manipulate the finger joints by means of motion blending.")]
    public class MoveFingersMMUImpl : MMUBase
    {

        private readonly List<HandContainer> ActiveHands = new List<HandContainer>();


        [MParameterAttribute("Hand", "Left/Right", "The hand type.", true)]
        [MParameterAttribute("Release", "bool", "Specifies whether the hand posture should be released.", false)]
        [MParameterAttribute("HandPose", "PostureConstraint", "The desired hand pose.", false)]
        [MParameterAttribute("Duration", "float", "The desired duration until the pose is established.", false)]
        [MParameterAttribute("AngularVelocity", "float", "The max angular velocity of the finger motions.", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {

            //Create a new instance of the skeleton access /intermedaite skeleton
            this.SkeletonAccess = new IntermediateSkeleton();

            //Setup the anthropometry
            this.SkeletonAccess.InitializeAnthropometry(this.AvatarDescription);

            base.AssignInstruction(instruction, simulationState);



            //Parse the duration parameter (if defined)
            bool durationSet = false;
            float duration = 1f;
            if (instruction.Properties.ContainsKey("Duration"))
                duration = float.Parse(instruction.Properties["Duration"], System.Globalization.CultureInfo.InvariantCulture);

            //Parse the angular velocity parameter (if defined)
            float angularVelocity = 90f;
            if (instruction.Properties.ContainsKey("AngularVelocity"))
                angularVelocity = float.Parse(instruction.Properties["AngularVelocity"], System.Globalization.CultureInfo.InvariantCulture);


            bool release = false;

            //Parse the release parameter (if defined)     
            if (instruction.Properties.ContainsKey("Release"))
                release = bool.Parse(instruction.Properties["Release"]);


            //If release is not defined -> the handposture is used
            if (instruction.Properties.ContainsKey("Hand"))
            {
                switch (instruction.Properties["Hand"])
                {
                    case "Left":
                        this.SetupHand(MJointType.LeftWrist, instruction, duration, angularVelocity, durationSet, release);
                        break;

                    case "Right":
                        this.SetupHand(MJointType.RightWrist, instruction, duration, angularVelocity, durationSet, release);
                        break;                  
                }
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Basic do step routine
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MSimulationEventAttribute("Fingers are positioned", "FingersPositioned")]
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

            //Cast to intermediate skeleton to get all functions
            IntermediateSkeleton iS = this.SkeletonAccess as IntermediateSkeleton;


            //Assign the approved posture of the last frame (proper finger rotations)
            this.SkeletonAccess.SetChannelData(simulationState.Initial);

            //First estimate the finger rotations
            //------------------------------------------------------------------------------------------------------------------

            //Handle each active hand
            for (int i = this.ActiveHands.Count - 1; i >= 0; i--)
            {
                //Get the current hand
                HandContainer hand = this.ActiveHands[i];

                if (hand.Release)
                {
                    //Use all finger joint if in release mode
                    foreach(MJointType jointType in fingerJoints)
                    {
                        //Get the current rotation of the finger
                        if (!hand.CurrentFingerRotations.ContainsKey(jointType))
                            hand.CurrentFingerRotations.Add(jointType, iS.GetLocalJointRotation(this.AvatarDescription.AvatarID, jointType));
                        else
                            hand.CurrentFingerRotations[jointType] = iS.GetLocalJointRotation(this.AvatarDescription.AvatarID, jointType);
                    }
                }

                else
                {
                    //Handle all joint constraints -> Use only the fingers that are constrained
                    foreach (MJointConstraint joint in hand.FinalPosture.JointConstraints)
                    {
                        //Skip if the joint is no finger joint
                        if (!this.IsFingerJoint(joint.JointType))
                            continue;

                        //Get the current rotation of the finger
                        if (!hand.CurrentFingerRotations.ContainsKey(joint.JointType))
                            hand.CurrentFingerRotations.Add(joint.JointType, iS.GetLocalJointRotation(this.AvatarDescription.AvatarID, joint.JointType));
                        else
                            hand.CurrentFingerRotations[joint.JointType] = iS.GetLocalJointRotation(this.AvatarDescription.AvatarID, joint.JointType);
                    }
                }
            }


            //Perform the blending
            //------------------------------------------------------------------------------------------------------------------

            //Assign the approved posture of the last frame (proper finger rotations)
            this.SkeletonAccess.SetChannelData(simulationState.Current);


            //Handle each active hand
            for (int i = this.ActiveHands.Count - 1; i >= 0; i--)
            {
                //Get the current hand
                HandContainer hand = this.ActiveHands[i];

                //Flag which indicates whether the fingers are positioned
                bool positioned = true;


                if (hand.Release)
                {
                    //If in release mode again use all joints being finger joints
                    foreach(MJointType jointType in fingerJoints)
                    {
                        //Get the current rotation of the finger
                        MQuaternion currentRotation = hand.CurrentFingerRotations[jointType];

                        //The current rotation is the result of the last frame
                        MQuaternion desiredRotation = iS.GetLocalJointRotation(this.AvatarDescription.AvatarID, jointType);

                        //The weight used for blending
                        double weight = 0;

                        //The angle between the current rotation and the desired one
                        double angle = MQuaternionExtensions.Angle(currentRotation, desiredRotation);


                        //Compute the weight based on the elapsed time if the duration is set
                        if (hand.HasDuration)
                            weight = hand.Elapsed / hand.Duration;

                        //By default use the angular velocity
                        else
                        {
                            //The max allowed angle in this frame
                            double maxAngle = time * hand.AngularVelocity;
                            weight = Math.Min(1, maxAngle / angle);
                        }

                        //Compute the new rotation
                        MQuaternion newRotation = MQuaternionExtensions.Slerp(currentRotation, desiredRotation, (float)weight);

                        //Set the local joint rotation of the intermediate skeleton
                        iS.SetLocalJointRotation(this.AvatarDescription.AvatarID, jointType, newRotation);

                        //Goal criteria
                        if (angle > 0.1f)
                            positioned = false;

                    }

                }

                else
                {
                    //Handle all joint constraints
                    foreach (MJointConstraint joint in hand.FinalPosture.JointConstraints)
                    {
                        //Skip if the joint is no finger joint
                        if (!this.IsFingerJoint(joint.JointType))
                            continue;

                        //Get the current rotation of the finger
                        MQuaternion currentRotation = hand.CurrentFingerRotations[joint.JointType];

                        //Get the desired rotation
                        MQuaternion desiredRotation = joint.GeometryConstraint.ParentToConstraint.Rotation;
                  
                        //The weight used for blending
                        double weight = 0;

                        //The angle between the current rotation and the desired one
                        double angle = MQuaternionExtensions.Angle(currentRotation, desiredRotation);


                        //Compute the weight based on the elapsed time if the duration is set
                        if (hand.HasDuration)
                            weight = hand.Elapsed / hand.Duration;

                        //By default use the angular velocity
                        else
                        {
                            //The max allowed angle in this frame
                            double maxAngle = time * hand.AngularVelocity;
                            weight = Math.Min(1, maxAngle / angle);
                        }

                        //Compute the new rotation
                        MQuaternion newRotation = MQuaternionExtensions.Slerp(currentRotation, desiredRotation, (float)weight);

                        //Set the local joint rotation of the intermediate skeleton
                        iS.SetLocalJointRotation(this.AvatarDescription.AvatarID, joint.JointType, newRotation);

                        //Goal criteria
                        if (angle > 0.1f)
                            positioned = false;
                    }
                }

                //Provide event if positioned successfully
                if (positioned && !hand.Positioned)
                {
                    hand.Positioned = true;
                    result.Events.Add(new MSimulationEvent()
                    {
                        Name = "MoveFingersMMU",
                        Type = "FingersPositioned",
                        Reference = hand.Instruction.ID
                    });
                }


                //Increment the time
                hand.Elapsed += (float)time;
            }
           
            //Recompute the posture given the performed changes
            result.Posture = iS.RecomputeCurrentPostureValues(this.AvatarDescription.AvatarID);

            //Return the simulation result for the given frame
            return result;
        }


        /// <summary>
        /// Sets up the respective hand using the given parameters
        /// </summary>
        /// <param name="jointType"></param>
        /// <param name="instruction"></param>
        /// <param name="duration"></param>
        /// <param name="angularVelocity"></param>
        /// <param name="durationSet"></param>
        /// <param name="release"></param>
        private void SetupHand(MJointType jointType, MInstruction instruction, float duration, float angularVelocity, bool durationSet, bool release)
        {

            //Create a new hand container
            HandContainer hand = new HandContainer()
            {
                Type = jointType,
                Instruction = instruction,
                Release = release,
                Duration = duration,
                AngularVelocity = angularVelocity,
                HasDuration = durationSet,
                Positioned = false
            };

            HandContainer old = this.ActiveHands.Find(s => s.Type == jointType);

            if (old != null)
                this.ActiveHands.Remove(old);

            //Handle the hand pose (if defined)
            if (instruction.Properties.ContainsKey("HandPose"))
            {
                //Get the constraint id
                string constraintID = instruction.Properties["HandPose"];

                //Get the corresponding hand constraint
                MConstraint handConstraint = instruction.Constraints.Find(s => s.ID == constraintID);

                //Check if hand constraint is defined and assign as final posture
                if (handConstraint != null && handConstraint.PostureConstraint != null)
                {
                    hand.FinalPosture = handConstraint.PostureConstraint;
                }
            }

            //Add as active hand
            this.ActiveHands.Add(hand);
        }


        /// <summary>
        /// Determines whether the given joint type is a finger joint
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsFingerJoint(MJointType type)
        { 
            return fingerJoints.Contains(type);
        }


        /// <summary>
        /// List of all finger joint types
        /// </summary>
        private readonly List<MJointType> fingerJoints = new List<MJointType>()
        {
             MJointType.LeftIndexDistal,
             MJointType.LeftIndexMeta,
             MJointType.LeftIndexProximal,
             MJointType.LeftIndexTip,

             MJointType.LeftLittleDistal,
             MJointType.LeftLittleMeta,
             MJointType.LeftLittleProximal,
             MJointType.LeftLittleTip,

             MJointType.LeftMiddleDistal,
             MJointType.LeftMiddleMeta,
             MJointType.LeftMiddleProximal,
             MJointType.LeftMiddleTip,

             MJointType.LeftRingDistal,
             MJointType.LeftRingMeta,
             MJointType.LeftRingProximal,
             MJointType.LeftRingTip,

             MJointType.LeftThumbCarpal,
             MJointType.LeftThumbMeta,
             MJointType.LeftThumbMid,
             MJointType.LeftThumbTip,

             MJointType.RightIndexDistal,
             MJointType.RightIndexMeta,
             MJointType.RightIndexProximal,
             MJointType.RightIndexTip,

             MJointType.RightLittleDistal,
             MJointType.RightLittleMeta,
             MJointType.RightLittleProximal,
             MJointType.RightLittleTip,

             MJointType.RightMiddleDistal,
             MJointType.RightMiddleMeta,
             MJointType.RightMiddleProximal,
             MJointType.RightMiddleTip,

             MJointType.RightRingDistal,
             MJointType.RightRingMeta,
             MJointType.RightRingProximal,
             MJointType.RightRingTip,

             MJointType.RightThumbCarpal,
             MJointType.RightThumbMeta,
             MJointType.RightThumbMid,
             MJointType.RightThumbTip,
        };
    }
}
