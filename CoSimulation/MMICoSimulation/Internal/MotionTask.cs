// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMICoSimulation.Internal
{
    /// <summary>
    /// Class to represent a motion task,its properties and all its states
    /// </summary>
    public class MotionTask
    {

        /// <summary>
        /// Unique id of the task
        /// </summary>
        public string ID;

        /// <summary>
        /// Indicates whether the current task is active
        /// </summary>
        public bool IsRunning = false;

        /// <summary>
        /// The MMU Container which encapsulates the MMU instance
        /// </summary>
        public MMUContainer MMUContainer;

        /// <summary>
        /// The corresponding motion instruction
        /// </summary>
        public MInstruction Instruction;

        /// <summary>
        /// All events related to the motion task
        /// </summary>
        public List<System.Tuple<long, MSimulationEvent>> Events = new List<System.Tuple<long, MSimulationEvent>>();

        /// <summary>
        /// The corresponding drawings (optional)
        /// </summary>
        public List<object> Drawings = new List<object>();

        /// <summary>
        /// The timing of the motion
        /// </summary>
        public MotionTiming Timing;

        /// <summary>
        /// The prerequesites which have to be fulfilled in order to start the task
        /// </summary>
        public List<MConstraint> BoundaryConstraints = null;



        public MotionTask()
        {
            this.ID = System.Guid.NewGuid().ToString();
        }


        public MotionTask(SerializableMotionTask data)
        {
            this.Events = data.Events;
            this.Instruction = data.Instruction;
            this.IsRunning = data.IsRunning;
            this.BoundaryConstraints = data.BoundaryConstraints;
            this.Timing = data.Timing;
            this.ID = data.ID;
        }

        public SerializableMotionTask GetAsSerializable()
        {
            return new SerializableMotionTask(this);
        }
    }

    [Serializable]
    public class SerializableMotionTask
    {
        public string ID;

        /// <summary>
        /// Indicates whether the current task is active
        /// </summary>
        public bool IsRunning = false;

        /// <summary>
        /// The MMU Container which encapsulates the MMU instance
        /// </summary>
        public string ContainerID;

        /// <summary>
        /// The corresponding motion instruction
        /// </summary>
        public MInstruction Instruction;

        /// <summary>
        /// All events related to the motion task
        /// </summary>
        public List<Tuple<long, MSimulationEvent>> Events = new List<Tuple<long, MSimulationEvent>>();

        /// <summary>
        /// The timing of the motion
        /// </summary>
        public MotionTiming Timing;

        /// <summary>
        /// The prerequesites which have to be fulfilled in order to start the task
        /// </summary>
        public List<MConstraint> BoundaryConstraints = null;

        public SerializableMotionTask()
        {

        }

        public SerializableMotionTask(MotionTask motionTask)
        {
            this.ID = motionTask.ID;
            this.IsRunning = motionTask.IsRunning;

            //Assign the id if a container is already assigned
            if (motionTask.MMUContainer != null)
                this.ContainerID = motionTask.MMUContainer.ID;

            this.Events = motionTask.Events;
            this.Instruction = motionTask.Instruction;
            this.BoundaryConstraints = motionTask.BoundaryConstraints;
            this.Timing = motionTask.Timing;
        }
    }

}
