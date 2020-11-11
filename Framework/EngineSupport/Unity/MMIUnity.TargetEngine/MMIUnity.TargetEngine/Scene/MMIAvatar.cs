// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICoSimulation;
using MMICSharp.Access;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MMICSharp.Common;

namespace MMIUnity.TargetEngine.Scene
{
    /// <summary>
    /// Basic class which represents an avatar and the underlaying functionality (e.g. behavior, co-sim)
    /// </summary>
    public class MMIAvatar : UnityAvatarBase
    {
        #region public variables

        /// <summary>
        /// Flag specifies whether the internal Unity-Cosimulation is utilized or a remotely connected CoSimulation 
        /// </summary>
        [Header("Flag specifies whether the internal Unity-Cosimulation is utilized or a remotely connected CoSimulation")]
        public bool UseRemoteCoSimulation = false;

        /// <summary>
        /// The (optional) name of the remote CoSimulation
        /// </summary>
        [Header("The (optional) name of the remote CoSimulation")]
        public string RemoteCoSimulationName;


        /// <summary>
        /// Flag specifies whether the bones of the avatar should be added as scene objects
        /// </summary>
        [Header("Flag specifies whether the bones of the avatar should be added as scene objects")]
        public bool AddBoneSceneObjects = false;


        [Header("Timeout for establishing a connection to the MMI framework and the remote MMUs specified in s.")]
        public float Timeout = 30f;


        /// <summary>
        /// Specifies whether the scene is accessible for external clients via thrift server
        /// </summary>
        [Header("Specifies whether the remote co-simulation access is enabled.")]
        public bool AllowRemoteCoSimulationAccess = true;

        /// <summary>
        /// The port for the external write access
        /// </summary>
        [Header("The port used for remotely accessin the cosimulation")]
        public int RemoteCoSimulationAccessPort = 9002;

        /// <summary>
        /// The port for the external write access
        /// </summary>
        [Header("The address used for remotely accessin the cosimulation")]
        public string RemoteCoSimulationAccessAddress = "127.0.0.1";


        /// <summary>
        /// The assigned CoSimulator (either local or remote)
        /// </summary>
        [HideInInspector]
        public MMICoSimulator CoSimulator;

        /// <summary>
        /// The behavior component which creates the instructions for the co-simulation and avatar
        /// </summary>
        [HideInInspector]
        public AvatarBehavior Behavior;

        /// <summary>
        /// Clas which provides access to the MMUs
        /// </summary>
        [HideInInspector]
        public MMUAccess MMUAccess;

        /// <summary>
        /// The posture of the last frame.
        /// </summary>
        [HideInInspector]
        public MAvatarPostureValues LastPosture;

        /// <summary>
        /// The avatar representation within the MMI framework
        /// </summary>
        [HideInInspector]
        public MAvatar MAvatar;


        /// <summary>
        /// Specifies whether the retargeting is loaded from configuration file
        /// </summary>
        [Header("Specifies whether the retargeting is loaded from configuration file.")]
        public bool LoadRetargetingConfiguration = true;


        #endregion

        #region protected fields

        /// <summary>
        /// A status text used for visualization
        /// </summary>
        protected string statusText;

        /// <summary>
        /// The hosted co-simulation access
        /// </summary>
        protected CoSimulationAccess coSimulationAccess;

        /// <summary>
        /// The hosted remote skeleton access server
        /// </summary>
        protected RemoteSkeletonAccessServer remoteSkeletonAccessServer;

        /// <summary>
        /// Indicates whether the setup of the retageting has been successfully done
        /// </summary>
        protected bool setupRetargeting = false;

        #endregion


        /// <summary>
        /// Basic awake routine
        /// </summary>
        protected override void Awake()
        {
            //Call the awake of the base class
            Debug.Log("Awake");
            base.Awake();
        }


        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            //Assign the behavior
            this.Behavior = this.GetComponent<AvatarBehavior>();

            //Add root scene object for the avatar
            this.gameObject.AddComponent<MMISceneObject>();

            //Add the scene objects of the bones (if desired)
            if (this.AddBoneSceneObjects)
            {
                //Setup the scene objects of the bones
                foreach (Transform transform in this.Joints)
                {
                    MMISceneObject s = transform.gameObject.AddComponent<MMISceneObject>();
                    s.ShowCoordinateSystem = false;
                }
            }


        }


        /// <summary>
        /// Update routine which is called for each frame
        /// </summary>
        protected virtual void Update()
        {
            //Check if avatar instance is already created (should be the case)
            if (this.MAvatar != null)
            {
                //Get the posture values of the current posture
                this.MAvatar.PostureValues = this.GetPosture();

                //To do define a threshold to avoid unnecessary calls

                //Notify the scene concerning the update
                UnitySceneAccess.PostureValuesChanged(this.MAvatar, this.MAvatar.PostureValues);
            }
        }

