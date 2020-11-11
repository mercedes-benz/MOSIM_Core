// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common.Attributes;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MMUDescriptionAutoGenerator
{
    /// <summary>
    /// Program automatically creates a MMU description based on a C# dll and specified attributes
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Get the desired path
                string path = args[0];

                string outputPath = System.IO.Path.GetDirectoryName(path) + @"\description.json";

                Console.WriteLine($"Loading .dll from filepath: {path}");

                Console.WriteLine("Auto-generating MMU description");

                //Auto-generate the description based on the dll located at the filepath
                MMUDescription mmuDescription = GetDescriptionFromClass(path);

                Console.WriteLine("MMU description successfully generated");

                try
                {
                    //Save the file to the same folder as the dll
                    System.IO.File.WriteAllText(outputPath, MMICSharp.Common.Communication.Serialization.ToJsonString(mmuDescription));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Problem saving file");
                }

                Console.WriteLine($"Description file successfully stored at {outputPath}");

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed at automatically generating the description file. Exception occured: " + e.Message + e.StackTrace);
            }
        }




        /// <summary>
        /// Instantiates a MMU  from file
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="mmuDescription"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        private static MMUDescription GetDescriptionFromClass(string path)
        {

            //Load the mmu from filesystem and instantiate
            Assembly assembly = Assembly.LoadFrom(path);

            //Get the specific type of the class which implementd the IMotionModelUnitDev interface
            Type classType = GetMMUClassType(assembly);

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

                    //Language can be automatically set
                    Language = "C#",

                    //Auto-generate an ID
                    ID = Guid.NewGuid().ToString(),

                    //Use the assembly name from the path
                    AssemblyName = System.IO.Path.GetFileName(path)
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

        /// <summary>
        /// Method which returns the class type which implements the MMU interface
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Type GetMMUClassType(Assembly assembly)
        {
            return assembly.GetTypes().ToList().Find(s => s != null && s.GetInterfaces().Contains(typeof(MotionModelUnit.Iface)));
        }

    }

}
