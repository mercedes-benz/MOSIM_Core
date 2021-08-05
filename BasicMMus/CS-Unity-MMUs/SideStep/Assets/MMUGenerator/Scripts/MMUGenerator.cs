// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using MMIStandard;
using MMIUnity;
using MMICSharp.Common;

/// <summary>
/// Class provides functions to build asset bandles, generate dlls and export MMUs
/// </summary>
public class MMUGenerator
{
    /// <summary>
    /// Returns all files from the given path (parses all subfolders)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetFiles(string path)
    {
        Queue<string> queue = new Queue<string>();
        queue.Enqueue(path);
        while (queue.Count > 0)
        {
            path = queue.Dequeue();
            try
            {
                foreach (string subDir in Directory.GetDirectories(path))
                {
                    queue.Enqueue(subDir);
                }
            }
            catch (System.Exception)
            {
            }
            string[] files = null;
            try
            {
                files = Directory.GetFiles(path);
            }
            catch (System.Exception)
            {
            }
            if (files != null)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    yield return files[i];
                }
            }
        }
    }



    /// <summary>
    /// Changes the asset bundle names to the desired name
    /// </summary>
    /// <param name="name"></param>
    public static void ChangeBundleName(string name, GameObject mmuPrefab = null)
    {
        if (mmuPrefab == null) {        
            var assets = Selection.objects.Where(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o))).ToArray();

            foreach (var a in assets)
            {
                string assetPath = AssetDatabase.GetAssetPath(a);
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(name, "");
            }       
        } else { //V2 code
            string assetPath = AssetDatabase.GetAssetPath(mmuPrefab);
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(name, "");
        }
    }

    /// <summary>
    /// Generates the dll with the classed and dependencies
    /// </summary>
    /// <param name="classFiles"></param>
    /// <param name="assemblyFiles"></param>
    /// <param name="path"></param>
    /// <param name="mmuName"></param>
    public static bool GenerateDll(List<string> classFiles, List<string> assemblyFiles, string path, string mmuName)
    {
        //Create the compiler parameters
        CompilerParameters parameters = new CompilerParameters
        {
            GenerateExecutable = false,
            OutputAssembly = path + mmuName + ".dll"
        };


        List<string> classes = new List<string>();

        //Load all scripts
        foreach (string filename in classFiles)
        {
            classes.Add(System.IO.File.ReadAllText(filename));
        }


        //Add all specified assemblies
        foreach (string assemblyFile in assemblyFiles)
        {
            //Copy files to new folder
            File.Copy(assemblyFile, path + Path.GetFileName(assemblyFile));

            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(assemblyFile);

            parameters.ReferencedAssemblies.Add(assembly.Location);
        }

        //Add the unity dll (UnityEngine dll has a different ending since otherwise unity would throw errors)
        System.IO.File.Copy("Assets\\MMUGenerator\\Dependencies\\UnityEngine.dllx", path + System.IO.Path.GetFileName("UnityEngine.dll"));

        parameters.ReferencedAssemblies.Add(path + System.IO.Path.GetFileName("UnityEngine.dll"));



        //Compile the dll
        using(CodeDomProvider com = CodeDomProvider.CreateProvider("CSharp"))
        {
            //Compile the dlls
            CompilerResults r = com.CompileAssemblyFromSource(parameters, classes.ToArray());

            bool success = true;

            if (r.Errors != null && r.Errors.Count >0)
            {
                if (HasErrors(r.Errors))
                {
                    success = false;
                    Debug.Log("Problem compiling the source code. The errors are listed below: ");
                }

                else
                {
                    Debug.Log("Source code compiled. The warnings are listed below: ");
                }

                foreach (CompilerError error in r.Errors)
                {
                    if (!error.IsWarning)
                    {
                        success = false;
                        Debug.LogError(error);
                    }

                    else
                    {
                        Debug.LogWarning(error);
                    }
                }


                Debug.Log("................................................................... ");

            }

            return success;
        }

    }

    /// <summary>
    /// Method indicates whether the compilation caused errors
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    private static bool HasErrors(CompilerErrorCollection errors)
    {
        bool hasErrors = false;

        foreach (CompilerError error in errors)
        {
            //Check if error is no warning
            if (!error.IsWarning)
            {
                hasErrors = true;
                break;
            }

        }
        return hasErrors;
    }

    /// <summary>
    /// Generates the description and stores it on the file system
    /// </summary>
    /// <param name="description"></param>
    /// <param name="path"></param>
    public static void GenerateDescription(MMUDescription description, string path)
    {
        //Get the description and add auto-generated information

        //Create a unique id
        description.AssemblyName = description.Name + ".dll";
        description.Language = "UnityC#";
        description.Dependencies = new List<MDependency>()
        {
            //Add AssetBundle as dependency
            new MDependency(description.Name.ToLower(), MDependencyType.ProgramLibrary, new MVersion(0), new MVersion(1))
            {
                 Name = $"{description.Name.ToLower()}assets"
            }
        };

        System.IO.File.WriteAllText("Assets//" +description.Name + "//description.json", MMICSharp.Common.Communication.Serialization.ToJsonString(description));

        //Save the desciption file
        System.IO.File.WriteAllText(path + "description.json", MMICSharp.Common.Communication.Serialization.ToJsonString(description));
    }


    /// <summary>
    /// Method cleans up the given directory and removes the files that are not relevant for the MMU
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="mmuName"></param>
    public static void CleanUpDirectory(string directory, string mmuName)
    {
        List<string> defaultFiles = GetFiles("Assets//MMUGenerator//Dependencies").Where(s => Path.GetExtension(s) == ".dll").ToList();
        defaultFiles = defaultFiles.Select(s => System.IO.Path.GetFileNameWithoutExtension(s)).ToList();

        string[] files = System.IO.Directory.GetFiles(directory);

        for(int i= 0; i< files.Length; i++)
        {
            //Remove the manifest data
            if (Path.GetExtension(files[i]) == ".manifest")
            {
                File.Delete(files[i]);
            }

            //Remove the asset bundles from other MMUs
            if (Path.GetExtension(files[i]) == "" && !files[i].Contains(mmuName.ToLower()))
            {
                File.Delete(files[i]);
            }


            if (defaultFiles.Exists(s => s.Contains(System.IO.Path.GetFileNameWithoutExtension(files[i]))))
            {
                File.Delete(files[i]);
            }

            if(Path.GetFileName(files[i]) == "UnityEngine.dll")
            {
                File.Delete(files[i]);
            }


        }
    }
}


