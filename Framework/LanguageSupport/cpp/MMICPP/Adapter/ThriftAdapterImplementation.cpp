// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Niclas Delfs, Stephan Adam

#include "ThriftAdapterImplementation.h"
#include "SessionData.h"
#include "SessionTools.h"
#include "SessionContent.h"
#include "SessionHandling.h"
#include "gen-cpp/MotionModelUnit.h"
#include "AdapterController.h"
#include "CPPMMUInstantiator.h"
#include "boost/exception/diagnostic_information.hpp"
#include <chrono>
#include "Utils/Logger.h"
#include "Extensions/MBoolResponseExtensions.h"

using namespace std;

void ThriftAdapterImplementation::Initialize(::MMIStandard::MBoolResponse & _return, const::MMIStandard::MAvatarDescription & avatarDescription, const std::map<std::string, std::string>& properties, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG,"Initialize");
	SessionData::lastAccess = std::time(0);
	_return.__set_Successful(true);

	try
	{
		SessionHandling::GetMMUbyId(sessionID,mmuID).Initialize(_return, avatarDescription, properties);
	}
	catch (...)
	{		
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message,false);
	}
}

void ThriftAdapterImplementation::AssignInstruction(::MMIStandard::MBoolResponse & _return, const MInstruction & instruction, const MSimulationState & simulationState, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "AssignInstruction");	
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).AssignInstruction(_return, instruction, simulationState);
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message,false);
	}
}

void ThriftAdapterImplementation::DoStep(MSimulationResult & _return, const double time, const MSimulationState & simulationState, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "DoStep");	
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).DoStep(_return, time, simulationState);
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}
}

void ThriftAdapterImplementation::GetBoundaryConstraints(std::vector<MConstraint>& _return, const MInstruction & instruction, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "GetBoundaryConstraints");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).GetBoundaryConstraints(_return, instruction);
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}
}

void ThriftAdapterImplementation::CheckPrerequisites(::MMIStandard::MBoolResponse & _return, const MInstruction & instruction, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "CheckPrerequisites");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).CheckPrerequisites(_return, instruction);
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message,false);
	}
}

void ThriftAdapterImplementation::Abort(::MMIStandard::MBoolResponse & _return, const std::string & instructionID, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "Abort");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).Abort(_return, instructionID);
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message,false);
	}
}

void ThriftAdapterImplementation::Dispose(::MMIStandard::MBoolResponse & _return, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "Dispose");
	SessionData::lastAccess = std::time(0);

	try
	{
		map<string, string> parameter;
		SessionHandling::GetMMUbyId(sessionID, mmuID).Dispose(_return, parameter);
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message, false);
	}
}

void ThriftAdapterImplementation::ExecuteFunction(std::map<std::string, std::string>& _return, const std::string & name, const std::map<std::string, std::string>& parameters, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "ExecuteFunction");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).ExecuteFunction(_return, name,parameters);
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information()); Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}
}

//TODO check version
void ThriftAdapterImplementation::GetStatus(std::map<std::string, std::string>& _return)
{
	_return["Version"] = "0.1";
	_return["Running since"] = strtok(ctime(&SessionData::startTime), "\n");
	_return["Total Sessions"] = std::to_string(SessionData::SessionContents.size()); ;
	
	if (SessionData::lastAccess == 0)
	{
		_return["Last Access"] = "None";
	}
	else
	{
		_return["Last Access"] = strtok(ctime(&SessionData::lastAccess), "\n");
	}
	_return["Loadable MMMUs"] = std::to_string(SessionData::mmuDescriptions.size());
}

void ThriftAdapterImplementation::GetAdapterDescription(MAdapterDescription & _return)
{
	Logger::printLog(L_DEBUG, "GetAdapterDescription");
	_return = SessionData::adapterDescription;
}

void ThriftAdapterImplementation::CreateSession(::MMIStandard::MBoolResponse & _return, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "CreateSession");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::CreateSessionContent(sessionID);

		_return.__set_Successful(true);
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message, false);
	}	
}

void ThriftAdapterImplementation::CloseSession(::MMIStandard::MBoolResponse & _return, const std::string & sessionID)
{
	Logger::printLog(L_INFO, "CloseSession " + sessionID );

	try {
		SessionHandling::RemoveSessionContent(sessionID);
		_return.__set_Successful(true);	
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message, false);
	}	
}

void ThriftAdapterImplementation::PushScene(::MMIStandard::MBoolResponse & _return, const::MMIStandard::MSceneUpdate & sceneUpdates, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "PushScene");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetSessionContentBySessionID(SessionTools::GetSplittedIds(sessionID)[0]).sceneBuffer->Apply(_return,sceneUpdates);	
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message, false);
	}
}

