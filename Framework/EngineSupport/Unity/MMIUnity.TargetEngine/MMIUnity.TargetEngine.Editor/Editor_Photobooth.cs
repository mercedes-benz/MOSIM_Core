// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace MMIUnity.TargetEngine.Editor
{
    [CustomEditor(typeof(Photobooth))]
    class Editor_Photobooth : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isEditor && !Application.isPlaying)
                if (GUILayout.Button("Start"))
                {
                    (target as Photobooth).StartPhotoSessionFromEditorMode();
                    /*
                    string Fname = Application.dataPath;
                    if (!(Application.dataPath.EndsWith("/") || Application.dataPath.EndsWith("\\")))
                        Fname = Fname + "/";
                    Fname += (target as Photobooth).shotpath + "miniatures.txt";
                    if (!Directory.Exists((target as Photobooth).shotpath))
                        Directory.CreateDirectory((target as Photobooth).shotpath);
                    if (File.Exists(Fname))
                        File.Delete(Fname);
                    File.WriteAllText(Fname, "run");
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                    */
                }
            if ((target as Photobooth).manualmode && Application.isPlaying)
             if (GUILayout.Button("Next object"))
                (target as Photobooth).nextObject();
            if (GUILayout.Button("Restore state"))
            {
                (target as Photobooth).ReenableScriptsFromFile();
            }
        }
    }
}
