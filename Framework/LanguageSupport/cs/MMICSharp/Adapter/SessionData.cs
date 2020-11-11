// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using CSharpAdapter;
using MMIStandard;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace MMICSharp.Adapter
{

    /// <summary>
    /// Class which contains the data of the sessions and MMUs
    /// </summary>
    public class SessionData
    {
        #region properties and variables

        /// <summary>
        /// The adapter description
        /// </summary>
        public MAdapterDescription AdapterDescription;

        /// <summary>
        /// The address of the MMIRegister
        /// </summary>
        public MIPAddress MMIRegisterAddress;


        /// <summary>
        /// The last access time
        /// </summary>
        public DateTime LastAccess = DateTime.MinValue;

        /// <summary>
        /// The start time of the session data
        /// </summary>
        public DateTime StartTime;


        /// <summary>
        /// The adapter itself
        /// </summary>
        public MMIAdapter.Iface AdapterInstance; 


        /// <summary>
        /// Dictionary which contains all sessions
        /// </summary>
        public ConcurrentDictionary<string, SessionContent> SessionContents = new ConcurrentDictionary<string, SessionContent>();


        /// <summary>
        /// The properties to load a specific MMU
        /// </summary>
        public Dictionary<string, MMULoadingProperty> MMULoadingProperties = new Dictionary<string, MMULoadingProperty>();





        /// <summary>
        /// The description files of the MMUs
        /// </summary>
        public List<MMUDescription> MMUDescriptions
        {
            get;
            set;
        } = new List<MMUDescription>();


        #endregion


        /// <summary>
        /// Returns the session content given the current session id
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public SessionContent CreateSessionContent(string sessionId)
        {
            if (sessionId == null)
            {
                Logger.Log(Log_level.L_ERROR, "Warning: Session id is null!");
                return null;
            }

            //Get the ids
            string sceneId = null;
            string avatarId = null;
            SessionID.GetSplittedIDs(sessionId, out sceneId, out avatarId);

            //Create a new session content
            SessionContent sessionContent = null;


            //Session already avilable
            if (SessionContents.TryGetValue(sceneId, out sessionContent))
            {
                //Check avatar content -> If not available add it
                if (!sessionContent.AvatarContent.ContainsKey(avatarId))
                {
                    AvatarContent avatarContent = new AvatarContent(avatarId);
                    sessionContent.AvatarContent.TryAdd(avatarId, avatarContent);
                }
            }

            //Session not available -> Create new 
            else
            {
                Logger.Log(Log_level.L_INFO, $"Create new session id: {sessionId})");

                //Create new session content
                sessionContent = new SessionContent(this,sceneId);


                AvatarContent avatarContent = new AvatarContent(avatarId);

                sessionContent.AvatarContent.TryAdd(avatarId, avatarContent);
                SessionContents.TryAdd(sceneId, sessionContent);
            }

            return sessionContent;
        }


        /// <summary>
        /// Removes the session content with the specific ID
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public MBoolResponse RemoveSessionContent(string sessionID)
        {
            //Get the ids
            string sceneId = null;
            string avatarId = null;
            SessionID.GetSplittedIDs(sessionID, out sceneId, out avatarId);

            //Check if sessionID is available
            if (SessionContents.ContainsKey(sceneId))
            {
                //Try to remove the respective session content
                SessionContent sessionContent = null;
                if (SessionContents.TryRemove(sceneId, out sessionContent))
                {
                    return new MBoolResponse(true);
                }
            }

            Logger.Log(Log_level.L_ERROR, $"Session content not available: {sessionID}");


            return new MBoolResponse(false)
            {
                LogData = new List<string>() { "Session content not available " + sessionID }
            };
        }


        /// <summary>
        /// Returns the session content (if available)
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="sessionContent"></param>
        /// <returns></returns>
        public MBoolResponse GetSessionContent(string sessionID, out SessionContent sessionContent)
        {
            sessionContent = null;

            //First check if session id is valid
            if (sessionID == null)
            {
                return new MBoolResponse(false)
                {
                    LogData = new List<string>()
                    {
                        "Session id is null"
                    }
                };
            }

            SessionID idContainer = new SessionID(sessionID);

            //Get session content
            if (!SessionContents.TryGetValue(idContainer.SceneID, out sessionContent))
            {
                //Session content not available
                return new MBoolResponse(false)
                {

                    LogData = new List<string>()
                    {
                        "Session content not available " + sessionID
                    }
                };
            }

            return new MBoolResponse(true);
        }

        /// <summary>
        /// Checks whether the sessionID is already available
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public bool SessionIDAvailable(string sessionID)
        {
            return GetSessionContent(sessionID, out SessionContent sessionContent).Successful;
        }


        /// <summary>
        /// Returns the contents of the respective sessionID (if available).
        /// Otherwise false is returned, whereas the specific message is provided in the LogData.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="sessionContent"></param>
        /// <param name="avatarContent"></param>
        /// <returns></returns>
        public MBoolResponse GetContents(string sessionID, out SessionContent sessionContent, out AvatarContent avatarContent)
        {
            sessionContent = null;
            avatarContent = null;


            MBoolResponse sessionResult = GetSessionContent(sessionID, out sessionContent);

            if (!sessionResult.Successful)
            {
                return sessionResult;
            }

            SessionID idContainer = new SessionID(sessionID);


            if (!sessionContent.AvatarContent.TryGetValue(idContainer.AvatarID, out avatarContent))
            {
                //Session content not available
                return new MBoolResponse(false)
                {

                    LogData = new List<string>()
                    {
                        "Avatar content not available " + sessionID + ", avatarId: " + idContainer.AvatarID
                    }
                };
            }


            return new MBoolResponse(true);
        }
    }

    /// <summary>
    /// Class representing loading properties for a specific MMU
    /// </summary>
    public class MMULoadingProperty
    {
        public MMUDescription Description;

        public string Path;

        public Dictionary<string, object> Data = new Dictionary<string, object>();
    }

}
