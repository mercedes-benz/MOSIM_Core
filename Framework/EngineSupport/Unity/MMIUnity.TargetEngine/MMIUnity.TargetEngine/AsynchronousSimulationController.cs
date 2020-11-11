// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICoSimulation;
using MMICSharp.Common.Communication;
using MMIStandard;
using MMIUnity.TargetEngine.Scene;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MMIUnity.TargetEngine
{
    /// <summary>
    /// Central class for coordinating the whole simulation.
    /// This is an experimental implementation allowing asynchronous updating.
    /// </summary>
    public class AsynchronousSimulationController : MonoBehaviour
    {
        #region public variables

        [Header("Defines whether the session ID is automatically created during runtime")]
        /// <summary>
        /// Flag specified whether a new unique session ID is created at the beginning of the execution
        /// </summary>
        public bool AutoCreateSessionID = true;

        /// <summary>
        /// The unique session id 
        /// </summary>
        [Header("The assigned session id ")]
        public string SessionId;

        /// <summary>
        /// Specifies the fixed frame time (if non realtime mode)
        /// </summary>
        [Header("Specifies the fixed frame time (if non realtime mode)")]

        public float FixedStepTime = 0.01f;

        /// <summary>
        /// The amount of physics updates within each frame
        /// </summary>
        [Header("The amount of physics updates within each frame")]
        public int PhysicsUpdatesPerFrame = 1;

        /// <summary>
        /// All assigned avatars
        /// </summary>
        public List<MMIAvatar> Avatars;


        /// <summary>
        /// The address of the central register of the MMI framework
        /// </summary>
        [Header("The address of the central register of the MMI framework")]
        public string RegisterAddress = "127.0.0.1";


        /// <summary>
        /// The port of the central register of the MMI framework
        /// </summary>
        [Header("The port of the central register of the MMI framework")]
        public int RegisterPort = 9009;

        #endregion

        #region debugging

        public bool RestoreStateFlag = false;

        public bool SaveStateFlag = false;

        public int currentCheckPointID = 0;

        #endregion

        #region private variables

        /// <summary>
        /// Flag which inicates whether the Co Simulator is initialized
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// The serialized co-simulation checkpoints -> to be extended inn future
        /// </summary>
        private readonly List<byte[]> coSimulationCheckpoints = new List<byte[]>();


        /// <summary>
        /// The checkoints of the scenes
        /// </summary>
        private readonly Dictionary<string, byte[]> sceneCheckpoints = new Dictionary<string, byte[]>();

        /// <summary>
        /// The present frame number
        /// </summary>
        private int frameNumber = 0;

        /// <summary>
        /// A timer used for asynchronous updates.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Flag indicating whether the do step is currently executed
        /// </summary>
        private bool inDoStepFlag = false;

        #endregion


        // Use this for initialization
        protected virtual void Start()
        {
            //Manually update the phsyics
            Physics.autoSimulation = false;

            //Specify the target frame rate
            Application.targetFrameRate = 90;

            //Creates a new session ID
            if (this.AutoCreateSessionID)
            {
                //Create a new session id
                this.SessionId = UnitySceneAccess.CreateUUID();
            }

            //Get all avatars 
            this.Avatars = this.GetComponentsInChildren<MMIAvatar>().ToList();

            //Setup the avatars
            foreach (MMIAvatar avatar in this.Avatars)
            {
                //Setup the avatar
                avatar.Setup(this.RegisterAddress, this.RegisterPort, this.SessionId);
            }

            //Wait and check if all connections are initialized
            this.StartCoroutine(CheckInitialization());

            //Create a timer
            this.timer = new Timer(TimerCallback, "finished", 0, (int)(this.FixedStepTime * 1000.0f));      
        }


        /// <summary>
        /// Method is executed if the application is qut
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            //Dispose the timer if utilized
            if (this.timer != null)
                this.timer.Dispose();
        }



        /// <summary>
        /// Performs a simulation cycle for a single frame
        /// </summary>
        /// <param name="time"></param>
        protected virtual void DoStepAsync(float time)
        {
            //##################### Scene synchronization of each MMU Access ####################################################
            this.PushScene();

            //##################### Do the Co Simulation for each Avatar ####################################################

            //Create a dictionary which contains the avatar states for each MMU
            ConcurrentDictionary<MMIAvatar, MSimulationResult> results = new ConcurrentDictionary<MMIAvatar, MSimulationResult>();


            //Compute the frame in parallel manner
            foreach (MMIAvatar avatar in this.Avatars)
            {
                MSimulationResult result = avatar.CoSimulator.DoStep(time, new MSimulationState()
                {
                    Current = avatar.LastPosture,
                    Initial = avatar.LastPosture
                });

                //Assign the resulting posture
                avatar.LastPosture = result.Posture;

                //Execute additional method which can be implemented by child class
                this.OnResultComputed(result, avatar);

                results.TryAdd(avatar, result);
            }

            //Increment the frame number
            this.frameNumber++;


            //####################### Incorporate Results ##################################################

            //Perform on main thread
            MainThreadDispatcher.Instance.ExecuteBlocking(() =>
            {
                //Apply the manipulations of the scene
                this.ApplySceneUpdate(results);

                //Update the physics
                this.UpdatePhysics(time);

                //Assign the posture of each avatar
                foreach (MMIAvatar avatar in this.Avatars)
                {
                    avatar.AssignPostureValues(avatar.LastPosture.Copy());
                }
            });
        }


        /// <summary>
        /// Method which is called for each frame and avatar.
        /// In here further processing can be done (e.g. setting variables of the MMIAvatar).
        /// </summary>
        /// <param name="result"></param>
        /// <param name="avatar"></param>
        protected virtual void OnResultComputed(MSimulationResult result, MMIAvatar avatar)
        {

        }


        #region private methods

        /// <summary>
        /// Coroutine to check the initialization state
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckInitialization()
        {
            while (this.Avatars.Exists(s => !s.MMUAccess.IsInitialized || s.CoSimulator == null))
            {
                yield return null;
            }

            Debug.Log("Initialized");
            this.initialized = true;
        }


        /// <summary>
        /// Timer callback is utilized if the simulation should be computed asynchronously (not depending on unity main thread)
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallback(object state)
        {
            //Skip if not initialized or still performing a do step
            if (this.inDoStepFlag || !this.initialized)
            {
                return;
            }

            //Set flag 
            this.inDoStepFlag = true;

            try
            {
                //Perform the asynchronous do step
                this.DoStepAsync(this.FixedStepTime);
            }
            catch (Exception e)
            {
                Debug.Log("Do step exception: " + e.Message + e.StackTrace);
            }
            finally
            {
                //Reset the flag
                this.inDoStepFlag = false;
            }
        }



        /// <summary>
        /// Applies the updates of the scene
        /// </summary>
        /// <param name="results"></param>
        private void ApplySceneUpdate(ConcurrentDictionary<MMIAvatar, MSimulationResult> results)
        {
            List<MSceneManipulation> sceneManipulations = new List<MSceneManipulation>();

            foreach (KeyValuePair<MMIAvatar, MSimulationResult> coSimulationResult in results)
            {
                //Assign the posture
                if (coSimulationResult.Value.Posture != null)
                    coSimulationResult.Key.AssignPostureValues(coSimulationResult.Value.Posture);

                //Add the scene manipulations
                if (coSimulationResult.Value.SceneManipulations != null)
                {
                    sceneManipulations.AddRange(coSimulationResult.Value.SceneManipulations);
                }
            }

            //Apply the manipulations of the scene
            UnitySceneAccess.Instance.ApplyManipulations(sceneManipulations);

            //Apply the remote scene updates
            this.ApplyRemoteSceneUpdates();

        }


        /// <summary>
        /// Applies scene updates which are remotely defined
        /// </summary>
        private void ApplyRemoteSceneUpdates()
        {
            //Fetch the remote changes
            while (UnitySceneAccess.Instance.RemoteSceneManipulations.Count > 0)
            {
                var remoteSceneManipulation = UnitySceneAccess.Instance.RemoteSceneManipulations.Dequeue();

                //Apply the manipulations
                UnitySceneAccess.Instance.ApplyManipulations(remoteSceneManipulation.SceneManipulations);

                Debug.Log("Applied remote scene manipulations");

                //Set signal to waiting thread
                remoteSceneManipulation.ResetEvent.Set();
            }
        }


        /// <summary>
        /// Manually updates the phsyics engine using the specified timespan and the PhysicsUpdatesPerFrame variable
        /// </summary>
        /// <param name="time"></param>
        private void UpdatePhysics(float time)
        {
            if (!Physics.autoSimulation)
            {
                float delta = time / (float)this.PhysicsUpdatesPerFrame;

                for (int i = 0; i < this.PhysicsUpdatesPerFrame; i++)
                {
                    Physics.Simulate(delta);
                }
            }
        }


        /// <summary>
        /// Pushes the scene to each adapter/MMU
        /// Scene synchronization
        /// </summary>
        private void PushScene()
        {
            //Synchronizes the scene in before each update
            //To do parallelize in future
            for (int i = 0; i < this.Avatars.Count; i++)
            {
                this.Avatars[i].MMUAccess.PushScene(false);
            }

            //Clear the events since the current events have been already synchronized
            UnitySceneAccess.ClearChanges();
        }


        /// <summary>
        /// Saves the state of the scene and all avatars
        /// </summary>
        private void SaveState()
        {
            this.currentCheckPointID++;

            //Save the entire scene of the given frame
            byte[] sceneSnapshot = this.SaveScene();
            sceneCheckpoints.Add(this.currentCheckPointID.ToString(), sceneSnapshot);

            //Save the co-simulation state
            foreach (MMICoSimulator coSimulator in this.Avatars.Select(s => s.CoSimulator))
            {
                byte[] checkpoint = coSimulator.CreateCheckpoint();
                this.coSimulationCheckpoints.Add(checkpoint);
            }
        }


        /// <summary>
        ///Restores the state of the scene and all avatars
        /// </summary>
        private void RestoreState()
        {
            //Restore the entire scene
            this.RestoreScene(sceneCheckpoints[this.currentCheckPointID.ToString()]);

            //Restore the co-simulation state
            foreach (MMICoSimulator coSimulator in this.Avatars.Select(s => s.CoSimulator))
            {
                coSimulator.RestoreCheckpoint(this.coSimulationCheckpoints[this.currentCheckPointID - 1]);
            }
        }


        /// <summary>
        /// Creates a snapshot of the scene
        /// </summary>
        /// <param name="id"></param>

        private byte[] SaveScene()
        {
            Dictionary<string, MTransform> sceneTransforms = new Dictionary<string, MTransform>();

            foreach (MMISceneObject sceneObject in this.GetComponentsInChildren<MMISceneObject>())
            {
                sceneTransforms.Add(sceneObject.MSceneObject.ID, new MTransform(sceneObject.MSceneObject.ID, sceneObject.MSceneObject.Transform.Position, sceneObject.MSceneObject.Transform.Rotation));
            }

            return Serialization.SerializeBinary(sceneTransforms);
        }


        /// <summary>
        /// Restores the scene based on a snapshot
        /// </summary>
        /// <param name="data"></param>
        private void RestoreScene(byte[] data)
        {
            var sceneTransforms = Serialization.DeserializeBinary<Dictionary<string, MTransform>>(data);

            foreach (MMISceneObject sceneObject in this.GetComponentsInChildren<MMISceneObject>())
            {
                MTransform transform = sceneTransforms[sceneObject.MSceneObject.ID];

                sceneObject.transform.position = transform.Position.ToVector3();
                sceneObject.transform.rotation = transform.Rotation.ToQuaternion();
                sceneObject.UpdateTransform();
            }
        }




        #endregion
    }
}
