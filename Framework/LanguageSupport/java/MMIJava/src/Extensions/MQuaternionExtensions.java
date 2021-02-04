// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser


package Extensions;

import de.mosim.mmi.math.MQuaternion;
import de.mosim.mmi.math.MVector3;

import java.util.ArrayList;
import java.util.List;

public class MQuaternionExtensions {
/*
		Class which extends MBoolResponse
	*/

    private static final double Deg2Rad = Math.PI / 180.0;
    private static final double Rad2Deg = 180.0 / Math.PI;

    //	Method which converts double values to MQuaternion
    public static MQuaternion toMQuaternion(List<Double> values) {
        return new MQuaternion(values.get(0), values.get(1), values.get(2), values.get(3));
    }


    /// <summary>
    /// Returns the values of the vector as list of doubles
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static List<Double> getValues(MQuaternion quat) {
        List<Double> temp = new ArrayList<>();
        temp.add(quat.getX());
        temp.add(quat.getY());
        temp.add(quat.getZ());
        temp.add(quat.getW());
        return temp;
    }

    /// <summary>
    /// Returns a new vector which is the result of the Multiplication of the specified vector and a scalar value
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static MVector3 multiply(MVector3 vector, double scalar) {
        return new MVector3(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    /// <summary>
    /// Quaternion multiplication.
    /// </summary>
    /// <param name="left">First quaternion.
    /// <param name="right">Second quaternion.
    /// <returns>Result of multiplication.</returns>
    public static MQuaternion multiply(MQuaternion left, MQuaternion right) {
        double x = left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y;
        double y = left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z;
        double z = left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X;
        double w = left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z;

        return new MQuaternion(x, y, z, w);
    }


    /// <summary>
    /// Multiplies a vector with a quaternion
    /// </summary>
    /// <param name="quat"></param>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static MVector3 multiply(MQuaternion quat, MVector3 vec) {
        double num = quat.X * 2f;
        double num2 = quat.Y * 2f;
        double num3 = quat.Z * 2f;
        double num4 = quat.X * num;
        double num5 = quat.Y * num2;
        double num6 = quat.Z * num3;
        double num7 = quat.X * num2;
        double num8 = quat.X * num3;
        double num9 = quat.Y * num3;
        double num10 = quat.W * num;
        double num11 = quat.W * num2;
        double num12 = quat.W * num3;

        MVector3 result = new MVector3();
        result.setX((1f - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z);
        result.setY((num7 + num12) * vec.X + (1f - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z);
        result.Z = (num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1f - (num4 + num5)) * vec.Z;
        return result;
    }


    /// <summary>
    /// Return length of quaternion.
    /// </summary>
    public static float length(MQuaternion quat) {

        double norm2 = quat.getX() * quat.getX() + quat.getY() * quat.getY() + quat.getZ() * quat.getZ() + quat.getW() * quat.getX();
        if (!(norm2 <= Double.MAX_VALUE)) {
            // Do this the slow way to avoid squaring large
            // numbers since the length of many quaternions is
            // representable even if the squared length isn't.  Of
            // course some lengths aren't representable because
            // the length can be up to twice as big as the largest
            // coefficient.

            double max = Math.max(Math.max(Math.abs(quat.X), Math.abs(quat.Y)),
                    Math.max(Math.abs(quat.Z), Math.abs(quat.W)));

            double x = quat.X / max;
            double y = quat.Y / max;
            double z = quat.Z / max;
            double w = quat.W / max;


            double smallLength = Math.sqrt(x * x + y * y + z * z + w * w);
            // Return length of this smaller vector times the scale we applied originally.

            return (float) (smallLength * max);
        }
        return (float) Math.sqrt(norm2);
    }

    /// <summary>
    /// Scale this quaternion by a scalar.
    /// </summary>
    /// <param name="scale">Value to scale by.
    static void scale(MQuaternion quat, float scale) {
        quat.X *= scale;
        quat.Y *= scale;
        quat.Z *= scale;
        quat.W *= scale;
    }


    /// <summary>
    /// Creates the inverse quaternion
    /// </summary>
    /// <param name="quaternion"></param>
    /// <returns></returns>
    public static MQuaternion inverse(MQuaternion quaternion) {
        //Inverse = Conjugate / Norm Squared
        MQuaternion inverted = new MQuaternion(-quaternion.X, -quaternion.Y, -quaternion.Z, quaternion.W);

        double norm = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;

        inverted.X /= norm;
        inverted.Y /= norm;
        inverted.Z /= norm;
        inverted.W /= norm;
        return inverted;
    }

    /// <summary>
    /// Smoothly interpolate between the two given quaternions using Spherical
    /// Linear Interpolation (SLERP).
    /// </summary>
    /// <param name="from">First quaternion for interpolation.
    /// <param name="to">Second quaternion for interpolation.
    /// <param name="t">Interpolation coefficient.
    /// <param name="useShortestPath">If true, Slerp will automatically flip the sign of
    ///     the destination Quaternion to ensure the shortest path is taken.
    /// <returns>SLERP-interpolated quaternion between the two given quaternions.</returns>
    public static MQuaternion slerp(MQuaternion from, MQuaternion to, float t, boolean useShortestPath) {

        double cosOmega;
        double scaleFrom, scaleTo;

        // Normalize inputs and stash their lengths
        double lengthFrom = MQuaternionExtensions.length(from);
        double lengthTo = MQuaternionExtensions.length(to);
        MQuaternionExtensions.scale(from, (float) lengthFrom);
        MQuaternionExtensions.scale(to, (float) lengthFrom);

        // Calculate cos of omega.
        cosOmega = from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W;

        if (useShortestPath) {
            // If we are taking the shortest path we flip the signs to ensure that
            // cosOmega will be positive.
            if (cosOmega < 0.0) {
                cosOmega = -cosOmega;
                to.X = -to.X;
                to.Y = -to.Y;
                to.Z = -to.Z;
                to.W = -to.W;
            }
        } else {
            // If we are not taking the UseShortestPath we clamp cosOmega to
            // -1 to stay in the domain of MathF.Acos below.
            if (cosOmega < -1.0) {
                cosOmega = -1.0f;
            }
        }

        // Clamp cosOmega to [-1,1] to stay in the domain of MathF.Acos below.
        // The logic above has either flipped the sign of cosOmega to ensure it
        // is positive or clamped to -1 already.  We only need to worry about the
        // upper limit here.
        if (cosOmega > 1.0) {
            cosOmega = 1.0f;
        }


        // The mainline algorithm doesn't work for extreme
        // cosine values.  For large cosine we have a better
        // fallback hence the asymmetric limits.
        final double maxCosine = 1.0f - 1e-6f;
        final double minCosine = 1e-10f - 1.0f;

        // Calculate scaling coefficients.
        if (cosOmega > maxCosine) {
            // Quaternions are too close - use linear interpolation.
            scaleFrom = 1.0f - t;
            scaleTo = t;
        } else if (cosOmega < minCosine) {
            // Quaternions are nearly opposite, so we will pretend to
            // is exactly -from.
            // First assign arbitrary perpendicular to "to".
            to = new MQuaternion(-from.Y, from.X, -from.W, from.Z);

            double theta = t * Math.PI;

            scaleFrom = Math.cos(theta);
            scaleTo = Math.sin(theta);
        } else {
            // Standard case - use SLERP interpolation.
            double omega = Math.acos(cosOmega);
            double sinOmega = Math.sqrt(1.0f - cosOmega * cosOmega);
            scaleFrom = Math.sin((1.0f - t) * omega) / sinOmega;
            scaleTo = Math.sin(t * omega) / sinOmega;
        }

        // We want the magnitude of the output quaternion to be
        // multiplicatively interpolated between the input
        // magnitudes, i.e. lengthOut = lengthFrom * (lengthTo/lengthFrom)^t
        //                            = lengthFrom ^ (1-t) * lengthTo ^ t

        double lengthOut = lengthFrom * Math.pow(lengthTo / lengthFrom, t);
        scaleFrom *= lengthOut;
        scaleTo *= lengthOut;

        return new MQuaternion(scaleFrom * from.X + scaleTo * to.X,
                scaleFrom * from.Y + scaleTo * to.Y,
                scaleFrom * from.Z + scaleTo * to.Z,
                scaleFrom * from.W + scaleTo * to.W);
    }


    /// <summary>
    ///   <para>Returns the angle in degrees between two rotations /a/ and /b/.</para>
    ///   https://gist.github.com/aeroson/043001ca12fe29ee911e
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static double angle(MQuaternion a, MQuaternion b) {
        double f = MQuaternionExtensions.dot(a, b);
        return Math.acos(Math.min(Math.abs(f), 1f)) * 2f * Rad2Deg;
    }


    /// <summary>
    ///   <para>The dot product between two rotations.</para>
    ///   https://gist.github.com/aeroson/043001ca12fe29ee911e
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static double dot(MQuaternion a, MQuaternion b) {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
    }

    /// <summary>
    /// Creates euler angles (in degree) form the given quaternion
    /// Source code from: https://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public static MVector3 ToEuler(MQuaternion q) {
        MVector3 euler = new MVector3();

        // if the input quaternion is normalized, this is exactly one. Otherwise, this acts as a correction factor for the quaternion's not-normalizedness
        double unit = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);

        // this will have a magnitude of 0.5 or greater if and only if this is a singularity case
        double test = q.X * q.W - q.Y * q.Z;

        if (test > 0.4995f * unit) // singularity at north pole
        {
            euler.X = Math.PI / 2;
            euler.Y = 2f * Math.atan2(q.Y, q.X);
            euler.Z = 0;
        } else if (test < -0.4995f * unit) // singularity at south pole
        {
            euler.X = -Math.PI / 2;
            euler.Y = -2f * Math.atan2(q.Y, q.X);
            euler.Z = 0;
        } else // no singularity - this is the majority of cases
        {
            euler.X = Math.asin(2f * (q.W * q.X - q.Y * q.Z));
            euler.Y = Math.atan2(2f * q.W * q.Y + 2f * q.Z * q.X, 1 - 2f * (q.X * q.X + q.Y * q.Y));
            euler.Z = Math.atan2(2f * q.W * q.Z + 2f * q.X * q.Y, 1 - 2f * (q.Z * q.Z + q.X * q.X));
        }

        // all the math so far has been done in radians. Before returning, we convert to degrees...
        euler = MQuaternionExtensions.multiply(euler, Rad2Deg);

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
    public static MQuaternion FromEuler(MVector3 euler) {
        double xOver2 = euler.X * Deg2Rad * 0.5f;
        double yOver2 = euler.Y * Deg2Rad * 0.5f;
        double zOver2 = euler.Z * Deg2Rad * 0.5f;

        double sinXOver2 = Math.sin(xOver2);
        double cosXOver2 = Math.cos(xOver2);
        double sinYOver2 = Math.sin(yOver2);
        double cosYOver2 = Math.cos(yOver2);
        double sinZOver2 = Math.sin(zOver2);
        double cosZOver2 = Math.cos(zOver2);

        MQuaternion result = new MQuaternion();
        result.X = cosYOver2 * sinXOver2 * cosZOver2 + sinYOver2 * cosXOver2 * sinZOver2;
        result.Y = sinYOver2 * cosXOver2 * cosZOver2 - cosYOver2 * sinXOver2 * sinZOver2;
        result.Z = cosYOver2 * cosXOver2 * sinZOver2 - sinYOver2 * sinXOver2 * cosZOver2;
        result.W = cosYOver2 * cosXOver2 * cosZOver2 + sinYOver2 * sinXOver2 * sinZOver2;

        return result;
    }

    /// <summary>
    /// Returns a deep copy of the quaternion
    /// </summary>
    /// <param name="quaternion"></param>
    /// <returns></returns>
    public static MQuaternion Clone(MQuaternion quaternion) {
        if (quaternion != null)
            return new MQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        else
            return null;
    }
}
