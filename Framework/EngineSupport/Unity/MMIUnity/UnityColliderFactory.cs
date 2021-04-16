// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Linq;
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
            //Skip if the collider or the transform are null
            if (collider == null || transform == null)
                return null;

            GameObject result = null;

            //Create gameobject and corresponding collider based on the defined type
            switch (collider.Type)
            {
                //Create a box collider
                case MColliderType.Box:
                    MBoxColliderProperties mboxCollider = collider.BoxColliderProperties;

                    if (mboxCollider == null)
                    {
                        Debug.Log("Box collider is null");
                        return null;
                    }

                    //Create a gameobject for the box collider
                    GameObject boxGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    boxGameObject.transform.position = transform.Position.ToVector3();
                    boxGameObject.transform.rotation = transform.Rotation.ToQuaternion();

                    //Assign the properties of the collider
                    BoxCollider boxCollider = boxGameObject.GetComponent<BoxCollider>();
                    boxCollider.center = collider.PositionOffset.ToVector3();
                    boxCollider.size = mboxCollider.Size.ToVector3();

                    //Assign the resulting object
                    result = boxGameObject;
                    break;

                //Create a sphere collider
                case MColliderType.Sphere:
                    MSphereColliderProperties mSphereCollider = collider.SphereColliderProperties;

                    if (mSphereCollider == null)
                    {
                        Debug.Log("Sphere collider is null");
                        return null;
                    }

                    //Create the corresponding gameobject which contains the sphere collider
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

                    GameObject capsuleGameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsuleGameObject.transform.position = transform.Position.ToVector3();
                    capsuleGameObject.transform.rotation = transform.Rotation.ToQuaternion();

                    CapsuleCollider capsuleCollider = capsuleGameObject.GetComponent<CapsuleCollider>();
                    capsuleCollider.center = collider.PositionOffset.ToVector3();
                    capsuleCollider.radius = (float)mCapsuleCollider.Radius;
                    capsuleCollider.height = (float)mCapsuleCollider.Height;

                    result = capsuleGameObject;

                    break;

                case MColliderType.Cylinder:
                    MCylinderColliderProperties mCylinderCollider = collider.CylinderColliderProperties;

                    if (mCylinderCollider == null)
                    {
                        Debug.Log("Cylinder collider is null");
                        return null;
                    }

                    GameObject cylinderGameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cylinderGameObject.transform.position = transform.Position.ToVector3();
                    cylinderGameObject.transform.rotation = transform.Rotation.ToQuaternion();

                    CapsuleCollider capsuleCollider2 = cylinderGameObject.GetComponent<CapsuleCollider>();
                    capsuleCollider2.center = collider.PositionOffset.ToVector3();
                    capsuleCollider2.radius = (float)mCylinderCollider.Radius;
                    capsuleCollider2.height = (float)mCylinderCollider.Height;

                    result = cylinderGameObject;

                    break;

                case MColliderType.Mesh:

                    MMeshColliderProperties mMeshCollider = collider.MeshColliderProperties;

                    if (mMeshCollider == null)
                    {
                        Debug.Log("Mesh collider is null");
                        return null;
                    }


                    GameObject meshObj = new GameObject();
                    MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                    MeshRenderer renderer = meshObj.AddComponent<MeshRenderer>();

                    meshFilter.mesh.SetVertices(mMeshCollider.Vertices.Select(s => s.ToVector3()).ToList());
                    meshFilter.mesh.SetTriangles(mMeshCollider.Triangles,0);

                    MeshCollider meshCollider = meshObj.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.mesh;

                    result = meshObj;
                    break;
            }

            return result.GetComponent<Collider>();
        }
    }

}
