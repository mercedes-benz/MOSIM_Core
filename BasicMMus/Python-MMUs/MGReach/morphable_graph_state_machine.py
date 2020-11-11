
import os
import glob
import copy
import threading
import time
import sys
sys.path.append(os.path.dirname(__file__))
import numpy as np
from transformations import euler_matrix, quaternion_from_matrix, quaternion_slerp
from anim_utils.animation_data.motion_state import MotionState, MotionStateInterface
from anim_utils.animation_data.motion_concatenation import align_joint, get_global_node_orientation_vector
from anim_utils.motion_editing import MotionGrounding
from anim_utils.animation_data.motion_vector import MotionVector
from morphablegraphs.motion_generator.algorithm_configuration import DEFAULT_ALGORITHM_CONFIG
from morphablegraphs.motion_model import MotionStateGraphLoader, NODE_TYPE_STANDARD, NODE_TYPE_END, NODE_TYPE_START, NODE_TYPE_IDLE
from morphablegraphs.motion_model.static_motion_primitive import StaticMotionPrimitive
from morphablegraphs.motion_generator.mg_state_planner import MGStatePlanner, get_node_aligning_2d_transform, KeyframeConstraint
from morphablegraphs.motion_generator.mg_state_queue import StateQueueEntry
from morphablegraphs.constraints.constraint_builder import UnityFrameConstraint
from anim_utils.motion_editing.footplant_constraint_generator import FootplantConstraintGenerator, SceneInterface
from .motion_state_machine import MotionStateMachine

DEFAULT_CONFIG = dict()
DEFAULT_CONFIG["algorithm"]  = DEFAULT_ALGORITHM_CONFIG
DEFAULT_CONFIG["algorithm"]["n_random_samples"] = 300
DEFAULT_CONFIG["algorithm"]["n_cluster_search_candidates"] = 4
DEFAULT_CONFIG["algorithm"]["local_optimization_settings"]["max_iterations"] = 1000
DEFAULT_CONFIG["algorithm"]["local_optimization_settings"]["method"] = "L-BFGS-B"
DEFAULT_CONFIG["algorithm"]["local_optimization_mode"] = "none"
DEFAULT_CONFIG["algorithm"]["inverse_kinematics_settings"]["interpolation_window"]= 120
DEFAULT_CONFIG["algorithm"]["inverse_kinematics_settings"]["transition_window"]= 60

DEFAULT_GROUND_HEIGHT = 0

EPS = 0.001
DEFAULT_SECONDARY_TARGET_SPEED = 50


def normalize(v):
    return v/np.linalg.norm(v)


