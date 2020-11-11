// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Eva Binder

using MMIStandard;
using System;
using System.Collections.Generic;
using MMICSharp.Adapter;
using MMICSharp.Adapter.MMUProvider;

namespace MMIAdapterCSharp
{
    /// <summary>
    /// Basic entry point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The utilized session data
        /// </summary>
        private static readonly SessionData sessionData = new SessionData();

        /// <summary>
        /// Session cleaner instance which removes inactive session after a specific timeout
        /// </summary>
        private static SessionCleaner sessionCleaner;

        /// The address of the thrift server
        private static MIPAddress address = new MIPAddress("127.0.0.1", 8900);

        ///The address of the register
        private static MIPAddress mmiRegisterAddress = new MIPAddress("127.0.0.1", 8900);

        /// The path of the mmus
        private static string mmuPath = "";


        /// <summary>
        /// Entry routine
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Create a new logger instance
            Logger.Instance = new Logger
            {
                //Log everything
                Level = Log_level.L_DEBUG
            };

            //Register for unhandled exceptions in the application
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;



            Console.WriteLine(@"   ______  __ __     ___       __            __           ");
            Console.WriteLine(@"  / ____/_/ // /_   /   | ____/ /___ _____  / /____  _____");
            Console.WriteLine(@" / /   /_  _  __/  / /| |/ __  / __ `/ __ \/ __/ _ \/ ___/");
            Console.WriteLine(@"/ /___/_  _  __/  / ___ / /_/ / /_/ / /_/ / /_/  __/ /    ");
            Console.WriteLine(@"\____/ /_//_/    /_/  |_\__,_/\__,_/ .___/\__/\___/_/     ");
            Console.WriteLine(@"                                  /_/                     ");
            Console.WriteLine(@"_________________________________________________________________");



            //Parse the command line arguments
            if (!ParseCommandLineArguments(args))
            {
                Logger.Log(Log_level.L_ERROR, "Cannot parse the command line arguments. Closing the adapter!");
                return;
            }

            Console.WriteLine($"Adapter is reachable at: {address.Address}:{address.Port}");
            Console.WriteLine($"Register is reachable at: {mmiRegisterAddress.Address}:{mmiRegisterAddress.Port}");
            Console.WriteLine($"MMUs will be loaded from: {mmuPath}");
            Console.WriteLine(@"_________________________________________________________________");


            //Create the adapter description -> To do load from file in future
            MAdapterDescription adapterDescription = new MAdapterDescription()
            {
                Name = "CSharpAdapter",
                Addresses = new List<MIPAddress>() { address },
                ID = "438543643-436436435-2354235",
                Language = "C#",
                Parameters = new List<MParameter>(),
                Properties = new Dictionary<string, string>()
            };


            //Create a session cleaner for the utilized session data
            sessionCleaner = new SessionCleaner(sessionData)
            {
                //Session shoould be cleaned after 60 minutes
                Timeout = TimeSpan.FromMinutes(60),

                //The session cleaner should check every minute
                UpdateTime = TimeSpan.FromMinutes(1)
            };

            //Start the session cleaner
            sessionCleaner.Start();

            //Create a new adapter controller which scans the filesystem and checks for MMUs there
            using (AdapterController adapterController = new AdapterController(sessionData, adapterDescription, mmiRegisterAddress, new FileBasedMMUProvider(sessionData, new List<string>() { mmuPath }, new List<string>() { "C#", "C++CLR" }), new CSharpMMUInstantiator()))
            {
                //Start the adapter controller
                adapterController.Start();

                Console.ReadLine();
            }
        }

        /// <summary>
        /// Callback for unhandled exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log(Log_level.L_ERROR, e.ExceptionObject.ToString());

            //Write a log file
            System.IO.File.WriteAllText("CSharpAdapter_Error.log", DateTime.Now.ToString() + " " + e.ExceptionObject.ToString());
        }


        /// <summary>
        /// Tries to parse the command line arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool ParseCommandLineArguments(string[] args)
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
                          address.Address = addr[0];
                          address.Port = int.Parse(addr[1]);
                      }
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
                  }
                },

                { "m|mmupath=", "The path of the mmu folder.",
                  v =>
                  {
                        mmuPath = v;
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
                Console.WriteLine("Cannot parse arguments");
            }


            return false;

        }
    }
}
