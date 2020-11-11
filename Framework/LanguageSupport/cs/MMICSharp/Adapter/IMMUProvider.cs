// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;


namespace MMICSharp.Adapter
{
    /// <summary>
    /// Interface representing a MMU provider utilized within the adapter. The class is responsible for providing a list of available MMUs (e.g. from file system).
    /// </summary>
    public interface IMMUProvider
    {
        event EventHandler<EventArgs> MMUsChanged;

        Dictionary<string,MMULoadingProperty> GetAvailableMMUs();
    }




}
