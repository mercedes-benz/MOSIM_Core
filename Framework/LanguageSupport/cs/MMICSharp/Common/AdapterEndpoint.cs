// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;

namespace MMICSharp.Common
{
    /// <summary>
    /// Class represents an adapter endpoint which can be utilized to identidy the address of the adapter and directly conenct to it locally
    /// </summary>
    [Serializable]
    public class AdapterEndpoint
    {
        /// <summary>
        /// The instance of the adapter in Cä
        /// </summary>
        public MMIAdapter.Iface Instance;

        /// <summary>
        /// The adapter description
        /// </summary>
        public MAdapterDescription Description;

        /// <summary>
        /// The address of the central utilized MMIRegister
        /// </summary>
        public MIPAddress MMIRegisterAddress;
    }
}
