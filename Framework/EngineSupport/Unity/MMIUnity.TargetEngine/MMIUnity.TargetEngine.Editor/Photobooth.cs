using UnityEngine;
using MMIUnity.TargetEngine.Scene;
using System;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

namespace MMIUnity.TargetEngine.Editor
{
    [ExecuteInEditMode]
    public class Photobooth : MonoBehaviour
    {
        [Header("Background color for pictures")]
        public Color backgroundColor = Color.white;
        public Camera MainCamera;

        [Header("Below are debug parameters only")]
        public Vector3 minxyz = new Vector3();
        public Vector3 maxxyz = new Vector3();
        public Vector3 sizexyz = new Vector3();
        public Vector3 MeshCenter = new Vector3();
        public Vector3 ObjectPosition = new Vector3();
        public GameObject TargetObject = null;
        public MMISceneObject.Types ObjType;
        
        public bool manualmode = false;
        public bool capture = false;
        public Texture2D shot;

        public MeshRenderer[] Meshes = new MeshRenderer[0];
        public SkinnedMeshRenderer[] SkinMeshes = new SkinnedMeshRenderer[0];
        public MMISceneObject[] SceneObjects = new MMISceneObject[0];
        public MonoBehaviour[] scripts;
        public scriptData[] scriptsState;
        private int CurrentIndex = -1;
        private int CapturedIndex = -1;
        private int countdown = 2;
        private string shotpath;

        public void DisableMMIScenObject(int index)
        {
            if ((index >= SceneObjects.Length) || (index < 0))
                return;
            var LocalMeshes = SceneObjects[index].gameObject.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < LocalMeshes.Length; i++)
                LocalMeshes[i].enabled = false;
        }

        public void EnableMMIScenObject(int index)
        {
            if ((index >= SceneObjects.Length) || (index < 0))
                return;
            var LocalMeshes = SceneObjects[index].gameObject.GetComponentsInChildren<MeshRenderer>();


            MeshCenter = new Vector3(0, 0, 0);
            minxyz = new Vector3(0, 0, 0);
            maxxyz = new Vector3(0, 0, 0);
            for (int i = 0; i < LocalMeshes.Length; i++)
            {
                LocalMeshes[i].enabled = true;
                if (i == 0)
                {
                    minxyz = LocalMeshes[i].bounds.center - LocalMeshes[i].bounds.extents;
                    maxxyz = LocalMeshes[i].bounds.center + LocalMeshes[i].bounds.extents;
                }
                else
                {
                    minxyz = Vector3.Min(minxyz, LocalMeshes[i].bounds.center - LocalMeshes[i].bounds.extents);
                    maxxyz = Vector3.Max(maxxyz, LocalMeshes[i].bounds.center + LocalMeshes[i].bounds.extents);
                }
                //MeshCenter += LocalMeshes[i].bounds.center;
            }
            //if (LocalMeshes!=null)
            //MeshCenter = MeshCenter / LocalMeshes.Length;
            MeshCenter = (maxxyz + minxyz) / 2;

            sizexyz = maxxyz - minxyz;
            ObjectPosition = SceneObjects[index].gameObject.transform.position;
            MainCamera.transform.position = (new Vector3(10, 10, 10)) + /*SceneObjects[index].gameObject.transform.position +*/  MeshCenter;
            MainCamera.transform.LookAt(/*SceneObjects[index].gameObject.transform.position +*/ MeshCenter);
            MainCamera.fieldOfView = sizexyz.magnitude * 6;
            MainCamera.clearFlags = CameraClearFlags.SolidColor;
            MainCamera.backgroundColor = backgroundColor;
            TargetObject = SceneObjects[index].gameObject;
            ObjType = SceneObjects[index].Type;
        }

        public void nextObject()
        {
            DisableMMIScenObject(CurrentIndex);
            CurrentIndex++;
            if (CurrentIndex >= SceneObjects.Length)
            {
                CurrentIndex = -1;
                capture = false;
                string fname = shotpath + "miniatures.txt";
                if (File.Exists(fname))
                    File.Delete(fname);
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }
            else
            {
                EnableMMIScenObject(CurrentIndex);
                capture = true;
                countdown = 2;
            }
        }

