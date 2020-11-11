#pragma once

#include "Access/ServiceAccesIf.h"
#include "src/MSceneAccess.h"
#include "src/MotionModelUnit.h"
#include "src/MSkeletonAccess.h"

using namespace MMIStandard;

class MotionModelUnitBaseIf : public MotionModelUnitIf
{
	/*
		Basic MMU for representing MMUs in CPP
	*/
public:

	//	The access to the services
	 ServiceAccessIf * serviceAccess ;

	//	The access to the scene
	 MSceneAccessIf * sceneAccess;

	//	The name of the MMU
	const string name;

	//	The id of the MMU
	const int id;

	//	The access to the skeleton
	MSkeletonAccessIf * skeletonAccess;

public:

	//	Basic constructor
	MotionModelUnitBaseIf(string name, int id);

	// virtual destructor
	virtual ~MotionModelUnitBaseIf();;

	//	Initializes the given MMU 
	virtual void Initialize(::MMIStandard::MBoolResponse & _return, const::MMIStandard::MAvatarDescription & avatarDescription, const std::map<std::string, std::string>& properties)=0;

	//	Method to apply the motion instruction
	virtual void AssignInstruction(::MMIStandard::MBoolResponse & _return, const MInstruction & motionInstruction, const MSimulationState & simulationState) = 0;

	//	Basic do step routine which is executed for each frame
	virtual void DoStep(MSimulationResult & _return, const double time, const MSimulationState & simulationState) = 0;

	//	Returns constraints which are relevant for the transition
	virtual void GetBoundaryConstraints(std::vector<MConstraint>& _return, const MInstruction & instruction) = 0;

	//	Check if the instruction can be executed given the current state
	virtual void CheckPrerequisites(::MMIStandard::MBoolResponse & _return, const MInstruction & instruction) = 0;

	//	Method which forces the MMU to finish
	virtual void Abort(::MMIStandard::MBoolResponse & _return, const std::string & instructionId) = 0;

	//	Disposes the respective MMU
	virtual void Dispose(::MMIStandard::MBoolResponse & _return, const std::map<std::string, std::string>& parameters) = 0;

	//	Method to create a checkpoint
	virtual void CreateCheckpoint(std::string & _return) = 0;

	//	Method to restore a checkpoint
	virtual void RestoreCheckpoint(::MMIStandard::MBoolResponse & _return, const std::string & data) = 0;

	//	Method for executing an arbitrary function (optionally)
	virtual void ExecuteFunction(std::map<std::string, std::string>& _return, const std::string & name, const std::map<std::string, std::string>& parameters) = 0;
};

// Method must be implemented in the cpp file
// specify name and id here
//extern "C" __declspec(dllexport) MotionModelUnitBaseIf* __cdecl instantiate()
//{
//	return new MotionModelUnitBaseIf{ "C++TestMMU",1337 };
//}
