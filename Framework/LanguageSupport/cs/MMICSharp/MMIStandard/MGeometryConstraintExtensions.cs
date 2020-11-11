// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

namespace MMIStandard
{
    /// <summary>
    /// Extensions for the MGeometryConstraint
    /// </summary>
    public static class MGeometryConstraintExtensions
    {
        /// <summary>
        /// Returns the global position of a given MGeometry constraint
        /// </summary>
        /// <param name="constraint"></param>
        /// <param name="sceneAccess"></param>
        /// <returns></returns>
        public static MVector3 GetGlobalPosition(this MGeometryConstraint constraint, MSceneAccess.Iface sceneAccess)
        {
            MTransform parentTransform = sceneAccess.GetTransformByID(constraint.ParentObjectID);

            if(parentTransform != null)
            {
                if (constraint.ParentToConstraint != null)
                    return parentTransform.TransformPoint(constraint.ParentToConstraint.Position);
                else
                    return parentTransform.Position;
            }
            //No parent defined
            else
            {
                if(constraint.ParentToConstraint != null)
                    return constraint.ParentToConstraint.Position;
            }

            return null;
        }


        /// <summary>
        /// Returns the global rotation of the specified constraint
        /// </summary>
        /// <param name="constraint"></param>
        /// <param name="sceneAccess"></param>
        /// <returns></returns>
        public static MQuaternion GetGlobalRotation(this MGeometryConstraint constraint, MSceneAccess.Iface sceneAccess)
        {
            MTransform parentTransform = sceneAccess.GetTransformByID(constraint.ParentObjectID);

            if (parentTransform != null)
            {
                if (constraint.ParentToConstraint != null)
                    return parentTransform.TransformRotation(constraint.ParentToConstraint.Rotation);
                else
                    return parentTransform.Rotation;
            }
            //No parent defined
            else
            {
                if (constraint.ParentToConstraint != null)
                    return constraint.ParentToConstraint.Rotation;
            }

            return null;
        }
    }
}