package Extensions;

import MMIStandard.MVector3;

import java.util.ArrayList;
import java.util.List;

public class MVector3Extensions {
/*
		Class which extends MVector3
	*/

    //	Method which converts double values to MVector3
    public static MVector3 toMVector3(List<Double> values) {
        return new MVector3(values.get(0), values.get(1), values.get(2));
    }

    //	Method calculates the euclidean distance between the two vectors
    public static float euclideanDistance(MVector3 vec1, MVector3 vec2) {
        return magnitude(subtract(vec1, vec2));
    }

    // Method subtracts two given vectors
    public static MVector3 subtract(MVector3 vector1, MVector3 vector2) {
        return new MVector3(vector1.getX() - vector2.getX(), vector1.getY() - vector2.getY(), vector1.getZ() - vector2.getZ());
    }

    //	Method calculates the magnitude of a vector
    public static float magnitude(MVector3 vector) {
        return (float) Math.sqrt(Math.pow(vector.getX(), 2) + Math.pow(vector.getY(), 2) + Math.pow(vector.getZ(), 2));
    }


    /// <summary>
    /// Returns the values of the vector as list of doubles
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static List<Double> getValues(MVector3 vector) {
        ArrayList<Double> temp = new ArrayList<>();
        temp.add(vector.getX());
        temp.add(vector.getY());
        temp.add(vector.getZ());
        return temp;
    }

    /// <summary>
    /// Normalizes the given vector
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static MVector3 normalize(MVector3 vector) {
        return MVector3Extensions.divide(vector, MVector3Extensions.magnitude(vector));
    }

    /// <summary>
    /// Returns a new vector which is the result of the Multiplication of the specified vector and a scalar value
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static MVector3 multiply(MVector3 vector, double scalar) {
        return new MVector3(vector.getX() * scalar, vector.getY() * scalar, vector.getZ() * scalar);
    }

    /// <summary>
    /// The euclidean distance between two vectors
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static double distance(MVector3 vector1, MVector3 vector2) {
        return MVector3Extensions.magnitude(MVector3Extensions.subtract(vector1, vector2));
    }


    /// <summary>
    /// Adds a vector on top of the current one and returns the result as new vector
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static MVector3 add(MVector3 v1, MVector3 v2) {
        return new MVector3(v1.getX() + v2.getX(), v1.getY() + v2.getY(), v1.getZ() + v2.getZ());
    }

    /// <summary>
    /// Performs a subtraction
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>

    /// <summary>
    /// Performs a division
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static MVector3 divide(MVector3 vector, float scalar) {
        return new MVector3(vector.getX() / scalar, vector.getY() / scalar, vector.getZ() / scalar);
    }


    /// <summary>
    /// Lerps the vector
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static MVector3 lerp(MVector3 from, MVector3 to, float t) {
        return MVector3Extensions.add(from, MVector3Extensions.multiply(MVector3Extensions.subtract(to, from), t));
    }

}
