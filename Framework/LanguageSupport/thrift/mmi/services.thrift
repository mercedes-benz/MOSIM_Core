namespace csharp MMIStandard
namespace py MMIStandard.services
namespace cpp MMIStandard
namespace java de.mosim.mmi.services

include "core.thrift"
include "scene.thrift"
include "math.thrift"
include "avatar.thrift"
include "constraints.thrift"


//thrift services incloude core.thrift and math.thrift and scene.thrift
//-------------------------------------------------------------------------------------------------------------------------------------

service MMIServiceBase
{
	//Number of clients, load, update latency (e.g. time to acquire main thread)
	map<string,string> GetStatus(),
	core.MServiceDescription GetDescription(),
	core.MBoolResponse Setup(1: avatar.MAvatarDescription avatar, 2: map<string,string> properties),
	map<string,string> Consume(1: map<string,string> properties),
        core.MBoolResponse Dispose(1: map<string,string> properties),
        core.MBoolResponse Restart(1: map<string,string> properties),
}

//Must not be declared in here since no communication
service MSceneAccess extends MMIServiceBase
{
  list<scene.MSceneObject> GetSceneObjects(),
	scene.MSceneObject GetSceneObjectByID(1: string id),
	scene.MSceneObject GetSceneObjectByName(1: string name),
	list<scene.MSceneObject> GetSceneObjectsInRange(1: math.MVector3 position, 2: double range),
	list<scene.MCollider> GetColliders(),
	scene.MCollider GetColliderById(1: string id),
	list<scene.MCollider> GetCollidersInRange(1: math.MVector3 position, 2: double range),
	list<scene.MMesh> GetMeshes(),
  	scene.MMesh GetMeshByID(1: string id),
  	list<math.MTransform> GetTransforms(),
  	math.MTransform GetTransformByID(1: string id),
	list<avatar.MAvatar> GetAvatars(),
	avatar.MAvatar GetAvatarByID(1: string id),
	avatar.MAvatar GetAvatarByName(1: string name),
	list<avatar.MAvatar> GetAvatarsInRange(1: math.MVector3 position, 2: double distance),
	double GetSimulationTime(),
	scene.MSceneUpdate GetSceneChanges(),
	scene.MSceneUpdate GetFullScene(),
	scene.MNavigationMesh GetNavigationMesh(),
        binary GetData(1: string fileFormat, 2: string selection),
  
  	list<scene.MAttachment> GetAttachments(),
  	list<scene.MAttachment> GetAttachmentsByID(1: string id),
  	list<scene.MAttachment> GetAttachmentsByName(1: string name),
  	list<scene.MAttachment> GetAttachmentsChildrenRecursive(1: string id),
  	list<scene.MAttachment> GetAttachmentsParentsRecursive(1: string id),

}

struct MIKServiceResult
{
    1: required avatar.MAvatarPostureValues Posture;
    2: required bool Success;
    3: required list<double> Error;
}

//IKService definition
service MInverseKinematicsService extends MMIServiceBase
{
        //Legacy support
    avatar.MAvatarPostureValues ComputeIK(1: avatar.MAvatarPostureValues postureValues, 2: list<MIKProperty> properties),

        //New method
    MIKServiceResult CalculateIKPosture(1: avatar.MAvatarPostureValues postureValues, 2: list<constraints.MConstraint> constraints, 3: map<string,string> properties),
}


//Service which provides collision detection functionality
service MCollisionDetectionService extends MMIServiceBase
{
	math.MVector3 ComputePenetration(1: scene.MCollider colliderA, 2: math.MTransform transformA, 3: scene.MCollider colliderB, 4: math.MTransform transformB),
	bool CausesCollision(1: scene.MCollider colliderA, 2: math.MTransform transformA, 3: scene.MCollider colliderB, 4: math.MTransform transformB),
}


