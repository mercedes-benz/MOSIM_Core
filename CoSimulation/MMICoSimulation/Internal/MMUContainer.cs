// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Access;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMICoSimulation.Internal
{
    /// <summary>
    /// Class wraps the mmu instance and containts various states and control properties
    /// </summary>
    public class MMUContainer
    {

        public string ID;

        /// <summary>
        /// The assigned MMU instance
        /// </summary>
        public IMotionModelUnitAccess MMU;


        /// <summary>
        /// The description file of the MMU
        /// </summary>
        public MMUDescription Description;

        /// <summary>
        /// The priority of the MMU
        /// </summary>
        public float Priority;

        /// <summary>
        /// Indicates whether the MMU is currently active
        /// </summary>
        public bool IsActive = false;

        /// <summary>
        /// The current motion task
        /// </summary>
        public List<MotionTask> CurrentTasks = new List<MotionTask>();


        /// <summary>
        /// Optinal data
        /// </summary>
        public Dictionary<string, object> Data = new Dictionary<string, object>();

        /// <summary>
        /// The history of motion tasks 
        /// </summary>
        public List<MotionTask> History = new List<MotionTask>();


        public List<MSimulationEvent> Events
        {
            get
            {
                List<MSimulationEvent> events = new List<MSimulationEvent>();

                foreach (MotionTask task in History)
                    if (task.Events != null)
                        events.AddRange(task.Events.Select(s => s.Item2));

                if (this.CurrentTasks != null)
                {

                    foreach(MotionTask task in this.CurrentTasks)
                    {
                        if(task.Events !=null)
                            events.AddRange(task.Events.Select(s => s.Item2));

                    }
                }

                return events;
            }
        }


        /// <summary>
        /// The last gathered result
        /// </summary>
        public MSimulationResult LastResult;

        /// <summary>
        /// A list of all results
        /// </summary>
        public Queue<MSimulationResult> LastResults = new Queue<MSimulationResult>();

        public MMUContainer()
        {
            this.ID = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="mmu"></param>
        public MMUContainer(IMotionModelUnitAccess mmu) : this()
        {
            this.MMU = mmu;
            this.Description = mmu.Description;
        }


        /// <summary>
        /// Constructor for a mmu container which is initialized by serialized data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mmus"></param>
        /// <param name="tasks"></param>
        public MMUContainer(SerializableMMUContainer data, List<IMotionModelUnitAccess> mmus, List<MotionTask> tasks)
        {
            this.History = new List<MotionTask>();

            if (data.History != null)
                foreach (string id in data.History)
                {
                    MotionTask task = tasks.Find(s => s.ID == id);

                    if (task != null)
                        this.History.Add(task);
                }


            this.IsActive = data.IsActive;
            this.LastResult = data.LastResult;
            this.LastResults = data.LastResults;
            this.MMU = mmus.Find(s => s.ID == data.MMUID);
            this.Priority = data.Priority;
            this.CurrentTasks = new List<MotionTask>();


            if (tasks != null)
            {
                foreach(string currentTaskId in data.CurrentTaskIDs)
                {
                    MotionTask match = tasks.Find(s => s.ID == currentTaskId);
                    this.CurrentTasks.Add(match);
                }
            }
        }


        /// <summary>
        /// Stores the mmu result within the container
        /// </summary>
        /// <param name="result"></param>
        /// <param name="maxQueueLength"></param>
        public void StoreResult(MSimulationResult result, int maxQueueLength = 3)
        {
            this.LastResult = result;

            this.LastResults.Enqueue(result);

            while (this.LastResults.Count > maxQueueLength)
                this.LastResults.Dequeue();
        }


        public SerializableMMUContainer GetAsSerializable()
        {
            return new SerializableMMUContainer(this);
        }
    }

    [Serializable]
    public class SerializableMMUContainer
    {

        public string ID;


        public string MMUID;

        /// <summary>
        /// The priority of the MMU
        /// </summary>
        public float Priority;

        /// <summary>
        /// Indicates whether the MMU is currently active
        /// </summary>
        public bool IsActive = false;

        /// <summary>
        /// The current motion task
        /// </summary>
        public List<string> CurrentTaskIDs = new List<string>();

        /// <summary>
        /// The history of motion tasks 
        /// </summary>
        public List<string> History = new List<string>();


        /// <summary>
        /// The last gathered result
        /// </summary>
        public MSimulationResult LastResult;


        /// <summary>
        /// A list of all results
        /// </summary>
        public Queue<MSimulationResult> LastResults = new Queue<MSimulationResult>();

        public SerializableMMUContainer()
        {

        }

        public SerializableMMUContainer(MMUContainer container)
        {
            if (container.CurrentTasks != null)
            {
                foreach(MotionTask task in container.CurrentTasks)
                {
                    this.CurrentTaskIDs.Add(task.ID);
                }
            }

            this.ID = container.ID;


            if (container.History != null)
                this.History = container.History.Select(s => s.ID).ToList();
            this.IsActive = container.IsActive;
            this.LastResult = container.LastResult;
            this.LastResults = container.LastResults;
            this.MMUID = container.MMU.ID;
            this.Priority = container.Priority;
        }
    }
}
