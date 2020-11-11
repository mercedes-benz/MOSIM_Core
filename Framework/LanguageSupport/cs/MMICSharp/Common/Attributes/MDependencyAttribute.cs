// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;


namespace MMICSharp.Common.Attributes
{
    /// <summary>
    /// Attribute class for defining a MDependencies in code
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MDependencyAttribute : Attribute
    {
        public string ID;
        public MDependencyType Type;
        public string Name;
        public float MinVersion;
        public float MaxVersion;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="minVersion"></param>
        /// <param name="maxVersion"></param>
        public MDependencyAttribute(string id, MDependencyType type, string name, float minVersion, float maxVersion)
        {
            this.ID = id;
            this.Type = type;
            this.Name = name;
            this.MinVersion = minVersion;
            this.MaxVersion = maxVersion;
        }
    }
}
