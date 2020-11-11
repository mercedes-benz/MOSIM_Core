// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Clients;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMICSharp.Access.Abstraction
{
    /// <summary>
    /// Class for accessing adapters via network
    /// </summary>
    public class RemoteAdapterAccess:IAdapter
    {
        #region public properties

        public MAdapterDescription Description
        {
            get;
            set;
        }

        /// <summary>
        /// The assigned address
        /// </summary>
        public string Address
        {
            get;
            set;
        }

        /// <summary>
        /// The assigned port
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Flag which indicates whether the communciaton has been successfully initialized
        /// </summary>
        public bool Initialized
        {
            get;
            set;
        } = false;


        /// <summary>
        /// Flag which indicates whether the initialization has been aborted
        /// </summary>
        public bool Aborted
        {
            get;
            set;
        } = false;


        public bool SceneSynchronized
        {
            get;
            set;
        } = false;

        public bool Loaded
        {
            get;
            set;
        } = false;


        public List<MMUDescription> MMUDescriptions
        {
            get;
            set;
        } = new List<MMUDescription>();

        #endregion


        /// <summary>
        /// Class representing a remote adapter client
        /// </summary>
        private class RemoteAdapterClient : IAdapterClient
        {
            /// <summary>
            /// The spefic interface of the adapter
            /// </summary>
            public MMIAdapter.Iface Access
            {
                get;
                set;
            }

            /// <summary>
            /// The utilized thrift client
            /// </summary>
            private readonly AdapterClient thriftClient;

            /// <summary>
            /// Basic constructor
            /// </summary>
            /// <param name="address"></param>
            /// <param name="port"></param>
            public RemoteAdapterClient(string address, int port)
            {
                this.thriftClient = new AdapterClient(address, port);
                this.Access = thriftClient.Access;
            }

            /// <summary>
            /// Disposes the thrift client
            /// </summary>
            public void Dispose()
            {
                try
                {
                    this.thriftClient.Dispose();
                }
                catch (Exception)
                {
                }
            }

        }

        private AdapterClient thriftClient;

        /// <summary>
        /// Instance of the mmu access
        /// </summary>
        private readonly MMUAccess mmuAccess;


        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="mmuAccess"></param>
        public RemoteAdapterAccess(string address, int port, MAdapterDescription adapterDescription, MMUAccess mmuAccess)
        {
            this.Address = address;
            this.Port = port;
            this.mmuAccess = mmuAccess;
            this.Description = adapterDescription;
        }

        /// <summary>
        /// Method starts the process
        /// </summary>
        public void Start()
        {
            //Create a new adapter client
            this.thriftClient = new AdapterClient(this.Address, this.Port);
            
            //Try to connect to adapter until status available
            while (!this.Initialized && !this.Aborted)
            {
                System.Threading.Thread.Sleep(30);

                try
                {
                    Dictionary<string,string> status = this.GetStatus();
                    this.Initialized = true;
                }
                catch (Exception)
                {

                }
            }
        }


        /// <summary>
        /// Method creates a new client for the adapter
        /// </summary>
        /// <returns></returns>
        public IAdapterClient CreateClient()
        {
            return new RemoteAdapterClient(this.Address, this.Port);
        }

        /// <summary>
        /// Returns all mmus which are available at the assigned adapter and for the given session Id
        /// </summary>
        /// <returns></returns>
        public List<MotionModelUnitAccess> CreateMMUConnections(string sessionId, List<MMUDescription> mmuDescriptions)
        {
            //Get the available MMUs
            List<MMUDescription> availableMMUs = this.thriftClient.Access.GetMMus(sessionId);
            
            if (availableMMUs == null)
                throw new Exception("Tcp server not available");


            List<MotionModelUnitAccess> result = new List<MotionModelUnitAccess>();
            foreach (MMUDescription description in availableMMUs)
            {
                //Create a new MMMU connection instance
                result.Add(new MotionModelUnitAccess(this.mmuAccess, this,this.mmuAccess.SessionId, description));
            }

            return result;

        }


        /// <summary>
        /// Synchronizes the scene of the module
        /// </summary>
        /// <param name="sceneUpdates"></param>
        /// <returns></returns>
        public MBoolResponse PushScene(MSceneUpdate sceneUpdates, string sessionId)
        {
            return this.thriftClient.Access.PushScene(sceneUpdates, sessionId); 
        }


        /// <summary>
        /// Fetches the entire scene from the adapter
        /// </summary>
        /// <returns></returns>
        public List<MSceneObject> GetScene(string sessionId)
        {
            return this.thriftClient.Access.GetScene(sessionId);
        }


        /// <summary>
        ///Gets the scene events of the current frame
        ///To check
        /// </summary>
        /// <returns></returns>
        public MSceneUpdate GetSceneChanges(string sessionId)
        {
            return this.thriftClient.Access.GetSceneChanges(sessionId);
        }


        /// <summary>
        /// Dispose method which closes the used application
        /// </summary>
        public MBoolResponse Dispose()
        {
            //Call the dispose method of the adapter
            this.thriftClient.Access.Dispose();

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Create a new session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="referenceAvatar">The utilized reference avatar</param>
        public MBoolResponse CreateSession(string sessionId, MAvatarDescription referenceAvatar)
        {
            return this.thriftClient.Access.CreateSession(sessionId);
        }


        public MBoolResponse CloseSession(string sessionID)
        {
            return this.thriftClient.Access.CloseSession(sessionID);
        }


        /// <summary>
        /// Returns all loadable MMUs identified by the adapater
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public List<MMUDescription> GetLoadableMMUs(string sessionId)
        {
            this.MMUDescriptions = this.thriftClient.Access.GetLoadableMMUs();
            return this.MMUDescriptions;
        }


        /// <summary>
        /// Loads the mmus by id
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="sessionId"></param>
        public MBoolResponse LoadMMUs(List<string> ids, string sessionId)
        {
            Dictionary<string,string> response = this.thriftClient.Access.LoadMMUs(ids, sessionId);

            if (response.Count == ids.Count)
            {
                this.Loaded = true;

                return new MBoolResponse(true);
            }
            else
            {
                return new MBoolResponse(false);
            }
        }


        /// <summary>
        /// Returns the status of the adapter
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetStatus()
        {
            return this.thriftClient.Access.GetStatus();
        }


        /// <summary>
        /// Creates a new checkpoint with the specific id
        /// </summary>
        /// <param name="mmuList"></param>
        /// <param name="checkpointID"></param>
        public byte[] CreateCheckpoint(string mmuID, string checkpointID)
        {
            return this.thriftClient.Access.CreateCheckpoint(mmuID, this.mmuAccess.SessionId);
        }


        /// <summary>
        /// Restores a checkpoint
        /// </summary>
        /// <param name="mmuList"></param>
        /// <param name="checkpointID"></param>
        public MBoolResponse RestoreCheckpoint(string mmuId, string checkpointID, byte[] checkpointData)
        {
            return this.thriftClient.Access.RestoreCheckpoint(mmuId, this.mmuAccess.SessionId, checkpointData);
        }

        public MBoolResponse CloseConnection()
        {
            if (this.thriftClient != null)
            {
                //Close the client
                this.thriftClient.Dispose();
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Destructor which closes the thrift client
        /// </summary>
         ~RemoteAdapterAccess()
        {
            //Close the connection of not already done
            this.CloseConnection();

        }

    }
}
