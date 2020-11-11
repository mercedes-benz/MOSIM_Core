
import os
import time
import numpy as np
import json
import math
from copy import copy
from datetime import datetime
from transformations import quaternion_inverse, quaternion_multiply, quaternion_from_matrix, quaternion_from_euler, quaternion_matrix, euler_from_quaternion
from MMIPython.core.interfaces.motion_model_interface import MotionModelInterface
from MMIStandard.core.ttypes import MBoolResponse
from MMIStandard.mmu.ttypes import MInstruction, MMUDescription, MDependency, MDependencyType
from MMIStandard.scene.ttypes import MAvatarPosture, MSceneManipulation, MChannel
from MMIStandard.mmu.ttypes import MSimulationResult, MSimulationState, MSimulationEvent, MConstraint, MTranslationConstraint, MRotationConstraint, MInterval3, MInterval, MJointConstraint, MGeometryConstraint, MTranslationConstraintType#, MEndeffectorConstraint
from MMIStandard.scene.ttypes import MJointType, MJoint, MEndeffectorType
from MMIStandard.math.ttypes import MVector3, MQuaternion
from MMIStandard.services import MRetargetingService
from MMIPython.extensions.MAvatarPostureExtensions import JSON2MAvatarPosture
#from MMIStandard.constants import MSimulationEvent_End
#from MMIStandard.mmu.ttypes import MSimulationEvent
#from MMIPython.core.skeleton import ISDescription
from .morphable_graph_state_machine import MorphableGraphStateMachine, DEFAULT_CONFIG, MotionStateGraphLoader
from .anim_utils.retargeting.analytical import align_axis
from .anim_utils.animation_data.joint_constraints import JointConstraint
from .anim_utils.animation_data import MotionVector
from .constants import JOINT_TYPE_MAP, IS_FINGER_JOINTS, DEFAULT_JOINTS
from .utils import normalize, array_from_mosim_t, array_from_mosim_q, array_from_mosim_r, map_joint_to_skeleton, get_inverted_joint_map, set_static_joints, get_chain_end_joint, get_long_chain_end_joint, generate_constraint_desc, generate_action_desc


CONFIG_FILE = "mg_config.json"

RIGHT = "Right"
LEFT = "Left"

def load_json_file(path):
    with open(path, "rt") as in_file:
        return json.load(in_file)



def MVector3ToArray(v):
    return np.array([-v.X, v.Y, v.Z])


def ArrayToMVector3(a):
    vec = MVector3(-a[0], a[1], a[2])
    return vec


def MQuaternionToArray(q):
    return np.array([-q.W, -q.X, q.Y, q.Z])


def ArrayToMQuaternion(a):
    return MQuaternion(-a[1], a[2], a[3], -a[0])


def quat_frame_to_m_avatar_posture(avatar_id, skeleton, frame, zero_posture, index_to_joint_map, scale_factor):
    """ Skeleton needs to have a T-Pose as reference pose
    Parameters:
        avatar_id : str
        skeleton : Skeleton
        zero_frame : np.array
        zero_posture : MAvatarPosture
        index_to_joint_map : dict
        scale_factor : float
    """
    posture = MAvatarPosture()
    posture.AvatarID = avatar_id
    posture.Joints = []
    for idx, zero_joint in enumerate(zero_posture.Joints):
        if idx in index_to_joint_map and index_to_joint_map[idx] in skeleton.nodes:
            name = index_to_joint_map[idx]
            m = skeleton.nodes[name].get_global_matrix(frame)
            pos = m[:3, 3]*scale_factor
            position = ArrayToMVector3(pos)
            q = quaternion_from_matrix(m)
            quat = ArrayToMQuaternion(q)
            joint = MJoint(zero_joint.ID, zero_joint.Type, position, quat, zero_joint.Channels, zero_joint.Parent)
            posture.Joints.append(joint)
        else:
            position = zero_joint.Position #ArrayToMVector3([0,0,0])
            quat = zero_joint.Rotation #ArrayToMQuaternion([1,0,0,0])
            joint = MJoint(zero_joint.ID, MJointType.Undefined, position, quat, zero_joint.Channels, zero_joint.Parent)
            posture.Joints.append(joint)
    return posture