class SecondaryIKTarget(object):
    def __init__(self, skeleton, joint_name, position, orientation, chain_end_joint, speed):
        self.skeleton = skeleton
        self.joint_name = joint_name
        self.position = position
        self.orientation = orientation
        dot = np.sum(self.orientation)
        if dot < 0:
            self.orientation = -self.orientation
        self.chain_end_joint = chain_end_joint        
        self.prev_target_pos = None
        self.joint_indices = []
        self.prev_frame = None
        self.speed = speed
        self.eps = EPS
        self.preserve_joint_name = None
        self.preserve_chain_end = None
        self.rotation_distance = 80.0
        self.lower_arm_distance  = 120.0
        self.max_ik_iter = 30
        self.original_rotation = None
        self.look_at_dir = None
        self.prev_delta = None
        self.prev_weight = 0
        self.weight_increase = 0.05
        
        self.head_joint_name = self.skeleton.skeleton_model["joints"]["head"]
        self.spine_joint_name = self.skeleton.skeleton_model["joints"]["spine_1"]

    def create_preservation_constraint(self, frame):
        global_matrix = self.skeleton.nodes[self.preserve_joint_name].get_global_matrix(frame)
        position = global_matrix[:3,3]
        orientation = normalize(quaternion_from_matrix(global_matrix))
        return KeyframeConstraint(0, self.preserve_joint_name, position, orientation, self.preserve_chain_end)

    def prepare_joint_parameter_indices(self):
        self.joint_indices = []
        joint_name = self.joint_name
        while joint_name != self.chain_end_joint:
            idx = self.skeleton.nodes[joint_name].quaternion_frame_index * 4 + 3
            joint_name = self.skeleton.nodes[joint_name].parent.node_name
            self.joint_indices += [idx, idx+1, idx+2, idx+3]
    
    def copy_frame_parameters(self, frame):
        frame[self.joint_indices] = self.prev_frame[self.joint_indices]
        return frame

    def get_delta(self):
        return self.position - self.prev_target_pos

    def create_constraint(self, frame):
        global_matrix = self.skeleton.nodes[self.joint_name].get_global_matrix(frame)
        if self.prev_target_pos is None:
            self.prev_target_pos = global_matrix[:3,3]
            delta = self.get_delta()
            self.distance = np.linalg.norm(delta)
            self.max_distance = self.distance
        else:
            delta = self.get_delta()
            self.distance = np.linalg.norm(delta)
        if self.distance > self.eps:
            delta /= self.distance
            delta *= min(self.distance, self.speed)
            new_target_pos = self.prev_target_pos+delta
            new_q = None
            if self.distance < self.rotation_distance:
                if self.original_rotation is None:
                    self.original_rotation = normalize(quaternion_from_matrix(global_matrix))
                target_q = self.orientation
                weight = (1.0-(self.distance/min(self.rotation_distance, self.max_distance)))
                new_q = quaternion_slerp(self.original_rotation, target_q, weight)
                #print("w",new_q, weight, self.original_rotation, target_q)
            c = KeyframeConstraint(0, self.joint_name, new_target_pos, new_q)
            self.prev_target_pos = new_target_pos
        else:
            c = KeyframeConstraint(0, self.joint_name, self.position, self.orientation)
        return c

    def reach(self, frame):
        c = self.create_constraint(frame)
        weight = (1.0-(self.distance/self.max_distance))
        if self.preserve_joint_name is not None:
            preserve_constraint = self.create_preservation_constraint(frame)
        if self.prev_frame is not None:
            frame = self.copy_frame_parameters(frame)
        frame = self.skeleton.reach_target_position(frame, c, max_iter=self.max_ik_iter, chain_end_joint=self.chain_end_joint)
        if self.look_at_dir is not None:
            new_frame = self.skeleton.look_at(np.array(frame), self.head_joint_name, self.position, 0.0001, 2, self.look_at_dir, self.spine_joint_name)
            frame = self.slerp_frame(self.head_joint_name, frame, new_frame, weight)
        if self.preserve_joint_name:
            frame = self.skeleton.reach_target_position(frame, preserve_constraint, max_iter=self.max_ik_iter, chain_end_joint=self.preserve_chain_end)
        self.prev_frame = frame
        return frame

    def reach2(self, frame):
        pos = self.skeleton.nodes[self.joint_name].get_global_position(frame)
        if self.prev_target_pos is None:
            self.prev_target_pos = pos
            delta = self.get_delta()
            self.distance = np.linalg.norm(delta)
            self.max_distance = self.distance
            self.direction = delta/self.distance
            ratio = 0.1
            self.prev_weight =0
        else:
            delta = self.get_delta()
            self.distance = np.linalg.norm(delta)
            ratio = 1.0 - min(self.distance/self.max_distance, 0.9)
            self.prev_weight += self.weight_increase# 0.05
        c = KeyframeConstraint(0, self.joint_name, self.position)
        weight = min(self.prev_weight, 1.0)
        lower_arm_weight =min(self.prev_weight+0.05, 1.0) # move lower arm faster
        hand_weight = min(self.prev_weight, 1.0)
        # print("closer", self.distance/self.max_distance,self.distance,self.max_distance)
        if self.preserve_joint_name is not None:
            preserve_constraint = self.create_preservation_constraint(frame)
        if self.prev_frame is not None:
            frame = self.copy_frame_parameters(frame)
        new_frame = self.skeleton.reach_target_position(np.array(frame), c, max_iter=self.max_ik_iter, chain_end_joint=self.chain_end_joint)
        new_frame = self.skeleton.set_joint_orientation(new_frame, self.joint_name, self.orientation)
        weights = [hand_weight, lower_arm_weight, weight]
        frame = self.blend_chain(frame, new_frame, weights)
        #frame = self.slerp_frame(self.joint_name, frame, new_frame, hand_weight)
        if self.look_at_dir is not None:
            new_frame = self.skeleton.look_at(np.array(frame), self.head_joint_name, self.position, 0.0001, 2, self.look_at_dir, self.spine_joint_name)
            frame = self.slerp_frame(self.head_joint_name, frame, new_frame, weight)
        if self.preserve_joint_name:
            frame = self.skeleton.reach_target_position(frame, preserve_constraint, max_iter=self.max_ik_iter, chain_end_joint=self.preserve_chain_end)
        self.prev_frame = frame
        self.prev_delta = delta
        self.prev_target_pos = self.prev_target_pos + self.direction * 0.2
        return frame

    def blend_chain(self, frame, new_frame, weights):
        self.joint_indices = []
        joint_name = self.joint_name
        w_idx = 0
        while joint_name != self.chain_end_joint:
            if w_idx >= len(weights):
                print("Not enough weights", joint_name, w_idx)
                return frame
            idx = self.skeleton.nodes[joint_name].parent.quaternion_frame_index * 4 + 3
            q1 = normalize(frame[idx:idx+4])
            q2 = normalize(new_frame[idx:idx+4])
            frame[idx:idx+4] = normalize(quaternion_slerp(q1, q2, weights[w_idx]))
            #print(joint_name, weights[w_idx], q2)
            joint_name = self.skeleton.nodes[joint_name].parent.node_name
            w_idx += 1
        return frame

    def slerp_frame(self, joint_name, frame1, frame2, weight):
        idx = self.skeleton.nodes[joint_name].parent.quaternion_frame_index * 4 + 3
        q1 = normalize(frame1[idx:idx+4])
        q2 = normalize(frame2[idx:idx+4])
        frame1[idx:idx+4] = normalize(quaternion_slerp(q1, q2, weight))
        return frame1

    def get_distance(self):
        return np.linalg.norm(self.get_delta())
    
    def get_velocity(self):
        if self.prev_delta is not None:
            return np.linalg.norm(self.get_delta()-self.prev_delta)
        else:
            return 1

    def has_reached(self):
        distance = self.get_distance()
        return  distance < self.eps or self.get_velocity() < self.eps or self.prev_weight >= 1.0
    


