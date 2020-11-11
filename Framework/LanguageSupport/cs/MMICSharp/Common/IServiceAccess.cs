// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Clients;
using MMIStandard;
using System.Collections.Generic;

namespace MMICSharp.Common
{
    /// <summary>
    /// Interface provides the access to all available services in the MMIFramework
    /// </summary>
    public interface IServiceAccess
    {
        /// <summary>
        /// Access to the grasp service
        /// </summary>
        MGraspPoseService.Iface GraspService
        {
            get;         
        }

        /// <summary>
        /// Access to the grasp service
        /// </summary>
        GraspPoseServiceClient GraspServiceClient
        {
            get;
        }

        /// <summary>
        /// Access to the IK service
        /// </summary>
        MInverseKinematicsService.Iface IKService
        {
            get;
        }

        /// <summary>
        /// Access to the IK service client
        /// </summary>
        IKServiceClient IKServiceClient
        {
            get;
        }

        /// <summary>
        /// Access to the path planning service
        /// </summary>
        MPathPlanningService.Iface PathPlanningService
        {
            get;
        }

        /// <summary>
        /// Access to the path planning service
        /// </summary>
        PathPlanningServiceClient PathPlanningServiceClient
        {
            get;
        }

        /// <summary>
        /// Access to the retargeting service
        /// </summary>
        MRetargetingService.Iface RetargetingService
        {
            get;
        }

        /// <summary>
        /// Access to the retargeting service
        /// </summary>
        RetargetingServiceClient RetargetingServiceClient
        {
            get;
        }

        /// <summary>
        /// Access to the register service
        /// </summary>
        MMIRegisterService.Iface RegisterService
        {
            get;
        }


        /// <summary>
        /// Access to the register service
        /// </summary>
        MMIRegisterServiceClient RegisterServiceClient
        {
            get;
        }

        /// <summary>
        /// Access to the collision detection service
        /// </summary>
        MCollisionDetectionService.Iface CollisionDetectionService
        {
            get;
        }


        /// <summary>
        /// Access to the collision detection service
        /// </summary>
        CollisionDetectionServiceClient CollisionDetectionServiceClient
        {
            get;
        }

        /// <summary>
        /// Access to the grasp pose service
        /// </summary>
        MGraspPoseService.Iface GraspPoseService
        {
            get;
        }

        /// <summary>
        /// Access to the grasp pose service
        /// </summary>
        GraspPoseServiceClient GraspPoseServiceClient
        {
            get;
        }




        /// <summary>
        /// Access to the posture blending service
        /// </summary>
        MPostureBlendingService.Iface PostureBlendingService
        {
            get;
        }

        /// <summary>
        /// Access to the client of the posture blending service
        /// </summary>
        PostureBlendingServiceClient PostureBlendingServiceClient
        {
            get;
        }


        /// <summary>
        /// Access to the walk point estimation service
        /// </summary>
        MWalkPointEstimationService.Iface WalkPointEstimationService
        {
            get;
        }

        /// <summary>
        /// Access to the client of the walk point estimation service
        /// </summary>
        WalkPointEstimationServiceClient WalkPointEstimationServiceClient
        {
            get;
        }

        /// <summary>
        /// Basic method to access an arbitray service
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Dictionary<string, string> Consume(MIPAddress address, Dictionary<string, string> parameters);
    }
}
