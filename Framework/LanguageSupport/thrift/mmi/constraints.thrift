namespace csharp MMIStandard
namespace py MMIStandard.constraints
namespace cpp MMIStandard
namespace java de.mosim.mmi.constraints

include "math.thrift"
include "avatar.thrift"


//Constraints
//dependencies from math.thrift
//dependencies from scene.thrift order here is important
//-------------------------------------------------------------------------------------------------------------------------------------


//Format to represent a constraint
struct MConstraint
{
	//The unique id of the constraint
	1: required string ID;
	2: optional MGeometryConstraint GeometryConstraint;
	3: optional MVelocityConstraint VelocityConstraint;
	4: optional MAccelerationConstraint AccelerationConstraint;
	5: optional MPathConstraint PathConstraint;
	6: optional MJointPathConstraint JointPathConstraint;
	7: optional MPostureConstraint PostureConstraint;
	8: optional MJointConstraint JointConstraint;
	9: optional map<string,string> Properties;
}



//New constraint system
//---------------------------------------------------------------------------------------------------------


enum MTranslationConstraintType 
{
    BOX,
    ELLIPSOID
}

struct MInterval 
{
    1: required double Min;
    2: required double Max;
}

struct MInterval3 
{
    1: required MInterval X;
    2: required MInterval Y;
    3: required MInterval Z;
}

struct MTranslationConstraint 
{
    1: required MTranslationConstraintType Type;
    2: required MInterval3 Limits; //Positive and negative extent of the box or ellipsoid along the coordinate axes. May take values of 0 to +-inf.
}

struct MRotationConstraint
{
    2: required MInterval3 Limits;
}

struct MGeometryConstraint 
{
    1: required string ParentObjectID; //ID of an object in the scene graph. The object's base coordinate system serves as base for the constraint definition. 
    2: optional math.MTransform ParentToConstraint; //Transformation from parent base coordinate system to constraint base coordinate system. This coordinate system represents the centroid of the TranslationConstraint or RotationConstraint. If not given -> identity matrix is assumed.
    3: optional MTranslationConstraint TranslationConstraint;
    4: optional MRotationConstraint RotationConstraint; //Rotation is defined along the coordinate system axes using right hand rule. Unit = [rad]. If not given intervals [-inf,inf] are assumed. 
    5: optional double WeightingFactor; //Linear factor with values of 0 to 1. 1 has to be at exact point, 0 functions as a hint. 
}

struct MVelocityConstraint 
{
    1: required string ParentObjectID; //ID of an object in the scene graph. The object's base coordinate system serves as base for the constraint definition. 
    2: optional math.MTransform ParentToConstraint; //Transformation from parent base coordinate system to constraint base coordinate system. This coordinate system represents the centroid of the TranslationConstraint or RotationConstraint. If not given -> identity matrix is assumed.    
    3: optional math.MVector3 TranslationalVelocity; // Unit = [m/s]
    4: optional math.MVector3 RotationalVelocity; // Unit = [rad/s]
    5: optional double WeightingFactor;
}

struct MAccelerationConstraint 
{
    1: required string ParentObjectID; //ID of an object in the scene graph. The object's base coordinate system serves as base for the constraint definition. 
    2: optional math.MTransform ParentToConstraint; //Transformation from parent base coordinate system to constraint base coordinate system. This coordinate system represents the centroid of the TranslationConstraint or RotationConstraint. If not given -> identity matrix is assumed.    
    3: optional math.MVector3 TranslationalAcceleration; // Unit = [m/s^2]
    4: optional math.MVector3 RotationalAcceleration; // Unit = [rad/s^2]
    5: optional double WeightingFactor;
}

struct MPathConstraint 
{
    1: required list<MGeometryConstraint> PolygonPoints;
    2: optional double WeightingFactor;
}

struct MJointConstraint 
{
    1: required avatar.MJointType JointType;
    2: optional MGeometryConstraint GeometryConstraint;
    3: optional MVelocityConstraint VelocityConstraint;
    4: optional MAccelerationConstraint AccelerationConstraint;
}

struct MJointPathConstraint 
{
    1: required avatar.MJointType JointType;
    2: required MPathConstraint PathConstraint;
}

struct MPostureConstraint
{
    1: required avatar.MAvatarPostureValues posture;
    2: optional list<MJointConstraint> JointConstraints;
}

