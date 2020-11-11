// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Clients;
using MMICSharp.Services;
using MMIStandard;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;

namespace MMICoSimulation
{
    /// <summary>
    /// Class realizes a remote co-simulation access
    /// </summary>
    public class CoSimulationAccess : MCoSimulationAccess.Iface, IDisposable
    {
        #region protected fields

        /// <summary>
        /// The assigned cosimulator
        /// </summary>
        protected MMICoSimulator coSimulator;

        protected class CoSimulationEventCallbackClient: ClientBase
        {
            public MIPAddress Address;
            /// <summary>
            /// The access of the client
            /// </summary>
            public MCoSimulationEventCallback.Client Access;

            /// <summary>
            /// Basic constructor
            /// </summary>
            /// <param name="address"></param>
            /// <param name="port"></param>
            public CoSimulationEventCallbackClient(MIPAddress address, bool autoStart = true) : base(address.Address, address.Port, autoStart)
            {
                this.Address = address;
            }

            protected override void AssignAccess(TProtocol protocol)
            {
                this.Access = new MCoSimulationEventCallback.Client(protocol);
            }
        }


        /// <summary>
        /// Mutex used for accessing/writing the current set of simulation events
        /// </summary>
        protected Mutex simulationEventMutex = new Mutex();

        /// <summary>
        /// Mutex for the registration of the clients
        /// </summary>
        protected Mutex registrationMutex = new Mutex();


        protected ConcurrentDictionary<int, MCoSimulationEvents> data = new ConcurrentDictionary<int, MCoSimulationEvents>();

        /// <summary>
        /// The events occured in the current frame
        /// </summary>
        protected MCoSimulationEvents currentEvents = new MCoSimulationEvents();

        /// <summary>
        /// To do provide a concurrent data structure
        /// </summary>
        protected Dictionary<string, List<CoSimulationEventCallbackClient>> callbackClients = new Dictionary<string, List<CoSimulationEventCallbackClient>>();

        protected ServiceController controller;

        protected MServiceDescription description = new MServiceDescription()
        {
            Name="coSimulationAccess",
            ID = Guid.NewGuid().ToString(),
            Language ="C#"
        };

        #endregion

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="coSimulator">The instance of the co-simulator</param>
        /// <param name="address">The address where the CoSimulationAccess should be hosted</param>
        /// <param name="registerAddress">The address of the register</param>
        public CoSimulationAccess(MMICoSimulator coSimulator, MIPAddress address, MIPAddress registerAddress)
        {
            //Assign the co simulator
            this.coSimulator = coSimulator;

            //Register at on result event
            this.coSimulator.OnResult += CoSimulator_OnResult;

            //Add the address to the description
            this.description.Addresses = new List<MIPAddress>() { address };

            //Create a new service controller
            this.controller = new ServiceController(this.description, registerAddress, new MCoSimulationAccess.Processor(this));
        }

        public void Start()
        {
            //Start asynchronously
            this.controller.StartAsync();
        }


        /// <summary>
        /// Disposes the controller/server
        /// </summary>
        public void Dispose()
        {
            //Unregister at event
            if (this.coSimulator != null)
                this.coSimulator.OnResult -= CoSimulator_OnResult;

            //Dispose the controller
            if (this.controller !=null)
                this.controller.Dispose();
        }


        //Callback which is called for each frame
        private void CoSimulator_OnResult(object sender, MSimulationResult e)
        {
            //Aquire the mutex during the writing of the new results
            this.simulationEventMutex.WaitOne();

            currentEvents.Events = new List<MSimulationEvent>(e.Events);
            currentEvents.FrameNumber = (int)this.coSimulator.FrameNumber;
            currentEvents.SimulationTime = this.coSimulator.Time;
            MAvatarPostureValues postureValues = e.Posture.Copy();

            //Add the data
            data.TryAdd(currentEvents.FrameNumber, currentEvents);

            this.simulationEventMutex.ReleaseMutex();


            //Perform in a new thread
            Task.Run(() =>
            {
                //Handle each event type which is registered
                foreach(var entry in callbackClients)
                {
                    //Handle each client
                    foreach (CoSimulationEventCallbackClient client in entry.Value)
                    {

                        if(entry.Key == "OnFrameEnd")
                        {
                            client.Access.OnFrameEnd(postureValues);
                        }

                        //Check if events available
                        if(currentEvents.Events!= null)
                        {
                            var relevantEvents = currentEvents.Events.Where(s => s.Type == entry.Key).ToList();

                            if (relevantEvents.Count > 0)
                            {
                                client.Access.OnEvent(new MCoSimulationEvents(relevantEvents, currentEvents.SimulationTime, currentEvents.FrameNumber));
                            }
                        }
                    }

                }
            });
        }



        //Callback for assigning an instruction to the co simulator
        public MBoolResponse AssignInstruction(MInstruction instruction, Dictionary<string, string> properties)
        {
            //Directly call the co-simulator
            return this.coSimulator.AssignInstruction(instruction, null);
        }


