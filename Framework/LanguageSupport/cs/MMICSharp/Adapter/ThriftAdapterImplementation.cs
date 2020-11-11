// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;
using MMIStandard;
using System.Linq;
using CSharpAdapter;
using MMICSharp.Common;
using System.Diagnostics;

namespace MMICSharp.Adapter
{
    /// <summary>
    /// Implementation of the thrift adapter functionality
    /// </summary>
    [Serializable]
    public class ThriftAdapterImplementation : MMIAdapter.Iface
    {

        /// <summary>
        /// The assigned mmuInstantiator
        /// </summary>
        private readonly IMMUInstantiation mmuInstantiator;

        /// <summary>
        /// Variable for storing the skeleton access
        /// </summary>
        private MSkeletonAccess.Iface skeletonAccess;


        /// <summary>
        /// The assigned session data
        /// </summary>
        private readonly SessionData SessionData;


        /// <summary>
        /// Basic constructor
        /// </summary>
        public ThriftAdapterImplementation(SessionData sessionData, IMMUInstantiation mmuInstantiator)
        {
            this.SessionData = sessionData;
            this.mmuInstantiator = mmuInstantiator;
        }

        #region Adapter specific features

        /// <summary>
        /// Working
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, string> GetStatus()
        {
            Dictionary<string, string> status = new Dictionary<string, string>();

            try
            {
                status = new Dictionary<string, string>
                {
                    { "Running since", SessionData.StartTime.ToLongDateString() + " " + SessionData.StartTime.ToLongTimeString() },
                    { "Total Sessions", SessionData.SessionContents.Count.ToString() },
                    { "Loadable MMUs", SessionData.MMUDescriptions.Count.ToString() }
                };


                try
                {
                    status.Add("Version", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());

                }
                catch (Exception)
                {
                
                }
                if (SessionData.LastAccess == DateTime.MinValue)
                    status.Add("Last Access", "None");

                else
                    status.Add("Last Access", SessionData.LastAccess.ToLongDateString() + " " + SessionData.LastAccess.ToLongTimeString());
            }
            catch (Exception e)
            {
                status.Add("Exception", e.Message);
            }

            return status;
        }


        /// <summary>
        /// Returns all available MMUs
        /// </summary>
        /// <returns></returns>
        public virtual List<MMUDescription> GetLoadableMMUs()
        {
            return SessionData.MMUDescriptions;
        }


        /// <summary>
        /// Returns the scene contained within the adapter
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public virtual List<MSceneObject> GetScene(string sessionID)
        {
            SessionContent sessionContent = null;

            MBoolResponse sessionResult = SessionData.GetSessionContent(sessionID, out sessionContent);

            if (sessionResult.Successful)
            {
                //Set the last access time
                sessionContent.UpdateLastAccessTime();

                Logger.Log(Log_level.L_INFO, "Transfer all scene objects");

                return sessionContent.SceneBuffer.GetSceneObjects();
            }

            else
            {
                Debug.Fail(sessionResult.LogData.ToString());
            }

            return new List<MSceneObject>();
        }


