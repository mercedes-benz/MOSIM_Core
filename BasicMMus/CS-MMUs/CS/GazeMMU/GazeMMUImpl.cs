// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;


namespace GazeMMU
{
    [MMUDescriptionAttribute("Felix Gaisbauer", "1.0", "GazeMMU", "Pose/Gaze", "", "A gaze MMU directly manipulating the head joint.", "A gaze MMU realized using joint angle manipulations.")]
    public class GazeMMUImpl : MMUBase
    {
        /// <summary>
        /// The transform of the gaze target (is dynamically updated)
        /// </summary>
        private MTransform gazeTarget;

        //Initial rotations
        private MQuaternion initialHeadRotation = null;
        private MQuaternion initialNeckRotation = null;


        //Limits [-30,20]
        private float lowerLimit = -20;
        private float upperLimit = 20;

        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            base.Initialize(avatarDescription, properties);

            //Setuo the skeleton access
            this.SkeletonAccess = new IntermediateSkeleton();
            this.SkeletonAccess.InitializeAnthropometry(avatarDescription);


            //Initial rotations
            this.initialHeadRotation = SkeletonAccess.GetLocalJointRotation(AvatarDescription.AvatarID, MJointType.HeadJoint);
            this.initialNeckRotation = SkeletonAccess.GetLocalJointRotation(AvatarDescription.AvatarID, MJointType.C4C5Joint);

            return new MBoolResponse(true);
        }

        /// <summary>
        /// Method to assign the actual instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        [MParameterAttribute("TargetID", "ID", "The id of the gaze target", true)]
        [MParameterAttribute("LowerLimit", "float", "The lower angular limit of bending the neck (degree)", false)]
        [MParameterAttribute("UpperLimit", "float", "The upper angular limit of bending the neck (degree)", false)]

        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            base.AssignInstruction(instruction, simulationState);

            //Get the gaze target
            if (instruction.Properties != null && instruction.Properties.ContainsKey("TargetID"))
            {
                this.gazeTarget = this.SceneAccess.GetTransformByID(instruction.Properties["TargetID"]);
            }

