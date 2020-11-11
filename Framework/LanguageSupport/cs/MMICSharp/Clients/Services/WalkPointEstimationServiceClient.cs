// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using Thrift.Protocol;

namespace MMICSharp.Clients
{
    /// <summary>
    /// A client for the walk point estimation service
    /// </summary>
    public class WalkPointEstimationServiceClient : ClientBase
    {
        /// <summary>
        /// The access of the client
        /// </summary>
        public MWalkPointEstimationService.Client Access;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public WalkPointEstimationServiceClient(string address, int port, bool autoStart = true) : base(address, port, autoStart)
        {
        }

        protected override void AssignAccess(TProtocol protocol)
        {
            this.Access = new MWalkPointEstimationService.Client(protocol);
        }
    }
}
