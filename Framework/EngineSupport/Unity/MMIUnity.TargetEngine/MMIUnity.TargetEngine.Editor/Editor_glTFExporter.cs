// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MMIUnity.TargetEngine.Scene;

namespace MMIUnity.TargetEngine.Editor
{
    [CustomEditor(typeof(GLTFExport))]
    public class Editor_glTFExport : UnityEditor.Editor
    {
        private MMISceneObject lastObject;

        public override void OnInspectorGUI()
        {
            if ((lastObject != (target as GLTFExport).ObjectToExport) && (target as GLTFExport).ObjectToExport!=null)
            {
                (target as GLTFExport).GenerateFileName();
                lastObject = (target as GLTFExport).ObjectToExport;
            }
            base.OnInspectorGUI();
            if (GUILayout.Button("Export object"))
                (target as GLTFExport).ExportglTF();
        }
    }
}
