// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Janis Sprenger

using MMIStandard;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity
{
    /// <summary>
    /// Class contains several useful transform extensions for Unity Transforms 
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Returns the children by name
        /// </summary>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Transform GetChildRecursiveByName(this Transform t, string name)
        {
            if (t.name.Equals(name))
            {
                return t;
            }

            Transform target;
            for (int i = 0; i < t.childCount; i++)
            {
                target = t.GetChild(i).GetChildRecursiveByName(name);
                if (target != null)
                {
                    return target;
                }
            }
            return null;
        }

        /// <summary>
        /// Generates a list of joints with global coordinates
        /// </summary>
        /// <param name="t"></param>
        /// <param name="map"></param>
        /// <param name="list"></param>
        public static void GenerateGlobalJoints(this Transform t, Dictionary<string, MJointType> map, List<MJoint> list)
        {
            if (t.name.Contains("vis123bone"))
            {
                return;
            }

            //Create a new joint
            MJoint j = new MJoint
            {
                ID = t.name
            };

            if (map != null && map.ContainsKey(t.name))
            {
                j.Type = map[t.name];
            }
            else
            {
                j.Type = MJointType.Undefined;
            }

            j.Position = t.position.ToMVector3();
            j.Rotation = t.rotation.ToMQuaternion();
            j.Parent = t.parent != null ? t.parent.name : null;

            list.Add(j);

            for (int i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).GenerateGlobalJoints(map, list);
            }
        }


        /// <summary>
        /// Generates a list of joints with global coordinates
        /// </summary>
        /// <param name="t"></param>
        /// <param name="map"></param>
        /// <param name="list"></param>
        public static void GenerateLocalJoints(this Transform t, Transform rootJoint, Dictionary<string, MJointType> map, List<MJoint> list)
        {
            if (t.name.Contains("vis123bone"))
            {
                return;
            }

            //Create a new joint
            MJoint j = new MJoint
            {
                ID = t.name
            };

            if (map != null && map.ContainsKey(t.name))
            {
                j.Type = map[t.name];
            }
            else
            {
                j.Type = MJointType.Undefined;
            }

            //Only use the global coordinates for the root joint
            if (t == rootJoint)
            {
                j.Position = t.position.ToMVector3();
                j.Rotation = t.rotation.ToMQuaternion();
            }
            else
            {
                j.Position = t.localPosition.ToMVector3();
                j.Rotation = t.localRotation.ToMQuaternion();
            }

            list.Add(j);

            for (int i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).GenerateGlobalJoints(map, list);
            }
        }

        /// <summary>
        /// Applies the global joint transformations to the actual transform
        /// </summary>
        /// <param name="t"></param>
        /// <param name="list"></param>
        public static void ApplyGlobalJoints(this Transform t, List<MJoint> list)
        {
            t.ApplyGlobalJoints(list, 1);
            t.FixNonMappedJoints(list, 1);
        }


        public static int FixNonMappedJoints(this Transform t, List<MJoint> list, int id)
        {
            if (t.name.Contains("vis123bone_"))
            {
                return id;
            }
            if (list.Count == 0)
            {
                //Debug.Log("List 0. " + t.name);
            }
            else
            {
                if (list[id].ID == t.name)
                {
                    if (list[id].Type == MJointType.Undefined)
                    {
                        if (t.childCount == 1 || (t.childCount == 2 && t.GetChild(1).name.Contains("vis123bone_")))
                        {
                            Vector3 src = Vector3.up;//t.InverseTransformDirection(t.up);
                            Vector3 trgPos = t.GetChild(0).position + new Vector3();
                            Quaternion trgRot = new Quaternion(0, 0, 0, 1) * t.GetChild(0).rotation;
                            Vector3 trg = Quaternion.Inverse(t.parent.rotation) * ((t.GetChild(0).position - t.position).normalized);
                            Quaternion q = Quaternion.FromToRotation(src, trg);
                            //t.localRotation = q;
                            t.rotation = t.parent.rotation * q;

                            t.GetChild(0).position = trgPos;
                            t.GetChild(0).rotation = trgRot;

                        }
                    }
                    id += 1;
                }
                /*
                if (list[id].ID == t.name)
                {
                    if(list[id].Rotation != null)
                    {
                        t.rotation = list[id].Rotation.ToQuaternion();
                    }
                    if (list[id].Position != null)
                    {
                        t.position = list[id].Position.ToVector3();
                    }
                    id += 1;
                }*/
            }
            for (int i = 0; i < t.childCount; i++)
            {
                id = t.GetChild(i).FixNonMappedJoints(list, id);
            }

            return id;
        }

        /// <summary>
        /// Applies the global joint transformations to the actual transform
        /// </summary>
        /// <param name="t"></param>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int ApplyGlobalJoints(this Transform t, List<MJoint> list, int id)
        {
            if (t.name.Contains("vis123bone_"))
            {
                return id;
            }
            if (list.Count == 0)
            {
                //Debug.Log("List 0. " + t.name);
            }
            else
            {
                
                if (list[id].ID == t.name)
                {
                    if (list[id].Type != MJointType.Undefined)
                    {
                        t.position = list[id].Position.ToVector3();
                        t.rotation = list[id].Rotation.ToQuaternion();
                    }
                    id += 1;
                }
             /*  
                if (list[id].ID == t.name)
                {
                    if (list[id].Rotation != null)
                    {
                        t.rotation = list[id].Rotation.ToQuaternion();
                    }
                    if (list[id].Position != null)
                    {
                        t.position = list[id].Position.ToVector3();
                    }
                    id += 1;
                }
                */
            }
            for (int i = 0; i < t.childCount; i++)
            {
                id = t.GetChild(i).ApplyGlobalJoints(list, id);
            }

            return id;
        }


    }

}
