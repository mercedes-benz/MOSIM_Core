// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Access;
using MMIStandard;
using System;
using System.Collections.Generic;
using MMICoSimulation;
using MMICSharp.Common;
using MMICoSimulation.Solvers;

namespace CoSimulationMMU
{
    /// <summary>
    /// Implementation of a basic co-simulation contained within an MMU.
    /// For the initialization the priorieties of the motiontypes have to be specified.
    /// Furthermore the motion instructions can have unrestricted boolean expression involving events and offset.
    /// For example a expression such as (test:MotionFinished + 0.5 && test2:MotionFinished) || test:MotionFinished can be interpeted.
    /// </summary>
    public class CoSimulationMMUImpl : MMUBase
    {
        /// <summary>
        /// The utilizied CoSimulator
        /// </summary>
        private MMICoSimulator coSimulator;

        /// <summary>
        /// Flag which indicates whether the full scene needs to be transferred
        /// </summary>
        private bool transmitFullScene = true;

        /// <summary>
        /// The unique session id
        /// </summary>
        private readonly string sessionId;

        /// <summary>
        /// The MMU Access
        /// </summary>
        private MMUAccess mmuAccess;

        private CoSimulationAccess cosimaccess;
        private MIPAddress accessAddress = new MIPAddress("127.0.0.1", 9011);
        private MIPAddress registryAddress;


        /// <summary>
        /// Basic constructor
        /// </summary>
        public CoSimulationMMUImpl(MIPAddress myAddress = null, MIPAddress registryAddress = null)
        {
            this.Name = "CoSimulation";
            this.sessionId = this.Name + MInstructionFactory.GenerateID();

            this.registryAddress = registryAddress;

        }

        /// <summary>
        /// MMU causes problems if initializing multiple times -> To check in future
        /// Basic initialization
        /// For specifying the priorities of the MMUs /motion types the properties can be specified (e.g. {"walk", 1.0}, {"grasp", 2.0})
        /// The listed motion types are also the ones which are loaded. If this porperty is not defined then every MMU is loaded.
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            base.Initialize(avatarDescription, properties);

            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("Initializing co-simulation MMU");

            //Full scene transmission initially required
            this.transmitFullScene = true;

            //Initialize the skeleton access
            MSkeletonAccess.Iface SkeletonAccess = new IntermediateSkeleton();
            SkeletonAccess.InitializeAnthropometry(avatarDescription);

            //Setup the mmu access
            this.mmuAccess = new MMUAccess(this.sessionId)
            {
                SkeletonAccess = SkeletonAccess,
                SceneAccess = this.SceneAccess
            };

            Console.WriteLine("Try to connect to mmu access...");


            //Connect to mmu access and load mmus
            if (this.mmuAccess.Connect(this.AdapterEndpoint, avatarDescription.AvatarID))
            {
                //Get all loadable MMUs within the current session
                List<MMUDescription> loadableMMUs = this.mmuAccess.GetLoadableMMUs();

                //Create a dictionary for storing the priorities
                Dictionary<string, float> priorities = new Dictionary<string, float>();

                //Select the MMUs to load if explictely specified by the user
                if (properties != null && properties.Count > 0)
                {
                    for (int i = loadableMMUs.Count - 1; i >= 0; i--)
                    {                       
                        MMUDescription description = loadableMMUs[i];

                        float priority = 1.0f;

                        //If MMU is listed -> add the priority
                        if (priorities.TryGetValue(description.MotionType, out priority))
                            priorities.Add(description.MotionType, priority);

                        //MMU is not explicetly listed -> remove from loading list
                        else
                            loadableMMUs.RemoveAt(i);
                    }
                }

                //No MMU list defined -> Load all MMUs with same priority (despite the own MMU)
                else
                {
                    //Remove the own MMU -> Avoid recursively instantiating own MMU (unless explictely forced)
                    if (loadableMMUs.Exists(s => s.Name == this.Name))
                    {
                        MMUDescription ownDescription = loadableMMUs.Find(s => s.Name == this.Name);
                        loadableMMUs.Remove(ownDescription);
                    }

                    //Use the default priorities -> hacky
                    priorities = new Dictionary<string, float>()
                    {
                        {"Default", -1 },

                        //Level 0
                        {"Pose", 0 },
                        {"Pose/Idle", 0 },
                        {"idle", 0 },

                        //Level 1
                        {"Locomotion", 1 },
                        {"Locomotion/Walk", 1 },
                        {"Locomotion/Run", 1 },
                        {"Locomotion/Jog", 1 },
                        {"Locomotion/Crouch", 1 },
                        {"Locomotion/Turn", 1 },
                        {"walk", 1 },

                        //Level 2
                        {"Pose/Reach",2},
                        {"positionObject",2},
                        {"releaseObject",2},

                        {"Object/Release",2},
                        {"Object/Carry",2},
                        {"Object/Move",2},
                        {"Object/Turn",2},

                        {"release",2},
                        {"carry",2},
                        {"move",3},
                        {"putDown",2},
                        {"pickupMTM-SD",2},
                        {"turn",2},
                        {"UseTool",2},


                        //Level 3
                        {"Pose/MoveFingers",3},
                        {"moveFingers",3},
                        {"grasp",3},

                        //Level 4
                        {"Pose/Gaze",4},
                        {"Pose/EyeTravel",4},
                    };

                }

                Console.WriteLine("Got loadable MMUs:");

                try
                {
                    //Load the relevant MMUs
                    bool success = this.mmuAccess.LoadMMUs(loadableMMUs, TimeSpan.FromSeconds(10));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error at loading MMUs : " + e.Message + e.StackTrace);

                    return new MBoolResponse(false)
                    {
                         LogData = new List<string>()
                         {
                             e.Message,
                             e.StackTrace
                         }
                    };
                }

                Console.WriteLine("All MMUs successfully loaded");
                Console.WriteLine("------------------------------------------");

                //Print all MMUs
                foreach (MMUDescription description in loadableMMUs)
                    Console.WriteLine(description.Name  + " " + description.MotionType + " " + description.ID);


                //Initialize all MMUs
                bool initialized = this.mmuAccess.InitializeMMUs(TimeSpan.FromSeconds(10), avatarDescription.AvatarID);

                //Instantiate the cosimulator
                this.coSimulator = new MMICoSimulator(mmuAccess.MotionModelUnits)
                {
                    OverwriteSimulationState = true
                };
                //Set the priorities of the motions
                this.coSimulator.SetPriority(priorities);

                //Create and add the solvers (by default we usa an ik solver)
                coSimulator.Solvers = new List<ICoSimulationSolver>
                {
                    new IKSolver(this.ServiceAccess, SkeletonAccess, this.AvatarDescription.AvatarID),
                    new LocalPostureSolver(SkeletonAccess)
                };

                //Record if in debuggin mode
                this.coSimulator.Recording = true;

                if (registryAddress != null)
                {
                    if (this.cosimaccess != null)
                    {
                        this.cosimaccess.Dispose();
                        System.Threading.Thread.Yield();
                        System.Threading.Thread.Sleep(1000);
                        System.Threading.Thread.Yield();
                    }
                    this.cosimaccess = new CoSimulationAccess(this.coSimulator, accessAddress, registryAddress);
                    this.cosimaccess.Start();
                }

                if (!initialized)
                {
                    Console.WriteLine("Problem at initializing MMUs");

                    return new MBoolResponse(false)
                    {
                        LogData = new List<string>()
                         {
                            {"Problem at initializing MMUs" }
                         }
                    };
                }

                return new MBoolResponse(true);
            }

