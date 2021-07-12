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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using MMIUnity.TargetEngine.Scene;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Cryptography;


namespace MMIUnity.TargetEngine.Editor
{

    [ExecuteInEditMode]
    public class HighLevelTaskEditor : MonoBehaviour
    {
        public int defaultAvatar = 0;
        [Header("Local host configuration")]
        public string port = "80";
        private short currentPort = 0;
        private short[] portsResult = new short[3] { 0, 0, 0 };

        public bool useProxy = false;
        public bool useProxyAuthenictation = false;
        public bool useHttpsProxy = false;
        public string proxyHttps = "";
        public string proxyPort = "";
        public string proxyUser = "";
        private string proxyPassword = "";
        private string regPass = "";

        [Header("Connection properties (use task editor to fill them in)")]
        public string taskEditorWWW = "";
        public string accessToken = "";
        public string lastSyncConnection=""; //sotres combo of taskEditorWWW and accessToken from the last successful sync connection, it is necessary to identify if user starts syncing to a new project then ids have to be set all to zero
        private bool dataModified = false;
        public bool connectionEstablished;
        [Header("Parts and tools in the scene")]
        public List<MMISceneObject> Products = new List<MMISceneObject>();
        public List<MMISceneObject> Tools = new List<MMISceneObject>();
        public List<MMISceneObject> InitialLocations = new List<MMISceneObject>();
        public List<MMISceneObject> FinalLocations = new List<MMISceneObject>();
        public List<MMISceneObject> WalkTargets = new List<MMISceneObject>();
        [Header("Part and tool 3D view extraction")]
        public Camera mainCamera;
        public Camera photoCamera;
        public GameObject target;
        private HttpListener server;
        private HttpClient client;
        private HttpClientHandler httpClientHandler;
        private WebProxy proxy;

        private bool sendTasks = false;
        private ulong taskEditorProjectID = 0;
        private string taskEditorURL = "";
        public string tasEditorProjectName = "";
        private ulong LocalMaxMMIId = 0;
        private ulong LocalMaxMMIAvatarId = 0;
        public int PictureUploadProgress = -1; //negative value means there is no upload happening currently, 0-100 is upload percentage, above 100, upload finished.
        public DateTime FinishedPictureUploading;

        private GLTFExport gltfexporter;
        private Camera MainCamera;
        public Photobooth cameraScript;

        [Header("Stations in project")]
        public bool stationsLoaded = false;

        [Header("Tool types allowed in the scene")]
        public bool tasksLoaded = false;

        public void setProxyPassword(string newpassword)
        {
            string pass = "";
            for (int i = 0; i < proxyPassword.Length; i++)
                pass += "0";
            if ((newpassword != pass) && (newpassword != proxyPassword))
            {
                proxyPassword = newpassword;
                saveProxySettings();
                proxyClient();
            }
        }

        public string getProxyPassword()
        {
            string pass = "";
            for (int i = 0; i < proxyPassword.Length; i++)
                pass += "0";
            return pass;
        }

        public class TPortsStruct : IComparable
        {
            public string port;
            public short preference;

            public TPortsStruct()
            {
                this.port = "80";
                this.preference = 0;
            }

            public TPortsStruct(string Port, short Preference)
            {
                this.port = Port;
                this.preference = Preference;
            }

            // implement IComparable interface
            public int CompareTo(object obj)
            {
                if (obj is TPortsStruct)
                {
                    return -this.preference.CompareTo((obj as TPortsStruct).preference);  // compare preference values in descending order
                }
                throw new ArgumentException("Object is not a TPortStruct");
            }
        }

        public TPortsStruct[] portsToTry = new TPortsStruct[3];

        public enum TSyncStatus
        {
            OutOfSync,
            Synchronizing,
            InSync
        }

        [Serializable]
        public class TJsonTools
        {
            public String projectName;
            public ulong projectID;
            public String type;
            public String[] tools;

            public static TJsonTools CreateFromJSON(string jsonString)
            {
                return JsonUtility.FromJson<TJsonTools>(jsonString);
            }
        }

        [Serializable]
        public class TJsonStations
        {
            public uint id;
            public String station;
        }

        [Serializable]
        public class TJsonWorkers
        {
            public uint id;
            public uint stationid;
            public ulong avatarid;
            public String worker;
            public bool simulate;
            public TSyncStatus syncstatus;
        }

        [Serializable]
        public class TJsonAvatars
        {
            public ulong id;
            public ulong stationid;
            public ulong localID;
            public String avatar;
        }

        public List<TJsonAvatars> avatarJson = new List<TJsonAvatars>();
        public List<TJsonWorkers> workersJson = new List<TJsonWorkers>();
        public List<TJsonStations> stationsJson = new List<TJsonStations>();
        public TJsonTools toolsJson;

        public uint stationID = 0;
        private bool lastStationsLoaded = false;
        private bool lastTaskLoaded = false;

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

        [Serializable]
        public class ToolPartData
        {
            public string name;
            public ulong id;
            public ulong localid;
        }

        [Serializable]
        public class PartData
        {
            public ulong id;
            public string name;
            public ulong engineid;
        }

        public List<ToolPartData> parts = new List<ToolPartData>();

        private void loadProxySettings()
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\MOSIM\\TaskEditor\\Unity");
            if (key != null)
            {
                object val;
                val = key.GetValue("ProxyUser");
                if ((val != null) && (regPass != "") && (regPass.Length == 32))
                {
                    try
                    {
                        proxyUser = DecryptString(regPass, val.ToString());
                    }
                    catch
                    {
                        proxyUser = "";
                    }
                }
                val = key.GetValue("ProxyPass");
                if ((val != null) && (regPass != "") && (regPass.Length == 32))
                {
                    try
                    {
                        proxyPassword = DecryptString(regPass, val.ToString());
                    }
                    catch
                    {
                        proxyPassword = "";
                    }
                }
                val = key.GetValue("ProxyAddress");
                if (val != null)
                    proxyHttps = val.ToString();
                val = key.GetValue("ProxyPort");
                if (val != null)
                    proxyPort = val.ToString();
                val = key.GetValue("UseProxy");
                if (val != null)
                    useProxy = (val.ToString() == "True");
                val = key.GetValue("UseProxyAuthentication");
                if (val != null)
                    useProxyAuthenictation = (val.ToString() == "True");
                val = key.GetValue("UseHTTPS");
                if (val != null)
                    useHttpsProxy = (val.ToString() == "True");
                key.Close();
            }
        }

