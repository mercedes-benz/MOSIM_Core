// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using UnityEngine;

namespace MMIUnity.TargetEngine
{
    /// <summary>
    /// Class which contains all MMI settings
    /// </summary>
    public class MMISettings : MonoBehaviour
    {
        /// <summary>
        /// The port for the access
        /// </summary>
        [Header("The port of the MMIRegister")]
        public int MMIRegisterPort = 9009;


        [Header("The address of the MMI Register")]
        public string MMIRegisterAddress = "127.0.0.1";

        /// <summary>
        /// The port for the access
        /// </summary>
        [Header("The port used for remotely accessing the scene")]
        public int RemoteSceneAccessPort = 9000;

        /// <summary>
        /// The port for the access
        /// </summary>
        [Header("The address used for remotely accessing the scene")]
        public string RemoteSceneAccessAddress = "127.0.0.1";

        /// <summary>
        /// The port for the external write access
        /// </summary>
        [Header("The port used for remotely manipulating the scene")]
        public int RemoteSceneWritePort = 9001;


        /// <summary>
        /// The port for the access
        /// </summary>
        [Header("The address used for remotely manipulating the scene")]
        public string RemoteSceneWriteAddress = "127.0.0.1";

        /// <summary>
        /// Specifies whether the scene is accessible for external clients via thrift server
        /// </summary>
        [Header("Specifies whether the scene is accessible for external clients via thrift server")]
        public bool AllowRemoteSceneConnections = true;

        
        /// <summary>
        /// The port for the skeleton access
        /// </summary>
        [Header("The port used for remotely accessing the skeleton")]
        public int RemoteSkeletonAccessPort = 9999;


        /// <summary>
        /// The port for the skeleton access
        /// </summary>
        [Header("The address used for remotely accessing the skeleton")]
        public string RemoteSkeletonAccessAddress = "127.0.0.1";



        /// <summary>
        /// Specifies whether the skeleton is accessible for external clients via thrift server
        /// </summary>
        [Header("Specifies whether the skeleton is accessible for external clients via thrift server")]
        public bool AllowRemoteSkeletonConnections = true;


    }


}
