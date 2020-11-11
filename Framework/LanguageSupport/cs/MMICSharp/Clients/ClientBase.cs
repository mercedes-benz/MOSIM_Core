// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using Thrift.Protocol;
using Thrift.Transport;

namespace MMICSharp.Clients
{
    /// <summary>
    /// Abstract base class for all clients
    /// </summary>
    public abstract class ClientBase : IDisposable
    {
        protected readonly string address;
        protected readonly int port;
        protected TTransport transport;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public ClientBase(string address, int port, bool autoStart = true)
        {
            //Assign address and port
            this.address = address;
            this.port = port;

            //Create a buffered transport
            this.transport = new TBufferedTransport(new TSocket(this.address, this.port));

            //Call the specific method to assign the access
            this.AssignAccess(new TCompactProtocol(transport));

            //Automtically start (if defined)
            if (autoStart)
                this.Start();
        }

        /// <summary>
        /// Starts the client
        /// </summary>
        public void Start()
        {
            try
            {
                //Open the transport -> close at the end -> improved performance
                if (!transport.IsOpen)
                    transport.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Abstract class which needs to be implemented.
        /// The access needs to be assigned (e.g. this.Access = new MMIRegisterService.Client(protocol);)
        /// </summary>
        protected abstract void AssignAccess(TProtocol protocol);

        /// <summary>
        /// Disposes the client
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.transport.Close();
            }
            catch (Exception)
            {

            }
        }
    }
}