        public void HideMeshes()
        {
             for (int i = 0; i<Meshes.Length; i++)
                Meshes[i].enabled = false;
             for (int i = 0; i < SkinMeshes.Length; i++)
                SkinMeshes[i].enabled = false;
        }

        public void DisableScripts()
        {
            scripts = GameObject.FindObjectsOfType<MonoBehaviour>();
            Debug.Log("Scripts length: " + scripts.Length.ToString());
            scriptsState = new scriptData[scripts.Length];
            for (int i = 0; i < scripts.Length; i++)
            {
                scriptsState[i] = new scriptData(scripts[i].GetInstanceID(), scripts[i].enabled, scripts[i].GetType().Name);
                if ((scripts[i].GetType().Name == "MMISceneObject") || (scripts[i].GetType().Name == "HighLevelTaskEditorBase")
                    || (scripts[i].GetType().Name == "Photobooth")) //scripts to enable
                    scripts[i].enabled = true;
                else
                {
                    //if ((scripts[i].GetType().BaseType.Name == "MonoBehaviour") && (scripts[i].GetType().Name != "Photobooth"))
                        scripts[i].enabled = false;
                    //if (scripts[i].GetType().IsSubclassOf(typeof(MonoBehaviour)) && (scripts[i].GetType().Name != "Photobooth"))
                    //    scripts[i].enabled = false;
                }
            }
            File.WriteAllText(shotpath + "objectsState.json", JsonHelper.ToJson<scriptData>(scriptsState));
        }

        [Serializable]
        public class scriptData
        {
            public scriptData(int id, bool en, string name)
            {
                this.ID = id;
                this.enabled = en;
                this.Name = name;
            }
            public int ID;
            public bool enabled;
            public string Name;
        }

        public static class JsonHelper
        {
            public static T[] FromJson<T>(string json)
            {
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.Items;
            }

            public static string ToJson<T>(T[] array)
            {
                Wrapper<T> wrapper = new Wrapper<T>();
                wrapper.Items = array;
                return JsonUtility.ToJson(wrapper);
            }

            public static string ToJson<T>(T[] array, bool prettyPrint)
            {
                Wrapper<T> wrapper = new Wrapper<T>();
                wrapper.Items = array;
                return JsonUtility.ToJson(wrapper, prettyPrint);
            }

            [Serializable]
            private class Wrapper<T>
            {
                public T[] Items;
            }
        }

        public void SaveEnableScriptsState()
        {
            scripts = GameObject.FindObjectsOfType<MonoBehaviour>();
            Debug.Log("Scripts length: " + scripts.Length.ToString());
            scriptsState = new scriptData[scripts.Length];
            for (int i = 0; i < scripts.Length; i++)
                scriptsState[i]=new scriptData(scripts[i].GetInstanceID(), scripts[i].enabled, scripts[i].name);
            File.WriteAllText(shotpath + "objectsState.json", JsonHelper.ToJson<scriptData>(scriptsState));
        }

        public void ReenableScripts()
        {
            if (scripts.Length != scriptsState.Length)
                return;

            for (int i = 0; i < scripts.Length; i++)
                scripts[i].enabled = scriptsState[i].enabled;

            scripts = null;
            scriptsState = null;
        }

        public void ReenableScriptsFromFile()
        {
            if (!File.Exists(shotpath + "objectsState.json"))
            {
                Debug.Log("Photobooth: no objectState.json to restore settings");
                return;
            }

            string stateData=File.ReadAllText(shotpath + "objectsState.json");
            try
            {
                scriptsState = JsonHelper.FromJson<scriptData>(stateData);
            }
            catch
            {
                Debug.Log("Photobooth: objectState.json is in incomparible format, file is now deleted");
                File.Delete(shotpath + "objectsState.json");
            }
        }

