// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;

namespace MMICSharp.Common
{
    /// <summary>
    /// Basic MMU interface for C# from a development point of view
    /// </summary>
    public interface IMotionModelUnitDev: MotionModelUnit.Iface
    {
        /// <summary>
        /// Access to the services which are by default shipped with the MMI standard
        /// </summary>
        IServiceAccess ServiceAccess
        {
            get;
            set;
        }

        /// <summary>
        /// The access to the scene
        /// </summary>
        MSceneAccess.Iface SceneAccess
        {
            get;
            set;
        }

        MSkeletonAccess.Iface SkeletonAccess
        {
            get;
            set;
        }

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
        /// Instance of the adapter to be used optionally (if accessing other MMUs via Abstraction)
        /// </summary>
        AdapterEndpoint AdapterEndpoint
        {
            get;
            set;
        }
    }
}
