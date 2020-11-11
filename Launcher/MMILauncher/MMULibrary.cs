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
        public string url;
        public string vendorID;
        public string name;
        public string version;
        public uint sortorder;
        public bool enabled;
    }

    class MMULibrary
    {
        private long chunkSize = 0;
        public List<MMUOrderAndDescriptionData> MMUs = new List<MMUOrderAndDescriptionData>();
        private HttpClient client;
        string RemoteLibraryUrl;
        string RemoteLibraryToken;
        string LocalMMULibrary;
        private List<int> UploadMMU = new List<int>();
        private List<int> DownloadMMU = new List<int>();
        private bool isUploading = false;
        private bool isDownloading = false;
        public RemoteMMUList[] RemoteMMUs;

        public int SyncUpCount()
        {
            return UploadMMU.Count;
        }

        public int SyncDownCount()
        {
            return DownloadMMU.Count;
        }

        public void ConfigureConnection(HttpClient httpClient, string MMULibraryPath, string MMULibraryUrl, string MMULibraryToken)
        {
            client = httpClient;
            RemoteLibraryUrl = MMULibraryUrl;
            RemoteLibraryToken = MMULibraryToken;
            LocalMMULibrary = MMULibraryPath;
        }

        public void AddToUpload(int mmuIndex)
        {
            UploadMMU.Add(mmuIndex);
        }

        public void ScanLibrary(string mmuPath)
        {
            string[] mmuFolders = Directory.GetDirectories(mmuPath);
            for (int i = 0; i < mmuFolders.Length; i++)
            {
                string descFile = mmuFolders[i] + (mmuFolders[i].EndsWith("/") ? "" : "/") + "description.json";
                if (File.Exists(descFile))
                {
                    var mmuDesc = Serialization.FromJsonString<MMUDescription>(File.ReadAllText(descFile));
                    var mmuExtDesc = new MMUOrderAndDescriptionData(mmuDesc, 0, mmuFolders[i]);
                    MMUs.Add(mmuExtDesc);
                }
            }
        }

        public void CompareRemoteAndLocal(string outputDir)
        {
            if (isUploading || isDownloading)
                return;

            UploadMMU.Clear();
            DownloadMMU.Clear();
            bool found = false;
            for (int i = 0; i < MMUs.Count; i++)
            {
                found = false;
                for (int j = 0; j < RemoteMMUs.Length; j++)
                    if (RemoteMMUs[j].vendorID == MMUs[i].ID)
                        found = true;
                if (!found)
                {
                    Pack(i, outputDir);
                    UploadMMU.Add(i);
                }
            }

            for (int i = 0; i < RemoteMMUs.Length; i++)
            {
                found = false;
                for (int j = 0; j < MMUs.Count; j++)
                    if (RemoteMMUs[i].vendorID == MMUs[j].ID)
                        found = true;
                if (!found)
                    DownloadMMU.Add(i);
            }
        }

        public async Task GetMMUListFromServer()
        {
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

        public async Task SendToServer(string inputDir)
        {
            if (isUploading)
                return;

            if (chunkSize == 0)
                await GetServerSettings();
            if (chunkSize == 0)
                chunkSize = 1024 * 128; //default chunk size if server settings cannot be obtained - generally they should always be available.

            isUploading = true;
            for (int k=0; k<UploadMMU.Count; k++)
            {
            int itemIndex = UploadMMU[k];
            string msg="";
            bool success = true;
            string html = "";
            int chunkEnd = 0;
            int chunkNum = 0;
            string fileID = "0";
            string sessionID = CreateBoundaryString();
            string inputFile = inputDir + "\\mmu" + itemIndex.ToString() + ".zip";

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
                PostData["chunkend"]= (chunkEnd+thisChunkSize).ToString();
                PostData["fileID"]= fileID;
                PostData["chunknum"]=chunkNum.ToString();
                var FormData = new MultipartFormDataContent(CreateBoundaryString());
                 for (int i=0; i<PostData.Keys.Count; i++)
                 FormData.Add(new StringContent(PostData.Values.ElementAt<string>(i)), String.Format("\"{0}\"", PostData.Keys.ElementAt<string>(i)));
                FormData.Add(new StreamContent(new MemoryStream(buffer, 0, thisChunkSize)), "chunk", "mmu" + itemIndex.ToString() + ".zip");
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
                                msg = MMUs[itemIndex].Name + " " + MMUs[itemIndex].Version + "\r\n" + GetTagValue(html, "msg");
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

        public async Task GetFromServer(string inputDir)
        {
            if (isUploading)
                return;

            if (chunkSize == 0)
                await GetServerSettings();
            if (chunkSize == 0)
                chunkSize = 1024 * 128; //default chunk size if server settings cannot be obtained - generally they should always be available.

            isDownloading = true;
            for (int k = 0; k < DownloadMMU.Count; k++)
            {
                int itemIndex = DownloadMMU[k];
                string msg = "";
                bool success = true;
                string html = "";
                string inputFile = inputDir + "\\mmu-d" + itemIndex.ToString() + ".zip";

                Dictionary<string, string> PostData = new Dictionary<string, string>();
                PostData.Add("token", RemoteLibraryToken);
                PostData.Add("action", "downloadMMU");
                PostData.Add("mmuID", RemoteMMUs[DownloadMMU[k]].url);

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

        public void Pack(int itemIndex, string outputDir)
        {
            string outputFile = outputDir + "\\mmu" + itemIndex.ToString() + ".zip";
            if (File.Exists(outputFile))
                File.Delete(outputFile);
            ZipFile.CreateFromDirectory(MMUs[itemIndex].FolderPath, outputFile ,CompressionLevel.Optimal,true);
            
            /*using (FileStream zipToOpen = new FileStream(outputDir+"\\mmu.zip", FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry("Readme.txt");
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.WriteLine("Information about this package.");
                        writer.WriteLine("========================");
                    }
                }
            }*/
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

        public int ImportMMUs(string[] mmuZipFiles)
        {
            int success = 0;
            for (int i = 0; i < mmuZipFiles.Length; i++)
                if (Extract(mmuZipFiles[i], LocalMMULibrary))
                    success++;
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
                }
                else
                {
                   // dir = dir.Replace("mmu", "").Replace("MMU", "");
                    zip.ExtractToDirectory(outputDir);
                }
            }
            

            zip.Dispose();
            return true;
            //ZipFile.ExtractToDirectory(mmuZipFile, outputDir);
        }
    }
}
