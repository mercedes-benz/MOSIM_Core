package Access.Abstraction;

import MMIStandard.*;
import ThriftClients.*;

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

    MInverseKinematicsService.Client getIkService();

    MPathPlanningService.Client getPathPlanningService();

    MRetargetingService.Client getRetargetingService();

    MBlendingService.Client getBlendingService();

    MCollisionDetectionService.Client getCollisionDetectionService();

    MGraspPoseService.Client getGraspPoseService();
}
