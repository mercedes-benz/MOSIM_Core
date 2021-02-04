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
using UnityEngine;

namespace MMIUnity.TargetEngine
{
    /// <summary>
    /// Central class for coordinating the whole simulation.
    /// The script should be added to the root object which contains all avatars and the scene.
    /// Open Points: One scene for all, or each co-simulation holds one scene (more strict separation)
    /// </summary>
    [RequireComponent(typeof(MMISettings))]
    public class SimulationController : MonoBehaviour
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
        /// Specifies whether the co simulator works in realtime
        /// </summary>
        [Header("Specifies whether the co simulator works in realtime")]
        public bool RealTime = true;

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

        [Header("Specifies whether the simulation controller is automatically started.")]
        public bool AutoStart = true;


        [Header("Specifies whether multiple avatars are executed in parallel.")]
        public bool ExecuteAvatarsParallel = false;

        /// <summary>
        /// On initialized event
        /// </summary>
        public virtual event EventHandler<EventArgs> OnInitialized;

        #endregion

        #region debugging

        [HideInInspector]
        public bool RestoreStateFlag = false;

        [HideInInspector]
        public bool SaveStateFlag = false;

        [HideInInspector]
        public int currentCheckPointID = 0;

        #endregion

        #region protected variables

        /// <summary>
        /// Flag which inicates whether the Co Simulator is initialized
        /// </summary>
        protected bool initialized = false;

        /// <summary>
        /// The delta time for the current frame
        /// </summary>
        protected float frameDeltaTime;

        /// <summary>
        /// The serialized co-simulation checkpoints -> to be extended inn future
        /// </summary>
        protected List<byte[]> coSimulationCheckpoints = new List<byte[]>();


        /// <summary>
        /// The checkoints of the scenes
        /// </summary>
        protected Dictionary<string, byte[]> sceneCheckpoints = new Dictionary<string, byte[]>();

        /// <summary>
        /// The present frame number
        /// </summary>
        protected int frameNumber = 0;

        /// <summary>
        /// The current fps 
        /// </summary>
        protected float currentUpdateFPS = 30;


        #endregion


 

        // Use this for initialization
        protected virtual void Start()
        {
            if(AutoStart)
                this.Setup();
        }


        /// <summary>
        /// Method to setup the simulation controller
        /// </summary>
        public virtual void Setup()
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


            MMISettings settings = this.GetComponent<MMISettings>();

            //Setup the avatars
            foreach (MMIAvatar avatar in this.Avatars)
            {
                //Setup the avatar
                avatar.Setup(settings.MMIRegisterAddress, settings.MMIRegisterPort, this.SessionId);
            }

            //Wait and check if all connections are initialized
            this.StartCoroutine(CheckInitialization());
        }


        // Update is called once per frame
        protected virtual void Update()
        {
            //Estimate the update fps
            this.currentUpdateFPS = 1.0f / Time.unscaledDeltaTime;

            //Set the time scale (simulated time per second)
            Time.timeScale = this.frameDeltaTime * currentUpdateFPS;

            //Get the desired delta time for the current frame
            this.frameDeltaTime = this.RealTime ? Time.unscaledDeltaTime : this.FixedStepTime;

            //Skip if not initialized
            if (!this.initialized)
            {
                return;
            }

            this.DoStep(frameDeltaTime);
        }



        /// <summary>
        /// Performs a simulation cycle for a single frame
        /// </summary>
        /// <param name="time"></param>
        protected virtual void DoStep(float time)
        {
            //#####################Save or restore the states of the co-simulations and environment#########################################

            //Save the entire state if desired
            if (this.SaveStateFlag)
            {
                this.SaveState();

                //Set flag to false
                this.SaveStateFlag = false;
            }

            //Restore the entired state if destired
            if (this.RestoreStateFlag)
            {
                this.RestoreState();

                //Set flag to false
                this.RestoreStateFlag = false;
            }


            //##################### Scene synchronization of each MMU Access ####################################################
            this.PushScene();


            //##################### Do the Co Simulation for each Avatar ####################################################

            //Create a dictionary which contains the avatar states for each MMU
            ConcurrentDictionary<MMIAvatar, MSimulationResult> results = new ConcurrentDictionary<MMIAvatar, MSimulationResult>();

            ///Optionall execute multiple avatars in parallel
            if (ExecuteAvatarsParallel && this.Avatars.Count > 1)
            {
                //Pre compute frame (on main thread)
                foreach (MMIAvatar avatar in this.Avatars)
                {
                    avatar.CoSimulator.PreComputeFrame();
                }

                //Perform in parallel
                System.Threading.Tasks.Parallel.ForEach(this.Avatars, (MMIAvatar avatar) =>
                {
                    MSimulationResult result = avatar.CoSimulator.ComputeFrame(time);
                    results.TryAdd(avatar, result);
                });


                //Pre compute frame (on main thread)
                foreach (MMIAvatar avatar in this.Avatars)
                {
                    avatar.CoSimulator.PostComputeFrame(results[avatar]);

                    //Execute additional method which can be implemented by child class
                    this.OnResultComputed(results[avatar], avatar);
                }
   
            }

            else
            {
                foreach (MMIAvatar avatar in this.Avatars)
                {
                    //Can be optimized in future -> Instead of simulation state -> transmit nothing
                    MSimulationResult result = avatar.CoSimulator.DoStep(time, new MSimulationState() { Initial = avatar.GetPosture(), Current = avatar.GetPosture() });

                    //Execute additional method which can be implemented by child class
                    this.OnResultComputed(result, avatar);

                    //Add the results
                    results.TryAdd(avatar, result);
                }
            }

            //####################### Incorporate Results ##################################################

            //Apply the manipulations of the scene
            this.ApplySceneUpdate(results);

            //Updates the physics of the scene
            this.UpdatePhysics(time);

            //Increment the frame number
            this.frameNumber++;
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
        /// Method is executed if the application is qut
        /// </summary>
        protected virtual void OnApplicationQuit()
        {

        }


        /// <summary>
        /// Coroutine to check the initialization state
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator CheckInitialization()
        {
            while (this.Avatars.Exists(s => !s.MMUAccess.IsInitialized || s.CoSimulator == null))
            {
                yield return null;
            }

            Debug.Log("Initialized");
            this.initialized = true;

            this.OnInitialized?.Invoke(this, new EventArgs());


            MMISettings settings = this.GetComponent<MMISettings>();
        }




        /// <summary>
        /// Applies the updates of the scene
        /// </summary>
        /// <param name="results"></param>
        protected void ApplySceneUpdate(ConcurrentDictionary<MMIAvatar, MSimulationResult> results)
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
        protected void ApplyRemoteSceneUpdates()
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
        protected void UpdatePhysics(float time)
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
        protected void PushScene()
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
        protected void SaveState()
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
        protected void RestoreState()
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

        protected byte[] SaveScene()
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
        protected void RestoreScene(byte[] data)
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
   
    }
}
