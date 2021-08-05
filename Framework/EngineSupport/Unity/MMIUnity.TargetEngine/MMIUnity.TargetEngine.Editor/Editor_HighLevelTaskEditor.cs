/*Copyright 2020, Adam Kłodowski

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using UnityEngine;
using UnityEditor;
using MMIUnity.TargetEngine.Scene;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Threading;

namespace MMIUnity.TargetEngine.Editor
{

    [CustomEditor(typeof(HighLevelTaskEditor))]
    public class Editor_HighLevelTaskEditor : UnityEditor.Editor
    {
        SerializedProperty m_port;

        SerializedProperty m_useProxy;
        SerializedProperty m_proxyHttps;
        SerializedProperty m_useHttpsProxy;
        SerializedProperty m_proxyPort;
        SerializedProperty m_useProxyAuthenictation;
        SerializedProperty m_proxyUser;

        bool b_useProxy;
        bool b_useHttpsProxy;
        bool b_useProxyAuthenictation;
        string b_proxyHttps;
        string b_proxyPort;
        string b_proxyUser;

        SerializedProperty m_taskEditorWWW;
        SerializedProperty m_accessToken;
        SerializedProperty m_connectionEstablished;
        SerializedProperty m_products;
        SerializedProperty m_tools;
        SerializedProperty m_mainCamera;
        SerializedProperty m_photoCamera;
        SerializedProperty m_target;
        SerializedProperty m_stationsLoaded;
        SerializedProperty m_tasksLoaded;
        SerializedProperty m_toolsJson;
        SerializedProperty m_stationsJson;
        SerializedProperty m_parts;

        private List<string> _stations;
        private int _stationID = 0;
        private Texture2D progressBar;
        private int EditorWidth = 0;
        private int LastProgress = 0;

        private void OnEnable()
        {
            m_port = serializedObject.FindProperty("port");
            m_useProxy = serializedObject.FindProperty("useProxy");
            m_proxyHttps = serializedObject.FindProperty("proxyHttps");
            m_proxyPort = serializedObject.FindProperty("proxyPort");
            m_useHttpsProxy = serializedObject.FindProperty("useHttpsProxy");
            m_useProxyAuthenictation = serializedObject.FindProperty("useProxyAuthenictation");
            m_proxyUser = serializedObject.FindProperty("proxyUser");

            m_taskEditorWWW = serializedObject.FindProperty("taskEditorWWW");
            m_accessToken = serializedObject.FindProperty("accessToken");
            m_connectionEstablished = serializedObject.FindProperty("connectionEstablished");
            //m_products = serializedObject.FindProperty("products");
            //m_tools = serializedObject.FindProperty("tools");
            m_mainCamera = serializedObject.FindProperty("mainCamera");
            m_photoCamera = serializedObject.FindProperty("photoCamera");
            m_target = serializedObject.FindProperty("target");
            m_stationsJson = serializedObject.FindProperty("stationsJson");
            m_stationsLoaded = serializedObject.FindProperty("stationsLoaded");
            m_tasksLoaded = serializedObject.FindProperty("tasksLoaded");
            //m_parts = serializedObject.FindProperty("parts");

            b_useProxy = m_useProxy.boolValue;
            b_useHttpsProxy = m_useHttpsProxy.boolValue;
            b_useProxyAuthenictation = m_useProxyAuthenictation.boolValue;
            b_proxyHttps = m_proxyHttps.stringValue;
            b_proxyPort = m_proxyPort.stringValue;
            b_proxyUser = m_proxyUser.stringValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var HLTE = Selection.activeGameObject.GetComponent<HighLevelTaskEditor>();
            if (HLTE == null)
                return;
            for (int i = 0; i < HLTE.stationsJson.Count; i++)
                if (HLTE.stationsJson[i].id == HLTE.stationID)
                    _stationID = i;

            EditorGUILayout.PropertyField(m_port, new GUIContent("Port"));
            EditorGUILayout.PropertyField(m_useProxy, new GUIContent("Use proxy"));

            if (m_useProxy.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_useHttpsProxy, new GUIContent("Use HTTPS"));
                EditorGUILayout.PropertyField(m_proxyHttps, new GUIContent("Address"));
                EditorGUILayout.PropertyField(m_proxyPort, new GUIContent("Port"));
                EditorGUILayout.PropertyField(m_useProxyAuthenictation, new GUIContent("Use authentication"));
                if (m_useProxyAuthenictation.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_proxyUser, new GUIContent("Username"));
                    HLTE.setProxyPassword(EditorGUILayout.PasswordField("Password", HLTE.getProxyPassword()));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            if (m_connectionEstablished.boolValue)
                EditorGUILayout.LabelField(new GUIContent("Status:  Connected"));
            else
                EditorGUILayout.LabelField(new GUIContent("Status:  Disconnected"));
            EditorGUILayout.PropertyField(m_taskEditorWWW, new GUIContent("API URL"));

            EditorGUILayout.PropertyField(m_accessToken, new GUIContent("Token"));
            if (m_connectionEstablished.boolValue && (HLTE.tasEditorProjectName != ""))
                EditorGUILayout.LabelField(new GUIContent("Project: " + HLTE.tasEditorProjectName));

            if (!Application.isPlaying && HLTE.cameraScript != null && HLTE.gltfexporter != null)
            GUILayout.BeginHorizontal();

            if (!Application.isPlaying)
             if (HLTE.cameraScript!=null)
              if (GUILayout.Button("Generate part thumbnails"))
                        HLTE.cameraScript.StartPhotoSessionFromEditorMode();

            if (HLTE.gltfexporter!=null)
              if (GUILayout.Button("Generate glTFs"))
                        HLTE.GenerateGLTFsForParts();

            if (!Application.isPlaying && HLTE.cameraScript != null && HLTE.gltfexporter != null)
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload tools and stations"))
            {
                HLTE.ReloadTools();
                HLTE.ReloadStations();
                if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            if ((HLTE.accessToken != "") && (HLTE.taskEditorWWW != "") && (HLTE.connectionEstablished) && (!Application.isPlaying))
                if (GUILayout.Button("Update parts"))
                {
                    var result2 = HLTE.syncGroupsAndStationsToTaskEditor();
                    var result4 = HLTE.syncMMIScenObjectsToTaskEditor();
                    var result5 = HLTE.syncAvatarsToTaskEditor();
                    var result = HLTE.sendDataToTaskEditor();
                    var result1 = HLTE.syncMarkersToTaskEditor();
                    var result3 = HLTE.UploadPictures();
                    if (!Application.isPlaying)
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }

            GUILayout.EndHorizontal();

            if (HLTE.PictureUploadProgress >= 0)
                PictureUploadProgressBar(HLTE.PictureUploadProgress, HLTE.FinishedPictureUploading);

            if (m_stationsLoaded.boolValue)
                EditorGUILayout.LabelField(new GUIContent("Stations:  Loaded"));
            else
                EditorGUILayout.LabelField(new GUIContent("Stations:  No connection"));
            if (m_tasksLoaded.boolValue)
                EditorGUILayout.LabelField(new GUIContent("Tasks:  Loaded"));
            else
                EditorGUILayout.LabelField(new GUIContent("Tasks:  No connection"));

            List<string> _avatarsWithDefault = new List<string>();
            _avatarsWithDefault.Add("Default (0)");
            List<string> _avatars = new List<string>();
            if (HLTE.defaultAvatar >= HLTE.avatarJson.Count)
                HLTE.defaultAvatar = 0;
            for (int i = 0; i < HLTE.avatarJson.Count; i++)
            {
                _avatars.Add(HLTE.avatarJson[i].avatar + " (" + (i + 1).ToString() + ")");
                _avatarsWithDefault.Add(HLTE.avatarJson[i].avatar+" ("+(i+1).ToString()+")"+(HLTE.avatarJson[i].localID==0?"*":""));
            }

            if (m_stationsLoaded.boolValue)
            {
                _stations = new List<string>();

                if (HLTE.stationsJson.Count == 0)
                    EditorGUILayout.LabelField(new GUIContent("Station:  No stations defined"));
                else
                {

                    for (int i = 0; i < HLTE.stationsJson.Count; i++)
                    {
                        _stations.Add(HLTE.stationsJson[i].station);
                    }
                    _stationID = EditorGUILayout.Popup("Station", _stationID, _stations.ToArray());
                    Undo.RecordObject(HLTE, "Update station id");
                    HLTE.stationID = HLTE.stationsJson[_stationID].id;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(HLTE);
                    EditorGUILayout.LabelField(new GUIContent("Workers to simulate:"));
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < HLTE.workersJson.Count; j++)
                        if (HLTE.workersJson[j].stationid == HLTE.stationID)
                        {
                            ulong ss = HLTE.workersJson[j].avatarid;
                            EditorGUILayout.BeginHorizontal();
                            HLTE.workersJson[j].simulate = EditorGUILayout.Toggle(HLTE.workersJson[j].worker, HLTE.workersJson[j].simulate);
                            int avatarselected=EditorGUILayout.Popup("- Avatar", HLTE.AvatarIDToIndex(HLTE.workersJson[j].avatarid,true), _avatarsWithDefault.ToArray());
                            HLTE.workersJson[j].avatarid = (avatarselected == 0 ? 0 : HLTE.avatarJson[avatarselected-1].id);
                            EditorGUILayout.EndHorizontal();
                            if (ss != HLTE.workersJson[j].avatarid)
                                HLTE.workersJson[j].syncstatus = HighLevelTaskEditor.TSyncStatus.OutOfSync;
                            //Debug.Log(j.ToString() + ": " + ss.ToString()+" -> "+avatarselected.ToString() + ", " + HLTE.workersJson[j].avatarid.ToString());
                        }
                    EditorGUI.indentLevel--;
                }
            }

            HLTE.defaultAvatar = EditorGUILayout.Popup("Default avatar", HLTE.defaultAvatar, _avatars.ToArray());


            //EditorGUILayout.PropertyField(m_products, new GUIContent("Parts"));
            //EditorGUILayout.PropertyField(m_tools, new GUIContent("Tools"));
            //EditorGUILayout.PropertyField(m_mainCamera, new GUIContent("Main camera"));
            //EditorGUILayout.PropertyField(m_photoCamera, new GUIContent("Photo Camera"));
            //EditorGUILayout.PropertyField(m_target, new GUIContent("Target"));
            //EditorGUILayout.PropertyField(m_parts, new GUIContent("Sync Parts"));

            serializedObject.ApplyModifiedProperties();

            if ((b_useProxy != m_useProxy.boolValue) ||
                (b_useHttpsProxy != m_useHttpsProxy.boolValue) ||
                (b_useProxyAuthenictation != m_useProxyAuthenictation.boolValue) ||
                (b_proxyHttps != m_proxyHttps.stringValue) ||
                (b_proxyPort != m_proxyPort.stringValue) ||
                (b_proxyUser != m_proxyUser.stringValue))
                HLTE.saveProxySettings();

            b_useProxy = m_useProxy.boolValue;
            b_useHttpsProxy = m_useHttpsProxy.boolValue;
            b_useProxyAuthenictation = m_useProxyAuthenictation.boolValue;
            b_proxyHttps = m_proxyHttps.stringValue;
            b_proxyPort = m_proxyPort.stringValue;
            b_proxyUser = m_proxyUser.stringValue;
        }

        private void PictureUploadProgressBar(int progress, DateTime TimeFinished)
        {
            if (progress > 100)
                EditorGUILayout.LabelField("Picture upload complete at " + TimeFinished.ToString("HH:mm:ss"));
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Picture upload");
                var w = Int32.Parse(Math.Floor(EditorGUIUtility.currentViewWidth - 50 - EditorGUIUtility.labelWidth).ToString());
                progress = w * progress / 100;
                if ((w != EditorWidth) || (progress != LastProgress))
                {
                    EditorWidth = w;
                    progressBar = new Texture2D(EditorWidth, 10);

                    for (int x = 0; x < progressBar.width; x++)
                        for (int y = 0; y < progressBar.height; y++)
                            progressBar.SetPixel(x, y, (x < progress ? Color.green : Color.white));
                    progressBar.Apply();
                }
                LastProgress = progress;
                GUILayout.Box(progressBar);
                EditorGUILayout.EndHorizontal();
            }
        }

    }
}