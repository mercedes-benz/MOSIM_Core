// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMICSharp.Access.Abstraction
{
    /// <summary>
    /// Interface which represents a basic adapter access
    /// </summary>
    public interface IAdapter
    {
        /// <summary>
        /// The descirption of the adapter
        /// </summary>
        MAdapterDescription Description
        {
            get;
            set;
        }

        /// <summary>
        /// Flag which indicates whether the communciaton has been successfully initialized
        /// </summary>
        bool Initialized
        {
            get;
            set;
        }


        /// <summary>
        /// Flag indicates whether the scene is synchronized
        /// </summary>
        bool SceneSynchronized
        {
            get;
            set;
        }

        /// <summary>
        /// A list of the available MMU Descriptions
        /// </summary>
        List<MMUDescription> MMUDescriptions
        {
            get;
            set;
        }

        /// <summary>
        /// Flag specifies whether the MMUs have been loaded
        /// </summary>
        bool Loaded
        {
            get;
            set;
        }


        /// <summary>
        /// Method starts the process
        /// </summary>
        void Start();


        /// <summary>
        /// Creates a client for direct communication with the adapter
        /// </summary>
        /// <returns></returns>
        IAdapterClient CreateClient();


        /// <summary>
        /// Returns all mmus which are available at the assigned adapter and for the given session Id
        /// </summary>
        /// <returns></returns>
        List<MotionModelUnitAccess> CreateMMUConnections(string sessionId, List<MMUDescription> mmuDescriptions);


        /// <summary>
        /// Synchronizes the scene of the module
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        MBoolResponse PushScene(MSceneUpdate sceneUpdates, string sessionId);


        /// <summary>
        /// Fetches the entire scene from the adapter
        /// </summary>
        /// <returns></returns>
        List<MSceneObject> GetScene(string sessionId);


        /// <summary>
        ///Gets the scene events of the current frame
        ///To check
        /// </summary>
        /// <returns></returns>
        MSceneUpdate GetSceneChanges(string sessionId);


        /// <summary>
        /// Dispose method which closes the used application
        /// </summary>
        MBoolResponse Dispose();


        /// <summary>
        /// Closes the connection of the adapter
        /// </summary>
        /// <returns></returns>
        MBoolResponse CloseConnection();



        /// <summary>
        /// Create a new session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="referenceAvatar">The utilized reference avatar</param>
        MBoolResponse CreateSession(string sessionId, MAvatarDescription referenceAvatar);


        /// <summary>
        /// Closes the session
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        MBoolResponse CloseSession(string sessionID);


        /// <summary>
        /// Returns all loadable MMUs identified by the adapater
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        List<MMUDescription> GetLoadableMMUs(string sessionId);


        /// <summary>
        /// Loads the mmus by id
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="sessionId"></param>
        MBoolResponse LoadMMUs(List<string> ids, string sessionId);


        /// <summary>
        /// Returns the status of the adapter
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetStatus();



        /// <summary>
        /// Creates a new checkpoint with the specific id
        /// </summary>
        /// <param name="mmuList"></param>
        /// <param name="checkpointID"></param>
        byte[] CreateCheckpoint(string mmuID, string checkpointID);


        /// <summary>
        /// Restores a checkpoint
        /// </summary>
        /// <param name="mmuList"></param>
        /// <param name="checkpointID"></param>
        MBoolResponse RestoreCheckpoint(string mmuID, string checkpointID, byte[] checkPointData);

    }
}
