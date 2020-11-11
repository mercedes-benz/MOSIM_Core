## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Janis Sprenger, Usama Bin Aslam


from MMIStandard.math.ttypes import MQuaternion, MVector3

from .MVector3Extensions import Multiply

import math
import sys

Rad2Deg = 180.0 / math.pi
Deg2Rad = math.pi / 180.0
    
def ToMQuaternion(values):
    return MQuaternion(values[0], values[1], values[2], values[3])

def GetValues(quat):
    return [quat.X, quat.Y, quat.Z, quat.W]

def QMultiply(left : MQuaternion, right : MQuaternion):
    x = left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y
    y = left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z
    z = left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X
    w = left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z

    result = MQuaternion(x, y, z, w)
    return result

def VMultiply(quat : MQuaternion, vec : MVector3):
    num = quat.X * 2.0 #round(quat.X, 2)
    num2 = quat.Y * 2.0 #round(quat.Y, 2)
    num3 = quat.Z * 2.0 #round(quat.Z, 2)
    num4 = quat.X * num
    num5 = quat.Y * num2
    num6 = quat.Z * num3
    num7 = quat.X * num2
    num8 = quat.X * num3
    num9 = quat.Y * num3
    num10 = quat.W * num
    num11 = quat.W * num2
    num12 = quat.W * num3

    result = MVector3()
    result.X = (1.0 - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z
    result.Y = (num7 + num12) * vec.X + (1.0 - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z
    result.Z = (num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1.0 - (num4 + num5)) * vec.Z
    return result

def Length(quat : MQuaternion):
    norm2 = quat.X * quat.X + quat.Y * quat.Y + quat.Z* quat.Z + quat.W * quat.W
    if not norm2 <= sys.float_info.max:
        maxV = max(max(abs(quat.X), abs(quat.Y)),
                    max(abs(quat.Z), abs(quat.W)))
        x = quat.X / maxV
        y = quat.Y/ maxV
        z = quat.Z / maxV
        w = quat.W / maxV
        
        smallLength = math.sqrt(x * x + y * y + z * z + w * w)

        return (smallLength * maxV)
    return (math.sqrt(norm2))

def Scale(quat : MQuaternion, scale):
    quat.X *= scale
    quat.Y *= scale
    quat.Z *= scale
    quat.W *= scale

def Inverse(quaternion : MQuaternion):
    inverted = MQuaternion(-quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W)
    norm = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W

    inverted.X /= norm
    inverted.Y /= norm
    inverted.Z /= norm
    inverted.W /= norm
    return inverted

def Slerp(xfrom : MQuaternion, yto : MQuaternion, t, useShortestPath = True):
    lengthFrom = Length(xfrom)
    lengthTo = Length(yto)
    Scale(xfrom,1.0 / float(lengthFrom) )
    Scale(yto,1.0 / float(lengthTo) )

    cosOmega = xfrom.X * yto.X + xfrom.Y * yto.Y + xfrom.Z * yto.Z + xfrom.W * yto.W

    if useShortestPath is True:
        if cosOmega < 0.0:
            cosOmega = -cosOmega
            yto.X = -yto.X
            yto.Y = -yto.Y
            yto.Z = -yto.Z
            yto.W = -yto.W
    else:
        if cosOmega < -1.0:
            cosOmega = -1.0
    if cosOmega > 1.0:
        cosOmega = 1.0
    
    maxCosine = 1.0 - 1e-6
    minCosine = 1e-6 - 1.0

    if cosOmega > maxCosine:
        scalexFrom = 1.0 - t
        scaleyTo = t
    elif cosOmega < minCosine:
        yto = MQuaternion(-xfrom.Y, xfrom.X, -xfrom.W, xfrom.Z)
        theta = t * math.pi
        scalexFrom = math.cos(theta)
        scaleyTo = math.sin(theta)
    else:
        omega = math.acos(cosOmega)
        sinOmega = math.sqrt(1.0 - cosOmega * cosOmega)
        scalexFrom = math.sin((1.0 - t) * omega) / sinOmega
        scaleyTo = math.sin(t * omega) / sinOmega
    
    lengthOut = lengthFrom * math.pow(lengthTo / lengthFrom, t)
    scalexFrom *= lengthOut
    scaleyTo *= lengthOut

    return MQuaternion(scalexFrom * xfrom.X + scaleyTo * yto.X,
                        scalexFrom * xfrom.Y + scaleyTo * yto.Y,
                        scalexFrom * xfrom.Z + scaleyTo * yto.Z,
                        scalexFrom * xfrom.W + scaleyTo * yto.W)

def Angle(a : MQuaternion, b : MQuaternion):
    f = Dot(a, b)
    return math.acos(min(abs(f), 1.0)) * 2.0 * Rad2Deg

def Dot(a : MQuaternion, b : MQuaternion):
    return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W

def ToEuler(q : MQuaternion):
    euler = MVector3()
    unit = (q.X* q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W)
    test = q.X * q.W - q.Y * q.Z
    if test > 0.4995 * unit:
        euler.X = math.pi / 2
        euler.Y = 2.0 * math.atan2(q.Y, q.X)
        euler.Z = 0
    elif test < -0.4995 * unit:
        euler.X = math.pi / 2
        euler.Y = -2.0 * math.atan2(q.Y, q.X)
        euler.Z = 0
    else:
        euler.X = math.asin(2.0 * (q.W * q.X - q.Y * q.Z))
        euler.Y = math.atan2(2.0 * q.W * q.Y + 2.0 * q.Z * q.X, 1 - 2.0 * (q.X * q.X + q.Y * q.Y))
        euler.Z = math.atan2(2.0 * q.W * q.Z + 2.0 * q.X * q.Y, 1 - 2.0 * (q.Z * q.Z + q.X * q.X))
    
    euler = Multiply(euler, Rad2Deg)
    euler.X = euler.X % 360
    euler.Y = euler.Y % 360 
    euler.Z = euler.Z % 360

    return euler

def FromEuler(euler : MVector3):
    xOver2 = euler.X * Deg2Rad * 0.5
    yOver2 = euler.Y * Deg2Rad * 0.5
    zOver2 = euler.Z * Deg2Rad * 0.5

    sinXOver2 = math.sin(xOver2)
    cosXOver2 = math.cos(xOver2)
    sinYOver2 = math.sin(yOver2)
    cosYOver2 = math.cos(yOver2)
    sinZOver2 = math.sin(zOver2)
    cosZOver2 = math.cos(zOver2)

    result = MQuaternion()
    result.X = cosYOver2 * sinXOver2 * cosZOver2 + sinYOver2 * cosXOver2 * sinZOver2
    result.Y = sinYOver2 * cosXOver2 * cosZOver2 - cosYOver2 * sinXOver2 * sinZOver2
    result.Z = cosYOver2 * cosXOver2 * sinZOver2 - sinYOver2 * sinXOver2 * cosZOver2
    result.W = cosYOver2 * cosXOver2 * cosZOver2 + sinYOver2 * sinXOver2 * sinZOver2

    return result

def Clone(quaternion : MQuaternion):
    if not quaternion is None:
        return MQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W)
    else:
        return None
