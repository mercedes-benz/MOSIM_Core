#pragma once

#include "src/MMIAdapter.h"

using namespace MMIStandard;
namespace MMIStandard {
	class ThriftAdapterImplementation :public MMIAdapterIf
	{
		/**
			Implementation of the thrift adapter functionality
		*/
	public:
		//	Basic initialization of a MMMU
		void Initialize(::MMIStandard::MBoolResponse& _return, const  ::MMIStandard::MAvatarDescription& avatarDescription, const std::map<std::string, std::string> & properties, const std::string& mmuID, const std::string& sessionID);

		//	Execute command of a MMU
		void AssignInstruction(::MMIStandard::MBoolResponse& _return, const MInstruction& instruction, const MSimulationState& simulationState, const std::string& mmuID, const std::string& sessionID);

		//	Basic do step routine which triggers the simulation update of the repsective MMU
		void DoStep(MSimulationResult& _return, const double time, const MSimulationState& simulationState, const std::string& mmuID, const std::string& sessionID);

		//	Returns constraints which are relevant for the transition
		void GetBoundaryConstraints(std::vector<MConstraint> & _return, const MInstruction& instruction, const std::string& mmuID, const std::string& sessionID);

		//	Check wether the instruction can be executed given the current state
		void CheckPrerequisites(::MMIStandard::MBoolResponse& _return, const MInstruction& instruction, const std::string& mmuID, const std::string& sessionID);

		//	Method which forces the MMU to finish
		void Abort(::MMIStandard::MBoolResponse& _return, const std::string& instructionID, const std::string& mmuID, const std::string& sessionID);

		//	Method diposes the MMU
		void Dispose(::MMIStandard::MBoolResponse& _return, const std::string& mmuID, const std::string& sessionID);

		//	Method for executing an arbitrary function (optionally)
		void ExecuteFunction(std::map<std::string, std::string> & _return, const std::string& name, const std::map<std::string, std::string> & parameters, const std::string& mmuID, const std::string& sessionID);

		//	Returns the status of the adapter
		void GetStatus(std::map<std::string, std::string> & _return);

		//	Returns the MAdapterDescription of the adatpter in detail
		void GetAdapterDescription(MAdapterDescription& _return);

		//	Creates a session
		void CreateSession(::MMIStandard::MBoolResponse& _return, const std::string& sessionID);

		//	Closes the session
		void CloseSession(::MMIStandard::MBoolResponse& _return, const std::string& sessionID);

		//	Method to synchronize the scene
		void PushScene(::MMIStandard::MBoolResponse& _return, const  ::MMIStandard::MSceneUpdate& sceneUpdates, const std::string& sessionID);

		//	Returns despritions of all MMUs which can be loaded
		void GetLoadableMMUs(std::vector<MMUDescription> & _return);

		//	Returns all MMUs of the session
		void GetMMus(std::vector<MMUDescription> & _return, const std::string& sessionID);

		//	Returns the description of the MMU
		void GetDescription(MMUDescription& _return, const std::string& mmuID, const std::string& sessionID);

		//	Returns the whole scene
		void GetScene(std::vector< ::MMIStandard::MSceneObject> & _return, const std::string& sessionID);

		//	Returns the scene changes of the current frame
		void GetSceneChanges(::MMIStandard::MSceneUpdate& _return, const std::string& sessionID);

		//	Method loads MMUs for the specific session
		void LoadMMUs(::MMIStandard::MBoolResponse& _return, const std::vector<std::string> & mmus, const std::string& sessionID);

		//	Method creates checkpoint of the given MMU
		void CreateCheckpoint(std::string& _return, const std::string& mmuID, const std::string& sessionID);

		//	Restores the checkpoint of the given MMU
		void RestoreCheckpoint(::MMIStandard::MBoolResponse& _return, const std::string& mmuID, const std::string& sessionID, const std::string& checkpointData);
	};
}

