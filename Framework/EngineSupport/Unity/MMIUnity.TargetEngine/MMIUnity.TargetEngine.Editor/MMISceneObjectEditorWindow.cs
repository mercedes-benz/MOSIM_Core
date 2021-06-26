// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Adam Kłodowski

using MMIUnity.TargetEngine.Scene;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using MMICSharp.Common.Communication;


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

        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("GameObject/MMI/Add to library", false, 0)]
        static void AddToLibrary()
        {
            if (Selection.activeGameObject.GetComponent<MMISceneObject>() != null)
            {
                Selection.activeGameObject.GetComponent<MMISceneObject>().AddToLibrary();
            }
            else
            {
                Debug.LogWarning("Cannot add component to a library, because it does not contain MMISceneObject script");
            }
        }

        // Add a menu item named "Clear constraints" to MyMenu in the menu bar.
        [MenuItem("GameObject/MMI/Constraints/Clear", false, 0)]
        static void ClearConstraints()
        {
            if (Selection.activeGameObject.GetComponent<MMISceneObject>() != null)
            {
                Selection.activeGameObject.GetComponent<MMISceneObject>().Constraints.Clear();
            }
            else
            {
                Debug.LogWarning("Cannot clear component's constraints, because it does not contain MMISceneObject script");
            }
        }

        [MenuItem("GameObject/MMI/Get bounds", false, 0)]
        static void GetBoundsMenuItem()
        {
            if (Selection.activeGameObject.GetComponent<MMISceneObject>() != null)
            {
                var bounds=Selection.activeGameObject.GetComponent<MMISceneObject>().GetCompleteBounds();
                Debug.Log("Bounds center/min/max: "+bounds.center.x.ToString()+", "+bounds.center.y.ToString()+", "+bounds.center.z.ToString()+" / "+
                          bounds.min.x.ToString()+", "+bounds.min.y.ToString()+", "+bounds.min.z.ToString() + " / "+
                          bounds.max.x.ToString()+", "+bounds.max.y.ToString()+", "+bounds.max.z.ToString());
            }
            else
            {
                Debug.LogWarning("Cannot show component's bounds, because it does not contain MMISceneObject script");
            }
        }


        SerializedProperty m_ShowCoordinateSystem;
        SerializedProperty m_Velocity;
        SerializedProperty m_OverrideColor;
        SerializedProperty m_Color;
        SerializedProperty m_TransferMesh;
	    SerializedProperty m_Constraint;
        SerializedProperty m_StationRef;
        SerializedProperty m_GroupRef;

        // Type related Fields
        SerializedProperty m_Type;
        SerializedProperty m_InitialLocation;
        SerializedProperty m_FinalLocation;
        SerializedProperty m_IsLocatedAt;
        SerializedProperty m_toolType;

        private string _newTool = "";
        HighLevelTaskEditor HLTE;
        public int selectedIndex = 0;
        public int toolTypeIndex = 0;
	    private int selectedConstraint = -1;
        private bool selectedConstraintEdit = false;
        private string newID = "";


        async void OnEnable()
        {
            m_ShowCoordinateSystem = serializedObject.FindProperty("ShowCoordinateSystem");
            m_Velocity = serializedObject.FindProperty("Velocity");
            m_OverrideColor = serializedObject.FindProperty("OverrideColor");
            m_Color = serializedObject.FindProperty("Color");
            m_TransferMesh = serializedObject.FindProperty("TransferMesh");
            m_StationRef = serializedObject.FindProperty("StationRef");
            m_GroupRef = serializedObject.FindProperty("GroupRef");
            //m_Constraint = serializedObject.FindProperty("Constraints");

            m_toolType = serializedObject.FindProperty("Tool");
            // Type related Fields
            m_Type = serializedObject.FindProperty("Type");
            m_InitialLocation = serializedObject.FindProperty("InitialLocation");
            m_FinalLocation = serializedObject.FindProperty("FinalLocation");
            m_IsLocatedAt = serializedObject.FindProperty("IsLocatedAt");
            HLTE = GameObject.FindObjectOfType<HighLevelTaskEditor>();
        
            if (HLTE!=null)
            if (!HLTE.tasksLoaded)
            {
                Debug.Log("Tasks are loaded: " + HLTE.tasksLoaded);
                await HLTE.LoadToolListU(HLTE.accessToken, HLTE.taskEditorWWW);
            }  
        }


        /// <summary>
        /// Method responsible for drawing lines in 3D to visualize constraints
        /// </summary>
        /*
        void OnSceneGUI()
        {
            if ((selectedConstraint==-1) || (selectedConstraint>=(target as MMISceneObject).Constraints.Count))
                return;

            if ((target as MMISceneObject).Constraints[selectedConstraint].GeometryConstraint.__isset.TranslationConstraint)
                  DrawConstraintBox((target as MMISceneObject).Constraints[selectedConstraint].GeometryConstraint.ParentToConstraint.Position.ToVector3(),
                                    (target as MMISceneObject).Constraints[selectedConstraint].GeometryConstraint.ParentToConstraint.Rotation.ToQuaternion(), 
                                    (target as MMISceneObject).Constraints[selectedConstraint].GeometryConstraint.TranslationConstraint.Limits);

        }*/

        private void integrateConstraintProperties(int constIndex, MMIStandard.MConstraint newConst)
        {
            if (!newConst.__isset.Properties)
                return;

            if (!(target as MMISceneObject).Constraints[constIndex].__isset.Properties)
                (target as MMISceneObject).Constraints[constIndex].Properties=new Dictionary<string, string>();

            foreach( KeyValuePair<string, string> newitem in newConst.Properties )
                if ((target as MMISceneObject).Constraints[constIndex].Properties.ContainsKey(newitem.Key))
                    (target as MMISceneObject).Constraints[constIndex].Properties[newitem.Key]=newitem.Value; //update existing keys
                else
                    (target as MMISceneObject).Constraints[constIndex].Properties.Add(newitem.Key,newitem.Value); //add new keys
        }

        private bool compareAccelerationConstraint(MMIStandard.MAccelerationConstraint constA, MMIStandard.MAccelerationConstraint constB)
        {
               if (constA.__isset.ParentToConstraint && constB.__isset.ParentToConstraint)
                {
                    if (!((constA.ParentToConstraint.Position==constB.ParentToConstraint.Position) &&
                         (constA.ParentToConstraint.Rotation==constB.ParentToConstraint.Rotation) &&
                         (constA.ParentToConstraint.Parent==constB.ParentToConstraint.Parent)))
                        return false;
                }
                else
                 return false;
                if (constA.__isset.RotationalAcceleration && constB.__isset.RotationalAcceleration)
                {
                    if (constA.RotationalAcceleration!=constB.RotationalAcceleration)
                      return false;
                }
                else
                 return false;
                if (constA.__isset.TranslationalAcceleration && constB.__isset.TranslationalAcceleration)
                {
                    if (constA.TranslationalAcceleration!=constB.TranslationalAcceleration)
                        return false;
                }
                else
                 return false;
                if (constA.__isset.WeightingFactor && constB.__isset.WeightingFactor)
                {
                    if (constA.WeightingFactor!=constB.WeightingFactor)
                        return false;
                }
                else
                 return false;
            return true;
        }

        private bool compareGeometryConstraint(MMIStandard.MGeometryConstraint constA, MMIStandard.MGeometryConstraint constB)
        {
            if (constA.ParentObjectID!=constB.ParentObjectID)
                return false;
            if (constA.__isset.ParentToConstraint && constB.__isset.ParentToConstraint)
            {
                if ((constA.ParentToConstraint.Parent!=constB.ParentToConstraint.Parent) ||
                    (constA.ParentToConstraint.Position!=constB.ParentToConstraint.Position) ||
                    (constA.ParentToConstraint.Rotation!=constB.ParentToConstraint.Rotation))
                   return false;
            }
            else
             return false;
            if (constA.__isset.RotationConstraint && constB.__isset.RotationConstraint)
            {
                if (constA.RotationConstraint.Limits!=constB.RotationConstraint.Limits)
                   return false;
            }
            else
             return false;

            if (constA.__isset.TranslationConstraint && constB.__isset.TranslationConstraint)
            {
                if (constA.TranslationConstraint.Limits!=constB.TranslationConstraint.Limits)
                    return false;
            }
            else
             return false;

            if (constA.__isset.WeightingFactor && constB.__isset.WeightingFactor)
            {
                if (constA.WeightingFactor!=constB.WeightingFactor)
                return false;
            }
            else
             return false;

            return true;
        }

        private bool compareParentToConstraint(MMIStandard.MTransform constA, MMIStandard.MTransform constB)
        {
            if (constA.ID!=constB.ID)
                return false;

            if (constA.__isset.Parent!=constB.__isset.Parent)
                return false;

            if (constA.__isset.Parent && constA.Parent!=constB.Parent)
                return false;

            if (constA.Position!=constB.Position)
                return false;

            if (constA.Rotation!=constB.Rotation)
                return false;

            return true;
        }

        private bool compareVelocityConstraint(MMIStandard.MVelocityConstraint constA, MMIStandard.MVelocityConstraint constB)
        {
            if ((constA.__isset.ParentToConstraint!=constB.__isset.ParentToConstraint) ||
                (constA.__isset.RotationalVelocity!=constB.__isset.RotationalVelocity) ||
                (constA.__isset.TranslationalVelocity!=constB.__isset.TranslationalVelocity) ||
                (constA.__isset.WeightingFactor!=constB.__isset.WeightingFactor))
                return false;

            if (constA.__isset.ParentToConstraint)
                if (!compareParentToConstraint(constA.ParentToConstraint,constB.ParentToConstraint))
                    return false;

            if (constA.__isset.RotationalVelocity)
                if (constA.RotationalVelocity!=constB.RotationalVelocity)
                    return false;

            if (constA.__isset.TranslationalVelocity)
                if (constA.TranslationalVelocity!=constB.TranslationalVelocity)
                    return false;

            if (constA.__isset.WeightingFactor)
                if (constA.WeightingFactor!=constB.WeightingFactor)
                    return false;

            return true;
        }

        private bool compareJointConstraint(MMIStandard.MJointConstraint constA, MMIStandard.MJointConstraint constB)
        {
            if ((constA.__isset.AccelerationConstraint!=constB.__isset.AccelerationConstraint) ||
                (constA.__isset.GeometryConstraint!=constB.__isset.GeometryConstraint) ||
                (constA.__isset.VelocityConstraint!=constB.__isset.VelocityConstraint))
               return false;

            if (constA.JointType!=constB.JointType)
                return false;

            if (constA.__isset.GeometryConstraint)
                if (!compareGeometryConstraint(constA.GeometryConstraint,constB.GeometryConstraint))
                    return false;

            if (constA.__isset.AccelerationConstraint)
                if (!compareAccelerationConstraint(constA.AccelerationConstraint,constB.AccelerationConstraint))
                    return false;

            if (constA.__isset.VelocityConstraint)
               if (!compareVelocityConstraint(constA.VelocityConstraint,constB.VelocityConstraint))
                    return false;

            return true;
        }

        private bool comparePathConstraint(MMIStandard.MPathConstraint constA, MMIStandard.MPathConstraint constB)
        {
            if (constA.__isset.WeightingFactor!=constB.__isset.WeightingFactor)
                return false;

            if (constA.__isset.WeightingFactor && constA.WeightingFactor!=constB.WeightingFactor)
                return false;

            if (constA.PolygonPoints!=null && constB.PolygonPoints!=null)
            {
                if (constA.PolygonPoints.Count!=constB.PolygonPoints.Count)
                    return false;

                for (int i=0; i<constA.PolygonPoints.Count; i++)
                    if (!compareGeometryConstraint(constA.PolygonPoints[i],constB.PolygonPoints[i]))
                        return false;
            }

            return true;
        }

        private bool comparePostureConstraint(MMIStandard.MAvatarPostureValues constA, MMIStandard.MAvatarPostureValues constB)
        {
            if (constA.AvatarID!=constB.AvatarID)
                return false;
            
            if (constA.__isset.PartialJointList!=constB.__isset.PartialJointList)
                return false;

            if (constA.__isset.PartialJointList)
            {
                if (constA.PartialJointList.Count!=constB.PartialJointList.Count)
                    return false;
                for (int i=0; i<constA.PartialJointList.Count; i++)
                    if (constA.PartialJointList[i]!=constB.PartialJointList[i])
                        return false;
            }

            if (constA.PostureData.Count!=constB.PostureData.Count)
                return false;

            for (int i=0; i<constA.PostureData.Count; i++)
                if (constA.PostureData[i]!=constB.PostureData[i])
                    return false;
            
            return true;
        }

        private bool compareConstraints(MMIStandard.MConstraint constA, MMIStandard.MConstraint constB)
        {
            if   ( (constA.__isset.AccelerationConstraint!=constB.__isset.AccelerationConstraint) ||
                   (constA.__isset.GeometryConstraint!=constB.__isset.GeometryConstraint) ||
                   (constA.__isset.JointConstraint!=constB.__isset.JointConstraint) ||
                   (constA.__isset.JointPathConstraint!=constB.__isset.JointPathConstraint) ||
                   (constA.__isset.PathConstraint!=constB.__isset.PathConstraint) ||
                   (constA.__isset.PostureConstraint!=constB.__isset.PostureConstraint) ||
                   //(constA.__isset.Properties!=constB.__isset.Properties) ||
                   (constA.__isset.VelocityConstraint!=constB.__isset.VelocityConstraint))
                return false;

            if (constB.__isset.AccelerationConstraint)
                if (!compareAccelerationConstraint(constA.AccelerationConstraint,constB.AccelerationConstraint))
                    return false;

            if (constB.__isset.GeometryConstraint)
                if (!compareGeometryConstraint(constA.GeometryConstraint,constB.GeometryConstraint))
                    return false;

            if (constB.__isset.JointConstraint)
                if (!compareJointConstraint(constA.JointConstraint,constB.JointConstraint))
                    return false;
            
            if (constB.__isset.JointPathConstraint)
            {
                if (constA.JointPathConstraint.JointType!=constA.JointPathConstraint.JointType)
                    return false;

                if (!comparePathConstraint(constA.JointPathConstraint.PathConstraint,constB.JointPathConstraint.PathConstraint))
                    return false;
            }

            if (constB.__isset.PathConstraint)
                if (!comparePathConstraint(constA.PathConstraint,constB.PathConstraint))
                    return false;

            if (constB.__isset.PostureConstraint)
            {
               if (constA.PostureConstraint.__isset.JointConstraints!=constB.PostureConstraint.__isset.JointConstraints)
                    return false;

               if (constA.PostureConstraint.__isset.JointConstraints)
               {
                    if (constA.PostureConstraint.JointConstraints.Count!=constB.PostureConstraint.JointConstraints.Count)
                        return false;
                    for (int i=0; i<constA.PostureConstraint.JointConstraints.Count; i++)
                      if (!compareJointConstraint(constA.PostureConstraint.JointConstraints[i],constB.PostureConstraint.JointConstraints[i]))
                        return false;
                }

               if (!comparePostureConstraint(constA.PostureConstraint.Posture,constB.PostureConstraint.Posture))
                    return false;
            }

            if (constB.__isset.VelocityConstraint)
                if (!compareVelocityConstraint(constA.VelocityConstraint,constB.VelocityConstraint))
                    return false;

            return true;
        }

        private bool compareMarkers(MMIStandard.MPathConstraint constA, MMIStandard.MTransform constB)
        {
            for (int j=0; j<constA.PolygonPoints.Count; j++)
                if ((constA.PolygonPoints[j].ParentToConstraint.Position.X==constB.Position.X) &&
                    (constA.PolygonPoints[j].ParentToConstraint.Position.Y==constB.Position.Y) &&
                    (constA.PolygonPoints[j].ParentToConstraint.Position.Z==constB.Position.Z) &&
                    (constA.PolygonPoints[j].ParentToConstraint.Rotation.X==constB.Rotation.X) &&
                    (constA.PolygonPoints[j].ParentToConstraint.Rotation.Y==constB.Rotation.Y) &&
                    (constA.PolygonPoints[j].ParentToConstraint.Rotation.Z==constB.Rotation.Z) &&
                    (constA.PolygonPoints[j].ParentToConstraint.Rotation.W==constB.Rotation.W) &&
                    (constA.PolygonPoints[j].ParentToConstraint.ID==constB.ID))
                    return true;
            return false;
        }

        private bool compareSpawningZones(MMIStandard.MConstraint constA, MMIStandard.MConstraint constB)
        {
          return   (constA.__isset.GeometryConstraint && constB.__isset.GeometryConstraint) &&
                   (constA.GeometryConstraint.__isset.ParentToConstraint && constB.GeometryConstraint.__isset.ParentToConstraint) &&
                   (constA.GeometryConstraint.__isset.TranslationConstraint && constB.GeometryConstraint.__isset.TranslationConstraint) &&
                   (constA.GeometryConstraint.ParentToConstraint.Position.X==constB.GeometryConstraint.ParentToConstraint.Position.X) &&
                   (constA.GeometryConstraint.ParentToConstraint.Position.Y==constB.GeometryConstraint.ParentToConstraint.Position.Y) &&
                   (constA.GeometryConstraint.ParentToConstraint.Position.Z==constB.GeometryConstraint.ParentToConstraint.Position.Z) &&
                   (constA.GeometryConstraint.TranslationConstraint.Limits.X.Min==constB.GeometryConstraint.TranslationConstraint.Limits.X.Min) &&
                   (constA.GeometryConstraint.TranslationConstraint.Limits.Y.Min==constB.GeometryConstraint.TranslationConstraint.Limits.Y.Min) &&
                   (constA.GeometryConstraint.TranslationConstraint.Limits.Z.Min==constB.GeometryConstraint.TranslationConstraint.Limits.Z.Min) &&
                   (constA.GeometryConstraint.TranslationConstraint.Limits.X.Max==constB.GeometryConstraint.TranslationConstraint.Limits.X.Max) &&
                   (constA.GeometryConstraint.TranslationConstraint.Limits.Y.Max==constB.GeometryConstraint.TranslationConstraint.Limits.Y.Max) &&
                   (constA.GeometryConstraint.TranslationConstraint.Limits.Z.Max==constB.GeometryConstraint.TranslationConstraint.Limits.Z.Max);
        }

        private int isNewConstraint(MMIStandard.MConstraint newConstraint)
        {
            if (newConstraint.ID=="SpawningZone") //special handling of spawning zones
            {
             for (int i=0; i<(target as MMISceneObject).Constraints.Count; i++)
                if ((target as MMISceneObject).Constraints[i].ID=="SpawningZone")
                 if (compareSpawningZones(newConstraint, (target as MMISceneObject).Constraints[i]))
                 {
                   integrateConstraintProperties(i,newConstraint);
                   return i;
                 }
             return -1;
            }
            //checking markers and adding straight away those that are not repeating - in case the marker goups are existing twice with same ID, then constraints will be added twice, but this should not happen due to consolidation routine run afterwards
            if (((newConstraint.ID=="FinalLocation") || (newConstraint.ID=="InitialLocation") || (newConstraint.ID=="WalkTarget")) && (newConstraint.__isset.PathConstraint && newConstraint.PathConstraint.PolygonPoints.Count>0))
            {
             int integratedNewOnes = -1;
              for (int i=0; i<(target as MMISceneObject).Constraints.Count; i++)
                if (((target as MMISceneObject).Constraints[i].ID=="FinalLocation") || ((target as MMISceneObject).Constraints[i].ID=="InitialLocation") || ((target as MMISceneObject).Constraints[i].ID=="WalkTarget"))
                    if ((target as MMISceneObject).Constraints[i].__isset.PathConstraint)
                    { 
                       if ((target as MMISceneObject).Constraints[i].PathConstraint.PolygonPoints==null)
                        (target as MMISceneObject).Constraints[i].PathConstraint.PolygonPoints=new List<MMIStandard.MGeometryConstraint>();
                        integratedNewOnes=i;

                        for (int j=0; j<newConstraint.PathConstraint.PolygonPoints.Count; j++)
                         if (newConstraint.PathConstraint.PolygonPoints[j].__isset.ParentToConstraint && !compareMarkers((target as MMISceneObject).Constraints[i].PathConstraint,newConstraint.PathConstraint.PolygonPoints[j].ParentToConstraint))
                          (target as MMISceneObject).Constraints[i].PathConstraint.PolygonPoints.Add(newConstraint.PathConstraint.PolygonPoints[j]);

                        integrateConstraintProperties(i,newConstraint);
                    }
              return integratedNewOnes;
             }
            //handling all general constraints (not special ones)
            for (int i=0; i<(target as MMISceneObject).Constraints.Count; i++)
              if ((target as MMISceneObject).Constraints[i].ID==newConstraint.ID)
                if (compareConstraints((target as MMISceneObject).Constraints[i],newConstraint))
                return i;
            return -1;
        }

        /// <summary>
        /// Method responsible for the inspector visualization
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get the cosimulation debugger instance
            MMISceneObject sceneObject = this.target as MMISceneObject;

            //Call the base inspector
            //base.OnInspectorGUI();

        //The variables and GameObject from the MyGameObject script are displayed in the Inspector with appropriate labels
        EditorGUILayout.PropertyField(m_ShowCoordinateSystem, new GUIContent("ShowCoordinateSystem"));
        EditorGUILayout.PropertyField(m_Velocity, new GUIContent("Velocity"));
        EditorGUILayout.PropertyField(m_OverrideColor, new GUIContent("OverrideColor"));
        EditorGUILayout.PropertyField(m_Color, new GUIContent("Color"));
        //EditorGUILayout.PropertyField(m_PhysicsEnabled, new GUIContent("PhysicsEnabled"));
        EditorGUILayout.PropertyField(m_TransferMesh, new GUIContent("TransferMesh"));

        // Horizontal line in inspector
        EditorGUILayout.Space();
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.width + 13, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Type related input fields
        EditorGUILayout.PropertyField(m_Type, new GUIContent("Type"));
        EditorGUI.indentLevel++;
        var instance = (MMISceneObject)this.target;
        
        if (instance.Type == MMISceneObject.Types.Tool)
        {
                if (_newTool == "")
                {
                    _newTool = instance.Tool;
                }
                if ((HLTE==null) || (!HLTE.tasksLoaded))
                    EditorGUILayout.PropertyField(m_toolType, new GUIContent("Tool","Add High Level Task Editor script to the scene object and connect to a project to use tools from the project"));
                else
                {
                    toolTypeIndex = -1;
                    for (var i = 0; i < HLTE.toolsJson.tools.Length; i++)
                        if (HLTE.toolsJson.tools[i] == m_toolType.stringValue)
                            toolTypeIndex = i;

                    if (m_Type.enumValueIndex == MMISceneObject.Types.Tool.GetHashCode())
                    {
                        toolTypeIndex = EditorGUILayout.Popup("Tool type", toolTypeIndex, HLTE.toolsJson.tools); //TODO: add tool category selection
                        if ((toolTypeIndex >= 0) && (toolTypeIndex < HLTE.toolsJson.tools.Length))
                            m_toolType.stringValue = HLTE.toolsJson.tools[toolTypeIndex];
                        else
                            m_toolType.stringValue = "";
                    }
                }
            //EditorGUILayout.PropertyField(m_InitialLocation, new GUIContent("InitialLocation"));
            //EditorGUILayout.PropertyField(m_FinalLocation, new GUIContent("FinalLocation"));
            //EditorGUILayout.PropertyField(m_IsLocatedAt, new GUIContent("IsLocatedAt"));
        }

        if (instance.Type == MMISceneObject.Types.StationResult)
        {
                if ((HLTE == null) || (!HLTE.tasksLoaded))
                {
                    EditorGUILayout.PropertyField(m_StationRef, new GUIContent("Station ID", "Add High Level Task Editor script to the scene object and connect to a project to use station references"));
                    EditorGUILayout.PropertyField(m_GroupRef, new GUIContent("Group ID", "Add High Level Task Editor script to the scene object and connect to a project to use station results references"));
                }
                else
                    StationResultReferenceEditor();
        }
    
        if ((instance.Type == MMISceneObject.Types.Part) || (instance.Type == MMISceneObject.Types.Group) || (instance.Type == MMISceneObject.Types.Tool))
        {
            if (HLTE==null) //|| (!HLTE.tasksLoaded)
            {
            EditorGUILayout.PropertyField(m_InitialLocation, new GUIContent("InitialLocation","Add High Level Task Editor script to the scene object and connect to a project to use tools from the project"));
            EditorGUILayout.PropertyField(m_FinalLocation, new GUIContent("FinalLocation","Add High Level Task Editor script to the scene object and connect to a project to use tools from the project"));
            }
            else
             InitialAndFinalLocationEditor();   

            EditorGUILayout.PropertyField(m_IsLocatedAt, new GUIContent("IsLocatedAt"));
        }
        if ((instance.Type == MMISceneObject.Types.InitialLocation) || (instance.Type == MMISceneObject.Types.FinalLocation) || (instance.Type == MMISceneObject.Types.WalkTarget))
        {
             MMISceneObject parentObject = instance.gameObject.transform.parent.GetComponent<MMISceneObject>();
                if (parentObject != null)
                {
                    ConsolidatePathConstraints(parentObject);
                    if (GUILayout.Button("Add to parent as constraint"))
                    {
                        string constType = "InitialLocation";
                        if (instance.Type == MMISceneObject.Types.FinalLocation)
                            constType = "FinalLocation";
                        if (instance.Type == MMISceneObject.Types.WalkTarget)
                            constType = "WalkTarget";
                        HLTE.updatePartToolList();
                        MMIStandard.MGeometryConstraint PosMarker = new MMIStandard.MGeometryConstraint(instance.TaskEditorID.ToString() + "/" + instance.TaskEditorLocalID.ToString());
                        PosMarker.ParentToConstraint = new MMIStandard.MTransform(instance.name, instance.gameObject.transform.localPosition.ToMVector3(),
                                                                                  instance.gameObject.transform.localRotation.ToMQuaternion());
                        MMIStandard.MConstraint Marker = null;
                        for (int i = 0; i < parentObject.Constraints.Count; i++)
                            if (parentObject.Constraints[i].__isset.PathConstraint)
                                if (parentObject.Constraints[i].ID == constType)
                                {
                                    Marker = parentObject.Constraints[i];
                                    int found = FindConstraint(Marker, instance.TaskEditorLocalID.ToString(), instance.name);
                                    if (found == -1)
                                        Marker.PathConstraint.PolygonPoints.Add(PosMarker);
                                    else
                                    {
                                        Marker.PathConstraint.PolygonPoints[found].ParentObjectID = instance.TaskEditorID.ToString() + "/" + instance.TaskEditorLocalID.ToString();
                                        Marker.PathConstraint.PolygonPoints[found].ParentToConstraint.ID = instance.name;
                                        Marker.PathConstraint.PolygonPoints[found].ParentToConstraint.Position = instance.gameObject.transform.localPosition.ToMVector3();
                                        Marker.PathConstraint.PolygonPoints[found].ParentToConstraint.Rotation = instance.gameObject.transform.localRotation.ToMQuaternion();
                                    }
                                    break;
                                }
                        if (Marker == null)
                        {
                            Debug.Log("Creating new constraints in parent ("+parentObject.name+")");
                            Marker = new MMIStandard.MConstraint(constType);
                            Marker.PathConstraint = new MMIStandard.MPathConstraint();
                            Marker.PathConstraint.PolygonPoints = new List<MMIStandard.MGeometryConstraint>();
                            Marker.PathConstraint.PolygonPoints.Add(PosMarker);
                            parentObject.Constraints.Add(Marker);
                        }
                        else
                            Debug.Log("Adding constraints to parent (" + parentObject.name + ")");
                        parentObject.SaveConstraints();
                    }
                }
        }

        EditorGUI.indentLevel--;

        //MConstraints editor/display
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Constraint set", GUILayout.MaxWidth(100));
        GUILayout.FlexibleSpace();    

            if (GUILayout.Button("Reload"))
            {
                (target as MMISceneObject).LoadConstraints();
            }

            if (GUILayout.Button("Load"))
            {
                string jsonfile = EditorUtility.OpenFilePanel("Load constraints", "", "json");
                if (jsonfile.Length != 0)
                {
                    try
                        {
                           var constrs=Serialization.FromJsonString<List<MMIStandard.MConstraint>>(File.ReadAllText(jsonfile));
                            for (int i=0; i<constrs.Count; i++) //fix for missing default posture values that crashes thrift if not present
                             if (constrs[i].__isset.PostureConstraint)
                                if (constrs[i].PostureConstraint.Posture==null)
                                    constrs[i].PostureConstraint.Posture=new MMIStandard.MAvatarPostureValues("",new List<double>());
                                else
                                    if (constrs[i].PostureConstraint.Posture.PostureData==null)
                                    constrs[i].PostureConstraint.Posture.PostureData = new List<double>();
                           (target as MMISceneObject).Constraints=constrs;
                           (target as MMISceneObject).SaveConstraints();
                            if (!Application.isPlaying)
                           UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                        }
                    catch 
                    {
                        Debug.LogWarning("Loading constraints error: Wrong file format");
                    }
                }
            }

            if (GUILayout.Button("Load+"))
            {
                string jsonfile = EditorUtility.OpenFilePanel("Load constraints", "", "json");
                if (jsonfile.Length != 0)
                {
                    try
                        {
                           var constrs=Serialization.FromJsonString<List<MMIStandard.MConstraint>>(File.ReadAllText(jsonfile));
                            for (int i=0; i<constrs.Count; i++) //fix for missing default posture values that crashes thrift if not present
                             if (constrs[i].__isset.PostureConstraint)
                                if (constrs[i].PostureConstraint.Posture==null)
                                    constrs[i].PostureConstraint.Posture=new MMIStandard.MAvatarPostureValues("",new List<double>());
                                else
                                    if (constrs[i].PostureConstraint.Posture.PostureData==null)
                                    constrs[i].PostureConstraint.Posture.PostureData = new List<double>();
                            
                            for (int i=0; i<constrs.Count; i++)
                            {
                             int foundAt = isNewConstraint(constrs[i]);
                             if (foundAt==-1)
                              (target as MMISceneObject).Constraints.Add(constrs[i]);
                             else
                               integrateConstraintProperties(foundAt, constrs[i]); 
                             }
                           (target as MMISceneObject).SaveConstraints();
                            if (!Application.isPlaying)
                           UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                        }
                    catch 
                    {
                        Debug.LogWarning("Loading constraints error: Wrong file format");
                    }
                }
            }

            if (GUILayout.Button("Save"))
            {
                string jsonfile = EditorUtility.SaveFilePanel("Save constraints","",(target as MMISceneObject).TaskEditorLocalID + ".json","json");
                if (jsonfile.Length != 0)
                {
                    if (!jsonfile.EndsWith(".json"))
                        jsonfile+=".json";
                    System.IO.File.WriteAllText(jsonfile, Serialization.ToJsonString((target as MMISceneObject).Constraints));
                }
            }

            GUILayout.FlexibleSpace();    
        if ((target as MMISceneObject).Constraints.Count > 0)
            if (GUILayout.Button("Clear"))
            { 
                if (EditorUtility.DisplayDialog("Clearing constraints", "Do you want to clear completely the constraints associated with the selected object?", "Yes", "No"))
                {
                    (target as MMISceneObject).Constraints.Clear();
                    (target as MMISceneObject).SaveConstraints();
                    if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }
            }

        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Add constraint", GUILayout.MaxWidth(100));
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("geometrical"))
        {
            if ((selectedConstraint != -1) && (selectedConstraint < (target as MMISceneObject).Constraints.Count))
            {
                MMIStandard.MConstraint w = (target as MMISceneObject).Constraints[selectedConstraint];
                w.GeometryConstraint = new MMIStandard.MGeometryConstraint();
                w.GeometryConstraint.RotationConstraint = new MMIStandard.MRotationConstraint();
                w.GeometryConstraint.RotationConstraint.Limits = new MMIStandard.MInterval3();
                w.GeometryConstraint.RotationConstraint.Limits.X = new MMIStandard.MInterval(-1, 1);
                w.GeometryConstraint.RotationConstraint.Limits.Y = new MMIStandard.MInterval(-10, 10);
                w.GeometryConstraint.RotationConstraint.Limits.Z = new MMIStandard.MInterval(-30, 23);
                w.GeometryConstraint.TranslationConstraint = new MMIStandard.MTranslationConstraint();
                w.GeometryConstraint.TranslationConstraint.Type = MMIStandard.MTranslationConstraintType.BOX;
                w.GeometryConstraint.TranslationConstraint.Limits = new MMIStandard.MInterval3();
                w.GeometryConstraint.TranslationConstraint.Limits.X = new MMIStandard.MInterval(0, 0);
                w.GeometryConstraint.TranslationConstraint.Limits.Y = new MMIStandard.MInterval(-50, 50);
                w.GeometryConstraint.TranslationConstraint.Limits.Z = new MMIStandard.MInterval(12, 15);
            }
            else
                selectedConstraint = -1;

            if (selectedConstraint == -1)
            {
                MMIStandard.MConstraint w = new MMIStandard.MConstraint("Geometrical constraint " + ((target as MMISceneObject).Constraints.Count + 1).ToString());
                w.GeometryConstraint = new MMIStandard.MGeometryConstraint();
                w.GeometryConstraint.ParentObjectID = (target as MMISceneObject).MSceneObject.ID;
                w.GeometryConstraint.RotationConstraint = new MMIStandard.MRotationConstraint();
                w.GeometryConstraint.RotationConstraint.Limits = new MMIStandard.MInterval3();
                w.GeometryConstraint.RotationConstraint.Limits.X = new MMIStandard.MInterval(-1, 1);
                w.GeometryConstraint.RotationConstraint.Limits.Y = new MMIStandard.MInterval(-10, 10);
                w.GeometryConstraint.RotationConstraint.Limits.Z = new MMIStandard.MInterval(-30, 23);
                w.GeometryConstraint.TranslationConstraint = new MMIStandard.MTranslationConstraint();
                w.GeometryConstraint.TranslationConstraint.Type = MMIStandard.MTranslationConstraintType.BOX;
                w.GeometryConstraint.TranslationConstraint.Limits = new MMIStandard.MInterval3();
                w.GeometryConstraint.TranslationConstraint.Limits.X = new MMIStandard.MInterval(0, 0);
                w.GeometryConstraint.TranslationConstraint.Limits.Y = new MMIStandard.MInterval(-50, 50);
                w.GeometryConstraint.TranslationConstraint.Limits.Z = new MMIStandard.MInterval(12, 15);
                (target as MMISceneObject).Constraints.Add(w);
            }
            (target as MMISceneObject).SaveConstraints();
            if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        if (GUILayout.Button("velocity"))
        {
            if ((selectedConstraint != -1) && (selectedConstraint < (target as MMISceneObject).Constraints.Count))
            {
                MMIStandard.MConstraint w = (target as MMISceneObject).Constraints[selectedConstraint];
                w.VelocityConstraint = new MMIStandard.MVelocityConstraint();
                w.VelocityConstraint.ParentObjectID = (target as MMISceneObject).MSceneObject.ID;
                w.VelocityConstraint.RotationalVelocity = new MMIStandard.MVector3();
                w.VelocityConstraint.TranslationalVelocity = new MMIStandard.MVector3();
                w.VelocityConstraint.RotationalVelocity = new MMIStandard.MVector3(0, 0, 0);
                w.VelocityConstraint.TranslationalVelocity = new MMIStandard.MVector3(0, 0, 0);
            }
            else
                selectedConstraint = -1;

            if (selectedConstraint == -1)
            {
                MMIStandard.MConstraint w = new MMIStandard.MConstraint("Velocity constraint " + ((target as MMISceneObject).Constraints.Count + 1).ToString());
                w.VelocityConstraint = new MMIStandard.MVelocityConstraint();
                w.VelocityConstraint.ParentObjectID = (target as MMISceneObject).MSceneObject.ID;
                w.VelocityConstraint.RotationalVelocity = new MMIStandard.MVector3();
                w.VelocityConstraint.TranslationalVelocity = new MMIStandard.MVector3();
                w.VelocityConstraint.RotationalVelocity = new MMIStandard.MVector3(0, 0, 0);
                w.VelocityConstraint.TranslationalVelocity = new MMIStandard.MVector3(0, 0, 0);
                (target as MMISceneObject).Constraints.Add(w);
            }
            (target as MMISceneObject).SaveConstraints();
            if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        if (GUILayout.Button("acceleration"))
        {
            if ((selectedConstraint != -1) && (selectedConstraint < (target as MMISceneObject).Constraints.Count))
            {
                MMIStandard.MConstraint w = (target as MMISceneObject).Constraints[selectedConstraint];
                w.AccelerationConstraint = new MMIStandard.MAccelerationConstraint();
                w.AccelerationConstraint.ParentObjectID = (target as MMISceneObject).MSceneObject.ID;
                w.AccelerationConstraint.RotationalAcceleration = new MMIStandard.MVector3(0, 0, 0);
                w.AccelerationConstraint.TranslationalAcceleration = new MMIStandard.MVector3(0, 0, 0);
            }
            else
                selectedConstraint = -1;

            if (selectedConstraint == -1)
            {
                MMIStandard.MConstraint w = new MMIStandard.MConstraint("Acceleration constraint " + ((target as MMISceneObject).Constraints.Count + 1).ToString());
                w.AccelerationConstraint = new MMIStandard.MAccelerationConstraint();
                w.AccelerationConstraint.ParentObjectID = (target as MMISceneObject).MSceneObject.ID;
                w.AccelerationConstraint.RotationalAcceleration = new MMIStandard.MVector3(0, 0, 0);
                w.AccelerationConstraint.TranslationalAcceleration = new MMIStandard.MVector3(0, 0, 0);
                (target as MMISceneObject).Constraints.Add(w);
            }
            (target as MMISceneObject).SaveConstraints();
            if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }      

        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("spawning zone"))
        {
            MMIStandard.MConstraint w = new MMIStandard.MConstraint("SpawningZone");
            w.GeometryConstraint=new MMIStandard.MGeometryConstraint("1"); //this should be standardized somehow
            w.GeometryConstraint.ParentToConstraint = new MMIStandard.MTransform(UnitySceneAccess.CreateSceneObjectID(),new MMIStandard.MVector3(),new MMIStandard.MQuaternion());
            w.GeometryConstraint.TranslationConstraint = new MMIStandard.MTranslationConstraint(0,new MMIStandard.MInterval3(new MMIStandard.MInterval(-0.1,0.1),new MMIStandard.MInterval(-0.1,0.1),new MMIStandard.MInterval(-0.1,0.1)));
            (target as MMISceneObject).Constraints.Add(w);
            (target as MMISceneObject).SaveConstraints();
            if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
            

        GUIStyle styleOn = new GUIStyle();
        styleOn.richText = true;

        EditorGUILayout.Space();
        bool ConstraintsChanged = false;
        int deleteConstraint = -1;
        //displaying all the constraints                        
        for (int i = 0; i < (target as MMISceneObject).Constraints.Count; i++)
        {
            string header = "<size=12><color=white>" +(selectedConstraint == i?"<b>":"") + (target as MMISceneObject).Constraints[i].ID + (selectedConstraint == i ? "</b>" : "") + "</color></size>";
            EditorGUILayout.BeginHorizontal();
            if (!((selectedConstraint == i) && (selectedConstraintEdit)))
                if (GUILayout.Button(header,styleOn))
                {
                    if (selectedConstraint == i)
                    {
                        selectedConstraint = -1;
                        selectedConstraintEdit = false;
                    }
                    else
                        selectedConstraint = i;
                    if ((target as MMISceneObject).selectedConstraint!=selectedConstraint)
                    {
                        (target as MMISceneObject).selectedConstraint=selectedConstraint;
                        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                        UnityEditor.SceneView.RepaintAll();
                    }
                }
            if (selectedConstraint == i)
                if (selectedConstraintEdit)
                {
                    EditorGUILayout.LabelField("Name", GUILayout.MaxWidth(100));
                    GUILayout.FlexibleSpace();

                    newID = EditorGUILayout.TextField(newID, GUILayout.MaxWidth(200));
                    if (GUILayout.Button("OK"))
                    {
                        (target as MMISceneObject).Constraints[i].ID = newID;
                        selectedConstraintEdit = false;
                        (target as MMISceneObject).SaveConstraints();
                        if (!Application.isPlaying)
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                    }
                    if (GUILayout.Button("Cancel"))
                        selectedConstraintEdit = false;
                }
                else
                {
                    if (GUILayout.Button("Edit"))
                    {
                        selectedConstraintEdit = true;
                        newID = (target as MMISceneObject).Constraints[i].ID;
                    }
                    if (GUILayout.Button("Delete"))
                        deleteConstraint =i;
                }
        
             EditorGUILayout.EndHorizontal();  

            //EditorGUILayout.LabelField("<size=12><color=white><b>" + (target as MMISceneObject).Constraints[i].ID + "</b></color></size>", styleOn);
            if ((target as MMISceneObject).Constraints[i].__isset.AccelerationConstraint)
            {
                if ((target as MMISceneObject).Constraints[i].AccelerationConstraint.__isset.TranslationalAcceleration)
                (target as MMISceneObject).Constraints[i].AccelerationConstraint.TranslationalAcceleration =
                Vector3Editor((target as MMISceneObject).Constraints[i].AccelerationConstraint.TranslationalAcceleration, "Translational A",ref ConstraintsChanged);
                if ((target as MMISceneObject).Constraints[i].AccelerationConstraint.__isset.RotationalAcceleration)
                (target as MMISceneObject).Constraints[i].AccelerationConstraint.RotationalAcceleration =
                Vector3Editor((target as MMISceneObject).Constraints[i].AccelerationConstraint.RotationalAcceleration, "Rotational A", ref ConstraintsChanged);
              
            }

            if ((target as MMISceneObject).Constraints[i].__isset.VelocityConstraint)
            {
                if ((target as MMISceneObject).Constraints[i].VelocityConstraint.__isset.TranslationalVelocity)
                (target as MMISceneObject).Constraints[i].VelocityConstraint.TranslationalVelocity =
                Vector3Editor((target as MMISceneObject).Constraints[i].VelocityConstraint.TranslationalVelocity, "Translational V", ref ConstraintsChanged);
                if ((target as MMISceneObject).Constraints[i].VelocityConstraint.__isset.RotationalVelocity)
                (target as MMISceneObject).Constraints[i].VelocityConstraint.RotationalVelocity =
                Vector3Editor((target as MMISceneObject).Constraints[i].VelocityConstraint.RotationalVelocity, "Rotational V", ref ConstraintsChanged);
                
            }

            if ((target as MMISceneObject).Constraints[i].__isset.GeometryConstraint)
            {
                if ((target as MMISceneObject).Constraints[i].GeometryConstraint.__isset.ParentToConstraint)
                {
                 (target as MMISceneObject).Constraints[i].GeometryConstraint.ParentToConstraint.Position =
                  Vector3Editor((target as MMISceneObject).Constraints[i].GeometryConstraint.ParentToConstraint.Position, "Position", ref ConstraintsChanged);
                 (target as MMISceneObject).Constraints[i].GeometryConstraint.ParentToConstraint.Rotation=MMIStandard.MQuaternionExtensions.FromEuler(
                  Vector3Editor(MMIStandard.MQuaternionExtensions.ToEuler((target as MMISceneObject).Constraints[i].GeometryConstraint.ParentToConstraint.Rotation), "Rotation", ref ConstraintsChanged));
                }

                if ((target as MMISceneObject).Constraints[i].GeometryConstraint.__isset.TranslationConstraint)
                (target as MMISceneObject).Constraints[i].GeometryConstraint.TranslationConstraint.Limits =
                LimitEditor((target as MMISceneObject).Constraints[i].GeometryConstraint.TranslationConstraint.Limits, "Translation limits", ref ConstraintsChanged);
                if ((target as MMISceneObject).Constraints[i].GeometryConstraint.__isset.RotationConstraint)
                (target as MMISceneObject).Constraints[i].GeometryConstraint.RotationConstraint.Limits =
                LimitEditor((target as MMISceneObject).Constraints[i].GeometryConstraint.RotationConstraint.Limits, "Rotation limits", ref ConstraintsChanged);
            }

            if ((target as MMISceneObject).Constraints[i].__isset.PathConstraint)
            {
                if ((target as MMISceneObject).Constraints[i].ID=="InitialLocation" || (target as MMISceneObject).Constraints[i].ID=="FinalLocation" || (target as MMISceneObject).Constraints[i].ID=="WalkTarget")
                MarkerEditor((target as MMISceneObject).Constraints[i].PathConstraint,i, ref ConstraintsChanged);
                else
                PathEditor((target as MMISceneObject).Constraints[i].PathConstraint, ref ConstraintsChanged);
            }

            if ((target as MMISceneObject).Constraints[i].__isset.PostureConstraint)
            {
                if ((target as MMISceneObject).Constraints[i].PostureConstraint.__isset.JointConstraints)
                {
                  EditorGUI.indentLevel++;
                    for (int j=0; j<(target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints.Count; j++)
                    {
                        if ((target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].__isset.GeometryConstraint)
                        {
                          if ((target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].__isset.GeometryConstraint)
                           if ((target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].GeometryConstraint.__isset.ParentToConstraint)
                           {
                                var constrID = (target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].GeometryConstraint.ParentToConstraint.ID.Replace("Right","").Replace("Left","");
                                (target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].GeometryConstraint.ParentToConstraint.Position =
                                Vector3Editor((target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].GeometryConstraint.ParentToConstraint.Position,constrID+" position" , ref ConstraintsChanged);
                                MMIStandard.MVector3 Rot=MMIStandard.MQuaternionExtensions.ToEuler((target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].GeometryConstraint.ParentToConstraint.Rotation);
                                Rot = Vector3Editor(Rot, constrID+" rotation", ref ConstraintsChanged);
                                (target as MMISceneObject).Constraints[i].PostureConstraint.JointConstraints[j].GeometryConstraint.ParentToConstraint.Rotation=MMIStandard.MQuaternionExtensions.FromEuler(Rot);
                          }
                        }
                    }
                  EditorGUI.indentLevel--;
                }
            }
        }

        if (deleteConstraint>-1)
        {
          (target as MMISceneObject).Constraints.RemoveAt(deleteConstraint);
          selectedConstraint = -1;
          (target as MMISceneObject).selectedConstraint=selectedConstraint;
          ConstraintsChanged=true;
        }

        if (ConstraintsChanged)
        {
            (target as MMISceneObject).SaveConstraints();
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
        }
        
        // Apply changes to the serializedProperty
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
            if (GUILayout.Button("Synchronize"))
                sceneObject.Synchronize();
        }
        /*
        void DrawConstraintBox(Vector3 Position, Quaternion Rotation, MMIStandard.MInterval3 Limits)
        {
            var colortop = Color.red;
            var color = Color.green;
            var v3FrontTopLeft = new Vector3(- (float)Limits.X.Min, (float)Limits.Y.Max, - (float)Limits.Z.Min);  // Front top left corner
            var v3FrontTopRight = new Vector3((float)Limits.X.Max, (float)Limits.Y.Max, - (float)Limits.Z.Min);  // Front top right corner
            var v3FrontBottomLeft = new Vector3(- (float)Limits.X.Min, - (float)Limits.Y.Min, - (float)Limits.Z.Min);  // Front bottom left corner
            var v3FrontBottomRight = new Vector3((float)Limits.X.Max, - (float)Limits.Y.Min, - (float)Limits.Z.Min);  // Front bottom right corner
            var v3BackTopLeft = new Vector3(- (float)Limits.X.Min,  (float)Limits.Y.Max, (float)Limits.Z.Max);  // Back top left corner
            var v3BackTopRight = new Vector3((float)Limits.X.Max,  (float)Limits.Y.Max,  (float)Limits.Z.Max);  // Back top right corner
            var v3BackBottomLeft = new Vector3(- (float)Limits.X.Min,- (float)Limits.Y.Min, (float)Limits.Z.Max);  // Back bottom left corner
            var v3BackBottomRight = new Vector3((float)Limits.X.Max, - (float)Limits.Y.Min, (float)Limits.Z.Max);  // Back bottom right corner
            
            v3FrontTopLeft = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3FrontTopLeft + Position);
            v3FrontTopRight = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3FrontTopRight + Position);
            v3FrontBottomLeft = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3FrontBottomLeft + Position);
            v3FrontBottomRight = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3FrontBottomRight + Position);
            v3BackTopLeft = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3BackTopLeft + Position);
            v3BackTopRight = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3BackTopRight + Position);
            v3BackBottomLeft = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3BackBottomLeft + Position);
            v3BackBottomRight = (target as MMISceneObject).gameObject.transform.TransformPoint(Rotation * v3BackBottomRight + Position);
            Handles.color=colortop;
            Handles.DrawLine(v3FrontTopLeft, v3FrontTopRight);
            Handles.color=color;
            Handles.DrawLine(v3FrontTopRight, v3FrontBottomRight);
            Handles.DrawLine(v3FrontBottomRight, v3FrontBottomLeft);
            Handles.DrawLine(v3FrontBottomLeft, v3FrontTopLeft);

            Handles.color=colortop;
            Handles.DrawLine(v3BackTopLeft, v3BackTopRight);
            Handles.color=color;
            Handles.DrawLine(v3BackTopRight, v3BackBottomRight);
            Handles.DrawLine(v3BackBottomRight, v3BackBottomLeft);
            Handles.DrawLine(v3BackBottomLeft, v3BackTopLeft);

            Handles.color=colortop;
            Handles.DrawLine(v3FrontTopLeft, v3BackTopLeft);
            Handles.DrawLine(v3FrontTopRight, v3BackTopRight);
            Handles.color=color;
            Handles.DrawLine(v3FrontBottomRight, v3BackBottomRight);
            Handles.DrawLine(v3FrontBottomLeft, v3BackBottomLeft);*/
            /*
            Debug.DrawLine(v3FrontTopLeft, v3FrontTopRight, colortop);
            Debug.DrawLine(v3FrontTopRight, v3FrontBottomRight, color);
            Debug.DrawLine(v3FrontBottomRight, v3FrontBottomLeft, color);
            Debug.DrawLine(v3FrontBottomLeft, v3FrontTopLeft, color);
    
            Debug.DrawLine(v3BackTopLeft, v3BackTopRight, colortop);
            Debug.DrawLine(v3BackTopRight, v3BackBottomRight, color);
            Debug.DrawLine(v3BackBottomRight, v3BackBottomLeft, color);
            Debug.DrawLine(v3BackBottomLeft, v3BackTopLeft, color);
    
            Debug.DrawLine(v3FrontTopLeft, v3BackTopLeft, colortop);
            Debug.DrawLine(v3FrontTopRight, v3BackTopRight, colortop);
            Debug.DrawLine(v3FrontBottomRight, v3BackBottomRight, color);
            Debug.DrawLine(v3FrontBottomLeft, v3BackBottomLeft, color);*/
        //}

        void ConsolidatePathConstraints(MMISceneObject instance)
        {
            int lastFinalLocation = -1;
            int lastInitialLocation = -1;
            int lastWalkTarget = -1;
            if (instance.Constraints!=null)
            for (int i=0; i<instance.Constraints.Count; i++)
            {
                if (instance.Constraints[i].ID == "FinalLocation")
                {
                    if (instance.Constraints[i].__isset.PathConstraint)
                        if (lastFinalLocation > -1)
                        {
                            for (int j = 0; j < instance.Constraints[i].PathConstraint.PolygonPoints.Count; j++)
                                instance.Constraints[lastFinalLocation].PathConstraint.PolygonPoints.Add(instance.Constraints[i].PathConstraint.PolygonPoints[j]);
                            instance.Constraints[i].PathConstraint.PolygonPoints.Clear();
                        }
                        else
                            lastFinalLocation = i;
                }
                if (instance.Constraints[i].ID == "InitialLocation")
                {
                    if (instance.Constraints[i].__isset.PathConstraint)
                        if (lastInitialLocation > -1)
                        {
                            for (int j = 0; j < instance.Constraints[i].PathConstraint.PolygonPoints.Count; j++)
                                instance.Constraints[lastInitialLocation].PathConstraint.PolygonPoints.Add(instance.Constraints[i].PathConstraint.PolygonPoints[j]);
                            instance.Constraints[i].PathConstraint.PolygonPoints.Clear();
                        }
                        else
                            lastInitialLocation = i;
                }
                if (instance.Constraints[i].ID == "WalkTarget")
                {
                    if (instance.Constraints[i].__isset.PathConstraint)
                        if (lastWalkTarget > -1)
                        {
                            for (int j = 0; j < instance.Constraints[i].PathConstraint.PolygonPoints.Count; j++)
                                instance.Constraints[lastWalkTarget].PathConstraint.PolygonPoints.Add(instance.Constraints[i].PathConstraint.PolygonPoints[j]);
                            instance.Constraints[i].PathConstraint.PolygonPoints.Clear();
                        }
                        else
                            lastWalkTarget = i;
                }
            }
        }

        string LocalId(string data)
        {
            if (data.Contains("/"))
            {
                var splitvalue = data.Split('/');
                return splitvalue[1];
            }
          return "0";
        }

        int FindConstraint(MMIStandard.MConstraint Marker, string localId, string cname)
        {
            int found = -1;
            for (int j = 0; j < Marker.PathConstraint.PolygonPoints.Count; j++)
                if (Marker.PathConstraint.PolygonPoints[j].__isset.ParentToConstraint)
                {
                    if (LocalId(Marker.PathConstraint.PolygonPoints[j].ParentObjectID) == localId)
                    {
                        found = j;
                        break;
                    }

                    if (Marker.PathConstraint.PolygonPoints[j].ParentToConstraint.ID == cname)
                        found = j;
                }
            return found;
        }

        void InitialLocationConstraint()
        {
            var instance = (MMISceneObject)this.target;
            var constInstance = instance.InitialLocation;
            if (constInstance == null)
                return;
            if (constInstance.Type == MMISceneObject.Types.InitialLocation)
                return;
            int selected = 0;
            String[] iLocations = new string[0];
            for (int i = 0; i < constInstance.Constraints.Count; i++)
                if (constInstance.Constraints[i].ID == "InitialLocation" && constInstance.Constraints[i].__isset.PathConstraint)
                {
                    iLocations = new string[constInstance.Constraints[i].PathConstraint.PolygonPoints.Count];
                    for (int j = 0; j < constInstance.Constraints[i].PathConstraint.PolygonPoints.Count; j++)
                    {
                        iLocations[j] = constInstance.Constraints[i].PathConstraint.PolygonPoints[j].ParentToConstraint.ID;
                        if (iLocations[j] == instance.InitialLocationConstraint)
                            selected = j;
                    }
                    break;
                }
            if (iLocations.Length == 0)
                return;
            EditorGUI.indentLevel++;
            selected = EditorGUILayout.Popup("Constraint", selected, iLocations);
            instance.InitialLocationConstraint = iLocations[selected];
            EditorGUI.indentLevel--;
        }

        void FinalLocationConstraint()
        {
            var instance = (MMISceneObject)this.target;
            var constInstance = instance.FinalLocation;
            if (constInstance == null)
                return;
            if (constInstance.Type == MMISceneObject.Types.FinalLocation)
                return;
            int selected = 0;
            String[] iLocations = new string[0];
            for (int i = 0; i < constInstance.Constraints.Count; i++)
                if (constInstance.Constraints[i].ID == "FinalLocation" && constInstance.Constraints[i].__isset.PathConstraint)
                {
                    iLocations = new string[constInstance.Constraints[i].PathConstraint.PolygonPoints.Count];
                    for (int j = 0; j < constInstance.Constraints[i].PathConstraint.PolygonPoints.Count; j++)
                    {
                        iLocations[j] = constInstance.Constraints[i].PathConstraint.PolygonPoints[j].ParentToConstraint.ID;
                        if (iLocations[j] == instance.FinalLocationConstraint)
                            selected = j;
                    }
                    break;
                }
            if (iLocations.Length == 0)
                return;
            EditorGUI.indentLevel++;
            selected = EditorGUILayout.Popup("Constraint", selected, iLocations);
            instance.FinalLocationConstraint = iLocations[selected];
            EditorGUI.indentLevel--;
        }

        void InitialAndFinalLocationEditor()
        {
            HLTE.updatePartToolList();
            var instance = (MMISceneObject)this.target;
            int InitialLocationIndex=0;
             String[] iLocations = new string[HLTE.InitialLocations.Count+1];
             iLocations[0]="- None -";
               for (var i = 0; i < HLTE.InitialLocations.Count; i++)
               {
                iLocations[i+1]=HLTE.InitialLocations[i].name;
                if (HLTE.InitialLocations[i] == instance.InitialLocation)
                InitialLocationIndex = i+1;
               }
              InitialLocationIndex = EditorGUILayout.Popup("Initial location", InitialLocationIndex, iLocations);
              if ((InitialLocationIndex > 0) && (InitialLocationIndex <= HLTE.InitialLocations.Count))
                instance.InitialLocation = HLTE.InitialLocations[InitialLocationIndex - 1];
              else
              {
                instance.InitialLocation = null;
                instance.InitialLocationConstraint = "";
              }
            InitialLocationConstraint();

             int FinalLocationIndex=0;
             String[] fLocations = new string[HLTE.FinalLocations.Count+1];
            fLocations[0]="- None -";
               for (var i = 0; i < HLTE.FinalLocations.Count; i++)
               {
                fLocations[i+1]=HLTE.FinalLocations[i].name;
                if (HLTE.FinalLocations[i] == instance.FinalLocation)
                FinalLocationIndex = i+1;
               }
              FinalLocationIndex = EditorGUILayout.Popup("Final location", FinalLocationIndex, fLocations);
              if ((FinalLocationIndex > 0) && (FinalLocationIndex <= HLTE.FinalLocations.Count))
                instance.FinalLocation = HLTE.FinalLocations[FinalLocationIndex - 1];
              else
              {
                instance.FinalLocation = null;
                instance.FinalLocationConstraint = "";
              }
            FinalLocationConstraint();
        }

        void StationResultReferenceEditor()
        {
            EditorGUI.indentLevel++;
            var instance = (MMISceneObject)this.target;
            int stationIndex = -1;
            int groupIndex = -1;
            String[] stationList = new string[HLTE.stationsJson.Count];
             for (int i=0; i<HLTE.stationsJson.Count; i++)
             {
                stationList[i] = HLTE.stationsJson[i].station;
                if (HLTE.stationsJson[i].id == instance.StationRef)
                    stationIndex = i;
             }
            stationIndex = EditorGUILayout.Popup("From station", stationIndex, stationList);
            if ((stationIndex >= 0) && (stationIndex < HLTE.stationsJson.Count))
                instance.StationRef = HLTE.stationsJson[stationIndex].id;
            else
                instance.StationRef = 0;
            EditorGUI.indentLevel--;
        }

        void PathEditor(MMIStandard.MPathConstraint PathConstraint, ref bool ConstraintsChanged)
        {
            EditorGUI.indentLevel++;
            for (int i=0; i<PathConstraint.PolygonPoints.Count; i++)
                if (PathConstraint.PolygonPoints[i].__isset.ParentToConstraint)
                {
                  EditorGUILayout.LabelField(PathConstraint.PolygonPoints[i].ParentToConstraint.ID, GUILayout.ExpandWidth(true));  
                }
            EditorGUI.indentLevel--;
        }

        void MarkerEditor(MMIStandard.MPathConstraint PathConstraint, int ConstraitnIndex , ref bool ConstraintsChanged)
        {
            bool selectedConstraint=false;
            GUIStyle styleOn = new GUIStyle();
            styleOn.richText = true;
            styleOn.alignment = TextAnchor.MiddleLeft;
            var instance = (MMISceneObject)this.target;
            EditorGUI.indentLevel++;
            for (int i=0; i<PathConstraint.PolygonPoints.Count; i++)
                if (PathConstraint.PolygonPoints[i].__isset.ParentToConstraint)
                {
                 selectedConstraint=instance.ShowConstraints.Contains(ConstraitnIndex,i);
                 string header = "<size=12><color=white>" +(selectedConstraint?"<b>":"") + PathConstraint.PolygonPoints[i].ParentToConstraint.ID + (selectedConstraint? "</b>" : "") + "</color></size>";
                 GUILayout.BeginHorizontal();
                 GUILayout.Space(EditorGUI.indentLevel*10);
                  if (GUILayout.Button(header,styleOn))   
                  {
                    if (selectedConstraint)
                    instance.ShowConstraints.Remove(ConstraitnIndex,i);
                    else
                    instance.ShowConstraints.Add(ConstraitnIndex,i);

                    UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                    UnityEditor.SceneView.RepaintAll();
                  }
                  GUILayout.FlexibleSpace();
                  GUILayout.EndHorizontal();
                }
            EditorGUI.indentLevel--;
        }

        MMIStandard.MVector3 Vector3Editor(MMIStandard.MVector3 Vector, string grouplabel, ref bool Changed)
        {
        const float labelw = 30;
        const float fieldW = 70;
        const float spacew = -20;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(grouplabel, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(100));
        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("X", GUILayout.MaxWidth(labelw));
        MMIStandard.MVector3 oldV = new MMIStandard.MVector3(Vector.X,Vector.Y,Vector.Z);
        GUILayout.Space(spacew);
        Vector.X = EditorGUILayout.DoubleField(Vector.X, GUILayout.MaxWidth(fieldW));

        EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(labelw));
        GUILayout.Space(spacew);
        Vector.Y = EditorGUILayout.DoubleField(Vector.Y, GUILayout.MaxWidth(fieldW));

        EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(labelw));
        GUILayout.Space(spacew);
        Vector.Z = EditorGUILayout.DoubleField(Vector.Z, GUILayout.MaxWidth(fieldW));
        EditorGUILayout.EndHorizontal();
        Changed = Changed || (Vector.X!=oldV.X) || (Vector.Y!=oldV.Y) || (Vector.Z!=oldV.Z);
        return Vector;
        }

        MMIStandard.MInterval3 LimitEditor(MMIStandard.MInterval3 Limit,string grouplabel, ref bool Changed)
        {
        const float labelw = 30;
        const float fieldW = 70;
        const float spacew = -20;
        
        EditorGUILayout.LabelField(grouplabel, GUILayout.ExpandWidth(true));
        EditorGUI.indentLevel++;
        //min
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(50));
        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("X", GUILayout.MaxWidth(labelw));
        MMIStandard.MInterval3 oldL = new MMIStandard.MInterval3(new MMIStandard.MInterval(Limit.X.Min,Limit.X.Max),
                                                                 new MMIStandard.MInterval(Limit.Y.Min,Limit.Y.Max),
                                                                 new MMIStandard.MInterval(Limit.Z.Min,Limit.Z.Max));
        GUILayout.Space(spacew);
        Limit.X.Min = EditorGUILayout.DoubleField(Limit.X.Min, GUILayout.MaxWidth(fieldW));

        EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(labelw));
        GUILayout.Space(spacew);
        Limit.Y.Min = EditorGUILayout.DoubleField(Limit.Y.Min, GUILayout.MaxWidth(fieldW));

        EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(labelw));
        GUILayout.Space(spacew);
        Limit.Z.Min =  EditorGUILayout.DoubleField(Limit.Z.Min, GUILayout.MaxWidth(fieldW));
        EditorGUILayout.EndHorizontal();

        //max
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(50));
        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField("X", GUILayout.MaxWidth(labelw));

        GUILayout.Space(spacew);
        Limit.X.Max = EditorGUILayout.DoubleField(Limit.X.Max, GUILayout.MaxWidth(fieldW));

        EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(labelw));
        GUILayout.Space(spacew);
        Limit.Y.Max = EditorGUILayout.DoubleField(Limit.Y.Max, GUILayout.MaxWidth(fieldW));

        EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(labelw));
        GUILayout.Space(spacew);
        Limit.Z.Max = EditorGUILayout.DoubleField(Limit.Z.Max, GUILayout.MaxWidth(fieldW));
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        Changed = Changed || (oldL.X.Min!=Limit.X.Min) || (oldL.Y.Min!=Limit.Y.Min) || (oldL.Z.Min!=Limit.Z.Min) ||
                             (oldL.X.Max!=Limit.X.Max) || (oldL.Y.Max!=Limit.Y.Max) || (oldL.Z.Max!=Limit.Z.Max);
        return Limit;
        }

    }
}
