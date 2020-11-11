namespace csharp MMIStandard
namespace py MMIStandard.avatar
namespace cpp MMIStandard
namespace java de.mosim.mmi.avatar

include "math.thrift"





//Class contains the relevant information for describing the characters properties such as the hierachy and anthropometric properties
// Enum is derived based on the MOSIM Skeleton Working Group andthe document MOSIM Skeleton V11
enum MJointType 
{
  Undefined,  

  LeftBallTip,
  LeftBall,
  LeftAnkle,
  LeftKnee,
  LeftHip,


  RightBallTip,
  RightBall,
  RightAnkle,
  RightKnee,
  RightHip,


  PelvisCentre,
  S1L5Joint,
  T12L1Joint,
  T1T2Joint,
  C4C5Joint,


  HeadJoint,
  HeadTip,
  MidEye,
  LeftShoulder,
  LeftElbow,
  LeftWrist,
  RightShoulder,
  RightElbow,
  RightWrist,


  LeftThumbMid,
  LeftThumbMeta,
  LeftThumbCarpal,
  LeftThumbTip,

  LeftIndexMeta,
  LeftIndexProximal,
  LeftIndexDistal,
  LeftIndexTip,

  LeftMiddleMeta,
  LeftMiddleProximal,
  LeftMiddleDistal,
  LeftMiddleTip,

  LeftRingMeta,
  LeftRingProximal,
  LeftRingDistal,
  LeftRingTip,

  LeftLittleMeta,
  LeftLittleProximal,
  LeftLittleDistal,
  LeftLittleTip,


  RightThumbMid,
  RightThumbMeta,
  RightThumbCarpal,
  RightThumbTip,

  RightIndexMeta,
  RightIndexProximal,
  RightIndexDistal,
  RightIndexTip,

  RightMiddleMeta,
  RightMiddleProximal,
  RightMiddleDistal,
  RightMiddleTip,

  RightRingMeta,
  RightRingProximal,
  RightRingDistal,
  RightRingTip,

  RightLittleMeta,
  RightLittleProximal,
  RightLittleDistal,
  RightLittleTip,
  Root
}

enum MEndeffectorType
{
	LeftHand,
	LeftFoot,
	RightHand,
	RightFoot,
	Root
}

//Channel for representing the position/rotation
enum MChannel
{
	XOffset,
	YOffset,
	ZOffset,
	XRotation,
	YRotation,
	ZRotation,
	WRotation
}

//Struct for storing the values of a posture (root transform + local rotations) in a ordered list
struct MAvatarPostureValues
{
    1: required string AvatarID; 
    2: required list<double> PostureData;
    3: optional list<MJointType> PartialJointList;
}

//Struct to store a bone
struct MJoint
{
	//Specific id of the bone
    1: required string ID;
	
	//The corresponding default type
	2: required MJointType Type;
    3: required math.MVector3 Position;
    4: required math.MQuaternion Rotation;
	5: optional list<MChannel> Channels;
	//Id of the parent bone
    6: optional string Parent;
}


//Description of an avatar posture with the full hierarchy and transform
struct MAvatarPosture
{
	1: required string AvatarID;
    2: required list<MJoint> Joints;
}

//Struct represents a hand posture 
struct MHandPose
{
	1: required list<MJoint> Joints;
	2: optional map<string,string> Properties;
}


struct MAvatarDescription
{
	1: required string AvatarID;

	//The zero posture
	2: required MAvatarPosture ZeroPosture;
	
	//The anthropometric values
	3: optional map<string,string> Properties;
}

struct MAvatar
{
	1: required string ID;
	2: required string Name;
	3: required MAvatarDescription Description;
	4: required MAvatarPostureValues PostureValues;
	5: optional list<string> SceneObjects;
	6: optional map<string,string> Properties;	
}



