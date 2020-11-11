namespace csharp MMIStandard
namespace py MMIStandard.scene
namespace cpp MMIStandard
namespace java de.mosim.mmi.scene

include "math.thrift"
include "core.thrift"
include "avatar.thrift"
include "constraints.thrift"


//thrift services core dependencies, math dependencies
//-------------------------------------------------------------------------------------------------------------


// enums no dependencies
//-------------------------------------------------------------------------------------------------------------------------------------


enum MDrawingCallType
{
	DrawLine2D,
	DrawLine3D,
	DrawPoint2D,
	DrawPoint3D,
	DrawText,
	Custom
}

//Interaction type with the physics engine
enum MPhysicsInteractionType
{
	AddForce,
	AddTorque,
	ChangeVelocity,
	ChangeAngularVelocity,
	ChangeMass,
	ChangeCenterOfMass,
	ChangeInertia
}

enum MColliderType 
{
	Box,
	Sphere,
	Capsule,
	Cone,
	Cylinder,
	Mesh,
	Custom
}



//no dependencies 
//---------------------------------------------------------------------------------------------------------
struct MAttachment
{
	1: required string Parent;
  	2: required string Child;
	3: optional string Type;
}

struct MAttachmentManipulation
{
	1: required string Parent;
	2: required string Child;
  	// True if add, False if Remove
  	3: required bool AddRemove;
	4: optional string Type;
}


struct MPropertyManipulation
{
	1: required string Target;
	2: required string Key;
  3: required bool AddRemove;
	4: optional string Value;
}

struct MTransformUpdate
{
	1: optional list<double> Position;
	2: optional list<double> Rotation;
	3: optional string Parent;
}

struct MPropertyUpdate
{
	1: required string Key;
	2: optional string Value;
}

//Struct to define all physics properties of a sceneobejct
struct MPhysicsProperties
{
    1: required double Mass;
    2: required list<double> CenterOfMass;
    3: optional list<double> Inertia;
    4: optional list<double> Velocity;
    5: optional list<double> AngularVelocity;
    //New fields
    6: optional list<double> NetForce; //force applied in the previous simulation update in global coordinate system
    7: optional list<double> NetTorque; //torque applied in the previous simulation update in global coordinate system
    8: optional double Mu1; // friction coefficient for the primary direction.
    9: optional double Mu2; // friction coefficient for the secondary direction.
    10: optional double Bounciness; // bouncyness
    11: optional double MuTorsion; // coefficient for torsional friction (ONLY Open Dynamics Engine)
    12: optional double TorsionSurfaceRadius; // parameter for torsional friction (ONLY Open Dynamics Engine) }
}

struct MSphereColliderProperties
{
	1: required double Radius;
}

struct MConeColliderProperties
{
	1: required double Radius;
	2: required double Height;
}

struct MCylinderColliderProperties
{
	1: required double Radius;
	2: required double Height;
}


// dependencies from math.thrift
//---------------------------------------------------------------------------------------------------------

struct MTransformManipulation
{
	1: required string Target;
	2: optional math.MVector3 Position;
	3: optional math.MQuaternion Rotation;
	4: optional string Parent;
}

struct MBoxColliderProperties
{
	1: required math.MVector3 Size;
}

struct MCapsuleColliderProperties
{
	1: required double Radius;
	2: required double Height;
	3: optional math.MVector3 MainAxis;	
}

struct MMeshColliderProperties
{
	1: required list<math.MVector3> Vertices;
	2: required list<i32> Triangles;
}

//Struct to store the information of a mesh
struct MMesh
{
   1: required string ID;
   2: required list<math.MVector3> Vertices;
   3: required list<i32> Triangles;
   4: optional list<math.MVector2> UVCoordinates;
   5: optional map<string,string> Properties;
}

struct MNavigationMesh
{
   1: required list<math.MVector3> Vertices;
   2: required list<i32> Triangles;
   3: optional map<string,string> Properties;
}

// dependencies from this file order is important
//---------------------------------------------------------------------------------------------------------


//Format to represent a drawing call which can be interpeted and executed by the target engine
struct MDrawingCall
{
	1: required MDrawingCallType Type;
	2: optional list<double> Data;
	3: optional map<string,string> Properties;
}

//Struct describes an interaction with the physics engine
struct MPhysicsInteraction
{
	1: required string Target;
	2: required MPhysicsInteractionType Type;
	3: required list<double> Values;
	4: optional map<string,string> Properties;
}

struct MSceneManipulation
{
	1: optional list<MTransformManipulation> Transforms;
	2: optional list<MPhysicsInteraction> PhysicsInteractions;
	3: optional list<MPropertyManipulation> Properties;
  	4: optional list<MAttachmentManipulation> Attachments;
}



//To check
struct MCollider
{
	1: required string ID;

	///The type of the collider
    	2: required MColliderType Type;
	
	//The specific collider description
	3: optional MBoxColliderProperties BoxColliderProperties;
	4: optional MSphereColliderProperties SphereColliderProperties;
	5: optional MCapsuleColliderProperties CapsuleColliderProperties;
	6: optional MConeColliderProperties ConeColliderProperties;
	7: optional MCylinderColliderProperties CylinderColliderProperties;
	8: optional MMeshColliderProperties MeshColliderProperties;
	9: optional math.MVector3 PositionOffset;
	10: optional math.MQuaternion RotationOffset;
	11: optional list<MCollider> Colliders;
	12: optional map<string,string> Properties;
}

//Format to represent a scene object
struct MSceneObject
{
    1: required string ID;
    2: required string Name;
    3: required math.MTransform Transform;
    4: optional MCollider Collider;
    5: optional MMesh Mesh;
    6: optional MPhysicsProperties PhysicsProperties; 
    8: optional map<string,string> Properties;
    9: optional list<MAttachment> Attachments;
    10: optional list<constraints.MConstraint> Constraints;
}
struct MSceneObjectUpdate
{
	1: required string ID;
	2: optional string Name;
	3: optional MTransformUpdate Transform;
	4: optional MCollider Collider;
	5: optional MMesh Mesh;
	6: optional MPhysicsProperties PhysicsProperties;
	7: optional list<avatar.MHandPose> HandPoses;
	8: optional list<MPropertyUpdate> Properties;
        9: optional list<MAttachment> Attachments;
	10: optional list<constraints.MConstraint> Constraints;
}

//Formats for efficient data transmission & synchronization of the scene
struct MSceneUpdate
{
	1: optional list<MSceneObject> AddedSceneObjects;
	2: optional list<MSceneObjectUpdate> ChangedSceneObjects;
        3: optional list<string> RemovedSceneObjects;
	5: optional list<avatar.MAvatar> AddedAvatars;
	6: optional list<MAvatarUpdate> ChangedAvatars;
        7: optional list<string> RemovedAvatars;
}

struct MAvatarUpdate
{
	1: required string ID;
	2: optional avatar.MAvatarPostureValues PostureValues;
	3: optional list<string> SceneObjects;
	4: optional avatar.MAvatarDescription Description;	
	5: optional list<MPropertyUpdate> Properties;
}