        public void saveProxySettings()
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\MOSIM\\TaskEditor\\Unity");
            key.SetValue("ProxyUser", EncryptString(regPass, proxyUser));
            key.SetValue("ProxyPass", EncryptString(regPass, proxyPassword));
            key.SetValue("ProxyAddress", proxyHttps);
            key.SetValue("ProxyPort", proxyPort);
            key.SetValue("UseProxy", useProxy);
            key.SetValue("UseHTTPS", useHttpsProxy);
            key.SetValue("UseProxyAuthentication", useProxyAuthenictation);
            key.Close();
        }

        private string userDir()
        {
            string userdir = Environment.GetEnvironmentVariable("userprofile");
            if (userdir.LastIndexOf('\\') != userdir.Length - 1)
                userdir += "\\";
            return userdir;
        }

        private void createRegPass()
        {
            const string chars = "QWERTYUIOPASDFGHJKLMNBVCXZqwertyuioplkjhgfdaszxcvbnm!@#$%^&*()_+=-{}{}|:;?/><,.1234567890";
            var rand = new System.Random();
            regPass = "";
            for (int i = 0; i < 32; i++)
                regPass += chars[rand.Next(0, chars.Length)];

            System.IO.File.WriteAllText(userDir() + ".taskEditorUnity", regPass);
        }

        private void proxyClient()
        {
            if (useProxy)
            {
                proxy = new WebProxy(new Uri($"http{(useHttpsProxy ? "s" : "")}://{proxyHttps}:{proxyPort}"), true);
                proxy.UseDefaultCredentials = false;

                if (useProxyAuthenictation)
                    proxy.Credentials = new NetworkCredential(proxyUser, proxyPassword);


                //(
                //Address = ,
                //BypassProxyOnLocal = true,
                //UseDefaultCredentials = true

                // *** These creds are given to the proxy server, not the web server ***
                /*  Credentials = new NetworkCredential(
              userName: proxyUserName,
              password: proxyPassword)
              };*/
                //);

                // Now create a client handler which uses that proxy
                httpClientHandler = new HttpClientHandler();
                httpClientHandler.Proxy = proxy;


                // Omit this part if you don't need to authenticate with the web server:
                /* if (needServerAuthentication)
                 {
                     httpClientHandler.PreAuthenticate = true;
                     httpClientHandler.UseDefaultCredentials = false;

                     // *** These creds are given to the web server, not the proxy server ***
                     httpClientHandler.Credentials = new NetworkCredential(
                         userName: serverUserName,
                         password: serverPassword);
                 }*/

                // Finally, create the HTTP client object
                client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            }
            else
                client = new HttpClient();
        }

        private static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

        private static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public async Task LoadToolListU(string token, string www)
        {
            if ((www != "") && (token != ""))
            {
                //Debug.Log("task list load: " + this.GetComponentInParent<HighLevelTaskEditor>().toolsJson.type);
                //HttpClient client = new HttpClient();
                //proxyClient();
                var L_data = new List<KeyValuePair<string, string>>();
                L_data.Add(new KeyValuePair<string, string>("action", "getToolList"));
                L_data.Add(new KeyValuePair<string, string>("token", token));
                var post_data = new FormUrlEncodedContent(L_data);
                var content = await client.PostAsync(www, post_data);
                string html = content.Content.ReadAsStringAsync().Result;
                Debug.Log(html);
                //this.GetComponentInParent<HighLevelTaskEditor>().toolsJson.type = "";
                //this.GetComponentInParent<HighLevelTaskEditor>().toolsJson.tools = new string[0];
                try
                {
                    this.toolsJson = JsonUtility.FromJson<TJsonTools>(html);
                    this.tasksLoaded = true;
                    connectionEstablished = true;
                }
                catch
                {
                    connectionEstablished = false;
                }
                if (connectionEstablished)
                {
                    taskEditorProjectID = 0; //TODO: grab value from response, add this value in response in task editor api.php
                    taskEditorURL = taskEditorWWW;
                    tasEditorProjectName = toolsJson.projectName;
                    taskEditorProjectID = toolsJson.projectID;
                }
                //  this.GetComponentInParent<HighLevelTaskEditor>().toolsJson = JsonUtility.FromJson<TJsonTools>(html);
                //  this.GetComponentInParent<HighLevelTaskEditor>().tasksLoaded = true;
                //var HLTE = GameObject.FindObjectOfType<HighLevelTaskEditor>();
                //HLTE.toolsJson = JsonUtility.FromJson<TJsonTools>(html);
                //HLTE.tasksLoaded = true;

                Debug.Log("Tool list has been reloaded from task list editor");
            }
        }

        public async Task LoadStationList(string token, string www)
        {
            if ((www != "") && (token != ""))
            {
                var L_data = new List<KeyValuePair<string, string>>();
                L_data.Add(new KeyValuePair<string, string>("action", "getStationList"));
                L_data.Add(new KeyValuePair<string, string>("token", token));
                var post_data = new FormUrlEncodedContent(L_data);
                var content = await client.PostAsync(www, post_data);
                string html = content.Content.ReadAsStringAsync().Result;
                Debug.Log(html);

                html = "{\"Items\":" + html + "}";
                stationsJson.Clear();
                if (html.IndexOf("id") > 0)
                {
                    TJsonStations[] stationData = JsonHelper.FromJson<TJsonStations>(html);
                    for (int i = 0; i < stationData.Length; i++)
                        this.stationsJson.Add(stationData[i]);
                }
                this.stationsLoaded = true;
                bool checkforId = false;
                for (int i = 0; i < stationsJson.Count; i++)
                    if (stationsJson[i].id == this.stationID)
                        checkforId = true;
                if (!checkforId)
                    if (stationsJson.Count > 0)
                        stationID = stationsJson[0].id;
                    else
                        stationID = 0;
                Debug.Log("Station list has been reloaded from task list editor");
            }
        }

        public async Task SaveWorkerList(string token, string www)
        {
            if ((www != "") && (token != ""))
            {
                var L_data = new List<KeyValuePair<string, string>>();
                L_data.Add(new KeyValuePair<string, string>("action", "setWorkerList"));
                L_data.Add(new KeyValuePair<string, string>("token", token));
                for (int i=0; i<this.workersJson.Count; i++)
                 if (this.workersJson[i].syncstatus==TSyncStatus.OutOfSync)
                 {
                    this.workersJson[i].syncstatus = TSyncStatus.Synchronizing;
                    L_data.Add(new KeyValuePair<string, string>("workerid[]", this.workersJson[i].id.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("simulate[]", this.workersJson[i].simulate.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("avatarid[]", this.workersJson[i].avatarid.ToString()));
                 }
                var post_data = new FormUrlEncodedContent(L_data);
                var content = await client.PostAsync(www, post_data);
                string html = content.Content.ReadAsStringAsync().Result;
                if (html.IndexOf("<result>OK</result>") >= 0)
                {
                    for (int i = 0; i < this.workersJson.Count; i++)
                        if (this.workersJson[i].syncstatus == TSyncStatus.Synchronizing)
                            this.workersJson[i].syncstatus = TSyncStatus.InSync;
                    Debug.Log("Worker list changes have been uploaded to task list editor");
                }
                else
                {
                    for (int i = 0; i < this.workersJson.Count; i++)
                        if (this.workersJson[i].syncstatus == TSyncStatus.Synchronizing)
                            this.workersJson[i].syncstatus = TSyncStatus.OutOfSync;
                    Debug.Log("Worker list could not be synchronized: " + html);
                    Debug.Log("Worker list could not be synchronized: " + GetTagValue(html, "result"));
                }
            }
        }

        public async Task LoadWorkerList(string token, string www)
        {
            if ((www != "") && (token != ""))
            {
                var L_data = new List<KeyValuePair<string, string>>();
                L_data.Add(new KeyValuePair<string, string>("action", "getWorkerList"));
                L_data.Add(new KeyValuePair<string, string>("token", token));
                var post_data = new FormUrlEncodedContent(L_data);
                var content = await client.PostAsync(www, post_data);
                string html = content.Content.ReadAsStringAsync().Result;
                Debug.Log(html);

                html = "{\"Items\":" + html + "}";
                workersJson.Clear();
                if (html.IndexOf("id") > 0)
                {
                    TJsonWorkers[] workerData = JsonHelper.FromJson<TJsonWorkers>(html);
                    for (int i = 0; i < workerData.Length; i++)
                    {
                        workerData[i].syncstatus = TSyncStatus.InSync;
                        this.workersJson.Add(workerData[i]);
                    }
                }

                Debug.Log("Worker list has been reloaded from task list editor");
            }
        }

        public async Task LoadToolList()
        {
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "getToolList"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            Debug.Log(html);
            toolsJson = TJsonTools.CreateFromJSON(html);
            tasksLoaded = true;
        }

        private string CreateBoundaryString()
        {
            const string charArray = "MNBVCXZASDFGHJKLPOIUYTREWQ1234567890qwertyuiopasdfghjklmnbvcxz";
            string hash = "";
            //Create a byte array from source data.
            var rand = new System.Random();
            for (int i = 0; i < 30; i++)
                hash += charArray[rand.Next(0, charArray.Length)];
            return hash;
        }

        private string GetTagValue(string data, string tag)
        {
            var L = tag.Length + 2;
            var a = data.IndexOf("<" + tag + ">");
            var b = data.IndexOf("</" + tag + ">");
            if ((a > -1) && (b > -1))
                return data.Substring(a + L, b - a - L);
            return "";
        }

        public async Task UploadPictures()
        {
            PictureUploadProgress = 0;
            int TotalSize = 0;
            int UploadSize = 0;
            bool abort = false;
            ulong partid = 0;
            string picturePath = Application.dataPath + "/../" + "Screenshots/";

            if (cameraScript != null)
                if (cameraScript.shotpath != "")
                    picturePath = cameraScript.shotpath;

            List <MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            for (var i = 0; i < Scene.Count; i++) //gathering total size of part pictures.
            {
                if (Scene[i].Type == MMISceneObject.Types.Part)
                    if (File.Exists(picturePath + Scene[i].TaskEditorLocalID.ToString() + ".png"))
                    {
                        var pngfile = new FileStream(picturePath + Scene[i].TaskEditorLocalID.ToString() + ".png", FileMode.Open);
                        TotalSize += Convert.ToInt32(pngfile.Length);
                        pngfile.Close();
                        pngfile.Dispose();
                    }
            }
            Debug.Log("Uploading pictures for " + Scene.Count.ToString() + " scene objects from: "+ picturePath);
            for (var j = 0; (j < Scene.Count) && (!abort); j++)
                if ((Scene[j].Type == MMISceneObject.Types.Part) && (File.Exists(picturePath + Scene[j].TaskEditorLocalID.ToString() + ".png")))
                { //main loop
                    partid = Scene[j].TaskEditorID;
                    string inputFile = picturePath + Scene[j].TaskEditorLocalID.ToString() + ".png";
                    Dictionary<string, string> PostData = new Dictionary<string, string>();
                    PostData.Add("action", "addPartPicture");
                    PostData.Add("token", accessToken);
                    PostData.Add("partid", partid.ToString());
                    PostData.Add("partlocalid", Scene[j].TaskEditorLocalID.ToString());

                    var fs = new FileStream(inputFile, FileMode.Open);
                    byte[] buffer = new byte[fs.Length];
                    int readSize = fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    fs.Close();
                    fs.Dispose();

                    var FormData = new MultipartFormDataContent(CreateBoundaryString());
                    for (int i = 0; i < PostData.Keys.Count; i++)
                        FormData.Add(new StringContent(PostData.Values.ElementAt<string>(i)), String.Format("\"{0}\"", PostData.Keys.ElementAt<string>(i)));
                    FormData.Add(new StreamContent(new MemoryStream(buffer, 0, readSize)), "part", partid.ToString() + ".png");

                    var content = await client.PostAsync(taskEditorWWW, FormData);
                    var html = content.Content.ReadAsStringAsync().Result;
                    var result = GetTagValue(html, "result");
                    if (result == "OK")
                    {
                        var partSize = GetTagValue(html, "uploadSize");
                        if (Int32.Parse(partSize) == readSize)
                        {
                            UploadSize += readSize;
                            PictureUploadProgress = UploadSize * 100 / TotalSize;
                        }
                        Debug.Log("Uploaded part " + partid.ToString() + " fsize: " + partSize.ToString());
                    }
                    else
                    {
                        if ((result == "ERR") || (result == "ERR-FATAL"))
                            Debug.Log("Task editor picture (partid=" + partid.ToString() + ") upload error: " + GetTagValue(html, "msg"));
                        if (result == "ERR-FATAL")
                            abort = true;
                    }
                } //main loop

            PictureUploadProgress = 101;
            FinishedPictureUploading = DateTime.Now;
        }

        ulong UpdateUniqueLocalID(MMISceneObject obj)
        {
            if (obj.TaskEditorLocalID == 0)
            {
                LocalMaxMMIId++;
                return LocalMaxMMIId;
            }

            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            foreach (MMISceneObject product in Scene)
                if ((product != obj) && (product.TaskEditorLocalID == obj.TaskEditorLocalID))
                {
                    LocalMaxMMIId++;
                    return LocalMaxMMIId;
                }
            return obj.TaskEditorLocalID;
        }

        ulong UpdateUniqueLocalID(MMIAvatar obj)
        {
            if (obj.getTaskEditorLocalID() == 0)
            {
                LocalMaxMMIAvatarId++;
                return LocalMaxMMIAvatarId;
            }

            List<MMIAvatar> Scene = this.GetComponentsInChildren<MMIAvatar>().ToList();
            foreach (MMIAvatar avatar in Scene)
                if ((avatar != obj) && (avatar.getTaskEditorLocalID() == obj.getTaskEditorLocalID()))
                {
                    LocalMaxMMIAvatarId++;
                    return LocalMaxMMIAvatarId;
                }
            return obj.getTaskEditorLocalID();
        }

        public void updateAvatarList()
        {
            List<MMIAvatar> Scene = this.GetComponentsInChildren<MMIAvatar>().ToList();
            avatarJson.Clear();
            foreach (MMIAvatar avatar in Scene)
            {
                TJsonAvatars item = new TJsonAvatars();
                item.id=avatar.getTaskEditorID();
                item.avatar=avatar.name;
                avatar.setTaskEditorLocalID(UpdateUniqueLocalID(avatar));
                item.localID=avatar.getTaskEditorLocalID();
                MMISceneObject station = avatar.GetParentStation();
                 if (station!=null)
                 item.stationid = station.TaskEditorID;
                 else
                 item.stationid=0;
                avatarJson.Add(item);
            }
        }

        public void updatePartToolList()
        {
            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            parts.Clear();
            InitialLocations.Clear();
            FinalLocations.Clear();
            WalkTargets.Clear();

            foreach (MMISceneObject product in Scene)
            {
                product.TaskEditorLocalID = UpdateUniqueLocalID(product);
                if (product.Type == MMISceneObject.Types.Part)
                {
                    var p = new ToolPartData();
                    p.name = product.name;
                    p.id = product.TaskEditorID;
                    p.localid = product.TaskEditorLocalID;
                    parts.Add(p);
                }
                if (product.Type == MMISceneObject.Types.InitialLocation)
                    InitialLocations.Add(product);
                else
                    if (product.HasConstraint("InitialLocation"))
                    InitialLocations.Add(product);
                if (product.Type == MMISceneObject.Types.FinalLocation)
                    FinalLocations.Add(product);
                else
                    if (product.HasConstraint("FinalLocation"))
                    FinalLocations.Add(product);
                if (product.Type == MMISceneObject.Types.WalkTarget)
                    WalkTargets.Add(product);
            }
        }

        public async Task syncMMIScenObjectsToTaskEditor() //firts sync operation, all MMI Scene Objects data is sent to Task editor
        {
            Debug.Log("Syncing Scene with task editor");
            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "syncScene"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            bool resetids=(lastSyncConnection!=taskEditorWWW+accessToken);
             
            foreach (MMISceneObject product in Scene)
            {               
              string parentID = "0";
              var parentPartGroup = product.GetParentPartOrGroup();
               if (parentPartGroup != null)
               parentID = parentPartGroup.TaskEditorLocalID.ToString();
              string parentStationID = "0";
              var parentStation = product.GetParentStation();
               if (parentStation != null)
               parentStationID = parentStation.TaskEditorLocalID.ToString();
              L_data.Add(new KeyValuePair<string, string>("names[]", product.name));
              L_data.Add(new KeyValuePair<string, string>("MMIIDs[]", product.TaskEditorLocalID.ToString()));
              L_data.Add(new KeyValuePair<string, string>("IDs[]", (resetids?"0":product.TaskEditorID.ToString()))); //<-should this at all be transmitted in this direction?
              L_data.Add(new KeyValuePair<string, string>("types[]", MMISceneObject.TypesToString(product.Type)));
              L_data.Add(new KeyValuePair<string, string>("parents[]", parentID));
              L_data.Add(new KeyValuePair<string, string>("stations[]", parentStationID));
            }
            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            lastSyncConnection=taskEditorWWW+accessToken;
            Debug.Log("Syncing Scene with task editor: " + html); //TODO: add TaskEditor to Scene syncing, currently it is one way Unity->TaskEditor
        }

        public async Task syncConstraintMarkersToTaskEditor()
        {
            Debug.Log("Syncing constraints markers with task editor");
            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "syncMarkers"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            foreach (MMISceneObject product in Scene)
            {
               for (int i = 0; i < product.Constraints.Count; i++)
                if ((product.Constraints[i].ID=="InitialLocation") || (product.Constraints[i].ID == "FinalLocation") || (product.Constraints[i].ID == "WalkTarget"))
                  if (product.Constraints[i].__isset.PathConstraint)
                    for (int j=0; j<product.Constraints[i].PathConstraint.PolygonPoints.Count; j++)
                     if (product.Constraints[i].PathConstraint.PolygonPoints[j].__isset.ParentToConstraint)
                     {
                      string parentID = "0";
                      var parentStation = product.GetParentStation();
                       if (parentStation != null)
                       parentID = parentStation.TaskEditorID.ToString();
                      L_data.Add(new KeyValuePair<string, string>("markerNames[]", product.name));
                      L_data.Add(new KeyValuePair<string, string>("markersMMIIDs[]", product.TaskEditorLocalID.ToString()));
                      L_data.Add(new KeyValuePair<string, string>("markersIDs[]", product.TaskEditorID.ToString()));
                      L_data.Add(new KeyValuePair<string, string>("markersType[]", product.Constraints[i].ID));
                      L_data.Add(new KeyValuePair<string, string>("markersConstraintID[]", product.Constraints[i].PathConstraint.PolygonPoints[j].ParentObjectID));
                      L_data.Add(new KeyValuePair<string, string>("markersConstraint[]", product.Constraints[i].PathConstraint.PolygonPoints[j].ParentToConstraint.ID));
                      L_data.Add(new KeyValuePair<string, string>("parentStation[]", parentID));
                     }
            }
            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            Debug.Log("Syncing constraint markers with task editor: " + html); //TODO: add TaskEditor to Scene syncing, currently it is one way Unity->TaskEditor
        }

        public async Task syncParentsToTaskEditor() //Syncing parents of parts, groups and markers, this has to be done as separate step to make sure all IDs are up to date unless engineids are used for parenting
        {
            Debug.Log("Syncing parent objects with task editor");
            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "syncParents"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            foreach (MMISceneObject product in Scene)
            {
                string markerType = "";
                switch (product.Type)
                {
                    case MMISceneObject.Types.InitialLocation: markerType = "InitialLocation"; break;
                    case MMISceneObject.Types.FinalLocation: markerType = "FinalLocation"; break;
                    case MMISceneObject.Types.WalkTarget: markerType = "WalkTarget"; break;
                }

                if (markerType != "")
                {
                    string parentID = "0";
                    var parentStation = product.GetParentStation();
                    if (parentStation != null)
                        parentID = parentStation.TaskEditorID.ToString();
                    L_data.Add(new KeyValuePair<string, string>("markerNames[]", product.name));
                    L_data.Add(new KeyValuePair<string, string>("markersMMIIDs[]", product.TaskEditorLocalID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("markersIDs[]", product.TaskEditorID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("markersType[]", markerType));
                    L_data.Add(new KeyValuePair<string, string>("parentStation[]", parentID));
                }
            }
            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            Debug.Log("Syncing parent objects with task editor: " + html); //TODO: add TaskEditor to Scene syncing, currently it is one way Unity->TaskEditor
        }

        public async Task syncMarkersToTaskEditor()
        {
            Debug.Log("Syncing markers with task editor");
            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "syncMarkers"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            foreach (MMISceneObject product in Scene)
            {
                string markerType = "";
                switch (product.Type)
                {
                    case MMISceneObject.Types.InitialLocation: markerType = "InitialLocation"; break;
                    case MMISceneObject.Types.FinalLocation: markerType = "FinalLocation"; break;
                    case MMISceneObject.Types.WalkTarget: markerType = "WalkTarget"; break;
                }

                if (markerType != "")
                {
                    string parentID = "0";
                    var parentStation = product.GetParentStation();
                    if (parentStation != null)
                        parentID=parentStation.TaskEditorID.ToString();
                    L_data.Add(new KeyValuePair<string, string>("markerNames[]", product.name));
                    L_data.Add(new KeyValuePair<string, string>("markersMMIIDs[]", product.TaskEditorLocalID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("markersIDs[]", product.TaskEditorID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("markersType[]", markerType));
                    L_data.Add(new KeyValuePair<string, string>("parentStation[]", parentID));
                }
            }
            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            Debug.Log("Syncing markers with task editor: "+html); //TODO: add TaskEditor to Scene syncing, currently it is one way Unity->TaskEditor
        }

        public async Task syncGroupsAndStationsToTaskEditor()
        {
            Debug.Log("Syncing groups and stations with task editor");
            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "syncGroupsAndStations"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            foreach (MMISceneObject product in Scene)
            {
                string markerType = "";
                switch (product.Type)
                {
                    case MMISceneObject.Types.Group: markerType = "group"; break;
                    case MMISceneObject.Types.Station: markerType = "station"; break;
                }

                if (markerType != "")
                {
                    L_data.Add(new KeyValuePair<string, string>(markerType + "Names[]", product.name));
                    L_data.Add(new KeyValuePair<string, string>(markerType + "MMIIDs[]", product.TaskEditorLocalID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>(markerType + "IDs[]", product.TaskEditorID.ToString()));
                }
            }
            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            Debug.Log("Syncing stations: "+html); //TODO: add TaskEditor to Scene syncing, currently it is one way Unity->TaskEditor
            html = "{\"Items\":" + html + "}";
            try
            {
                PartData[] TaskEditorStations = JsonHelper.FromJson<PartData>(html);
                foreach (MMISceneObject product in Scene)
                    if (product.Type == MMISceneObject.Types.Station)
                        for (int i = 0; i < TaskEditorStations.Length; i++)
                            if (product.TaskEditorLocalID == TaskEditorStations[i].engineid)
                            {
                                product.TaskEditorID = TaskEditorStations[i].id;
                                product.name = TaskEditorStations[i].name;
                            }

                for (int i = 0; i < TaskEditorStations.Length; i++)
                    if (TaskEditorStations[i].engineid == 0)
                    {
                        Debug.Log("Added station: " + TaskEditorStations[i].name);
                        var newStation = new GameObject(TaskEditorStations[i].name);
                        newStation.transform.parent = this.gameObject.transform;
                        newStation.AddComponent<MMISceneObject>();
                        newStation.GetComponent<MMISceneObject>().TaskEditorID = TaskEditorStations[i].id;
                        newStation.GetComponent<MMISceneObject>().name = TaskEditorStations[i].name;
                        newStation.GetComponent<MMISceneObject>().TaskEditorLocalID = UpdateUniqueLocalID(newStation.GetComponent<MMISceneObject>());
                        newStation.GetComponent<MMISceneObject>().Type = MMISceneObject.Types.Station;
                    }
                    else
                    {
                        bool found = false;
                        foreach (MMISceneObject product in Scene)
                            if (product.Type == MMISceneObject.Types.Station && product.TaskEditorLocalID == TaskEditorStations[i].engineid)
                            {
                                found = true;
                                product.TaskEditorID=TaskEditorStations[i].id; //update ID from task editor to Unity
                                break;
                            }
                        if (!found)
                        {
                            Debug.Log("Added station: " + TaskEditorStations[i].name);
                            var newStation = new GameObject(TaskEditorStations[i].name);
                            newStation.transform.parent = this.gameObject.transform;
                            newStation.AddComponent<MMISceneObject>();
                            newStation.GetComponent<MMISceneObject>().TaskEditorID = TaskEditorStations[i].id;
                            newStation.GetComponent<MMISceneObject>().name = TaskEditorStations[i].name;
                            newStation.GetComponent<MMISceneObject>().TaskEditorLocalID = TaskEditorStations[i].engineid;
                            newStation.GetComponent<MMISceneObject>().TaskEditorLocalID = UpdateUniqueLocalID(newStation.GetComponent<MMISceneObject>());
                            newStation.GetComponent<MMISceneObject>().Type = MMISceneObject.Types.Station;
                        }

                    }
            }
            catch {
                Debug.Log("Syncing stations: Bad server response");
            };
        }

        private void UpdateAvatar(ref MMIAvatar avatar)
        {
            for (int i=0; i<avatarJson.Count; i++)
                if ((avatar.getTaskEditorLocalID()!=0) && (avatarJson[i].localID==avatar.getTaskEditorLocalID()))
                {
                    avatar.setTaskEditorID(avatarJson[i].id);
                    return;
                }
        }

        public async Task syncAvatarsToTaskEditor() //currenlty one direction synchronization, but should become two way in near future
        {
            Debug.Log("Syncing avatars with task editor");
            List<MMIAvatar> Scene = this.GetComponentsInChildren<MMIAvatar>().ToList();
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "syncAvatars"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            foreach (MMIAvatar avatar in Scene)
            {
               string parentID = "0";
               var parentStation = avatar.GetParentStation();
               if (parentStation != null)
                 parentID=parentStation.TaskEditorID.ToString();
               L_data.Add(new KeyValuePair<string, string>("avatarsNames[]", avatar.name));
               L_data.Add(new KeyValuePair<string, string>("avatarsMMIIDs[]", avatar.getTaskEditorLocalID().ToString()));
               L_data.Add(new KeyValuePair<string, string>("avatarsIDs[]", avatar.getTaskEditorID().ToString()));
               L_data.Add(new KeyValuePair<string, string>("avatarsStation[]", parentID));
            }
            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            Debug.Log("Syncing avatars with task editor: "+html); //TODO: add TaskEditor to Scene syncing, currently it is one way Unity->TaskEditor
            if ((html.IndexOf("id") > 0) && (html.IndexOf("localID") > 0))
            {
                html = "{\"Items\":" + html + "}";
                //TJsonAvatars[] avatarData = JsonHelper.FromJson<TJsonAvatars>(html);               
                avatarJson = JsonHelper.FromJson<TJsonAvatars>(html).ToList();
                for (int j = 0; j < Scene.Count; j++)
                    for (int i = 0; i < avatarJson.Count; i++)
                        if ((avatarJson[i].localID != 0) && (avatarJson[i].localID == Scene[j].getTaskEditorLocalID()))
                        {
                            Scene[j].setTaskEditorID(avatarJson[i].id);
                            //Debug.Log("Changing taskeditorid of avatar " + avatarJson[i].localID.ToString() + " to " + avatarJson[i].id);
                        }
            }
            else
                Debug.Log("Syncing avatars: response not compatible with the current script version");
        }

        public async Task sendDataToTaskEditor() //sending parts and tools
        {
            updatePartToolList();
            updateAvatarList();
            Debug.Log("Sending parts and tools to task editor");
            List<MMISceneObject> Scene = this.GetComponentsInChildren<MMISceneObject>().ToList();
            var L_data = new List<KeyValuePair<string, string>>();
            L_data.Add(new KeyValuePair<string, string>("action", "addParts"));
            L_data.Add(new KeyValuePair<string, string>("token", accessToken));
            Products.Clear();
            Tools.Clear();
            foreach (MMISceneObject product in Scene)
            {
                if (product.Type == MMISceneObject.Types.Part)
                {
                    Products.Add(product);
                    string parentID = "0";
                    string parentStationID = "0";
                    var parentStation = product.GetParentStation();
                    if (parentStation != null)
                        parentStationID = parentStation.TaskEditorID.ToString();
                    var parentPart = product.GetParentPartOrGroup();
                    if (parentPart != null)
                        parentID = parentStation.TaskEditorID.ToString();
                    L_data.Add(new KeyValuePair<string, string>("partsNames[]", product.name));
                    L_data.Add(new KeyValuePair<string, string>("partsMMIIDs[]", product.TaskEditorLocalID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("partsIDs[]", product.TaskEditorID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("partsStation[]", parentStationID));
                    L_data.Add(new KeyValuePair<string, string>("partsParent[]", parentID));
                }
                if (product.Type == MMISceneObject.Types.Tool)
                {
                    Tools.Add(product);
                    L_data.Add(new KeyValuePair<string, string>("toolsNames[]", product.name));
                    L_data.Add(new KeyValuePair<string, string>("toolMMIIDs[]", product.TaskEditorLocalID.ToString()));
                    L_data.Add(new KeyValuePair<string, string>("toolsIDs[]", product.TaskEditorID.ToString()));
                }
                Debug.Log("Part/Tool: " + product.Type + " " + product.name + " " + product.MSceneObject.ID + "/" + product.TaskEditorID.ToString());
            }

            var post_data = new FormUrlEncodedContent(L_data);
            var content = await client.PostAsync(taskEditorWWW, post_data);
            var html = content.Content.ReadAsStringAsync().Result;
            Debug.Log(html);
            html = "{\"Items\":" + html + "}";
            try
            {
                PartData[] partData = JsonHelper.FromJson<PartData>(html);
                for (int i = 0; i < partData.Length; i++)
                    for (int j = 0; j < parts.Count; j++)
                        if (partData[i].engineid == parts[j].localid)
                        {
                            parts[j].id = partData[i].id;
                            parts[j].name = partData[i].name;
                            foreach (MMISceneObject product in Scene)
                                if (product.TaskEditorLocalID == partData[i].engineid)
                                {
                                    product.TaskEditorID = partData[i].id;
                                    product.name = partData[i].name;
                                }
                        }
            }
            catch
            {
                Debug.LogWarning("Part synchronization - reciving data failed data not in proper json format" + html);
            }
            connectionEstablished = (html.IndexOf("engineid") > 0);
            if (connectionEstablished)
            {
                taskEditorProjectID = 0; //TODO: grab value from response, add this value in response in task editor api.php
                taskEditorURL = taskEditorWWW;
            }

            sendTasks = false;
        }

        #region API methods

        public string URLTaskList()
        {
            return taskEditorWWW + "?action=getTaskList&token=" + accessToken + "&station=" + stationID.ToString();
        }

        public string CurrentProjectName()
        {
            return tasEditorProjectName;
        }

        public async void ReloadTools()
        {
            await LoadToolListU(accessToken, taskEditorWWW);
        }

        public async void ReloadStations()
        {
            await LoadStationList(accessToken, taskEditorWWW);
            await SaveWorkerList(accessToken, taskEditorWWW);
            await LoadWorkerList(accessToken, taskEditorWWW);
        }

        public int AvatarIDToIndex(ulong avatarid, bool withDefault) //takes taskeditor avatar id and returns index in the avatarsJson structure
        {
            for (int i=0; i<avatarJson.Count; i++)
            {
                if ((avatarJson[i].id!=0) && (avatarJson[i].id == avatarid))
                    return i+(withDefault?1:0); //if default value should be counted as well, then the result needs to be offset by 1
            }
            return 0;
        }

        public void SetDefaultAvatarByLocalId(ulong AvatarsLocalID)
        {
            for (int i=0; i<avatarJson.Count; i++)
                if (avatarJson[i].localID==AvatarsLocalID)
                {
                    defaultAvatar = i;
                    return;
                }
        }

        public string GetDefaultAvatarName() //this returns default avatar name - this might not be unique as there is no restriction on avatar naming
        {
            updateAvatarList();
            return avatarJson[defaultAvatar].avatar;
        }

        public ulong GetDefaultAvatarID() //returns task editor id for the avatar, this might be zero or correspond to some other avatar before sync
        {
            return avatarJson[defaultAvatar].id;
        }

        public ulong GetDefaultAvatarLocalID() //returns Id that is always unique within the unity scene
        {
            updateAvatarList();
            return avatarJson[defaultAvatar].id;
        }

        public List<uint> GetActiveStationWorkers() //returns list of worker ids that are selected as active for the current station
        {
            List<uint> result = new List<uint>();
            for (int i=0; i<workersJson.Count; i++)
                if ((workersJson[i].stationid==stationID) && (workersJson[i].simulate))
                    result.Add(workersJson[i].id);
            return result;
        }

        public List<uint> GetActiveStationWorkers(ulong station) //returns list of worker ids that are selected as active for the given station ID
        {
            List<uint> result = new List<uint>();
            for (int i=0; i<workersJson.Count; i++)
                if ((workersJson[i].stationid==station) && (workersJson[i].simulate))
                    result.Add(workersJson[i].id);
            return result;
        }

        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }
        
        private void AddGLTFExporter()
        {
            gltfexporter = GameObject.FindObjectOfType<GLTFExport>();
            if (gltfexporter == null)
                gltfexporter=this.gameObject.AddComponent<GLTFExport>();
        }

        private void AddCameraScript()
        {
            Camera[] Cameras = GameObject.FindObjectsOfType<Camera>();
            for (int i = 0; i < Cameras.Length; i++)
             if (Cameras[i].tag == "MainCamera")
             {
                MainCamera = Cameras[i];
                cameraScript = Cameras[i].GetComponent<Photobooth>();
                    if (cameraScript==null)
                    {
                        cameraScript = MainCamera.gameObject.AddComponent<Photobooth>();
                        cameraScript.MainCamera = MainCamera;
                    }
                break;
             }
        }
        
        private async void OnEnable()
        {
            if (cameraScript==null)
            AddCameraScript();
            AddGLTFExporter();
            PictureUploadProgress = 101;
            FinishedPictureUploading = DateTime.Now;
            if (Application.isEditor)
            {
                if ((regPass == "") || (regPass.Length != 32))
                {
                    if (File.Exists(userDir() + ".taskEditorUnity"))
                    {
                        regPass = System.IO.File.ReadAllText(userDir() + ".taskEditorUnity");
                    }
                    else
                        createRegPass();
                }

                if ((regPass != "") && (regPass.Length == 32))
                    loadProxySettings();

                string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(this));
                var index = scriptPath.LastIndexOf("/");
                scriptPath = Application.dataPath + scriptPath.Substring(6, index - 6);
                if (scriptPath.LastIndexOf("/") != scriptPath.Length - 1)
                    scriptPath += "/";

                portsToTry[0] = new TPortsStruct("80", 3);
                portsToTry[1] = new TPortsStruct("8080", 2);
                portsToTry[2] = new TPortsStruct("8081", 1);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                settings.NewLineOnAttributes = true;

                XmlSerializer ser = new XmlSerializer(typeof(TPortsStruct[]));
                if (!System.IO.File.Exists(scriptPath + "config.xml"))
                    using (XmlWriter writer = XmlWriter.Create(scriptPath + "config.xml", settings))
                        ser.Serialize(writer, portsToTry);
                else
                    using (XmlReader reader = XmlReader.Create(scriptPath + "config.xml"))
                    {
                        portsToTry = (TPortsStruct[])ser.Deserialize(reader);
                    }
                if (portsToTry.Length == 0)
                    portsToTry = new TPortsStruct[1];
                Array.Sort(portsToTry);
                currentPort = 0;
                Debug.Log("Cloud project ID: " + Application.cloudProjectId + ", Build guid: " + Application.buildGUID + ", Data path: " + Application.dataPath);
                updatePartToolList();
                updateAvatarList();
                for (short i = 0; i < portsResult.Count(); i++)
                    portsResult[i] = 0;
            }
            StartWebServer();
            proxyClient();
            //client = new HttpClient();
            //Debug.Log("Task Editor web server interface is starting on port "+port+"...");
            if ((!tasksLoaded) && (taskEditorWWW != ""))
            {
                Debug.Log("Task editor: Trying to get tool list...");
                await LoadToolListU(accessToken, taskEditorWWW);
            }
            if ((!stationsLoaded) && (taskEditorWWW != ""))
            {
                Debug.Log("Task editor: Trying to get station list...");
                await LoadStationList(accessToken, taskEditorWWW);
            }
        }

        private void OnDisable()
        {
            if (server != null)
                if (server.IsListening)
                    server.Stop();
            if (client != null)
                client.Dispose();
            Debug.Log("Task editor web server interface is shutting down");
        }

        async Task ServeClients()
        {
            while (server.IsListening)
            {
                HttpListenerContext context = server.GetContext();
                //context: provides access to httplistener's response
                HttpListenerResponse response = context.Response;

                Debug.Log("Web Server: " + context.Request.QueryString);
                string msg = "<html><head><title>MOSIM task editor connector</title></head><body>This is mosim task editor connector<br>" +
                              context.Request.QueryString + "</body></html>";
                //the response tells the server where to send the data

                //this will get the page requested by the browser 
                if (context.Request.Url.LocalPath != string.Empty)  //if there's no page, we'll say it's index.html
                {
                    string page = "c:/temp/" + context.Request.Url.LocalPath;
                    TextReader tr = new StreamReader(page);
                    msg = tr.ReadToEnd();  //getting the page's content
                }
                byte[] buffer = Encoding.UTF8.GetBytes(msg);
                //then we transform it into a byte array
                response.ContentLength64 = buffer.Length;  // set up the messasge's length
                Stream st = response.OutputStream;  // here we create a stream to send the message
                st.Write(buffer, 0, buffer.Length); // and this will send all the content to the browser
                context.Response.Close();  // here we close the connection
            }
        }

        private void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<!DOCTYPE html><html><head><title>MOSIM Scene Web interface</title></head><body><h1>Use the following links to get scene data</h1>" +
                "<h3><ul><li><a href=\"?action=sceneparts\">Parts</a></li><li><a href=\"?action=scenetools\">Tools</a></li></ul></h3></body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            if (context.Request.QueryString["part"] != null)
            {
                string page = "c:/temp/" + context.Request.QueryString["part"] + ".png";
                if (File.Exists(page))
                {
                    BinaryReader tr = new BinaryReader(File.Open(page, FileMode.Open));
                    buffer = tr.ReadBytes((int)(new FileInfo(page).Length));  //getting the page's content
                    response.AppendHeader("content-type", "image/png");
                }
            }
            if (context.Request.QueryString["action"] != null)
            {
                if ((context.Request.QueryString["action"] == "importparts") && (context.Request.QueryString["token"] != null) && (context.Request.QueryString["url"] != null))
                {
                    //UnityEditor.Undo.RecordObject(this, "Task editor connection established");
                    taskEditorWWW = context.Request.QueryString["url"];
                    accessToken = context.Request.QueryString["token"];
                    dataModified = true;

                    if (context.Request.QueryString["name"] != null)
                    {
                        tasEditorProjectName = context.Request.QueryString["name"];
                        Debug.Log("Project name: " + tasEditorProjectName);
                    }
                    sendTasks = true;
                    responseString = "<!DOCTYPE html><html><head><title>MOSIM Scene Web interface</title></head><body><response>OK</response></body></html>";
                    buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                }
                if ((context.Request.QueryString["action"] == "importtools") && (context.Request.QueryString["token"] != null) && (context.Request.QueryString["url"] != null))
                {
                    //UnityEditor.Undo.RecordObject(this, "Task editor connection established");
                    taskEditorWWW = context.Request.QueryString["url"];
                    accessToken = context.Request.QueryString["token"];
                    dataModified = true;

                    if (context.Request.QueryString["name"] != null)
                    {
                        tasEditorProjectName = context.Request.QueryString["name"];
                        Debug.Log("Project name: " + tasEditorProjectName);
                    }
                    ReloadTools();
                    responseString = "<!DOCTYPE html><html><head><title>MOSIM Scene Web interface</title></head><body><response>OK</response></body></html>";
                    buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                }
                if (context.Request.QueryString["action"] == "scenetools")
                {
                    responseString = "{\"tools\":[";

                    //MMISceneObject[] MMISceneObjects = FindObjectsOfType(typeof(MMISceneObject)) as MMISceneObject[];
                    int count = 0;
                    ToolPartData tooldata = new ToolPartData();
                    for (int i = 0; i < Tools.Count; i++)
                    //for (int i = 0; i < MMISceneObjects.Length; i++)
                    //if (MMISceneObjects[i].Type == MMISceneObject.Types.Tool)
                    {
                        tooldata.name = Tools[i].Tool;
                        tooldata.id = Tools[i].TaskEditorID;
                        tooldata.localid = 0;
                        responseString += (count > 0 ? "," : "") + JsonUtility.ToJson(tooldata);
                        count++;
                    }
                    responseString += "]}";
                    buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.AppendHeader("Content-Type", "application/json");
                }
                if (context.Request.QueryString["action"] == "sceneparts")
                {
                    responseString = "{\"parts\":[";
                    try
                    {
                        //                    test[] MMISceneObjects = FindObjectsOfType(typeof(test)) as test[];
                        int count = 0;
                        for (int i = 0; i < parts.Count; i++)
                        //for (int i = 0; i < MMISceneObjects.Length; i++)
                        //if (MMISceneObjects[i].Type == MMISceneObject.Types.Tool)
                        {
                            responseString += (count > 0 ? "," : "") + JsonUtility.ToJson(parts[i]);
                            count++;
                        }
                        responseString += "]}";
                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.AppendHeader("Content-Type", "application/json");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }

            // Get a response stream and write the response to it.
            response.AppendHeader("Access-Control-Allow-Origin", "*");
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }

        async Task WebServerListen()
        {
            while (server.IsListening)
            {
                IAsyncResult result = server.BeginGetContext(new AsyncCallback(ListenerCallback), server);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        void StartWebServer(string changePort = "")
        {
            if (changePort != "")
                port = changePort;
            if (port == "")
                port = portsToTry[currentPort].port;
            server = new HttpListener();  // this is the http server
            server.Prefixes.Add("http://*:" + port + "/");
            //server.Prefixes.Add("http://127.0.0.1:"+port+"/");  //we set a listening address here (localhost)
            //server.Prefixes.Add("http://localhost:"+port+"/");
            try
            {
                server.Start();   // and start the server
                IAsyncResult result = server.BeginGetContext(new AsyncCallback(ListenerCallback), server);
                Debug.Log("Task Editor web server started on port " + port);
            }
            catch (Exception e)
            {
                portsResult[currentPort] = -1;
                short freePorts = 0;
                for (short i = 0; i < portsResult.Count(); i++)
                    if (portsResult[i] >= 0)
                    {
                        freePorts++;
                        currentPort = i;
                        StartWebServer(portsToTry[currentPort].port);
                        break;
                    }
                if (freePorts == 0)
                {
                    Debug.LogWarning("Task Editor: No more ports specified to try for a server, use custom port and update web service configuration accordingly.");
                    Debug.LogWarning(e.Message);
                }

            }
        }

        // Update is called once per frame
        async void Update()
        {
            if (PictureUploadProgress > 100)
                if (FinishedPictureUploading.AddMinutes(1.0) < DateTime.Now)
                    PictureUploadProgress = -1;
            if (dataModified)
            {
                dataModified = false;
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            IAsyncResult result;
            if (server != null)
                if (server.IsListening)
                    result = server.BeginGetContext(new AsyncCallback(ListenerCallback), server);

            if (sendTasks)
                await sendDataToTaskEditor();//.RunSynchronously();

            if ((lastTaskLoaded == true) && (tasksLoaded == false))
                await LoadToolListU(accessToken, taskEditorWWW);

            if ((lastStationsLoaded == true) && (stationsLoaded == false))
                await LoadStationList(accessToken, taskEditorWWW);

            lastStationsLoaded = stationsLoaded;
            lastTaskLoaded = tasksLoaded;
        }

    }

}
