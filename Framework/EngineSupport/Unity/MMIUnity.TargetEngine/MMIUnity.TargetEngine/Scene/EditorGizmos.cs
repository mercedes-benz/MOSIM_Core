// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System;
using UnityEngine;
using System.IO;

namespace MMIUnity.TargetEngine.Scene
{
    public static class TWalkPointGizmo
    {
        public static Mesh myMesh;
        public static Material material;
        private static float pixelsPerUnit = 1000;
        public static Texture2D feetTexture;
        public static MaterialPropertyBlock mpb;
        public static Vector3 scale = new Vector3(1, 1, 1);
        public static bool isReady = false;

        /*
        public static bool isReady()
        {
            return feetTexture!=null;
        }
        */
        public static bool LoadTexture(bool forcereload = false)
        {
            if (!forcereload && TWalkPointGizmo.isReady)
                return true;

            if (MMISettings.Instance == null)
                return false;

            if (!MMISettings.Instance.texturesFolder.EndsWith("/"))
                MMISettings.Instance.texturesFolder += "/";
            string feetpng = MMISettings.BasePath() + MMISettings.Instance.texturesFolder + MMISettings.feetIcon;

            if (!File.Exists(feetpng))
                return false;

            feetTexture = new Texture2D(2, 2);
            feetTexture.LoadImage(File.ReadAllBytes(feetpng));
            pixelsPerUnit = feetTexture.height / 0.288f;
            if (myMesh == null)
                myMesh = CreateQuad();
            if (material == null)
                material = new Material(Shader.Find("Sprites/Default"));
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", Color.black);
            }
            mpb.SetTexture("_MainTex", feetTexture);

            scale = new Vector3(feetTexture.width, feetTexture.height, 1) / pixelsPerUnit;
            TWalkPointGizmo.isReady = true;
            return true;
        }

        //creates mesh plane 1x1x0
        private static Mesh CreateQuad()
        {
            var mesh = new Mesh
            {
                vertices = new[]
                         {
                                new Vector3(-.5f, -.5f, 0),
                                new Vector3(-.5f, +.5f, 0),
                                new Vector3(+.5f, +.5f, 0),
                                new Vector3(+.5f, -.5f, 0),
                             },

                normals = new[]
                        {
                                    Vector3.forward,
                                    Vector3.forward,
                                    Vector3.forward,
                                    Vector3.forward,
                            },

                triangles = new[] { 0, 1, 2, 2, 3, 0 },

                uv = new[]
                        {
                                new Vector2(0, 0),
                                new Vector2(0, 1),
                                new Vector2(1, 1),
                                new Vector2(1, 0),
                            }
            };
            return mesh;
        }

        public static bool Draw(MMISceneObject sceneObject, MMISceneObject.TConstraintIndex index)
        {
            if (!(index.index <= sceneObject.Constraints.Count && sceneObject.Constraints[index.index].__isset.PathConstraint && index.pathIndex < sceneObject.Constraints[index.index].PathConstraint.PolygonPoints.Count))
                return false;

            if (sceneObject.Constraints[index.index].ID != "WalkTarget")
                return false;

            if (!TWalkPointGizmo.isReady)
                if (!TWalkPointGizmo.LoadTexture())
                    return false;

            var p = sceneObject.Constraints[index.index].PathConstraint.PolygonPoints[index.pathIndex].ParentToConstraint.Position.ToVector3();     //rotation of the sprite
            var r = sceneObject.Constraints[index.index].PathConstraint.PolygonPoints[index.pathIndex].ParentToConstraint.Rotation.ToQuaternion() * Quaternion.Euler(90, 0, 0);

            var matrix = Matrix4x4.TRS(sceneObject.transform.position + sceneObject.transform.rotation * p, sceneObject.transform.rotation * r, TWalkPointGizmo.scale);
            Graphics.DrawMesh(TWalkPointGizmo.myMesh, matrix, TWalkPointGizmo.material, 0, null, 0, TWalkPointGizmo.mpb);
            return true;
        }

        public static bool Draw(MMISceneObject sceneObject)
        {
            if (sceneObject.Type != MMISceneObject.Types.WalkTarget)
                return false;

            if (!TWalkPointGizmo.isReady)
                if (!TWalkPointGizmo.LoadTexture())
                    return false;

            var r = Quaternion.Euler(90, 0, 0);

            var matrix = Matrix4x4.TRS(sceneObject.transform.position, sceneObject.transform.rotation * r, TWalkPointGizmo.scale);
            Graphics.DrawMesh(TWalkPointGizmo.myMesh, matrix, TWalkPointGizmo.material, 0, null, 0, TWalkPointGizmo.mpb);
            return true;
        }

    }
}
