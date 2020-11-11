// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Janis Sprenger

using System;
using System.Collections.Generic;


namespace MMIStandard
{
    /// <summary>
    /// Extension class for MQuaternion
    /// </summary>
    public static class MQuaternionExtensions
    {

        /// <summary>
        /// Const for rad<->degree conversions
        /// </summary>
        private const double Rad2Deg = 180.0 / Math.PI;
        private const double Deg2Rad = Math.PI / 180.0;


        /// <summary>
        /// Creates a new MQuaternion instance based on a list of double values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static MQuaternion ToMQuaternion(this List<double> values)
        {
            return new MQuaternion(values[0], values[1], values[2], values[3]);
        }


        /// <summary>
        /// Returns the values of the vector as list of doubles
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static List<double> GetValues(this MQuaternion quat)
        {
            return new List<double>() { quat.X, quat.Y, quat.Z, quat.W };
        }

        /// <summary> 
        /// Method performs a quaternion multiplication. 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>/returns>
        public static MQuaternion Multiply(this MQuaternion left, MQuaternion right)
        {
            return new MQuaternion()
            {
                X = left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y,
                Y = left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z,
                Z = left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X,
                W = left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z
            };

        }


        /// <summary>
        /// Multiplies a vector with a quaternion
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MVector3 Multiply(this MQuaternion rotation, MVector3 vector)
        {
            double rx2 = rotation.X * 2f;
            double ry2 = rotation.Y * 2f;
            double rz2 = rotation.Z * 2f;
            double rx = rotation.X * rx2;
            double ry = rotation.Y * ry2;
            double rz = rotation.Z * rz2;
            double rxy = rotation.X * ry2;
            double rxz = rotation.X * rz2;
            double ryz = rotation.Y * rz2;
            double rwx = rotation.W * rx2;
            double rwy = rotation.W * ry2;
            double rwz = rotation.W * rz2;

            //Create a new vector representing the rotated one and return it
            return new MVector3
            {
                X = (1f - (ry + rz)) * vector.X + (rxy - rwz) * vector.Y + (rxz + rwy) * vector.Z,
                Y = (rxy + rwz) * vector.X + (1f - (rx + rz)) * vector.Y + (ryz - rwx) * vector.Z,
                Z = (rxz - rwy) * vector.X + (ryz + rwx) * vector.Y + (1f - (rx + ry)) * vector.Z
            };
        }



        /// <summary>
        /// Returns the lenght of tquaternion
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float Length(this MQuaternion rotation)
        {
            double norm2 = rotation.X * rotation.X + rotation.Y * rotation.Y + rotation .Z* rotation.Z + rotation.W * rotation.W;

            if (!(norm2 <= double.MaxValue))
            {
                // Do this the slow way to avoid squaring large
                // numbers since the length of many quaternions is
                // representable even if the squared length isn't.  Of 
                // course some lengths aren't representable because
                // the length can be up to twice as big as the largest 
                // coefficient. 

                double max = Math.Max(Math.Max(Math.Abs(rotation.X), Math.Abs(rotation.Y)),
                                      Math.Max(Math.Abs(rotation.Z), Math.Abs(rotation.W)));

                double x = rotation.X / max;
                double y = rotation.Y/ max;
                double z = rotation.Z / max;
                double w = rotation.W / max;


                double smallLength = Math.Sqrt(x * x + y * y + z * z + w * w);
                // Return length of this smaller vector times the scale we applied originally. 

                return (float)(smallLength * max);
            }
            return (float)Math.Sqrt(norm2);
        }


        /// <summary>
        /// Scale this quaternion by a scalar.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public static void Scale(this MQuaternion rotation, float scale)
        {
            rotation.X *= scale;
            rotation.Y *= scale;
            rotation.Z *= scale;
            rotation.W *= scale;
        }


        /// <summary>
        /// Creates the inverse quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public static MQuaternion Inverse(MQuaternion quaternion)
        {
            //Inverse = Conjugate / Norm Squared 
            MQuaternion inverted = new MQuaternion(-quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);

            //Further normalize the quaternion
            double norm = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;

            inverted.X /= norm;
            inverted.Y /= norm;
            inverted.Z /= norm;
            inverted.W /= norm;

            return inverted;
        }


        /// <summary>
        /// Normalizes the quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        public static void Normalize(this MQuaternion quaternion)
        {
            //Further normalize the quaternion
            double denominator = Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);

            quaternion.X /= denominator;
            quaternion.Y /= denominator;
            quaternion.Z /= denominator;
            quaternion.W /= denominator;
        }

