using UnityEngine;
using UnityEditor;

public class BuildUnityAdapter{

    public static void CreateServerBuild ()
    {
        Debug.Log("Building Unity Adapter Server Build"); 
        string[] scenes = new string[] {"Assets/main.unity"};
        BuildPipeline.BuildPlayer(scenes,"./build/UnityAdapter.exe", BuildTarget.StandaloneWindows, BuildOptions.EnableHeadlessMode);
    }
}