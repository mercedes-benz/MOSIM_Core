// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;
using MMIStandard;
using MMICSharp.Common;
using MMICSharp.Common.Tools;

namespace MMICSharp.Access.Abstraction
{

    /// <summary>
    /// Representation of a MMU instance (in the consumer view of the abstraction)
    /// </summary>
    public class MotionModelUnitAccess : IMotionModelUnitAccess
    {

        #region private variables
        /// <summary>
        /// The assigned MMU access
        /// </summary>
        private readonly MMUAccess mmuAccess;

        /// <summary>
        /// The assigned sessionId
        /// </summary>
        private readonly string sessionId;


        /// <summary>
        /// A client to communicate with the adapters
        /// </summary>
        private IAdapterClient adapterClient;

        #endregion


        #region public properties

        /// <summary>
        /// The assigned adapter
        /// </summary>
        internal IAdapter Adapter;


        /// <summary>
        /// The ID of the MMU
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the MMU
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The motion type of the MMU
        /// </summary>
        public string MotionType
        {
            get;
            set;
        }

        public MMUDescription Description
        {
            get;
            set;
        }


        #endregion


        /// <summary>
        /// Basic constructor of a (remote) MMU
        /// </summary>
        /// <param name="mmuAccess"></param>
        /// <param name="adapter"></param>
        /// <param name="sessionId"></param>
        /// <param name="description"></param>
        public MotionModelUnitAccess(MMUAccess mmuAccess, IAdapter adapter, string sessionId, MMUDescription description)
        {
            this.Adapter = adapter;

            //Assign all variables
            this.Description = description;
            this.sessionId = sessionId;
            this.MotionType = description.MotionType;
            this.mmuAccess = mmuAccess;
            this.Name = description.Name;
            this.ID = description.ID;

            //Create a new client for the MMU
            this.adapterClient = adapter.CreateClient();
        }


        /// <summary>
        /// Changes the adapter of the MMU
        /// </summary>
        /// <param name="newAdapter"></param>
        public void ChangeAdapter(IAdapter newAdapter)
        {
            if(this.adapterClient != null)
            {
                this.adapterClient.Dispose();
            }

            this.adapterClient = newAdapter.CreateClient();
        }


        /// <summary>
        /// Initialization function in which the MMU is initialized (called remote)
        /// </summary>
        public MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string,string> properties)
        {
            //Call the remote MMU
            return this.adapterClient.Access.Initialize(avatarDescription, new Dictionary<string, string>(), this.ID, this.sessionId);       
        }

        /// <summary>
        /// Calls the default method 
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState simulationState)
        {
            return this.adapterClient.Access.AssignInstruction(instruction, simulationState, this.ID, this.sessionId);
        }

        /// <summary>
        /// Calls the default method
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            return this.adapterClient.Access.DoStep(time, simulationState, this.ID, this.sessionId);
        }


        /// <summary>
        /// Method fetches the prerequisites which are required to execute the motion instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public List<MConstraint> GetBoundaryConstraints(MInstruction instruction)
        {
            return this.adapterClient.Access.GetBoundaryConstraints(instruction, this.ID, this.sessionId);
        }


        /// <summary>
        /// Forces to finish the current execution
        /// </summary>
        public MBoolResponse Abort(string instructionID)
        {
            return this.adapterClient.Access.Abort(instructionID,this.ID, this.mmuAccess.SessionId);
        }




        /// <summary>
        /// Returns the ID of the MMU (uses local variable)
        /// </summary>
        /// <returns></returns>
        public string GetID()
        {
            return this.ID;
        }

        /// <summary>
        /// Returns the motion type of the MMU
        /// </summary>
        /// <returns></returns>
        public string GetMotionType()
        {
            return this.MotionType;
        }



        /// <summary>
        /// Disposes the MMU
        /// </summary>
        public MBoolResponse Dispose(Dictionary<string,string> parameters)
        {
            return this.adapterClient.Access.Dispose(this.ID, this.sessionId);
        }


        /// <summary>
        /// Nothing to do in here
        /// </summary>
        /// <returns></returns>
        public byte[] CreateCheckpoint()
        {
            return this.adapterClient.Access.CreateCheckpoint(this.ID, this.sessionId);
        }

        /// <summary>
        /// Nothing to do in here
        /// </summary>
        /// <param name="data"></param>
        public MBoolResponse RestoreCheckpoint(byte[] data)
        {
            return this.adapterClient.Access.RestoreCheckpoint(this.ID, this.sessionId, data);
        }

        public MBoolResponse CheckPrerequisites(MInstruction instruction)
        {
            return this.adapterClient.Access.CheckPrerequisites(instruction, this.ID, this.sessionId);
        }

        public Dictionary<string, string> ExecuteFunction(string name, Dictionary<string, string> parameters)
        {
            return this.adapterClient.Access.ExecuteFunction(name,parameters, this.ID, this.sessionId);
        }

        /// <summary>
        /// Closes the connection of the adapter client
        /// </summary>
        internal void CloseConnection()
        {
            this.adapterClient.Dispose();
        }


    }
}
