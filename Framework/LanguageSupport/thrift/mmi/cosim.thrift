namespace csharp MMIStandard
namespace py MMIStandard.cosim
namespace cpp MMIStandard
namespace java de.mosim.mmi.cosim

include "core.thrift"
include "avatar.thrift"
include "mmu.thrift"
include "services.thrift"


///Struct that is used for the Co-Simulation access
struct MCoSimulationEvents
{
  1: required list<mmu.MSimulationEvent> Events;
  2: required double SimulationTime;
  3: required i32 FrameNumber;
}

//Class which represent a callback for the MCoSimulationEvent
service MCoSimulationEventCallback
{
   void OnEvent(1: MCoSimulationEvents event);
   // _OnFrameEnd is a virtual eventType
   void OnFrameEnd(1: avatar.MAvatarPostureValues newPosture);
}
 
///Service for co-simulation access
service MCoSimulationAccess extends services.MMIServiceBase
{
   //Register event handler/callback at server
   core.MBoolResponse RegisterAtEvent(1: core.MIPAddress clientAddress, 2: string eventType);

   //Unregister event handler/callback at server
   core.MBoolResponse UnregisterAtEvent(1: core.MIPAddress clientAddress, 2: string eventType);

   //Assigns an instruction at the co-simulation
   core.MBoolResponse AssignInstruction(1: mmu.MInstruction instruction, 2: map<string,string> properties),

   //Aborts all instructions
   core.MBoolResponse Abort(),

   //Aborts a single instruction
   core.MBoolResponse AbortInstruction(1: string instructionID),

   //Aborts a list of instructions
   core.MBoolResponse AbortInstructions(1: list<string> instructionIDs),

   //Get the history
   list<MCoSimulationEvents> GetHistoryFromTime(1: double startTime, 2: double endTime, 3: string eventType),
   list<MCoSimulationEvents> GetHistoryFromFrames(1: i32 fromFrame, 2: i32 toFrame, 3: string eventType),
   list<MCoSimulationEvents> GetHistory(1: string eventType),
  
   //Returns all events occured in the current frame
   MCoSimulationEvents GetCurrentEvents();
}
