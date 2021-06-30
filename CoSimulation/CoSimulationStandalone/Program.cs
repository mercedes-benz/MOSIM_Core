// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using CoSimulationMMU;
using MMICSharp.Adapter;
using MMICSharp.Adapter.MMUProvider;
using MMIStandard;
using MMICSharp.Common;
using System;
using System.Collections.Generic;

namespace CoSimulationStandalone
{
    /// <summary>
    /// Class to store the CoSim and adapter description.
    /// </summary>
    public static class Data
    {
        internal static MMUDescription CoSimMMUDescription = new MMUDescription()
        {
            ID = Guid.NewGuid().ToString(),
            AssemblyName = "co-simulation",
            Author = "xy",
            Language = "C#",
            MotionType = "co-simulation",
            Name = "CoSimulation",
            ShortDescription = "short",
            LongDescription = "long",
            Parameters = new List<MParameter>(),
            Version = "Debug"
        };

        internal static MAdapterDescription AdapterDescription = new MAdapterDescription()
        {
            ID = "co-sim16102019-standalone-v1.0",
            Language = "C#",
            Name = "CoSimulation Adapter C#",
            Properties = new Dictionary<string, string>(),
            Addresses = new List<MIPAddress>() { new MIPAddress("127.0.0.1", 8998) }
        };


        internal static SessionData SessionData;

    }


    /// <summary>
    /// The application can be utilized for hosting the co-simulation as standalone application or for debugging purposes.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"   ______           _____ _                 __      __  _           ");
            Console.WriteLine(@"  / ____/___       / ___/(_)___ ___  __  __/ /___ _/ /_(_)___  ____ ");
            Console.WriteLine(@" / /   / __ \______\__ \/ / __ `__ \/ / / / / __ `/ __/ / __ \/ __ \");
            Console.WriteLine(@"/ /___/ /_/ /_____/__/ / / / / / / / /_/ / / /_/ / /_/ / /_/ / / / /");
            Console.WriteLine(@"\____/\____/     /____/_/_/ /_/ /_/\__,_/_/\__,_/\__/_/\____/_/ /_/ ");

            Data.SessionData = new SessionData();

            //Create a new adapter controller
            using (AdapterController adapterController = new AdapterController(Data.SessionData, Data.AdapterDescription, new MIPAddress("127.0.0.1", 9009), new DescriptionBasedMMUProvider(Data.CoSimMMUDescription), new CosimInstantiator(Data.AdapterDescription.Addresses[0], new MIPAddress("127.0.0.1", 9009))))
            {
                adapterController.Start();

                Console.ReadLine();
            }
        }
    }     



    /// <summary>
    /// Create a new class which instantiates the respective co-simulation MMU
    /// </summary>
    public class CosimInstantiator : IMMUInstantiation
    {

        private MIPAddress adapterAddress;
        private MIPAddress registryAddress;
        public CosimInstantiator(MIPAddress adapterAddress, MIPAddress registryAddress)
        {
            this.adapterAddress = adapterAddress;
            this.registryAddress = registryAddress;
        }

        public IMotionModelUnitDev InstantiateMMU(MMULoadingProperty mmuLoadingProperty)
        {
            if (mmuLoadingProperty.Description.ID  == Data.CoSimMMUDescription.ID)
            {
                CoSimulationMMUImpl instance = new CoSimulationMMUImpl(adapterAddress, registryAddress);

                return instance;
            }
            return null;
        }
    }
}



