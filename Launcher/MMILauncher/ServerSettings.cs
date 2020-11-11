// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Klodowski

using System;


namespace MMILauncher
{
    /// <summary>
    /// Class for storing the server settings
    /// </summary>
    [Serializable]
    public class ServerSettings
    {
        /// <taskeditor>
        public string TaskEditorApiUrl = "";
        public string TaskEditorToken = "";
        /// </taskeditor>

        /// <proxy>
        /// Settings are stored in encrypted form in registry as the are shared between all software instances installed for current user
        [NonSerialized]
        public string ProxyHost = "";
        [NonSerialized]
        public string ProxyPort = "";
        [NonSerialized]
        public string ProxyUser = "";
        [NonSerialized]
        public string ProxyPass = "";
        [NonSerialized]
        public bool ProxyEnable = false;
        [NonSerialized]
        public bool ProxyAuthenticate = false;
        [NonSerialized]
        public bool ProxyUseHTTPS = false;
        /// </proxy>


        /// <summary>
        /// Specifies whether the windows are hidden
        /// </summary>
        public bool HideWindows = false;

        /// <summary>
        /// The address used for hosting the register (use 127.0.0.1 for localhost or use other ip assigned to one of the network adapters)
        /// </summary>
        public string RegisterAddress = "127.0.0.1";

        /// <summary>
        /// The network adapter name that is associated with the register address, if address changes but interface name doesn't, the new ip address will be used that is associated witht the interface
        /// </summary>
        public string RegisterInterface = "";

        /// <summary>
        /// The port used for hosting the register
        /// </summary>
        public int RegisterPort = 9009;

        /// <summary>
        /// The min port used for the automatic port assignemnt of the adapters/services
        /// </summary>
        public int MinPort = 8900;

        /// <summary>
        /// The max port used for the automatic port assignment of the adapters/services
        /// </summary>
        public int MaxPort = 9000;

        /// <summary>
        /// The path of the mmi environment
        /// </summary>
        public string DataPath = @"MMIEnvironment/";

        /// <summary>
        /// Specifies whether all processes are automatically started at startup
        /// </summary>
        public bool AutoStart = true;
    }

}
