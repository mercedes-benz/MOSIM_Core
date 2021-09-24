// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Adapter;
using MMICSharp.Common;
using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DebugAdapter
{
    /// <summary>
    /// Class represents an adapter that hosts C# MMU and allows debugging if integrated within the MMU Repository
    /// </summary>
    public class DebugAdapter:IDisposable
    {
        public static SessionData SessionData = new SessionData();
        public static MIPAddress RegisterAddress = new MIPAddress("127.0.0.1", 9009);
        private AdapterController adapterController;


        public DebugAdapter(Type mmuType, int hostingPort = 8999, string registerAddress = "127.0.0.1", int registerPort = 9009)
        {

            RegisterAddress = new MIPAddress(registerAddress, registerPort);

            //Create a new logger instance
            Logger.Instance = new Logger
            {
                //Log everything
                Level = Log_level.L_DEBUG
            };

            MAdapterDescription adapterDescription = new MAdapterDescription()
            {
                ID = Guid.NewGuid().ToString(),
                Language = "C#",
                Name = "Debug Adapter C#",
                Properties = new Dictionary<string, string>(),
                Addresses = new List<MIPAddress>() { new MIPAddress("127.0.0.1", hostingPort) }
            };

            adapterController = new AdapterController(SessionData, adapterDescription, RegisterAddress, new MMUProvider(mmuType), new MMUInstantiator(mmuType));
        }

        public void Start()
        {
            adapterController.Start();
        }

        public void Dispose()
        {
            if(adapterController!=null)
                adapterController.Dispose();
        }



        public class MMUProvider : IMMUProvider
        {
            public event EventHandler<EventArgs> MMUsChanged;

            private Type mmuType;

            public MMUProvider(Type mmuType)
            {
                this.mmuType = mmuType;
            }

            public Dictionary<string, MMULoadingProperty> GetAvailableMMUs()
            {

                //Load the description from path -> Change this line for testing a different MMU
                MMUDescription mmuDescription = GetDescriptionFromClass(mmuType);


                return new Dictionary<string, MMULoadingProperty>()
                {
                    {mmuDescription.ID, new MMULoadingProperty(){ Description = mmuDescription} }
                };
            }
        }

        public class MMUInstantiator : IMMUInstantiation
        {

            private Type mmuType;

            public MMUInstantiator(Type mmuType)
            {
                this.mmuType = mmuType;
            }

            public IMotionModelUnitDev InstantiateMMU(MMULoadingProperty loadingProperty)
            {
                //Instantiate the respective MMU -> Change this line for testing a different MMU
                return Activator.CreateInstance(mmuType) as IMotionModelUnitDev;
            }
        }


        /// <summary>
        /// Instantiates a MMU  from file
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="mmuDescription"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        private static MMUDescription GetDescriptionFromClass(Type classType)
        {
            if (classType != null)
            {
                //Get all defined attributes
                List<object> attributes = GetAttributes(classType);


                MMUDescriptionAttribute mmuDescriptionAttr = attributes.Find(s => s is MMUDescriptionAttribute) as MMUDescriptionAttribute;
                var mParamterAttr = attributes.Where(s => s is MParameterAttribute);
                var simEvents = attributes.Where(s => s is MSimulationEventAttribute);


                //Create the mmu description
                MMUDescription mmuDescription = new MMUDescription()
                {
                    Name = mmuDescriptionAttr.Name,
                    Author = mmuDescriptionAttr.Author,
                    Version = mmuDescriptionAttr.Version,
                    MotionType = mmuDescriptionAttr.MotionType,
                    LongDescription = mmuDescriptionAttr.LongDescription,
                    ShortDescription = mmuDescriptionAttr.ShortDescription,
                    AssemblyName ="debug",

                    //Language can be automatically set
                    Language = "C#",

                    //Auto-generate an ID
                    ID = Guid.NewGuid().ToString(),
                };


                //Add all parameters
                if (mParamterAttr != null)
                {
                    mmuDescription.Parameters = new System.Collections.Generic.List<MParameter>();
                    foreach (MParameterAttribute param in mParamterAttr)
                    {
                        mmuDescription.Parameters.Add(new MParameter(param.Name, param.Type, param.Description, param.Required));
                    }
                }


                //Add all events (if defined)
                if (simEvents != null)
                {
                    mmuDescription.Events = new System.Collections.Generic.List<string>();
                    foreach (MSimulationEventAttribute param in simEvents)
                    {
                        mmuDescription.Events.Add(param.Type);
                    }
                }

                //Return the created MMU Description
                return mmuDescription;


            }

            return null;
        }

        private static List<object> GetAttributes(Type classType)
        {
            List<object> result = new List<object>();

            object[] attributes = classType.GetCustomAttributes(true);

            if (attributes != null || attributes.Length > 0)
                result.AddRange(attributes.ToList());


            var methods = classType.GetMethods();
            foreach (var method in methods)
            {
                var methodAttributes = method.GetCustomAttributes(true);

                if (methodAttributes != null && methodAttributes.Length > 0)
                    result.AddRange(methodAttributes.ToList());
            }

            return result;
        }



    }
}
