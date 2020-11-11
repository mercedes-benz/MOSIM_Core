// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using Thrift;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;

namespace MMICSharp.Common
{
    /// <summary>
    /// A Server which handles the thrift communication
    /// </summary>
    public class ThriftServerBase: IDisposable
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
        private TProcessor processor;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="port"></param>
        /// <param name="processor"></param>
        public ThriftServerBase(int port, TProcessor processor)
        {
            this.port = port;
            this.processor = processor;
        }

        /// <summary>
        /// Constructor to create a new server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="implementation"></param>
        public ThriftServerBase(string address, int port, TProcessor processor):this(port, processor)
        {
            this.address = address;
        }


        /// <summary>
        /// Starts the adapter server
        /// </summary>
        public void Start()
        {
            //Create a new server transport
            TServerTransport serverTransport = new TServerSocket(this.port);

            //Use a multithreaded server
            this.server = new TThreadPoolServer(this.processor, serverTransport, new BufferedTransportFactory(), new TCompactProtocol.Factory());

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