//To check -> in future provide motion/posture blending service
service MBlendingService extends MMIServiceBase
{
	core.MBoolResponse SetBlendingMask(1: map<math.MTransform, double> mask, 2: string avatarID),
	avatar.MAvatarPostureValues Blend (1: avatar.MAvatarPostureValues startPosture, 2: avatar.MAvatarPostureValues targetPosture, 3: double weight)
}

//Specific interface for a posture blending service
service MPostureBlendingService extends MMIServiceBase
{
	avatar.MAvatarPostureValues Blend (1: avatar.MAvatarPostureValues startPosture, 2: avatar.MAvatarPostureValues targetPosture, 3: double weight, 4: map<math.MTransform, double> mask, 5:map<string,string> properties),
	list<avatar.MAvatarPostureValues> BlendMany (1: avatar.MAvatarPostureValues startPosture, 2: avatar.MAvatarPostureValues targetPosture, 3: list<double> weights, 4: map<math.MTransform, double> mask, 5:map<string,string> properties)
}


//Interface of the path planning service
service MPathPlanningService extends MMIServiceBase
{
	constraints.MPathConstraint ComputePath(1: math.MVector start, 2: math.MVector goal, 3: list<scene.MSceneObject> sceneObjects, 4: map<string,string> properties),
  
  // properties: "ReuseEnvironment" should be used to prevent overhead
  // returns current target direction scaled by velocity (in m/s) 
  math.MVector ComputePathDirection(1: math.MVector current, 2: math.MVector goal, 3: list<scene.MSceneObject> sceneObject, 4: map<string,string> properties)
}

 
service MRetargetingService extends MMIServiceBase
{
    avatar.MAvatarDescription SetupRetargeting(1:avatar.MAvatarPosture globalTarget),
    avatar.MAvatarPostureValues RetargetToIntermediate(1:avatar.MAvatarPosture globalTarget),
    avatar.MAvatarPosture RetargetToTarget(1:avatar.MAvatarPostureValues intermediatePostureValues),
}

service MGraspPoseService extends MMIServiceBase
{
  list<constraints.MGeometryConstraint> GetGraspPoses(1: avatar.MAvatarPostureValues posture, 2: math.MTransform handType, 3: scene.MSceneObject sceneObject, 4: bool repositionHand),
	//scene.MHandPose ComputeGraspPose(1: avatar.MAvatarPosture posture, 2: math.MTransform handType, 3: scene.MSceneObject sceneObject, 4: bool repositionHand),
}


///Struct that is used for the Co-Simulation access
struct MWalkPoint
{
  1: required constraints.MGeometryConstraint PositionConstraint;
  2: required double Suitability;
}

//Estimates walk points based on the scene and a given target object. The service returns a list of MWalkPoints which contain the position as well as the suitability
service MWalkPointEstimationService extends MMIServiceBase
{
	list<MWalkPoint> EstimateWalkPoints(1: list<scene.MSceneObject> sceneObjects, 2: scene.MSceneObject target, 3: i32 amount, 4: map<string,string> properties),
}

service MSkeletonAccess extends MMIServiceBase
{
	void InitializeAnthropometry(1: avatar.MAvatarDescription description),
	avatar.MAvatarDescription GetAvatarDescription(1: string avatarID),
	void SetAnimatedJoints(1: string avatarID, 2: list<avatar.MJointType> joints),
	void SetChannelData(1: avatar.MAvatarPostureValues values),
	avatar.MAvatarPosture GetCurrentGlobalPosture(1: string avatarID),
  avatar.MAvatarPosture GetCurrentLocalPosture(1: string avatarID),
	avatar.MAvatarPostureValues GetCurrentPostureValues(1: string avatarID),
  avatar.MAvatarPostureValues GetCurrentPostureValuesPartial(1: string avatarID, 2:list<avatar.MJointType> joints),
	
  list<math.MVector3> GetCurrentJointPositions(1: string avatarID),
	
  math.MVector3 GetRootPosition(1: string avatarID),
	math.MQuaternion GetRootRotation(1: string avatarID),
	math.MVector3 GetGlobalJointPosition(1: string avatarId, 2:avatar.MJointType joint),
	math.MQuaternion GetGlobalJointRotation(1: string avatarId, 2:avatar.MJointType joint),
  math.MVector3 GetLocalJointPosition(1: string avatarId, 2: avatar.MJointType joint),
  math.MQuaternion GetLocalJointRotation(1: string avatarId, 2: avatar.MJointType joint),

  void SetRootPosition(1: string avatarId, 2:math.MVector3 position),
  void SetRootRotation(1: string avatarId, 2:math.MQuaternion rotation),
  void SetGlobalJointPosition(1: string avatarId, 2: avatar.MJointType joint, 3:math.MVector3 position),
  void SetGlobalJointRotation(1: string avatarId, 2: avatar.MJointType joint, 3:math.MQuaternion rotation),
  void SetLocalJointPosition(1: string avatarId, 2:avatar.MJointType joint, 3:math.MVector3 position),
  void SetLocalJointRotation(1: string avatarId, 2:avatar.MJointType joint, 3:math.MQuaternion rotation),

  avatar.MAvatarPostureValues RecomputeCurrentPostureValues(1: string avatarId),
}

