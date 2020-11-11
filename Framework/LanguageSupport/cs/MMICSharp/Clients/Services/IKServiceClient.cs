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
    public class IKServiceClient:ClientBase
    {

        /// <summary>
        /// The access of the client
        /// </summary>
        public MInverseKinematicsService.Iface Access;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public IKServiceClient(string address, int port, bool autoStart = true):base(address,port,autoStart)
        {
        }

        protected override void AssignAccess(TProtocol protocol)
        {
            this.Access = new MInverseKinematicsService.Client(protocol);
        }
    }
}

