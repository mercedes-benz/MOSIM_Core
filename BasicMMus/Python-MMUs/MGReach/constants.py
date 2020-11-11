from MMIStandard.scene.ttypes import MJointType, MJoint, MChannel
from MMIStandard.math.ttypes import MVector3, MQuaternion

JOINT_TYPE_MAP = dict()
JOINT_TYPE_MAP["pelvis"] = MJointType.PelvisCentre#, # Hips

JOINT_TYPE_MAP["right_hip"] = MJointType.RightHip # Right Leg
JOINT_TYPE_MAP["right_knee"] =MJointType.RightKnee
JOINT_TYPE_MAP["right_ankle"] =MJointType.RightAnkle

JOINT_TYPE_MAP["left_hip"] = MJointType.LeftHip # Left Leg
JOINT_TYPE_MAP["left_knee"] = MJointType.LeftKnee
JOINT_TYPE_MAP["left_ankle"] = MJointType.LeftAnkle

JOINT_TYPE_MAP["spine"] = MJointType.S1L5Joint   # spine # not known
JOINT_TYPE_MAP["spine_1"] = MJointType.T12L1Joint
JOINT_TYPE_MAP["spine_2"] = MJointType.T1T2Joint
#JOINT_TYPE_MAP["spine"] = MJointType.Undefined
JOINT_TYPE_MAP["neck"] = MJointType.C4C5Joint # not known
JOINT_TYPE_MAP["head"] = MJointType.HeadJoint


# Right Arm
JOINT_TYPE_MAP["right_shoulder"] = MJointType.RightShoulder
JOINT_TYPE_MAP["right_elbow"] = MJointType.RightElbow
JOINT_TYPE_MAP["right_wrist"] = MJointType.RightWrist

 # Left Arm
JOINT_TYPE_MAP["left_shoulder"] = MJointType.LeftShoulder
JOINT_TYPE_MAP["left_elbow"] = MJointType.LeftElbow
JOINT_TYPE_MAP["left_wrist"] = MJointType.LeftWrist
include_fingers = True
if include_fingers:
    #right
    JOINT_TYPE_MAP["right_thumb_base"] = MJointType.RightThumbMeta# thumb
    JOINT_TYPE_MAP["right_thumb_mid"] = MJointType.RightThumbMid  
    JOINT_TYPE_MAP["right_thumb_tip"] = MJointType.RightThumbCarpal 

    JOINT_TYPE_MAP["right_index_finger_base"] = MJointType.RightIndexProximal
    JOINT_TYPE_MAP["right_index_finger_mid"] = MJointType.RightIndexMeta
    JOINT_TYPE_MAP["right_index_finger_tip"] = MJointType.RightIndexDistal

    JOINT_TYPE_MAP["right_middle_finger_base"] = MJointType.RightMiddleProximal
    JOINT_TYPE_MAP["right_middle_finger_mid"] = MJointType.RightMiddleMeta
    JOINT_TYPE_MAP["right_middle_finger_tip"] = MJointType.RightRingDistal

    JOINT_TYPE_MAP["right_ring_finger_base"] = MJointType.RightRingProximal
    JOINT_TYPE_MAP["right_ring_finger_mid"] =  MJointType.RightRingMeta
    JOINT_TYPE_MAP["right_ring_finger_tip"] = MJointType.RightMiddleDistal

    JOINT_TYPE_MAP["right_pinky_finger_base"] = MJointType.RightLittleProximal
    JOINT_TYPE_MAP["right_pinky_finger_mid"] =  MJointType.RightLittleMeta
    JOINT_TYPE_MAP["right_pinky_finger_tip"] = MJointType.RightLittleDistal
    # left
    JOINT_TYPE_MAP["left_thumb_base"] = MJointType.LeftThumbMeta # thumb
    JOINT_TYPE_MAP["left_thumb_mid"] =  MJointType.LeftThumbMid
    JOINT_TYPE_MAP["left_thumb_tip"] = MJointType.LeftThumbCarpal 

    JOINT_TYPE_MAP["left_index_finger_base"] =  MJointType.LeftIndexProximal
    JOINT_TYPE_MAP["left_index_finger_mid"] = MJointType.LeftIndexMeta
    JOINT_TYPE_MAP["left_index_finger_tip"] =MJointType.LeftIndexDistal

    JOINT_TYPE_MAP["left_middle_finger_base"] = MJointType.LeftMiddleProximal
    JOINT_TYPE_MAP["left_middle_finger_mid"] = MJointType.LeftMiddleMeta
    JOINT_TYPE_MAP["left_middle_finger_tip"] = MJointType.LeftRingDistal

    JOINT_TYPE_MAP["left_ring_finger_base"] = MJointType.LeftRingProximal
    JOINT_TYPE_MAP["left_ring_finger_mid"] =  MJointType.LeftRingMeta
    JOINT_TYPE_MAP["left_ring_finger_tip"] = MJointType.LeftMiddleDistal

    JOINT_TYPE_MAP["left_pinky_finger_base"] = MJointType.LeftLittleProximal
    JOINT_TYPE_MAP["left_pinky_finger_mid"] =  MJointType.LeftLittleMeta
    JOINT_TYPE_MAP["left_pinky_finger_tip"] = MJointType.LeftLittleDistal

