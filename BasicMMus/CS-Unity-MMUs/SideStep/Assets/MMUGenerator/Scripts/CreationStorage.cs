#if UNITY_EDITOR
using MMIStandard;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// This class is responsible for storing and loading MMU creation progress objects.
/// Temporary session or persistent disk storage can be used for load and store operations.
/// </summary>
public static class CreationStorage
{
    public enum Location
    {
        Session,
        Disk,
        Both
    }

    private const string CURRENT_CREATION_NAME = "currentMMUCreation";

    /// <summary>
    /// Saves the given creation progress as current progress.
    /// </summary>
    /// <param name="current">The progress to save</param>
    /// <param name="location">Where the progress should be saved</param>
    public static void SaveCurrent(MMUCreation current, Location location)
    {
        Save(CURRENT_CREATION_NAME, current, location);
    }

    /// <summary>
    /// Tries to load the creation progress that was saved as current one.
    /// </summary>
    /// <param name="location">Where to load the current progress from</param>
    /// <param name="current"></param>
    /// <returns></returns>
    public static bool TryLoadCurrent(Location location, out MMUCreation current)
    {
        current = Load(CURRENT_CREATION_NAME, location);
        return current != null;
    }

    /// <summary>
    /// Deletes the progress which is saved as current.
    /// </summary>
    public static void DeleteCurrent()
    {
        SessionState.EraseString(CURRENT_CREATION_NAME);
        EditorPrefs.DeleteKey(CURRENT_CREATION_NAME);
    }

    /// <summary>
    /// Reads JSON for given key and parses the progress object.
    /// </summary>
    /// <param name="key">The key under which the progress is stored</param>
    /// <param name="location">Where to load the progress from</param>
    /// <returns>The parsed progress</returns>
    private static MMUCreation Load(string key, Location location)
    {
        string json;
        switch (location)
        {
            case Location.Session:
                json = SessionState.GetString(key, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<MMUCreation>(json, new MMUCreationConverter(), new MMUDescriptionConverter());
                }
                break;
            case Location.Disk:
                json = EditorPrefs.GetString(key, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<MMUCreation>(json, new MMUCreationConverter(), new MMUDescriptionConverter());
                }
                break;
            case Location.Both:
                //Prefer session storage
                MMUCreation creation = Load(key, Location.Session);
                if (creation == null)
                {
                    //Try disk as fallback
                    creation = Load(key, Location.Disk);
                }
                return creation;
        }
        return null;
    }

    /// <summary>
    /// Saves the progress as JSON for given key at defined location.
    /// </summary>
    /// <param name="key">The key under which the progress is stored</param>
    /// <param name="creation">The creation progress</param>
    /// <param name="location">Where to store the progress</param>
    private static void Save(string key, MMUCreation creation, Location location)
    {
        string json = JsonConvert.SerializeObject(creation, Formatting.Indented, new MMUCreationConverter(), new MMUDescriptionConverter());
        switch (location)
        {
            case Location.Session:
                SessionState.SetString(key, json);
                break;
            case Location.Disk:
                EditorPrefs.SetString(key, json);
                break;
            case Location.Both:
                Save(key, creation, Location.Session);
                Save(key, creation, Location.Disk);
                break;
        }
    }
}

#region JSON Serialization
/// <summary>
/// This class provides JSON serialization and deserialization of the current MMU creation progress
/// </summary>
public class MMUCreationConverter : JsonConverter<MMUCreation>
{
    public override MMUCreation ReadJson(JsonReader reader, Type objectType, MMUCreation existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        MMUCreation toCreate;
        if (hasExistingValue)
        {
            toCreate = existingValue;
        }
        else
        {
            toCreate = new MMUCreation();
            toCreate.Description = new MMUDescription();
        }
        string currentProperty = string.Empty;
        while (reader.Read())
        {
            //Debug.Log($"TokenType: {reader.TokenType}, Value: {reader.Value}");
            if (reader.TokenType == JsonToken.PropertyName)
            {
                currentProperty = reader.Value as string;
            }
            else if (reader.TokenType == JsonToken.StartObject && currentProperty == "MMUDescription")
            {
                toCreate.Description = serializer.Deserialize<MMUDescription>(reader);
            }
            else if (reader.TokenType != JsonToken.Null && reader.TokenType != JsonToken.StartObject && reader.TokenType != JsonToken.EndObject)
            {
                string assetPath;
                switch (currentProperty)
                {
                    case "Prefab":
                        assetPath = reader.Value as string;
                        toCreate.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        break;
                    case "Instance":
                        int instanceId = Convert.ToInt32(reader.Value);
                        toCreate.Instance = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                        break;
                    case "AnimatorController":
                        assetPath = reader.Value as string;
                        toCreate.AnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
                        break;
                    case "IsMoCapMMU":
                        toCreate.IsMoCapMMU = (bool)reader.Value;
                        break;
                    case "FbxFilePath":
                        toCreate.FbxFilePath = reader.Value as string;
                        break;
                    case "CreationStatus":
                        int status = Convert.ToInt32(reader.Value);
                        toCreate.Status = (MMUCreation.CreationStatus)status;
                        break;
                }
            }

        }
        return toCreate;
    }

