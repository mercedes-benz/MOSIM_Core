// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MMICSharp.Adapter
{
    /// <summary>
    /// Basic class which tracks available MMUs and hosts an adapter server
    /// </summary>
    public class AdapterController : IDisposable
    {

        /// <summary>
        /// Flag which indicates whether the adapter controller has been started
        /// </summary>
        public bool Started
        {
            get;
            private set;
        } = false;


        protected SessionData SessionData
        {
            get;
            private set;
        }


        #region private fields


        /// <summary>
        /// The utilized MMU instantiator
        /// </summary>
        private IMMUInstantiation mmuInstantiator;


        /// <summary>
        /// The assigned mmu provider
        /// </summary>
        private IMMUProvider mmuProvider;

        /// <summary>
        /// Instance of the utilized server to communicate with the mmu abstraction
        /// </summary>
        private AdapterServer thriftServer;


        /// <summary>
        /// The address of the thrift server
        /// </summary>
        private readonly MIPAddress address;


        /// <summary>
        /// The address of the register
        /// </summary>
        private readonly MIPAddress mmiRegisterAddress;


        /// <summary>
        /// The description of the adapter
        /// </summary>
        private readonly MAdapterDescription adapterDescription;


        /// <summary>
        /// The instance of the adapter implementation
        /// </summary>
        private readonly MMIAdapter.Iface adapterImplementation;


        /// <summary>
        /// The utilized registration handler (manages the registration of the adapter at the central register)
        /// </summary>
        private AdapterRegistrationHandler registrationHandler;

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="description">The adapter description. The first address in  the Addressed field is used for hosting the server.</param>
        /// <param name="mmiRegisterAddress">The address of the register (where all services, adapters and mmus are registered)</param>
        /// <param name="mmuPath">The path where the MMUs are located</param>
        /// <param name="mmuProvider">A class which provides information regarding the available MMUs</param>
        /// <param name="mmuInstantiatior">A class which can instantiate MMUs based on the file and description</param>
        /// <param name="customAdapterImplementation">An optionally specified customAdapterImplementation which is utilized instead of the default one</param>
        public AdapterController(SessionData sessionData, MAdapterDescription description, MIPAddress mmiRegisterAddress, IMMUProvider mmuProvider, IMMUInstantiation mmuInstantiatior, MMIAdapter.Iface customAdapterImplementation = null)
        {
            //Assign the session data
            this.SessionData = sessionData;

            //Is the default implementation if not explicitly set
            if(customAdapterImplementation == null)
                this.adapterImplementation = new ThriftAdapterImplementation(sessionData, mmuInstantiatior);
            else
                this.adapterImplementation = customAdapterImplementation;

            //Assign the adapter implementation instance
            SessionData.AdapterInstance = this.adapterImplementation;

            //Assign the mmu instantiator
            this.mmuInstantiator = mmuInstantiatior;
            this.mmuProvider = mmuProvider;

            //Assign the adapter description
            this.adapterDescription = description;
            SessionData.AdapterDescription = description;

            //Assign the addresses
            this.address = description.Addresses[0];
            this.mmiRegisterAddress = mmiRegisterAddress;

            //Assign the MMI register address
            SessionData.MMIRegisterAddress = mmiRegisterAddress;

            //Register on changed event
            this.mmuProvider.MMUsChanged += MmuProvider_MMUsChanged;
        }


        /// <summary>
        /// Starts the controller async
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            return await Task.Factory.StartNew(()=> 
            {
                this.Start();
                return true;
            });
                
        }

        /// <summary>
        /// Starts the adapter controller
        /// </summary>
        /// <param name="adapterImplementation"></param>
        public virtual void Start()
        {
            Logger.Log(Log_level.L_INFO, $"Starting adapter server at {address.Address} {address.Port}: ");

            //Set the start time
            this.SessionData.StartTime = DateTime.Now;

            //Create and start the registration handler
            this.registrationHandler = new AdapterRegistrationHandler(this.mmiRegisterAddress, this.adapterDescription);
           
            //Scan the loadable MMUs
            SessionData.MMULoadingProperties = this.mmuProvider.GetAvailableMMUs();
            this.UpdateAvailableMMUDescriptions(SessionData.MMULoadingProperties);

            //Create and start the thrift server
            this.thriftServer = new AdapterServer(this.address.Address, this.address.Port, this.adapterImplementation);

            //Start the adapter controller in separate thread
            ThreadPool.QueueUserWorkItem(delegate
            {
                this.thriftServer.Start();
                this.Started = true;
            });
        }



        /// <summary>
        /// Disposes the adapter controller
        /// </summary>
        public void Dispose()
        {
            Logger.Log(Log_level.L_INFO, $"Disposing the Adapter Controller");

            //Unregister at event handler
            this.mmuProvider.MMUsChanged -= MmuProvider_MMUsChanged;

            //Dispose the registration handler
            this.registrationHandler.Dispose();

            //Dispose the thrift server
            this.thriftServer.Dispose();
        }


        /// <summary>
        /// Method is called if the set of available MMUs was changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MmuProvider_MMUsChanged(object sender, EventArgs e)
        {
            //Refetch all MMUs if something has changed
            SessionData.MMULoadingProperties = this.mmuProvider.GetAvailableMMUs();
            this.UpdateAvailableMMUDescriptions(SessionData.MMULoadingProperties);
        }

        /// <summary>
        /// Method is utilized to update the available MMU descriptions
        /// </summary>
        /// <param name="loadingProperties"></param>
        private void UpdateAvailableMMUDescriptions(Dictionary<string,MMULoadingProperty> loadingProperties)
        {
            SessionData.MMUDescriptions.Clear();
            foreach(var entry in loadingProperties)
            {
                SessionData.MMUDescriptions.Add(entry.Value.Description);
            }           
        }

    }
}
