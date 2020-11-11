// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common.Communication;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;


namespace MMICSharp.Adapter.MMUProvider
{
    /// <summary>
    /// Class which scans the filesystem for MMUs realized as zip archives.
    /// The class provides the detected MMUs as output for further instantiation and utilization.
    /// The class utilizes a file watcher an can dynamically add/remove MMUs.
    /// </summary>
    public class ZipBasedMMUProvider : IMMUProvider
    {
        /// <summary>
        /// Event handler which is called whenever mmus have been changed
        /// </summary>
        public event EventHandler<EventArgs> MMUsChanged;


        #region private fields

        /// <summary>
        /// The path of the mmus
        /// </summary>
        private readonly List<string> mmuPaths;

        /// <summary>
        /// The supported languages
        /// </summary>
        private readonly List<string> supportedLanguages;

        /// <summary>
        /// A filewatcher for the tracked directories
        /// </summary>
        private readonly List<FileSystemWatcher> fileWatchers = new List<FileSystemWatcher>();


        /// <summary>
        /// The utilized session data
        /// </summary>
        private readonly SessionData sessionData;


        /// <summary>
        /// A list of all the found MMUs
        /// </summary>
        private Dictionary<string, MMULoadingProperty> availableMMUs = new Dictionary<string, MMULoadingProperty>();


        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="sessionData"></param>
        /// <param name="paths"></param>
        /// <param name="supportedLanguages"></param>
        public ZipBasedMMUProvider(SessionData sessionData, List<string> paths, List<string> supportedLanguages)
        {
            this.sessionData = sessionData;
            this.mmuPaths = paths;
            this.supportedLanguages = supportedLanguages;

            //Add a filewatcher for each path
            foreach (string mmuPath in this.mmuPaths)
            {
                //Create a filewatcher for the mmu path
                FileSystemWatcher fileWatcher = new FileSystemWatcher(mmuPath);
                fileWatcher.Created += FileWatcher_Created;
                fileWatcher.Deleted += FileWatcher_Deleted;
                fileWatcher.EnableRaisingEvents = true;

                //Add to the filewatchers
                this.fileWatchers.Add(fileWatcher);
            }
        }


        /// <summary>
        ///Additional constructor (just calling the base constructor)
        /// </summary>
        /// <param name="sessionData"></param>
        /// <param name="path"></param>
        /// <param name="supportedLanguages"></param>
        public ZipBasedMMUProvider(SessionData sessionData, string path, params string[] supportedLanguages) : this(sessionData, new List<string>() { path }, supportedLanguages.ToList())
        {

        }


        /// <summary>
        /// Returns the available MMUs
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, MMULoadingProperty> GetAvailableMMUs()
        {
            //Try to add all MMUs
            this.UpdateAvailableMMUs();

            //Return the list of available MMUs
            return this.availableMMUs;
        }


        /// <summary>
        /// Updates the list of available MMUs
        /// </summary>
        private void UpdateAvailableMMUs()
        {
            Logger.Log(Log_level.L_DEBUG, "Update available MMUs");

            foreach (string mmuPath in this.mmuPaths)
            {
                //Get all zip files
                foreach (string zipFile in Directory.GetFiles(mmuPath, "*.zip"))
                {
                    this.TryAddMMU(zipFile);
                }
            }
        }