            else
            {
                Console.WriteLine("Connection to MMUAccess/MMIRegister failed");
                return new MBoolResponse(false)
                {
                    LogData = new List<string>() { "Connection to MMUAccess/MMIRegister failed" }
                };
            }
        }


        /// <summary>
        /// Method to assign an instruction to the co-simulation
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            //Co-simulation internally interprets the instruction and the timing
            return this.coSimulator.AssignInstruction(instruction, simulationState);
        }


        /// <summary>
        /// Basic do step call
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            //Transmit the scene (if first frame-> transmit full scene otherwise just deltas)
            this.mmuAccess.PushScene(this.transmitFullScene);

            //Full transmission only required at first frame
            this.transmitFullScene = false;

            //Perform the do step of the co-simulation
            MSimulationResult result = this.coSimulator.DoStep(time, simulationState);

            if (result.Events != null)
            {
                foreach(MSimulationEvent ev in result.Events)
                {
                    Console.WriteLine("Event: " + ev.Name + " " + ev.Type + " " + ev.Reference);
                }
            }

            return result;
        }


        /// <summary>
        /// Abort method which aborts the present task in the co-simulation
        /// </summary>
        /// <returns></returns>
        public override MBoolResponse Abort(string instructionId)
        {
            return this.coSimulator.Abort(instructionId);
        }


        /// <summary>
        /// Method for disposing the MMU
        /// </summary>
        /// <returns></returns>
        public override MBoolResponse Dispose(Dictionary<string,string> parameters)
        {
            //Dispose the MMU-Access
            this.mmuAccess.Dispose();

            if (this.cosimaccess != null)
            {
                this.cosimaccess.Dispose();
                System.Threading.Thread.Yield();
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Yield();
            }

            return this.coSimulator.Dispose(parameters);
        }


        #region further methods just being forwarded to co-simulation

        public override byte[] CreateCheckpoint()
        {
            return this.coSimulator.CreateCheckpoint();
        }

        public override MBoolResponse RestoreCheckpoint(byte[] data)
        {
            return this.coSimulator.RestoreCheckpoint(data);
        }

        public override Dictionary<string, string> ExecuteFunction(string name, Dictionary<string, string> parameters)
        {
            switch (name)
            {
                //Returnst the co-simulation record for debugging
                case "GetRecord":
                    Dictionary<string, string> recordResult = new Dictionary<string, string>();

                    CoSimulationRecord record = this.coSimulator.GetRecord();

                    recordResult.Add("Record", MMICSharp.Common.Communication.Serialization.ToJsonString(record));

                    return recordResult;
            }

            return this.coSimulator.ExecuteFunction(name, parameters);
        }

        #endregion

    }
}