public class AutoCodeGenerator
{

    [MenuItem("MMI/Advanced/Import Asset Bundle")]
    public static void ImportAssetBundle()
    {
        string path = EditorUtility.OpenFilePanel("Select asset bundle", "", "");
        if (path.Length != 0)
        {
            AssetBundle.UnloadAllAssetBundles(true);

            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);

            Object[] objects = assetBundle.LoadAllAssets();




            foreach (Object obj in objects)
            {
                if (obj is GameObject)
                {
                    GameObject instance = GameObject.Instantiate(obj as GameObject);

                }

                //if (instance.GetComponent<Animator>() != null)
                //{
                //    //AssetDatabase.CreateAsset(instance.GetComponent<Animator>().runtimeAnimatorController, "Assets/animationController.asset");
                //    //instance.GetComponent<Animator>().runtimeAnimatorController
                      
                //}
                //bool success = false;
                //PrefabUtility.SaveAsPrefabAsset(instance, "Assets/" + instance.name+ ".prefab", out success);

            }


            //assetBundle.Unload(true);

        }
    }

    public static void SetupBoneMapping(GameObject mmuInstance)
    {
        UnityMMUBase mmuBase = mmuInstance.GetComponent<UnityMMUBase>();

        Animator animator = mmuInstance.GetComponent<Animator>();

        if (animator != null && mmuBase != null)
        {

            Dictionary<string, MJointType> boneMapping = new Dictionary<string, MJointType>();

            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                try
                {
                    Transform transform = animator.GetBoneTransform(bone);

                    if (transform != null)
                    {
                        boneMapping.Add(transform.name, MMIUnity.UnityJointTypeMapping.ToJointType[bone]);
                        //boneMapping.Add(transform.name, (MJointType)System.Enum.Parse(typeof(MJointType), bone.ToString()));
                    }
                }
                catch (System.Exception)
                {

                }
            }

            //Generate the source code for the bone mapping
            List<string> newLines = new List<string>();
            newLines.Add("\t\t" + "this.BoneTypeMapping = new Dictionary<string,MJointType>()");
            newLines.Add("\t\t" + "{");

            foreach (var entry in boneMapping)
            {
                newLines.Add("\t\t\t" + "{\"" + entry.Key + "\", MJointType." + entry.Value.ToString() + "},");
            }

            newLines.Add("\t\t" + "};");


            //Find the script with the same name
            string file = MMUGenerator.GetFiles("Assets//" + mmuBase.name + "//Scripts//").ToList().Find(s => Path.GetExtension(s) == ".cs");

            if (file == null)
            {
                EditorUtility.DisplayDialog("Automatic code generation failed!", "Cannot find the file of the basic MMU script of thie gameobject. Please ensure that the gameobject has the same nam as the MMUScript.", "Continue");
                return;
            }

            List<string> lines = System.IO.File.ReadAllLines(file).ToList();

            int boneMappingIndex = lines.IndexOf(lines.Find(s => s.Contains("@BoneTypeMapping")));

            if (boneMappingIndex > 0)
            {
                lines[boneMappingIndex] = "\t\t" + "//Auto generated source code for bone type mapping:";



                for (int j = 0; j < newLines.Count; j++)
                {
                    lines.Insert(boneMappingIndex + 1 + j, newLines[j]);
                }
            }

            int boneAssignmentIndex = lines.IndexOf(lines.Find(s => s.Contains("@BoneAssignment")));

            if (boneAssignmentIndex > 0)
            {

                //Insert the source in the start routine
                lines[boneAssignmentIndex] = "\t\t" + "//Auto generated source code for assignemnt of root transform and root bone:";
                lines.Insert(boneAssignmentIndex + 1, "\t\t" + "this.Pelvis = this.gameObject.GetComponentsInChildren<Transform>().First(s=>s.name == \"" + boneMapping.First(s => s.Value == MJointType.PelvisCentre).Key + "\");");
                //lines.Insert(boneAssignmentIndex + 2, "\t\t" + "this.RootTransform = this.gameObject.GetComponentsInChildren<Transform>().First(s=>s.name == \"" + mmuBase.name + "\");");
                lines.Insert(boneAssignmentIndex + 2, "\t\t" + "this.RootTransform = this.transform;");


                //Set the mapping of the current instance in the editor
                mmuBase.Pelvis = mmuBase.GetComponentsInChildren<Transform>().First(s => s.name == boneMapping.First(x => x.Value == MJointType.PelvisCentre).Key);
                mmuBase.RootTransform = mmuBase.GetComponentsInChildren<Transform>().First(s => s.name == mmuBase.name);

            }

            File.WriteAllLines(file, lines.ToArray());
        }
    }

    public static void AutoGenerateScriptInitialization(GameObject mmuInstance)
    {
        UnityMMUBase mmuBase = mmuInstance.GetComponent<UnityMMUBase>();

        if (mmuBase != null)
        {
            //Find the script with the same name
            string file = MMUGenerator.GetFiles("Assets//" + mmuBase.name + "//Scripts//").ToList().Find(s => Path.GetExtension(s) == ".cs");

            List<string> lines = File.ReadAllLines(file).ToList();

            int addScriptsIndex = lines.IndexOf(lines.Find(s => s.Contains("@AddScripts")));

            if (addScriptsIndex > 0)
            {
                lines[addScriptsIndex] = "\t\t" + "//Auto generated source code for script initialization.";

                Component[] components = mmuBase.GetComponents<Component>();

                int index = 0;
                for (int i = 0; i < components.Length; i++)
                {
                    Component component = components[i];

                    //Skip if a unity eninge specific script or the mmu script itself should be added
                    if (component.GetType().ToString().Contains("UnityEngine.") || component.GetType().ToString() == mmuBase.name)
                        continue;

                    index++;
                    lines.Insert(addScriptsIndex + index, "\t\tthis.gameObject.AddComponent<" + components[i].GetType() + ">();");
                }
            }
            File.WriteAllLines(file, lines.ToArray());
        }
    }
}

#endif