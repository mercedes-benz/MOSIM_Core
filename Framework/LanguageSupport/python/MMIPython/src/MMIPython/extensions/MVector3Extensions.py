## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Janis Sprenger, Usama Bin Aslam

from MMIStandard.math.ttypes import MQuaternion, MVector3

import math
import sys


def Magnitude(vector : MVector3):
    return float(math.sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z))


def ToMVector3(values):
    return MVector3(values[0], values[1], values[2])


def GetValues(vector : MVector3):
    return [vector.X, vector.Y, vector.Z]


def Normalize(vector : MVector3):
    return Divide(vector, Magnitude(vector))


def Multiply(vector : MVector3, scalar):
    return MVector3(vector.X * scalar,vector.Y * scalar, vector.Z * scalar)


def Subtract(v1 : MVector3, v2 : MVector3):
    return MVector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z)


def Distance(vector1 : MVector3, vector2 : MVector3):
    return Magnitude(Subtract(vector1 , vector2))


def Add(v1 : MVector3, v2 : MVector3):
    return MVector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z)


def Divide(vector : MVector3, scalar):
    return MVector3(vector.X / scalar, vector.Y / scalar, vector.Z / scalar)


def Lerp(xfrom : MVector3, yto: MVector3, t):
    return Add(xfrom, Multiply(Subtract(yto, xfrom), t))

def Clone(vector : MVector3):
    if not vector is None:
        return MVector3(vector.X, vector.Y, vector.Z)
    else:
        return None