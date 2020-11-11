// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;

namespace MMIStandard
{
    /// <summary>
    /// A factory to instantiate Motion Instructions (automatically assigning an id)
    /// </summary>
    public class MInstructionFactory
    {
        /// <summary>
        /// Creates a new MInstruction with a unique id
        /// </summary>
        /// <returns></returns>
        public static MInstruction Create()
        {
            return new MInstruction()
            {
                ID = GenerateID()   
            };
        }

        /// <summary>
        /// Returns a unique id
        /// </summary>
        /// <returns></returns>
        public static string GenerateID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}

