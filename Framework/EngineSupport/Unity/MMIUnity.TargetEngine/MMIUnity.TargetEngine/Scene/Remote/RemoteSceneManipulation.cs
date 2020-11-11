// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections.Generic;
using System.Threading;

namespace MMIUnity.TargetEngine.Scene
{
    /// <summary>
    /// Class represent a scene manipulation which is requested remotely
    /// </summary>
    public class RemoteSceneManipulation
    {

        public ManualResetEvent ResetEvent;

        /// <summary>
        /// The intended scene manipulations
        /// </summary>
        public List<MSceneManipulation> SceneManipulations;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="sceneManipulations"></param>
        public RemoteSceneManipulation(List<MSceneManipulation> sceneManipulations)
        {
            this.SceneManipulations = sceneManipulations;
            this.ResetEvent = new ManualResetEvent(false);
        }
    }
}
