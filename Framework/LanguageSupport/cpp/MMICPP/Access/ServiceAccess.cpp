#include "ServiceAccess.h"
#include "src/MMIRegisterService.h"
#include "src/MGraspPoseService.h"
#include "src/MCollisionDetectionService.h"
#include "src/MPathPlanningService.h"
#include "src/MInverseKinematicsService.h"
#include "ThriftClient/ThriftClient.cpp"
#include "Utils/Logger.h"

ServiceAccess::ServiceAccess(const MIPAddress  &registerAddress, const string &sessionID) : sessionID{ sessionID } {
	this->mmiRegisterAddress = &registerAddress;
}


MServiceDescription* ServiceAccess::getServiceDescription(const string &serviceName)
{
	unordered_map<string, MServiceDescription>::iterator it = this->serviceDescriptions.find(serviceName);
	if (it == this->serviceDescriptions.end())
	{
		this->initialize();	
	}

	it = this->serviceDescriptions.find(serviceName);
	if (it == this->serviceDescriptions.end())
	{
		return nullptr;
	}
	else
	{
		return &it->second;
	}
}

void ServiceAccess::initialize()
{
	Logger::printLog(L_INFO, "Trying to fetch the services from register: " +  this->mmiRegisterAddress->Address + ":" +  to_string(this->mmiRegisterAddress->Port) );

	try
	{
		ThriftClient<MMIRegisterServiceClient> client{ this->mmiRegisterAddress->Address,this->mmiRegisterAddress->Port};
		client.Start();
		vector<MServiceDescription> serviceDescriptions;
		client.access->GetRegisteredServices(serviceDescriptions,this->sessionID);
	
		for (MServiceDescription &serviceDescription : serviceDescriptions)
		{
			Logger::printLog(L_INFO, "found: " +serviceDescription.Name + " at " + serviceDescription.Addresses[0].Address + ":" + to_string(serviceDescription.Addresses[0].Port));
			unordered_map<string, MServiceDescription>::iterator it = this->serviceDescriptions.find(serviceDescription.Name);
			if (it == this->serviceDescriptions.end())
			{
				this->serviceDescriptions[serviceDescription.Name] = serviceDescription;
			}
		}
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, "Problem fetching available services from the register.Wrong address ? Server down ? ");
	}
}

 ThriftClient<MInverseKinematicsServiceClient>& ServiceAccess::getIkThriftClient()
{
	MServiceDescription* serviceDescription = this->getServiceDescription("ikService");
	
		if (serviceDescription == nullptr)
		{
			throw std::runtime_error("IK-Service not found");
		}
		else
		{
			if (!this->ikService)
			{
				this->ikService = make_shared<ThriftClient<MInverseKinematicsServiceClient>>(serviceDescription->Addresses[0].Address, serviceDescription->Addresses[0].Port);
			}
			this->ikService->Start();
			return *(this->ikService);
		}
}

ThriftClient<MPathPlanningServiceClient>& ServiceAccess::getPathPlanningThriftClient()
{
	MServiceDescription* serviceDescription = this->getServiceDescription("pathPlanningService");

	if (serviceDescription == nullptr)
	{
		//cerr << "Problem comuting IK" << endl;
		throw std::runtime_error("pathPlanning-Service not found");
	}
	else
	{
		if (!this->ikService)
		{
			this->pathPlanningService = make_shared<ThriftClient<MPathPlanningServiceClient>>(serviceDescription->Addresses[0].Address, serviceDescription->Addresses[0].Port);	
		}
		this->pathPlanningService->Start();
		return *(this->pathPlanningService);
	}
}

 ThriftClient<MRetargetingServiceClient>& ServiceAccess::getRetargetingThriftClient()
{
	MServiceDescription* serviceDescription = this->getServiceDescription("retargetingService");

	if (serviceDescription == nullptr)
	{
		//cerr << "Problem comuting IK" << endl;
		throw std::runtime_error("retargeting-Service not found");
	}
	else
	{
		if (!this->ikService)
		{
			this->retargetingService = make_shared<ThriftClient<MRetargetingServiceClient>>(serviceDescription->Addresses[0].Address, serviceDescription->Addresses[0].Port);
		}
		this->retargetingService->Start();
		return *(this->retargetingService);
	}
}

 ThriftClient<MBlendingServiceClient>& ServiceAccess::getBlendingThriftClient()
{
	throw std::runtime_error("Function not implemented yet");

	MServiceDescription* serviceDescription = this->getServiceDescription("blendingService");

	if (serviceDescription == nullptr)
	{
		//cerr << "Problem comuting IK" << endl;
		throw std::runtime_error("blending-Service not found");
	}
	else
	{
		if (!this->ikService)
		{
			this->blendingService = make_shared<ThriftClient<MBlendingServiceClient >>(serviceDescription->Addresses[0].Address, serviceDescription->Addresses[0].Port);		
		}
		this->blendingService->Start();
		return *(this->blendingService);
	}
}

 ThriftClient<MCollisionDetectionServiceClient>& ServiceAccess::getCollisionDetectionThriftClient()
{
	MServiceDescription* serviceDescription = this->getServiceDescription("collisionDetectionService");

	if (serviceDescription == nullptr)
	{
		//cerr << "Problem comuting IK" << endl;
		throw std::runtime_error("collisionDetection-Service not found");
	}
	else
	{
		if (!this->ikService)
		{
			this->collisionDetectionService = make_shared<ThriftClient<MCollisionDetectionServiceClient>>(serviceDescription->Addresses[0].Address, serviceDescription->Addresses[0].Port);
		}
		this->collisionDetectionService->Start();
		return *(this->collisionDetectionService);
	}
}

ThriftClient<MGraspPoseServiceClient>& ServiceAccess::getGraspPoseThfriftClient()
{
	MServiceDescription* serviceDescription = this->getServiceDescription("graspPoseService");

	if (serviceDescription == nullptr)
	{
		//cerr << "Problem comuting IK" << endl;
		throw std::runtime_error("graspPose-Service not found");
	}
	else
	{
		if (!this->ikService)
		{
			this->graspPoseService = make_shared<ThriftClient<MGraspPoseServiceClient>>(serviceDescription->Addresses[0].Address, serviceDescription->Addresses[0].Port);
		}
		this->graspPoseService->Start();
		return *(this->graspPoseService);
	}
}

MInverseKinematicsServiceClient & ServiceAccess::getIkService()
{
	return *(this->getIkThriftClient().access);
}

MPathPlanningServiceClient & ServiceAccess::getPathPlanningService()
{
	return *(this->getPathPlanningThriftClient().access);
}

MRetargetingServiceClient & ServiceAccess::getRetargetingService()
{
	return *(this->getRetargetingThriftClient().access);
}

MBlendingServiceClient & ServiceAccess::getBlendingService()
{
	throw std::runtime_error("Function not implemented yet");
	return *(this->getBlendingThriftClient().access);
}

MCollisionDetectionServiceClient & ServiceAccess::getCollisionDetectionServicet()
{
	return *(this->getCollisionDetectionThriftClient().access);
}

MGraspPoseServiceClient & ServiceAccess::getGraspPoseService()
{
	return *(this->getGraspPoseThfriftClient().access);
}


