// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Klodowski

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using MMICSharp.Common.Communication;
using MMILauncher.Core;
using MMIStandard;
using System.Net.Http;
using System.Windows;
using System.Security.Cryptography;
using System.Globalization;

namespace MMIULibrary
{
    [Serializable]
    public class RemoteMMUList
    {
        public string url { get; set; }
        public string vendorID { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string motiontype { get; set; }
        public uint sortorder { get; set; }
        public bool enabled { get; set; }
        public bool inLocal { get; set; }
        public bool excludeFromSync { get; set; }

        public Visibility inLocalImg { get { return (inLocal ? Visibility.Visible : Visibility.Hidden); } }
    }

    [Serializable]
    public class LibraryLink
    {
        public string url;
        public string token;
        public string name;
        /*
        public LibraryLink(string _url, string _token, string _name)
        {
            this.url = _url;
            this.token = _token;
            this.name = _name;
        }*/
    }

    public class LibraryDetails
    {
        public LibraryDetails(int Index, string Name)
        {
            this.index = Index;
            this.name = Name;
            this.canDownload = (Index >= 0);
            this.canRemove = (Index == -1);
            this.canAdd = (Index == -1);
        }

        public LibraryDetails(int Index, string Name, bool allowRemove, bool allowAdd, bool allowDownload)
        {
            this.index = Index;
            this.name = Name;
            this.canDownload = allowDownload;
            this.canRemove = allowRemove;
            this.canAdd = allowAdd;
        }

        public int index { get; set; } //negative index means local library, positive index is remote library
        public string name { get; set; }
        public bool canRemove { get; set; }
        public bool canAdd { get; set; }
        public bool canDownload { get; set; }
        public bool isLocal { get { return index < 0; } }
        public bool isRemote { get { return index >= 0; } }
    }

    public class RemoteLibrary
    {
        public RemoteLibrary(string url, string token)
        {
            this.URL = url;
            this.Token = token;
            this.MMUs = new RemoteMMUList[0];
        }

        public RemoteLibrary(string url, string token, string name)
        {
            this.URL = url;
            this.Token = token;
            this.Name = name;
            this.MMUs = new RemoteMMUList[0];
        }

        public RemoteLibrary(LibraryLink link)
        {
            this.URL = link.url;
            this.Token = link.token;
            this.Name = link.name;
            this.MMUs = new RemoteMMUList[0];
        }

        public RemoteLibrary(LibraryLink link, string fileName)
        {
            this.URL = link.url;
            this.Token = link.token;
            this.Name = link.name;
            this.LocalFileName = fileName;
            this.MMUs = new RemoteMMUList[0];
        }

        public void UpdateConnection(HttpClient client)
        {
            _client = client;
        }

        private bool _canDownload;
        private bool _canUpload;
        private bool _canRemove;
        private long _chunkSize = 0;
        private HttpClient _client;

        public long chunkSize { get { return _chunkSize; } }
        public string Token;
        public string URL;
        public string Name { get; set; }
        public string LocalFileName;
        public bool canDownload { get { return _canDownload; } }
        public bool canUpload { get { return _canUpload; } }
        public bool canRemove { get { return _canRemove; } }
        public RemoteMMUList[] MMUs;

        private string GetTagValue(string data, string tag)
        {
            var L = tag.Length + 2;
            var a = data.IndexOf("<" + tag + ">");
            var b = data.IndexOf("</" + tag + ">");
            if ((a > -1) && (b > -1))
                return data.Substring(a + L, b - a - L);
            return "";
        }

