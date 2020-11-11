namespace csharp MMIStandard
namespace py MMIStandard
namespace cpp MMIStandard
namespace java de.mosim.mmi
namespace php MMIStandard


include "math.thrift"
include "core.thrift"
include "scene.thrift"
include "services.thrift"
include "mmu.thrift"
include "AJAN.thrift"
include "avatar.thrift"
include "constraints.thrift"
include "cosim.thrift"
include "register.thrift"


// const no dependencies
//-------------------------------------------------------------------------------------------------------------------------------------
#const list<string> DefaultEventTypes = {'start', 'ready', "stroke_start", "stroke", "stroke_end","end", 'abort', 'warning', 'exception'}
const string MSimulationEvent_Start = 'start'
const string MSimulationEvent_Ready = 'ready'
const string MSimulationEvent_Stroke_Start = 'stroke_start'
const string MSimulationEvent_Stroke = 'stroke'
const string MSimulationEvent_Stroke_End = 'stroke_end'
const string MSimulationEvent_End = 'end'
const string MSimulationEvent_Abort = 'abort'
const string MSimulationEvent_Warning = 'warning'
const string MSimulationEvent_Exception = 'exception'
const string MSimulationEvent_InitError = 'initError'
const string MSimulationEvent_CycleEnd = 'cycleEnd'