IS_FINGER_JOINTS = ["LeftMiddleProximal","LeftMiddleMeta","LeftMiddleDistal", "LeftMiddleTip", "LeftIndexProximal", "LeftIndexMeta","LeftIndexDistal","LeftRingProximal","LeftRingMeta","LeftRingDistal","LeftLittleProximal", "LeftLittleMeta", "LeftLittleDistal", "LeftThumbMid", "LeftThumbMeta", "LeftThumbCarpal"]
IS_FINGER_JOINTS += ["RightMiddleProximal","RightMiddleMeta","RightMiddleDistal", "RightMiddleTip", "RightIndexProximal", "RightIndexMeta","RightIndexDistal","RightRingProximal","RightRingMeta","RightRingDistal","RightLittleProximal", "RightLittleMeta", "RightLittleDistal", "RightThumbMid", "RightThumbMeta", "RightThumbCarpal"]

def NewMJoint(id, jtype : MJointType, offset : MVector3, rotation : MQuaternion, parentID, channels):
    j = MJoint(id, jtype, offset, rotation)
    j.Parent = parentID
    j.Channels = channels
    return j
identityQ = MQuaternion(0,0,0,1)
defaultJointChannels = [MChannel.WRotation, MChannel.XRotation, MChannel.YRotation, MChannel.ZRotation]
defaultRootChannels = [MChannel.XOffset, MChannel.YOffset, MChannel.ZOffset, MChannel.WRotation, MChannel.XRotation, MChannel.YRotation, MChannel.ZRotation]
zeroChannels = []

