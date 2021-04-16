// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMICSharp.Access.Abstraction;
using MMICSharp.Common;
using MMICSharp.Clients;

namespace MMICSharp.Access
{
    /// <summary>
    /// Class for accessing Motion Model Units through different programming languages
    /// </summary>
    public class MMUAccess:IDisposable
    {
        #region internal variables

     
        /// <summary>
        /// The unique session id
        /// </summary>
        internal string SessionId;

        /// <summary>
        /// List of all available adapters
        /// </summary>
        internal List<IAdapter> Adapters = new List<IAdapter>();

        #endregion


        /// <summary>
        /// Event is fired whenever the environment and the modules have been loaded
        /// </summary>
        public event EventHandler<EventArgs> OnLoaded;


        /// <summary>
        /// Event is fired whenever all MMUs have been initialized
        /// </summary>
        public event EventHandler<EventArgs> OnInitialized;



        #region public properties

        /// <summary>
        /// The descirption and specifications of the intermediate avatar
        /// </summary>
        public MSkeletonAccess.Iface SkeletonAccess { get; set; }
        public string AvatarID { get; set; }

        /// <summary>
        /// The list of available Motion Model Units
        /// </summary>
        public List<IMotionModelUnitAccess> MotionModelUnits
        {
            get;
            set;
        } = new List<IMotionModelUnitAccess>();

