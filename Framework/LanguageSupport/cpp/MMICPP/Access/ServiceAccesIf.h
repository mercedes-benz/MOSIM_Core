#pragma once
#include "src/scene_types.h"
#include "src/services_types.h"
#include "ThriftClient/ThriftClient.h"
#include "src/MInverseKinematicsService.h"
#include "src/MPathPlanningService.h"
#include "src/MGraspPoseService.h"
#include "src/MRetargetingService.h"
#include "src/MBlendingService.h"
#include "src/MCollisionDetectionService.h"
#include "src/MGraspPoseService.h"

using namespace MMIStandard;
using namespace std;

namespace MMIStandard {
	class ServiceAccessIf {
		/*
			Interface provides the access to all available services in the MMIFramework
		*/
	protected:
		shared_ptr<ThriftClient<MInverseKinematicsServiceClient>> ikService;
		shared_ptr<ThriftClient<MPathPlanningServiceClient>> pathPlanningService;
		shared_ptr<ThriftClient<MRetargetingServiceClient>> retargetingService;
		shared_ptr<ThriftClient<MBlendingServiceClient>> blendingService;
		shared_ptr<ThriftClient<MCollisionDetectionServiceClient>> collisionDetectionService;
		shared_ptr<ThriftClient<MGraspPoseServiceClient>> graspPoseService;

	public:

		virtual const ThriftClient<MInverseKinematicsServiceClient> & getIkThriftClient() = 0;
		virtual const ThriftClient<MPathPlanningServiceClient> & getPathPlanningThriftClient() = 0;
		virtual const ThriftClient<MRetargetingServiceClient> & getRetargetingThriftClient() = 0;
		virtual const ThriftClient<MBlendingServiceClient> & getBlendingThriftClient() = 0;
		virtual const ThriftClient<MCollisionDetectionServiceClient> & getCollisionDetectionThriftClient() = 0;
		virtual const ThriftClient<MGraspPoseServiceClient> & getGraspPoseThfriftClient() = 0;

		virtual MInverseKinematicsServiceClient & getIkService() = 0;
		virtual MPathPlanningServiceClient & getPathPlanningService() = 0;
		virtual MRetargetingServiceClient & getRetargetingService() = 0;
		virtual MBlendingServiceClient & getBlendingService() = 0;
		virtual MCollisionDetectionServiceClient & getCollisionDetectionServicet() = 0;
		virtual MGraspPoseServiceClient & getGraspPoseService() = 0;

		//	virtual destructor
		virtual ~ServiceAccessIf();
	};
}
