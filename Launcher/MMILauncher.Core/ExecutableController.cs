// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace MMILauncher.Core
{
    /// <summary>
    /// Class represents a controller which can start executable applications
    /// </summary>
    public class ExecutableController : IExecutableController
    {
        #region properties

        /// <summary>
        /// The description format of the controller
        /// </summary>
        public MExecutableDescription Description
        {
            get;
            set;
        }

        /// <summary>
        /// The assigned address which is transmitted at the startup
        /// </summary>
        public string Address
        {
            get;
            set;
        }

        /// <summary>
        /// The assigned port
        /// </summary>
        public int Port
        {
            get;
            set;
        }


        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The utilized process
        /// </summary>
        public Process Process
        {
            get;
            set;
        }

        /// <summary>
        /// The filepath of the assembly
        /// </summary>
        public string Filepath
        {
            get;
            set;
        }

        /// <summary>
        /// Flag which indicates whether the communciaton has been successfully initialized
        /// </summary>
        public bool Initialized
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Flag which indicates whether the initialization has been aborted
        /// </summary>
        public bool Aborted
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Specifies whether the process window is hidden
        /// </summary>
        public bool HideWindow
        {
            get;
            set;
        } = false;

        public bool SceneSynchronized
        {
            get;
            set;
        } = false;

        public bool useFolder { get; set; }

        #endregion


        #region private variables

        private readonly string mmuPath;
        private readonly MIPAddress registerAddress;

        #endregion

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="webserverAddress"></param>
        /// <param name="filepath"></param>
        public ExecutableController(MExecutableDescription description, MIPAddress address, MIPAddress registerAddress, string mmuPath, string filepath, bool hideWindow, bool useFolder = false)
        {
            this.Name = description.Name;
            this.Description = description;
            this.Address = address.Address;
            this.Port = address.Port;
            this.registerAddress = registerAddress;

            this.Filepath = filepath;
            this.mmuPath = mmuPath;
            this.HideWindow = hideWindow;
            this.useFolder = useFolder;
        }



        /// <summary>
        /// Method starts the processes.
        /// Can be overwritten by a child class
        /// </summary>
        public virtual MBoolResponse Start()
        {
            //Define the startup
            ProcessStartInfo pStartInfo = new ProcessStartInfo
            {
                FileName = Filepath,
                Arguments = "-a " + this.Address + ":" + this.Port + " -r " + (this.registerAddress.Address + ":" + this.registerAddress.Port) + " -m " + "\"" + this.mmuPath + "\"",
            };

            if (this.useFolder)
            {
                pStartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Filepath);
                pStartInfo.FileName = System.IO.Path.GetFileName(Filepath);
            }


            if (this.HideWindow)
            {
                pStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            try
            {
                //Create a new process
                this.Process = Process.Start(pStartInfo);
            }
            catch (Exception e)
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>()
                    {
                        e.Message, e.StackTrace, e.Source
                    }
                };

            }

            return new MBoolResponse(true);

        }


        /// <summary>
        /// Disposes the actual executable controller/closes the process.
        /// The method can be overwritten by a child class
        /// </summary>
        /// <returns></returns>
        public virtual MBoolResponse Dispose()
        {
            try
            {
                if (this.Process != null)
                    this.Process.Kill();

                this.Aborted = true;

            }
            catch (Exception e)
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>()
                    {
                        e.Message,
                        e.StackTrace
                    }
                };
            }
            return new MBoolResponse(true);
        }




        public void SetupWindow(int x, int y, int width, int height)
        {
            if (this.Process != null)
                MoveWindow(this.Process.MainWindowHandle, x, y, width, height, true);
        }


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);





    }
}

