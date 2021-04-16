/*Copyright 2021, Adam Kłodowski

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using MMIUnity.TargetEngine.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MMIUnity.TargetEngine.Editor
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(MMIAvatar))]
    public class Editor_MMIAvatar : UnityEditor.Editor
    {
        static HighLevelTaskEditor HLTE;

        void OnEnable()
        {
            //Debug.Log("Getting task editor handle");
            HLTE = GameObject.FindObjectOfType<HighLevelTaskEditor>();
        }

        public override void OnInspectorGUI()
        {
            if ((HLTE!=null) && (HLTE.defaultAvatar < HLTE.avatarJson.Count))
            {
                bool _isDefault = (HLTE.avatarJson[HLTE.defaultAvatar].localID == (target as MMIAvatar).TaskEditorLocalID);
                bool  _isDefaultnew = EditorGUILayout.Toggle("Use as default avatar", _isDefault);
                if (_isDefaultnew)
                    HLTE.SetDefaultAvatarByLocalId((target as MMIAvatar).TaskEditorLocalID);
                else
                 if (_isDefault)
                    HLTE.defaultAvatar = 0;
            }
            base.OnInspectorGUI();
        }

        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("GameObject/MMI/Set as default", false, 0)]
        static void SetAvatarAsDefault()
        {
            if (HLTE != null)
            {
                MMIAvatar target = Selection.activeGameObject.GetComponent<MMIAvatar>();
                Debug.Log("Changing default avatar to: "+ target.name+" (" +target.TaskEditorLocalID.ToString()+")");
                HLTE.SetDefaultAvatarByLocalId(target.TaskEditorLocalID);
            }
           /* else
                Debug.Log("No high level task editor script attached");*/
        }
    }
}
