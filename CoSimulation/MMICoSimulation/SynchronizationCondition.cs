// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;


namespace MMICoSimulation
{
    /// <summary>
    /// Class represents a condition for 
    /// </summary>
    [Serializable]
    public class SynchronizationCondition
    {
        /// <summary>
        /// The reference (e.g. the dependend motion instruction id)
        /// </summary>
        public string Reference
        {
            get;
            set;
        }

        /// <summary>
        /// Optionally defined time offset [in seconds]
        /// </summary>
        public float TimeOffset
        {
            get;
            set;
        }

        /// <summary>
        /// The specific condition type (condition which should be fulfilled)
        /// </summary>
        public ConditionType ConditionType
        {
            get;
            set;
        } = ConditionType.Event;


        /// <summary>
        /// The name of the event for the condition
        /// </summary>
        public string EventName
        {
            get;
            set;
        }

        /// <summary>
        /// An optional expression
        /// </summary>
        public Func<bool> Expression;



        /// <summary>
        /// Further sub conditions (optional)
        /// </summary>
        public List<SynchronizationCondition> SubConditions = new List<SynchronizationCondition>();

    }

}
