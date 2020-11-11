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
    public class MParameterAttribute : Attribute
    {
        public string Name;

        public string Type;

        public string Description;

        public bool Required;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="description"></param>
        public MParameterAttribute(string name, string type, string description, bool required)
        {
            this.Name = name;
            this.Type = type;
            this.Description = description;
            this.Required = required;
        }
    }
}