DEFAULT_JOINTS = [
            # 7 joints along the spine (6 animated, 39 channels)
            NewMJoint("PelvisCenter", MJointType.PelvisCentre, MVector3(0,0,0), identityQ, None, defaultRootChannels),
            NewMJoint("S1L5Joint", MJointType.S1L5Joint, MVector3(0,0.18,0), identityQ, "PelvisCenter", defaultRootChannels),
            NewMJoint("T12L1Joint", MJointType.T12L1Joint, MVector3(0,0.15,0), identityQ, "S1L5Joint", defaultRootChannels),
            NewMJoint("T1T2Joint", MJointType.T1T2Joint, MVector3(0,0.43,0), identityQ, "T12L1Joint", defaultRootChannels),
            NewMJoint("C4C5Joint", MJointType.C4C5Joint, MVector3(0,0.11,0), identityQ, "T1T2Joint", defaultRootChannels),
            NewMJoint("HeadJoint", MJointType.HeadJoint, MVector3(0,0.13,0), identityQ, "C4C5Joint", defaultJointChannels),
            NewMJoint("HeadTip", MJointType.HeadTip, MVector3(0,0.16,0), identityQ, "HeadJoint", zeroChannels),

            # Left Arm: 3 joints (3 animated, 15 channels)
            NewMJoint("LeftShoulder", MJointType.LeftShoulder, MVector3(-1, 0, 0), MQuaternion(0, 0, 0.7071055, 0.7071055), "T1T2Joint", defaultRootChannels),
            NewMJoint("LeftElbow", MJointType.LeftElbow, MVector3(0, 1, 0), identityQ, "LeftShoulder", defaultJointChannels),
            NewMJoint("LeftWrist", MJointType.LeftWrist, MVector3(0, 1, 0), identityQ, "LeftElbow", defaultJointChannels),

            #left hand: 20 joints (16 animated, 75 channels)
            NewMJoint("LeftMiddleProximal", MJointType.LeftMiddleProximal, MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftMiddleMeta", MJointType.LeftMiddleMeta, MVector3(0, 1, 0), identityQ, "LeftMiddleProximal", defaultJointChannels),
            NewMJoint("LeftMiddleDistal", MJointType.LeftMiddleDistal, MVector3(0, 1, 0), identityQ, "LeftMiddleMeta", defaultJointChannels),
            NewMJoint("LeftMiddleTip", MJointType.LeftMiddleTip, MVector3(0, 0.03, 0), identityQ, "LeftMiddleDistal", zeroChannels),

            NewMJoint("LeftIndexProximal", MJointType.LeftIndexProximal, MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftIndexMeta", MJointType.LeftIndexMeta, MVector3(0, 1, 0), identityQ, "LeftIndexProximal", defaultJointChannels),
            NewMJoint("LeftIndexDistal", MJointType.LeftIndexDistal, MVector3(0, 1, 0), identityQ, "LeftIndexMeta", defaultJointChannels),
            NewMJoint("LeftIndexTip", MJointType.LeftIndexTip, MVector3(0, 0.03, 0), identityQ, "LeftIndexDistal", zeroChannels),

            NewMJoint("LeftRingProximal", MJointType.LeftRingProximal, MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftRingMeta", MJointType.LeftRingMeta, MVector3(0, 1, 0), identityQ, "LeftRingProximal", defaultJointChannels),
            NewMJoint("LeftRingDistal", MJointType.LeftRingDistal, MVector3(0, 1, 0), identityQ, "LeftRingMeta", defaultJointChannels),
            NewMJoint("LeftRingTip", MJointType.LeftRingTip, MVector3(0, 0.03, 0), identityQ, "LeftRingDistal", zeroChannels),

            NewMJoint("LeftLittleProximal", MJointType.LeftLittleProximal, MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftLittleMeta", MJointType.LeftLittleMeta, MVector3(0, 1, 0), identityQ, "LeftLittleProximal", defaultJointChannels),
            NewMJoint("LeftLittleDistal", MJointType.LeftLittleDistal, MVector3(0, 1, 0), identityQ, "LeftLittleMeta", defaultJointChannels),
            NewMJoint("LeftLittleTip", MJointType.LeftLittleTip, MVector3(0, 0.03, 0), identityQ, "LeftLittleDistal", zeroChannels),
            
            NewMJoint("LeftThumbMid", MJointType.LeftThumbMid, MVector3(0, 1, 0), MQuaternion(0.3826834559440613, 0, 0,  0.9238795042037964), "LeftWrist",defaultRootChannels),
            NewMJoint("LeftThumbMeta", MJointType.LeftThumbMeta, MVector3(0, 1, 0), identityQ, "LeftThumbMid", defaultJointChannels),
            NewMJoint("LeftThumbCarpal", MJointType.LeftThumbCarpal, MVector3(0, 1, 0), identityQ, "LeftThumbMeta", defaultJointChannels),
            NewMJoint("LeftThumbTip", MJointType.LeftThumbTip, MVector3(0, 0.03, 0), identityQ, "LeftThumbCarpal", zeroChannels),


            # Right Arm: 3 joints (3 animated, 15 channels)
            NewMJoint("RightShoulder", MJointType.RightShoulder, MVector3(1, 0, 0), MQuaternion(0, 0, -0.7071055, 0.7071055), "T1T2Joint",defaultRootChannels),
            NewMJoint("RightElbow", MJointType.RightElbow, MVector3(0, 1, 0), identityQ, "RightShoulder", defaultJointChannels),
            NewMJoint("RightWrist", MJointType.RightWrist, MVector3(0, 1, 0), identityQ, "RightElbow", defaultJointChannels),

            # left hand: 20 joints (16 animated, 75 channels)
            NewMJoint("RightMiddleProximal", MJointType.RightMiddleProximal, MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightMiddleMeta", MJointType.RightMiddleMeta, MVector3(0, 1, 0), identityQ, "RightMiddleProximal", defaultJointChannels),
            NewMJoint("RightMiddleDistal", MJointType.RightMiddleDistal, MVector3(0, 1, 0), identityQ, "RightMiddleMeta", defaultJointChannels),
            NewMJoint("RightMiddleTip", MJointType.RightMiddleTip, MVector3(0, 0.03, 0), identityQ, "RightMiddleDistal", zeroChannels),

            NewMJoint("RightIndexProximal", MJointType.RightIndexProximal, MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightIndexMeta", MJointType.RightIndexMeta, MVector3(0, 1, 0), identityQ, "RightIndexProximal", defaultJointChannels),
            NewMJoint("RightIndexDistal", MJointType.RightIndexDistal, MVector3(0, 1, 0), identityQ, "RightIndexMeta", defaultJointChannels),
            NewMJoint("RightIndexTip", MJointType.RightIndexTip, MVector3(0, 0.03, 0), identityQ, "RightIndexDistal", zeroChannels),

            NewMJoint("RightRingProximal", MJointType.RightRingProximal, MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightRingMeta", MJointType.RightRingMeta, MVector3(0, 1, 0), identityQ, "RightRingProximal", defaultJointChannels),
            NewMJoint("RightRingDistal", MJointType.RightRingDistal, MVector3(0, 1, 0), identityQ, "RightRingMeta", defaultJointChannels),
            NewMJoint("RightRingTip", MJointType.RightRingTip, MVector3(0, 0.03, 0), identityQ, "RightRingDistal", zeroChannels),

            NewMJoint("RightLittleProximal", MJointType.RightLittleProximal, MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightLittleMeta", MJointType.RightLittleMeta, MVector3(0, 1, 0), identityQ, "RightLittleProximal", defaultJointChannels),
            NewMJoint("RightLittleDistal", MJointType.RightLittleDistal, MVector3(0, 1, 0), identityQ, "RightLittleMeta", defaultJointChannels),
            NewMJoint("RightLittleTip", MJointType.RightLittleTip, MVector3(0, 0.03, 0), identityQ, "RightLittleDistal", zeroChannels),

            NewMJoint("RightThumbMid", MJointType.RightThumbMid, MVector3(0, 1, 0), MQuaternion(0.3826834559440613, 0, 0,  0.9238795042037964), "RightWrist",defaultRootChannels),
            NewMJoint("RightThumbMeta", MJointType.RightThumbMeta, MVector3(0, 1, 0), identityQ, "RightThumbMid", defaultJointChannels),
            NewMJoint("RightThumbCarpal", MJointType.RightThumbCarpal, MVector3(0, 1, 0), identityQ, "RightThumbMeta", defaultJointChannels),
            NewMJoint("RightThumbTip", MJointType.RightThumbTip, MVector3(0, 0.03, 0), identityQ, "RightThumbCarpal", zeroChannels),

            # Left leg: 5 joints (4 animated, 16 channels)
            NewMJoint("LeftHip", MJointType.LeftHip, MVector3(-1, 0, 0), MQuaternion(0, 0, -1, 0), "PelvisCenter", defaultJointChannels),
            NewMJoint("LeftKnee", MJointType.LeftKnee, MVector3(0, 1, 0), identityQ, "LeftHip", defaultJointChannels),
            NewMJoint("LeftAnkle", MJointType.LeftAnkle, MVector3(0, 1, 0), MQuaternion(0.53730, 0, 0,0.84339), "LeftKnee", defaultJointChannels),
            NewMJoint("LeftBall", MJointType.LeftBall, MVector3(0, 1, 0), MQuaternion(0.2164396196603775, 0, 0,  0.9762960076332092), "LeftAnkle", defaultJointChannels),
            NewMJoint("LeftBallTip", MJointType.LeftBallTip, MVector3(0, 0.08, 0), identityQ, "LeftBall", zeroChannels),

            # Right leg: 5 joints (4 animated, 16 channels)
            NewMJoint("RightHip", MJointType.RightHip, MVector3(1, 0, 0), MQuaternion(0, 0, 1, 0), "PelvisCenter", defaultJointChannels),
            NewMJoint("RightKnee", MJointType.RightKnee, MVector3(0, 1, 0), identityQ, "RightHip", defaultJointChannels),
            NewMJoint("RightAnkle", MJointType.RightAnkle, MVector3(0, 1, 0), MQuaternion(0.53730, 0, 0, 0.84339), "RightKnee", defaultJointChannels),
            NewMJoint("RightBall", MJointType.RightBall, MVector3(0, 1, 0), MQuaternion(0.2164396196603775, 0, 0,  0.9762960076332092), "RightAnkle", defaultJointChannels),
            NewMJoint("RightBallTip", MJointType.RightBallTip, MVector3(0, 0.08, 0), identityQ, "RightBall", zeroChannels)
        ]