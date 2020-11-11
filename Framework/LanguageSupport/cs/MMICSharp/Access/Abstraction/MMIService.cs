// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Diagnostics;

namespace MMICSharp.Access.Abstraction
{
    /// <summary>
    /// Class which encapsulates a service
    /// </summary>
    public class MMIService:IDisposable
    {
        /// <summary>
        /// The utilized process
        /// </summary>
        public Process Process;

        /// <summary>
        /// The description of the MMU
        /// </summary>
        public MServiceDescription Description;

        /// <summary>
        /// Specifies whether the process window is hidden
        /// </summary>
        private readonly bool hideWindow;

        /// <summary>
        /// The filepath of the exe
        /// </summary>
        private readonly string filepath;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="description"></param>
        /// <param name="hideWindow"></param>
        public MMIService(string filepath, MServiceDescription description, bool hideWindow)
        {
            this.Description = description;
            this.filepath = filepath;
            this.hideWindow = hideWindow;
        }


        /// <summary>
        /// Start method
        /// </summary>
        public void Start()
        {
            ProcessStartInfo pStartInfo = new ProcessStartInfo
            {
                FileName = filepath
            };

            if (this.hideWindow)
            {
                pStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            //Create a new process
            this.Process = Process.Start(pStartInfo);
        }

        /// <summary>
        /// Disposes the serverice
        /// </summary>
        public void Dispose()
        {
            this.Process.CloseMainWindow();
        }

    }
}
