from MMIStandard.math.ttypes import MVector3, MQuaternion
from MMIStandard.avatar.ttypes import MAvatarPosture, MJoint, MJointType, MChannel, MAvatarPostureValues

import json, uuid

def MVector3ToArray(v):
    return np.array([-v.X, v.Y, v.Z])

def ArrayToMVector3(a):
    vec = MVector3(-a[0], a[1], a[2])
    #vec = MVector3(a[0], a[1], a[2])
    return vec

def MQuaternionToArray(q):
    return np.array([-q.W, -q.X, q.Y, q.Z])
 #   return np.array([q.W, q.X, q.Y, q.Z])


def ArrayToMQuaternion(a):
    #return MQuaternion(-a[1], a[2], a[3], -a[0])
    return MQuaternion(-a[1], a[2], a[3], -a[0])

def LoadMAvatarPosture(filename):
    with open(filename, "r") as f:
        data = json.load(f)
    
    p = MAvatarPosture()
    p.AvatarID = str(uuid.uuid1()) #data["AvatarID"]

    def processJoint(d):
        j = MJoint()
        j.ID = d["ID"]
        typ = d["Type"]
        j.Type = MJointType._NAMES_TO_VALUES[typ]
        j.Position = MVector3(d["Position"]["X"], d["Position"]["Y"], d["Position"]["Z"])
        j.Rotation = MQuaternion(d["Rotation"]["X"], d["Rotation"]["Y"], d["Rotation"]["Z"], d["Rotation"]["W"])
        j.Channels = d["Channels"]
        j.Parent = d["Parent"]
        return j

    p.Joints = [processJoint(d) for d in data["Joints"]]
    return p