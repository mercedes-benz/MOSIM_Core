// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using Thrift.Protocol;
using Thrift.Transport;

namespace MMICSharp.Clients
{
    /// <summary>
    /// Basic client for an adapter
    /// </summary>
    public class AdapterClient:ClientBase
    {

        /// <summary>
        /// The access of the client
        /// </summary>
        public MMIAdapter.Client Access;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public AdapterClient(string address, int port, bool autoStart = true):base(address,port,autoStart)
        {
        }


        protected override void AssignAccess(TProtocol protocol)
        {
            this.Access = new MMIAdapter.Client(protocol);
        }
    }
}
