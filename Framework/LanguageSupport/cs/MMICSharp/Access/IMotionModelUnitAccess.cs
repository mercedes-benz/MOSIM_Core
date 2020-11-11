// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;


namespace MMICSharp.Access
{
    /// <summary>
    /// Basic interface for representing MMUs in C# (accessing view)
    /// </summary>
    public interface IMotionModelUnitAccess: MotionModelUnit.Iface
    {
        /// <summary>
        /// The ID of the MMU
        /// </summary>
        string ID
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the MMU
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The MMUDescription of the MMU
        /// </summary>
        MMUDescription Description
        {
            get;
            set;
        }
    }
}
