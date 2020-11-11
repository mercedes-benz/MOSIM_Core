// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Linq;
using MMIStandard;
using UnityEngine;
using System.Reflection;
using MMICSharp.Adapter;
using MMIUnity;
using MMICSharp.Common;

namespace MMIAdapterUnity
{
    /// <summary>
    /// Implementation of an IMMUInstntiator for the loading and instantiating Unity based MMUs
    /// </summary>
    public class UnityMMUInstantiator : IMMUInstantiation
    {
        /// <summary>
        /// Instantiates a MMU  from file
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="mmuDescription"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public IMotionModelUnitDev InstantiateMMU(MMULoadingProperty mmuLoadingProperty)
        {

            Debug.Log("Calling Unity MMU instantiator " + mmuLoadingProperty.Path);

            MMUDescription mmuDescription = mmuLoadingProperty.Description;
            string filePath = mmuLoadingProperty.Path;

            //Check if the MMU supports the specified language
            if (mmuDescription.Language != "UnityC#" && mmuDescription.Language != "Unity")
                throw new NotSupportedException("MMU is not supported");

            //Skip if file path invalid
            if (filePath == null)
                throw new NullReferenceException("File path of MMU is null " + mmuDescription.Name);

            //Skip if the MMU has no dependencies
            if (mmuDescription.Dependencies.Count == 0)
                throw new Exception("No dependencies of MMU specified " + mmuDescription.Name);

            //Create a variable which hols the mmu
            IMotionModelUnitDev mmu = null;

            //Get the paths
            string folderPath = System.IO.Directory.GetParent(filePath).ToString();
            string assetBundlePath = folderPath + "\\" + mmuDescription.Dependencies[0].Name;


            //Perform on unity main thread
            MainThreadDispatcher.Instance.ExecuteBlocking(delegate
            {
                //To do in futre -> Create a new scene for each MMU
                //Scene scene = SceneManager.CreateScene("Scene" + mmuDescription.Name);
                //SceneManager.SetActiveScene(scene);

                //First load the resources
                AssetBundle mmuAssetBundle = null;
                try
                {
                    if (!mmuLoadingProperty.Data.ContainsKey(assetBundlePath))
                    {
                        mmuAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
                        if (mmuAssetBundle == null)
                        {

                            MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, "Failed to load AssetBundle! AssetBundle is null!" + mmuDescription.Name);
                            return;
                        }

                        mmuLoadingProperty.Data.Add(assetBundlePath, mmuAssetBundle);
                    }
                    else
                    {
                        mmuAssetBundle = mmuLoadingProperty.Data[assetBundlePath] as AssetBundle;
                    }
                }
                catch (Exception e)
                {
                    MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, "Cannot load asset bundle: " + mmuDescription.Name + " " + e.Message);
                    return;
                }

                GameObject prefab = null;
                try
                {
                    prefab = mmuAssetBundle.LoadAsset<GameObject>(mmuDescription.Name);
                }
                catch (Exception e)
                {
                    MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, "Cannot load prefab: " + mmuDescription.Name + " " + e.Message);
                }

                GameObject mmuObj = null;

                try
                {
                    mmuObj = GameObject.Instantiate(prefab);
                }
                catch (Exception e)
                {
                    MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, "Cannot instantiate prefab: " + mmuDescription.Name + " " + e.Message);
                }

                try
                {
                    Assembly assembly = Assembly.LoadFile(filePath);

                    //Find the class type which implements the IMotionModelInterface
                    Type classType = assembly.GetTypes().ToList().Find(s => s.GetInterfaces().Contains(typeof(IMotionModelUnitDev)));
                    if (classType != null)
                    {
                        //Add the script to the game object
                        mmu = mmuObj.AddComponent(classType) as IMotionModelUnitDev;

                        ///Loading and assigning the configuration file
                        if (mmuObj.GetComponent<UnityMMUBase>() != null)
                        {
                            MMICSharp.Adapter.Logger.Log(Log_level.L_DEBUG, $"MMU {mmuDescription.Name} implements the UnityMMUBase");

                            //Get the UnityMMUBse interface
                            UnityMMUBase mmuBase = mmuObj.GetComponent<UnityMMUBase>();

                            //By default set the configuration file path and define it globally
                            string avatarConfigFilePath = AppDomain.CurrentDomain.BaseDirectory + "/" + "avatar.mos";

                            //Check if the MMU has a custom configuration file
                            if (this.GetLocalAvatarConfigurationFile(folderPath, out string localConfigFilePath))
                            {
                                //Setting the local avatar as reference for retargeting
                                MMICSharp.Adapter.Logger.Log(Log_level.L_DEBUG, $"Local avatar configuration was found: {mmuDescription.Name} ({avatarConfigFilePath}).");

                                //Set to the utilized path
                                avatarConfigFilePath = localConfigFilePath;
                            }

                           
                            //Further perform a check whether the file is available at all
                            if (System.IO.File.Exists(avatarConfigFilePath))
                            {
                                mmuBase.ConfigurationFilePath = avatarConfigFilePath;
                                MMICSharp.Adapter.Logger.Log(Log_level.L_DEBUG, $"Setting the configuration file path for loading the avatar of {mmuDescription.Name} to {avatarConfigFilePath}");
                            }

                            //Problem in here -> File is obviously not available
                            else
                            {
                                MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, $"Problem setting the configuration file path for loading the avatar of {mmuDescription.Name} to {avatarConfigFilePath}. File not available.");
                            }
                        }

                    }
                    else
                    {
                        MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, "Cannot instantiate MMU: " + mmuDescription.Name);
                    }
                }
                catch (Exception e)
                {
                    MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, "Problem at instantiating class of MMU: " + mmuDescription.Name + " " + e.Message + " " + e.StackTrace);
                }
                

                //Disable all renderers in the scene
                foreach (Renderer renderer in GameObject.FindObjectsOfType<Renderer>())
                    renderer.enabled = false;

            });

         


            return mmu;
        }


        /// <summary>
        /// Returns the local avatar configuration file (if defined)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool GetLocalAvatarConfigurationFile(string folderPath, out String filePath)
        {
            filePath = null;

            //To do -> Check if local avatar.mos is available
            foreach (string file in System.IO.Directory.GetFiles(folderPath))
            {
                if (file.Contains("avatar.mos"))
                {
                    filePath = file;
                    return true;
                }
            }

            return false;
        }
    }

}
