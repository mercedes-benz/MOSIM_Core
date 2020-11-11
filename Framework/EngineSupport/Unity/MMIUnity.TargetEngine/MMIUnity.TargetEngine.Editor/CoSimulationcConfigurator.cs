// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIUnity.TargetEngine.Scene;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MMIUnity.TargetEngine.Editor
{

    [CustomEditor(typeof(CoSimulationcConfigurator))]
    public class CoSimulationcConfiguratorEditorWindow : UnityEditor.Editor
    {

        /// <summary>
        /// Method responsible for the inspector visualization
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get the cosimulation debugger instance
            CoSimulationcConfigurator config = this.target as CoSimulationcConfigurator;

            //Call the base inspector
            base.OnInspectorGUI();



            if (GUILayout.Button("Push Priorities"))
            {
                config.Push();
            }

            if (GUILayout.Button("Pull Priorities"))
            {
                config.Pull();
            }

        }

    }


    [RequireComponent(typeof(MMIAvatar))]
    public class CoSimulationcConfigurator:MonoBehaviour
    {
        public List<CoSimPriority> Priorities = new List<CoSimPriority>();
        
        public void Push()
        {
            Dictionary<string, float> priorities = new Dictionary<string, float>();
            foreach(CoSimPriority p in this.Priorities)
            {
                priorities.Add(p.MotionType, p.Priority);
            }

            this.GetComponent<MMIAvatar>().CoSimulator.SetPriority(priorities);
        }

        public void Pull()
        {
            this.Priorities.Clear();
            foreach(var item in this.GetComponent<MMIAvatar>().CoSimulator.GetPriorities())
            {
                this.Priorities.Add(new CoSimPriority() { MotionType = item.Key, Priority = item.Value });
            }
        }
    }


    [System.Serializable]
    public class CoSimPriority
    {
        public string MotionType;

        public float Priority;
    }
}
