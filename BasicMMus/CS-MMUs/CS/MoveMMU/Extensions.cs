// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace MoveMMU
{
    public static class Extensions
    {
        /// <summary>
        /// Maps the path constraint to list of MVector2
        /// </summary>
        /// <param name="pathConstraint"></param>
        /// <returns></returns>
        public static List<MVector3> GetMVector3List(this MPathConstraint pathConstraint)
        {
            //Create a list for storing the full trajectory
            List<MVector3> list = new List<MVector3>();

            foreach (MGeometryConstraint mg in pathConstraint.PolygonPoints)
            {
                list.Add(mg.TranslationConstraint.GetVector3());
            }

            return list;
        }

        /// <summary>
        /// Returns a list of MTRansform 
        /// </summary>
        /// <param name="pathConstraint"></param>
        /// <returns></returns>
        public static List<MTransform> GetMTransformList(this MPathConstraint pathConstraint)
        {
            //Create a list for storing the full trajectory
            List<MTransform> list = new List<MTransform>();

            foreach (MGeometryConstraint mg in pathConstraint.PolygonPoints)
            {
                MTransform t = null;

                if (mg.ParentToConstraint != null)
                {
                    t = new MTransform
                    {
                        ID = "",
                        Position = mg.ParentToConstraint.Position,
                        Rotation = mg.ParentToConstraint.Rotation
                    };
                }

                else
                {
                    t = new MTransform
                    {
                        ID = "",
                        Position = mg.TranslationConstraint.GetVector3(),
                        Rotation = MQuaternionExtensions.FromEuler(mg.TranslationConstraint.GetVector3())
                    };
                }

  
                list.Add(t);
            }

            return list;
        }

        public static String GetValue(this Dictionary<string,string>dict, params string[] keys)
        {
            foreach(string key in keys)
            {
                if (dict.ContainsKey(key))
                    return dict[key];
            }

            return null;
        }

    }
}