        /// <summary>
        /// Specifies whether the environment and modules have been loaded
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                if (this.Adapters.Count == 0)
                    return false;
                else
                    return this.Adapters.Where(s => !s.Initialized).Count() == 0;   
            }
        }


        /// <summary>
        /// Specifies whether the MMUs are initialized
        /// </summary>
        public bool IsInitialized
        {
            get;
            private set;
        }


        /// <summary>
        /// The assigned scene access -> has to be implemented by the target engine
        /// </summary>
        public MSceneAccess.Iface SceneAccess
        {
            get;
            set;
        }


        /// <summary>
        /// Access to the available services (readonly)
        /// </summary>
        public IServiceAccess ServiceAccess
        {
            get;
            private set;
        } 
            

        /// <summary>
        /// The descriptions of the available adapter
        /// </summary>
        public List<MAdapterDescription> AdapterDescriptions
        {
            get
            {
                return this.Adapters.Select(s => s.Description).ToList();
            }
        }


        /// <summary>
        /// All gathered MMU descriptions
        /// </summary>
        public List<MMUDescription> MMUDescriptions
        {
            get;
            private set;
        } = new List<MMUDescription>();


        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public MMUAccess()
        {
            //Create a unique session id
            this.SessionId = Guid.NewGuid().ToString(); 
        }

        /// <summary>
        /// Constructor to specify the session id
        /// </summary>
        /// <param name="sessionId"></param>
        public MMUAccess(string sessionId)
        {
            this.SessionId = sessionId;
        }


        #region connecting

        /// <summary>
        /// Connects to the given adapter. This method can be used for testing purposes or if the adapter adress is already known.
        /// </summary>
        /// <param name="adapterEndpoint">The desired adapter endpoint.</param>
        /// <param name="allowRemoteConnections">Specifies whether a remote connection should be established.</param>
        public bool Connect(AdapterEndpoint adapterEndpoint, string AvatarID, bool allowRemoteConnections = true)
        {
            //Create a list for the adapter descriptions
            List<MAdapterDescription> adapterDescriptions = new List<MAdapterDescription>();

            //Add the local adapter
            this.Adapters.Add(new LocalAdapterAccess(adapterEndpoint.Description, adapterEndpoint.Instance, this));
           
                                         
            //If remote connections are allowed
            if(allowRemoteConnections)
            {
                try
                {
                    //Get the adapter names
                    using (MMIRegisterServiceClient client = new MMIRegisterServiceClient(adapterEndpoint.MMIRegisterAddress.Address, adapterEndpoint.MMIRegisterAddress.Port))
                    {
                        client.Start();

                        //To do
                        adapterDescriptions = client.Access.GetRegisteredAdapters(SessionId);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot connect to mmi register " + e.Message);
                    return false;
                }

                foreach (MAdapterDescription description in adapterDescriptions)
                {
                    Console.WriteLine(description.Name);

                    //Skip if adapter is already available
                    if (this.Adapters.Exists(s => s.Description.Name == description.Name && description.Addresses[0] == description.Addresses[0] && s.Description.Addresses[0].Port == description.Addresses[0].Port))
                        continue;

                    Console.WriteLine("Add new remote adapter: " + description.Name);

                    //Add the adapter
                    this.Adapters.Add(new RemoteAdapterAccess(description.Addresses[0].Address, description.Addresses[0].Port,description, this));
                }
            }


            //Create the service access
            this.ServiceAccess = new ServiceAccess(adapterEndpoint.MMIRegisterAddress, SessionId);

            
            //Start all adapters
            foreach (IAdapter adapter in this.Adapters)
            {
                //Start the adapter
                adapter.Start();

                //Create the session
                adapter.CreateSession(this.SessionId, this.SkeletonAccess.GetAvatarDescription(AvatarID));
            }

            return true;
        }


        /// <summary>
        /// Connects to the central MMIRegister (launcher) and furthermore queries all adapters and creates a session.
        /// </summary>
        /// <param name="mmiRegisterAddress">The address of the central MMIRegister (launcher)</param>
        /// <param name="timeout">The timout</param>
        /// <param name="AvatarID">The id of the avatar that is used.</param>
        public bool Connect(MIPAddress mmiRegisterAddress, TimeSpan timeout, string AvatarID)
        {
            //Create a list representing the adapter descriptions
            List<MAdapterDescription> adapterDescriptions = new List<MAdapterDescription>();

            //Get all registered adapters -> Execute this task in background with the imposed timeout
            bool adapterDescriptionReceived = Threading.ExecuteTask((CancellationTokenSource cls) =>
            {
                //The actual function which is executed in the background
                while (!cls.IsCancellationRequested)
                {
                    try
                    {
                        //Get all adapter descriptions and create a client for this purpose
                        using (MMIRegisterServiceClient client = new MMIRegisterServiceClient(mmiRegisterAddress.Address, mmiRegisterAddress.Port))
                        {
                            adapterDescriptions = client.Access.GetRegisteredAdapters(SessionId);
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(100);
                    }
                }

            }, timeout);

            //Directly return if no adapter description was received
            if (!adapterDescriptionReceived)
                return false;

            //Create the service access
            this.ServiceAccess = new ServiceAccess(mmiRegisterAddress, SessionId);

            //Fetch all adapter descriptions and create a new adapter instance
            foreach (MAdapterDescription description in adapterDescriptions)
            {
                //Skip if adapter is already available
                if (this.Adapters.Exists(s => s.Description.Name == description.Name && description.Addresses[0] == description.Addresses[0] && s.Description.Addresses[0].Port == description.Addresses[0].Port))
                    continue;

                //Add a new remote adapter instance to the list
                this.Adapters.Add(new RemoteAdapterAccess(description.Addresses[0].Address, description.Addresses[0].Port, description, this));
            }


            ///Dictionary which contains the connected adapters
            ConcurrentDictionary<IAdapter, bool> connectedAdapters = new ConcurrentDictionary<IAdapter, bool>();


            //Create the sessions for each adapter in parallel
            bool success = Threading.ExecuteTasksParallel(this.Adapters, (IAdapter adapter, CancellationTokenSource cls) =>
            {
                //Start the adapter
                adapter.Start();

                //Create the session
                adapter.CreateSession(this.SessionId, this.SkeletonAccess.GetAvatarDescription(AvatarID));

                //Add flag if finished
                connectedAdapters.TryAdd(adapter, true);

            }, timeout);

            //Remove all adapters not being connected (therfore iterate over all added adapters)
            for (int i = this.Adapters.Count - 1; i >= 0; i--)
            {
                //Remove the adapter if not connected
                if (!connectedAdapters.ContainsKey(this.Adapters[i]))
                {
                    //Close the connection
                    this.Adapters[i].CloseConnection();
                    this.Adapters.Remove(this.Adapters[i]);
                }
            }

            //Return true if at least one adapter is connected
            return this.Adapters.Count >0;
        }


        /// <summary>
        /// Connects to the given adapters
        /// </summary>
        /// <param name="mmiRegisterAddress">The address of the central MMIRegister (launcher)</param>
        /// <param name="timeout">The timout in milliseconds</param>
        public async Task<bool> ConnectAsync(MIPAddress mmiRegisterAddress, TimeSpan timeout, Action<bool> callback, string AvatarID)
        {
            return await Task.Factory.StartNew(() =>
            {
                bool success = this.Connect(mmiRegisterAddress, timeout, AvatarID);

                callback?.Invoke(success);

                return success;
            }); ;
        }


        /// <summary>
        /// Connects directly to the specified adapters
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal bool Connect(List<Tuple<string,int>> connections, TimeSpan timeout, string AvatarID)
        {
            //Fetch all modules and start them
            foreach (Tuple<string, int> connection in connections)
            {
                //Skip if adapter is already available
                if (this.Adapters.Exists(s => s.Description.Addresses[0].Address == connection.Item1 && s.Description.Addresses[0].Port == connection.Item2))
                    continue;

                RemoteAdapterAccess module = new RemoteAdapterAccess(connection.Item1, connection.Item2, new MAdapterDescription("Manual connection", Guid.NewGuid().ToString(), "Remote", new List<MIPAddress>() { new MIPAddress(connection.Item1, connection.Item2) }), this);

                this.Adapters.Add(module);
            }


            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool finished = false;

            ThreadPool.QueueUserWorkItem(delegate
            {
                //Start all modules
                foreach (IAdapter accessingModule in this.Adapters)
                {
                    //To do investigate mutlithreading support
                    //Start the module
                    accessingModule.Start();

                    //Create the session
                    accessingModule.CreateSession(this.SessionId, this.SkeletonAccess.GetAvatarDescription(AvatarID));
                }

                finished = true;
            });

            //Wait until timeout or finished
            while (!finished && watch.Elapsed < timeout)
            {
                Thread.Sleep(10);
            }

            return finished;
        }

        #endregion


        /// <summary>
        /// Adds a new adapter to the access.
        /// </summary>
        /// <param name="adapter"></param>
        public void AddAdapter(MAdapterDescription description, MMIAdapter.Iface adapter)
        {
            IAdapter newAdapter = new LocalAdapterAccess(description, adapter, this);

            //Check if the adapter is already available as a remote one 
            if (this.Adapters.Exists(s=>s.Description.Name == description.Name && description.Addresses[0] == description.Addresses[0] && s.Description.Addresses[0].Port == description.Addresses[0].Port))
            {
                //Find the old adapter
                IAdapter oldAdapter = this.Adapters.Find(s => s.Description.Name == description.Name && description.Addresses[0] == description.Addresses[0] && s.Description.Addresses[0].Port == description.Addresses[0].Port);

                //Find MMUs which utilize the old adapter
                List<Abstraction.MotionModelUnitAccess> mmusWithOldAdapter = this.MotionModelUnits.Where(s => ((Abstraction.MotionModelUnitAccess)s).Adapter == oldAdapter).Select(s => ((Abstraction.MotionModelUnitAccess)s)).ToList();

                //Change the adapter to the new one
                foreach(Abstraction.MotionModelUnitAccess mmu in mmusWithOldAdapter)
                {
                    mmu.ChangeAdapter(newAdapter);
                }

                //Disposes the adapter
                oldAdapter.Dispose();

                //Remove the old adapter
                this.Adapters.Remove(oldAdapter);
            }

            //Add the new adapter
            this.Adapters.Add(newAdapter);
        }


        /// <summary>
        /// Returns all loadable MMUs in form of their description
        /// </summary>
        /// <returns></returns>
        public List<MMUDescription> GetLoadableMMUs()
        {
            //Create a new empty list
            List<MMUDescription> loadableMMUs = new List<MMUDescription>();

            //Get the loadable MMUs of the adapters
            foreach (IAdapter adapter in this.Adapters)
            {
                //Call the function of the respective adapter
                List<MMUDescription> mmuDescriptions = adapter.GetLoadableMMUs(this.SessionId);

                //Add to list
                loadableMMUs.AddRange(mmuDescriptions);
                this.MMUDescriptions.AddRange(mmuDescriptions);          
            }
            
            //Returns all MMU descriptions which are available
            return loadableMMUs;
        }


        /// <summary>
        /// Loads the MMUs asynchronously
        /// </summary>
        /// <param name="mmuDescriptions"></param>
        /// <param name="timeout"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task<bool> LoadMMUsAsync(List<MMUDescription> mmuDescriptions, TimeSpan timeout, Action<bool> callback)
        {
            return await Task.Factory.StartNew(() =>
            {
                bool success = this.LoadMMUs(mmuDescriptions, timeout);

                callback?.Invoke(success);

                return success;
            }); 
        }


        /// <summary>
        /// Loads the defined MMUs in an async manner
        /// </summary>
        /// <param name="list"></param>
        /// <param name="timeout"></param>
        /// <param name="async">Defines whether the operation should be executed asynchonously</param>
        public bool LoadMMUs(List<MMUDescription> list, TimeSpan timeout)
        {
            //Create a concurrent dictionary for storing the createad MMUAccesses
            ConcurrentDictionary<IAdapter, List<MotionModelUnitAccess>> adapterMMUAccesses = new ConcurrentDictionary<IAdapter, List<MotionModelUnitAccess>>();
            List<Task> tasks = new List<Task>();

            //Execute the loading using different tasks
            bool allFinished = Threading.ExecuteTasksParallel(this.Adapters, (IAdapter adapter, CancellationTokenSource cts) =>
            {
                    //Load the mmus based on their id
                    adapter.LoadMMUs(list.Select(s => s.ID).ToList(), this.SessionId);

                    //Add the created connections to the concurrent dictionary
                    adapterMMUAccesses.TryAdd(adapter, adapter.CreateMMUConnections(this.SessionId, this.MMUDescriptions));

            }, timeout);

            //Check if all tasks have been finished
            if (allFinished)
            {
                //Add the MMU connections to the global list
                foreach (List<MotionModelUnitAccess> mmuAccesses in adapterMMUAccesses.Values)
                {
                    this.MotionModelUnits.AddRange(mmuAccesses);
                }

                //raise event
                this.OnLoaded?.Invoke(this, new EventArgs());

                return true;
            }

            else
            {
                //throw new Exception("Timeout during establishing connections to MMUs");

                return false;
            }
        }



        /// <summary>
        /// Initializes the mmus asynchronously
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="callback"></param>
        /// <param name="initializationProperties"></param>
        public async Task<bool> InitializeMMUsAsync(TimeSpan timeout, Action<bool> callback, string AvatarID, Dictionary<string, string> initializationProperties = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                bool success = this.InitializeMMUs(timeout, AvatarID, initializationProperties);

                callback?.Invoke(success);

                return success;
            });
        }


        /// <summary>
        /// Initializes the mmus
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="initializationProperties"></param>
        public bool InitializeMMUs(TimeSpan timeout, string AvatarID, Dictionary<string, string> initializationProperties = null)
        {
            //Directly return if not loaded
            if (!this.IsLoaded)
                return false;

            if (initializationProperties == null)
                initializationProperties = new Dictionary<string, string>();


            //Initialize all mmus based on the specified timeout
            bool success = Threading.ExecuteTasksParallel(this.MotionModelUnits, (IMotionModelUnitAccess mmu, CancellationTokenSource tcs) =>
            {
                mmu.Initialize(this.SkeletonAccess.GetAvatarDescription(AvatarID), initializationProperties);
            }, timeout);


            //Raise the event and set flag if successful
            if (success)
            {
                //Set initialized to true
                this.IsInitialized = true;
                this.OnInitialized?.Invoke(this, new EventArgs());
            }

            return success;
        }



        /// <summary>
        /// Synchronizes the scene
        /// </summary>
        /// <param name="transmitFullScene">Specified whether the full scene should be transferred</param>
        public void PushScene(bool transmitFullScene = false)
        {
            //Get the events 
            MSceneUpdate sceneUpdates = this.SceneAccess.GetSceneChanges();

            if (transmitFullScene)
                sceneUpdates = this.SceneAccess.GetFullScene();

 

            int serversToSynchronize = this.Adapters.Count;
            int adapterCount = this.Adapters.Count;

            //Perform every synchronization in parallel
            Parallel.For(0, serversToSynchronize, delegate (int index)
            {
                //Get the corresponding adapter
                IAdapter adapter = this.Adapters[index];

                //Set synchronized flag to false
                adapter.SceneSynchronized = false;
                adapter.PushScene(sceneUpdates, this.SessionId);
                adapter.SceneSynchronized = true;
            });

        }



        /// <summary>
        /// Synchronizes the scene
        /// </summary>
        /// <param name="transmitFullScene">Specified whether the full scene should be transferred</param>
        public void PushSceneUpdate(MSceneUpdate sceneUpdates)
        {

            int serversToSynchronize = this.Adapters.Count;
            int adapterCount = this.Adapters.Count;

            //Perform every synchronization in parallel
            Parallel.For(0, serversToSynchronize, delegate (int index)
            {
                //Get the corresponding adapter
                IAdapter adapter = this.Adapters[index];

                //Set synchronized flag to false
                adapter.SceneSynchronized = false;
                adapter.PushScene(sceneUpdates, this.SessionId);
                adapter.SceneSynchronized = true;
            });
            
        }




        /// <summary>
        /// Creates a new checkpoint for all MMUs specified
        /// </summary>
        /// <param name="mmuList"></param>
        /// <param name="checkpointID"></param>
        public Dictionary<string,byte[]> CreateCheckpoint(List<string> mmuIDs)
        {
            Dictionary<string, byte[]> checkpointData = new Dictionary<string, byte[]>();

            foreach(MotionModelUnitAccess mmu in this.MotionModelUnits)
            {
                if(mmuIDs.Contains(mmu.ID))
                    checkpointData.Add(mmu.ID, mmu.CreateCheckpoint());
            }

            return checkpointData;
        }


        /// <summary>
        /// Restores a checkpoint
        /// </summary>
        /// <param name="mmuList"></param>
        /// <param name="checkpointID"></param>
        public void RestoreCheckpoint(Dictionary<string,byte[]> checkpointData)
        {
            foreach (Abstraction.MotionModelUnitAccess mmu in this.MotionModelUnits)
            {
                if (checkpointData.ContainsKey(mmu.ID))
                    mmu.RestoreCheckpoint(checkpointData[mmu.ID]);
            }
        }


        /// <summary>
        /// Fetches the scene information from the adapter
        /// </summary>
        public List<MSceneObject> FetchScene()
        {
            List<MSceneObject> sceneObjects = new List<MSceneObject>();

            if(this.Adapters.Count > 0)
            {
                sceneObjects = this.Adapters[0].GetScene(this.SessionId);
            }

            return sceneObjects;
        }


        /// <summary>
        /// Fetches the scene information from the adapter
        /// </summary>
        public MSceneUpdate FetchSceneChanges()
        {
            MSceneUpdate sceneUpdate = new MSceneUpdate();

            if (this.Adapters.Count > 0)
            {
                sceneUpdate = this.Adapters[0].GetSceneChanges(this.SessionId);
            }

            return sceneUpdate;
        }


        /// <summary>
        /// Closes the access
        /// </summary>
        public void Dispose()
        {
            //Dispose every adapter
            foreach(IAdapter adapter in this.Adapters)
            {
                //Close the connection
                adapter.CloseSession(this.SessionId);

                //Dispose the adapter
                adapter.Dispose();

                //Finaly close the connection
                adapter.CloseConnection();            
            }

            //Dispose every module
            foreach (MotionModelUnitAccess mmu in this.MotionModelUnits)
            {
                //Dispose the respective mmu
                mmu.Dispose(new Dictionary<string, string>());

                //Close the connection
                mmu.CloseConnection();
            }
        }

    }
}
