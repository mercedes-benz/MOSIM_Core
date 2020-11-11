// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;

namespace CarryMMU
{
    public static class Extensions
    {
        /// <summary>
        /// Function taken from: 
        /// https://gist.github.com/aeroson/043001ca12fe29ee911e
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static MVector3 ToEulerRad(this MQuaternion rotation)
        {
            double sqw = rotation.W * rotation.W;
            double sqx = rotation.X * rotation.X;
            double sqy = rotation.Y * rotation.Y;
            double sqz = rotation.Z * rotation.Z;
            double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            double test = rotation.X * rotation.W - rotation.Y * rotation.Z;
            MVector3 v = new MVector3();

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.Y = 2f * Math.Atan2(rotation.Y, rotation.X);
                v.X = Math.PI / 2;
                v.Z = 0;
                return NormalizeAngles(v.Multiply(Rad2Deg));
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.Y = -2f * Math.Atan2(rotation.Y, rotation.X);
                v.X = -Math.PI / 2;
                v.Z = 0;
                return NormalizeAngles(v.Multiply(Rad2Deg));
            }
            MQuaternion q = new MQuaternion(rotation.W, rotation.Z, rotation.X, rotation.Y);
            v.Y = Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));     // Yaw
            v.X = Math.Asin(2f * (q.X * q.Z - q.W * q.Y));                             // Pitch
            v.Z = Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));      // Roll
            return NormalizeAngles(v.Multiply(Rad2Deg));
        }

        public static MVector3 ToEuler(this MQuaternion rotation)
        {
            return rotation.ToEulerRad().Multiply(Rad2Deg);
        }

        private static MVector3 NormalizeAngles(MVector3 angles)
        {
            angles.X = NormalizeAngle(angles.X);
            angles.Y = NormalizeAngle(angles.Y);
            angles.Z = NormalizeAngle(angles.Z);
            return angles;
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }


        public static double Angle(MVector3 from, MVector3 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            double denominator = Math.Sqrt(from.SqrMagnitude() * to.SqrMagnitude());
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            double dot = Clamp(Dot(from, to) / denominator, -1F, 1F);
            return (Math.Acos(dot)) * Rad2Deg;
        }

        // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
        // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
        // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
        public static double SignedAngle(MVector3 from, MVector3 to, MVector3 axis)
        {
            double unsignedAngle = Angle(from, to);

            double cross_x = from.Y * to.Z - from.Z * to.Y;
            double cross_y = from.Z * to.X - from.X * to.Z;
            double cross_z = from.X * to.Y - from.Y * to.X;
            double sign = Math.Sign(axis.X * cross_x + axis.Y * cross_y + axis.Z * cross_z);
            return unsignedAngle * sign;
        }


        public static double Dot(MVector3 vector1, MVector3 vector2)
        {
            return vector1.X * vector2.X +vector1.Y * vector2.Y +vector1.Z * vector2.Z;
        }

        public static double SqrMagnitude(this MVector3 vector)
        {
            return vector.Magnitude() * vector.Magnitude();
        }

        /// <summary>
        /// Clamps a value to a given max and min range.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                double help = min;
                min = max;
                max = help;
            }
            if (value < min)
                value = min;
            else if (value > max)
                value = max;

            return value;
        }



        private static double Rad2Deg = 180 / Math.PI;
        // *Undocumented*
        public const float kEpsilon = 0.00001F;
        // *Undocumented*
        public const float kEpsilonNormalSqrt = 1e-15F;
    }
}