ROOT_COS_MAP = {
                "y": [
                    0.99945471815356,
                    0.033019181706821506,
                    4.653201266875375e-11
                ],
                "x": [
                    -4.6531988683287224e-11,
                    -7.688567263622184e-13,
                    -1.0
                ]
            }
            
def get_intermediate_skeleton_channel_indices(joint_filter):
    joint_list = DEFAULT_JOINTS
    channel_offset = 0
    channel_indices = []
    for joint_idx, joint in enumerate(joint_list):
        n_joint_channels = len(joint.Channels)
        if joint.ID in joint_filter:
            channel_indices.append((channel_offset, n_joint_channels))
        channel_offset += n_joint_channels
    return channel_indices


def position_from_json(v):
    x = 0
    if "X" in v:
        x = v["X"]
    y = 0
    if "Y" in v:
        y = v["Y"]
    z = 0
    if "Z" in v:
        z = v["Z"]
    return MVector3(x, y, z)

def rotation_from_json(v):
    x = 0
    if "X" in v:
        x = v["X"]
    y = 0
    if "Y" in v:
        y = v["Y"]
    z = 0
    if "Z" in v:
        z = v["Z"]
    w = 1
    if "W" in v:
        w = v["W"]
    return MQuaternion(x, y, z, w)
    

def get_joint_type(index):
    if index in MJointType._VALUES_TO_NAMES:
        return getattr(MJointType, MJointType._VALUES_TO_NAMES[index])
    else:
        return MJointType.Undefined


def load_joint_from_json(j_data, ref_joint):
    print(j_data)
    joint_id = j_data["ID"]
    joint_type = get_joint_type(j_data["Type"])
    position = position_from_json(j_data["Position"])
    rotation = rotation_from_json(j_data["Rotation"])
    return MJoint(joint_id, joint_type, position, rotation, ref_joint.Channels, ref_joint.Parent)


def get_ref_joint(ref_avatar, j_type_idx):
    joint_type = get_joint_type(j_type_idx)
    for j in ref_avatar.Joints:
        if j.Type == joint_type:
            return j
    return None


def load_zero_posture_from_json(avatar_id, filename, ref_avatar):
    posture = MAvatarPosture()
    posture.AvatarID = avatar_id
    posture.Joints = []
    with open(filename, "rt") as in_file:
        data = json.load(in_file)
    if "Joints" not in data:
        return posture
    for j_data in data["Joints"]:
        if "ID" in j_data and "Type" in j_data:
            ref_joint = get_ref_joint(ref_avatar, j_data["Type"])
            if ref_joint is not None:
                joint = load_joint_from_json(j_data, ref_joint)
                posture.Joints.append(joint)
    return posture


def create_m_constraint_old(side, target_transform, set_rotation=False):
    """ Create MConstraint to be applied after the MMU is finished
    """
    if side == LEFT:
        joint_type = MEndeffectorType.LeftHand
    else:
        joint_type = MEndeffectorType.RightHand
    name = "mg_reach_constraint_"+side + str(datetime.now().strftime("%d%m%y_%H%M%S"))
    p_channels = [MChannel.XOffset, MChannel.YOffset, MChannel.ZOffset]
    p_values = [target_transform.Position.X, target_transform.Position.Y, target_transform.Position.Z]
    p = MPositionConstraint(p_channels, p_values)
    if set_rotation:
        r_channels = [MChannel.WRotation, MChannel.XRotation, MChannel.YRotation, MChannel.ZRotation]
        r_values = [target_transform.Rotation.W, target_transform.Rotation.X, target_transform.Rotation.Y, target_transform.Rotation.Z]
        #r_values [1,0,0,0]
        r = MRotationConstraint(r_channels, r_channels)
    else:
        r = None
    ec = MEndeffectorConstraint(joint_type, p, r)
    constraint = MConstraint(ID=name, EndeffectorConstraint=ec)
    return constraint


