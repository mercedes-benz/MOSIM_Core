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
    /// A server for remotely accessing  the scene
    /// </summary>
    public class RemoteSceneAccessServer : IDisposable
    {
        /// <summary>
        /// The service controller to host the remote scene access
        /// </summary>
        private readonly ServiceController controller;

        /// <summary>
        /// The utilized description
        /// </summary>
        private readonly MServiceDescription description = new MServiceDescription()
        {
            ID = Guid.NewGuid().ToString(),
            Name ="remoteSceneAccessUnity",
            Language ="Unity"          
        };


        /// <summary>
        /// Constructor to create a new server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="implementation"></param>
        public RemoteSceneAccessServer(MIPAddress address, MIPAddress registerAddress,  MSceneAccess.Iface implementation)
        {
            //Add the address to the description
            this.description.Addresses = new List<MIPAddress>() { address };

            //Create a new controller
            this.controller = new ServiceController(description, registerAddress, new MSceneAccess.Processor(implementation));

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