        /// <summary>
        /// Smoothly interpolate between the two given quaternions using Spherical
        /// Linear Interpolation (SLERP).
        /// https://referencesource.microsoft.com/#PresentationCore/Core/CSharp/system/windows/Media3D/Quaternion.cs
        /// </summary> 
        /// <param name="from">First quaternion for interpolation.
        /// <param name="to">Second quaternion for interpolation. 
        /// <param name="t">Interpolation coefficient. 
        /// <param name="useShortestPath">If true, Slerp will automatically flip the sign of
        /// the destination Quaternion to ensure the shortest path is taken. 
        /// <returns>SLERP-interpolated quaternion between the two given quaternions.</returns>
        public static MQuaternion Slerp(this MQuaternion from, MQuaternion to, float t, bool useShortestPath = true)
        {

            double cosOmega;
            double scaleFrom, scaleTo;

            // Normalize inputs and stash their lengths 
            double lengthFrom = from.Length();
            double lengthTo = to.Length();
            from.Scale(1.0f / (float)lengthFrom);
            to.Scale(1.0f / (float)lengthTo);

            // Calculate cos of omega. 
            cosOmega = from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W;

            if (useShortestPath)
            {
                // If we are taking the shortest path we flip the signs to ensure that 
                // cosOmega will be positive.
                if (cosOmega < 0.0)
                {
                    cosOmega = -cosOmega;
                    to.X = -to.X;
                    to.Y = -to.Y;
                    to.Z = -to.Z;
                    to.W = -to.W;
                }
            }
            else
            {
                // If we are not taking the UseShortestPath we clamp cosOmega to 
                // -1 to stay in the domain of MathF.Acos below.
                if (cosOmega < -1.0)
                {
                    cosOmega = -1.0f;
                }
            }

            // Clamp cosOmega to [-1,1] to stay in the domain of MathF.Acos below.
            // The logic above has either flipped the sign of cosOmega to ensure it 
            // is positive or clamped to -1 aready.  We only need to worry about the
            // upper limit here. 
            if (cosOmega > 1.0)
            {
                cosOmega = 1.0f;
            }


            // The mainline algorithm doesn't work for extreme 
            // cosine values.  For large cosine we have a better 
            // fallback hence the asymmetric limits.
            const double maxCosine = 1.0f - 1e-6f;
            const double minCosine = 1e-10f - 1.0f;

            // Calculate scaling coefficients.
            if (cosOmega > maxCosine)
            {
                // Quaternions are too close - use linear interpolation. 
                scaleFrom = 1.0f - t;
                scaleTo = t;
            }
            else if (cosOmega < minCosine)
            {
                // Quaternions are nearly opposite, so we will pretend to
                // is exactly -from. 
                // First assign arbitrary perpendicular to "to".
                to = new MQuaternion(-from.Y, from.X, -from.W, from.Z);

                double theta = t * Math.PI;

                scaleFrom = Math.Cos(theta);
                scaleTo = Math.Sin(theta);
            }
            else
            {
                // Standard case - use SLERP interpolation. 
                double omega = Math.Acos(cosOmega);
                double sinOmega = Math.Sqrt(1.0f - cosOmega * cosOmega);
                scaleFrom = Math.Sin((1.0f - t) * omega) / sinOmega;
                scaleTo = Math.Sin(t * omega) / sinOmega;
            }

            // We want the magnitude of the output quaternion to be 
            // multiplicatively interpolated between the input
            // magnitudes, i.e. lengthOut = lengthFrom * (lengthTo/lengthFrom)^t 
            //                            = lengthFrom ^ (1-t) * lengthTo ^ t 

            double lengthOut = lengthFrom * Math.Pow(lengthTo / lengthFrom, t);
            scaleFrom *= lengthOut;
            scaleTo *= lengthOut;

            return new MQuaternion(scaleFrom * from.X + scaleTo * to.X,
                                  scaleFrom * from.Y + scaleTo * to.Y,
                                  scaleFrom * from.Z + scaleTo * to.Z,
                                  scaleFrom * from.W + scaleTo * to.W);
        }



        /// <summary>
        /// Computes teh angle (in degree) between to rotations specified using quaternions
        /// </summary>
        /// <param name="rotation1"></param>
        /// <param name="rotation2"></param>
        /// <returns></returns>
        public static double Angle(MQuaternion rotation1, MQuaternion rotation2)
        {
            double dot = Dot(rotation1, rotation2);

            return Math.Acos(Math.Min(Math.Abs(dot), 1f)) * 2f * Rad2Deg;
        }


        /// <summary>
        /// Computes the dot product of two quaternion rotations
        /// </summary>
        /// <param name="rotation1"></param>
        /// <param name="rotation2"></param>
        /// <returns></returns>
        public static double Dot(MQuaternion rotation1, MQuaternion rotation2)
        {
            return rotation1.X * rotation2.X + rotation1.Y * rotation2.Y + rotation1.Z * rotation2.Z + rotation1.W * rotation2.W;
        }


