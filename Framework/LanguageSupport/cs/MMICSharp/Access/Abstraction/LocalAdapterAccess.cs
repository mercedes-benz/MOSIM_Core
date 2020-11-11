// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;
using MMIStandard;


namespace MMICSharp.Access.Abstraction
{
    /// <summary>
    /// An adapter access which can directly to connect to a local CSharp adapter
    /// </summary>
    public class LocalAdapterAccess : IAdapter
    {
        #region public properties
        public MAdapterDescription Description
        {
            get;
            set;
        }

        public bool Initialized
        {
            get;
            set;
        }

        public bool SceneSynchronized
        {
            get;
            set;
        }

        public List<MMUDescription> MMUDescriptions
        {
            get;
            set;
        } = new List<MMUDescription>();


        public bool Loaded
        {
            get;
            set;
        } = false;

        #endregion


        private class LocalAdapterClient : IAdapterClient
        {
            public MMIAdapter.Iface Access
            {
                get;
                set;
            }

            public LocalAdapterClient(MMIAdapter.Iface instance)
            {
                this.Access = instance;
            }

            public void Dispose()
            {
                //Nothing to do
            }
        }

        private readonly MMIAdapter.Iface instance;
        private readonly MMUAccess mmuAccess;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="description"></param>
        /// <param name="instance"></param>
        public LocalAdapterAccess(MAdapterDescription description, MMIAdapter.Iface instance, MMUAccess mmuAccess)
        {
            this.Description = description;
            this.instance = instance;
            this.mmuAccess = mmuAccess;
        }


        public void Start()
        {
            this.Initialized = true;
        }

        public IAdapterClient CreateClient()
        {
            return new LocalAdapterClient(this.instance);
        }

        public byte[] CreateCheckpoint(string mmuID, string checkpointID)
        {
            return instance.CreateCheckpoint(mmuID, mmuAccess.SessionId);
        }

        public List<MotionModelUnitAccess> CreateMMUConnections(string sessionId, List<MMUDescription> mmuDescriptions)
        {
            List<MMUDescription> availableMMUs = this.instance.GetMMus(sessionId);

            if (availableMMUs == null)
                throw new Exception("No MMUs available");



            List<MotionModelUnitAccess> result = new List<MotionModelUnitAccess>();
            foreach (MMUDescription description in availableMMUs)
            {
                //Create a new MMMU connection instance
                result.Add(new MotionModelUnitAccess(this.mmuAccess, this, this.mmuAccess.SessionId, description));
            }

            return result;
        }

        public MBoolResponse CreateSession(string sessionId, MAvatarDescription referenceAvatar)
        {
            return this.instance.CreateSession(sessionId);
        }

        public MBoolResponse Dispose()
        {
            throw new NotImplementedException();
        }

        public List<MMUDescription> GetLoadableMMUs(string sessionId)
        {
            this.MMUDescriptions = this.instance.GetLoadableMMUs();
            return this.MMUDescriptions;
        }

        public List<MSceneObject> GetScene(string sessionId)
        {
            return this.instance.GetScene(sessionId);
        }

        public MSceneUpdate GetSceneChanges(string sessionId)
        {
            return this.instance.GetSceneChanges(sessionId);
        }

        public Dictionary<string, string> GetStatus()
        {
            return this.instance.GetStatus();
        }

        public MBoolResponse LoadMMUs(List<string> ids, string sessionId)
        {
            this.instance.LoadMMUs(ids, sessionId);

            this.Loaded = true;

            return new MBoolResponse(true);
        }

        public MBoolResponse RestoreCheckpoint(string mmuID, string checkpointID,byte[] checkpointData)
        {
            return this.instance.RestoreCheckpoint(mmuID, this.mmuAccess.SessionId, checkpointData);
        }



        public MBoolResponse PushScene(MSceneUpdate sceneUpdates, string sessionId)
        {
            return this.instance.PushScene(sceneUpdates, sessionId);
        }

        public MBoolResponse CloseSession(string sessionID)
        {
            return this.instance.CloseSession(sessionID);
        }

        public MBoolResponse CloseConnection()
        {
            //Nothing to do -> since local access

            return new MBoolResponse(true);
        }
    }
}
