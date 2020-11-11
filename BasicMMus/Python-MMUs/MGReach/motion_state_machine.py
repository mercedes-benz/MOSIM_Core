import copy
import numpy as np

class MotionStateMachine(object):
    def __init__(self, skeleton, state, current_node, node_type):
        self.skeleton = skeleton
        self.state = state
        self.current_node = current_node
        self.node_type = node_type 
        self.pose_buffer = []
        self.play = True
        self.buffer_size = 10

    def reset(self, state, current_node, node_type):
        self.state = state
        self.current_node = current_node
        self.node_type = node_type 
        self.pose_buffer = []

    def update(self, dt):
        return self.state.update(dt)

    def set_state_entry(self, state_entry):
        self.state = state_entry.state
        self.current_node = state_entry.node
        self.node_type = state_entry.node_type
        self.pose_buffer = copy.copy(state_entry.pose_buffer)

    def set_global_position(self, position):
        self.state.set_position(position)
        self.set_buffer_position(position)

    def set_global_orientation(self, orientation):
        self.state.set_orientation(orientation)
        self.set_buffer_orientation(orientation)

    def set_buffer_position(self, pos):
        for idx in range(len(self.pose_buffer)):
            self.pose_buffer[idx][:3] = pos

    def set_buffer_orientation(self, orientation):
        for idx in range(len(self.pose_buffer)):
            self.pose_buffer[idx][3:7] = orientation
        
    def unpause(self):
        self.state.hold_last_frame = False
        self.state.paused = False

    def update_transformation(self):
        pose = self.state.get_pose()
        self.pose_buffer.append(np.array(pose))
        self.pose_buffer = self.pose_buffer[-self.buffer_size:]

    def get_position(self):
        if self.state is not None:
            return self.state.get_pose()[:3]
        else:
            return [0, 0, 0]

    def get_global_transformation(self):
        return self.skeleton.nodes[self.skeleton.root].get_global_matrix(self.pose_buffer[-1])

    def get_n_frames(self):
        return self.state.get_n_frames()

    def get_frame_time(self):
        return self.state.get_frame_time()

    def get_pose(self, frame_idx=None):
        return self.state.get_pose(frame_idx)
        
    def get_current_frame_idx(self):
        return self.state.frame_idx

    def get_skeleton(self):
        if self.target_skeleton is not None:
            return self.target_skeleton
        else:
            return self.skeleton

    def get_animated_joints(self):
        return self.skeleton.animated_joints

    def get_current_frame(self):
        return self.state.get_pose(None)

