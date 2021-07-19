#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Callbacks;
using UnityEngine;

public class AssetImportHelper : AssetPostprocessor
{
    public static Dictionary<string, Action> PendingMotionImports = new Dictionary<string, Action>();

    void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (PendingMotionImports.ContainsKey(importer.assetPath))
        {
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.SaveAndReimport();
            }
        }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            Debug.Log("Imported Asset: " + str);
            if (PendingMotionImports.TryGetValue(str, out Action callback))
            {
                callback();
                PendingMotionImports.Remove(str);
            }
        }
        foreach (string str in deletedAssets)
        {
            Debug.Log("Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }
    }
}
#endif