        public void StartPhotoSessionFromEditorMode()
        {
            if (!(Application.isEditor && !Application.isPlaying))
                return;
            CreateShotDirectory();            
            string Fname = shotpath + "miniatures.txt";
            if (!Directory.Exists(shotpath))
                Directory.CreateDirectory(shotpath);
            if (File.Exists(Fname))
                File.Delete(Fname);
            File.WriteAllText(Fname, "run");
            //SaveEnableScriptsState();
            DisableScripts();
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        public void CreateShotDirectory()
        {
            shotpath = MMISettings.BasePath()+MMISettings.Instance.ShotFolder;
            if (!(shotpath.EndsWith("/") || shotpath.EndsWith("\\")))
                shotpath+="/";

            if (!Directory.Exists(shotpath))
                Directory.CreateDirectory(shotpath);
        }

        public void OnEnable()
        {             
            if (!Application.isPlaying)
            {
                ReenableScripts();
                return;
            }
            capture = false;
            Meshes = GameObject.FindObjectsOfType<MeshRenderer>();
            SkinMeshes = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
            SceneObjects = GameObject.FindObjectsOfType<MMISceneObject>();
        
                CreateShotDirectory();
                string runfile = shotpath + "miniatures.txt";
                if (File.Exists(runfile))
                    if (File.ReadAllText(runfile) == "run")
                    {
                        HighLevelTaskEditor HLTE = GameObject.FindObjectOfType<HighLevelTaskEditor>();
                         if (HLTE!=null)
                         HLTE.updatePartToolList();
                        HideMeshes();
                        capture = true;
                    }
        }

        void Update()
        {
            if (!manualmode)
            if (capture)
                if (CurrentIndex == CapturedIndex)
                    nextObject();
        }

        private void OnPostRender()
        {
            if (!capture)
                return;
            if (countdown > 0)
            {
                countdown--;
                return;
            }
            shot = new Texture2D(Mathf.RoundToInt(MainCamera.pixelWidth), Mathf.RoundToInt(MainCamera.pixelHeight), TextureFormat.ARGB32, false);
            shot.ReadPixels(new Rect(0, 0, MainCamera.pixelWidth, MainCamera.pixelHeight), 0, 0, false);
            shot.Apply();
            int counts;
            int LeftFree = -1;
            int RightFree = -1;
            Color32[] pixels = shot.GetPixels32();
            for (int x = 0; (x < shot.width) && (LeftFree == -1); x++)
            {
                counts = 0;
                for (int y = 0; y < shot.height; y++)
                    if (pixels[x + y * shot.width] != MainCamera.backgroundColor)
                    {
                        LeftFree = x;
                        break;
                    }
            }

            if (LeftFree < shot.width - 1)
                for (int x = shot.width - 1; (x > 0) && (RightFree == -1); x--)
                {
                    counts = 0;
                    for (int y = 0; y < shot.height; y++)
                        if (pixels[x + y * shot.width] != MainCamera.backgroundColor)
                        {
                            RightFree = x;
                            break;
                        }
                }
            if (shot.height < shot.width)
            {
                if (RightFree - LeftFree < shot.height)
                {
                    var dx = (shot.height - (RightFree - LeftFree)) / 2;
                    if (dx < LeftFree)
                        LeftFree = LeftFree - dx;
                    else
                        LeftFree = 0;
                    if (dx + RightFree < shot.width)
                        RightFree += dx;
                    else
                        RightFree = shot.width;

                    if (RightFree - LeftFree < shot.height)
                    {
                        dx = shot.height - (RightFree - LeftFree);
                        if (dx <= LeftFree)
                            LeftFree = LeftFree - dx;
                        else
                            if (dx + RightFree <= shot.width)
                            RightFree += dx;
                    }
                }
                var cropPixels = shot.GetPixels(LeftFree,0, RightFree - LeftFree, shot.height);
                shot = new Texture2D(RightFree - LeftFree, shot.height);
                shot.SetPixels(cropPixels);
                shot.Apply();
            }

            System.IO.File.WriteAllBytes(shotpath +"/"+ SceneObjects[CurrentIndex].TaskEditorLocalID.ToString() + ".png", shot.EncodeToPNG());
            CapturedIndex = CurrentIndex;
        }

    }

}
