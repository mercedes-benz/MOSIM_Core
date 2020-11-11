// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity.TargetEngine
{
    /// <summary>
    /// Class contains serveral functions for drawing
    /// </summary>
    public class DrawingUtils
    {
        /// <summary>
        /// Method draws a point based on a list of double values
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static GameObject DrawPoint(List<double> data)
        {
            if (data.Count == 3)
                return DrawingUtils.DrawPoint3D(data.ToVector3(), Color.red, 0.1f);
            if (data.Count == 2)
                return DrawingUtils.DrawPoint2D(new Vector2((float)data[0], (float)data[1]), Color.red, 0.1f);

            return null;
        }

        /// <summary>
        /// Draws a line based on a list of 2D coordinates
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static GameObject DrawLine2D(List<double> data)
        {
            List<Vector3> line = new List<Vector3>();
            for (int i = 0; i < data.Count; i += 2)
            {
                Vector3 p = new Vector3((float)data[i], 0.005f, (float)data[i + 1]);
                line.Add(p);
            }

            GameObject root = DrawLine(line, Color.red);

            return root;
        }


        /// <summary>
        /// Draws a line based on a list of 3D coordinates
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static GameObject DrawLine3D(List<double> data)
        {
            List<Vector3> line = new List<Vector3>();
            for (int i = 0; i < data.Count; i += 3)
            {
                Vector3 p = new Vector3((float)data[i], (float)data[i + 1], (float)data[i + 2]);
                line.Add(p);
            }

            GameObject root = DrawLine(line, Color.red);

            return root;
        }


        /// <summary>
        /// Draws a given text and returns the gameobject representing it
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static GameObject DrawText(string text)
        {
            //Create a box primitive
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            TextMesh textMesh = box.AddComponent<TextMesh>();
            textMesh.text = text;

            return box;
        }


        /// <summary>
        /// Draws the given avatar posture
        /// </summary>
        /// <param name="posture"></param>
        /// <returns></returns>
        public static GameObject DrawAvatarPosture(MAvatarPosture posture)
        {
            GameObject root = new GameObject();

            Dictionary<string, GameObject> transforms = new Dictionary<string, GameObject>();

            if (posture == null || posture.Joints == null)
                return root;

            foreach (MJoint boneTransform in posture.Joints)
            {
                //Create an empty gameobject for each bone
                GameObject joint = new GameObject(boneTransform.ID);
                joint.transform.position = boneTransform.Position.ToVector3();
                joint.transform.rotation = boneTransform.Rotation.ToQuaternion();

                transforms.Add(joint.name, joint);
            }


            foreach (MJoint boneTransform in posture.Joints)
            {
                GameObject joint = transforms[boneTransform.ID];

                //Compute the global position
                if (boneTransform.Parent != null && transforms.ContainsKey(boneTransform.Parent))
                {
                    joint.transform.position = transforms[boneTransform.Parent].transform.TransformPoint(joint.transform.position);
                    joint.transform.rotation = transforms[boneTransform.Parent].transform.rotation * joint.transform.rotation;

                    joint.transform.parent = transforms[boneTransform.Parent].transform;
                }
                else
                {
                    joint.transform.parent = root.transform;
                }
            }

            foreach (GameObject gameObject in transforms.Values)
            {
                if (gameObject.transform.parent != null && transforms.ContainsKey(gameObject.transform.parent.name))
                {
                    var lr = gameObject.AddComponent<LineRenderer>();
                    lr.material.shader = Shader.Find("Particles/Standard Surface");
                    lr.SetPositions(new Vector3[] { gameObject.transform.parent.position, gameObject.transform.position });
                    lr.startWidth = 0.02f;
                    lr.endWidth = 0.02f;
                    lr.startColor = Color.red;
                    lr.endColor = Color.blue;
                }
            }

            return root;
        }

        #region private methods

        /// <summary>
        /// Draws a point based on 3D coordinates
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static GameObject DrawPoint3D(Vector3 position, Color color, float scale = 0.01f)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.localScale = new Vector3(scale, scale, scale);
            point.transform.position = position;
            point.GetComponent<Renderer>().material.color = color;
            GameObject.Destroy(point.GetComponent<Collider>());
            return point;
        }


        /// <summary>
        /// Draws a point based on 2D coordinates
        /// </summary>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static GameObject DrawPoint2D(Vector2 position, Color color, float scale = 0.01f)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.localScale = new Vector3(scale, scale, scale);
            point.transform.position = new Vector3(position.x, scale / 2f, position.y);
            point.GetComponent<Renderer>().material.color = color;
            GameObject.Destroy(point.GetComponent<Collider>());
            return point;
        }

        /// <summary>
        /// Visualizes the global path
        /// </summary>
        /// <param name="path"></param>
        private static GameObject DrawLine(List<Vector3> path, Color color, float scale = 0.01f, float lineWidth = 0.02f)
        {
            if (path == null)
                return null;


            GameObject pathRoot = new GameObject("Global path");


            LineRenderer lineRenderer = pathRoot.AddComponent<LineRenderer>();


            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.positionCount = path.Count;
            lineRenderer.material.color = color;
            lineRenderer.SetPositions(path.ToArray());

            return pathRoot;
        }

        #endregion
    }

}
