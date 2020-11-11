// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using Thrift.Protocol;
using MMIStandard;


namespace MMICSharp.Clients
{
    public class CollisionDetectionServiceClient:ClientBase
    {
        /// <summary>
        /// The access of the client
        /// </summary>
        public MCollisionDetectionService.Client Access;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public CollisionDetectionServiceClient(string address, int port, bool autoStart = true):base(address,port,autoStart)
        {
        }

        protected override void AssignAccess(TProtocol protocol)
        {
            this.Access = new MCollisionDetectionService.Client(protocol);
        }
    }
}

