// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System;
using System.Collections.Generic;
using UnityEngine;


namespace MMIUnity.TargetEngine.Scene
{
    //Function producting union of all meshses beloniging to the given MMISceneObject, ommiting disabled meshes and meshes belonging to children MMISceneObjects
    public static class Utils3D
    {
        static Material material;
        static MaterialPropertyBlock mpb, mpb1;
        static Color cok, cok1;
        static bool GhostMaterialReady = false;

        public static void prepareGhostMaterial()
        {
            if (GhostMaterialReady)
                return;
            material = new Material(Shader.Find("Transparent/Diffuse"));
            mpb = new MaterialPropertyBlock();
            cok = new Color(Color.green.r, Color.green.g, Color.green.b, 0.3f);
            mpb.SetColor("_Color", cok);

            mpb1 = new MaterialPropertyBlock();
            cok1 = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.3f);
            mpb1.SetColor("_Color", cok1);

            GhostMaterialReady = true;
        }

        public static Mesh ObjectGhoster(MMISceneObject sceneObject)
        {
            List<Vector3> MeshVertices = new List<Vector3>();
            List<int> MeshTriangles = new List<int>();
            Mesh myMesh = new Mesh();
            var mf = sceneObject.GetComponentsInChildren<MeshFilter>();
            MeshVertices.Clear();
            MeshTriangles.Clear();
            myMesh.Clear();
            int offset = 0;
            for (int i = 0; i < mf.Length; i++)
                if (mf[i].GetComponentInParent<MeshRenderer>() != null && mf[i].GetComponentInParent<MeshRenderer>().enabled &&
                       mf[i].GetComponentInParent<MMISceneObject>() != null && ((sceneObject.Type == MMISceneObject.Types.Group) || (sceneObject.Type!=MMISceneObject.Types.Group && mf[i].GetComponentInParent<MMISceneObject>().GetParentMMIScenObject() == sceneObject)))
                {
                    var rotation = (mf[i].transform.gameObject != sceneObject.gameObject ? mf[i].transform.localRotation : new Quaternion());
                    var position = (mf[i].transform.gameObject != sceneObject.gameObject ? mf[i].transform.localPosition : new Vector3());
                    for (int j = 0; j < mf[i].sharedMesh.vertices.Length; j++)
                    {
                        var vert = position + rotation * Vector3.Scale(mf[i].sharedMesh.vertices[j], mf[i].transform.localScale);
                        if (mf[i].transform.gameObject != sceneObject.gameObject)
                        {
                            Transform T = mf[i].transform.parent;
                            while (T.gameObject != sceneObject.gameObject)
                            {
                                rotation = T.localRotation;
                                position = mf[i].transform.localPosition;
                                vert = position + rotation * Vector3.Scale(vert, T.localScale);
                                T = T.transform.parent;
                            }
                        }
                        MeshVertices.Add(vert);
                    }
                    for (int j = 0; j < mf[i].sharedMesh.triangles.Length; j++)
                        MeshTriangles.Add(mf[i].sharedMesh.triangles[j] + offset);
                    offset += mf[i].sharedMesh.vertices.Length;
                }
            myMesh.vertices = MeshVertices.ToArray();
            myMesh.triangles = MeshTriangles.ToArray();
            myMesh.RecalculateNormals();
            myMesh.RecalculateTangents();
            myMesh.RecalculateBounds();
            myMesh.MarkModified();
            return myMesh;
        }

        public static void ShowGhosts(Mesh ghostMesh, MMISceneObject targetObject, int matsel=0)
        {
            if (ghostMesh != null && ghostMesh.vertexCount > 0)
            {
                prepareGhostMaterial();
                /*
                material = new Material(Shader.Find("Transparent/Diffuse"));
                mpb = new MaterialPropertyBlock();
                cok = new Color(Color.green.r, Color.green.g, Color.green.b, 0.3f);
                mpb.SetColor("_Color", cok);
                */
                var scale = new Vector3(1, 1, 1);
                var matrix = Matrix4x4.TRS(targetObject.transform.position, targetObject.transform.rotation, scale);
                Graphics.DrawMesh(ghostMesh, matrix, material, 0, null, 0, (matsel==0? mpb: mpb1));
            }
        }
    }
}
