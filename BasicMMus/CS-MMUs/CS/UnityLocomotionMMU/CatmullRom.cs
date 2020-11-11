// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using UnityEngine;

namespace UnityLocomotionMMU
{
    /// <summary>
    /// Implementation of CatmullRom polynomial interpolation
    /// </summary>
    public class CatmullRom
    {
        /// <summary>
        /// Gets the point between p1 and p2 using the catmull rom curve
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static Vector2 ComputePoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            Vector2 a = 2f * p1;
            Vector2 b = p2 - p0;
            Vector2 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector2 d = -p0 + 3f * p1 - 3f * p2 + p3;

            return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        }

        /// <summary>
        /// Gets the point between p1 and p2 using the catmull rom curve
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static float ComputePoint(float p0, float p1, float p2, float p3, float t)
        {
            float a = 2f * p1;
            float b = p2 - p0;
            float c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            float d = -p0 + 3f * p1 - 3f * p2 + p3;

            return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        }
    }

}
