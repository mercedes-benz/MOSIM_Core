// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using UnityEngine;

namespace MMIUnity.TargetEngine
{
    /// <summary>
    /// Class contains several extensions for the Unity transforms
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Returns the local position and ignores the scale
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Vector3 GetLocalPositionScaleIndependent(this Transform transform)
        {
            return new Vector3(transform.localPosition.x * transform.parent.lossyScale.x, transform.localPosition.y * transform.parent.lossyScale.y, transform.localPosition.z * transform.parent.lossyScale.z);
        }
    }
}
