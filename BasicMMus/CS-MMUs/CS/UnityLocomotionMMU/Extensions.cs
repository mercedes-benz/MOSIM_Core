// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLocomotionMMU
{
    /// <summary>
    /// Class comprises several extenion method related to Vector2
    /// </summary>
    public static class Vector2Extensions
    {

        /// <summary>
        /// Converts the vector2 to a two dimensional array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float[] ToArray(this Vector2 v)
        {
            return new float[] { v.x, v.y };
        }


        /// <summary>
        /// Maps the path constraint to list of MVector2
        /// </summary>
        /// <param name="pathConstraint"></param>
        /// <returns></returns>
        public static List<Vector2> GetVector2List(this MPathConstraint pathConstraint)
        {
            //Create a list for storing the full trajectory
            List<Vector2> list = new List<Vector2>();

            foreach (MGeometryConstraint mg in pathConstraint.PolygonPoints)
            {
                list.Add(new Vector2((float)mg.ParentToConstraint.Position.X, (float)mg.ParentToConstraint.Position.Z));
            }

            return list;
        }
    }
}
