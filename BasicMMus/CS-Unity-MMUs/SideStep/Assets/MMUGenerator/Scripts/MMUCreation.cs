#if UNITY_EDITOR
using MMIStandard;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// This class reflects the state of the current MMU creation process.
/// </summary>
public class MMUCreation : IDisposable
{
    /// <summary>
    /// The MMU description of the MMU which is created by this process.
    /// </summary>
    public MMUDescription Description { get; set; }

    /// <summary>
    /// Reference to the prefab asset which has been created by this process.
    /// </summary>
    public GameObject Prefab { get; set; }

    /// <summary>
    /// Reference to the GameObject which has been created by this process.
    /// </summary>
    public GameObject Instance { get; set; }

    /// <summary>
    /// Reference to the mechanim animator controller, which is used by MoCap MMUs
    /// </summary>
    public AnimatorController AnimatorController { get; set; }

    /// <summary>
    /// Indicates if the MMU creation process refers to a MoCap MMU or not.
    /// </summary>
    public bool IsMoCapMMU { get; set; }

    /// <summary>
    /// The file path to a fbx file containing an animation clip which is automatically
    /// attached to the animator controller of MoCap MMUs
    /// </summary>
    public string FbxFilePath { get; set; }

    private bool _disposed = false;

    public enum CreationStatus
    {
        Created,
        FilesSetup,
        AnimationSetup,
        Completed
    }

    /// <summary>
    /// Reflects the current status of the creation process.
    /// </summary>
    public CreationStatus Status { get; set; }

    public MMUCreation()
    {
        this.Description = new MMUDescription();
        this.IsMoCapMMU = false;
        this.FbxFilePath = string.Empty;
        this.Status = CreationStatus.Created;
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose() => Dispose(true);

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
           // Dispose managed state (managed objects).
           if (this.Instance != null)
           {
                GameObject.DestroyImmediate(this.Instance);
           }
           string directoryName = $"Assets/{this.Description.Name}";
           if (Directory.Exists(directoryName))
           {
                Directory.Delete(directoryName, true);
           }
           AssetDatabase.Refresh();
        }

        _disposed = true;
    }
}
#endif