// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common.Communication;
using MMILauncher.Core;
using MMIStandard;
using System;
using System.IO;
using System.Linq;

namespace MMILauncher.Console
{
    class Program
    {
        private static MMIRegisterServiceImplementation registerService;
        private static MMIRegisterThriftServer registerServer;
        private static string address = "127.0.0.1";
        private static int port = 9500;

        static void Main(string[] args)
        {
            //
            registerService = new MMIRegisterServiceImplementation();

            registerService.OnAdapterRegistered += RegisterService_OnAdapterRegistered;

            ///Start the register server
            registerServer = new MMIRegisterThriftServer(RuntimeData.MMIRegisterAddress.Port, registerService);
            registerServer.Start();

            //Setup the environment
            SetupEnvironment(@"..\Adapters", @"..\MMUs", @"..\Services");

            System.Console.ReadLine();

            Dispose();
        }


        private static void RegisterService_OnAdapterRegistered(object sender, RemoteAdapter e)
        {
            System.Console.WriteLine("Adapter registered: " + e.Name);
        }



        /// <summary>
        /// Setups the environment and starts the modules
        /// </summary>
        /// <param name="adapterPath">The path of the modules</param>
        /// <param name="mmuPath">The path of the mmus</param>
        private static void SetupEnvironment(string adapterPath, string mmuPath, string servicePath)
        {

            //Fetch all modules and start them
            foreach (string folderPath in Directory.GetDirectories(adapterPath))
            {
                //Find the description file
                string descriptionFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains("description.json"));

                //Skip if no description file
                if (descriptionFile == null)
                    continue;

                //Get the ExecutableDescription of the adapter
                MExecutableDescription executableDescription = Serialization.FromJsonString<MExecutableDescription>(File.ReadAllText(descriptionFile));

                //Determine the filename of the executable file
                string executableFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains(executableDescription.ExecutableName));

                //Create a controller for the executable process
                ExecutableController exeController = new ExecutableController(executableDescription, new MIPAddress(address, port), RuntimeData.MMIRegisterAddress, mmuPath, executableFile, false);

                RuntimeData.ExecutableControllers.Add(exeController);
                port += 1;
            }

            //Setup the services
            foreach (string folderPath in Directory.GetDirectories(servicePath))
            {
                //Find the description file
                string descriptionFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains("description.json"));

                //Skip if no description file
                if (descriptionFile == null)
                    continue;


                //Get the ExecutableDescription of the service
                MExecutableDescription executableDescription = Serialization.FromJsonString<MExecutableDescription>(File.ReadAllText(descriptionFile));
                string executableFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains(executableDescription.ExecutableName));

                ExecutableController exeController = new ExecutableController(executableDescription, new MIPAddress(address, port), RuntimeData.MMIRegisterAddress, mmuPath, executableFile, false);

                RuntimeData.ExecutableControllers.Add(exeController);
                port += 1;

            }

            //Start the controllers
            foreach (ExecutableController executableController in RuntimeData.ExecutableControllers)
            {
                MBoolResponse response = executableController.Start();

                if (!response.Successful)
                {
                    System.Console.WriteLine("Cannot start application: " + executableController.Name + " " + (response.LogData.Count > 0 ? response.LogData[0] : ""));
                }
            }
        }


        /// <summary>
        /// Disposes all connections and processes
        /// </summary>
        public static void Dispose()
        {
            try
            {
                //Dispose every executed instance
                foreach (ExecutableController executableController in RuntimeData.ExecutableControllers)
                {
                    executableController.Dispose();
                }

                //Dispose every adapter
                foreach (RemoteAdapter remoteAdapter in RuntimeData.AdapterInstances.Values)
                {
                    remoteAdapter.Dispose();
                }
            }
            catch (Exception)
            {
            }

            try
            {
                //Dispose every service connection
                foreach (RemoteService service in RuntimeData.ServiceInstances.Values)
                {
                    service.Dispose();
                }
            }
            catch (Exception)
            {

            }

            try
            {
                registerServer.Dispose();

            }
            catch (Exception)
            {

            }

            //Clear all the data
            RuntimeData.AdapterInstances.Clear();
            RuntimeData.ExecutableControllers.Clear();
            RuntimeData.MMUDescriptions.Clear();
            RuntimeData.ServiceInstances.Clear();
            RuntimeData.SessionIds.Clear();
        }
    }
}
