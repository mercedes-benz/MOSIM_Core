// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser

package Access.Abstraction;

import ThriftClients.*;
import de.mosim.mmi.services.*;

public interface IServiceAccess {
    /*
		Interface provides the access to all available services in the MMIFramework
	*/
    //	not implemented yet


    IKServiceClient getIkThriftClient();

    PathPlanningServiceClient getPathPlanningThriftClient();

    RetargetingServiceClient getRetargetingThriftClient();

    BlendingServiceClient getBlendingThriftClient();

    CollisionDetectionServiceClient getCollisionDetectionThriftClient();

    GraspPoseServiceClient getGraspPoseThriftClient();

    WalkPointEstimationServiceClient getWalkPointEstimationThriftClient();

    MInverseKinematicsService.Client getIkService();

    MPathPlanningService.Client getPathPlanningService();

    MRetargetingService.Client getRetargetingService();

    MBlendingService.Client getBlendingService();

    MCollisionDetectionService.Client getCollisionDetectionService();

    MGraspPoseService.Client getGraspPoseService();

    MWalkPointEstimationService.Client getWalkPointEstimationService();

}