        /// <summary>
        /// Returns the deltas of the last frame
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public virtual MSceneUpdate GetSceneChanges(string sessionID)
        {
            SessionContent sessionContent = null;

            MBoolResponse sessionResult = SessionData.GetSessionContent(sessionID, out sessionContent);

            if (sessionResult.Successful)
            {
                //Set the last access time
                sessionContent.UpdateLastAccessTime();

                return sessionContent.SceneBuffer.GetSceneChanges();
            }

            else
            {
                Logger.Log(Log_level.L_ERROR, sessionResult.LogData.ToString());
            }

            return new MSceneUpdate();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mmus"></param>
        /// <param name="sessionID"></param>
        /// <returns>A mapping from MMUID to a specific instance id</returns>
        public virtual Dictionary<string,string> LoadMMUs(List<string> mmus, string sessionID)
        {
            SessionContent sessionContent = null;
            SessionID idContainer = new SessionID(sessionID);

            //Get the session content for the id
            MBoolResponse sessionResult = SessionData.GetSessionContent(sessionID, out sessionContent);

            //Skip if invalid session result
            if (!sessionResult.Successful)
            {
                Logger.Log(Log_level.L_ERROR, "Cannot generate session content");
                return new Dictionary<string, string>();
            }

            //Set the last access time
            sessionContent.UpdateLastAccessTime();

            Dictionary<string, string> mmuInstanceMapping = new Dictionary<string, string>();

            //Iterate over each desired MMU
            foreach (string mmuID in mmus)
            {
                MMULoadingProperty mmuLoadingProperty = null;

                //Skip MMU is not contained in adapter
                if (!SessionData.MMULoadingProperties.TryGetValue(mmuID, out mmuLoadingProperty))
                    continue;

                IMotionModelUnitDev mmu = null;

                //Instantiate MMU 
                try
                {
                    mmu = this.mmuInstantiator.InstantiateMMU(mmuLoadingProperty);
                }
                catch (Exception e)
                {
                    Logger.Log(Log_level.L_ERROR, $"Problem at loading MMU {mmuLoadingProperty.Description.Name}, Exception: {e.Message}, {e.StackTrace}");

                    return new Dictionary<string, string>();
                }

                //Assign the service access
                mmu.ServiceAccess = sessionContent.ServiceAccess;

                //Assign the scene
                mmu.SceneAccess = sessionContent.SceneBuffer;

                //Assign a new instance of the skeleton access
                mmu.SkeletonAccess = this.skeletonAccess;//new SkeletonAccess();

                //Set the instance as the adapter
                mmu.AdapterEndpoint = new AdapterEndpoint()
                {
                    Instance = SessionData.AdapterInstance,
                    Description = SessionData.AdapterDescription,
                    MMIRegisterAddress = SessionData.MMIRegisterAddress
                };


                Logger.Log(Log_level.L_INFO, $"Loaded MMU: {mmuLoadingProperty.Description.Name} for session: {sessionID}");

                //Add to the specific avatar content
                AvatarContent avatarContent = null;

                if (!sessionContent.AvatarContent.TryGetValue(idContainer.AvatarID, out avatarContent))
                {
                    avatarContent = new AvatarContent(idContainer.AvatarID);

                    sessionContent.AvatarContent.TryAdd(idContainer.AvatarID, avatarContent);
                }

                //Add the mmu
                avatarContent.MMUs.Add(mmuLoadingProperty.Description.ID, mmu);

                //To do -> create a unique instance ID
                mmuInstanceMapping.Add(mmuLoadingProperty.Description.ID, "tbd");
            }

            return mmuInstanceMapping;
        }


        /// <summary>
        /// Should work
        /// </summary>
        /// <param name="sceneManipulations"></param>
        /// <param name="sessionID"></param>
        public virtual MBoolResponse PushScene(MSceneUpdate sceneUpdates, string sessionID)
        {
            SessionContent sessionContent = null;

            //Get the session content for the id
            MBoolResponse sessionResult = SessionData.GetSessionContent(sessionID, out sessionContent);

            //Skip if invalid session result
            if (!sessionResult.Successful)
            {
                Debug.Fail(sessionResult.LogData.ToString());
                return sessionResult;
            }


            //Set the last access time
            sessionContent.UpdateLastAccessTime();

            //Synchronize the respective scene
            return sessionContent.SceneBuffer.Apply(sceneUpdates);

        }


        /// <summary>
        /// Creates a new session for the specified id
        /// </summary>
        /// <param name="sessionID"></param>
        public virtual MBoolResponse CreateSession(string sessionID)
        {
            //Skip if the sessionID is invalid
            if (sessionID == null || sessionID.Count() == 0)
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Session ID invalid" }
                };

            //Skip if sessionID already available
            if (SessionData.SessionIDAvailable(sessionID))
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Session ID already available" }
                };

            //Create a new session content using the specified id
            SessionContent sessionContent = SessionData.CreateSessionContent(sessionID);

            //Set the last access time
            sessionContent.UpdateLastAccessTime();

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Clsoes the session
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public virtual MBoolResponse CloseSession(string sessionID)
        {
            Logger.Log(Log_level.L_INFO, $"Closing the session: {sessionID}");
            return SessionData.RemoveSessionContent(sessionID);
        }



        #endregion

        #region MMU functionality

        /// <summary>
        /// Basic initialization of a MMMU
        /// </summary>
        /// <param name="mmuID"></param>
        /// <param name="sessionID"></param>
        public virtual MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties, string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);
            this.skeletonAccess = new IntermediateSkeleton();
            this.skeletonAccess.InitializeAnthropometry(avatarDescription);

            //Skip if invalid session result
            if (!sessionResult.Successful)
                return sessionResult;

