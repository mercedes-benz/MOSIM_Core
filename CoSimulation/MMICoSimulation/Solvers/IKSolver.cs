// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMICoSimulation.Solvers
{
    /// <summary>
    /// Solver which can be used to apply ik related constraints
    /// </summary>
    public class IKSolver : ICoSimulationSolver
    {
        /// <summary>
        /// The position threshold for starting to solve the posture (in meter)
        /// </summary>
        public float PositionThreshold = 0.01f;

        /// <summary>
        /// The threshold for starting to solve the posture (in degree)
        /// </summary>
        public float RotationThreshold = 1f;

        /// <summary>
        /// How many times the ik is computed 
        /// </summary>
        public int Iterations = 1;

        /// <summary>
        /// Flag indicates whether the solver only computes new results if violated constraints are available
        /// </summary>

        public bool SolveViolatedConstraintsOnly = true;

        /// <summary>
        /// Reference to the service access
        /// </summary>
        private readonly IServiceAccess serviceAccess;

        /// <summary>
        /// Reference to the skeleton access
        /// </summary>
        private MSkeletonAccess.Iface skeletonAccess;


        /// <summary>
        /// Default constructor
        /// </summary>
        public IKSolver()
        {
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="serviceAccess"></param>
        /// <param name="skeletonAccess"></param>
        /// <param name="avatarID"></param>
        public IKSolver(IServiceAccess serviceAccess, MSkeletonAccess.Iface skeletonAccess, string avatarID)
        {
            this.serviceAccess = serviceAccess;
            this.skeletonAccess = skeletonAccess;
            this.serviceAccess.IKService.Setup(skeletonAccess.GetAvatarDescription(avatarID), skeletonAccess.GetAvatarDescription(avatarID).Properties);
        }

        /// <summary>
        /// Function to check if a solving is required
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool RequiresSolving(MSimulationResult result, float timespan)
        {
            //If at least one constraint is violated solving is required
            return this.GetViolatedIKConstraints(result.Constraints, result.Posture).Count > 0;
        }

        /// <summary>
        /// Performs the actual solving
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mmuResults"></param>
        /// <returns></returns>
        public virtual MSimulationResult Solve(MSimulationResult input, List<MSimulationResult> mmuResults, float timespan)
        {
            MSimulationResult result = input;

            //By default use all constraints
            List<MConstraint> constraints = new List<MConstraint>(input.Constraints);

            //Only solve the constraints which are not already fulfilled
            if (this.SolveViolatedConstraintsOnly)
                constraints = this.GetViolatedIKConstraints(input.Constraints, result.Posture);

            //Compute the ik if at least one constraint is not fulfilled
            if (constraints.Count > 0)
            {
                MAvatarPostureValues currentPosture = input.Posture;

                //Solve n times
                for (int i = 0; i < this.Iterations; i++)
                {
                    MIKServiceResult ikResult = this.serviceAccess.IKService.CalculateIKPosture(currentPosture, constraints, new Dictionary<string, string>());
                    currentPosture = ikResult.Posture;
                }

                result.Posture = currentPosture;

            }


            return result;
        }


        /// <summary>
        /// Returns all ik constraints which are violated (avove specified threshold)
        /// </summary>
        /// <param name="constraints"></param>
        /// <param name="currentPosture"></param>
        /// <returns></returns>
        private List<MConstraint> GetViolatedIKConstraints(List<MConstraint> constraints, MAvatarPostureValues currentPosture)
        {
            List<MConstraint> violated = new List<MConstraint>();

            if (constraints != null)
            {
                // Apply result posture values to the skeleton
                skeletonAccess.SetChannelData(currentPosture);

                string avatarID = currentPosture.AvatarID;

                //Check each joint constraint
                foreach (MConstraint mconstraint in constraints)
                {
                    if (mconstraint.JointConstraint != null)
                    {
                        MJointConstraint endeffectorConstraint = mconstraint.JointConstraint;

                        //Skip if no gemometry constraint is defined
                        if (endeffectorConstraint.GeometryConstraint == null)
                            continue;


                        double distance = 0f;
                        double angularDistance = 0f;


                        //Default (parent to constraint is set)
                        if (endeffectorConstraint.GeometryConstraint.ParentToConstraint != null)
                        {
                            MVector3 position = endeffectorConstraint.GeometryConstraint.ParentToConstraint.Position;
                            MQuaternion rotation = endeffectorConstraint.GeometryConstraint.ParentToConstraint.Rotation;

                            switch (endeffectorConstraint.JointType)
                            {
                                case MJointType.LeftWrist:
                                    distance = MVector3Extensions.Distance(position, this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.LeftWrist));
                                    angularDistance = MQuaternionExtensions.Angle(rotation, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.LeftWrist));

                                    break;

                                case MJointType.RightWrist:
                                    distance = MVector3Extensions.Distance(position, this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.RightWrist));
                                    angularDistance = MQuaternionExtensions.Angle(rotation, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.RightWrist));

                                    break;

                                case MJointType.LeftBall:
                                    distance = MVector3Extensions.Distance(position, this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.LeftAnkle));
                                    angularDistance = MQuaternionExtensions.Angle(rotation, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.LeftAnkle));

                                    break;

                                case MJointType.RightBall:
                                    distance = MVector3Extensions.Distance(position, this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.RightAnkle));
                                    angularDistance = MQuaternionExtensions.Angle(rotation, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.RightAnkle));

                                    break;

                                case MJointType.PelvisCentre:
                                    distance = MVector3Extensions.Distance(position, this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.PelvisCentre));
                                    angularDistance = MQuaternionExtensions.Angle(rotation, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.PelvisCentre));

                                    break;
                            }
                        }

                        //Legacy fallback mechanism -> Remove in future
                        else
                        {
                            MTranslationConstraint positionConstraint = endeffectorConstraint.GeometryConstraint.TranslationConstraint;


                            if (endeffectorConstraint.GeometryConstraint.TranslationConstraint != null)
                            {
                                switch (endeffectorConstraint.JointType)
                                {
                                    case MJointType.LeftWrist:
                                        distance = MVector3Extensions.Distance(new MVector3(positionConstraint.X(), positionConstraint.Y(), positionConstraint.Z()), this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.LeftWrist));
                                        break;

                                    case MJointType.RightWrist:
                                        distance = MVector3Extensions.Distance(new MVector3(positionConstraint.X(), positionConstraint.Y(), positionConstraint.Z()), this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.RightWrist));
                                        break;

                                    case MJointType.LeftBall:
                                        distance = MVector3Extensions.Distance(new MVector3(positionConstraint.X(), positionConstraint.Y(), positionConstraint.Z()), this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.LeftAnkle));
                                        break;

                                    case MJointType.RightBall:
                                        distance = MVector3Extensions.Distance(new MVector3(positionConstraint.X(), positionConstraint.Y(), positionConstraint.Z()), this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.RightAnkle));
                                        break;

                                    case MJointType.PelvisCentre:
                                        distance = MVector3Extensions.Distance(new MVector3(positionConstraint.X(), positionConstraint.Y(), positionConstraint.Z()), this.skeletonAccess.GetGlobalJointPosition(avatarID, MJointType.PelvisCentre));
                                        break;
                                }
                            }

                            //Handle the rotation constraint
                            if (endeffectorConstraint.GeometryConstraint.RotationConstraint != null)
                            {
                                MRotationConstraint rotationConstraint = endeffectorConstraint.GeometryConstraint.RotationConstraint;

                                //Compute a quaternion based on the euler angles
                                MQuaternion quaternion = MQuaternionExtensions.FromEuler(new MVector3(rotationConstraint.X(), rotationConstraint.Y(), rotationConstraint.Z()));

                                if (endeffectorConstraint.GeometryConstraint.ParentObjectID == null || endeffectorConstraint.GeometryConstraint.ParentObjectID == "")
                                {
                                    switch (endeffectorConstraint.JointType)
                                    {
                                        case MJointType.LeftWrist:
                                            angularDistance = MQuaternionExtensions.Angle(quaternion, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.LeftWrist));
                                            break;

                                        case MJointType.RightWrist:
                                            angularDistance = MQuaternionExtensions.Angle(quaternion, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.RightWrist));
                                            break;

                                        case MJointType.LeftBall:
                                            angularDistance = MQuaternionExtensions.Angle(quaternion, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.LeftAnkle));
                                            break;

                                        case MJointType.RightBall:
                                            angularDistance = MQuaternionExtensions.Angle(quaternion, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.RightAnkle));
                                            break;

                                        case MJointType.PelvisCentre:
                                            angularDistance = MQuaternionExtensions.Angle(quaternion, this.skeletonAccess.GetGlobalJointRotation(avatarID, MJointType.PelvisCentre));
                                            break;
                                    }
                                }

                            }
                        }

                        //Check if solving is required
                        if (distance > this.PositionThreshold || angularDistance > this.RotationThreshold)
                        {
                            violated.Add(mconstraint);
                        }

                    }
                }
            }

            return violated;
        }



        /// <summary>
        /// Determines whether the given translation constraint is fulfilled
        /// </summary>
        /// <param name="desiredPosition"></param>
        /// <param name="currentPosition"></param>
        /// <param name="translationConstraint"></param>
        /// <returns></returns>
        private bool IsFulfilled(MVector3 desiredPosition, MVector3 currentPosition, MTranslationConstraint translationConstraint)
        {
            //By default return true -> It is assumed that interval is [-inv,+inv]
            if (translationConstraint == null)
                return true;

            switch (translationConstraint.Type)
            {
                case MTranslationConstraintType.BOX:
                    //Determine the global min and max coordinate
                    MVector3 min = new MVector3(translationConstraint.Limits.X.Min, translationConstraint.Limits.Y.Min, translationConstraint.Limits.Z.Min).Add(currentPosition);
                    MVector3 max = new MVector3(translationConstraint.Limits.X.Max, translationConstraint.Limits.Y.Max, translationConstraint.Limits.Z.Max).Add(currentPosition);

                    //Check if within bounding box
                    return (currentPosition.X >= min.X && currentPosition.Y >= min.Y && currentPosition.Z >= min.Z && currentPosition.X <= max.X && currentPosition.Y <= max.Y && currentPosition.Z <= max.Z);

                case MTranslationConstraintType.ELLIPSOID:

                    double xDist = Math.Abs(currentPosition.X - desiredPosition.X);
                    double yDist = Math.Abs(currentPosition.Y - desiredPosition.Y);
                    double zDist = Math.Abs(currentPosition.Z - desiredPosition.Z);

                    return xDist <= translationConstraint.Limits.X() && yDist <= translationConstraint.Limits.Y() && zDist <= translationConstraint.Limits.Z();
            }

            //By default return true -> It is assumed that interval is [-inv,+inv]
            return true;
        }


        /// <summary>
        /// Determines whether the constraint is fulflled
        /// </summary>
        /// <param name="desiredRotation"></param>
        /// <param name="currentRotation"></param>
        /// <param name="rotationConstraint"></param>
        /// <returns></returns>
        private bool IsFulfilled(MQuaternion desiredRotation, MQuaternion currentRotation, MRotationConstraint rotationConstraint)
        {
            //By default return true -> It is assumed that interval is [-inv,+inv]
            if (rotationConstraint == null)
                return true;
            
            MVector3 currentRotationEuler = MQuaternionExtensions.ToEuler(currentRotation);

            //Determine the global min and max coordinate
            MVector3 min = new MVector3(rotationConstraint.Limits.X.Min, rotationConstraint.Limits.Y.Min, rotationConstraint.Limits.Z.Min).Add(currentRotationEuler);
            MVector3 max = new MVector3(rotationConstraint.Limits.X.Max, rotationConstraint.Limits.Y.Max, rotationConstraint.Limits.Z.Max).Add(currentRotationEuler);


            return (currentRotationEuler.X >= min.X && currentRotationEuler.Y >= min.Y && currentRotationEuler.Z >= min.Z && currentRotationEuler.X <= max.X && currentRotationEuler.Y <= max.Y && currentRotationEuler.Z <= max.Z);
            


        }


    }
}