        /// <summary>
        /// Setup method for the MMIAvatar
        /// </summary>
        /// <param name="address">The address of the MMU server</param>
        /// <param name="sessionId">A unique id for the session</param>
        /// <param name="avatarID">A unique id for the avatar</param>
        public virtual void Setup(string address, int port, string sessionId)
        {
            //Setup the retargeting
            this.SetupRetargeting();

            //Create the mmu access which creates the connection to the MMI framework
            this.MMUAccess = new MMUAccess(sessionId + ":" + this.MAvatar.ID)
            {
                //Set the scene access
                SceneAccess = GameObject.FindObjectOfType<UnitySceneAccess>(),

                //Define the retargeting here
                SkeletonAccess = this.GetSkeletonAccess(),
            };

            //Set the id
            this.MMUAccess.AvatarID = this.MAvatar.ID;

            //Setthe status text
            this.statusText = "Connecting to MMUAccess " + address + ":" + port;

            //Connect asynchronously and wait for the result
            this.MMUAccess.ConnectAsync(new MIPAddress(address, port), TimeSpan.FromSeconds(this.Timeout), this.ConnectionCallback, this.MAvatar.ID);

            // Spawn Skeleton Access Service, if required. 
            MMISettings settings = GetComponentInParent<MMISettings>();
            if (settings.AllowRemoteSkeletonConnections)
            {
                //Start the remote skeleton access
                this.remoteSkeletonAccessServer = new RemoteSkeletonAccessServer(new MIPAddress(settings.RemoteSkeletonAccessAddress, settings.RemoteSkeletonAccessPort), new MIPAddress(settings.MMIRegisterAddress, settings.MMIRegisterPort), this.GetRetargetingService().GetSkeleton());
                this.remoteSkeletonAccessServer.Start();

                Debug.Log("Started Remote Skeleton Access Server with Avatar <" + this.AvatarID + ">");
            }
        }

        /// <summary>
        /// Method to setup the actual retargeting
        /// </summary>
        protected virtual void SetupRetargeting()
        {
            //Only do the retargeting setup if not happened in before
            if (!setupRetargeting)
            {
                setupRetargeting = true;
                // find and load retargeting configuration file
                //Create a unique id for the avatar (only valid in the current session -> otherwise UUID required)
                string id = UnitySceneAccess.CreateAvatarID();

                //Only load the configuration if defined
                if (LoadRetargetingConfiguration)
                {
                    if (!System.IO.File.Exists(Application.dataPath + "/" + this.ConfigurationFilePath))
                    {
                        Debug.LogError($"Problem setting up retargeting: The required file: {Application.dataPath + "/" + this.ConfigurationFilePath} is not available");
                        return;
                    }

                    string skelConf = System.IO.File.ReadAllText(Application.dataPath + "/" + this.ConfigurationFilePath);



                    MAvatarPosture p = MMICSharp.Common.Communication.Serialization.FromJsonString<MAvatarPosture>(skelConf);//JsonConvert.DeserializeObject<MAvatarPosture>(skelConf);
                    p.AvatarID = id;

                    this.SetupRetargeting(id, p);
                    this.AssignPostureValues(retargetingService.RetargetToIntermediate(p));
                }

                //If not defined use the global posture
                else
                {
                    this.SetupRetargeting(id);
                }

                MAvatarDescription avatarDescription = this.GetSkeletonAccess().GetAvatarDescription(id);

                //Create a new MAvatar (the representation within MMI framework)
                MAvatarPostureValues zeroPosture = this.GetSkeletonAccess().GetCurrentPostureValues(id);

                //Create the avatar
                this.MAvatar = new MAvatar()
                {
                    Name = this.name,
                    ID = id,
                    Description = avatarDescription,
                    PostureValues = zeroPosture,
                    Properties = new Dictionary<string, string>(),
                    SceneObjects = new List<string>()
                };

                //Add the avatar to the scene access
                UnitySceneAccess.AddAvatar(this.MAvatar);

                Debug.Log("Retargeting successfully set up");

            }

        }


