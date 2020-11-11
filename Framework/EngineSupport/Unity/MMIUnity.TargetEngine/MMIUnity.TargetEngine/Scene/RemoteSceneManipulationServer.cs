// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Services;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Threading;


namespace MMIUnity.TargetEngine.Scene
{
    /// <summary>
    /// Class represents a server which provides the possibility to handle scene manipulations based on remote requests
    /// </summary>
    public class RemoteSceneManipulationServer : IDisposable
    {
        /// <summary>
        /// The utilized base server with the default configuration used within the MMI framework.
        /// </summary>
        private readonly ServiceController controller;

        /// <summary>
        /// The utilized description
        /// </summary>
        private readonly MServiceDescription description = new MServiceDescription()
        {
            ID = Guid.NewGuid().ToString(),
            Name = "remoteSceneManipulationUnity",
            Language = "Unity"
        };


        /// <summary>
        /// Constructor to create a new server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="implementation"></param>
        public RemoteSceneManipulationServer(MIPAddress address, MIPAddress registerAddress, MSynchronizableScene.Iface implementation)
        {
            //Add the address to the description
            this.description.Addresses = new List<MIPAddress>() { address };

            //Create a new controller
            this.controller = new ServiceController(description, registerAddress, new MSynchronizableScene.Processor(implementation));
        }


        /// <summary>
        /// Starts the adapter server
        /// </summary>
        public void Start()
        {

            ThreadPool.QueueUserWorkItem(delegate
            {
                this.controller.Start();
            });
        }

        /// <summary>
        /// Disposes the adapter server
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.controller.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }

}