        /// <summary>
        /// Creates euler angles (in degree) form the given quaternion
        /// Source code from: https://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static MVector3 ToEuler(MQuaternion q)
        {
            MVector3 euler = new MVector3();

            // if the input quaternion is normalized, this is exactly one. Otherwise, this acts as a correction factor for the quaternion's not-normalizedness
            double unit = (q.X* q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);

            // this will have a magnitude of 0.5 or greater if and only if this is a singularity case
            double test = q.X * q.W - q.Y * q.Z;

            if (test > 0.4995f * unit) // singularity at north pole
            {
                euler.X = Math.PI / 2;
                euler.Y = 2f * Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else if (test < -0.4995f * unit) // singularity at south pole
            {
                euler.X = -Math.PI / 2;
                euler.Y = -2f * Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else // no singularity - this is the majority of cases
            {
                euler.X = Math.Asin(2f * (q.W * q.X - q.Y * q.Z));
                euler.Y = Math.Atan2(2f * q.W * q.Y + 2f * q.Z * q.X, 1 - 2f * (q.X * q.X + q.Y * q.Y));
                euler.Z = Math.Atan2(2f * q.W * q.Z + 2f * q.X * q.Y, 1 - 2f * (q.Z * q.Z + q.X * q.X));
            }

            // all the math so far has been done in radians. Before returning, we convert to degrees...
            euler = euler.Multiply(Rad2Deg);

            //...and then ensure the degree values are between 0 and 360
            euler.X %= 360;
            euler.Y %= 360;
            euler.Z %= 360;

            return euler;
        }


        /// <summary>
        /// Creates a quaternion based on the given euler angles (in degree).
        /// Source code from: https://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine
        /// </summary>
        /// <param name="euler"></param>
        /// <returns></returns>
        public static MQuaternion FromEuler(MVector3 euler)
        {
            double xOver2 = euler.X * Deg2Rad * 0.5f;
            double yOver2 = euler.Y * Deg2Rad * 0.5f;
            double zOver2 = euler.Z * Deg2Rad * 0.5f;

            double sinXOver2 = Math.Sin(xOver2);
            double cosXOver2 = Math.Cos(xOver2);
            double sinYOver2 = Math.Sin(yOver2);
            double cosYOver2 = Math.Cos(yOver2);
            double sinZOver2 = Math.Sin(zOver2);
            double cosZOver2 = Math.Cos(zOver2);

            MQuaternion result = new MQuaternion
            {
                X = cosYOver2 * sinXOver2 * cosZOver2 + sinYOver2 * cosXOver2 * sinZOver2,
                Y = sinYOver2 * cosXOver2 * cosZOver2 - cosYOver2 * sinXOver2 * sinZOver2,
                Z = cosYOver2 * cosXOver2 * sinZOver2 - sinYOver2 * sinXOver2 * cosZOver2,
                W = cosYOver2 * cosXOver2 * cosZOver2 + sinYOver2 * sinXOver2 * sinZOver2
            };

            return result;
        }


        /// <summary>
        /// Returns a deep copy of the quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        /// <returns></returns>
        public static MQuaternion Clone(this MQuaternion quaternion)
        {
            if(quaternion!=null)
                return new MQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
            else
                return null;                   
        }



        /// <summary>
        /// Computes the rotation to rotate from one vector to the other
        /// </summary>
        /// <param name="from">The start direction</param>
        /// <param name="to">The desired direction</param>
        /// <returns></returns>
        private static MQuaternion FromToRotation(MVector3 from, MVector3 to)
        {
            //Normalize both vectors
            from = from.Normalize();
            to = to.Normalize();

            //Estimate the rotation axis
            MVector3 axis = MVector3Extensions.Cross(from, to).Normalize();

            //Compute the phi angle
            double phi = Math.Acos(MVector3Extensions.Dot(from, to)) / (from.Magnitude() * to.Magnitude());

            //Create a new quaternion representing the rotation
            MQuaternion result = new MQuaternion()
            {
                X = Math.Sin(phi / 2) * axis.X,
                Y = Math.Sin(phi / 2) * axis.Y,
                Z = Math.Sin(phi / 2) * axis.Z,
                W = Math.Cos(phi / 2)
            };

            //Perform is nan check and return identity quaternion
            if (double.IsNaN(result.W) || double.IsNaN(result.X) || double.IsNaN(result.Y) || double.IsNaN(result.Z))
                result = new MQuaternion(0, 0, 0, 1);

            //Return the estimated rotation
            return result;
        }
    }
}
