// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using MMIStandard;
using MMICSharp.Common.Communication;
using MMIUnity;

/// <summary>
/// Window for editiing MMU Descriptions.
/// </summary>
public class AddParameterWindow : EditorWindow
{
    MParameter parameter = new MParameter();

    private EditDescriptionWindow editWindow;

    public AddParameterWindow(EditDescriptionWindow editWindow)
    {
        this.editWindow = editWindow;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Add Parameter:");
        parameter.Name = EditorGUILayout.TextField("Name", parameter.Name);
        parameter.Type = EditorGUILayout.TextField("Type", parameter.Type);
        parameter.Description = EditorGUILayout.TextField("Description", parameter.Description);
        parameter.Required = EditorGUILayout.Toggle("Required", parameter.Required);


        if (GUILayout.Button("Ok"))
        {
            editWindow.description.Parameters.Add(parameter);
            Close();
        }


        if (GUILayout.Button("Abort"))
            Close();
    }
}

/// <summary>
/// Window for editiing MMU Descriptions.
/// </summary>
public class EditDescriptionWindow : EditorWindow
{

    /// <summary>
    /// The assigned description file
    /// </summary>
    public MMUDescription description = new MMUDescription()
    {
        Properties = new Dictionary<string, string>(),
        Parameters = new List<MParameter>()
    };

    /// <summary>
    /// Dictionary which contains the selection state of the parameters
    /// </summary>
    private Dictionary<MParameter, bool> parameterSelection = new Dictionary<MParameter, bool>();


    void Awake()
    {
        this.parameterSelection = new Dictionary<MParameter, bool>();
        this.LoadDescription();
    }





    void OnGUI()
    {
        EditorGUILayout.LabelField("Please enter the relevant description of the desired MMU:");

        description.ID = EditorGUILayout.TextField("ID", description.ID);
        description.Name = EditorGUILayout.TextField("Name", description.Name);
        description.MotionType = EditorGUILayout.TextField("MotionType", description.MotionType);
        description.Author = EditorGUILayout.TextField("Author", description.Author);
        description.ShortDescription = EditorGUILayout.TextField("ShortDescription", description.ShortDescription);
        description.LongDescription = EditorGUILayout.TextField("LongDescription", description.LongDescription);  

        if (description.Parameters != null && description.Parameters.Count >0)
        {
            EditorGUILayout.LabelField("Parameters:");
            foreach (var param in description.Parameters)
            {
                if (!parameterSelection.ContainsKey(param))
                {
                    parameterSelection.Add(param, false);
                }
                parameterSelection[param] = EditorGUILayout.ToggleLeft("Name: " + param.Name + ", Type: " + param.Type + ", Required: " + param.Required.ToString() + ", Description: " + param.Description, parameterSelection[param]);
            }
        }

        if (GUILayout.Button("Add Parameter"))
        {
            AddParameterWindow window = new AddParameterWindow(this);
            window.Show();
        }

        if (this.parameterSelection.Values.ToList().Exists(s=>s == true) && GUILayout.Button("Remove Parameter"))
        {
            //Remove the selected parameters
            List<MParameter> selections = this.parameterSelection.Keys.Where(s => this.parameterSelection[s] == true).ToList();

            foreach(MParameter parameter in selections)
            {
                this.description.Parameters.Remove(parameter);
            }
        }

        if (GUILayout.Button("Save"))
        {
            File.WriteAllText("Assets//" + this.description.Name + "//description.json", Serialization.ToJsonString(this.description));

            AssetDatabase.Refresh();

            //Close the window
            Close();
        }

        if (GUILayout.Button("Abort"))
        {
            this.LoadDescription();

            this.description = null;
            Close();
        }
    }

    [MenuItem("Assets/Edit Description")]
    public static void EditDescription()
    {
        EditDescriptionWindow window = new EditDescriptionWindow();
        window.Show();
    }

    [MenuItem("Assets/Edit Description", true)]
    public static bool EditDescriptionValidation()
    {
        return Selection.activeObject.name == "description";
    }

    private void LoadDescription()
    {
        try
        {
            string jsonString = Selection.activeObject.ToString();

            this.description = Serialization.FromJsonString<MMUDescription>(jsonString);

            if (description.Properties == null)
                description.Properties = new Dictionary<string, string>();

            if (description.Parameters == null)
                description.Parameters = new List<MParameter>();

            Debug.Log("Available description file loaded");
        }
        catch (System.Exception)
        {
            Debug.Log("Problem loading description file! A new one is created.");
        }
    }
}

#endif