def rotate_vector_deg(vec, a):
    a = np.radians(a)
    m = euler_matrix(0, a, 0)[:3,:3]
    vec = np.dot(m, vec)
    return vec


class MorphableGraphStateMachine(MotionStateInterface):
    def __init__(self, graph, start_node=None, use_all_joints=False, activate_grounding=False, config=DEFAULT_CONFIG):
        self.play = True
        self._graph = graph
        if start_node is None or start_node not in self._graph.nodes:
            start_node = self._graph.start_node
        self.start_node = start_node
        self.start_node_type = NODE_TYPE_IDLE
        self.frame_time = self._graph.skeleton.frame_time
        self.skeleton = self._graph.skeleton
        self.thread = None
        self.use_all_joints = use_all_joints
        state = self.get_initial_idle_state(self.start_node, use_all_joints)
        self.initial_pose_buffer = state.get_frames()
        self.global_position = self.initial_pose_buffer[0][:3]
        self.global_orientation = self.initial_pose_buffer[0][3:7]
        self.state_machine = MotionStateMachine(self.skeleton, state, self.start_node, self.start_node_type)
        print("start node", self.start_node)
        #self.start_pose = {"position": [0, 0, 0], "orientation": [0, 0, 0]}
        self.speed = 1
        if "speed" in config:
            self.speed = config["speed"]
        self.max_step_length = 80
        self.direction_vector = np.array([-1.0, 0.0, 0.0])
        self.action_constraint = None
        self.target_projection_len = 0
        self.n_joints = len(self.skeleton.animated_joints)
        self.n_max_state_queries = 20
        self.aligning_transform = np.eye(4)
        self.planner = MGStatePlanner(self, self._graph, config)
        self.actions = self.planner.action_definitions
        self.planner.settings.use_all_joints = use_all_joints
        self.state_machine.state.play = True
        self.thread = None
        self.success = True
        self.stop_current_state = False
        self.lock = threading.Lock()

        self.secondary_ik_target = None

        self.target_ground_height = DEFAULT_GROUND_HEIGHT
        self.scene_interface = SceneInterface(self.target_ground_height)
        ik_settings = config["algorithm"]["inverse_kinematics_settings"]
        
        
        self.activate_grounding = activate_grounding
        #add temporary heels
        self.motion_grounding = MotionGrounding(self.skeleton, ik_settings, self.skeleton.skeleton_model, use_analytical_ik=True)

        if self.motion_grounding.initialized:
            footplant_settings = {"window": 20, "tolerance": 1, "constraint_range": 10, "smoothing_constraints_window": 15, "foot_lift_search_window": 20, "contact_tolerance": 0.1, "foot_lift_tolerance":0.1}
            self.foot_constraint_generator = FootplantConstraintGenerator(self.skeleton, footplant_settings, self.scene_interface)
            self.idle_constraints = self.foot_constraint_generator.generate_grounding_constraints(state.mv.frames, 0, self.foot_constraint_generator.contact_joints)
        else:
            self.foot_constraint_generator = None
            self.activate_grounding = False

    def set_graph(self, graph, start_node):
        self.lock.acquire()
        self.stop_current_state = True
        if self.thread is not None:
            print("stop thread")
            self.planner.stop_thread = True
            self.thread.join()
            self.stop_current_state = True
            self.thread = None
        self._graph = graph
        state = self.get_initial_idle_state(self.start_node, self.planner.settings.use_all_joints)
        self.state_machine.reset(state, self.start_node, self.start_node_type)
        self.planner.state_queue.reset()
        self.lock.release()

    def get_initial_idle_state(self, start_node, use_all_joints=False):
        mv = MotionVector(self.skeleton)
        print("node", start_node)
        mv.frames = self._graph.nodes[start_node].sample().get_motion_vector()
        mv.frame_time = self.frame_time
        mv.n_frames = len(mv.frames)
        if use_all_joints:
            animated_joints = self._graph.nodes[start_node].get_animated_joints()
            assert len(animated_joints) == 0, "Number of joints are zero"
            full_frames = np.zeros((len(mv.frames), self.skeleton.reference_frame_length))
            for idx, reduced_frame in enumerate(mv.frames):
                full_frames[idx] = self.skeleton.add_fixed_joint_parameters_to_other_frame(reduced_frame,
                                                                                           animated_joints)
            mv.frames = full_frames
        state = MotionState(mv)
        return state
    
    def set_config(self, config):
        if "activate_grounding" in config:
            self.activate_grounding =config["activate_grounding"]
        self.planner.set_config(config)

    def is_planner_active(self):
        return self.planner.active or len(self.planner.state_queue) > 0

    def update(self, dt):
        """ update current frame and global joint transformation matrices
        """
        if self.play:
            transition = self.state_machine.update(self.speed * dt)
            self.lock.acquire()
            if transition or (len(self.planner.state_queue) > 0 and self.stop_current_state):
                # decide if the state planner should be used based on a given task and the number of states in the queue
                use_state_planner = False
                #self.planner.state_queue.mutex.acquire()
                if self.planner.is_processing or len(self.planner.state_queue) > 0:
                    use_state_planner = True
                #self.planner.state_queue.mutex.release()
                if use_state_planner:
                    # if the state planner should be used wait until a state was generated
                    success = self.wait_for_planner()
                    if not success:
                        print("Warning: transition to idle state due to empty state queue")
                        state_entry = self.planner.state_queue.generate_idle_state(dt, self.state_machine.pose_buffer, False)
                        self.state_machine.set_state_entry(state_entry)
                    self.stop_current_state = False
                else:
                    # otherwise transition to new state without the planner, e.g. to idle state
                    self.transition_to_next_state_controlled()
                    print("WAIT")
            self.lock.release()
            self.update_transformation()
        
    def wait_for_planner(self):
        success = False
        n_queries = 0
        while not success and n_queries < self.n_max_state_queries:
            self.planner.state_queue.mutex.acquire()
            success = self.pop_motion_state_from_queue()
            if not success:
                n_queries += 1
            self.planner.state_queue.mutex.release()
        return success

    def pop_motion_state_from_queue(self):
        if len(self.planner.state_queue) > 0:
            state_entry = self.planner.state_queue.get_first_state()
            self.state_machine.set_state_entry(state_entry)
            self.planner.state_queue.pop_first_state()
            return True
        else:
            return False

    def generate_action_constraints(self, action_desc):
        action_name = action_desc["name"]
        velocity_factor = 1.0
        n_cycles = 0
        upper_body_gesture = None
        constrain_look_at = False
        look_at_constraints = False
        if "locomotionUpperBodyAction" in action_desc:
            upper_body_gesture = dict()
            upper_body_gesture["name"] = action_desc["locomotionUpperBodyAction"]
        elif "upperBodyGesture" in action_desc:
            upper_body_gesture = action_desc["upperBodyGesture"]
        if "velocityFactor" in action_desc:
            velocity_factor = action_desc["velocityFactor"]
        if "nCycles" in action_desc:
            n_cycles = action_desc["nCycles"]
        if "constrainLookAt" in action_desc:
            constrain_look_at = action_desc["constrainLookAt"]
        if "lookAtConstraints" in action_desc:
            look_at_constraints = action_desc["lookAtConstraints"]
        print("enqueue states", action_name)
        frame_constraints, end_direction, body_orientation_targets = self.planner.constraint_builder.extract_constraints_from_dict(action_desc, look_at_constraints)
        out = dict()
        out["action_name"] = action_name
        out["frame_constraints"] = frame_constraints
        out["end_direction"] = end_direction
        out["body_orientation_targets"] = body_orientation_targets
        if "controlPoints" in action_desc:
            out["control_points"] = action_desc["controlPoints"]
        elif "directionAngle" in action_desc and "nSteps" in action_desc and "stepDistance" in action_desc:
            root_dir = get_global_node_orientation_vector(self.skeleton, self.skeleton.aligning_root_node, self.get_current_frame(), self.skeleton.aligning_root_dir)
            root_dir = np.array([root_dir[0], 0, root_dir[1]])
            out["direction"] = rotate_vector_deg(root_dir, action_desc["directionAngle"])
            out["n_steps"] = action_desc["nSteps"]
            out["step_distance"] = action_desc["stepDistance"]
        elif "direction" in action_desc and "nSteps" in action_desc and "stepDistance" in action_desc:
            out["direction"] = action_desc["direction"]
            out["n_steps"] = action_desc["nSteps"]
            out["step_distance"] = action_desc["stepDistance"]
        out["upper_body_gesture"] = upper_body_gesture
        out["velocity_factor"] = velocity_factor
        out["n_cycles"] = n_cycles
        out["constrain_look_at"] = constrain_look_at
        out["look_at_constraints"] = look_at_constraints
        return out


    def enqueue_states(self, action_sequence, dt, refresh=False):
        """ generates states until all control points have been reached
            should to be called by extra thread to asynchronously
        """

        _action_sequence = []
        for action_desc in action_sequence:
            a = self.generate_action_constraints(action_desc)
            _action_sequence.append(a)

        if self.thread is not None:
            print("stop thread")
            self.planner.stop_thread = True
            self.thread.join()
            self.stop_current_state = refresh
            self.thread = None

        self.planner.state_queue.mutex.acquire()
        start_node = self.state_machine.current_node
        start_node_type = self.state_machine.node_type
        self.planner.state_queue.reset()
        self.planner.state_queue.mutex.release()
        self.planner.stop_thread = False
        self.planner.is_processing = True
        if refresh:
            self.lock.acquire()
            self.stop_current_state = True
            self.state_machine.current_node = self.start_node
            self.state_machine.node_type = self.start_node_type
            self.state_machine.state = self.get_initial_idle_state(self.start_node, self.use_all_joints)
            self.state_machine.state.mv.frames = np.array([p for p in self.initial_pose_buffer])
            pose_buffer = [p for p in self.initial_pose_buffer]
            self.lock.release()
        else:
            pose_buffer = [np.array(frame) for frame in self.state.get_frames()[-self.buffer_size:]]

        
        
        method_args = (_action_sequence, start_node, start_node_type, pose_buffer, dt)
        self.thread = threading.Thread(target=self.planner.generate_motion_states_from_action_sequence, name="c", args=method_args)
        self.thread.start()

    def set_aligning_transform(self, current_node, pose_buffer):
        """ uses a random sample of the morphable model to find an aligning transformation to bring constraints into the local coordinate system"""
        sample = self._graph.nodes[current_node].sample(False)
        frames = sample.get_motion_vector()
        m = get_node_aligning_2d_transform(self.skeleton, self.skeleton.aligning_root_node, pose_buffer, frames)
        self.aligning_transform = np.linalg.inv(m)

    def transition_to_next_state_controlled(self):
        current_node, node_type = self.select_next_node(self.state_machine.current_node, 
                                                                    self.state_machine.node_type, 
                                                                    self.target_projection_len)
        self.set_aligning_transform(current_node, self.state_machine.pose_buffer)
        if isinstance(self._graph.nodes[current_node].motion_primitive, StaticMotionPrimitive):
            spline = self._graph.nodes[current_node].sample()
            new_frames = spline.get_motion_vector()
        else:
            mp_constraints = self.planner.constraint_builder.generate_walk_constraints(current_node, 
                                                                                        self.aligning_transform, 
                                                                                        self.direction_vector, 
                                                                                        self.target_projection_len, 
                                                                                        self.state_machine.pose_buffer)
            s = self.planner.mp_generator.generate_constrained_sample(self._graph.nodes[current_node], mp_constraints)
            spline = self._graph.nodes[current_node].back_project(s, use_time_parameters=False)
            new_frames = spline.get_motion_vector()
        
        if self.planner.settings.use_all_joints:
            new_frames = self.planner.complete_frames(current_node, new_frames)
        ignore_rotation = False
        if current_node[1] == "idle" and self.planner.settings.ignore_idle_rotation:
            ignore_rotation = True
        state = self.planner.state_queue.build_state(new_frames,  self.state_machine.pose_buffer, ignore_rotation)
        state.play = self.play

        self.state_machine.current_node = current_node
        self.state_machine.node_type = node_type
        self.state_machine.state = state

    def select_next_node(self, current_node, current_node_type, step_distance):
        next_node_type = self.planner.get_next_node_type(current_node_type, step_distance)
        next_node = self._graph.nodes[current_node].generate_random_transition(next_node_type)
        if next_node is None:
            next_node = self.start_node
            next_node_type = self.start_node_type
        return next_node, next_node_type

    def reset_planner(self):
        print("reset planner")
        self.planner.state_queue.mutex.acquire()
        if self.planner.is_processing:
            self.planner.stop_thread = True
            if self.thread is not None:
                self.thread.stop()
            self.planner.is_processing = False
            #self.current_node = ("walk", "idle")
            state = self.get_initial_idle_state(self.start_node, self.use_all_joints)
            self.state_machine.reset(state, self.start_node, self.start_node_type)
            self.planner.state_queue.reset()

        self.planner.state_queue.mutex.release()

    def get_actions(self):
        return list(self.actions.keys())

    def set_global_position(self, position):
        self.lock.acquire()
        self.global_position = position
        for idx in range(len(self.initial_pose_buffer)):
            self.initial_pose_buffer[idx][:3] = position
        self.state_machine.set_global_position(position)
        self.lock.release()
        if len(self.state_machine.pose_buffer) > 0:
            assert not np.isnan(self.state_machine.pose_buffer[-1]).any(), "Error in set pos "+str(position)

    def set_global_orientation(self, orientation):
        self.lock.acquire()
        self.global_orientation = orientation
        for idx in range(len(self.initial_pose_buffer)):
            self.initial_pose_buffer[idx][3:7] = orientation
        self.state_machine.set_global_orientation(orientation)
        self.lock.release()
        if len(self.state_machine.pose_buffer) >0:
            assert not np.isnan(self.state_machine.pose_buffer[-1]).any(), "Error in set orientation "+str(orientation)

    def unpause(self):
        self.state_machine.unpause()

    def update_transformation(self):
        self.state_machine.update_transformation()

    def get_position(self):
        return self.state_machine.get_position()

    def get_global_transformation(self):
        return self.state_machine.get_global_transformation()

    def get_n_frames(self):
        return self.state_machine.get_global_get_n_framestransformation()

    def get_frame_time(self):
        return self.state_machine.get_frame_time()
    
    def reached_secondary_ik_target(self):
        if self.secondary_ik_target is not None:
            return self.secondary_ik_target.has_reached()
        else:
            return True

    def get_pose(self, frame_idx=None):
        frame = self.state_machine.get_pose(frame_idx)
        if self.secondary_ik_target is not None:
            frame = self.secondary_ik_target.reach2(frame)
        if self.activate_grounding:
            frame = self.motion_grounding.apply_on_frame(frame, self.idle_constraints, self.scene_interface)
        return frame
        
    def get_current_frame_idx(self):
        return self.state_machine.get_current_frame_idx()

    def get_skeleton(self):
        if self.target_skeleton is not None:
            return self.target_skeleton
        else:
            return self.skeleton

    def get_animated_joints(self):
        return self.skeleton.animated_joints

    def get_current_frame(self):
        return self.get_pose(None)

    def update_idle_foot_grounding_constraints(self):
        self.idle_constraints = self.foot_constraint_generator.generate_grounding_constraints(self.initial_pose_buffer, 0, self.foot_constraint_generator.contact_joints)
    
    def reset(self):
        self.lock.acquire()
        if self.thread is not None:
            print("stop thread")
            self.planner.stop_thread = True
            self.thread.join()
            self.stop_current_state = True
            self.thread = None
        self.planner.state_queue.reset()
        self.lock.release()
        self.reset_planner()

    def is_idle(self):
        return self.state_machine.node_type == NODE_TYPE_IDLE
      

    def is_paused(self):
        return self.state_machine.state.paused

    def set_secondary_ik_target(self, joint_name, position, orientation=None, chain_end_joint=None, look_at=False):
        speed = DEFAULT_SECONDARY_TARGET_SPEED * self.frame_time
        self.secondary_ik_target = SecondaryIKTarget(self.skeleton, joint_name, position, orientation, chain_end_joint, speed)
        self.secondary_ik_target.prepare_joint_parameter_indices()
        if look_at:
            self.secondary_ik_target.look_at_dir = self.planner.look_at_dir

    def remove_secondary_ik_target(self):
        self.secondary_ik_target = None
