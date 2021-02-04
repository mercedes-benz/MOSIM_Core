// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;

namespace MMICoSimulation
{
    /// <summary>
    /// Class contains relevant information for the timings of the motion
    /// </summary>
    [Serializable]
    public class MotionTiming
    {

        /// <summary>
        /// The start condition of the motion
        /// </summary>
        public SynchronizationCondition StartCondition
        {
            get;
            set;
        }

        /// <summary>
        /// The end condition of the motion
        /// </summary>
        public SynchronizationCondition EndCondition
        {
            get;
            set;
        }


        public MotionTiming()
        {

        }


        public MotionTiming(SynchronizationCondition startCondition, SynchronizationCondition endCondition)
        {
            this.StartCondition = startCondition;
            this.EndCondition = endCondition;
        }
    }


    /// <summary>
    /// Condition type enum
    /// </summary>
    public enum ConditionType
    {
        Event,
        Expression
    }

}