        /// <summary>
        /// Returns the events of the current frame
        /// </summary>
        /// <returns></returns>
        public MCoSimulationEvents GetCurrentEvents()
        {
            return this.currentEvents;
        }

        /// <summary>
        /// Returns all events of the specific type
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public List<MCoSimulationEvents> GetHistory(string eventType)
        {
            return this.data.Values.Where(s => s.Events.Exists(x => x.Type == eventType)).ToList();
        }


        /// <summary>
        /// Returns the simulation events of the specified evnt type occured within the given frames
        /// </summary>
        /// <param name="fromFrame"></param>
        /// <param name="toFrame"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public List<MCoSimulationEvents> GetHistoryFromFrames(int fromFrame, int toFrame, string eventType)
        {
            return this.data.Values.Where(s => s.FrameNumber >= fromFrame && s.FrameNumber < toFrame &&  s.Events.Exists(x => x.Type == eventType)).ToList();
        }


        /// <summary>
        /// Returns the simulation events of the specified evnt type occured within the given timespan
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public List<MCoSimulationEvents> GetHistoryFromTime(double startTime, double endTime, string eventType)
        {
            return this.data.Values.Where(s => s.SimulationTime >= startTime && s.SimulationTime < endTime && s.Events.Exists(x => x.Type == eventType)).ToList();
        }


        /// <summary>
        /// Unregisters at a specific event given the event type (e.g. simulation end).
        /// The given clientAddress is used to provide an event based communication.
        /// </summary>
        /// <param name="clientAddress"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public MBoolResponse RegisterAtEvent(MIPAddress clientAddress, string eventType)
        {
            this.registrationMutex.WaitOne();

            if (!this.callbackClients.ContainsKey(eventType))
                this.callbackClients.Add(eventType, new List<CoSimulationEventCallbackClient>());

            this.callbackClients[eventType].Add(new CoSimulationEventCallbackClient(clientAddress));

            this.registrationMutex.ReleaseMutex();


            return new MBoolResponse(true);
        }

        /// <summary>
        /// Unregisters at a specific event given the event type (e.g. simulation end)
        /// </summary>
        /// <param name="clientAddress"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public MBoolResponse UnregisterAtEvent(MIPAddress clientAddress, string eventType)
        {
            if (this.callbackClients.ContainsKey(eventType))
            {
                //Remove the client with the respective address
                for(int i= this.callbackClients[eventType].Count - 1; i >= 0; i--)
                {
                    var client = this.callbackClients[eventType][i];

                    if(client.Address.Address == clientAddress.Address && client.Address.Port == clientAddress.Port)
                    {
                        this.registrationMutex.WaitOne();

                        client.Dispose();
                        this.callbackClients[eventType].RemoveAt(i);

                        this.registrationMutex.ReleaseMutex();
                    }
                }
            }

            return new MBoolResponse(true);
        }

        /// <summary>
        /// Aborts all instructions
        /// </summary>
        /// <returns></returns>
        public MBoolResponse Abort()
        {
            return this.coSimulator.Abort();
        }

        /// <summary>
        /// Aborts a single instruction
        /// </summary>
        /// <param name="instructionID"></param>
        /// <returns></returns>
        public MBoolResponse AbortInstruction(string instructionID)
        {
            return this.coSimulator.Abort(instructionID);
        }

        /// <summary>
        /// Aborts multiple instructions given by the id
        /// </summary>
        /// <param name="instructionIDs"></param>
        /// <returns></returns>
        public MBoolResponse AbortInstructions(List<string> instructionIDs)
        {
            foreach(string instructionID in instructionIDs)
                this.coSimulator.Abort(instructionID);

            return new MBoolResponse(true);
        }

        /// <summary>
        /// Returns the present status (required by MMIServiceBase)
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetStatus()
        {
            return new Dictionary<string, string>()
            {
                { "Running", true.ToString()}
            };
        }

        /// <summary>
        /// Returns the description of the co simulation access
        /// </summary>
        /// <returns></returns>
        public MServiceDescription GetDescription()
        {
            return this.description;
        }

        /// <summary>
        /// Basic setup routine -> Nothing to do in here
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual MBoolResponse Setup(MAvatarDescription avatar, Dictionary<string, string> properties)
        {
            return new MBoolResponse(true);
        }

        /// <summary>
        /// Generic consume method as required by MMIServiceBase -> Nothing to do in here
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> Consume(Dictionary<string, string> properties)
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Basic dispose method that is remotely called
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public MBoolResponse Dispose(Dictionary<string, string> properties)
        {
            //Dispose the current service
            this.Dispose();

            return new MBoolResponse(true);
        }

        /// <summary>
        /// Restart function being remotely called -> tbd
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public MBoolResponse Restart(Dictionary<string, string> properties)
        {
            return new MBoolResponse(false);
        }
    }
}
