// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraspMMUSimple
{
    /// <summary>
    /// Class used for debugging the MMU
    /// </summary>
    class Debug
    {
        static void Main(string[] args)
        {
            using (var debugAdapter = new DebugAdapter.DebugAdapter(typeof(GraspMMUSimpleImpl)))
            {
                Console.ReadLine();
            }
        }
    }


    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "GraspMMUSimple", "Pose/Grasp", "", "MMU allows to manipulate the finger joints by means of motion blending.", "MMU allows to manipulate the finger joints by means of motion blending.")]
    public class GraspMMUSimpleImpl : MMUBase
    {
        private readonly List<HandContainer> ActiveHands = new List<HandContainer>();
        private MHandPose closedHandLeft;
        private MHandPose closedHandRight;




        public GraspMMUSimpleImpl()
        {
        }

        /// <summary>
        /// Basic initialize method
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {

            //Create a new instance of the skeleton access /intermedaite skeleton
            this.SkeletonAccess = new IntermediateSkeleton();

            //Setup the anthropometry
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);



            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            MMICSharp.Adapter.Logger.Log( MMICSharp.Adapter.Log_level.L_INFO, path);
            //Load the closed hand posture for left and right hand
            if (!System.IO.File.Exists(path +"/leftHandClosed.json"))
            {
                MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Left hand cannot be loaded. Please ensure that leftHandClosed.json is in the same folder as the MMU");
                return new MBoolResponse(false);
            }

            //Load the closed hand posture for left and right hand
            if (!System.IO.File.Exists(path + "/rightHandClosed.json"))
            {
                MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Right hand cannot be loaded. Please ensure that righHandClosed.json is in the same folder as the MMU");
                return new MBoolResponse(false);
            }

            //Load the closed hand posture from disk
            this.closedHandLeft = MMICSharp.Common.Communication.Serialization.FromJsonString<MHandPose>(System.IO.File.ReadAllText(path + "/leftHandClosed.json"));
            this.closedHandRight = MMICSharp.Common.Communication.Serialization.FromJsonString<MHandPose>(System.IO.File.ReadAllText(path + "/rightHandClosed.json"));


            return base.Initialize(avatarDescription, properties);
        }

        [MParameterAttribute("Hand", "Left/Right", "The hand type.", true)]
        [MParameterAttribute("HandPose", "PostureConstraint", "The desired hand pose, joint constraints of the finger tips.", true)]
        [MParameterAttribute("UseGlobalCoordinates", "bool", "Specified whether the global coordinates of the fingers are used for establishing the hand pose (by default true).", false)]

        [MParameterAttribute("Duration", "float", "The desired duration until the pose is established.", false)]
        [MParameterAttribute("KeepHandPose", "bool", "Specifies whether the MMU finishes once the hand pose is establish, or if the MMU continues actively holding the posture", false)]
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            float duration = 1f;
            //Parse optional duration parameter
            if (instruction.Properties.ContainsKey("Duration"))
                duration = float.Parse(instruction.Properties["Duration"], System.Globalization.CultureInfo.InvariantCulture);


            //Check if parameter hand pose is defined
            if (!instruction.Properties.ContainsKey("HandPose"))
            {
                //Return false if the hand parameter is not set
                MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Mandatory parameter HandPose is not defined (GraspMMUSimple)");

                return new MBoolResponse(false);
            }

            //Check if mandatory parameter hand is defined
            if (!instruction.Properties.ContainsKey("Hand"))
            {
                //Return false if the hand parameter is not set
                MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Mandatory parameter Hand is not defined (GraspMMUSimple)");

                return new MBoolResponse(false);
            }

            try
            {
                //Setup the respective hand
                switch (instruction.Properties["Hand"])
                {
                    case "Left":
                        this.SetupHand(MJointType.LeftWrist, instruction, duration, simulationState);
                        break;

                    case "Right":
                        this.SetupHand(MJointType.RightWrist, instruction, duration, simulationState);
                        break;

                    default:
                        //Return false if the hand parameter is not set
                        MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Unknown Hand defined (either Left or Right) (GraspMMUSimple)");
                        return new MBoolResponse(false);
                }
            }
            catch (Exception e)
            {
                MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Exception occured, aborting MMU (GraspMMUSimple). " + e.Message + " " + e.StackTrace);
                return new MBoolResponse(false);
            }


            //Call the base class method
            return base.AssignInstruction(instruction, simulationState);
        }


        [MSimulationEvent("PositioningFinished", "PositioningFinished")]
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
            this.SkeletonAccess.SetChannelData(simulationState.Current);


            //Handle each active hand
            for (int i = this.ActiveHands.Count - 1; i >= 0; i--)
            {
                //Get the current hand
                HandContainer hand = this.ActiveHands[i];


                foreach(Finger finger in hand.Fingers)
                {
                    foreach(MJointType jt in finger.JointTypes)
                    {
                        //To check whether initial rotation should be used
                        //MQuaternion current = this.SkeletonAccess.GetLocalJointRotation(this.AvatarDescription.AvatarID, jt);
                        MQuaternion current = hand.InitialFingerRotations[jt];

                        MQuaternion desired = finger.Best[jt];

                        float blendWeight = Math.Min(1f,hand.Elapsed / hand.Duration);

                        MQuaternion newRotation = MQuaternionExtensions.Slerp(current, desired, blendWeight);

                        this.SkeletonAccess.SetLocalJointRotation(this.AvatarDescription.AvatarID, jt, newRotation);
                    }
                }

                hand.Elapsed += (float)time;

                if(!hand.Positioned && hand.Elapsed >= hand.Duration)
                {
                    hand.Positioned = true;
                    result.Events.Add(new MSimulationEvent("Grasp motion finished", "PositioningFinished", hand.Instruction.ID));
                }

                //Only terminate MMU if hand pose should not be kept
                if (!hand.KeepHandPose && hand.Elapsed >= hand.Duration)
                {
                    result.Events.Add(new MSimulationEvent("Grasp motion finished", mmiConstants.MSimulationEvent_End, hand.Instruction.ID));
                }
            }


            result.Posture = this.SkeletonAccess.RecomputeCurrentPostureValues(this.AvatarDescription.AvatarID);

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
        private bool SetupHand(MJointType jointType, MInstruction instruction, float duration, MSimulationState simulationState)
        {
            //Create a new hand container
            HandContainer hand = new HandContainer()
            {
                Type = jointType,
                Instruction = instruction,
                Duration = duration,
                Positioned = false
            };

            //Find an old container adressing the same hand (if available)
            HandContainer old = this.ActiveHands.Find(s => s.Type == jointType);

            //Remove the old if available
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

                else
                {
                    MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_ERROR, "Problem getting HandPose, please ensure that a PostureConstraint is set for the HandPose with the defined constraint ID " + constraintID);
                    return false;
                }
            }

            if (instruction.Properties.ContainsKey("KeepHandPose"))
            {
                bool.TryParse(instruction.Properties["KeepHandPose"], out hand.KeepHandPose);
            }

            if (instruction.Properties.ContainsKey("UseGlobalCoordinates"))
            {
                bool.TryParse(instruction.Properties["UseGlobalCoordinates"], out hand.UseGlobalCoordinates);
            }
            


            //Setup the fingers
            switch (jointType)
            {
                //Setup left fingers
                case MJointType.LeftWrist:
                    hand.Fingers.Add(new Finger() { JointTypes = leftIndex, FingerTip = MJointType.LeftIndexTip });
                    hand.Fingers.Add(new Finger() { JointTypes = leftLittle, FingerTip = MJointType.LeftLittleTip });
                    hand.Fingers.Add(new Finger() { JointTypes = leftMiddle, FingerTip = MJointType.LeftMiddleTip });
                    hand.Fingers.Add(new Finger() { JointTypes = leftRing, FingerTip = MJointType.LeftRingTip });
                    hand.Fingers.Add(new Finger() { JointTypes = leftThumb, FingerTip = MJointType.LeftThumbTip });

                    hand.ClosedHand = this.closedHandLeft;

                    break;

                //Setup right fingers
                case MJointType.RightWrist:
                    hand.Fingers.Add(new Finger() { JointTypes = rightIndex, FingerTip = MJointType.RightIndexTip });
                    hand.Fingers.Add(new Finger() { JointTypes = rightLittle, FingerTip = MJointType.RightLittleTip });
                    hand.Fingers.Add(new Finger() { JointTypes = rightMiddle,  FingerTip = MJointType.RightMiddleTip });
                    hand.Fingers.Add(new Finger() { JointTypes = rightRing, FingerTip = MJointType.RightRingTip });
                    hand.Fingers.Add(new Finger() { JointTypes = rightThumb, FingerTip = MJointType.RightThumbTip });

                    //Load the closed hand
                    hand.ClosedHand = this.closedHandRight;

                    break;
            }


            //Get the current hand posture
            this.SkeletonAccess.SetChannelData(simulationState.Initial);


            //First get the initial rotation of each joint
            foreach (Finger finger in hand.Fingers)
            {
                foreach (MJointType jt in finger.JointTypes)
                {
                    MQuaternion currentRot = this.SkeletonAccess.GetLocalJointRotation(this.AvatarDescription.AvatarID, jt);

                    hand.InitialFingerRotations.Add(jt, currentRot);
                }
            }

            int iterations = 10; 

            //Perform blending with different weights
            for (float w = 0; w < 1; w += 1f/(float)iterations)
            {

                //Blend to the target hand posture
                //Close each finger and determine the minimu
                foreach (Finger finger in hand.Fingers)
                {
                    foreach (MJointType jt in finger.JointTypes)
                    {
                        //The current rotation
                        MQuaternion currentRot = hand.InitialFingerRotations[jt];

                        //The desired rotation of the closed hand
                        MQuaternion targetRotation = hand.ClosedHand.Joints.Find(s => s.Type == jt).Rotation;

                        //Blend
                        MQuaternion newRotation =  MQuaternionExtensions.Slerp(currentRot, targetRotation, w);

                        ////Hack
                        //newRotation = targetRotation;

                        //Set the new local rotation
                        this.SkeletonAccess.SetLocalJointRotation(this.AvatarDescription.AvatarID, jt, newRotation);

                        if (!finger.Current.ContainsKey(jt))
                            finger.Current.Add(jt, newRotation);

                        //Update the current rotation
                        finger.Current[jt] = newRotation;
                    }


                    MAvatarPostureValues values = this.SkeletonAccess.RecomputeCurrentPostureValues(this.AvatarDescription.AvatarID);
                    this.SkeletonAccess.SetChannelData(values);
                    //Evaluate the distance


                    //Get the desired global position
                    MVector3 globalPos = hand.FinalPosture.JointConstraints.Find(s => s.JointType == finger.FingerTip).GeometryConstraint.GetGlobalPosition(this.SceneAccess);


                    if(!hand.UseGlobalCoordinates && hand.FinalPosture.JointConstraints.Exists(s=>s.JointType == MJointType.LeftWrist))
                    {
                        //Get the geometry constraint
                        MGeometryConstraint geomConstraint = hand.FinalPosture.JointConstraints.Find(s => s.JointType == MJointType.LeftWrist).GeometryConstraint;

                        //Get the transform of the wrist as defined in the joint constraint (recompute the global position (if locally defined))
                        MTransform leftWristTransform = new MTransform("", geomConstraint.GetGlobalPosition(this.SceneAccess), geomConstraint.GetGlobalRotation(this.SceneAccess));

                        //Compute the offset of the particular finger
                        MVector3 offset = MTransformExtensions.InverseTransformPoint(leftWristTransform, globalPos);
                            
                        //Create a transform representing the current wrist location
                        MTransform currentGlobalWrist = new MTransform("", this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.LeftWrist), this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, MJointType.LeftWrist));

                        //Recompute the global position of the finger based on the current wrist transform and the offset
                        globalPos = MTransformExtensions.TransformPoint(currentGlobalWrist, offset);
                    }



                    if (!hand.UseGlobalCoordinates && hand.FinalPosture.JointConstraints.Exists(s => s.JointType == MJointType.RightWrist))
                    {
                        //Get the geometry constraint
                        MGeometryConstraint geomConstraint = hand.FinalPosture.JointConstraints.Find(s => s.JointType == MJointType.RightWrist).GeometryConstraint;

                        //Get the transform of the wrist as defined in the joint constraint (recompute the global position (if locally defined))
                        MTransform rightWristTransform = new MTransform("", geomConstraint.GetGlobalPosition(this.SceneAccess), geomConstraint.GetGlobalRotation(this.SceneAccess));

                        //Compute the offset of the particular finger
                        MVector3 offset = MTransformExtensions.InverseTransformPoint(rightWristTransform, globalPos);

                        //Create a transform representing the current wrist location
                        MTransform currentGlobalWrist = new MTransform("", this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, MJointType.RightWrist), this.SkeletonAccess.GetGlobalJointRotation(this.AvatarDescription.AvatarID, MJointType.RightWrist));

                        //Recompute the global position of the finger based on the current wrist transform and the offset
                        globalPos = MTransformExtensions.TransformPoint(currentGlobalWrist, offset);
                    }


                    //Get the current global
                    MVector3 currentGlobal = this.SkeletonAccess.GetGlobalJointPosition(this.AvatarDescription.AvatarID, finger.FingerTip);

                    //The distance between the current finger tip position and the desired one 
                    float distance = (globalPos.Subtract(currentGlobal)).Magnitude();

                    //MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_DEBUG, finger.JointTypes[0] + " " + distance);

                    if (distance < finger.MinDistance)
                    {
                        MMICSharp.Adapter.Logger.Log(MMICSharp.Adapter.Log_level.L_INFO, finger.JointTypes[0] + " " + distance);

                        finger.MinDistance = distance;
                        //Assign the current to the best (and clone)
                        finger.Best = finger.Current.ToDictionary(entry => entry.Key,entry => entry.Value);
                    }
                }
            } 

            //Add as active hand
            this.ActiveHands.Add(hand);

            return true;
        }

        #region finger joints

        private readonly List<MJointType> leftIndex = new List<MJointType>()
        {
             MJointType.LeftIndexDistal,
             MJointType.LeftIndexMeta,
             MJointType.LeftIndexProximal,
             MJointType.LeftIndexTip,
        };

        private readonly List<MJointType> leftLittle = new List<MJointType>()
        {
             MJointType.LeftLittleDistal,
             MJointType.LeftLittleMeta,
             MJointType.LeftLittleProximal,
             MJointType.LeftLittleTip,
        };

        private readonly List<MJointType> leftMiddle = new List<MJointType>()
        {
             MJointType.LeftMiddleDistal,
             MJointType.LeftMiddleMeta,
             MJointType.LeftMiddleProximal,
             MJointType.LeftMiddleTip,
        };

        private readonly List<MJointType> leftRing = new List<MJointType>()
        {
             MJointType.LeftRingDistal,
             MJointType.LeftRingMeta,
             MJointType.LeftRingProximal,
             MJointType.LeftRingTip,
        };


        private readonly List<MJointType> leftThumb = new List<MJointType>()
        {
            MJointType.LeftThumbCarpal,
             MJointType.LeftThumbMeta,
             MJointType.LeftThumbMid,
             MJointType.LeftThumbTip,
        };

        private readonly List<MJointType> rightIndex = new List<MJointType>()
        {
             MJointType.RightIndexDistal,
             MJointType.RightIndexMeta,
             MJointType.RightIndexProximal,
             MJointType.RightIndexTip,
        };

        private readonly List<MJointType> rightLittle = new List<MJointType>()
        {
             MJointType.RightLittleDistal,
             MJointType.RightLittleMeta,
             MJointType.RightLittleProximal,
             MJointType.RightLittleTip,
        };

        private readonly List<MJointType> rightMiddle = new List<MJointType>()
        {
             MJointType.RightMiddleDistal,
             MJointType.RightMiddleMeta,
             MJointType.RightMiddleProximal,
             MJointType.RightMiddleTip,
        };

        private readonly List<MJointType> rightRing = new List<MJointType>()
        {
             MJointType.RightRingDistal,
             MJointType.RightRingMeta,
             MJointType.RightRingProximal,
             MJointType.RightRingTip,
        };


        private readonly List<MJointType> rightThumb = new List<MJointType>()
        {
            MJointType.RightThumbCarpal,
             MJointType.RightThumbMeta,
             MJointType.RightThumbMid,
             MJointType.RightThumbTip,
        };
        #endregion

    }
}
