## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Janis Sprenger


from MMIStandard.avatar.ttypes import MAvatarPosture, MJoint, MJointType, MChannel
from MMIStandard.math.ttypes import MQuaternion, MVector3

import json

def JSON2MAvatarPosture(data):
    data = {}
    with open(filepath, "r") as f:
        data = json.load(f)
    name = data["1"]["str"]
    jointlist = []
    for i in range(2, len(data["2"]["lst"])):
        bonename = data["2"]["lst"][i]["1"]["str"]
        bonetype = data["2"]["lst"][i]["2"]["i32"]
        bonetype = MJointType._VALUES_TO_NAMES[bonetype]
        
        vd = data["2"]["lst"][i]["3"]["rec"]
        vector = MVector3(vd["1"]["dbl"], vd["2"]["dbl"], vd["3"]["dbl"])


        qd = data["2"]["lst"][i]["4"]["rec"]
        quat = MQuaternion(qd["1"]["dbl"], qd["2"]["dbl"], qd["3"]["dbl"], qd["4"]["dbl"])

        jointlist.append(MJoint(bonename, bonetype, vector, quat))
    return MAvatarPosture(name, jointlist)

def NewJson2MAvatarPosture(data, avatarID = "Avatar"):
    joints = []
    for j in data["Joints"]:
        name = j["ID"]
        type = j["Type"]
        pos = MVector3(j["Position"]["X"], j["Position"]["Y"], j["Position"]["Z"])
        #rot = MQuaternion(j["Rotation"]["X"], j["Rotation"]["Y"], j["Rotation"]["Z"], j["Rotation"]["W"])
        rot = MQuaternion(0,0,0,1)
        joints.append(MJoint(name, type, pos, rot))
    return MAvatarPosture(avatarID, joints)

def String2MAvatarPosture(lines, avatarID = "Avatar"):
    parent_map = {None : None}
    joints = []
    name = ""
    pos = None
    rot = None
    channels = []
    parentJointName = None
    for i in range(len(lines)):
        line = lines[i].strip().split(" ")
        if line[0] == "ROOT" or line[0] == "JOINT":
            name = lines[i].strip().split(" ")[1]
            if name == "PelvisCenter":
                name = "PelvisCentre"
        elif line[0] == "OFFSET":
            pos = MVector3(float(line[1]), float(line[2]), float(line[3]))
        elif line[0] == "ROTATION":
            rot = MQuaternion(float(line[4]), float(line[1]), float(line[2]), float(line[3]))
        elif line[0] == "CHANNELS":
            channels = [MChannel._NAMES_TO_VALUES[line[j]] for j in range(1, len(line))]
        joint = MJoint(name, MJointType._NAMES_TO_VALUES[name], pos, rot, channels, parentJointName)
        parent_map[name] = parentJointName
        parentJointName = name
        joints.append(joint)
        if line[0] == "}":
            name = parentJointName
            parentJointName = parent_map[parentJointName]
    
    return MAvatarPosture(avatarID, joints)


def MAvatarPosture2String(avatarPosture):# : MAvatarPosture) -> str:
    ret = "HIERARCHY\n"
    jointTexts = {}
    children = {}
    for j in avatarPosture.Joints:
        jointtxt = [""]
        if j.Parent is None:
            jointtxt[0] = "ROOT "
        else:
            jointtxt[0] = "JOINT "
        jointtxt[0] += j.ID
        jointtxt.append("OFFSET %.5f %.5f %.5f"%(j.Position.X, j.Position.Y,j.Position.Z))
        jointtxt.append("ROTATION %.5f %.5f %.5f %.5f"%(j.Rotation.W, j.Rotation.X, j.Rotation.Y,j.Rotation.Z))
        jointtxt.append("CHANNELS ")
        for c in j.Channels:
            jointtxt[-1] += MChannel._VALUES_TO_NAMES[c] + " "
        jointTexts[j.ID] = jointtxt
        if not j.Parent is None:
            if not j.Parent in children:
                children[j.Parent] = [j.ID]
            else:
                children[j.Parent].append(j.ID)