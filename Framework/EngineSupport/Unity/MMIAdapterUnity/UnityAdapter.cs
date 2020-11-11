// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Andreas Kaiser

using MMICSharp.Adapter;
using MMICSharp.Adapter.MMUProvider;
using MMIStandard;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MMIAdapterUnity
{
    /// <summary>
    /// Main class for the adapter
    /// </summary>
    public class UnityAdapter : MonoBehaviour
    {
        /// <summary>
        /// The singleton instance of the Unity Adapter
        /// </summary>
        public static UnityAdapter Instance;


        public string mmuPath;


        #region privat variables

        private bool isServerBuild = false;

        private SessionCleaner sessionCleaner;
        private SessionData sessionData;
        private readonly MIPAddress address = new MIPAddress("127.0.0.1", 8900);
        private static readonly MIPAddress mmiRegisterAddress = new MIPAddress("127.0.0.1", 8900);
        private AdapterController adapterController;

        #endregion

        /// <summary>
        /// Basic awake method
        /// </summary>
        private void Awake()
        {
            //Determine if server build
            isServerBuild = IsHeadlessMode();

            //Set the logger to a unity specific one (using debug log)
            MMICSharp.Adapter.Logger.Instance = new UnityLogger(isServerBuild)
            {
                //Log everything
                Level = Log_level.L_DEBUG
            };

            //Assign the instance
            Instance = this;


            //Only visualizes the text if server build
            if (isServerBuild)
            {
                Console.WriteLine(@"   __  __      _ __           ___       __            __           ");
                Console.WriteLine(@"  / / / /___  (_) /___  __   /   | ____/ /___ _____  / /____  _____");
                Console.WriteLine(@" / / / / __ \/ / __ / / / /  / /| |/ __ / __ `/ __ \/ __ / _ \/ ___ / ");
                Console.WriteLine(@"/ /_/ / / / / / /_/ /_/ /  / ___ / /_/ / /_/ / /_/ / /_/  __/ /    ");
                Console.WriteLine(@"\____/_/ /_/_/\__/\__, /  /_/  |_\__,_/\__,_/ .___/\__/\___/_/     ");
                Console.WriteLine(@"                 /____/                    /_/                     ");
                Console.WriteLine(@"_________________________________________________________________");
            }





            //Only use this if self_hosted and within edit mode -> Otherwise the launcher which starts the service assigns the address and port
#if UNITY_EDITOR
        this.address.Address = "127.0.0.1";
        this.address.Port = 8950;

        this.registerAddress.Port = 9009;
        this.registerAddress.Address = "127.0.0.1";
#else
            //Parse the command line arguments
            if (!this.ParseCommandLineArguments(System.Environment.GetCommandLineArgs()))
            {
                MMICSharp.Adapter.Logger.Log(Log_level.L_ERROR, "Cannot parse the command line arguments. The adapter is meant to be started with specified arguments only.");
                return;
            }
#endif


            //Create a new session data
            this.sessionData = new SessionData();

            //Create a session cleaner for the utilized session data
            this.sessionCleaner = new SessionCleaner(sessionData)
            {
                //Session shoould be cleaned after 60 minutes
                Timeout = TimeSpan.FromMinutes(60),

                //The session cleaner should check every minute
                UpdateTime = TimeSpan.FromMinutes(1)
            };


            //Set the quality properties
            Application.targetFrameRate = 30;
            QualitySettings.SetQualityLevel(0, true);

            //Create an adapter description -> to do in future load properties such as the ID from file
            MAdapterDescription adapterDescription = new MAdapterDescription()
            {
                ID = "14456546241413414",
                Addresses = new List<MIPAddress>() { this.address },
                Name = "UnityAdapter",
                Language = "UnityC#",
                Parameters = new List<MParameter>(),
                Properties = new Dictionary<string, string>()
            };

            //Create a new adapter controller which listens on the file system and scans for MMUs
            this.adapterController = new AdapterController(this.sessionData, adapterDescription, mmiRegisterAddress, new FileBasedMMUProvider(sessionData, mmuPath ,  "UnityC#", "Unity"), new UnityMMUInstantiator());


            //Log the startup info text
            MMICSharp.Adapter.Logger.Log(Log_level.L_INFO, $"Hosting thrift server at {this.address.Address} {this.address.Port}: ");
        }

        private void Start()
        {
            //Start the session cleaner
            this.sessionCleaner.Start();

            //Start the adapter controller asynchronously
            this.adapterController.StartAsync();

        }

        /// <summary>
        /// Tries to parse the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private bool ParseCommandLineArguments(string[] args)
        {
            //Parse the command line arguments
            OptionSet p = new OptionSet()
            {
                { "a|address=", "The address of the hostet tcp server.",
                  v =>
                  {
                      //Split the address to get the ip and port
                      string[] addr  = v.Split(':');

                      if(addr.Length == 2)
                      {
                          this.address.Address = addr[0];
                          this.address.Port = int.Parse(addr[1]);
                      }
                      Debug.Log("Address: " + v);
                  }
                },

                { "r|raddress=", "The address of the register which holds the central information.",
                  v =>
                  {
                      //Split the address to get the ip and port
                      string[] addr  = v.Split(':');

                      if(addr.Length == 2)
                      {
                          mmiRegisterAddress.Address = addr[0];
                          mmiRegisterAddress.Port = int.Parse(addr[1]);
                      }
                      Debug.Log("Register address: " + v);
                  }
                },

                { "m|mmupath=", "The path of the mmu folder.",
                  v =>
                  {
                      mmuPath = v;
                      Debug.Log("MMUpath: " + v);
                  }
                },
            };

            try
            {
                p.Parse(args);
                return true;
            }

            catch (Exception)
            {
                Debug.Log("Cannot parse arguments");
            }

            return false;
        }


        /// <summary>
        /// Function is executed if application is closed
        /// </summary>
        private void OnApplicationQuit()
        {
            try
            {
                if(this.sessionCleaner !=null)
                    this.sessionCleaner.Dispose();

                if (this.adapterController != null)
                    this.adapterController.Dispose();
            }
            catch (Exception e)
            {
                Debug.Log("Problem at disposing server!");
            }
        }


        /// <summary>
        /// Indicates whether the current build is in headless mode (no graphics device)
        /// </summary>
        /// <returns></returns>
        private static bool IsHeadlessMode()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
        }

    }
}
