// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Access;
using MMICSharp.Common;
using MMIStandard;
using System;
using System.Collections.Generic;


namespace MMICoSimulation
{
    /// <summary>
    /// Base class for a nested MMU. 
    /// This class can be used if a MMU should be created which calls other MMUs while using a co-simulator.
    /// </summary>
    public class NestedMMUBase : MMUBase
    {
        #region fields

        /// <summary>
        /// Flag specifies whether debug messages are shown
        /// </summary>
        protected bool PrintDebugMessages = true;

        /// <summary>
        /// The utilizied CoSimulator
        /// </summary>
        protected MMICoSimulator coSimulator;

        /// <summary>
        /// Flag which indicates whether the full scene needs to be transferred
        /// </summary>
        private bool transmitFullScene = true;

        /// <summary>
        /// The unique session id
        /// </summary>
        protected readonly string sessionId;

        /// <summary>
        /// The MMU Access
        /// </summary>
        protected MMUAccess mmuAccess;


        /// <summary>
        /// Method for obtaining the priorities
        /// </summary>
        protected Func<Dictionary<string, float>> GetPriorities;

        /// <summary>
        /// Method for checking the end condition
        /// </summary>
        protected Func<MSimulationResult, bool> CheckEndCondition;

        /// <summary>
        /// The assigned instruction
        /// </summary>
        protected MInstruction Instruction;

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        public NestedMMUBase()
        {
            this.Name = "CoSimulation";
            this.sessionId = this.Name + MInstructionFactory.GenerateID();
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

            //Full scene transmission initial required
            this.transmitFullScene = true;

            //Setup the mmu access
            this.mmuAccess = new MMUAccess(this.sessionId)
            {
                AvatarID = avatarDescription.AvatarID,
                SceneAccess = this.SceneAccess
            };

            Console.WriteLine("Try to connect to mmu access...");


            //Connect to mmu access and load mmus
            if (this.mmuAccess.Connect(this.AdapterEndpoint, avatarDescription.AvatarID))
            {
                //Get all loadable MMUs within the current session
                List<MMUDescription> loadableMMUs = this.mmuAccess.GetLoadableMMUs();


                //Select the MMUs which should be loaded
                loadableMMUs = this.SelectMMUsToLoad(loadableMMUs);

                //Create a dictionary for storing the priorities
                Dictionary<string, float> priorities = new Dictionary<string, float>();
                priorities = this.GetPriorities?.Invoke();



                //Select the MMUs to load if explictely specified by the user
                if (properties != null && properties.Count > 0)
                {
                    for (int i = loadableMMUs.Count - 1; i >= 0; i--)
                    {
                        MMUDescription description = loadableMMUs[i];

                        float priority = 1;

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
                }

                Console.WriteLine("Got loadable MMUs:");

                try
                {
                    //Load the relevant MMUs
                    bool success = this.mmuAccess.LoadMMUs(loadableMMUs, TimeSpan.FromSeconds(20));
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


                foreach (MMUDescription description in loadableMMUs)
                {
                    Console.WriteLine(description.Name);
                }


                //Initialize all MMUs
                bool initialized = this.mmuAccess.InitializeMMUs(TimeSpan.FromSeconds(20), avatarDescription.AvatarID);

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

                //Instantiate the cosimulator
                this.coSimulator = new MMICoSimulator(mmuAccess.MotionModelUnits);

                //Set the priorities of the motions
                this.coSimulator.SetPriority(priorities);


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
            this.Instruction = instruction;

            instruction.Instructions = this.CreateSubInstructions(instruction, simulationState);

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

            //Write the events
            if (result.Events != null && PrintDebugMessages)
            {
                foreach (MSimulationEvent ev in result.Events)
                {
                    Console.WriteLine("Event: " + ev.Name + " " + ev.Type + " " + ev.Reference);
                }
            }

            //Check the endcondition
            if (this.CheckEndCondition != null && this.CheckEndCondition(result))
            {
                result.Events.Add(new MSimulationEvent("Finished", mmiConstants.MSimulationEvent_End, this.Instruction.ID));
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
        public override MBoolResponse Dispose(Dictionary<string, string> parameters)
        {
            //Dispose the MMU-Access
            this.mmuAccess.Dispose();

            return this.coSimulator.Dispose(parameters);
        }

        /// <summary>
        /// This method must be overwritten by the child class
        /// </summary>
        /// <param name="availableMMUs"></param>
        /// <returns></returns>
        protected virtual List<MMUDescription> SelectMMUsToLoad(List<MMUDescription> availableMMUs)
        {
            return availableMMUs;
        }

        /// <summary>
        /// This method must be overwritten by the child class
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual List<MInstruction> CreateSubInstructions(MInstruction instruction, MSimulationState state)
        {
            return null;
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
            return this.coSimulator.ExecuteFunction(name, parameters);
        }

        #endregion
    }

}
