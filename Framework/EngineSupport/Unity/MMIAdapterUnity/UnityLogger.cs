// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Andreas Kaiser

using UnityEngine;

namespace MMIAdapterUnity
{
    /// <summary>
    /// Implementation of a logger which outputs the text on the unity console
    /// </summary>
    public class UnityLogger : MMICSharp.Adapter.Logger
    {

        /// <summary>
        /// Flag which specifies whether a server build is utilized
        /// </summary>
        private bool isServerBuild = false;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="isServerBuild"></param>
        public UnityLogger(bool isServerBuild = false)
        {
            this.isServerBuild = isServerBuild;
        }


        protected override void LogDebug(string text)
        {
            if (this.isServerBuild)
                base.LogDebug(text);
            else
                Debug.Log(text);
            
        }

        protected override void LogError(string text)
        {
            if (this.isServerBuild)
                base.LogError(text);
            else
                Debug.LogError(text);
        }

        protected override void LogInfo(string text)
        {
            if (this.isServerBuild)
                base.LogInfo(text);
            else
                Debug.Log(text);
        }
    }
}
