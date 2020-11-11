namespace csharp MMIStandard
namespace py MMIStandard.register
namespace cpp MMIStandard
namespace java de.mosim.mmi.register

include "core.thrift"
include "avatar.thrift"
include "scene.thrift"
include "mmu.thrift"
include "constraints.thrift"



//Service interface description of a MMI Adapter
service MMIAdapter
{
    //-------------------------------Basic mmu accessing methods-----------------------------------------------------------
	
	//Initializes the given MMU 
    core.MBoolResponse Initialize(1: avatar.MAvatarDescription avatarDescription, 2: map<string,string> properties, 3: string mmuID, 4: string sessionID),
	
	//Method to execute the motion instruction
    core.MBoolResponse AssignInstruction(1: mmu.MInstruction instruction, 2: mmu.MSimulationState simulationState, 3: string mmuID, 4: string sessionID),
	
	//Basic do step routine which is executed for each frame
	mmu.MSimulationResult DoStep(1: double time, 2: mmu.MSimulationState simulationState,3: string mmuID, 4: string sessionID),
	
	//Returns constraints which are relevant for the transition
    list<constraints.MConstraint> GetBoundaryConstraints(1:mmu.MInstruction instruction,2: string mmuID, 3: string sessionID),

	//Check whether the instruction can be executed given the current state
	core.MBoolResponse CheckPrerequisites(1: mmu.MInstruction instruction, 2: string mmuID, 3: string sessionID),
	
	//Method which forces the MMU to finish
    core.MBoolResponse Abort(1: string instructionID, 2: string mmuID, 3: string sessionID),
	
	//Method diposes the MMU
    core.MBoolResponse Dispose(1: string mmuID, 2: string sessionID),

	//Method for executing an arbitrary function (optionally)
	map<string,string> ExecuteFunction(1:string name, 2: map<string,string> parameters, 3: string mmuID, 4: string sessionID),
		
	//---------------------------------------------------------------------------------------------------------------------
	
	//Returns the status of the adapter
	map<string,string> GetStatus(),

    //Returns the MAdapterDescription describing the adatpter in detail
	MAdapterDescription GetAdapterDescription(),
	
	//Creates a session
	core.MBoolResponse CreateSession(1: string sessionID),
	
	//Closes the session
	core.MBoolResponse CloseSession(1:string sessionID),
	
	//Method to synchronize the scene
    core.MBoolResponse PushScene(1: scene.MSceneUpdate sceneUpdates, 2:string sessionID),
	
	//Returns despritions of all MMUs which can be loaded
    list<mmu.MMUDescription> GetLoadableMMUs(),
	
	//Returns all MMUs of the session
	list<mmu.MMUDescription> GetMMus(1: string sessionID),
	
	//Returns the description of the MMU
    mmu.MMUDescription GetDescription(1: string mmuID, 2: string sessionID),
	
	//Returns the whole scene
	list<scene.MSceneObject> GetScene(1:string sessionID),
	
	//Returns the scene schanges of the current frame
	scene.MSceneUpdate GetSceneChanges(1: string sessionID),
	
	//Method loads MMUs for the specific session
    map<string,string> LoadMMUs(1: list<string> mmus, 2: string sessionID),
	
	//Creates a checkpoint for the given MMUs
	binary CreateCheckpoint(1: string mmuID, 2: string sessionID),
	
	//Restores the checkpoint of the given MMUs
	core.MBoolResponse RestoreCheckpoint(1: string mmuID, 2: string sessionID, 3: binary checkpointData),
}

//Description format for an adapter
struct MAdapterDescription
{
    1: required string Name; 
    2: required string ID;
    3: required string Language;
	4: required list<core.MIPAddress> Addresses;
	5: optional map<string,string> Properties;
	6: optional list<core.MParameter> Parameters;
}


//A service which provides accessing functionalities to the currently hosted adpaters and services
service MMIRegisterService
{
	//Debug
	list<MAdapterDescription> GetRegisteredAdapters(1: string sessionID),
	
	//Returns all registered services 
	list<core.MServiceDescription> GetRegisteredServices(1: string sessionID),	
	map<mmu.MMUDescription,list<core.MIPAddress>>GetAvailableMMUs(1: string sessionID),

	
	core.MBoolResponse RegisterAdapter(1: MAdapterDescription adapterDescription),
	core.MBoolResponse UnregisterAdapter(1: MAdapterDescription adapterDescription)
	
	core.MBoolResponse RegisterService(1: core.MServiceDescription serviceDescription),
	core.MBoolResponse UnregisterService(1: core.MServiceDescription serviceDescription),
	
	//Creates a unique session id
	string CreateSessionID(1: map<string,string> properties)
}
