// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;


namespace MMIStandard
{
    /// <summary>
    /// Extensions for the MVector3 class
    /// </summary>
    public static class MVector3Extensions
    {
        /// <summary>
        /// Returns the magnitude of the vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float Magnitude(this MVector3 vector)
        {
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }


        /// <summary>
        /// Creates a new vector based on the given list of values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static MVector3 ToMVector3(this List<double> values)
        {
            return new MVector3(values[0], values[1], values[2]);
        }

        /// <summary>
        /// Returns the values of the vector as list of doubles
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static List<double> GetValues(this MVector3 vector)
        {
            return new List<double>() { vector.X, vector.Y, vector.Z };
        }

        /// <summary>
        /// Normalizes the given vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MVector3 Normalize(this MVector3 vector)
        {
            return vector.Divide(vector.Magnitude());
        }

        /// <summary>
        /// Returns a new vector which is the result of the Multiplication of the specified vector and a scalar value
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static MVector3 Multiply(this MVector3 vector, double scalar)
        {
            return new MVector3(vector.X * scalar,vector.Y * scalar, vector.Z * scalar);
        }

        /// <summary>
        /// The euclidean distance between two vectors
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static double Distance(MVector3 vector1, MVector3 vector2)
        {
            return (vector1.Subtract(vector2)).Magnitude();
        }


        /// <summary>
        /// Adds a vector on top of the current one and returns the result as new vector
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static MVector3 Add(this MVector3 v1, MVector3 v2)
        {
            return new MVector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        /// <summary>
        /// Performs a subtraction
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static MVector3 Subtract(this MVector3 v1, MVector3 v2)
        {
            return new MVector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Performs a division
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static MVector3 Divide(this MVector3 vector, float scalar)
        {
            return new MVector3(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
        }


        /// <summary>
        /// Lerps the vector
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static MVector3 Lerp(this MVector3 from, MVector3 to, float t)
        {
            return from.Add(to.Subtract(from).Multiply(t));
        }


        /// <summary>
        /// Returns a deep copy of the MVector3
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MVector3 Clone(this MVector3 vector)
        {
            if(vector !=null)
                return new MVector3(vector.X, vector.Y, vector.Z);
            else
                return null;
        }

        /// <summary>
        /// Converts the vector to an interval
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MInterval3 ToMInterval3 (this MVector3 vector)
        {
            return new MInterval3(new MInterval(vector.X, vector.X), new MInterval(vector.Y, vector.Y), new MInterval(vector.Z, vector.Z));
        }


        /// <summary>
        /// Converts the vector to an interval
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MInterval3 ToMInterval3(this MVector3 vector, MVector3 deviation)
        {
            return new MInterval3(new MInterval(vector.X- deviation.X/2f, vector.X+deviation.X/2f), new MInterval(vector.Y-deviation.Y/2f, vector.Y+ deviation.Y/2f), new MInterval(vector.Z-deviation.Z/2f, vector.Z+deviation.Z/2f));
        }

        /// <summary>
        /// Converts the vector to an interval
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MInterval3 ToMInterval3(this MVector3 vector, float deviation)
        {
            return new MInterval3(new MInterval(vector.X - deviation / 2f, vector.X + deviation / 2f), new MInterval(vector.Y - deviation / 2f, vector.Y + deviation / 2f), new MInterval(vector.Z - deviation / 2f, vector.Z + deviation / 2f));
        }

        public static MVector3 Cross(this MVector3 v1, MVector3 v2)
        {
            return new MVector3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        }

        public static double Dot(this MVector3 v1, MVector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static double Angle(this MVector3 from, MVector3 to)
        {
            double rad = from.Normalize().Dot(to.Normalize());
            // clamp
            rad = Math.Max(Math.Min(rad, 1), -1);
            return Math.Acos(rad) * 180 / Math.PI;
        }
        
        public static MQuaternion AngleAxis(double aAngle, MVector3 aAxis)
        {
            aAxis = aAxis.Normalize();
            double rad = aAngle * Math.PI / 180 * 0.5;
            aAxis = aAxis.Multiply(Math.Sin(rad));
            return new MQuaternion(aAxis.X, aAxis.Y, aAxis.Z, Math.Cos(rad));
        }

        public static MQuaternion FromToRotation(MVector3 aFrom, MVector3 aTo)
        {
            MVector3 axis = aFrom.Cross(aTo);
            double angle = aFrom.Angle(aTo);
            return AngleAxis(angle, axis.Normalize());
        }
    }
}
