// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#pragma once
#include "ServiceAccesIf.h"
#include "gen-cpp/core_types.h"
#include <unordered_map>
namespace MMIStandard {
	class ServiceAccess : public ServiceAccessIf
	{
		/*
				Class provides the access to all available services in the MMIFramework
		*/

	private:
		//	The address of the MMIRegister
		MIPAddress const *mmiRegisterAddress;

		//	Map which contains all descriptions of the avaialable services
		unordered_map<string, MServiceDescription> serviceDescriptions;

		//	The id of the session to which it belongs
		string sessionID;

	private:
		//	Getter for the service description based on the service name
		MServiceDescription* getServiceDescription(const string &servicename);

	public:
		//	Basic constructor
		ServiceAccess(const MIPAddress  &registerAddress, const string &sessionID);

		//	Fetches all service descriptions from the MMIRegister
		void initialize();

		// Inherited via ServiceAccessIf
		virtual ThriftClient<MInverseKinematicsServiceClient>& getIkThriftClient() override;
		virtual ThriftClient<MPathPlanningServiceClient>& getPathPlanningThriftClient() override;
		virtual ThriftClient<MRetargetingServiceClient>& getRetargetingThriftClient() override;
		virtual ThriftClient<MBlendingServiceClient>& getBlendingThriftClient() override;
		virtual ThriftClient<MCollisionDetectionServiceClient>& getCollisionDetectionThriftClient() override;
		virtual ThriftClient<MGraspPoseServiceClient>& getGraspPoseThriftClient() override;
		virtual MInverseKinematicsServiceClient & getIkService() override;
		virtual MPathPlanningServiceClient & getPathPlanningService() override;
		virtual MRetargetingServiceClient & getRetargetingService() override;
		virtual MBlendingServiceClient & getBlendingService() override;
		virtual MCollisionDetectionServiceClient & getCollisionDetectionServicet() override;
		virtual MGraspPoseServiceClient & getGraspPoseService() override;
	};
}