    public override void WriteJson(JsonWriter writer, MMUCreation value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        //Serializing MMU description
        writer.WritePropertyName("MMUDescription");
        serializer.Serialize(writer, value.Description);

        //Serializing Prefab
        if (value.Prefab)
        {
            writer.WritePropertyName("Prefab");
            writer.WriteValue(AssetDatabase.GetAssetPath((value.Prefab)));
        }

        //Serializing GameObject Instance
        if (value.Instance)
        {
            writer.WritePropertyName("Instance");
            writer.WriteValue(value.Instance.GetInstanceID());
        }

        //Serializing AnimatorController
        if (value.AnimatorController)
        {
            writer.WritePropertyName("AnimatorController");
            writer.WriteValue(AssetDatabase.GetAssetPath((value.AnimatorController)));
        }

        //Serializing other properties
        writer.WritePropertyName("IsMoCapMMU");
        writer.WriteValue(value.IsMoCapMMU);
        writer.WritePropertyName("FbxFilePath");
        writer.WriteValue(value.FbxFilePath);
        writer.WritePropertyName("CreationStatus");
        writer.WriteValue(value.Status);

        writer.WriteEndObject();
    }
}

/// <summary>
/// This class provides JSON serialization and deserialization of the MMU description nested in the current MMU creation state
/// </summary>
public class MMUDescriptionConverter : JsonConverter<MMUDescription>
{
    public override MMUDescription ReadJson(JsonReader reader, Type objectType, MMUDescription existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        MMUDescription toCreate;
        if (hasExistingValue)
        {
            toCreate = existingValue;
        }
        else
        {
            toCreate = new MMUDescription();
        }
        string currentProperty = string.Empty;
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                currentProperty = reader.Value as string;
            }
            else if (reader.TokenType != JsonToken.Null && reader.TokenType != JsonToken.StartObject && reader.TokenType != JsonToken.EndObject)
            {
                switch (currentProperty)
                {
                    case "ID":
                        toCreate.ID = reader.Value as string;
                        break;
                    case "Name":
                        toCreate.Name = reader.Value as string;
                        break;
                    case "MotionType":
                        toCreate.MotionType = reader.Value as string;
                        break;
                    case "Author":
                        toCreate.Author = reader.Value as string;
                        break;
                    case "ShortDescription":
                        toCreate.ShortDescription = reader.Value as string;
                        break;
                    case "LongDescription":
                        toCreate.LongDescription = reader.Value as string;
                        break;
                    case "AssemblyName":
                        toCreate.AssemblyName = reader.Value as string;
                        break;
                    case "Language":
                        toCreate.Language = reader.Value as string;
                        break;
                    case "Version":
                        toCreate.Version = reader.Value as string;
                        break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
                return toCreate;
            }
        }
        return toCreate;
    }

    public override void WriteJson(JsonWriter writer, MMUDescription value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("ID");
        writer.WriteValue(value.ID);
        writer.WritePropertyName("Name");
        writer.WriteValue(value.Name);
        writer.WritePropertyName("MotionType");
        writer.WriteValue(value.MotionType);
        writer.WritePropertyName("Author");
        writer.WriteValue(value.Author);
        writer.WritePropertyName("ShortDescription");
        writer.WriteValue(value.ShortDescription);
        writer.WritePropertyName("LongDescription");
        writer.WriteValue(value.LongDescription);
        writer.WritePropertyName("AssemblyName");
        writer.WriteValue(value.AssemblyName);
        writer.WritePropertyName("Language");
        writer.WriteValue(value.Language);
        writer.WritePropertyName("Version");
        writer.WriteValue(value.Version);
        writer.WriteEndObject();
    }
}
#endregion
#endif