            try
            {
                //Update the access time
                sessionContent.UpdateLastAccessTime();

                //Get the corresponding MMU
                IMotionModelUnitDev mmu = avatarContent.MMUs[mmuID];

                Logger.Log(Log_level.L_INFO, "MMU initialized: " + mmu.Name + " " + sessionID);

                //Call the respective MMU
                return avatarContent.MMUs[mmuID].Initialize(avatarDescription, properties);
            }
            catch (Exception e)
            {
                Logger.Log(Log_level.L_ERROR, $"Problem at initializing MMU: {mmuID}, message: {e.Message}");

                return new MBoolResponse(false)
                {
                    LogData = new List<string>()
                     {
                         e.Message,
                         e.StackTrace,
                         e.InnerException.ToString(),
                         e.StackTrace
                     }
                };

            }
        }


        /// <summary>
        /// Execute command of a MMU
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <param name="hierarchy"></param>
        /// <param name="mmuID"></param>
        /// <param name="sessionID"></param>
        public virtual MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState, string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            //Directly return if not successfull
            if (!sessionResult.Successful)
                return sessionResult;

            sessionContent.UpdateLastAccessTime();


            Logger.Log(Log_level.L_DEBUG, $"Execute instruction {instruction.Name}, {mmuID}");


            //Directly assign the instruction
            return avatarContent.MMUs[mmuID].AssignInstruction(instruction, simulationState);        
        }


        /// <summary>
        /// Basic do step routine which triggers the simulation update of the repsective MMU
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <param name="mmuID"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public virtual MSimulationResult DoStep(double time, MSimulationState simulationState, string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            //Skip if invalid session result
            if (!sessionResult.Successful)
                return null;

            sessionContent.UpdateLastAccessTime();

            //Execute the do step of the respective MMU
            return avatarContent.MMUs[mmuID].DoStep(time, simulationState);
        }


        public virtual MBoolResponse CheckPrerequisites(MInstruction instruction, string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return sessionResult;


            sessionContent.UpdateLastAccessTime();

            //Execute the method of the MMU
            return avatarContent.MMUs[mmuID].CheckPrerequisites(instruction);
        }


        public virtual MBoolResponse Abort(string instructionId, string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return sessionResult;

            sessionContent.UpdateLastAccessTime();

            //Abort the respective MMU
            return avatarContent.MMUs[mmuID].Abort(instructionId);
        }


        public virtual MMUDescription GetDescription(string mmuID, string sessionID)
        {
            return SessionData.MMUDescriptions.Find(s => s.Name == mmuID);
        }


        public virtual List<MConstraint> GetBoundaryConstraints(MInstruction instruction, string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return new List<MConstraint>();

            sessionContent.UpdateLastAccessTime();

            return avatarContent.MMUs[mmuID].GetBoundaryConstraints(instruction);
        }

        public virtual MBoolResponse Dispose(string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return sessionResult;

            sessionContent.UpdateLastAccessTime();

            //Call the dispose method of the respective MMU
            return avatarContent.MMUs[mmuID].Dispose(new Dictionary<string, string>());
        }

        public virtual List<MMUDescription> GetMMus(string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return new List<MMUDescription>();

            sessionContent.UpdateLastAccessTime();

            return SessionData.MMUDescriptions.Where(s => avatarContent.MMUs.Keys.Contains(s.ID)).ToList();
        }


        /// <summary>
        /// Creates a checkpoint for all specified MMUs.
        /// The checkpoint contains the internal state of each MMU whoch can be later used to restore the state.
        /// </summary>
        /// <param name="mmuIDs"></param>
        /// <param name="sessionID"></param>
        /// <param name="checkpointID"></param>
        public virtual byte[] CreateCheckpoint(string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return null;

            sessionContent.UpdateLastAccessTime();

            //Add method to interface
            byte[] checkpointData = avatarContent.MMUs[mmuID].CreateCheckpoint();

            Logger.Log(Log_level.L_INFO, $"Checkpoint of {mmuID} sucessfully created ({checkpointData.Length} bytes)");

            return checkpointData;
        }

        public virtual MBoolResponse RestoreCheckpoint(string mmuID, string sessionID, byte[] checkpointData)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return sessionResult;

            sessionContent.UpdateLastAccessTime();

            Logger.Log(Log_level.L_INFO, $"Restore checkpoint of {mmuID}");


            return avatarContent.MMUs[mmuID].RestoreCheckpoint(checkpointData);
        }

        public Dictionary<string, string> ExecuteFunction(string name, Dictionary<string, string> parameters, string mmuID, string sessionID)
        {
            SessionContent sessionContent = null;
            AvatarContent avatarContent = null;

            MBoolResponse sessionResult = SessionData.GetContents(sessionID, out sessionContent, out avatarContent);

            if (!sessionResult.Successful)
                return null;

            sessionContent.UpdateLastAccessTime();


            Logger.Log(Log_level.L_DEBUG, $"Ecexute function {name} of {mmuID}");

            return avatarContent.MMUs[mmuID].ExecuteFunction(name, parameters);

        }

        /// <summary>
        /// Returns the assigned adapter description
        /// </summary>
        /// <returns></returns>
        public virtual MAdapterDescription GetAdapterDescription()
        {
            return SessionData.AdapterDescription;
        }








        #endregion
    }
}
