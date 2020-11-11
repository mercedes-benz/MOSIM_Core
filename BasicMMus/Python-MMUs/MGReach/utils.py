import numpy as np
from .anim_utils.animation_data.joint_constraints import JointConstraint

def normalize(v):
    return v/np.linalg.norm(v)

def array_from_mosim_t(_t):
    t = np.zeros(3)
    t[0] = -_t.X
    t[1] = _t.Y
    t[2] = _t.Z
    return t

def array_from_mosim_q(_q):
    q = np.zeros(4)
    q[0] = -_q.W
    q[1] = -_q.X
    q[2] = _q.Y
    q[3] = _q.Z
    q = normalize(q)
    return q

def array_from_mosim_r(r):
    q = quaternion_from_euler(-np.radians(r.X), np.radians(r.Y), np.radians(r.Z))
    return q


def set_static_joints(skeleton, default_joint_names=["left_clavicle", "right_clavicle"]):
    for j in default_joint_names:
        joint_name = None
        if j in skeleton.skeleton_model["joints"]:
            joint_name = skeleton.skeleton_model["joints"][j]
            c =  JointConstraint()
            c.is_static = True
            skeleton.nodes[joint_name].joint_constraint = c

def get_inverted_joint_map(joint_map):
    inverted_map = dict()
    for name in joint_map:
        key = joint_map[name]
        inverted_map[key] = name
    return inverted_map

def map_joint_to_skeleton(skeleton, side):
    if side == "Left":
        default_joint_name = "left_wrist"
    else:
        default_joint_name = "right_wrist"
    joint_name = None
    if default_joint_name in skeleton.skeleton_model["joints"]:
        joint_name = skeleton.skeleton_model["joints"][default_joint_name]
    return joint_name


def get_chain_end_joint(skeleton, side):
    if side == "Left":
        default_joint_name = "left_clavicle"
    else:
        default_joint_name = "right_clavicle"
    joint_name = None
    if default_joint_name in skeleton.skeleton_model["joints"]:
        joint_name = skeleton.skeleton_model["joints"][default_joint_name]
    return joint_name

def get_long_chain_end_joint(skeleton):
    default_joint_name = "spine_1"
    joint_name = None
    if default_joint_name in skeleton.skeleton_model["joints"]:
        joint_name = skeleton.skeleton_model["joints"][default_joint_name]
    return joint_name

def generate_constraint_desc(joint, keyframe_label, target_transform, set_orientation, hold_frame, look_at, scale):
    c_desc = dict()
    c_desc["keyframe"] = keyframe_label
    c_desc["position"] = array_from_mosim_t(target_transform.Position)*scale
    if set_orientation:
        c_desc["orientation"] = array_from_mosim_q(target_transform.Rotation)
    else:
        c_desc["orientation"] = None
    c_desc["constrainOrientation"] = set_orientation
    c_desc["joint"] = joint
    print("set hand target", c_desc["position"], c_desc["orientation"], target_transform.Rotation)
    c_desc["hold"] = hold_frame
    return c_desc

def generate_action_desc(action_name, constraints, look_at):
    action_desc = dict()
    action_desc["name"] = action_name
    action_desc["frameConstraints"] = constraints
    action_desc["lookAtConstraints"] = look_at
    return action_desc