void ThriftAdapterImplementation::GetLoadableMMUs(std::vector<MMUDescription>& _return)
{
	//Logger::printLog(L_DEBUG, "GetLoadableMMUs");
	//SessionData::lastAccess = std::time(0);

	for (const MMUDescription &description : SessionData::mmuDescriptions)
	{
		_return.emplace_back(description);
	}
	
}

void ThriftAdapterImplementation::GetMMus(std::vector<MMUDescription>& _return, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "GetMMUs");
	SessionData::lastAccess = std::time(0);

	try
	{
		const AvatarContent *avatarContent = &SessionHandling::GetAvatarContentBySessionID(sessionID);

		for(auto const& mmu : avatarContent->MMUs)
		{
			_return.emplace_back(SessionData::GetMMUDescription(mmu.first));
		}	
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}
}

void ThriftAdapterImplementation::GetDescription(MMUDescription & _return, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "GetDescription");
	SessionData::lastAccess = std::time(0);
	_return =SessionData::GetMMUDescription(mmuID);
}

void ThriftAdapterImplementation::GetScene(std::vector<::MMIStandard::MSceneObject>& _return, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "GetScene");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetSessionContentBySessionID(SessionTools::GetSplittedIds(sessionID)[0]).sceneBuffer->GetSceneObjects(_return);
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}
}

void ThriftAdapterImplementation::GetSceneChanges(::MMIStandard::MSceneUpdate & _return, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "GetSceneChanges");

	try
	{		
		SessionData::lastAccess = std::time(0);
		SessionHandling::GetSessionContentBySessionID(SessionTools::GetSplittedIds(sessionID)[0]).sceneBuffer->GetSceneChanges(_return);
				
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}	
}

//TODO failure handling
void ThriftAdapterImplementation::LoadMMUs(std::map<std::string, std::string>& _return, const std::vector<std::string>& mmus, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "LoadMMUs");
	try
	{
		std::vector<string> splittedIds = SessionTools::GetSplittedIds(sessionID);
		std::string sceneId = splittedIds[0];
		std::string avatarId = splittedIds[1];
		const SessionContent *sessionContent = &SessionHandling::GetSessionContentBySceneID(sceneId);
		SessionData::lastAccess = std::time(0);
		// save the session content of the id 
		//MBoolResponse SessionResults = SessionData::GetSessionContent(sessionID, out sessionContent);

		for (const std::string &mmuId : mmus)
		{
			unique_ptr<MotionModelUnitBaseIf> mmu = nullptr;
			auto it = SessionData::mmuPaths.find(mmuId);
			if (it != SessionData::mmuPaths.end())
			{
				CPPMMUInstantiator instantiator = AdapterController::GetMMUInstantiator();
				mmu = move(instantiator.InstantiateMMU(it->second));
			}
			if (mmu != nullptr)
			{
				mmu->serviceAccess = &sessionContent->GetServiceAccess();
				mmu->sceneAccess = &sessionContent->GetScene();

				Logger::printLog(L_INFO, "Loaded MMU : " + mmu->name + " for session: "+ sessionID );

				// insert values into map 
				_return.insert(std::pair<std::string, std::string>(mmuId, sessionID));  // added, sadam
				
				auto it = sessionContent->avatarContent.find(avatarId);
				if (it == sessionContent->avatarContent.end())
				{	
					unique_ptr avatarcontent = make_unique<AvatarContent>(avatarId);
					sessionContent->avatarContent[avatarId] = move(avatarcontent);
					avatarcontent->MMUs[mmuId] = move(mmu);
				}
				sessionContent->avatarContent[avatarId]->MMUs[mmuId] = move(mmu);					
			}
		}
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		//MBoolResponseExtensions::Update(_return, message, false);
	}

}

void ThriftAdapterImplementation::CreateCheckpoint(std::string & _return, const std::string & mmuID, const std::string & sessionID)
{
	Logger::printLog(L_DEBUG, "CreateCheckPoint");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).CreateCheckpoint(_return);
	}
	catch (...)
	{
		Logger::printLog(L_ERROR, boost::current_exception_diagnostic_information());
	}
}

void ThriftAdapterImplementation::RestoreCheckpoint(::MMIStandard::MBoolResponse & _return, const std::string & mmuID, const std::string & sessionID, const std::string & checkpointData)
{
	Logger::printLog(L_DEBUG, "RestoreCheckpoint");
	SessionData::lastAccess = std::time(0);

	try
	{
		SessionHandling::GetMMUbyId(sessionID, mmuID).RestoreCheckpoint(_return, checkpointData);
	}
	catch (...)
	{
		string message = boost::current_exception_diagnostic_information();
		Logger::printLog(L_ERROR, message);
		MBoolResponseExtensions::Update(_return, message, false);
	}
}


