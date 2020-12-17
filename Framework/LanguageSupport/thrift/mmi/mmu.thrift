namespace csharp MMIStandard
namespace py MMIStandard.mmu
namespace cpp MMIStandard
namespace java de.mosim.mmi.mmu

include "core.thrift"
include "avatar.thrift"
include "scene.thrift"
include "constraints.thrift"


//-------------------------------------------------------------------------------------------------------------------------------------

//Schematic implementation of a MMU which could be used within the adapters
service MotionModelUnit
{	
	//Initializes the given MMU 
    core.MBoolResponse Initialize(1: avatar.MAvatarDescription avatarDescription, 2: map<string,string> properties),
	
	//Method to apply the motion instruction
    core.MBoolResponse AssignInstruction(1: MInstruction motionInstruction,2:MSimulationState simulationState),
	
	//Basic do step routine which is executed for each frame
	MSimulationResult DoStep(1: double time, 2: MSimulationState simulationState),
		
	//Returns constraints which are relevant for the transition
    list<constraints.MConstraint> GetBoundaryConstraints(1:MInstruction instruction),

	//Check whether the instruction can be executed given the current state
	core.MBoolResponse CheckPrerequisites(1:MInstruction instruction),
	
	//Method which forces the MMU to finish
    core.MBoolResponse Abort(1:string instructionId),
	
	//Disposes the respective MMU
	core.MBoolResponse Dispose(1: map<string,string> parameters),
		
	//Method to create a checkpoint
	binary CreateCheckpoint(),
	
	//Method to restore a checkpoint
	core.MBoolResponse RestoreCheckpoint(1:binary data), 

	//Method for executing an arbitrary function (optionally)
	map<string,string> ExecuteFunction(1:string name, 2: map<string,string> parameters)
}



//Struct which represents the input for a MMU (the present state of the simulation)
struct MSimulationState
{
	1: required avatar.MAvatarPostureValues Initial;
    2: required avatar.MAvatarPostureValues Current;
	3: optional list<constraints.MConstraint> Constraints;
	4: optional list<scene.MSceneManipulation> SceneManipulations;
	5: optional list<MSimulationEvent> Events;
}

//Result which is provided by the mmus
struct MSimulationResult
{
    1: required avatar.MAvatarPostureValues Posture;
	2: optional list<constraints.MConstraint> Constraints;
    3: optional list<MSimulationEvent> Events;
    4: optional list<scene.MSceneManipulation> SceneManipulations;
    5: optional list<scene.MDrawingCall> DrawingCalls;
	6: optional list<string> LogData;
}


// enums no dependencies
//-------------------------------------------------------------------------------------------------------------------------------------
//Type of the coordinate system
enum MCoordinateSystemType
{
	Global,
	Local
}	


//no dependencies 
//---------------------------------------------------------------------------------------------------------
//Simulation event
struct MSimulationEvent
{
	1: required string Name;
	2: required string Type;
	3: required string Reference;
	4: optional map<string,string> Properties;
}



//dependencies from core.thrift
//-------------------------------------------------------------------------------------------------------------------------------------


enum MDependencyType
{
  Service,
  MMU,
  ProgramLibrary,
  MMIFramework,
  Other
}

struct MVersion
{
  1: required i16 Major;
  2: optional i16 Minor;
  3: optional i16 Sub;
  4: optional i16 Subsub;
}

struct MDependency
{
   1: required string ID;
   2: required MDependencyType Type;
   3: required MVersion MinVersion;
   4: required MVersion MaxVersion;
   5: optional list <MVersion> ExcludedVersions;
   6: optional string Name;
}




///The description file for the MMU
struct MMUDescription
{
    1: required string Name; 	
    2: required string ID;
	3: required string AssemblyName;
    4: required string MotionType;

    6: required string Language;
    7: required string Author;
	8: required string Version;
	9: optional list<constraints.MConstraint> Prerequisites;

	11: optional map<string,string> Properties;
	12: optional list<MDependency> Dependencies;
	13: optional list<string> Events;
	14: optional string LongDescription;
	15: optional string ShortDescription;
	//All parameters which are provided by the MMU (e.g. subject, target, velocity)
	16: optional list<core.MParameter> Parameters;
	17: optional list<core.MParameter> SceneParameters;
	//Additional parameters utilized by MMU library, first two should actually be required but are left as optional for backward compatibility, their enforcing could come from MMU Description Editor or MMU validator
	18: optional string Vendor;
	19: optional string VendorDomain;
	20: optional string MmuUrl;
	21: optional string UpdateUrl;
}




//Format for intended motion instructions/control input
struct MInstruction 
{
    1: required string ID;
    2: required string Name;
    3: required string MotionType;
    4: optional map<string,string> Properties;
	5: optional list<constraints.MConstraint> Constraints;
	6: optional string StartCondition;
	7: optional string EndCondition;
	8: optional string Action;
	9: optional list<MInstruction> Instructions;
}



















