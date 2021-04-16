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
using System.Threading.Tasks;
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
        [Header("Specifies the temporal accuracy (in s)")]
        public float TemporalAccuracy = 0.01f;


        /// <summary>
        /// Specifies the intervall after which a do step is triggered (0 as fast as possible)
        /// </summary>
        public float UpdateTime = 0.01f;

        /// <summary>
        /// The amount of physics updates within each frame
        /// </summary>
        [Header("The amount of physics updates within each frame")]
        public int PhysicsUpdatesPerFrame = 1;

        /// <summary>
        /// All assigned avatars
        /// </summary>
        protected List<MMIAvatar> Avatars = new List<MMIAvatar>();
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


        public bool ShowFPS = false;
        public bool ExecuteParallel = false;
        public int MaxNumberThreads = 4;

        [HideInInspector]
        public int currentCheckPointID = 0;

        private float renderingFPS = 0;
        private float mmiFPS = 0;
        private List<float> mmiFPSList = new List<float>();
        private List<float> renderingFPSList = new List<float>();
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        protected Mutex avatarModificationMutex = new Mutex();


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
            Application.targetFrameRate = 60;

            //Creates a new session ID
            if (this.AutoCreateSessionID)
            {
                //Create a new session id
                this.SessionId = UnitySceneAccess.CreateUUID();
            }

            //Get all avatars 
            foreach(MMIAvatar avatar in this.GetComponentsInChildren<MMIAvatar>())
                this.Avatars.Add(avatar);


            //Setup the avatars
            foreach (MMIAvatar avatar in this.Avatars)
            {
                //Setup the avatar
                avatar.Setup(this.RegisterAddress, this.RegisterPort, this.SessionId);
            }

            //Wait and check if all connections are initialized
            this.StartCoroutine(CheckInitialization());
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


        protected virtual void OnGUI()
        {
            //Display the current fps if enabled
            if (ShowFPS)
            {
                int w = Screen.width, h = Screen.height;

                GUIStyle style = new GUIStyle();

                Rect rect = new Rect(w / 2, 0, w, h * 2 / 100);
                style.alignment = TextAnchor.UpperLeft;
                style.fontSize = h * 4 / 100;
                style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
                string text = "Rendering " + this.renderingFPS.ToString("F0") + " fps" + ", MMI Framework " + this.mmiFPS.ToString("F0") + " fps";
                GUI.Label(rect, text, style);
            }
        }


        bool eventHandlerRegistered = false;
        /// <summary>
        /// Performs a simulation cycle for a single frame
        /// </summary>
        /// <param name="time"></param>
        protected virtual void DoStepAsync(float time)
        {
            //Variable for storing the changes occured in the scene 
            MSceneUpdate sceneUpdates = null;

            //Perform on main thread
            MainThreadDispatcher.Instance.ExecuteBlocking(() =>
            {
                //Get the scene updates
                sceneUpdates = UnitySceneAccess.Instance.GetSceneChanges();

                //Clear the events since the current events have been already synchronized
                UnitySceneAccess.ClearChanges();

                //Pre compute frame (on main thread)
                foreach (MMIAvatar avatar in this.Avatars)
                    avatar.CoSimulator.PreComputeFrame();
            });

            //##################### Do the Co Simulation for each Avatar ####################################################

            //Create a dictionary which contains the avatar states for each MMU
            ConcurrentDictionary<MMIAvatar, MSimulationResult> results = new ConcurrentDictionary<MMIAvatar, MSimulationResult>();



            //Perform in parallel
            if (this.Avatars.Count > 1 && this.ExecuteParallel)
            {
                //Perform in parallel
                System.Threading.Tasks.Parallel.ForEach(this.Avatars, new ParallelOptions { MaxDegreeOfParallelism = this.MaxNumberThreads },(MMIAvatar avatar) =>
                {
                    //Synchronizes the scene in before each update
                    avatar.MMUAccess.PushSceneUpdate(sceneUpdates);

                    //Perform the actual do step
                    MSimulationResult result = avatar.CoSimulator.ComputeFrame(time);

                    //Assign the resulting posture
                    avatar.LastPosture = result.Posture;

                    //Execute additional method which can be implemented by child class
                    //this.OnResultComputed(result, avatar);

                    results.TryAdd(avatar, result);
                });

            }

            //Perform sequential
            else
            {
                //Synchronizes the scene of each avatar in before update
                for (int i = 0; i < this.Avatars.Count; i++)
                {
                    this.Avatars.ElementAt(i).MMUAccess.PushSceneUpdate(sceneUpdates);
                }

                if (!eventHandlerRegistered)
                {
                    foreach (MMIAvatar avatar in this.Avatars)
                    {
                        avatar.CoSimulator.LogEventHandler += CoSimulator_LogEventHandler;
                    }
                    eventHandlerRegistered = true;
                }

                //Compute the frames
                foreach (MMIAvatar avatar in this.Avatars)
                {


                    MSimulationResult result = avatar.CoSimulator.ComputeFrame(time);

                    //Assign the resulting posture
                    avatar.LastPosture = result.Posture;

                    //Execute additional method which can be implemented by child class
                    this.OnResultComputed(result, avatar);

                    results.TryAdd(avatar, result);
                }
            }

            //Increment the frame number
            this.frameNumber++;


            //####################### Incorporate Results ##################################################

            //Perform on main thread
            MainThreadDispatcher.Instance.ExecuteBlocking(() =>
            {
                //Execute post compute frame
                foreach (var result in results)
                {
                    result.Key.CoSimulator.PostComputeFrame(result.Value);

                    //Execute additional method which can be implemented by child class
                    this.OnResultComputed(result.Value, result.Key);
                }
            
                //Apply the manipulations of the scene
                this.ApplySceneUpdate(results);

                //Update the physics
                this.UpdatePhysics(time);
            });

            watch.Stop();

            if (this.mmiFPSList.Count > 30)
                this.mmiFPSList.RemoveAt(0);

            this.mmiFPSList.Add((float)(1.0 / watch.Elapsed.TotalSeconds));
            this.mmiFPS = this.mmiFPSList.Sum() / this.mmiFPSList.Count;

            watch.Restart();

        }

        private void CoSimulator_LogEventHandler(object sender, CoSimulationLogEvent e)
        {
            Debug.Log(e.Message);
        }


        // Update is called once per frame
        void Update()
        {
            if (this.renderingFPSList.Count > 30)
                this.renderingFPSList.RemoveAt(0);

            this.renderingFPSList.Add(1.0f / Time.deltaTime);
            this.renderingFPS = this.renderingFPSList.Sum() / this.renderingFPSList.Count;

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

        /// <summary>
        /// Registers a new avatar at the simulation controller
        /// </summary>
        /// <param name="avatar"></param>
        public void RegisterAvatar(MMIAvatar avatar)
        {
            //Execute in new thread to avoid dead lock
            Task.Run(() => 
            {
                this.avatarModificationMutex.WaitOne();

                try
                {
                    this.Avatars.Add(avatar);

                }
                catch (Exception)
                {
                    Debug.Log("Problem at adding avatar");
                }
                finally
                {
                    this.avatarModificationMutex.ReleaseMutex();
                }
            });

        }

        /// <summary>
        /// Unregisters an avatar at the simulation controller
        /// </summary>
        /// <param name="avatar"></param>
        public void UnregisterAvatar(MMIAvatar avatar)
        {
            //Execute in new thread to avoid dead lock
            Task.Run(() =>
            {
                this.avatarModificationMutex.WaitOne();

                try
                {
                    this.Avatars.Remove(avatar);

                }
                catch (Exception)
                {
                    Debug.Log("Problem at removing avatar");
                }
                finally
                {
                    this.avatarModificationMutex.ReleaseMutex();
                }
                this.avatarModificationMutex.WaitOne();



            });

        }




        #region private methods

        /// <summary>
        /// Coroutine to check the initialization state
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckInitialization()
        {
            while (this.Avatars.ToList().Exists(s => !s.MMUAccess.IsInitialized || s.CoSimulator == null))
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("Initialized");
            this.initialized = true;

            this.watch.Start();

            //Create a timer
            this.timer = new Timer(TimerCallback, "finished", 0, (int)(this.UpdateTime* 1000.0f));
        }


        /// <summary>
        /// Timer callback is utilized if the simulation should be computed asynchronously (not depending on unity main thread)
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallback(object state)
        {
            //Ensure that the scene changes have been already incorporated

            //Skip if not initialized or still performing a do step
            if (this.inDoStepFlag || !this.initialized)
            {
                return;
            }

            //Set flag 
            this.inDoStepFlag = true;

            try
            {
                //Acquire mutex for avatar modifications
                this.avatarModificationMutex.WaitOne();

                //Perform the asynchronous do step
                this.DoStepAsync(this.TemporalAccuracy);
            }
            catch (Exception e)
            {
                Debug.Log("Do step exception: " + e.Message + e.StackTrace);
            }
            finally
            {
                //Reset the flag
                this.inDoStepFlag = false;

                this.avatarModificationMutex.ReleaseMutex();
            }

            //Start the next round
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
