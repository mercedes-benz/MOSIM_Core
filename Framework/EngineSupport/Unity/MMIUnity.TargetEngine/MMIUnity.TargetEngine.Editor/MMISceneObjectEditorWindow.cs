// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIUnity.TargetEngine.Scene;
using UnityEditor;
using UnityEngine;

namespace MMIUnity.TargetEngine.Editor
{
    [CustomEditor(typeof(MMISceneObject))]
    public class MMISceneObjectEditorWindow : UnityEditor.Editor
    {

        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("GameObject/MMI/Add MMISceneObjects (recursive)", false, 0)]
        static void AddMMISceneObjectsRecursive()
        {
            foreach (Transform t in Selection.activeGameObject.GetComponentsInChildren<Transform>())
            {
                if (t.GetComponent<MMISceneObject>() == null)
                    t.gameObject.AddComponent<MMISceneObject>();
            }
        }

        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("GameObject/MMI/Add MMISceneObjects (recursive, mesh only)", false, 0)]
        static void AddMMISceneObjectsRecursiveMeshOnly()
        {
            foreach (MeshFilter t in Selection.activeGameObject.GetComponentsInChildren<MeshFilter>())
            {
                if (t.GetComponent<MMISceneObject>() == null)
                    t.gameObject.AddComponent<MMISceneObject>();
            }
        }

        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("GameObject/MMI/Synchronize (recursive)", false, 0)]
        static void SynchronizeRecursive()
        {
            foreach (MMISceneObject s in Selection.activeGameObject.GetComponentsInChildren<MMISceneObject>())
                s.Synchronize();
        }

        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("GameObject/MMI/Synchronize", false, 0)]
        static void Synchronize()
        {
            if (Selection.activeGameObject.GetComponent<MMISceneObject>() != null)
            {
                Selection.activeGameObject.GetComponent<MMISceneObject>().Synchronize();
            }
            else
            {
                Debug.LogWarning("Cannot synchronize object, because it does not contain MMISceneObject script");
            }
        }

        /// <summary>
        /// Method responsible for the inspector visualization
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get the cosimulation debugger instance
            MMISceneObject sceneObject = this.target as MMISceneObject;

            //Call the base inspector
            base.OnInspectorGUI();

            if (GUILayout.Button("Synchronize"))
            {
                sceneObject.Synchronize();
            }

            if (GUILayout.Button("Create scene objects recursive"))
            {
                foreach (Transform t in sceneObject.GetComponentsInChildren<Transform>())
                {
                    if (t.GetComponent<MMISceneObject>() == null)
                        t.gameObject.AddComponent<MMISceneObject>();
                }
            }

            if (GUILayout.Button("Create scene objects recursive (mesh only)"))
            {
                foreach (MeshFilter t in sceneObject.GetComponentsInChildren<MeshFilter>())
                {
                    if (t.GetComponent<MMISceneObject>() == null)
                        t.gameObject.AddComponent<MMISceneObject>();
                }
            }
        }
    }
}
