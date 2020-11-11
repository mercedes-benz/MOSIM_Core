// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;


namespace MMIStandard
{
    /// <summary>
    /// Class contains several extensions for MPathConstraints
    /// </summary>
    public static class MPathConstraintExtensions
    {
        /// <summary>
        /// Returns a list of MTRansform based on the MPathConstraint
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

                //Use the parent to constraint if defined
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
                    //Use the translation/rotation limits
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


        /// <summary>
        /// Returns a list of MVector3 based on the MPathConstraint
        /// </summary>
        /// <param name="pathConstraint"></param>
        /// <returns></returns>
        public static List<MVector3> GetMVector3List(this MPathConstraint pathConstraint)
        {
            //Create a list for storing the full trajectory
            List<MVector3> list = new List<MVector3>();

            foreach (MGeometryConstraint mg in pathConstraint.PolygonPoints)
            {
                MVector3 p = new MVector3();

                //Use the parent to constraint if defined
                if (mg.ParentToConstraint != null)
                    p = mg.ParentToConstraint.Position.Clone();

                else
                    p = mg.TranslationConstraint.GetVector3();

                list.Add(p);
            }

            return list;
        }


        /// <summary>
        /// Returns a list of MQuaternion based on the MPathConstraint
        /// </summary>
        /// <param name="pathConstraint"></param>
        /// <returns></returns>
        public static List<MQuaternion> GetMQuaternionList(this MPathConstraint pathConstraint)
        {
            //Create a list for storing the full trajectory
            List<MQuaternion> list = new List<MQuaternion>();

            foreach (MGeometryConstraint mg in pathConstraint.PolygonPoints)
            {
                MQuaternion r = new MQuaternion();

                //Use the parent to constraint if defined
                if (mg.ParentToConstraint != null)
                    r = mg.ParentToConstraint.Rotation.Clone();

                else
                    r = mg.RotationConstraint.GetQuaternion();

                list.Add(r);
            }

            return list;
        }
    }
}
