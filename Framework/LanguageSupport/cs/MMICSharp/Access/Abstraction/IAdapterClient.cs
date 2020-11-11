// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;

namespace MMICSharp.Access.Abstraction
{
    /// <summary>
    /// Interface for a client to access the adapter 
    /// </summary>
    public interface IAdapterClient:IDisposable
    {
        /// <summary>
        /// The actual instance/access of the adapter
        /// </summary>
        MMIAdapter.Iface Access
        {
            get;
            set;
        }
    }
}
