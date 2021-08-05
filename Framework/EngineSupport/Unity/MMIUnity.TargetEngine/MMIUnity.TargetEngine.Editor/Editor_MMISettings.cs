// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System;
using MMIUnity.TargetEngine;
using UnityEditor;
using UnityEngine;

namespace MMIUnity.TargetEngine.Editor
{
	[CustomEditor(typeof(MMISettings))]
	public class Editor_MMISettings : UnityEditor.Editor
	{	
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			/*
			MMISettings.Instance.ShotFolder = EditorGUILayout.TextField("Photo folder",MMISettings.Instance.ShotFolder);
			MMISettings.Instance.glTFFolder = EditorGUILayout.TextField("glTF output folder", MMISettings.Instance.glTFFolder);		
			*/
		}
	}

}
