// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Adapter;
using MMICSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MMIAdapterCSharp
{
    /// <summary>
    /// Class which instantiates basic C# MMUs
    /// </summary>
    public class CSharpMMUInstantiator : IMMUInstantiation
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
            //Check if the MMU supports the specified language
            if (mmuLoadingProperty.Description.Language != "C#" && mmuLoadingProperty.Description.Language != "C++CLR")
                return null;


            //Load the mmu from filesystem and instantiate
            if (mmuLoadingProperty.Path != null)
            {
                Assembly assembly = Assembly.LoadFrom(mmuLoadingProperty.Path);

                //Get the specific type of the class which implementd the IMotionModelUnitDev interface
                Type classType = GetMMUClassType(assembly);

                if (classType != null)
                {
                    try
                    {

                        //Create a new mmu instance within the same app domain                            
                        IMotionModelUnitDev mmuInstance = Activator.CreateInstance(classType) as IMotionModelUnitDev;

                        //To do -> in future instantiate directly from ram -> Loading properties already provide within the data dictionary all dlls and resources

                        //To do  -> incorporate app domain -> currently not working
                        //IMotionModelUnitDev mmuInstance = InstantiateInAppDomain(mmuDescription, sessionContent.SessionId, filePath);

                        return mmuInstance;
                    }
                    catch (Exception e)
                    {
                        Logger.Log(Log_level.L_ERROR, $"Could not load MMU: {mmuLoadingProperty.Description.Name} {mmuLoadingProperty.Description.AssemblyName}, exception: {e.Message}");
                    }

                }
            }

            return null;
        }


        /// <summary>
        /// Method which returns the class type which implements the MMU interface
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Type GetMMUClassType(Assembly assembly)
        {
            List<Type> classTypes = assembly.GetTypes().ToList();

            Type classType = classTypes.Find(s => s != null && s.GetInterfaces().Contains(typeof(IMotionModelUnitDev)));

            return classType;
        }

    }
}
