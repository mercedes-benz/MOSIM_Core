// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;


namespace MMICoSimulation.Internal
{
    [Serializable]
    public class SerializableCoSimulationState
    {
        /// <summary>
        /// Dictionary holds the transformation of the scene
        /// </summary>
        public Dictionary<string, byte[]> MMUData = new Dictionary<string, byte[]>();

        /// <summary>
        /// Dictionary which containts the relative time of the co-sim simulation at the specific frame
        /// </summary>
        public Dictionary<long, float> FrameTimes = new Dictionary<long, float>();


        /// <summary>
        /// The time of the checkpoint
        /// </summary>
        public float Time;


        /// <summary>
        /// The frame number of the checkpoint
        /// </summary>
        public long FrameNumber = 0;


        //Dictionary to map between the event and the frame(s) in which the event occured
        public Dictionary<MSimulationEvent, List<long>> EventFrameMapping = new Dictionary<MSimulationEvent, List<long>>();



        /// <summary>
        /// The list of all motion tasks known
        /// </summary>
        public Dictionary<string, SerializableMotionTask> MotionTasks = new Dictionary<string, SerializableMotionTask>();

        /// <summary>
        /// Dictionary to store the events on a per frame basis
        /// </summary>
        public Dictionary<long, List<MSimulationEvent>> EventDictionary = new Dictionary<long, List<MSimulationEvent>>();


        /// <summary>
        /// The id of the tasks which are within the buffer
        /// </summary>
        public List<string> TaskBuffer = new List<string>();

        /// <summary>
        /// A list of all instances and their present state
        /// </summary>
        public List<SerializableMMUContainer> MMUContainers = new List<SerializableMMUContainer>();


        public MSimulationState AvatarState;
    }
}