        /// <summary>
        /// Callback  for the connection setup
        /// </summary>
        /// <param name="connected"></param>
        protected virtual void ConnectionCallback(bool connected)
        {
            if (connected)
            {
                this.statusText = "Loading MMUs";

                Debug.Log($"Connections to {this.MMUAccess.AdapterDescriptions.Count} adapters established: " + String.Join(String.Empty, this.MMUAccess.AdapterDescriptions.Select(s => s.Name + " ")));

                //Get all loadable mmus
                List<MMUDescription> loadableMMUs = this.MMUAccess.GetLoadableMMUs();

                //Just load the co-simulator
                if (this.UseRemoteCoSimulation)
                {
                    //Find the MMU with the specified co-simulation name
                    MMUDescription cosim = loadableMMUs.Find(s => s.Name == this.RemoteCoSimulationName);

                    if (cosim == null)
                    {
                        throw new Exception("Remote co-simulator with specified name not available");
                    }

                    //Only load the co-simulation
                    loadableMMUs = new List<MMUDescription>() { cosim };
                }

                //Load the mmus asynchronously
                this.MMUAccess.LoadMMUsAsync(loadableMMUs, TimeSpan.FromSeconds(this.Timeout), this.LoadingCallback);
            }
            else
            {
                this.statusText = "Problem at establishing a connection";
                Debug.LogError("Problem at establishing a connection");
            }
        }


        /// <summary>
        /// Callback for the loading of the MMUs
        /// </summary>
        /// <param name="loaded"></param>
        protected virtual void LoadingCallback(bool loaded)
        {
            if (loaded)
            {
                Debug.Log($"{this.MMUAccess.MotionModelUnits.Count} mmus loaded: " + String.Join(String.Empty, this.MMUAccess.MotionModelUnits.Select(s => s.Description.Name + " ")));

                this.statusText = "Initializing MMUs";

                this.MMUAccess.InitializeMMUsAsync(TimeSpan.FromSeconds(this.Timeout), this.InitializingCallback, this.MAvatar.ID);
            }

            else
            {
                this.statusText = "Problem at loading MMUs";

                Debug.LogError("Problem at loading MMUs");
            }
        }


        /// <summary>
        /// Callback for the initialization of the MMUs
        /// </summary>
        /// <param name="initialized"></param>
        protected virtual void InitializingCallback(bool initialized)
        {
            if (initialized)
            {
                //Create a new instance of the remote cosimulation class
                if (this.UseRemoteCoSimulation)
                    this.CoSimulator = new RemoteCoSimulation(this.MMUAccess.MotionModelUnits.Find(s => s.Name == this.RemoteCoSimulationName), this.MMUAccess.ServiceAccess, this, null);

                //Create the local co-simulator (directly integrated in Unity)
                else
                    this.CoSimulator = new LocalCoSimulation(this.MMUAccess.MotionModelUnits, this.MMUAccess.ServiceAccess, this);


                if (this.AllowRemoteCoSimulationAccess)
                {
                    if(MainThreadDispatcher.Instance == null)
                    {
                        Debug.LogError("Please add a MainTrhead Dispatcher to the scene in order to allow remote co simulation acess");
                        return;
                    }

                    //Must be executed on main thread
                    MainThreadDispatcher.Instance.ExecuteBlocking(() =>
                    {
                        //Get the settings in the parent object
                        MMISettings settings = this.gameObject.GetComponentInParent<MMISettings>();

                        if (settings == null)
                        {
                            Debug.LogError("Please add the MMI settings to the UnitySceneAccess it is required to access the MMIRegisterAddress");
                            return;
                        }

                        try
                        {
                            //Start the remote co-simulation access for the given avatar
                            this.coSimulationAccess = new CoSimulationAccess(this.CoSimulator, new MIPAddress(this.RemoteCoSimulationAccessAddress, this.RemoteCoSimulationAccessPort), new MIPAddress(settings.MMIRegisterAddress, settings.MMIRegisterPort));
                            this.coSimulationAccess.Start();
                            Debug.Log("Started remote co-simulation access");

                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    });
                }
            }

            else
            {
                this.statusText = "Problem at initializing MMUs";
                Debug.LogError("Problem at initializing MMUs");
            }
        }


        /// <summary>
        /// Basic on gui method which is called for each frame
        /// </summary>
        protected virtual void OnGUI()
        {
            ///Display the status text if not initialized
            if (this.MMUAccess == null ||!this.MMUAccess.IsInitialized)
            {
                GUI.Label(new Rect(10, 10, 200, 50), this.statusText);
            }
        }

        /// <summary>
        /// Method is called if the application is quit
        /// </summary>
        protected virtual void OnApplicationQuit()
        {

            //Close the MMUAccess if the application is quit
            if(this.MMUAccess!=null)
                this.MMUAccess.Dispose();

            if (this.coSimulationAccess != null)
                this.coSimulationAccess.Dispose();

            if (this.remoteSkeletonAccessServer != null)
                remoteSkeletonAccessServer.Dispose();
        }


    }
}
