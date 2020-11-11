// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;

namespace MMICSharp.Common.Attributes
{

    /// <summary>
    /// An attribute class for attribute definition within the source code
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
    public class MSimulationEventAttribute : Attribute
    {
        /// <summary>
        /// The name of the simulation event
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the simulation event
        /// </summary>
        public string Type;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="description"></param>
        public MSimulationEventAttribute(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
