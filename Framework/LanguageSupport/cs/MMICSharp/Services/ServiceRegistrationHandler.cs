// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Clients;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MMICSharp.Services
{
    /// <summary>
    /// Class which automatically handles the registration of the specified service description at the central MMI register.
    /// </summary>
    public class ServiceRegistrationHandler : IDisposable
    {
        /// <summary>
        /// The timespan for the update
        /// </summary>
        public TimeSpan UpdateTime = TimeSpan.FromSeconds(1);


        #region private fields

        /// <summary>
        /// The utilized thread
        /// </summary>
        private readonly Thread thread;

        /// <summary>
        /// The utilized cancellation token for the thread
        /// </summary>
        private readonly CancellationTokenSource cts;

        /// <summary>
        /// The address of the register
        /// </summary>
        private readonly MIPAddress address;

        /// <summary>
        /// The assigned adapter description
        /// </summary>
        private readonly MServiceDescription description;

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="registerAddress"></param>
        /// <param name="description"></param>
        public ServiceRegistrationHandler(MIPAddress registerAddress, MServiceDescription description, bool autoStart = true)
        {
            this.address = registerAddress;
            this.description = description;
            this.thread = new Thread(new ThreadStart(this.HandleRegistration));
            this.cts = new CancellationTokenSource();

            if (autoStart)
                this.Start();
        }


        /// <summary>
        /// Method to actually handle the registration
        /// </summary>
        private void HandleRegistration()
        {
            while (!this.cts.IsCancellationRequested)
            {
                //Check if registration is already available
                if (!RegistrationAvailable(description))
                {
                    try
                    {
                        //Try to register
                        using (MMIRegisterServiceClient client = new MMIRegisterServiceClient(this.address.Address, this.address.Port))
                        {
                            client.Access.RegisterService(description);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                Thread.Sleep(this.UpdateTime);
            }
        }

        /// <summary>
        /// Method starts the registration of the adapter
        /// </summary>
        /// <param name="adapterDescription"></param>
        private void Start()
        {
            if (!this.thread.IsAlive)
                this.thread.Start();
        }


        /// <summary>
        /// Disposes the handler
        /// </summary>
        public void Dispose()
        {
            this.cts.Cancel();

            try
            {
                //Try to register
                using (MMIRegisterServiceClient client = new MMIRegisterServiceClient(this.address.Address, this.address.Port))
                {
                    client.Access.UnregisterService(description);
                }
            }
            catch (Exception)
            {
            }

            this.cts.Dispose();
        }


        /// <summary>
        /// Checks whether the registration is already available at the register
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <returns></returns>
        private bool RegistrationAvailable(MServiceDescription serviceDescription)
        {
            try
            {
                using (MMIRegisterServiceClient client = new MMIRegisterServiceClient(this.address.Address, this.address.Port))
                {
                    List<MServiceDescription> serviceDescriptions = client.Access.GetRegisteredServices("");

                    if (serviceDescriptions.Exists(s => s.ID == serviceDescription.ID && s.Addresses.Exists(x => x.Address == serviceDescription.Addresses[0].Address && x.Port == serviceDescription.Addresses[0].Port)))
                        return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
