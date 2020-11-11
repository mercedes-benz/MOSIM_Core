namespace csharp MMIStandard
namespace py MMIStandard.math
namespace cpp MMIStandard
namespace java de.mosim.mmi.math

//no dependencies
//Optional structs which are provided
struct MVector3
{
    1: required double X;
    2: required double Y;
    3: required double Z;
}

struct MVector2
{
    1: required double X;
    2: required double Y;
}

struct MQuaternion
{
    1: required double X;
    2: required double Y;
    3: required double Z;
    4: required double W;
}

//Struct represents a vector with arbitrary elements
struct MVector
{
	1: list<double> Values;
}

//Struct to store transformation values
struct MTransform
{
    1: required string ID;
    2: required MVector3 Position;
    3: required MQuaternion Rotation;
    4: optional string Parent;
}
