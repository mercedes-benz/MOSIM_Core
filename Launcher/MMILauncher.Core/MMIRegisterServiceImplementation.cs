// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMILauncher.Core
{
    /// <summary>
    /// The implementation of the MMIRegister Service which represents the central information provider within the framework
    /// </summary>
    public class MMIRegisterServiceImplementation : MMIRegisterService.Iface
    {
        public event EventHandler<RemoteAdapter> OnAdapterRegistered;
        public event EventHandler<RemoteAdapter> OnAdapterUnregistered;

        public event EventHandler<RemoteService> OnServiceRegistered;
        public event EventHandler<RemoteService> OnServiceUnregistered;


        /// <summary>
        /// Default constructor
        /// </summary>
        public MMIRegisterServiceImplementation()
        {
        }


        /// <summary>
        /// Method generates a unique session id
        /// </summary>
        /// <returns></returns>
        public virtual string CreateSessionID(Dictionary<string, string> properties)
        {
            string sessionID = Guid.NewGuid().ToString();

            //Generate a new session ID until a new one is found
            while (RuntimeData.SessionIds.Contains(sessionID))
            {
                sessionID = Guid.NewGuid().ToString();
            }

            RuntimeData.SessionIds.Add(sessionID);
            return sessionID;
        }




        /// <summary>
        /// Returns all available MMUs 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public virtual Dictionary<MMUDescription, List<MIPAddress>> GetAvailableMMUs(string sessionID)
        {
            Dictionary<MMUDescription, List<MIPAddress>> dict = new Dictionary<MMUDescription, List<MIPAddress>>();

            //Check all adapters
            foreach (RemoteAdapter adapter in RuntimeData.AdapterInstances.Values)
            {
                //Fetch all loadable MMUs
                List<MMUDescription> mmuDescriptions = adapter.GetLoadableMMUs(sessionID);

                foreach (MMUDescription description in mmuDescriptions)
                {
                    MMUDescription match = dict.Keys.ToList().Find(s => s.ID == description.ID && s.Name == description.Name);

                    if (match == null)
                    {
                        dict.Add(description, new List<MIPAddress>());
                        match = description;
                    }
                    //mask MMUs that are marked to be disabled
                    dict[match].Add(new MIPAddress(adapter.Address, adapter.Port));
                }
            }

            return dict;
        }


        /// <summary>
        /// Returns all registered adapters
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public virtual List<MAdapterDescription> GetRegisteredAdapters(string sessionID)
        {
            return RuntimeData.AdapterInstances.Values.Select(s => s.Description).ToList();
        }


        /// <summary>
        /// Returns all registered services
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public virtual List<MServiceDescription> GetRegisteredServices(string sessionID)
        {
            return RuntimeData.ServiceInstances.Values.Select(s => s.Description).ToList();
        }


        /// <summary>
        /// Method is utilized to register an external adapter
        /// </summary>
        /// <param name="adapterDescription"></param>
        /// <returns></returns>
        public virtual MBoolResponse RegisterAdapter(MAdapterDescription adapterDescription)
        {
            //Check if the adapter description is already available
            if (RuntimeData.AdapterInstances.ContainsKey(adapterDescription.ID))
            {
                Console.WriteLine($"Adapter: {adapterDescription.Name} already available -> nothing to do.");
                return new MBoolResponse(true);
            }

            ////Add a new remote adapter 
            RemoteAdapter remoteAdapter = new RemoteAdapter(adapterDescription);
            remoteAdapter.Start();

            //Add the newly generated adapter description
            if (!RuntimeData.AdapterInstances.TryAdd(adapterDescription.ID, remoteAdapter))
            {
                Console.WriteLine($"Cannot add adapter description {adapterDescription.Name}");
            }

            //Fire event
            this.OnAdapterRegistered?.Invoke(this, remoteAdapter);

            return new MBoolResponse(true);
        }

        /// <summary>
        /// Method is utilized to register an external service
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <returns></returns>
        public MBoolResponse RegisterService(MServiceDescription serviceDescription)
        {
            //Check if the adapter description is already available
            if (RuntimeData.ServiceInstances.ContainsKey(serviceDescription.ID))
            {
                Console.WriteLine($"Service: {serviceDescription.Name} already available -> nothing to do.");
                return new MBoolResponse(true);
            }


            //Add a new remote service controller
            RemoteService remoteService = new RemoteService(serviceDescription);
            remoteService.Start();

            //Add the newly generated service description
            if (!RuntimeData.ServiceInstances.TryAdd(serviceDescription.ID, remoteService))
            {
                Console.WriteLine($"Cannot add service description {serviceDescription.Name}");
            }

            //Fire event
            this.OnServiceRegistered?.Invoke(this, remoteService);

            return new MBoolResponse(true);
        }

        /// <summary>
        /// Method is called if an adapter should be unregistered
        /// </summary>
        /// <param name="adapterDescription"></param>
        /// <returns></returns>
        public MBoolResponse UnregisterAdapter(MAdapterDescription adapterDescription)
        {
            if (!RuntimeData.AdapterInstances.ContainsKey(adapterDescription.ID))
            {
                //Nothing to do no adapter available
                return new MBoolResponse(true);
            }


            if (RuntimeData.AdapterInstances.ContainsKey(adapterDescription.ID))
            {
                RemoteAdapter adapter = null;

                RuntimeData.AdapterInstances.TryGetValue(adapterDescription.ID, out adapter);

                //Dispose the adapter
                adapter.Dispose();

                //Remove the adapter
                RuntimeData.AdapterInstances.TryRemove(adapterDescription.ID, out adapter);

                //Fire event
                this.OnAdapterUnregistered?.Invoke(this, adapter);

                return new MBoolResponse(true);
            }


            Console.WriteLine("No matching adapter to unregister found");
            return new MBoolResponse(false)
            {
                LogData = new List<string>() { "No matching adapter found" }
            };
        }


        /// <summary>
        /// Method is called if a service should be unregistered
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <returns></returns>
        public MBoolResponse UnregisterService(MServiceDescription serviceDescription)
        {
            if (!RuntimeData.ServiceInstances.ContainsKey(serviceDescription.ID))
            {
                //Nothing to do no adapter available
                return new MBoolResponse(true);
            }

            if (RuntimeData.ServiceInstances.ContainsKey(serviceDescription.ID))
            {
                RemoteService service = null;

                RuntimeData.ServiceInstances.TryGetValue(serviceDescription.ID, out service);

                //Dispose the adapter
                service.Dispose();

                //Remove the adapter
                RuntimeData.ServiceInstances.TryRemove(serviceDescription.ID, out service);

                //Fire event
                this.OnServiceUnregistered?.Invoke(this, service);

                //Return true
                return new MBoolResponse(true);
            }

            return new MBoolResponse(false)
            {
                LogData = new List<string>() { "No matching service found" }
            };
        }




    }
}
