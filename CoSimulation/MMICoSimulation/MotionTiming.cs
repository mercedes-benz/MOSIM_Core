// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Condition type enum
    /// </summary>
    public enum ConditionType
    {
        Event,
        Expression
    }

    #region bml test 

    public class BmlBlock
    {
        /// <summary>
        /// It is generally assumed that the behavior realizer will attempt to realize all behaviors in a block, but if some of the behaviors don’t successfully complete for some reason, 
        /// other behaviors still get carried out (see Feedback and Failure and Fallback).
        /// If there is an all-or-nothing requirement for all or some of the behaviors, they can be enclosed in a<required> block inside the<bml> block.
        /// </summary>
        public bool Required;

        /// <summary>
        /// An identifier that is unique within a specified context (see <bml> and "behavior element")
        /// </summary>
        public string ID;

        public string Name;

        public string Type;

        public string Target;

        public string Subject;


        /// <summary>
        /// The list of synchronization points
        /// </summary>
        public Dictionary<string, SyncRef> SynchronizationPoints = new Dictionary<string, SyncRef>();


        /// <summary>
        /// Optional constraints which have to be fulfilled
        /// </summary>
        public List<BMLConstraint> Constraints;


        /// <summary>
        /// Optional properties
        /// </summary>
        public Dictionary<string, string> Properties;



        public List<BmlBlock> Instructions;
    }

    public class SynchronizationPoint
    {
        public string Action;

        //public List<SyncRef> 
    }
    /// <summary>
    /// Class describes the relative timing of sync points
    /// </summary>
    [Serializable]

    public class SyncRef
    {
        /// <summary>
        /// The id of the syncref
        /// </summary>
        public string ID;

        /// <summary>
        /// Optional time offset
        /// </summary>
        public float TimeOffset = 0f;

        /// <summary>
        /// The Name of the event
        /// </summary>
        public string Event;

        /// <summary>
        /// Returns the bml string ot the syncref point
        /// </summary>
        /// <returns></returns>
        public string GetBMLString()
        {
            if (ID != null)
            {
                if (TimeOffset > 0)
                    return ID + ":" + Event + " + " + TimeOffset;

                else
                    return ID + ":" + Event;
            }

            else
            {
                return TimeOffset.ToString();
            }

        }
    }


    /// <summary>
    /// Class represents a basic bml constraint
    /// </summary>
    public class BMLConstraint
    {
        /// <summary>
        /// Unique id
        /// </summary>
        public string ID;

        /// <summary>
        /// Constraints which have to be fullfilled on order to start the specific instruction
        /// </summary>
        public List<SynchronizationConstraint> SyncConstraints;

        /// <summary>
        /// Constraints which specify sync points which have to be fulfilled after
        /// </summary>
        public List<AfterConstraint> After;

        /// <summary>
        /// Constraints which specify sync points which have to be fulffiled before
        /// </summary>
        public List<BeforeConstraint> Before;
    }


    /// <summary>
    /// <before> constrains one or more sync-points to perform before a specified sync-point notation.
    /// </summary>
    public class BeforeConstraint
    {
        public List<SyncRef> List;
    }

    /// <summary>
    /// <after> constrains one or more sync-points to perform after a specified sync-point notation.
    /// </summary>
    public class AfterConstraint
    {
        public List<SyncRef> List;
    }

    public class SynchronizationConstraint
    {
        public List<SyncRef> List;
    }
    #endregion bml test
}
