// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common.Communication;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MMICSharp.Adapter.MMUProvider
{
    /// <summary>
    /// The class represent an adapter MMU provider which scans the filesystem for supported MMUs.
    /// The MMU instantiator can then be utilized with the gathered filepaths.
    /// </summary>
    public class FileBasedMMUProvider : IMMUProvider
    {
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
        public FileBasedMMUProvider(SessionData sessionData, List<string> paths, List<string> supportedLanguages)
        {
            this.sessionData = sessionData;
            this.mmuPaths = paths;
            this.supportedLanguages = supportedLanguages;

            //Add a filewatcher for each path
            foreach (string mmuPath in this.mmuPaths)
            {
                try
                {
                    //Create a filewatcher for the mmu path
                    FileSystemWatcher fileWatcher = new FileSystemWatcher(mmuPath);
                    fileWatcher.Changed += FileWatcher_Changed;
                    fileWatcher.Created += FileWatcher_Created;
                    fileWatcher.Deleted += FileWatcher_Deleted;
                    fileWatcher.EnableRaisingEvents = true;

                    //Add to the filewatchers
                    this.fileWatchers.Add(fileWatcher);
                }
                catch (Exception)
                {
                    Logger.Log(Log_level.L_ERROR, "Problem setting up file watchers");
                }
            }
        }


        /// <summary>
        /// Basic constructor with params 
        /// </summary>
        /// <param name="sessionData"></param>
        /// <param name="path"></param>
        /// <param name="supportedLanguages"></param>
        public FileBasedMMUProvider(SessionData sessionData, string path, params string[] supportedLanguages):this(sessionData, new List<string>() { path}, supportedLanguages.ToList())
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
                foreach (string directoryPath in Directory.GetDirectories(mmuPath))
                {
                    this.TryAddMMU(directoryPath);
                }
            }
        }


        /// <summary>
        /// Tries to add/read the respective MMU
        /// </summary>
        /// <param name="zipFilePath">The filepath of the zip archive</param>
        /// <returns></returns>
        private bool TryAddMMU(string directoryPath)
        {
            try
            {
                //Check whether a description is available
                MMUDescription mmuDescription = this.GetMMUDescription(directoryPath);

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


                    //Determine the mmu file
                    string mmuFile = Directory.GetFiles(directoryPath).ToList().Find(s => s.Contains(mmuDescription.AssemblyName));


                    //Create the new loading properties for the MMU
                    MMULoadingProperty loadingProperties = new MMULoadingProperty()
                    {
                        //Assign the description
                        Description = mmuDescription,

                        //Store all the data
                        Data = GetFolderContent(directoryPath),

                        //Set the path of the directory
                        Path = mmuFile //directory path -> in future use directory path in here
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
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Returns the MMU description contained in the zip archive (if available)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private MMUDescription GetMMUDescription(string folderPath)
        {
            MMUDescription mmuDescription = null;

            try
            {
                string descriptionFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains("description.json"));
                mmuDescription = Serialization.FromJsonString<MMUDescription>(File.ReadAllText(descriptionFile));
            }
            catch (Exception)
            {
                return null;
            }

            return mmuDescription;
        }


        /// <summary>
        /// Loads all the available files within the folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetFolderContent(string folderPath)
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();

            try
            {

                Logger.Log(Log_level.L_DEBUG, $"Scanning folder path: {folderPath}");

                foreach (string file in Directory.GetFiles(folderPath))
                {
                    string fileName = Path.GetFileName(file);

                    Logger.Log(Log_level.L_DEBUG, $"{fileName}");


                    if (!Data.ContainsKey(fileName))
                        Data.Add(fileName, File.ReadAllBytes(file));
                    else
                        Logger.Log(Log_level.L_DEBUG, $"{fileName} already available.");
                }
            }
            catch (Exception e)
            {
                Logger.Log(Log_level.L_ERROR, $"Problem obtaining content of folder: {e.Message} {e.InnerException}");
                return null;
            }

            return Data;

        }



        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            this.MMUsChanged?.Invoke(this, new EventArgs());
        }

        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            this.MMUsChanged?.Invoke(this, new EventArgs());
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.MMUsChanged?.Invoke(this, new EventArgs());
        }


        #region legacy

        ///// <summary>
        ///// Returns the available MMUs
        ///// </summary>
        ///// <returns></returns>
        //public Dictionary<string, MMULoadingProperty> GetAvailableMMUs()
        //{
        //    Dictionary<string, MMULoadingProperty> availableMMUs = new Dictionary<string, MMULoadingProperty>();

        //    int loaded = 0;

        //    foreach (string mmuPath in this.mmuPaths)
        //    {
        //        foreach (string folderPath in Directory.GetDirectories(mmuPath))
        //        {
        //            MMULoadingProperty mmuLoadingProperty = this.ScanFolder(folderPath);

        //            if (mmuLoadingProperty != null)
        //            {
        //                availableMMUs.Add(mmuLoadingProperty.Description.ID, mmuLoadingProperty);
        //                loaded++;
        //            }
        //        }
        //    }

        //    Logger.Log(Log_level.L_INFO, $"Scanned for loadable MMUs: {loaded} loadable MMUs found.");

        //    return availableMMUs;
        //}


        /// <summary>
        /// Checks the respective folder and tracks the properties/ mmu descriptions
        /// Returns true if a new MMU has been added 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private MMULoadingProperty ScanFolder(string folderPath)
        {
            //Check whether a description is available
            MMUDescription mmuDescription = this.GetMMUDescription(folderPath);

            //Skip if no description file
            if (mmuDescription == null)
                return null;

            //Check if the MMU supports the specified language
            if (!this.supportedLanguages.Contains(mmuDescription.Language))
                return null;

            //Check if already available -> skip
            if (this.sessionData.MMULoadingProperties.ContainsKey(mmuDescription.ID))
                return null;


            //Determine the mmu file
            string mmuFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains(mmuDescription.AssemblyName));

            //Load the mmu from filesystem and instantiate
            if (mmuFile != null)
            {
                //Add the description to the dictionary
                return new MMULoadingProperty()
                {
                    //Assign the description
                    Description = mmuDescription,

                    //Store all the data
                    Data = GetFolderContent(folderPath),

                    //Set the path of the MMU file
                    Path = mmuFile
                };
            }

            else
            {
                Logger.Log(Log_level.L_ERROR, $"Cannot find corresponding assembly name. {mmuDescription.AssemblyName} of MMU: {mmuDescription.Name}");
            }

            return null;
        }

        #endregion

    }
}
