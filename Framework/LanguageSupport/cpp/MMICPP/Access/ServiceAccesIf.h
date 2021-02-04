// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "gen-cpp/scene_types.h"
#include "gen-cpp/services_types.h"
#include "ThriftClient/ThriftClient.h"
#include "gen-cpp/MInverseKinematicsService.h"
#include "gen-cpp/MPathPlanningService.h"
#include "gen-cpp/MGraspPoseService.h"
#include "gen-cpp/MRetargetingService.h"
#include "gen-cpp/MBlendingService.h"
#include "gen-cpp/MCollisionDetectionService.h"
#include "gen-cpp/MGraspPoseService.h"

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
		virtual const ThriftClient<MGraspPoseServiceClient> & getGraspPoseThriftClient() = 0;

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
