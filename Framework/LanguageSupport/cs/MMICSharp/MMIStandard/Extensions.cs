// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;

namespace MMIStandard
{
    public static class Extensions
    {

        #region clone methods

        /// <summary>
        /// Returns a deep copy of a MJoint
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static MJoint Clone(this MJoint bone)
        {
            return new MJoint(bone.ID, bone.Type, bone.Position.Clone(), bone.Rotation.Clone())
            {
                Parent = bone.Parent,
            };
        }



        /// <summary>
        /// Returns a deep copy of a MTransform
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static MTransform Clone(this MTransform original)
        {
            return new MTransform(original.ID, original.Position.Clone(), original.Rotation.Clone())
            {
                Parent = original.Parent
            };
        }

        public static MSceneObject Clone(this MSceneObject original)
        {
            MSceneObject clone = new MMIStandard.MSceneObject()
            {
                ID = original.ID,
                Name = original.Name,
                Collider = original.Collider!=null? original.Collider.Clone(): null,
                Properties = original.Properties != null ? original.Properties.Clone():null,
                Transform = original.Transform!=null?original.Transform.Clone():null,
                Mesh = original.Mesh!=null?original.Mesh.Clone():null
            };

            return clone;
        }


        public static MCollider Clone(this MCollider original)
        {
            MCollider clone = new MCollider()
            {
                Type = original.Type,
                BoxColliderProperties = original.BoxColliderProperties,
                SphereColliderProperties = original.SphereColliderProperties,
                CapsuleColliderProperties = original.CapsuleColliderProperties,
                MeshColliderProperties = original.MeshColliderProperties,
                ConeColliderProperties = original.ConeColliderProperties,
                CylinderColliderProperties = original.CylinderColliderProperties,
                ID = original.ID,
                //To do
                Colliders = original.Colliders,
                PositionOffset = original.PositionOffset.Clone(),
                RotationOffset = original.RotationOffset.Clone(),
                Properties = original.Properties.Clone()
            };

            return clone;
        }

        /// <summary>
        /// To DO 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static MMesh Clone(this MMesh original)
        {
            MMesh clone = new MMIStandard.MMesh()
            {
                ID = original.ID,
                Properties = original.Properties,
                Triangles = new List<int>(original.Triangles),
                Vertices = new List<MVector3>(original.Vertices)
            };
            
            return clone;
        }

        public static Dictionary<string,string> Clone(this Dictionary<string,string> original)
        {
            Dictionary<string, string> clone = new Dictionary<string, string>();

            if(original != null)
            {
                foreach(KeyValuePair<string,string> pair in original)
                {
                    clone.Add(pair.Key, pair.Value);
                }
            }

            return clone;
        }

        #endregion


        public static Dictionary<MJointType, string> Invert(this Dictionary<string, MJointType> boneMapping)
        {
            Dictionary<MJointType, string> result = new Dictionary<MJointType, string>();


            foreach (var entry in boneMapping)
            {
                if (!result.ContainsKey(entry.Value))
                    result.Add(entry.Value, entry.Key);
            }

            return result;
        }


        /// <summary>
        /// Returns a hash of the scene manipulations
        /// Can be utilized to check if there have been any differences
        /// </summary>
        /// <param name="sceneUpdates"></param>
        /// <returns></returns>
        public static int GetSceneHash(this MSceneUpdate sceneUpdates)
        {
            int hash = sceneUpdates.GetHashCode();

            return hash;
        }

    }
}
