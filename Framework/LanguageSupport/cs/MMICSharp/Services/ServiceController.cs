// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMIStandard;
using System.Threading;
using System.Threading.Tasks;
using Thrift;

namespace MMICSharp.Services
{
    /// <summary>
    /// Basic class which provides a service controller that manages a server as well as the automatic registration at the register. 
    /// </summary>
    public class ServiceController
    {
        /// <summary>
        /// Flag which indicates whether the service controller has been started
        /// </summary>
        public bool Started
        {
            get;
            private set;
        } = false;


        #region private fields


        /// <summary>
        /// Instance of the utilized server to communicate with the MMI framework
        /// </summary>
        private ThriftServerBase thriftServer;


        /// <summary>
        /// The address of the thrift server
        /// </summary>
        private readonly MIPAddress address;


        /// <summary>
        /// The address of the register
        /// </summary>
        private readonly MIPAddress mmiRegisterAddress;


        /// <summary>
        /// The description of the service
        /// </summary>
        private readonly MServiceDescription serviceDescription;


        /// <summary>
        /// The instance of the adapter implementation
        /// </summary>
        private readonly TProcessor processor;


        /// <summary>
        /// The utilized registration handler (manages the registration of the service at the central register)
        /// </summary>
        private ServiceRegistrationHandler registrationHandler;

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="description">The service description</param>
        /// <param name="mmiRegisterAddress">The address of the mmi register</param>
        /// <param name="processor">The assigned processor of the service controller</param>
        public ServiceController(MServiceDescription description, MIPAddress mmiRegisterAddress, TProcessor processor)
        {
            //Assign the adapter description
            this.serviceDescription = description;
            //Assign the addresses
            this.address = description.Addresses[0];
            this.mmiRegisterAddress = mmiRegisterAddress;
            //Assign the processor
            this.processor = processor;
        }


        /// <summary>
        /// Starts the controller async
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                this.Start();
                return true;
            });

        }

        /// <summary>
        /// Starts the service controller
        /// </summary>
        /// <param name="adapterImplementation"></param>
        public virtual void Start()
        {

            //Create and start the registration handler
            this.registrationHandler = new ServiceRegistrationHandler(this.mmiRegisterAddress, this.serviceDescription);

            //Create and start the thrift server
            this.thriftServer = new ThriftServerBase(this.address.Address, this.address.Port, this.processor);

            //Start the adapter controller in separate thread
            ThreadPool.QueueUserWorkItem(delegate
            {
                this.thriftServer.Start();
                this.Started = true;
            });
        }



        /// <summary>
        /// Disposes the service controller
        /// </summary>
        public void Dispose()
        { 
            //Dispose the registration handler
            this.registrationHandler.Dispose();

            //Dispose the thrift server
            this.thriftServer.Dispose();
        }
    }
}
