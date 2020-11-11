// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using CSharpAdapter;
using MMICSharp.Common;
using System;
using System.Collections.Concurrent;


namespace MMICSharp.Adapter
{
    /// <summary>
    /// Class which containts the content which is related to specific session
    /// </summary>
    public class SessionContent
    {
        /// <summary>
        /// The corresponding scene access
        /// </summary>
        public MMIScene SceneBuffer;

        /// <summary>
        /// The specific service access for the session
        /// </summary>
        public ServiceAccess ServiceAccess;

        /// <summary>
        /// Dictionary to stroe the avatar content
        /// </summary>
        public ConcurrentDictionary<string, AvatarContent> AvatarContent = new ConcurrentDictionary<string, CSharpAdapter.AvatarContent>();

        /// <summary>
        /// The session id
        /// </summary>
        public string SessionID;

        /// <summary>
        /// The last time the session has been used
        /// </summary>
        public DateTime LastAccess;


        /// <summary>
        /// A reference to the session data
        /// </summary>
        private readonly SessionData sessionData;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="sessionID"></param>
        public SessionContent(SessionData sessionData, string sessionID)
        {
            this.SessionID = sessionID;
            this.SceneBuffer = new MMIScene();
            this.sessionData = sessionData;

            this.ServiceAccess = new ServiceAccess(this.sessionData.MMIRegisterAddress, sessionID);
            this.ServiceAccess.Initialize();
        }

        /// <summary>
        /// Returns the avatar content based on the given id
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        public AvatarContent GetAvatarContent(string avatarId)
        {
            if (this.AvatarContent.ContainsKey(avatarId))
                return this.AvatarContent[avatarId];

            return null;
        }

        /// <summary>
        /// Creates a new avatar content with the specified id
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        public AvatarContent CreateAvatarContent(string avatarId)
        {

            if (this.AvatarContent.ContainsKey(avatarId))
                this.AvatarContent[avatarId] = new CSharpAdapter.AvatarContent(avatarId);
            else
                this.AvatarContent.TryAdd(avatarId, new CSharpAdapter.AvatarContent(avatarId));

            return this.AvatarContent[avatarId];
        }


        /// <summary>
        /// Updates the last access time
        /// </summary>
        public void UpdateLastAccessTime()
        {
            //Set the last access time
            this.LastAccess = DateTime.Now;
            this.sessionData.LastAccess = DateTime.Now;
        }

    }
}
