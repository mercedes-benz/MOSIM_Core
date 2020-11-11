// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections.Generic;


namespace MMIUnity.TargetEngine.Scene
{
    /// <summary>
    /// Class represents a scene manipulation request which is defined remotely
    /// </summary>
    public class RemoteSceneManipulationRequest : MSynchronizableScene.Iface, MMIServiceBase.Iface
    {
        /// <summary>
        /// Reference to the sceneAccess
        /// </summary>
        private readonly UnitySceneAccess sceneAccess;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="sceneAccess"></param>
        public RemoteSceneManipulationRequest(UnitySceneAccess sceneAccess)
        {
            this.sceneAccess = sceneAccess;
        }

        /// <summary>
        /// Method to apply the remote scene manipulations
        /// </summary>
        /// <param name="sceneManipulations"></param>
        /// <returns></returns>
        public MBoolResponse ApplyManipulations(List<MSceneManipulation> sceneManipulations)
        {
            //Create a new container
            RemoteSceneManipulation remoteSceneManipulation = new RemoteSceneManipulation(sceneManipulations);

            //To do -> Blocking call
            this.sceneAccess.RemoteSceneManipulations.Enqueue(remoteSceneManipulation);

            //Block until the scene manipulation is processed
            remoteSceneManipulation.ResetEvent.WaitOne();

            return new MBoolResponse(true);
        }


        public MBoolResponse ApplyUpdates(MSceneUpdate sceneUpdates)
        {
            MBoolResponse response = new MBoolResponse(false);

            //Execute on main thread
            MainThreadDispatcher.Instance.ExecuteBlocking(() =>
            {
                response = this.sceneAccess.ApplyUpdates(sceneUpdates);
            });

            return response;
        }

        public Dictionary<string, string> Consume(Dictionary<string, string> properties)
        {
            return new Dictionary<string, string>();
        }

        public MBoolResponse Dispose(Dictionary<string, string> properties)
        {
            throw new System.NotImplementedException();
        }

        public MServiceDescription GetDescription()
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, string> GetStatus()
        {
            return new Dictionary<string, string>()
            {
                { "Running", true.ToString()}
            };
        }

        public MBoolResponse Restart(Dictionary<string, string> properties)
        {
            throw new System.NotImplementedException();
        }

        public MBoolResponse Setup(MAvatarDescription avatar, Dictionary<string, string> properties)
        {
            return new MBoolResponse(true);
        }
    }
}
