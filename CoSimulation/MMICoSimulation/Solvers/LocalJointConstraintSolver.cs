// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;
using MMIStandard;

namespace MMICoSimulation.Solvers
{
    /// <summary>
    /// Directly applies the constraints
    /// </summary>
    public class LocalJointConstraintSolver : ICoSimulationSolver
    {

        /// <summary>
        /// The assigned skeleton access
        /// </summary>
        private readonly MSkeletonAccess.Iface skeletonAccess;


        /// <summary>
        /// The joint types that are explictely considerd for applying the constraints
        /// </summary>
        public List<MJointType> ConsideredTypes = new List<MJointType>()
        {
             MJointType.LeftWrist,
             MJointType.RightWrist
        };


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="skeletonAccess"></param>
        public LocalJointConstraintSolver(MSkeletonAccess.Iface skeletonAccess)
        {
            this.skeletonAccess = skeletonAccess;
        }

        /// <summary>
        /// Checks whether solving is required
        /// </summary>
        /// <param name="result"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public bool RequiresSolving(MSimulationResult result, float timeSpan)
        {
            List<MJointConstraint> jointConstraints = new List<MJointConstraint>();

            foreach (MConstraint constraint in result.Constraints)
            {
                if (constraint.JointConstraint != null)
                    jointConstraints.Add(constraint.JointConstraint);

                if (constraint.PostureConstraint != null && constraint.PostureConstraint.JointConstraints != null)
                    jointConstraints.AddRange(constraint.PostureConstraint.JointConstraints);
            }


            foreach (MJointConstraint constraint in jointConstraints)
            {
                //Skip if joint is not considered
                if (!this.ConsideredTypes.Contains(constraint.JointType))
                    continue;

                //To do integrate further checks

            }

            return true;
        }

        /// <summary>
        /// Performs the actual solving
        /// </summary>
        /// <param name="currentResult"></param>
        /// <param name="mmuResults"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public MSimulationResult Solve(MSimulationResult currentResult, List<MSimulationResult> mmuResults, float timeSpan)
        {
            List<MJointConstraint> jointConstraints = new List<MJointConstraint>();

            foreach (MConstraint constraint in currentResult.Constraints)
            {
                if (constraint.JointConstraint != null)
                    jointConstraints.Add(constraint.JointConstraint);

                if (constraint.PostureConstraint != null && constraint.PostureConstraint.JointConstraints != null)
                    jointConstraints.AddRange(constraint.PostureConstraint.JointConstraints);
            }

            //Set the channel data
            this.skeletonAccess.SetChannelData(currentResult.Posture);

            foreach(MJointConstraint constraint in jointConstraints)
            {
                //Skip if joint is not considered
                if (!this.ConsideredTypes.Contains(constraint.JointType))
                    continue;



                MGeometryConstraint geometryConstraint = constraint.GeometryConstraint;


                if (geometryConstraint.ParentObjectID == "")
                {
                    //No parent -> Global joint constraints cannot be solved here
                }

                else
                {

                    //To do -> Further check the parent
                    if (geometryConstraint.ParentToConstraint != null)
                    {
                        this.skeletonAccess.SetLocalJointPosition(currentResult.Posture.AvatarID, constraint.JointType, geometryConstraint.ParentToConstraint.Position);
                        this.skeletonAccess.SetLocalJointRotation(currentResult.Posture.AvatarID, constraint.JointType, geometryConstraint.ParentToConstraint.Rotation);
                    }
                }
            }

            currentResult.Posture = this.skeletonAccess.RecomputeCurrentPostureValues(currentResult.Posture.AvatarID);

            return currentResult;

        }
    }
}
