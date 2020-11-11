// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;

namespace MMICSharp.Common.Attributes
{
    /// <summary>
    /// Attribute class for defining a MMU description in code
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MMUDescriptionAttribute : Attribute
    {
        public string Name;
        public string MotionType;
        public string SubMotionType;
        public string Author;
        public string Version;
        public string LongDescription;
        public string ShortDescription;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="author"></param>
        /// <param name="version"></param>
        /// <param name="shortDescription"></param>
        /// <param name="longDescription"></param>
        public MMUDescriptionAttribute(string author, string version, string name, string motionType, string subMotionType, string shortDescription, string longDescription)
        {
            this.Author = author;
            this.Version = version;
            this.Name = name;
            this.MotionType = motionType;
            this.SubMotionType = subMotionType;
            this.ShortDescription = shortDescription;
            this.LongDescription = longDescription;
        }
    }
}
