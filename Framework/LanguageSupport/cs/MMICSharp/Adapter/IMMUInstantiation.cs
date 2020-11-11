// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMIStandard;

namespace MMICSharp.Adapter
{
    /// <summary>
    /// Interface for encapsulating different MMU Instantiation methods
    /// </summary>
    public interface IMMUInstantiation
    {
        IMotionModelUnitDev InstantiateMMU(MMULoadingProperty loadingProperty);
    }
}
