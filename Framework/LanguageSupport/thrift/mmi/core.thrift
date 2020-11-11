namespace csharp MMIStandard
namespace py MMIStandard.core
namespace cpp MMIStandard
namespace java de.mosim.mmi.core


//no dependencies

//Description format of a web adddress
struct MIPAddress
{
    1: required string Address;
    2: required i32 Port;
}

struct MBoolResponse
{
	1:required bool Successful;
	2:optional list<string> LogData;
}

///Format to represent a paramter which can be set (e.g. for a MMU)
struct MParameter
{
	///The name of the parameter
	1:required string Name;

	//The type (e.g. bool, string)
	2:required string Type;

	//A detailed description of the parameter
	3:required string Description;

	//Indicates whether the parameter is required or optional
	4:required bool Required;
}

//Description format of an executable file
struct MExecutableDescription
{
    1: required string Name; 
    2: required string ID;
    3: required string Language;
	4: required string ExecutableName;
	5: required string Author;
	6: required string Version;
	7: optional list<string> Dependencies;
}

//Format to describe the main aspects of a service
struct MServiceDescription
{
	1: required string Name;
	2: required string ID;
    3: required string Language;
	4: required list<MIPAddress> Addresses;
	5: optional map<string,string> Properties;
	6: optional list<MParameter> Parameters;
}