// Coordinate system mapper service to map a coordinate system defined with an up-axis and handedness information to the MMI framework coordinate system. 
enum MDirection { Right, Left, Up, Down, Forward, Backward}

service MCoordinateSystemMapper extends MMIServiceBase
{
   math.MTransform TransformToMMI_L(1: math.MTransform transform, 2: list<MDirection> coordinateSystem),
   math.MTransform TransformToMMI(1: math.MTransform transform, 2: MDirection firstAxis, 3: MDirection secondAxis, 4: MDirection thirdAxis),
   math.MTransform TransformFromMMI_L(1: math.MTransform transform, 2: list<MDirection> coordinateSystem),
   math.MTransform TransformFromMMI(1: math.MTransform transform, 2: MDirection firstAxis, 3: MDirection secondAxis, 4: MDirection thirdAxis),

   math.MQuaternion QuaternionToMMI_L(1: math.MQuaternion quat, 2: list<MDirection> coordinateSystem),
   math.MQuaternion QuaternionToMMI(1: math.MQuaternion quat, 2: MDirection firstAxis, 3: MDirection secondAxis, 4: MDirection thirdAxis),
   math.MQuaternion QuaternionFromMMI_L(1: math.MQuaternion quat, 2: list<MDirection> coordinateSystem),
   math.MQuaternion QuaternionFromMMI(1: math.MQuaternion quat, 2: MDirection firstAxis, 3: MDirection secondAxis, 4: MDirection thirdAxis),
   
   math.MVector3 VectorToMMI_L(1: math.MVector3 quat, 2: list<MDirection> coordinateSystem),
   math.MVector3 VectorToMMI(1: math.MVector3 quat, 2: MDirection firstAxis, 3: MDirection secondAxis, 4: MDirection thirdAxis),
   math.MVector3 VectorFromMMI_L(1: math.MVector3 quat, 2: list<MDirection> coordinateSystem),
   math.MVector3 VectorFromMMI(1: math.MVector3 quat, 2: MDirection firstAxis, 3: MDirection secondAxis, 4: MDirection thirdAxis),  
 
}

service MSynchronizableScene extends MMIServiceBase
{
	core.MBoolResponse ApplyUpdates(1: scene.MSceneUpdate sceneUpdates),
	core.MBoolResponse ApplyManipulations(1: list<scene.MSceneManipulation> sceneManipulations)
}


// enums no dependencies
//-------------------------------------------------------------------------------------------------------------------------------------
//IK Service specific enum/classes

enum MIKOperationType 
{
	SetPosition,
	SetRotation
}

//dependencies from scene.thrift
//-------------------------------------------------------------------------------------------------------------------------------------

//IK Service Properties
struct MIKProperty
{
    1: required list<double> Values;
    2: required double Weight;
    3: required avatar.MEndeffectorType Target;
	4: required MIKOperationType OperationType;
}
//dependencies from core.thrift
//-------------------------------------------------------------------------------------------------------------------------------------