def create_m_constraint(side, transform, set_rotation=False):
    """ Create MConstraint to be applied after the MMU is finished
    """
    if side == LEFT:
        joint_type = MEndeffectorType.LeftHand
    else:
        joint_type = MEndeffectorType.RightHand
    name = "mg_reach_constraint_"+side + str(datetime.now().strftime("%d%m%y_%H%M%S"))
    p = [transform.Position.X, transform.Position.Y, transform.Position.Z]
    t = MTranslationConstraint(MTranslationConstraintType.BOX,  MInterval3(MInterval(p[0],p[0]), MInterval(p[1],p[1]), MInterval(p[2],p[2])))
    if set_rotation:
        q = [transform.Rotation.W, transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z]
        e = euler_from_quaternion(q)
        r = MRotationConstraint(MInterval3(MInterval(e[0],e[0]), MInterval(e[1],e[1]), MInterval(e[2],e[2])))
    else:
        r = None
    geometry_cosntraint = MGeometryConstraint(None, TranslationConstraint=t, RotationConstraint=r)
    joint_constraint = MJointConstraint(JointType=joint_type, GeometryConstraint=geometry_cosntraint)
    constraint = MConstraint(ID=name, JointConstraint=joint_constraint)
    return constraint


class MMUReachMG(MotionModelInterface):

    def Initialize(self, avatarDescription, properties):
        """
        The initializationof the MMU
        
        Parameters
        ----------
        avatarDescription : MAvatarDescription
            The intermediate avatar description
        properties : dict
            Properties for the actual MMU
            
        Returns
        --------
        MBoolResponse

        """
        print("DEBUG MMUReachMG Initialize")
        self.static_offset =  [-3.2, 0.0, 7.39] # workaround for root position offset
        self.avatarDesc = avatarDescription
        config_filename = os.path.join(os.path.dirname(__file__), CONFIG_FILE)
        config = load_json_file(config_filename)
        self.target_to_mmu_scale = config["target_to_mmu_scale"]
        self.right_reach_action_name = config["right_reach_action_name"]# "placeRight"
        self.left_reach_action_name = config["left_reach_action_name"]# "placeLeft"
        self.two_hand_reach_action_name = config["two_hand_reach_action_name"]
        self.reach_keyframe_label = config["reach_keyframe_label"]#"contact0"#
        self.use_long_chain = config["use_long_chain_for_secondary_ik"]
        self.look_at_secondary_targets = config["look_at_secondary_targets"]
        activate_grounding = False
        if "activate_grounding" in config:
            activate_grounding= config["activate_grounding"]
        self.set_orientation_constraints = False
        if "set_orientation_constraints" in config:
            self.set_orientation_constraints = config["set_orientation_constraints"]
        use_retargeting_config = False
        if "use_retargeting_config" in config:
            self.use_retargeting_config = config["use_retargeting_config"]
        self.retargeting_config_file = None
        if "retargeting_config_file" in config:
            self.retargeting_config_file = os.path.join(os.path.dirname(__file__),  config["retargeting_config_file"])
        self.look_at_constraints = True
        file_path = config["model_file"]
        loader = MotionStateGraphLoader()
        loader.use_all_joints = config["use_all_joints"]# = set animated joints to all
        loader.set_data_source(file_path[:-4])
        graph = loader.build()
        set_static_joints(graph.skeleton)
        start_node = None
        self.hold_frame = True
        self.mg_state_machine = MorphableGraphStateMachine(graph, start_node, activate_grounding=activate_grounding)
        self.frame_time = self.mg_state_machine.skeleton.frame_time
        self.skeleton = self.mg_state_machine.skeleton
        #self.mg_state_machine.set_config(config)
        self.skeleton.reference_frame[:3] = [0,0,0]
        self.mg_state_machine.planner.settings.debug_export = False
        self.mg_state_machine.planner.settings.orient_spine = config["orient_spine"]
        print("start registering with retargeting service")
        
        inverted_type_map = get_inverted_joint_map(JOINT_TYPE_MAP)
        self.index_to_joint_map = dict()
        for idx, joint in enumerate(self.avatarDesc.ZeroPosture.Joints):
            if joint.Type in inverted_type_map:
                default_name = inverted_type_map[joint.Type]
                if default_name in self.skeleton.skeleton_model["joints"]:
                    name = self.skeleton.skeleton_model["joints"][default_name]
                    self.index_to_joint_map[idx] = name
                    print("add", idx, name)
                else:
                    print("dont add", default_name)
        self.retargeting_id = self.avatarDesc.AvatarID
        self.retargeting = self.service_access.GetRetargetingService()
        try:
            if self.retargeting_config_file is not None and self.use_retargeting_config:
                print("init from file")
                self.zero_posture = load_zero_posture_from_json(self.retargeting_id, self.retargeting_config_file,self.avatarDesc.ZeroPosture)
            else:
                self.zero_posture = quat_frame_to_m_avatar_posture(self.retargeting_id, self.skeleton, self.skeleton.reference_frame, self.avatarDesc.ZeroPosture, self.index_to_joint_map, 1.0/self.target_to_mmu_scale)
            rp = self.retargeting.SetupRetargeting(self.zero_posture)
        except Exception as e:
            print("Error: could not set up retargeting", e.args)
            pass
            

        self.mg_state_machine.play = True
        self.mg_state_machine.update(self.frame_time)
        print("done")
        self.finished_event = None
        self.finger_channel_indices = get_intermediate_skeleton_channel_indices(IS_FINGER_JOINTS)
        self.prev_hand = None
        self.record_frames = False
        self.frames = []
        self.final_constraints = []

        return MBoolResponse(True)

    def is_finished(self):
        return self.mg_state_machine.is_paused() and self.mg_state_machine.reached_secondary_ik_target()

 
    def AssignInstruction(self, instruction, simulationState):
        """
        Method sets the motion instruction
        
        Parameters
        ---------
        instruction : MInstruction
            The motion to assign
        simulationState : MSimulationState
            The current simulation state
            
        Returns
        --------
        bool
        """
        print("DEBUG MMUReachMG AssignInstruction")
        success = False
        log_data = []
        if "TargetID" in instruction.Properties and "Hand" in instruction.Properties:
            print("get targt id", instruction.Properties["TargetID"])
            target = self.scene_access.GetTransformByID(instruction.Properties["TargetID"])
            print(target.Position)
            side = instruction.Properties["Hand"]
            succcess = self.create_one_handed_reach(side, target, simulationState)
        elif "LeftTargetID" in instruction.Properties and "RightTargetID" in instruction.Properties:
            left_target = self.scene_access.GetTransformByID(instruction.Properties["LeftTargetID"])
            right_target = self.scene_access.GetTransformByID(instruction.Properties["RightTargetID"])
            succcess = self.create_two_handed_reach(left_target, right_target, simulationState)
        else:
            log_data = ["Required parameters not defined"]
        if success:
            self.finished_event = MSimulationEvent(instruction.Name, MSimulationEvent_End, instruction.ID)
        resp = MBoolResponse(success)
        resp.LogData = log_data
        return resp

    def create_one_handed_reach(self, side, target_transform, simulationState):
        success = False
        if not self.mg_state_machine.is_planner_active() or self.is_finished():
            self.final_constraints = []
            self.prev_hand = side
            self.mg_state_machine.remove_secondary_ik_target() # disable target
            action_desc = self.generate_one_handed_reach_action_desc(side, target_transform)
            self.final_constraints += [create_m_constraint(side, target_transform, True)]
            self.assign_instruction_to_state_machine(simulationState, action_desc)
            success = True
        elif side != self.prev_hand:
            self.set_secondary_ik_target(side, target_transform)
            success = True
        else:
            if side == RIGHT:
                other_side = LEFT
            else:
                other_side = RIGHT
            self.set_secondary_ik_target(other_side, target_transform)
        return success

    def create_two_handed_reach(self, left_target, right_target, simulationState):
        success = False
        if not self.mg_state_machine.is_planner_active() or self.is_finished():
            self.final_constraints = []
            self.prev_hand = "both"
            self.mg_state_machine.remove_secondary_ik_target() # disable target
            action_desc = self.generate_two_handed_reach_action_desc(left_target, right_target)
            self.final_constraints += [create_m_constraint("left", left_target, True)]
            self.final_constraints += [create_m_constraint("right", right_target, True)]
            self.assign_instruction_to_state_machine(simulationState, action_desc)
            success = True
        return success

    def assign_instruction_to_state_machine(self, simulationState, action_desc):
        self.mg_state_machine.unpause()
        self.reset_state_machine()
        self.mg_state_machine.update(self.frame_time)
        self.set_pose(simulationState)
        self.mg_state_machine.update_idle_foot_grounding_constraints()
        self.mg_state_machine.enqueue_states([action_desc], self.frame_time, refresh=True)


    def set_secondary_ik_target(self, side, target_transform):
        print("set secondary ik target")
        position = array_from_mosim_t(target_transform.Position)*self.target_to_mmu_scale
        orientation = array_from_mosim_q(target_transform.Rotation)
        joint_name = map_joint_to_skeleton(self.skeleton, side)
        if self.use_long_chain:
            chain_end_joint = get_long_chain_end_joint(self.skeleton)
        else:
            chain_end_joint = get_chain_end_joint(self.skeleton, side)
        self.mg_state_machine.set_secondary_ik_target(joint_name, position, orientation, chain_end_joint, self.look_at_secondary_targets)
        if self.use_long_chain:
            other_side = RIGHT
            if other_side == side:
                other_side = LEFT
            self.mg_state_machine.secondary_ik_target.preserve_joint_name = map_joint_to_skeleton(self.skeleton, other_side)
            self.mg_state_machine.secondary_ik_target.preserve_chain_end = get_chain_end_joint(self.skeleton, other_side)
        
        self.final_constraints += [create_m_constraint(side, target_transform, True)]

    def set_pose(self, simulationState):
        pose = simulationState.Current.PostureData
        # set rotation
        rot = [-pose[3], -pose[4], pose[5], pose[6]]
        rot = normalize(rot)
        m = quaternion_matrix(rot)[:3, :3]
        local_root_axes = copy(ROOT_COS_MAP)
        target_x_vec = np.dot(m, [1,0,0])
        twist_q, axes = align_axis(local_root_axes, "x", target_x_vec) 
        twist_q = normalize(twist_q)
        swing_q = self.skeleton.reference_frame[3:7]
        q = quaternion_multiply(twist_q, swing_q)
        q = normalize(q)
        self.mg_state_machine.set_global_orientation(q)

        # set position
        target_pose = self.retargeting.RetargetToTarget(simulationState.Current)
        start_pos = array_from_mosim_t(target_pose.Joints[0].Position)*self.target_to_mmu_scale
        start_pos[1] = 0

        #start_pos += self.static_offset
        twist_m = quaternion_matrix(twist_q)[:3,:3]
        start_pos += np.dot(twist_m, self.static_offset)
        print("set start position", start_pos, target_pose.Joints[0].Position)
        self.mg_state_machine.set_global_position(start_pos)
        self.mg_state_machine.update(self.frame_time)

    def generate_one_handed_reach_action_desc(self, side, target_transform):
        if side == "Right":
            name = self.right_reach_action_name
        else:
            name = self.left_reach_action_name
        joint = map_joint_to_skeleton(self.skeleton, side)
        assert joint is not None, "Error: joint could not be assigned"
        constraint_desc = generate_constraint_desc(joint, self.reach_keyframe_label, target_transform, self.set_orientation_constraints, self.hold_frame, self.look_at_constraints, self.target_to_mmu_scale)
        action_desc = generate_action_desc(name, [constraint_desc], self.look_at_constraints)
        return action_desc
    
    def generate_two_handed_reach_action_desc(self, left_target, right_target):
        name = self.two_hand_reach_action_name
        left_joint = map_joint_to_skeleton(self.skeleton, "Left")
        right_joint = map_joint_to_skeleton(self.skeleton, "Right")
        left_c_desc = generate_constraint_desc(left_joint, self.reach_keyframe_label, left_target, self.set_orientation_constraints, self.hold_frame, False, self.target_to_mmu_scale)
        right_c_desc = generate_constraint_desc(right_joint, self.reach_keyframe_label, right_target, self.set_orientation_constraints, self.hold_frame, False, self.target_to_mmu_scale)
        action_desc =  action_desc = generate_action_desc(name, [left_c_desc, right_c_desc], False)
        return action_desc

    def reset_state_machine(self):
        self.mg_state_machine.reset()

    def DoStep(self, dtime, simulationState : MSimulationState):
        """
        Do step routine updates the computation within the MMU
        
        Parameters
        ---------
        time : double
            The delta time
        simulationState : MSimulationState
            The current simulation state
            
        Returns
        ------
        SimulationResult
            The computed result of the do step routine

        """
        #print("DEBUG MMUReachMG DoStep")
        if not self.mg_state_machine.is_idle():
            frame = self.mg_state_machine.get_current_frame()
            if self.record_frames: self.frames.append(frame)
            self.mg_state_machine.update(dtime)

            posture = quat_frame_to_m_avatar_posture(self.retargeting_id, self.skeleton, frame, self.avatarDesc.ZeroPosture, self.index_to_joint_map, 1.0/self.target_to_mmu_scale)
            postureVals = self.retargeting.RetargetToIntermediate(posture)
            postureVals.AvatarID = self.avatarDesc.AvatarID
            #print("hip_pos",frame[:3], postureVals.PostureData[:7])
            #postureVals.PostureData[0] = -frame[0] * 1/self.target_to_mmu_scale
            #postureVals.PostureData[2] = frame[2] * 1/self.target_to_mmu_scale * 4
            #postureVals = self.set_finger_channels_to_zero(postureVals, simulationState.Current)
            simres = MSimulationResult(postureVals)
            if simulationState.Events is not None:
                simres.Events = simulationState.Events
            if simulationState.Constraints is not None:
                simres.Constraints = simulationState.Constraints
            
            simres.SceneManipulations = simulationState.SceneManipulations  
            if self.finished_event is not None and self.is_finished():
                simres.Constraints += self.final_constraints
                simres.Events = list()
                print("finished", self.final_constraints)
                simres.Events.append(self.finished_event)
                if self.record_frames:
                    self.export_frames()
                    self.frames = []
        else:
            self.mg_state_machine.update(dtime)
            simres = MSimulationResult(simulationState.Current)
        return simres

    def export_frames(self):
        mv = MotionVector()
        mv.frames = self.frames
        mv.frame_time = self.mg_state_machine.frame_time
        mv.export(self.mg_state_machine.skeleton, "out.bvh")
    
    def set_finger_channels_to_zero(self, postureVals, refPostureVals):
        for idx, n_channels in self.finger_channel_indices:
            if n_channels == 4:
                postureVals.PostureData[idx] = refPostureVals.PostureData[idx]
                postureVals.PostureData[idx+1] = refPostureVals.PostureData[idx+1]
                postureVals.PostureData[idx+2] = refPostureVals.PostureData[idx+2]
                postureVals.PostureData[idx+3] = refPostureVals.PostureData[idx+3]
            elif n_channels == 7:
                postureVals.PostureData[idx+3] = refPostureVals.PostureData[idx+3]
                postureVals.PostureData[idx+4] = refPostureVals.PostureData[idx+4]
                postureVals.PostureData[idx+5] = refPostureVals.PostureData[idx+5]
                postureVals.PostureData[idx+6] = refPostureVals.PostureData[idx+6]
        return postureVals

    def GetBoundaryConstraints(self, instruction):
        """
        Returns constraints which are relevant for the transition

        Parameters
        ---------
        instruction : MInstruction
          The motion to assign
            
        Returns
        ------
        list<MConstraint>
        
        """
        print("DEBUG MMUReachMG GetBoundaryConstraints")
        return []


    def CheckPrerequisites(self, instruction):
        """
        Parameters
        ---------
         - instruction: MInstruction
            The motion to assign

        Returns
        ------
        MBoolResponse
        """
        print("DEBUG MMUReachMG CheckPrerequisites")
        return MBoolResponse(True)


    def Abort(self, instructionId):
        """
        Method forces the termination of a MMU

        Parameters
        ---------
        instructionID: MInstruction
          The id of the instruction
        
        Returns
        --------
        MBoolResponse

        """
        print("DEBUG MMUReachMG Abort")
        return MBoolResponse(True)


    def Dispose(self, parameters):
        """
        A method for disposing the internally used resources
        
         Parameters
        ---------
        parameters: dictionary<str,stra>
          optional parameters
        
          Returns
        --------
        MBoolResponse

        """
        print("DEBUG MMUReachMG Dispose")
        return MBoolResponse(True)


    def CreateCheckpoint(self):
        """
        Creates a Checkpoint
        
         Parameters
        ---------
        parameters: dictionary<str,stra>
          optional parameters
        
            Returns
        -------
        bytearray
            The checkpoint data
        """
        print("DEBUG MMUReachMG CreateCheckpoint")
        return []


    def RestoreCheckpoint(self, data):
        """
        Restores the checkpoint from the data
        
        Parameters
        ----------
        data : object
            The checkpoint data
            
        Returns
        -------
        MBoolResponse
        """
        print("DEBUG MMUReachMG RestoreCheckpoint")
        return MBoolResponse(True)


    def ExecuteFunction(self, name, parameters):
        """
        //Method for executing an arbitrary function (optionally)
        Parameters:
         - name : str
            name of the isntruction
         - parameters : dictionary<str,str> 
            optional parameters

        """
        print("DEBUG MMUReachMG ExecuteFunction")
        return
