// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;

namespace MMICSharp.Adapter
{

    /// <summary>
    /// A Server which handles the thrift communication
    /// </summary>
    public class AdapterServer : IDisposable
    {
        /// <summary>
        /// Class representation of a buffered transport factory
        /// </summary>
        private class BufferedTransportFactory : TTransportFactory
        {
            public override TTransport GetTransport(TTransport trans)
            {
                return new TBufferedTransport(trans);
            }
        }

        private TThreadPoolServer server;
        private readonly string address;
        private readonly int port;
        private readonly MMIAdapter.Iface implementation;

        /// <summary>
        /// Constructor to create a new server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="implementation"></param>
        public AdapterServer(string address, int port, MMIAdapter.Iface implementation)
        {
            this.address = address;
            this.port = port;
            this.implementation = implementation;
        }


        /// <summary>
        /// Starts the adapter server
        /// </summary>
        public void Start()
        {
            MMIAdapter.Processor processor = new MMIAdapter.Processor(implementation);

            TServerTransport serverTransport = new TServerSocket(this.port);

            //Use a multithreaded server
            this.server = new TThreadPoolServer(processor, serverTransport, new BufferedTransportFactory(), new TCompactProtocol.Factory());
         
            this.server.Serve();
        }

        /// <summary>
        /// Disposes the adapter server
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.server.Stop();
            }
            catch (Exception)
            {
            }
        }


    }
}