        public async Task GetSettings()
        {
            Dictionary<string, string> PostData = new Dictionary<string, string>();
            PostData.Add("token", Token);
            PostData.Add("action", "getSettings");

            string html = "";
            var PostForm = new FormUrlEncodedContent(PostData);
            try
            {
                var content = await _client.PostAsync(URL, PostForm);
                html += content.Content.ReadAsStringAsync().Result;
                var sChunkSize = GetTagValue(html, "chunkSize");
                if (sChunkSize != "")
                    _chunkSize = long.Parse(sChunkSize);
                var scanDownload = GetTagValue(html, "canDownload");
                if (scanDownload != "")
                    _canDownload = (scanDownload == "True");
                var scanUpload = GetTagValue(html, "canUpload");
                if (scanUpload != "")
                    _canUpload = (scanUpload == "True");
                var scanRemove = GetTagValue(html, "canRemove");
                if (scanRemove != "")
                    _canRemove = (scanRemove == "True");
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    html = "Connection error to MMU Library "+URL+": \r\n" + err.InnerException.Message;
                else
                    html = "Connection error to MMU Library "+URL+": \r\n" + err.Message;
                System.Windows.MessageBox.Show(html, "Remote MMU Library settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public async Task GetSettingsIfNecessary()
        {
            if (_chunkSize == 0)
                await GetSettings();
            if (_chunkSize == 0)
                _chunkSize = 1024 * 128;
        }

        public async Task GetMMUListFromServer()
        {
            Dictionary<string, string> PostData = new Dictionary<string, string>();
            PostData.Add("token", Token);
            PostData.Add("action", "getMMUList");

            string html = "";
            var PostForm = new FormUrlEncodedContent(PostData);
            try
            {
                var content = await _client.PostAsync(URL, PostForm);
                html = content.Content.ReadAsStringAsync().Result;
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    html = "Connection error to MMU Library: \r\n" + err.InnerException.Message;
                else
                    html = "Connection error to MMU Library: \r\n" + err.Message;
                System.Windows.MessageBox.Show(html, "Remote MMU Library", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                MMUs = Serialization.FromJsonString<RemoteMMUList[]>(html);
            }
            catch
            {
                System.Windows.MessageBox.Show("Serialization error of " +URL+ "\r\n" + html, "Remote MMU Library", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async Task RemoveMMU(string id)
        {
            if (!_canRemove)
                return;

            Dictionary<string, string> PostData = new Dictionary<string, string>();
            PostData.Add("token", Token);
            PostData.Add("action", "removeMMU");
            PostData.Add("vendorID", id);

            string html = "";
            var PostForm = new FormUrlEncodedContent(PostData);
            try
            {
                var content = await _client.PostAsync(URL, PostForm);
                html += content.Content.ReadAsStringAsync().Result;
            
                var scanRemove = GetTagValue(html, "result");

                if (scanRemove == "ERR")
                {
                    System.Windows.MessageBox.Show(GetTagValue(html, "msg"), "Remote MMU delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (scanRemove=="")
                    try
                    {
                        MMUs = Serialization.FromJsonString<RemoteMMUList[]>(html);
                    }
                    catch
                    {
                        html = "MMU list serialization error";
                        System.Windows.MessageBox.Show(html, "Remote MMU delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    html = "Connection error to MMU Library " + URL + ": \r\n" + err.InnerException.Message;
                else
                    html = "Connection error to MMU Library " + URL + ": \r\n" + err.Message;
                System.Windows.MessageBox.Show(html, "Remote MMU delete", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    
    public class CopyPair
    {
        public CopyPair(int sourceMmuId, int sourceLibraryId, int destLibraryId)
        {
            this.mmu = sourceMmuId;
            this.sourceLibrary = sourceLibraryId;
            this.destLibrary = destLibraryId;
        }

        public int mmu;
        public int sourceLibrary;
        public int destLibrary;
    }

    public class UpDownPair
    {
        public UpDownPair(int mmuID, int libraryID)
        {
            this.mmu = mmuID;
            this.library = libraryID;
            this.zipFile = "";
        }

        public UpDownPair(string zipFileName, int libraryID)
        {
            this.mmu = -1;
            this.zipFile = zipFileName;
            this.library = libraryID;
        }

        public int mmu;
        public int library;
        public string zipFile;
    }

    /*
    public class MyList : List<UpDownPair> //example of list extension with custom add method.
    {
        public void Add(int a, int b)
        {
            this.Add(new UpDownPair(a, b));
        }
    }
    */
    public class MMULibrary
    {
        private long chunkSize = 0;
        public List<MMUOrderAndDescriptionData> MMUs = new List<MMUOrderAndDescriptionData>();
        private HttpClient client;
        string RemoteLibraryUrl;
        string RemoteLibraryToken;
        string LocalMMULibrary;
        string localZipMMULibrary;
        private List<CopyPair> CopyMMU = new List<CopyPair>();
        private List<UpDownPair> UploadMMU = new List<UpDownPair>();
        private List<UpDownPair> DownloadMMU = new List<UpDownPair>();
        private bool isUploading = false;
        private bool isDownloading = false;
        public RemoteMMUList[] RemoteMMUs;
        public List<RemoteLibrary> Remotes = new List<RemoteLibrary>();

        public delegate void Progress(long max, long progress);

        public int SyncUpCount()
        {
            return UploadMMU.Count;
        }

        public int SyncDownCount()
        {
            return DownloadMMU.Count;
        }

        public void ConfigureConnection(HttpClient httpClient, string MMULibraryPath, string mmuZipLibraryPath, string MMULibraryUrl, string MMULibraryToken)
        {
            client = httpClient;
            RemoteLibraryUrl = MMULibraryUrl;
            RemoteLibraryToken = MMULibraryToken;
            LocalMMULibrary = MMULibraryPath;
            localZipMMULibrary = mmuZipLibraryPath;
        }

        public void ClearUploadList()
        {
            UploadMMU.Clear();
        }

        public void ClearDownloadList()
        {
            DownloadMMU.Clear();
        }

        public void ClearCopyList()
        {
            CopyMMU.Clear();
        }

        public void AddToCopy(int sourceMMU, int sourceLibrary, int destLibrary)
        {
            CopyMMU.Add(new CopyPair(sourceMMU, sourceLibrary, destLibrary));
        }

        public void AddToUpload(int localMmuIndex,int remoteLibraryIndex)
        {
            UploadMMU.Add(new UpDownPair(localMmuIndex, remoteLibraryIndex));
        }

        public void AddToUpload(string zipFile, int remoteLibraryIndex)
        {
            UploadMMU.Add(new UpDownPair(zipFile, remoteLibraryIndex));
        }

        public void AddToDownload(int remoteMmuIndex, int remoteLibraryIndex)
        {
            DownloadMMU.Add(new UpDownPair(remoteMmuIndex, remoteLibraryIndex));
        }

        public void AddToDownload(string url, int remoteLibraryIndex)
        {
            DownloadMMU.Add(new UpDownPair(url, remoteLibraryIndex));
        }

        public bool FindLibrary(LibraryLink library)
        {
            for (int i = 0; i < Remotes.Count; i++)
                if ((Remotes[i].URL == library.url) && (Remotes[i].Token == library.token))
                    return true;
            return false;
        }

        public bool FindLibrary(string url, string token)
        {
            for (int i = 0; i < Remotes.Count; i++)
                if ((Remotes[i].URL == url) && (Remotes[i].Token == token))
                    return true;
            return false;
        }

        public void AddRemote(RemoteLibrary newLib, HttpClient client)
        {
            Remotes.Add(newLib);
            Remotes[Remotes.Count - 1].UpdateConnection(client);
        }

        public void InsertRemote(int InsertIndex, RemoteLibrary newLib, HttpClient client)
        {
            Remotes.Insert(InsertIndex,newLib);
            Remotes[InsertIndex].UpdateConnection(client);
        }

        public void ScanLibrary()
        {
            MMUs.Clear();
            string[] mmuFolders = Directory.GetDirectories(LocalMMULibrary);
            for (int i = 0; i < mmuFolders.Length; i++)
            {
                string descFile = mmuFolders[i] + (mmuFolders[i].EndsWith("/") || mmuFolders[i].EndsWith("\\") ? "" : "\\") + "description.json";
                if (File.Exists(descFile))
                {
                    var mmuDesc = Serialization.FromJsonString<MMUDescription>(File.ReadAllText(descFile));
                    var mmuExtDesc = new MMUOrderAndDescriptionData(mmuDesc, 0, mmuFolders[i]);
                    MMUs.Add(mmuExtDesc);
                }
            }
        }

        public void ScanMMU(string mmuPath)
        {
            string descFile = mmuPath + (mmuPath.EndsWith("/") || mmuPath.EndsWith("\\") ? "" : "\\") + "description.json";
            if (File.Exists(descFile))
            {
                var mmuDesc = Serialization.FromJsonString<MMUDescription>(File.ReadAllText(descFile));
                var mmuExtDesc = new MMUOrderAndDescriptionData(mmuDesc, 0, mmuPath);
                MMUs.Add(mmuExtDesc);
            }
        }

        public void CompareRemoteAndLocal(int remoteIndex)
        {
            bool found = false;

            for (int i = 0; i < Remotes[remoteIndex].MMUs.Length; i++)
            {
                found = false;
                for (int j = 0; (j < MMUs.Count) && !found; j++)
                    if (Remotes[remoteIndex].MMUs[i].vendorID == MMUs[j].ID)
                        found = true;
                Remotes[remoteIndex].MMUs[i].inLocal = found;
            }
        }

        public void CompareAndPackRemoteAndLocal(int remoteIndex)
        {
            if (isUploading || isDownloading)
                return;
            
            UploadMMU.Clear();
            DownloadMMU.Clear();
            bool found = false;
            for (int i = 0; i < MMUs.Count; i++)
            {
                found = false;
                for (int j = 0; j < Remotes[remoteIndex].MMUs.Length; j++)
                    if (Remotes[remoteIndex].MMUs[j].vendorID == MMUs[i].ID)
                        found = true;
                if (!found)
                {
                    Pack(i);
                    UploadMMU.Add(new UpDownPair(i, remoteIndex));
                }
            }

            for (int i = 0; i < Remotes[remoteIndex].MMUs.Length; i++)
            {
                found = false;
                for (int j = 0; (j < MMUs.Count) && !found; j++)
                    if (Remotes[remoteIndex].MMUs[i].vendorID == MMUs[j].ID)
                        found = true;
                Remotes[remoteIndex].MMUs[i].inLocal = found;
                if (!found)
                    DownloadMMU.Add(new UpDownPair(i, remoteIndex));
            }
        }

        public async Task GetMMUListFromServer()
        {
            if (RemoteLibraryUrl=="")
            {
                RemoteMMUs = new RemoteMMUList[0];
                return;
            }
            Dictionary<string, string> PostData = new Dictionary<string, string>();
            PostData.Add("token", RemoteLibraryToken);
            PostData.Add("action", "getMMUList");

            string html = "";
            var PostForm = new FormUrlEncodedContent(PostData);
            try
            {
                var content = await client.PostAsync(RemoteLibraryUrl, PostForm);
                html = content.Content.ReadAsStringAsync().Result;
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    html = "Connection error to MMU Library: \r\n" + err.InnerException.Message;
                else
                    html = "Connection error to MMU Library: \r\n" + err.Message;
                System.Windows.MessageBox.Show(html, "Remote MMU Library", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                RemoteMMUs = Serialization.FromJsonString<RemoteMMUList[]>(html);
            }
            catch
            {
                System.Windows.MessageBox.Show("Serialization error of \r\n" + html, "Remote MMU Library", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RemoveRemoteLibrary(int index)
        {
            File.Delete(Remotes[index].LocalFileName);
            Remotes.RemoveAt(index); //add refreshing in the user interface after this operation in the MMULibraryWindow.xaml.cs
        }

        private string GetTagValue(string data, string tag)
        {
            var L = tag.Length + 2;
            var a = data.IndexOf("<"+tag+">");
            var b = data.IndexOf("</"+tag+">");
            if ((a>-1) && (b>-1))
            return data.Substring(a + L, b - a - L);
            return "";
        }

        private async Task GetServerSettings()
        {
            Dictionary<string, string> PostData = new Dictionary<string, string>();
            PostData.Add("token", RemoteLibraryToken);
            PostData.Add("action", "getSettings");

            string html = "";
            var PostForm = new FormUrlEncodedContent(PostData);
            try
            {
                var content = await client.PostAsync(RemoteLibraryUrl, PostForm);
                html += content.Content.ReadAsStringAsync().Result;
                var sChunkSize = GetTagValue(html, "chunkSize");
                 if (sChunkSize!="")
                 chunkSize = long.Parse(sChunkSize);
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    html = "Connection error to MMU Library: \r\n" + err.InnerException.Message;
                else
                    html = "Connection error to MMU Library: \r\n" + err.Message;
                System.Windows.MessageBox.Show(html, "MMU Library settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string CreateBoundaryString()
        {
            const string charArray = "MNBVCXZASDFGHJKLPOIUYTREWQ1234567890qwertyuiopasdfghjklmnbvcxz";
            string hash="";
            //Create a byte array from source data.
            var rand = new Random();
            for (int i = 0; i < 30; i++)
                hash += charArray[rand.Next(0, charArray.Length)];
            return hash;
        }

        public async Task SendToServer(Progress UploadProgress = null)
        {
            if (isUploading)
                return;

            long uploadSize = 0;
            long uploadedSize = 0;
            for (int k = 0; k < UploadMMU.Count; k++)
            {
                string inputFile = localZipMMULibrary + "mmu" + k.ToString() + ".zip";
                if (UploadMMU[k].mmu == -1)
                    inputFile = localZipMMULibrary + UploadMMU[k].zipFile;
                var fs = new FileStream(inputFile, FileMode.Open);
                uploadSize += fs.Length;
                fs.Close();
            }

            //string summary = "";

            UploadProgress?.Invoke(uploadSize, uploadedSize);

            isUploading = true;
            for (int k=0; k<UploadMMU.Count; k++)
            {
            await Remotes[UploadMMU[k].library].GetSettingsIfNecessary();
                chunkSize = Remotes[UploadMMU[k].library].chunkSize;
            string msg="";
            bool success = true;
            string html = "";
            int chunkEnd = 0;
            int chunkNum = 0;
            string fileID = "0";
            string sessionID = CreateBoundaryString();
            string inputFile = localZipMMULibrary + "\\mmu" + k.ToString() + ".zip";
                if (UploadMMU[k].mmu == -1)
                    inputFile = localZipMMULibrary + UploadMMU[k].zipFile;

            byte[] buffer = new byte[chunkSize];
            var fs = new FileStream(inputFile, FileMode.Open);
            
            Dictionary<string, string> PostData = new Dictionary<string, string>();
            PostData.Add("token", RemoteLibraryToken);
            PostData.Add("action", "uploadMMU");
            PostData.Add("sessionID", sessionID);
            PostData.Add("TotalSize", fs.Length.ToString());
            PostData.Add("chunkend", fs.Position.ToString());
            PostData.Add("fileID", fileID);
            PostData.Add("chunknum", chunkNum.ToString());

            do {
                fs.Position = chunkEnd;
                int thisChunkSize = fs.Read(buffer, 0, Convert.ToInt32(Math.Min(fs.Length-chunkEnd,chunkSize)));
                uploadedSize+= thisChunkSize;
                PostData["chunkend"]= (chunkEnd+thisChunkSize).ToString();
                PostData["fileID"]= fileID;
                PostData["chunknum"]=chunkNum.ToString();
                var FormData = new MultipartFormDataContent(CreateBoundaryString());
                 for (int i=0; i<PostData.Keys.Count; i++)
                 FormData.Add(new StringContent(PostData.Values.ElementAt<string>(i)), String.Format("\"{0}\"", PostData.Keys.ElementAt<string>(i)));
                FormData.Add(new StreamContent(new MemoryStream(buffer, 0, thisChunkSize)), "chunk", Path.GetFileName(inputFile));
                try
                {
                    var content = await client.PostAsync(RemoteLibraryUrl, FormData);
                    html = content.Content.ReadAsStringAsync().Result;
                    string val = GetTagValue(html, "chunkresult");
                     if (val=="ERR")
                     {
                        success = false;
                        msg = GetTagValue(html, "chunkmsg");
                     }
                     else
                     {
                        val = GetTagValue(html, "result");
                        if (val != "")
                        {
                            if (val == "ERR")
                            {
                                success = false;
                                msg = MMUs[k].Name + " " + MMUs[k].Version + "\r\n" + GetTagValue(html, "msg");
                            }
                            else
                                msg = GetTagValue(html, "mmuName") + " " + GetTagValue(html, "mmuVersion");
                        }
                     }

                    val = GetTagValue(html, "nextChunk");
                    if (val == "")
                        success = false;
                    else
                        if (Int32.Parse(val) != chunkNum + 1)
                        {
                            chunkNum = Int32.Parse(val);
                            chunkEnd = Convert.ToInt32(chunkNum * chunkSize);
                        }
                    val = GetTagValue(html, "nextStart");
                    if (val == "")
                        success = false;
                    else
                        chunkEnd = Int32.Parse(val);
                    val = GetTagValue(html, "fileID");
                    if (val == "")
                        success = false;
                    else
                        fileID = val;
                 UploadProgress?.Invoke(uploadSize, uploadedSize);
                }
                catch (Exception err)
                {
                    if (err.InnerException != null)
                        html = "Connection error: \r\n" + err.InnerException.Message;
                    else
                        html = "Connection error: \r\n" + err.Message;
                    System.Windows.MessageBox.Show(html, "MMU upload", MessageBoxButton.OK, MessageBoxImage.Information);
                    success = false;
                }

                chunkNum++;
                FormData.Dispose();
            } while ((chunkEnd<fs.Length) && success);
            fs.Close();
            if (success)
            System.Windows.MessageBox.Show("File upload complete.\r\n"+msg, "MMU upload", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            System.Windows.MessageBox.Show("File upload has finished with errors.\r\n"+msg, "MMU upload", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            isUploading = false;
        }

        public async Task GetFromServer(Progress DownloadProgress = null)
        {
            if (isUploading)
                return;

            if (chunkSize == 0)
                await GetServerSettings();
            if (chunkSize == 0)
                chunkSize = 1024 * 128; //default chunk size if server settings cannot be obtained - generally they should always be available.

            DownloadProgress?.Invoke(DownloadMMU.Count, 0);

            isDownloading = true;
            for (int k = 0; k < DownloadMMU.Count; k++)
            {
                int itemIndex = DownloadMMU[k].mmu;
                string msg = "";
                bool success = true;
                string html = "";
                string inputFile = localZipMMULibrary + "\\mmu-d" + itemIndex.ToString() + ".zip";

                Dictionary<string, string> PostData = new Dictionary<string, string>();
                PostData.Add("token", RemoteLibraryToken);
                PostData.Add("action", "downloadMMU");
                 if (DownloadMMU[k].mmu>=0)
                 PostData.Add("mmuID", Remotes[DownloadMMU[k].library].MMUs[DownloadMMU[k].mmu].url);
                 else
                    PostData.Add("mmuID", DownloadMMU[k].zipFile);

                var FormData = new MultipartFormDataContent(CreateBoundaryString());
                    for (int i = 0; i < PostData.Keys.Count; i++)
                        FormData.Add(new StringContent(PostData.Values.ElementAt<string>(i)), String.Format("\"{0}\"", PostData.Keys.ElementAt<string>(i)));
                try
                {
                    var content = await client.PostAsync(RemoteLibraryUrl, FormData);
                    if (content.Content.Headers.Contains("Content-Type"))
                    {
                        if (content.Content.Headers.ContentType.MediaType == "application/zip")
                        {
                            FileStream OutputFile = File.Create(inputFile);
                            Stream data = content.Content.ReadAsStreamAsync().Result;
                            data.CopyTo(OutputFile);
                            OutputFile.Close();
                            data.Dispose();
                            OutputFile.Dispose();
                            Extract(inputFile, LocalMMULibrary);
                        }
                        else
                        {
                            success = false;
                            msg = GetTagValue(html, "result");
                        }
                    }
                    else
                        success = false;
                    DownloadProgress?.Invoke(DownloadMMU.Count, k+1);
                }
                catch (Exception err)
                {
                   if (err.InnerException != null)
                        html = "Connection error: \r\n" + err.InnerException.Message;
                   else
                       html = "Connection error: \r\n" + err.Message;
                   System.Windows.MessageBox.Show(html, "MMU upload", MessageBoxButton.OK, MessageBoxImage.Information);
                 success = false;
                }

                FormData.Dispose();

                if (success)
                    System.Windows.MessageBox.Show("File download complete.\r\n" + msg, "MMU download", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    System.Windows.MessageBox.Show("File download has finished with errors.\r\n" + msg, "MMU download", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            isDownloading = false;
        }

        public void Pack(int itemIndex)
        {
            string outputFile = localZipMMULibrary + "\\mmu" + itemIndex.ToString() + ".zip";
            if (File.Exists(outputFile))
                File.Delete(outputFile);
            ZipFile.CreateFromDirectory(MMUs[itemIndex].FolderPath, outputFile ,CompressionLevel.Optimal,true);
        }

        public string NameMUUDir(ZipArchive zip)
        {
            string dir;
            var desc = zip.GetEntry("description.json");
            var descFile = desc.Open();
            byte[] descBuff = new byte[descFile.Length];
            descFile.Read(descBuff, 0, Convert.ToInt32(descFile.Length));
            var mmudescription = Serialization.FromJsonString<MMUDescription>(Encoding.UTF8.GetString(descBuff));
            mmudescription.Name=mmudescription.Name.Trim();
            mmudescription.Version = mmudescription.Version.Trim();
            if (mmudescription.MotionType == "")
                mmudescription.Name = "invalidMotion";
            else
            if (mmudescription.Name == "")
            {
                var p = mmudescription.MotionType.LastIndexOf('\\');
                if (p==-1)
                    p = mmudescription.MotionType.LastIndexOf('/');
                if (p >= 0)
                    mmudescription.Name = mmudescription.MotionType.Substring(p + 1);
                else
                    mmudescription.Name = mmudescription.MotionType;
            }
            if (mmudescription.Version == "")
                mmudescription.Version = "0.0";
            dir = mmudescription.Name + "-" + mmudescription.Version;
            descFile.Dispose();
            return dir;
        }

        public int ImportMMUs(string[] mmuZipFiles,int mmuLibrary=-1, Progress ProgressUpdate = null)
        {
            int success = 0;
            if (mmuLibrary == -1) //local library import
            {
                for (int i = 0; i < mmuZipFiles.Length; i++)
                    if (Extract(mmuZipFiles[i], LocalMMULibrary))
                        success++;
            }
            else //remote library direct mmu add
                success = ImportMMUsRemote(mmuZipFiles, mmuLibrary, ProgressUpdate);
            return success;
        }

        public int ImportMMUsRemote(string[] mmuZipFiles, int mmuLibrary, Progress ProgressUpdate = null)
        {
            int success = 0;
            UploadMMU.Clear();
                for (int i = 0; i < mmuZipFiles.Length; i++)
                {
                 var fname = "mmu-up-" + i.ToString() + ".zip";
                if (File.Exists(localZipMMULibrary + "\\" + fname))
                    File.Delete(localZipMMULibrary + "\\" + fname);
                 File.Copy(mmuZipFiles[i], localZipMMULibrary + "\\"+fname, true);
                 AddToUpload(fname, mmuLibrary);
                 success++;
                }
            SendToServer(ProgressUpdate);
            return success;
        }

        public bool Extract(string mmuZipFile, string outputDir)
        {    
            System.IO.Compression.ZipArchive zip = ZipFile.Open(mmuZipFile,ZipArchiveMode.Update);
            string dir = "";
            bool noFolder = false;
            for (int i = 0; i < zip.Entries.Count; i++)
            {
                var p = zip.Entries[i].FullName.IndexOf('\\');
                 if (p==-1)
                    p = zip.Entries[i].FullName.IndexOf('/');
                if (p >= 0)
                {
                    if ((i > 0) && (dir != zip.Entries[i].FullName.Substring(0, p)))
                        noFolder = true;
                    dir = zip.Entries[i].FullName.Substring(0, p);
                }
                else
                    noFolder = true;
            }

            if (noFolder)
            { //there is no folder in the zip archive containing all the files, folder needs to be created, extract MMU name from file and make sure the name is unique in the folder structure
                dir = NameMUUDir(zip);
                if (Directory.Exists(outputDir + dir))
                {
                    int k = 1;
                    while (Directory.Exists(outputDir + dir + "-" + k.ToString()))
                        k++;
                    dir = dir + "-" + k.ToString() + "\\";
                }
                dir = dir.Replace("mmu", "").Replace("MMU", "");
                zip.ExtractToDirectory(outputDir + dir);
                ScanMMU(outputDir + dir);
            }
            else
            { //there is common folder - check if it is unque in the folder sturcture, if not change the folder name using MMU name and version
                if (Directory.Exists(outputDir+dir))
                {
                    Stream descFile = null;
                    for (int i = 0; i < zip.Entries.Count; i++)
                        if (zip.Entries[i].Name == "description.json")
                        {
                            descFile = zip.Entries[i].Open();
                            break;
                        }
                    if (descFile == null)
                        return false;

                    byte[] descBuff = new byte[descFile.Length];
                    descFile.Read(descBuff, 0, Convert.ToInt32(descFile.Length));
                    var mmudescription = Serialization.FromJsonString<MMUDescription>(Encoding.UTF8.GetString(descBuff));
                    dir = mmudescription.Name + "-" + mmudescription.Version;
                    dir = dir.Replace("mmu", "").Replace("MMU", "");
                    if (Directory.Exists(outputDir + dir))
                    {
                        int k = 1;
                        while (Directory.Exists(outputDir + dir + "-" + k.ToString()))
                            k++;
                        dir = dir + "-" + k.ToString() + "\\";
                    }
                    else
                        dir += "\\";
                    descFile.Dispose();
                    
                    Directory.CreateDirectory(outputDir + dir);
                    
                    //extraction file by file
                    for (int i = 0; i < zip.Entries.Count; i++)
                    {
                        var p = zip.Entries[i].FullName.IndexOf('\\');
                        if (p==-1)
                            p = zip.Entries[i].FullName.IndexOf('/');
                        var basefile = zip.Entries[i].FullName.Substring(p + 1);
                        p = basefile.LastIndexOf('\\');
                        if (p == -1)
                            p = basefile.LastIndexOf('/');
                        if (p > -1)
                        {
                            if (!Directory.Exists(outputDir + dir + basefile.Substring(0,p)))
                                Directory.CreateDirectory(outputDir + dir + basefile.Substring(0, p));
                        }
                        if (basefile!="") //it is empty in case of root directory entry
                        zip.Entries[i].ExtractToFile(outputDir + dir + basefile);
                    }
                    ScanMMU(outputDir + dir);
                }
                else
                {
                   // dir = dir.Replace("mmu", "").Replace("MMU", "");
                    zip.ExtractToDirectory(outputDir);
                    ScanMMU(outputDir + dir);
                }
            }

            zip.Dispose();
            return true;
            //ZipFile.ExtractToDirectory(mmuZipFile, outputDir);
        }
    }
}