        /// <summary>
        /// Tries to add/read the respective MMU
        /// </summary>
        /// <param name="zipFilePath">The filepath of the zip archive</param>
        /// <returns></returns>
        private bool TryAddMMU(string zipFilePath)
        {
            //Check whether a description is available
            MMUDescription mmuDescription = this.GetMMUDescription(zipFilePath);

            //Only continue if description is not null
            if (mmuDescription != null)
            {
                //Check if the MMU supports the specified language
                if (!this.supportedLanguages.Contains(mmuDescription.Language))
                    return false;

                //Check if already available -> skip
                if (this.sessionData.MMULoadingProperties.ContainsKey(mmuDescription.ID))
                    return false;

                //Skip if already registered
                if (this.availableMMUs.ContainsKey(mmuDescription.ID))
                    return false;

                //Create the new loading properties for the MMU
                MMULoadingProperty loadingProperties = new MMULoadingProperty()
                {
                    //Assign the description
                    Description = mmuDescription,

                    //Store all the data
                    Data = GetZipContent(zipFilePath),

                    //Store the path to the zip file
                    Path = zipFilePath
                };

                //Add to available MMUs
                this.availableMMUs.Add(mmuDescription.ID, loadingProperties);


                Logger.Log(Log_level.L_INFO, $"Successfully added MMU: {mmuDescription.Name} {mmuDescription.AssemblyName}");

                //Success return true
                return true;
            }

            //Problem -> return false
            return false;
        }



        #region file watcher callbacks

        /// <summary>
        /// Callbak if a file has been deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            //Check if the removed file is an available MMU
            if (this.availableMMUs.ToList().Exists(s => Path.GetFileName(s.Value.Path) == e.Name))
            {
                //In this case -> remove
                KeyValuePair<string, MMULoadingProperty> match = this.availableMMUs.ToList().Find(s => Path.GetFileName(s.Value.Path) == e.Name);

                Logger.Log(Log_level.L_INFO, $"Removed MMU: {match.Value.Description.Name} {match.Value.Description.AssemblyName}");

                this.availableMMUs.Remove(match.Key);
            }

            //Trigger an update
            this.MMUsChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Callback if a file has been created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            //First check if the file is a zip archive
            if (Path.GetExtension(e.FullPath) == ".zip")
            {
                Logger.Log(Log_level.L_DEBUG, $"New zip file added: {e.FullPath}");

                //Try to add it (if its a compatible MMU)
                this.TryAddMMU(e.FullPath);
            }
            this.MMUsChanged?.Invoke(this, new EventArgs());

        }

        #endregion


        #region zip related methods

        /// <summary>
        /// Returns the MMU description contained in the zip archive (if available)
        /// </summary>
        /// <param name="zipArchivePath"></param>
        /// <returns></returns>
        private MMUDescription GetMMUDescription(string zipArchivePath)
        {
            MMUDescription mmuDescription = null;

            try
            {
                using (var file = File.OpenRead(zipArchivePath))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    //First get the description
                    ZipArchiveEntry descriptionEntry = zip.Entries.ToList().Find(s => s.Name == "description.json");

                    string jsonString = System.Text.Encoding.UTF8.GetString(GetByteData(descriptionEntry));

                    mmuDescription = Serialization.FromJsonString<MMUDescription>(jsonString);

                }
            }
            catch (Exception)
            {
                return null;
            }

            return mmuDescription;
        }


        /// <summary>
        /// Returns the zip file as byte array
        /// </summary>
        /// <param name="zipEntry"></param>
        /// <returns></returns>
        private byte[] GetByteData(ZipArchiveEntry zipEntry)
        {
            byte[] byteData = null;

            using (var stream = zipEntry.Open())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    byteData = memoryStream.ToArray();
                }
            }

            return byteData;
        }


        /// <summary>
        /// Returns the content of the zip archive as dictionary
        /// </summary>
        /// <param name="zipArchivePath"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetZipContent(string zipArchivePath)
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();

            Logger.Log(Log_level.L_DEBUG, $"Scanning zip archive: {zipArchivePath}");

            try
            {
                using (var file = File.OpenRead(zipArchivePath))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        Logger.Log(Log_level.L_DEBUG, $"{entry.Name}");

                        Data.Add(entry.Name, GetByteData(entry));
                    }
                }
                Logger.Log(Log_level.L_DEBUG, $"__________________________________________________________________");
            }
            catch (Exception e)
            {
                Logger.Log(Log_level.L_ERROR, $"Problem obtaining content of zip archive: {e.Message} {e.InnerException}");
                return null;
            }

            return Data;
        }

        #endregion



    }
}
