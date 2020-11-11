// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;

namespace MMILauncher.Core
{
    public interface IExecutableController
    {
        #region properties

        /// <summary>
        /// The description format of the controller
        /// </summary>
        MExecutableDescription Description
        {
            get;
            set;
        }

        /// <summary>
        /// The assigned address which is transmitted at the startup
        /// </summary>
        string Address
        {
            get;
            set;
        }

        /// <summary>
        /// The assigned port
        /// </summary>
        int Port
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the executable controller
        /// </summary>
        string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Flag which indicates whether the communciaton has been successfully initialized
        /// </summary>
        bool Initialized
        {
            get;
            set;
        }

        /// <summary>
        /// Flag which indicates whether the initialization has been aborted
        /// </summary>
        bool Aborted
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether the process window is hidden
        /// </summary>
        bool HideWindow
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Starts the controller
        /// </summary>
        /// <returns></returns>
        MBoolResponse Start();


        //Disposes the controller
        MBoolResponse Dispose();

    }
}
