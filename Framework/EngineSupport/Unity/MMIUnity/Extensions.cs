// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer


using MMIStandard;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity
{
    /// <summary>
    /// Extensions for the transform
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates an MJOint based on the given transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useGlobalPosition"></param>
        /// <returns></returns>
        public static MJoint ToJointTransform(this Transform transform, bool useGlobalPosition = false)
        {
            ///Create a new joint transform
            MJoint jointTransform = new MJoint
            {
                ID = transform.name,
                Type = MJointType.Undefined
            };

            if (useGlobalPosition)
            {
                jointTransform.Position = transform.position.ToMVector3();
                jointTransform.Rotation = transform.rotation.ToMQuaternion();
            }
            else
            {
                jointTransform.Position = transform.localPosition.ToMVector3();
                jointTransform.Rotation = transform.localRotation.ToMQuaternion();
            }

            if (transform.parent != null)
                jointTransform.Parent = transform.parent.name;

            return jointTransform;
        }


        /// <summary>
        /// Converts a vector3 to a float array representing the 3D coordinates
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float[] ToArray(this Vector3 vector)
        {
            return new float[] { vector.x, vector.y, vector.z };
        }

        /// <summary>
        /// Converts a vector3 to a double list representing the 3D coordinates
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static List<double> ToList(this Vector3 vector)
        {
            return new List<double> { vector.x, vector.y, vector.z };
        }

        /// <summary>
        /// Converts a Quaternion to a float array representing the 4 rotation components  (xyzw)
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public static float[] ToArray(this Quaternion quaternion)
        {
            return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }

        /// <summary>
        /// Converts a Quaternion to a double list representing the 4 rotation components (xyzw)
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public static List<double> ToList(this Quaternion quaternion)
        {
            return new List<double> { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }

        /// <summary>
        /// Generates a new Vector3 from the float array (x,y,z)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this float[] data)
        {
            return new Vector3(data[0], data[1], data[2]);
        }

        /// <summary>
        /// Converts the MVector3 of the MMI framework to the Unity Vector3
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this MVector3 data)
        {
            return new Vector3((float)data.X, (float)data.Y, (float)data.Z);
        }

        /// <summary>
        /// Casts the double list to a vector 3 (x,y,z)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this List<double> data)
        {
            return new Vector3((float)data[0], (float)data[1], (float)data[2]);
        }

        /// <summary>
        /// Creates a quaternion from the float array (x,y,z,w)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(this float[] data)
        {
            return new Quaternion(data[0], data[1], data[2], data[3]);
        }

        /// <summary>
        /// Creates a quaternion from the double list  (x,y,z,w)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(this List<double> data)
        {
            return new Quaternion((float)data[0], (float)data[1], (float)data[2], (float)data[3]);
        }

        /// <summary>
        /// Converts the MQuaternion of the MMI framework to the Unity Quaternion
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(this MQuaternion quat)
        {
            return new Quaternion((float)quat.X, (float)quat.Y, (float)quat.Z, (float)quat.W);
        }

        /// <summary>
        /// Returns the total length (sum of euclidean distances) of the vector3 list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static float Length(this List<Vector3> list)
        {
            float length = 0;
            for (int i = 0; i < list.Count - 1; i++)
            {
                length += (list[i + 1] - list[i]).magnitude;
            }
            return length;
        }

        /// <summary>
        /// Casts the vector3 to vector 2 (using x-z coordinates)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        /// <summary>
        /// Casts the vector 2 to Vector 3 (x,0,y)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, 0, vector.y);
        }

        /// <summary>
        /// Converts a Unity Vector3 to the MMI framework MVector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MVector3 ToMVector3(this Vector3 vector)
        {
            return new MVector3(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Converts a Unity Quaternion to the MMI framework MQuaternion
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        public static MQuaternion ToMQuaternion(this Quaternion quat)
        {
            return new MQuaternion(quat.x, quat.y, quat.z, quat.w);
        }

    }

}
