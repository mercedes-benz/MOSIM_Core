// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using UnityEngine;

namespace MMIUnity
{
    /// <summary>
    /// Class instantiates Unity colliders based on the MCollider class 
    /// </summary>
    public static class UnityColliderFactory
    {
        /// <summary>
        /// Creates a Unity collider based on the MCollider and a transform
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Collider CreateCollider(MCollider collider, MTransform transform)
        {
            if (collider == null || transform == null)
                return null;

            GameObject result = null;
            switch (collider.Type)
            {
                case MColliderType.Box:
                    MBoxColliderProperties mboxCollider = collider.BoxColliderProperties;

                    if (mboxCollider == null)
                    {
                        Debug.Log("Box collider is null");
                        return null;
                    }

                    GameObject boxGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    boxGameObject.transform.position = transform.Position.ToVector3();
                    boxGameObject.transform.rotation = transform.Rotation.ToQuaternion();

                    //Assign the properties of the collider
                    BoxCollider boxCollider = boxGameObject.GetComponent<BoxCollider>();
                    boxCollider.center = collider.PositionOffset.ToVector3();
                    boxCollider.size = mboxCollider.Size.ToVector3();

                    result = boxGameObject;
                    break;

                case MColliderType.Sphere:
                    MSphereColliderProperties mSphereCollider = collider.SphereColliderProperties;

                    if (mSphereCollider == null)
                    {
                        Debug.Log("Sphere collider is null");
                        return null;
                    }

                    GameObject sphereGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphereGameObject.transform.position = transform.Position.ToVector3();
                    sphereGameObject.transform.rotation = transform.Rotation.ToQuaternion();

                    SphereCollider sphereCollider = sphereGameObject.GetComponent<SphereCollider>();
                    sphereCollider.center = collider.PositionOffset.ToVector3();
                    sphereCollider.radius = (float)mSphereCollider.Radius;

                    result = sphereGameObject;
                    break;

                case MColliderType.Capsule:
                    MCapsuleColliderProperties mCapsuleCollider = collider.CapsuleColliderProperties;

                    if (mCapsuleCollider == null)
                    {
                        Debug.Log("Capsule collider is null");
                        return null;
                    }

                    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsule.transform.position = transform.Position.ToVector3();
                    capsule.transform.rotation = transform.Rotation.ToQuaternion();

                    CapsuleCollider capsuleCollider = capsule.GetComponent<CapsuleCollider>();
                    capsuleCollider.center = collider.PositionOffset.ToVector3();
                    capsuleCollider.radius = (float)mCapsuleCollider.Radius;
                    capsuleCollider.height = (float)mCapsuleCollider.Height;

                    result = capsule;

                    break;

                case MColliderType.Mesh:

                    MMeshColliderProperties mMeshCollider = collider.MeshColliderProperties;

                    if (mMeshCollider == null)
                    {
                        Debug.Log("Mesh collider is null");
                        return null;
                    }



                    //To do
                    break;
            }

            return result.GetComponent<Collider>();
        }
    }

}
