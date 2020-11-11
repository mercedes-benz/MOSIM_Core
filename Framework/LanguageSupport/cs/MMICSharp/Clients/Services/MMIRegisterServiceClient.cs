// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using Thrift.Protocol;

namespace MMICSharp.Clients
{
    public class MMIRegisterServiceClient : ClientBase
    {

        /// <summary>
        /// The access of the client
        /// </summary>
        public MMIRegisterService.Client Access;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public MMIRegisterServiceClient(string address, int port, bool autoStart = true):base(address,port,autoStart)
        {
        }

        /// <summary>
        /// Method assigns the actual access
        /// </summary>
        /// <param name="protocol"></param>
        protected override void AssignAccess(TProtocol protocol)
        {
            this.Access = new MMIRegisterService.Client(protocol);
        }
    }
}
