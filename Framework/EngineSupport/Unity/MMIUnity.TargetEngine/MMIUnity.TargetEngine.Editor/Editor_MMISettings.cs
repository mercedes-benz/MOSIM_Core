// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System;
using System.Collections.Generic;
using MMIUnity.TargetEngine;
using MMIUnity.TargetEngine.Scene;
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
			
			if (GUILayout.Button("Refresh all target visualizations"))
			{
				MMISceneObject[] Scene = GameObject.FindObjectsOfType<MMISceneObject>();
				for (int i = 0; i < Scene.Length; i++)
					if ((Scene[i].Type == MMISceneObject.Types.Group || Scene[i].Type == MMISceneObject.Types.Part || Scene[i].Type == MMISceneObject.Types.Tool)
						&& (Scene[i].FinalLocation != null | Scene[i].InitialLocation != null))
						{
							Scene[i].ghostMesh = Utils3D.ObjectGhoster(Scene[i]);
							if (Scene[i].InitialLocation != null)
							{ 
								updateLocationReference(ref Scene[i].InitialLocation.initialLocationGhosts, Scene[i]);
								//Scene[i].InitialLocation.initialLocationGhosts.Add(new MMISceneObject.TGhosts(Scene[i]));
							}
							if (Scene[i].FinalLocation != null)
							{
								updateLocationReference(ref Scene[i].FinalLocation.finalLocationGhosts, Scene[i]);
								//Scene[i].FinalLocation.finalLocationGhosts.Add(new MMISceneObject.TGhosts(Scene[i]));
							}

					}
			}
			/*
			MMISettings.Instance.ShotFolder = EditorGUILayout.TextField("Photo folder",MMISettings.Instance.ShotFolder);
			MMISettings.Instance.glTFFolder = EditorGUILayout.TextField("glTF output folder", MMISettings.Instance.glTFFolder);		
			*/
		}

        #region helper functions
		void updateLocationReference(ref List<MMISceneObject.TGhosts> targetGhost, MMISceneObject ghostObject)
		{
			if (targetGhost == null)
				targetGhost = new List<MMISceneObject.TGhosts>();
			for (int i = 0; i < targetGhost.Count; i++)
				if (targetGhost[i].sceneObject == ghostObject)
					return;
			targetGhost.Add(new MMISceneObject.TGhosts(ghostObject));
		}
        #endregion
    }

	//This class forces reload of WalkPoint texture whenever switching between play and edit mode. Without it switching back to edit mode results in lack of walk point visualization
	[InitializeOnLoadAttribute]
	public static class PlayModeStateChangedExample
	{
		// register an event handler when the class is initialized
		static PlayModeStateChangedExample()
		{
			EditorApplication.playModeStateChanged += LogPlayModeState;
		}

		private static void LogPlayModeState(PlayModeStateChange state)
		{
			MMIUnity.TargetEngine.Scene.TWalkPointGizmo.LoadTexture(true);
		}
	}

}