            else
            {
                //Return false if no gaze target is defined
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Error, no gaze target defined" }
                };
            }

            if (instruction.Properties.ContainsKey("LowerLimit"))
                this.lowerLimit = float.Parse(instruction.Properties["LowerLimit"], System.Globalization.CultureInfo.InstalledUICulture);

            if (instruction.Properties.ContainsKey("UpperLimit"))
                this.upperLimit = float.Parse(instruction.Properties["UpperLimit"], System.Globalization.CultureInfo.InstalledUICulture);

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Default do step routine
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Create a new simulation result
            MSimulationResult result = new MSimulationResult()
            {
                Events = simulationState.Events ?? new List<MSimulationEvent>(),
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),
                SceneManipulations = simulationState.SceneManipulations?? new List<MSceneManipulation>(),
                Posture = simulationState.Current
            };

            //Set the channel data to reflect to current posture
            SkeletonAccess.SetChannelData(simulationState.Current);


            //Set the default rotation of the head and neck joints
            SkeletonAccess.SetLocalJointRotation(AvatarDescription.AvatarID, MJointType.HeadJoint, initialHeadRotation);
            SkeletonAccess.SetLocalJointRotation(AvatarDescription.AvatarID, MJointType.C4C5Joint, initialNeckRotation);


            //Create a transform representing the current head location
            MTransform currentTransform = new MTransform()
            {
                ID = "",
                Position = SkeletonAccess.GetGlobalJointPosition(AvatarDescription.AvatarID, MJointType.HeadJoint),
                Rotation = SkeletonAccess.GetGlobalJointRotation(AvatarDescription.AvatarID, MJointType.HeadJoint)
            };

            //Create a transform representing the parent location (neck)
            MTransform parentTransform = new MTransform()
            {
                ID = "",
                Position = SkeletonAccess.GetGlobalJointPosition(AvatarDescription.AvatarID, MJointType.C4C5Joint),
                Rotation = SkeletonAccess.GetGlobalJointRotation(AvatarDescription.AvatarID, MJointType.C4C5Joint)
            };

            //The current head forward vector
            MVector3 currentHeadForward = new MVector3(-1, 0, 0);



            //Compute the local position of the desired object (relative to the neck)
            MVector3 localPosition = parentTransform.InverseTransformPoint(this.gazeTarget.Position);

            //Get the xz distance in local space
            float distance = new MVector3(localPosition.X, 0, localPosition.Z).Magnitude();
            float height = (float)localPosition.Y;

            //Compute the current angle
            float currentAngle =(float)(Math.Atan(height / distance) * 180 / Math.PI);

            //Limit if below lower limit
            if(currentAngle< lowerLimit)
                localPosition.Y = Math.Tan(lowerLimit * Math.PI/180)  * distance;

            //Limit if above upper angle limit
            if(currentAngle > upperLimit)
                localPosition.Y = Math.Tan(upperLimit * Math.PI / 180) * distance;


            float maxYAngle = 80f;

            //Limit xz position
            float yAngle = (float)MVector3Extensions.Angle(currentHeadForward, new MVector3(localPosition.X, 0, localPosition.Z));

            if(yAngle > maxYAngle)
            {
                //The interpolated direction
                MVector3 interpolatedDirection = MVector3Extensions.Lerp(currentHeadForward, new MVector3(localPosition.X, 0, localPosition.Z).Normalize(), (maxYAngle / yAngle)).Normalize();

                //Perform correction
                MVector3 newLocalPositionsXZ = interpolatedDirection.Multiply(distance);

                localPosition.X = newLocalPositionsXZ.X;
                localPosition.Z = newLocalPositionsXZ.Z;
            }
            

            //Compute the desired and current facing direction
            MVector3 desiredHeadForward = localPosition.Normalize();


            //Estimate the rotation that is required to rotate from the current head direction towards the desired one
            MQuaternion deltaRotation = FromToRotation(currentHeadForward, new MVector3(desiredHeadForward.X, -desiredHeadForward.Y, desiredHeadForward.Z));

            //Gather the current location rotation
            MQuaternion currentLocalRotation = SkeletonAccess.GetLocalJointRotation(AvatarDescription.AvatarID, MJointType.HeadJoint);

            //Update the local joint rotation to adjust the facing direction to the desired values

            SkeletonAccess.SetLocalJointRotation(AvatarDescription.AvatarID, MJointType.HeadJoint, currentLocalRotation.Multiply(deltaRotation));

            //Set the updated postures
            result.Posture = SkeletonAccess.RecomputeCurrentPostureValues(AvatarDescription.AvatarID);

            //Return the simulation results
            return result;

        }


        /// <summary>
        /// Computes the rotation to rotate from one vector to the other
        /// </summary>
        /// <param name="from">The start direction</param>
        /// <param name="to">The desired direction</param>
        /// <returns></returns>
        private static MQuaternion FromToRotation(MVector3 from, MVector3 to)
        {
            //Normalize both vectors
            from = from.Normalize();
            to = to.Normalize();

            //Estimate the rotation axis
            MVector3 axis = MVector3Extensions.Cross(from, to).Normalize();

            //Compute the phi angle
            double phi = Math.Acos(MVector3Extensions.Dot(from, to)) / (from.Magnitude() * to.Magnitude());

            //Create a new quaternion representing the rotation
            MQuaternion result =  new MQuaternion()
            {
                X = Math.Sin(phi / 2) * axis.X,
                Y = Math.Sin(phi / 2) * axis.Y,
                Z = Math.Sin(phi / 2) * axis.Z,
                W = Math.Cos(phi / 2)
            };

            //Perform is nan check and return identity quaternion
            if (double.IsNaN(result.W) || double.IsNaN(result.X) || double.IsNaN(result.Y) || double.IsNaN(result.Z))
                result = new MQuaternion(0, 0, 0, 1);

            //Return the estimated rotation
            return result;
        }
    }

}
