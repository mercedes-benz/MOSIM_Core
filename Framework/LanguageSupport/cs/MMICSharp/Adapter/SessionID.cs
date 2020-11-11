// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

namespace MMICSharp.Adapter
{
    /// <summary>
    /// Wrapper class for a session ID
    /// </summary>
    public class SessionID
    {
        /// <summary>
        /// The postfix 
        /// </summary>
        public string SceneID
        {
            get;
            private set;
        }

        /// <summary>
        /// The avatar id sub-part
        /// </summary>
        public string AvatarID
        {
            get;
            private set;
        }

        /// <summary>
        /// The full ID
        /// </summary>
        public string Full
        {
            get;
            private set;
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="sessionID"></param>
        public SessionID(string sessionID)
        {
            string sceneID;
            string avatarID;

            SessionID.GetSplittedIDs(sessionID, out sceneID, out avatarID);

            this.SceneID = sceneID;
            this.AvatarID = avatarID;
            this.Full = sessionID;
        }


        /// <summary>
        /// Gets the splitted ids
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="sceneId"></param>
        /// <param name="avatarId"></param>
        public static void GetSplittedIDs(string sessionId, out string sceneId, out string avatarId)
        {
            //Set the default values
            avatarId = "0";
            sceneId = sessionId;

            string[] splitted = sessionId.Split(':');

            // Test if the Format was correct
            if (splitted.Length == 2 && splitted[0] != "" && splitted[1] != "")
            {
                sceneId = splitted[0];
                avatarId = splitted[1];
            }
        }
    }
}
