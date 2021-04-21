// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Janis Sprenger

using MMICoSimulation.Internal;
using MMICoSimulation.Solvers;
using MMICSharp.Access;
using MMICSharp.Common.Communication;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace MMICoSimulation
{
    /// <summary>
    /// A basic implementation of a co-simulation for the MMI framework
    /// </summary>
    public class MMICoSimulator : IMotionModelUnitAccess
    {
        #region protected variables

        /// <summary>
        /// Watch for time measurements
        /// </summary>
        protected System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// The assigned avatar description
        /// </summary>
        protected MAvatarDescription avatarDescription;

        /// <summary>
        /// Dictionary to store the events on a per frame basis
        /// </summary>
        protected Dictionary<long, List<MSimulationEvent>> eventDictionary = new Dictionary<long, List<MSimulationEvent>>();

        /// <summary>
        /// Dictionary to map between the event and the frame(s) in which the event occured
        /// </summary>
        protected Dictionary<MSimulationEvent, List<long>> eventFrameMapping = new Dictionary<MSimulationEvent, List<long>>();

        /// <summary>
        /// Dictionary which containts the relative time of the co-sim simulation at the specific frame
        /// </summary>
        protected Dictionary<long, float> frameTimes = new Dictionary<long, float>();

        /// <summary>
        /// A list of all instances
        /// </summary>
        protected List<MMUContainer> mmuContainers = new List<MMUContainer>();

        /// <summary>
        /// The buffer of tasks which should be executed
        /// </summary>
        protected List<MotionTask> taskBuffer = new List<MotionTask>();

        /// <summary>
        /// Instruction ids which should be aborted in the current frame
        /// </summary>
        protected List<string> toAbort = new List<string>();


        /// <summary>
        /// The ids which should be aborted
        /// </summary>
        protected List<string> toStart = new List<string>();


        //The simulation state -> This information is provided to each MMU and contains the initial, current posture as well as constraints, scene manipulations and events.
        //The constraints are active below the frame duration.
        protected MSimulationState SimulationState {
            get;
            set;
        } = new MSimulationState()
        {
            Constraints = new List<MConstraint>(),
            Events = new List<MSimulationEvent>(),
            SceneManipulations = new List<MSceneManipulation>()
        };

        /// <summary>
        /// The priority list of the mmus
        /// </summary>
        protected Dictionary<string, float> priorities = new Dictionary<string, float>();

        /// <summary>
        /// The mmu instances provided by the superior instance
        /// </summary>
        protected List<IMotionModelUnitAccess> mmuInstances;

        /// <summary>
        /// List contains all events of the current frame which have been created by the CoSimulation.
        /// The list is cleared after the end of each frame.
        /// </summary>
        protected List<MSimulationEvent> coSimulationEvents = new List<MSimulationEvent>();

        /// <summary>
        /// Access to the scene
        /// </summary>
        protected MSceneAccess.Iface sceneAccess;

        /// <summary>
        /// History of CoSimulationFrames
        /// </summary>
        protected CoSimulationRecord record = new CoSimulationRecord();

        #endregion

        #region public fields

        /// <summary>
        /// Flag specifies whether the co-simulation logs the computation times of the MMUs and co-simulation solvers
        /// </summary>
        public bool LogTimes = true;

        /// <summary>
        /// Flag specified whether the individual frames should be stored for later utilization
        /// </summary>
        public bool Recording = false;

        /// <summary>
        /// List of utilized solvers for the merging process 
        /// </summary>
        public List<ICoSimulationSolver> Solvers = new List<ICoSimulationSolver>();

        /// <summary>
        /// Is called at the end of the frame
        /// </summary>
        public virtual event EventHandler<MSimulationResult> OnResult;

        /// <summary>
        /// Event handler which provides all MMU events
        /// </summary>
        public virtual event EventHandler<MSimulationEvent> MSimulationEventHandler;

        /// <summary>
        /// Event handler which raises new events if all tasks have been finished
        /// </summary>
        public virtual event EventHandler<EventArgs> AllTasksFinished;


        /// <summary>
        /// Event handler for providing co-simulation logs and error
        /// </summary>
        public virtual event EventHandler<CoSimulationLogEvent> LogEventHandler;


        /// <summary>
        /// The current Time of the CoSimulation
        /// </summary>
        public float Time
        {
            get;
            protected set;
        }

        /// <summary>
        /// The current frame number
        /// </summary>
        public long FrameNumber
        {
            get;
            protected set;
        } = 0;


        /// <summary>
        /// The ID of the co-simulator
        /// </summary>
        public string ID
        {
            get;
            set;
        }


        /// <summary>
        /// The name of the co-simulator
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// If enabled the co-simulation solves the constraints after each do step call of the MMU.
        /// By default this is disabled.
        /// </summary>
        public bool HierachicalSolving
        {
            get;
            set;
        } = false;


        /// <summary>
        /// Specifies whether the optimization stage is enabled.
        /// By default this is enabled.
        /// </summary>
        public bool OptimizationStage
        {
            get;
            set;
        } = true;

        /// <summary>
        /// The Description of the co-simulation in terms of MMUDescription
        /// </summary>
        public MMUDescription Description
        {
            get;
            set;
        }

        /// <summary>
        /// Flag specifies whether the simulation state is overwritten by externally specified MSimulationStates occuring in AssignInstruction and Initialize
        /// </summary>
        public bool OverwriteSimulationState
        {
            get;
            set;
        } = false;


        /// <summary>
        /// Flag specifies whether the instruction is validated first within the AssignInstruction before being actually applied
        /// </summary>
        public bool ValidateInstructions
        {
            get;
            set;
        } = false;

        #endregion

        /// <summary>
        /// Basic constructor
        /// </summary>
        ///<param name="mmus">A list of the MMU instances considered for co-simulation.</param>
        ///<param name="sceneAccess">The provided scene access.</param>
        public MMICoSimulator(List<IMotionModelUnitAccess> mmus, MSceneAccess.Iface sceneAccess = null)
        {
            //Assign the instances and the scene access
            this.mmuInstances = mmus;
            this.sceneAccess = sceneAccess;

            //Add the mmu instances
            foreach (IMotionModelUnitAccess mmu in mmus)
            {
                //Extract the priorities (if defined)
                float priority = 1;
                if (!this.priorities.TryGetValue(mmu.Description.MotionType, out priority))
                    priority = 1;

                //Add a new MMUContainer which contains the actual mmu instance
                this.mmuContainers.Add(new MMUContainer(mmu)
                {
                    IsActive = false,
                    Priority = priority
                });
            }

            //Sort according to priorities
            this.SortMMUPriority();
        }


        #region accessing functionality

        /// <summary>
        /// Returns the stored frames (only recorded if StoreFrames == true)
        /// </summary>
        /// <returns></returns>
        public CoSimulationRecord GetRecord()
        {
            return this.record;
        }


        /// <summary>
        /// Returns the presently active instructions
        /// </summary>
        /// <returns></returns>
        public List<MInstruction> GetActiveInstructions()
        {
            List<MInstruction> instructions = new List<MInstruction>();

            foreach (MMUContainer activeMMU in this.mmuContainers.Where(s => s.IsActive))
            {
                foreach (var task in activeMMU.CurrentTasks)
                {
                    if (task.IsRunning)
                        instructions.Add(task.Instruction);
                }
            }

            return instructions;
        }

        /// <summary>
        /// Returns the upcoming motion instructions
        /// </summary>
        /// <returns></returns>
        public List<MInstruction> GetUpcomingInstructions()
        {
            return this.taskBuffer.Select(s => s.Instruction).ToList();
        }


        /// <summary>
        /// Returns the presently active instructions
        /// </summary>
        /// <param name="motionType">The desired motion type</param>
        /// <param name="parameters">Parameters that must be specified in the instruction</param>
        /// <returns></returns>
        public List<MInstruction> GetActiveInstructionsByMotionType(string motionType, params Tuple<string, string>[] parameters)
        {
            List<MInstruction> instructions = new List<MInstruction>();

            foreach (MMUContainer activeMMU in this.mmuContainers.Where(s => s.IsActive))
            {
                foreach (var task in activeMMU.CurrentTasks)
                {
                    if (task.IsRunning && task.Instruction.MotionType == motionType)
                    {
                        bool matching = true;
                        if (parameters != null)
                        {
                            foreach (var entry in parameters)
                            {
                                if (task.Instruction.Properties != null && task.Instruction.Properties.ContainsKey(entry.Item1) && task.Instruction.Properties[entry.Item1] == entry.Item2)
                                {
                                    //Valid
                                }

                                else
                                {
                                    //Not matching the requirements
                                    matching = false;
                                    break;
                                }
                            }
                        }

                        //Only add if matching
                        if (matching) 
                            instructions.Add(task.Instruction);
                    }
                }
            }

            return instructions;
        }


        /// <summary>
        /// Returns the presently active instructions
        /// </summary>
        /// <param name="motionType">The desired motion type</param>
        /// <param name="keys">The keys that must be specified in the instruction</param>
        /// <returns></returns>
        public List<MInstruction> GetActiveInstructionsByMotionType(string motionType, params string [] keys)
        {
            List<MInstruction> instructions = new List<MInstruction>();

            foreach (MMUContainer activeMMU in this.mmuContainers.Where(s => s.IsActive))
            {
                foreach (var task in activeMMU.CurrentTasks)
                {
                    if (task.IsRunning && task.Instruction.MotionType == motionType)
                    {
                        bool matching = true;
                        if (keys != null)
                        {
                            foreach (string key in keys)
                            {
                                if (task.Instruction.Properties != null && task.Instruction.Properties.ContainsKey(key))
                                {
                                    //Valid
                                }

                                else
                                {
                                    //Not matching the requirements
                                    matching = false;
                                    break;
                                }
                            }
                        }

                        //Only add if matching
                        if (matching)
                            instructions.Add(task.Instruction);
                    }
                }
            }

            return instructions;
        }
        /// <summary>
        /// Returns a deep copy of the priorities used in the cosimulator 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,float> GetPriorities()
        {
            Dictionary<string, float> clone = new Dictionary<string, float>();

            foreach(var entry in this.priorities)
                clone.Add(entry.Key, entry.Value);

            return clone;
        }

        #endregion


        /// <summary>
        /// Sets the MMU priorities
        /// </summary>
        /// <param name="priorities"></param>
        public void SetPriority(Dictionary<string, float> priorities)
        {
            this.priorities = priorities;
            this.SortMMUPriority();
        }

        /// <summary>
        /// Sets a single priority for the motion type
        /// </summary>
        /// <param name="motionType"></param>
        /// <param name="priorityValue"></param>
        public void SetPriority(string motionType, float priorityValue)
        {
            if (this.priorities.ContainsKey(motionType))
                this.priorities[motionType] = priorityValue;
            else
                this.priorities.Add(motionType, priorityValue);

            this.SortMMUPriority();
        }



        /// <summary>
        /// Method resets the current record
        /// </summary>
        public void ResetRecord()
        {
            this.record.Instructions.Clear();
            this.record.Frames.Clear();
        }


        /// <summary>
        /// Functionality if provided based on the MMU interface
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties">All MMus(motion types) which should be loaded as key + priority as value</param>
        public virtual MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            this.avatarDescription = avatarDescription;

            //Set the priorities if defined
            if (properties != null)
            {
                this.priorities = new Dictionary<string, float>();

                foreach (KeyValuePair<string, string> tuple in properties)
                {
                    MMUDescription mmuDescription = this.mmuInstances.Select(s => s.Description).ToList().Find(s => s.MotionType == tuple.Key);

                    if (mmuDescription != null)
                    {
                        this.priorities.Add(mmuDescription.MotionType, float.Parse(tuple.Value, CultureInfo.InvariantCulture));
                    }
                }

                //Sort according to priority
                this.SortMMUPriority();
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// MMU conform instruction assignment. The co-simulation automatically tries to interpret the start and endconditions if defined.
        /// Event related expression such as (test:MotionFinished + 0.5 && test2:MotionFinished) || test:MotionFinished can be interpeted.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="avatarState"></param>
        public virtual MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState avatarState)
        {
            //One instruction might contain multiple sub-instructions -> therefore consider all instruction (if sub-instructions are defined)
            if(instruction.Instructions != null && instruction.Instructions.Count> 0)
            {
                //Assign each instruction and call recursively
                foreach(MInstruction subInstruction in instruction.Instructions)
                    this.AssignInstruction(subInstruction, avatarState);
            }

            //Optionally validate the instructions
            if (this.ValidateInstructions)
            {
                MMICSharp.Common.Tools.InstructionValidation instructionValidator = new MMICSharp.Common.Tools.InstructionValidation();
                MBoolResponse validationResult = instructionValidator.Validate(instruction, this.mmuContainers.Select(s => s.Description).ToList());


                //Create a new event for logging
                this.LogEventHandler?.Invoke(this, new CoSimulationLogEvent("Instruction is not valid: " + instruction.Name + " " + instruction.MotionType));

                if (!validationResult.Successful)
                    return validationResult;
            }

            //Check if start or endconditions are defined
            if (instruction.StartCondition != null || instruction.EndCondition != null)
            {
                //Create a new motion timing which can be handled by the co-simulation
                MotionTiming timing = new MotionTiming();

                //Check if a start condition is provided
                if (instruction.StartCondition != null)
                {
                    //Create an expression for checking the start condition
                    timing.StartCondition = new SynchronizationCondition
                    {
                        //Change the condition type to expression
                        ConditionType = ConditionType.Expression,

                        //Expression delegate which is directly interpreted by the co-simulation (check if the event is already fired and offset fulfilled)
                        Expression = () =>
                        {
                            return EvaluateExpression(instruction.StartCondition, this.EvaluateTimingExpression);
                        }
                    };
                }

                //Check if an end condition is provided
                if (instruction.EndCondition != null)
                {
                    //Create an expression for checking the end-condition
                    timing.EndCondition = new SynchronizationCondition
                    {
                        //Change the condition type to expression
                        ConditionType = ConditionType.Expression,

                        //Expression delegate which is directly interpreted by the co-simulation (check if the event is already fired and offset fulfilled)
                        Expression = () =>
                        {
                            return EvaluateExpression(instruction.EndCondition, this.EvaluateTimingExpression);
                        }
                    };
                }

                //Add the instruction with the explicit timing
                this.AddInstruction(instruction, timing);
            }
            else
            {
                //Add the instruction without any further timing
                this.AddInstruction(instruction);
            }

            if (this.OverwriteSimulationState && avatarState != null)
            {
                //Assign the initial and current posture -> Avoid overwriting the constraints
                this.SimulationState.Initial = avatarState.Initial;
                this.SimulationState.Current = avatarState.Current;
            }

            //Return true
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Do step in analogy to MMU.
        /// Comprises pre-compute, compute and post-compute.
        /// For parallelization and performance optimized utilization it is recommended to use pre and post-compute frame separately.
        /// </summary>
        /// <param name="time">The delta time for simulation</param>
        /// <param name="avatarState">The input avatar state for the co-simulation. If set to null it is ignored.</param>
        /// <returns></returns>
        public virtual MSimulationResult DoStep(double time, MSimulationState avatarState)
        {
            //Assign the avatar state if defined
            if (this.OverwriteSimulationState && avatarState != null)
            {
                this.SimulationState.Initial = avatarState.Initial;
                this.SimulationState.Current = avatarState.Current;
            }

            //Perform precomputation operations
            this.PreComputeFrame();

            //Compute the actual frame using the given timespan
            MSimulationResult result = this.ComputeFrame((float)time);

            //Raise event
            this.OnResult?.Invoke(this,result);

            //Perform the post comptuing
            this.PostComputeFrame(result);

            //Return the co-simulated result of the present frame
            return result;
        }


        /// <summary>
        /// Aborts the current co-simulation process
        /// </summary>
        /// <param name="instructionId">The id of the instruction that should be aborted (in case of null, all tasks are aborted)</param>
        public virtual MBoolResponse Abort(string instructionId = null)
        {
            //Terminate all tasks if instruction id undefined or "-1"
            if (instructionId == null || instructionId == "-1")
            {
                //Clear the task buffer
                this.taskBuffer.Clear();

                //Terminate each mmucontainer
                foreach (MMUContainer mmuContainer in this.mmuContainers)
                {
                    if (mmuContainer == null)
                        continue;

                    //Provide abort for each subInstruction
                    foreach (MotionTask task in mmuContainer.CurrentTasks)
                    {
                        if (task.IsRunning)
                        {
                            //Add aborted event
                            MSimulationEvent simEvent = new MSimulationEvent("Aborted", mmiConstants.MSimulationEvent_Abort, task.Instruction.ID);
                            coSimulationEvents.Add(simEvent);
                            this.MSimulationEventHandler?.Invoke(this, simEvent);
                            this.toAbort.Add(task.Instruction.ID);
                        }
                    }
                }
            }
            //Just terminate the specific task
            else
            {
                this.toAbort.Add(instructionId);
            }

            //Clear all constraints
            this.SimulationState.Constraints = new List<MConstraint>();

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Aborts instruction by motion type
        /// </summary>
        /// <param name="motionType"></param>
        /// <returns></returns>
        public virtual MBoolResponse AbortByMotionType(string motionType)
        {
            //Terminate each mmucontainer
            foreach (MMUContainer mmuContainer in this.mmuContainers)
            {
                if (mmuContainer == null)
                    continue;

                //Provide abort for each subInstruction
                foreach (MotionTask task in mmuContainer.CurrentTasks)
                {
                    //Check whether the motion type matches
                    if (task.IsRunning && task.Instruction.MotionType == motionType)
                    {
                        //Add aborted event
                        MSimulationEvent simEvent = new MSimulationEvent("Aborted", mmiConstants.MSimulationEvent_Abort, task.Instruction.ID);
                        coSimulationEvents.Add(simEvent);
                        this.MSimulationEventHandler?.Invoke(this, simEvent);
                        this.toAbort.Add(task.Instruction.ID);
                    }
                }
            }

            return new MBoolResponse(true);
        }


        /// <summary>
        /// Pauses an instruction given the defined instruction id.
        /// Currently pausing an instruction will detach the corresponding MMU from the update routine/do step.
        /// </summary>
        /// <param name="instructionID"></param>
        /// <returns></returns>
        public virtual MBoolResponse PauseInstruction(string instructionID)
        {
            //Find the corresponding MMU container
            MMUContainer match = this.mmuContainers.Find(s => s.CurrentTasks.Exists(x => x.ID == instructionID));

            //Return false if no instruction is available
            if (match == null)
                return new MBoolResponse(false);

            //Set to paused
            match.IsPaused = true;

            //Return true
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Resumes a paused instruction given the defined instruction id
        /// </summary>
        /// <param name="instructionID"></param>
        /// <returns></returns>
        public virtual MBoolResponse ResumeInstruction(string instructionID)
        {
            //Find the corresponding MMU container
            MMUContainer match = this.mmuContainers.Find(s => s.CurrentTasks.Exists(x => x.ID == instructionID));

            //Return false if no instruction is available
            if (match == null)
                return new MBoolResponse(false);

            //Set paused to false
            match.IsPaused = false;

            //Return true
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Methods overwrites an endcondition for a given instruction
        /// </summary>
        /// <param name="instructionID"></param>
        /// <param name="endCondition"></param>
        /// <returns></returns>
        public virtual bool OverwriteEndCondition(string instructionID, string endCondition)
        {
            MotionTask task = null;

            //First of all search in the current task buffer
            if (this.taskBuffer.Exists(s => s.Instruction.ID == instructionID))
                task = this.taskBuffer.Find(s => s.Instruction.ID == instructionID);

            else
                task = this.FindMotionTask(instructionID);


            //Set the endcondition if task available
            if (task != null)
            {
                if (task.Timing == null)
                    task.Timing = new MotionTiming();

                task.Instruction.EndCondition = endCondition;

                //Create an expression for checking the end-condition
                task.Timing.EndCondition = new SynchronizationCondition
                {
                    //Change the condition type to expression
                    ConditionType = ConditionType.Expression,

                    //Expression delegate which is directly interpreted by the co-simulation (check if the event is already fired and offset fulfilled)
                    Expression = () =>
                    {
                        return EvaluateExpression(endCondition, this.EvaluateTimingExpression);
                    }
                };

                return true;
            }

            return false;
        }


        /// <summary>
        /// Methods overwrites a start condition for a given instruction
        /// </summary>
        /// <param name="instructionID"></param>
        /// <param name="endCondition"></param>
        /// <returns></returns>
        public virtual bool OverwriteStartCondition(string instructionID, string startCondition)
        {
            MotionTask task = null;

            //First of all search in the current task buffer
            if (this.taskBuffer.Exists(s => s.Instruction.ID == instructionID))
                task = this.taskBuffer.Find(s => s.Instruction.ID == instructionID);

            else
                task = this.FindMotionTask(instructionID);


            //Set the endcondition if task available
            if (task != null)
            {
                if (task.Timing == null)
                    task.Timing = new MotionTiming();

                task.Instruction.StartCondition = startCondition;

                //Create an expression for checking the end-condition
                task.Timing.StartCondition = new SynchronizationCondition
                {
                    //Change the condition type to expression
                    ConditionType = ConditionType.Expression,

                    //Expression delegate which is directly interpreted by the co-simulation (check if the event is already fired and offset fulfilled)
                    Expression = () =>
                    {
                        return EvaluateExpression(startCondition, this.EvaluateTimingExpression);
                    }
                };

                return true;
            }

            return false;
        }


        /// <summary>
        /// Method is exceucted before the compute frame. 
        /// In main thread -> Has not to be thread-safe
        /// </summary>
        public virtual void PreComputeFrame()
        {

        }


        /// <summary>
        /// Method which does the co-simulation for one frame.
        /// Method has to be thread-safe.
        /// If scene management is activated, the co-simulation automatically writes and synchronizes the scene/otherwise it has to be realized by a superior instance
        /// </summary>
        /// <param name="time">The delta time for the simulation</param>
        /// <returns></returns>
        public virtual MSimulationResult ComputeFrame(float time)
        {
            //Create a container format to store the information provided within a frame
            CoSimulationFrame currentFrame = new CoSimulationFrame((int)this.FrameNumber, TimeSpan.FromSeconds(this.Time))
            {
                //Directly assign the initial posture for debugging purposes
                Initial = this.SimulationState.Initial?.Copy(),                             
            };

            //Increment the frame number
            this.FrameNumber++;

            //Increment the time
            this.Time += time;

            //Add the frame time to the dictionary
            this.frameTimes.Add((int)this.FrameNumber, this.Time);

            //Handle the mmus
            foreach (MMUContainer mmuContainer in mmuContainers)
            {
                //Handle the available tasks for the given mmu and start if possible
                this.HandleTasksStart(mmuContainer);

                //Handle the task end processing (remove/abort the MMU if desired)
                this.HandleTasksEnd(mmuContainer);
            }

            foreach (MMUContainer mmuContainer in mmuContainers)
            {
                //Only perform do step if the MMU is active and not paused
                if (mmuContainer.IsActive && !mmuContainer.IsPaused)
                {
                    //Perform the do step routine with the specified time and fetch the result
                    try
                    {
                        //Restart the stopwatch for time measurement
                        this.stopwatch.Restart();

                        //Perform the actual do step (computationally expensive)
                        MSimulationResult result = mmuContainer.MMU.DoStep(time, this.SimulationState);

                        //Log the times if enabled
                        if (LogTimes)
                        {
                            if (result.LogData == null)
                                result.LogData = new List<string>();

                            result.LogData.Add("executionTime:" + this.stopwatch.Elapsed.TotalMilliseconds.ToString("F8", CultureInfo.InvariantCulture));
                        }

                        //Add the result to the results list -> Hacky beacuse multiple instances might be duplicated
                        for (int i=0; i< mmuContainer.CurrentTasks.Count;i++)
                            currentFrame.Results.Add(result);           

                        //Add all presently active instructions of the MMUContainer
                        currentFrame.Instructions.AddRange(mmuContainer.CurrentTasks.Select(s=>s.Instruction.ID));

                        //Handle the drawing calls (if defined)
                        if(result.DrawingCalls!=null && result.DrawingCalls.Count >0)
                            this.HandleDrawingCalls(result, mmuContainer);

                        //Handle the events of the MMUs (if available)
                        if(result.Events!=null && result.Events.Count >0)
                            this.HandleEvents(mmuContainer, result.Events);

                        //Update the current simulation state which is the input for the next MMU in hiearchy
                        this.SimulationState.Current = result.Posture.Copy();

                        //Assign the values to the simulation state and create a new list for each element (elements are accessed by reference)
                        this.SimulationState.Constraints = result.Constraints != null ? new List<MConstraint>(result.Constraints) : new List<MConstraint>();
                        this.SimulationState.Events = result.Events != null ? new List<MSimulationEvent>(result.Events) : new List<MSimulationEvent>();
                        this.SimulationState.SceneManipulations = result.SceneManipulations !=null? new List<MSceneManipulation>(result.SceneManipulations): new List<MSceneManipulation>();


                        //Optionally perform already a solving in here and update the state
                        if (this.HierachicalSolving && this.Solvers != null)
                        {
                            foreach (ICoSimulationSolver solver in this.Solvers)
                            {
                                if (solver != null && solver.RequiresSolving(result, time))
                                {
                                    //Restart the stopwatch for time measurement
                                    this.stopwatch.Restart();

                                    //Execute the solver
                                    result = solver.Solve(result, currentFrame.Results, time);

                                    //Log the times if enabled
                                    if (LogTimes)
                                    {
                                        if (result.LogData == null)
                                            result.LogData = new List<string>();

                                        result.LogData.Add("executionTime:" + this.stopwatch.Elapsed.TotalMilliseconds.ToString("F8", CultureInfo.InvariantCulture));
                                    }
                                }
                            }

                            //Assign the current posture
                            this.SimulationState.Current = result.Posture.Copy();
                        }
                    }
                    catch (Exception e)
                    {
                        //Create a new event for logging
                        this.LogEventHandler?.Invoke(this, new CoSimulationLogEvent("Problem at performing do step: " + mmuContainer.MMU.ID + " " + e.Message + " " + e.StackTrace + " " + e.InnerException));
                    }
                }
            }

            //Do the final merging
            currentFrame.MergedResult = this.MergeResults(currentFrame.Results, ref currentFrame, time);


            //Add all CoSimulation events to the result
            foreach (MSimulationEvent simEvent in this.coSimulationEvents)
                currentFrame.MergedResult.Events.Add(simEvent);

            //Clear the events occured in the current frame
            this.coSimulationEvents.Clear();
            this.SimulationState.Events.Clear();

            //Assign the final merged result to the initial state of the next frame
            this.SimulationState.Initial = currentFrame.MergedResult.Posture?.Copy();

            //Reset the scene manipulations
            this.SimulationState.SceneManipulations.Clear();

            //Only preserve the constraints for the next frame

            //Optionally store the results
            if (this.Recording)
                this.record.Frames.Add(currentFrame);

            //Return the cosimulated result
            return currentFrame.MergedResult;
        }


        /// <summary>
        /// Method is executed after the compute frame. 
        /// In main thread -> Has not to be thread-safe
        /// </summary>
        public virtual void PostComputeFrame(MSimulationResult result)
        {
        }


        #region actual co-simulation methods

        /// <summary>
        /// Adds a new instruction
        /// </summary>
        /// <param name="instruction">The description of the motion instruction</param>
        /// <param name="timing">Optional timing informations about the instruction</param>
        protected virtual void AddInstruction(MInstruction instruction, MotionTiming timing = null, int priority = 0)
        {
            //Find a compatible MMU which supports the instruction
            IMotionModelUnitAccess mmu = this.FindCompatibleMMU(instruction);

            //Skip if no matching MMU is available
            if (mmu == null)
            {
                Console.WriteLine($"No Compatible MMU found: {instruction.Name}, skipping the instruction assignment!");
                return;
            }

            //Create a new wrapper class called motion task
            MotionTask motionTask = new MotionTask()
            {
                MMUContainer = this.mmuContainers.Find(s => s.MMU == mmu),
                IsRunning = true,
                Instruction = instruction,
                Timing = timing
            };

            //Add to the buffer/ queue
            this.taskBuffer.Add(motionTask);

            Console.WriteLine("Instruction added to co-simulator: " + instruction.Name + " " + instruction.MotionType);
        }


        /// <summary>
        /// Handles (possible) tasks which can be executed by the mmu instance.
        /// The method is actually responsible for assigning the instruction and starting the respective MMU.
        /// </summary>
        /// <param name="instance"></param>
        protected virtual void HandleTasksStart(MMUContainer instance)
        {
            //Nothing to do if task buffer is empty
            if (taskBuffer.Count == 0)
                return;

            //Get all tasks which address the MMU
            List<MotionTask> tasks = this.taskBuffer.Where(s => s.MMUContainer == instance).ToList();

            //Handle each task
            foreach (MotionTask task in tasks)
            {
                //Check the start condition
                if (!this.CheckStartCondition(task))
                {
                    //Skip -> task cannot be started
                    continue;
                }

                //Instruction can be started -> Perform further operations

                //Remove the current task from the buffer
                taskBuffer.Remove(task);

                //Set instance active
                instance.IsActive = true;

                //Set the current task
                instance.CurrentTasks.Add(task);

                //Get the next tasks which are started after the present motion is finished (direct dependency to current task)
                List<MotionTask> subsequentTasks = this.GetSubsequentTasks(task.Instruction);

                //Incorporate the required boundary constraints by the subsequent tasks
                if (subsequentTasks != null)
                {
                    if (task.Instruction.Constraints == null)
                        task.Instruction.Constraints = new List<MConstraint>();

                    //Iterate over all subsequent tasks which have an assigned MMUContainer
                    foreach (MotionTask subsequentTask in subsequentTasks.Where(s=>s.MMUContainer!=null))
                    {
                        //Get the boundary constraints
                        List<MConstraint> boundaryConstraints = subsequentTask.MMUContainer.MMU.GetBoundaryConstraints(subsequentTask.Instruction);

                        //Add the boundary constraints to the instruction
                        task.Instruction.Constraints.AddRange(boundaryConstraints);
                    }
                }

                //Handle start action (if defined for the instruction)
                this.HandleStartActions(task);

                //Create a list for storing the events related to the assign instruction
                List<MSimulationEvent> events = new List<MSimulationEvent>();

                //Assign the actual instruction/call the MMU
                MBoolResponse response = instance.MMU.AssignInstruction(task.Instruction, this.SimulationState);

                if (response.Successful)
                {
                    //Create the start event
                    events.Add(new MSimulationEvent(task.Instruction.Name, mmiConstants.MSimulationEvent_Start, task.Instruction.ID));
                    Console.WriteLine($"Task started: {task.Instruction.Name}, {task.MMUContainer.MMU.ID}, frameNumber: {this.FrameNumber}");
                }

                else
                {
                    //Cannot start-> Raise abort event
                    events.Add(new MSimulationEvent(task.Instruction.Name, mmiConstants.MSimulationEvent_Abort, task.Instruction.ID));

                    //Further raise an init error event
                    events.Add(new MSimulationEvent(task.Instruction.Name, mmiConstants.MSimulationEvent_InitError, task.Instruction.ID));

                    Console.WriteLine($"Task cannot be started-> Error at assign instruction: {task.Instruction.Name}, {task.MMUContainer.MMU.ID}, frameNumber: {this.FrameNumber}");

                    if (response.LogData != null && response.LogData.Count > 0)
                    {
                        Console.WriteLine("LogData:");
                        foreach (string entry in response.LogData)                  
                            Console.WriteLine(entry);
                        
                    }
                }

                //Add the instruction
                this.record.Instructions.Add(task.Instruction);

                //Add to events which have been created by CoSimulation
                this.coSimulationEvents.AddRange(events);

                //Call event handler
                this.HandleEvents(instance, events);
            }
        }


        /// <summary>
        /// Handles tasks and checks whether the task is terminated
        /// </summary>
        /// <param name="mmuContainer"></param>
        protected virtual void HandleTasksEnd(MMUContainer mmuContainer)
        {
            //Only consider the container if active
            if (mmuContainer.IsActive)
            {
                //Iterate over all current tasks
                for (int i = mmuContainer.CurrentTasks.Count - 1; i >= 0; i--)
                {
                    MotionTask currentTask = mmuContainer.CurrentTasks[i];

                    //Check end condition or if the instruction should be aborted
                    if (this.CheckEndCondition(currentTask))
                    {
                        //Handle end action (if defined for the instruction)
                        this.HandleEndActions(currentTask);

                        //If the endcondition is fulfilled -> MMU is set to inactive and aborted
                        mmuContainer.MMU.Abort(currentTask.Instruction.ID);

                        //Remove the current task and set running to false
                        mmuContainer.CurrentTasks.Remove(currentTask);
                        mmuContainer.History.Add(currentTask);
                        currentTask.IsRunning = false;


                        //Create new event
                        MSimulationEvent simEvent = new MSimulationEvent(currentTask.Instruction.Name + ": Finished", mmiConstants.MSimulationEvent_End, currentTask.Instruction.ID);
                        coSimulationEvents.Add(simEvent);

                        //Throw new event
                        this.MSimulationEventHandler?.Invoke(this, simEvent);

                        //Deactive the MMU if no more task is active
                        if (!mmuContainer.CurrentTasks.Exists(s => s.IsRunning))
                            mmuContainer.IsActive = false;
                    }
                } 
            }
        }


        /// <summary>
        /// Handles the events of MMUs (can be overwritten by child classes)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="events"></param>
        protected virtual void HandleEvents(MMUContainer instance, List<MSimulationEvent> events)
        {
            //Skip if no events available
            if (events == null)
            {
                if (!this.eventDictionary.ContainsKey(this.FrameNumber))
                    this.eventDictionary.Add(this.FrameNumber, new List<MSimulationEvent>());

                return;
            }


            //Add to event frame mapping dictionary
            foreach (MSimulationEvent mmuEvent in events)
            {
                if (!this.eventFrameMapping.ContainsKey(mmuEvent))
                {
                    this.eventFrameMapping.Add(mmuEvent, new List<long>() { this.FrameNumber });
                }
                else
                {
                    this.eventFrameMapping[mmuEvent].Add(this.FrameNumber);
                }
            }

            //Add to the event dictionary
            if (this.eventDictionary.ContainsKey(this.FrameNumber))
            {
                this.eventDictionary[this.FrameNumber].AddRange(events);
            }
            else
            {
                //Add to the event dictionary
                this.eventDictionary.Add(this.FrameNumber, events);
            }

            //Print the events and provide it to the event handler
            foreach (MSimulationEvent mmuEvent in events)
            {
                //Raise event 
                this.MSimulationEventHandler?.Invoke(this, mmuEvent);
            }


            //Consider all current tasks
            for(int i= instance.CurrentTasks.Count-1; i>=0;i--)
            {
                //Get the current task
                MotionTask motionTask = instance.CurrentTasks[i];

                //Add all events to the motion task
                foreach (MSimulationEvent mmuEvent in events)
                {
                    motionTask.Events.Add(new Tuple<long, MSimulationEvent>(this.FrameNumber, mmuEvent));
                }

                //Check if finished event is contained
                if (events.Exists(s => s.Type == mmiConstants.MSimulationEvent_End && s.Reference == motionTask.Instruction.ID))
                {

                    //Handle the end actions
                    this.HandleEndActions(motionTask);


                    //Deactivate the task
                    motionTask.IsRunning = false;
                    instance.History.Add(motionTask);

                    instance.CurrentTasks.Remove(motionTask);

                    //Set inactive
                    if(instance.CurrentTasks.Count ==0)
                        instance.IsActive = false;


                    //Raise all finished event if no more taks available
                    if (this.taskBuffer.Count == 0 && !this.mmuContainers.Exists(s => s.IsActive))
                    {
                        this.AllTasksFinished?.Invoke(this, new EventArgs());
                    }
                }
            }
        }


        /// <summary>
        /// Handles the drawing calls. This method can be for instance implemented by the target engine.
        /// </summary>
        /// <param name="mmuResult"></param>
        /// <param name="instance"></param>
        protected virtual void HandleDrawingCalls(MSimulationResult mmuResult, MMUContainer instance)
        {
        }


        /// <summary>
        /// Method which does the actual merging process of the MMU results.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="resultPosture"></param>
        /// <param name="sceneActions"></param>
        /// <returns></returns>
        protected virtual MSimulationResult MergeResults(List<MSimulationResult> results, ref CoSimulationFrame currentFrame,float timespan)
        {
            //Create object wich contains the merged result
            MSimulationResult mergedResult = new MSimulationResult();

            //Use the last posture
            if (results.Count > 0)
                mergedResult.Posture = results.Last().Posture;
            else
                mergedResult.Posture = this.SimulationState.Initial.Copy();


            //Just add the scene manipulations
            mergedResult.SceneManipulations = new List<MSceneManipulation>();
            mergedResult.Events =  new List<MSimulationEvent>();
            mergedResult.Constraints = new List<MConstraint>();

            //Use the result of the last MMU
            if (results.Count > 0)
            {
                MSimulationResult lastResult = results.Last();

                //Create a new list which references all elements
                if (lastResult.SceneManipulations != null)
                    mergedResult.SceneManipulations = new List<MSceneManipulation>(lastResult.SceneManipulations);

                if (lastResult.Events != null)
                    mergedResult.Events = new List<MSimulationEvent>(lastResult.Events);

                if (lastResult.Constraints != null)
                    mergedResult.Constraints = new List<MConstraint>(lastResult.Constraints);
            }


            //Call each solver to generate the final results
            //Only prform if optimization stage is enabled
            if (this.OptimizationStage && this.Solvers != null)
            {
                foreach (ICoSimulationSolver solver in this.Solvers)
                {
                    //Check if solver needs to be called
                    if (solver != null && solver.RequiresSolving(mergedResult, timespan))
                    {
                        //Restart the stopwatch for measurement
                        this.stopwatch.Restart();

                        //Execute the solver
                        mergedResult = solver.Solve(mergedResult, results, timespan);

                        //Log the times if enabled
                        if (LogTimes)
                        {
                            if (mergedResult.LogData == null)
                                mergedResult.LogData = new List<string>();

                            mergedResult.LogData.Add("executionTime:" + this.stopwatch.Elapsed.TotalMilliseconds.ToString("F8", CultureInfo.InvariantCulture));
                        }

                        //Add to the current frame
                        currentFrame.CoSimulationSolverResults.Add(mergedResult);
                    }
                }
            }

            return mergedResult;
        }


        /// <summary>
        /// Looks for a compatible mmu where the motion type corresponds to the one specified in the instruction.
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        protected virtual IMotionModelUnitAccess FindCompatibleMMU(MInstruction instruction)
        {
            //By default use the motion type
            bool useMMUID = false;

            if (instruction.Properties != null && instruction.Properties.ContainsKey("MMUID"))
                useMMUID = true;


            foreach (MMUContainer mmuContainer in this.mmuContainers)
            {
                if (useMMUID)
                {
                    //Check if the MMU id matches and return the instance if true
                    if (instruction.Properties["MMUID"] == mmuContainer.Description.ID)
                    {
                        return mmuContainer.MMU;
                    }
                }

                //By default use the motion type
                else
                {
                    //Check if the motion type matches and return the instance if true
                    if (mmuContainer.Description.MotionType == instruction.MotionType)
                    {
                        return mmuContainer.MMU;
                    }
                }

  
            }

            return null;
        }


        /// <summary>
        /// Checks the start conditions of the given motion task
        /// </summary>
        /// <param name="motionTask"></param>
        /// <returns></returns>
        protected virtual bool CheckStartCondition(MotionTask motionTask)
        {
            //1) First-> Check all internal constraints (timing specified by user within co-sim)

            //Check if timing constraint are specified by the user and the start of the instruction is not explicitly forced 
            //(otherwise the check can be skipped, since the execution is principally allowed by the user) 
            if ((motionTask.Timing != null && motionTask.Timing.StartCondition != null) && !this.toStart.Contains(motionTask.Instruction.ID))
            {
                //Directly return if condition is false -> avoid checking the prerequisites
                if (!EvaluateCondition(motionTask.Timing.StartCondition))
                    return false;
            }

            //2) Next -> Check the prerequisites of the MMU
            MBoolResponse result = new MBoolResponse(false);
            //Check if the mmu can be started by considering the prerequisites (expensive call -> try to avoid unnecessary calls)
            try
            {
                result = motionTask.MMUContainer.MMU.CheckPrerequisites(motionTask.Instruction);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured during execution of CheckPrerequisites of MMU: " + motionTask.MMUContainer.MMU.Name + " " + e.Message);
                this.LogEventHandler?.Invoke(this, new CoSimulationLogEvent("Exception occured during execution of CheckPrerequisites of MMU: " + motionTask.MMUContainer.MMU.Name + " " + e.Message));
            }

            return result.Successful;
        }


        /// <summary>
        /// Checks the end condition of a task
        /// </summary>
        /// <param name="motionTask"></param>
        /// <returns></returns>
        protected virtual bool CheckEndCondition(MotionTask motionTask)
        {
            //Terminate if the task should be aborted explitelly
            if (this.toAbort.Contains(motionTask.Instruction.ID))
            {
                return true;
            }

            //Terminate if endcondition available and expression true
            if (motionTask.Timing != null && motionTask.Timing.EndCondition != null)
            {
                return EvaluateCondition(motionTask.Timing.EndCondition);
            }

            //Default return false
            return false;
        }


        /// <summary>
        /// Checks whether the specified event is already available
        /// </summary>
        /// <param name="instructionID"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public virtual bool EventAvailable(string instructionID, string eventName)
        {
            //Iterate over each container and check if the event is available
            foreach (MMUContainer container in this.mmuContainers)
            {
                if (container.Events != null && container.Events.Exists(s => s.Reference != null && s.Reference == instructionID && s.Type == eventName))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Method finds the next tasks which are started after the instruction is finished (direct dependcy to end event of examined instruction)
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        protected virtual List<MotionTask> GetSubsequentTasks(MInstruction instruction)
        {
            List<MotionTask> subsequentTasks = new List<MotionTask>();

            //Iterate over each task in the buffer and return the first result
            foreach (MotionTask task in taskBuffer)
            {
                MInstruction subsequentInstruction = task.Instruction;
                if (subsequentInstruction.StartCondition != null)
                {
                    //Evaluate whether the expression is true if the instruction is finished -> if so we have our subsequent task
                    if (this.EvaluateExpressionHypthothetical(subsequentInstruction.StartCondition, instruction))
                        subsequentTasks.Add(task);
                }
            }

            return subsequentTasks;
        }


        /// <summary>
        /// Checks if the time offset is fulfilled considering the event as reference
        /// </summary>
        /// <param name="instructionID"></param>
        /// <param name="eventName"></param>
        /// <param name="timeOffset"></param>
        /// <returns></returns>
        public virtual bool TimeOffsetFulfilled(string instructionID, string eventName, float timeOffset)
        {
            //To do check if sufficient amount of time has been elapsed
            MSimulationEvent mmuEvent = this.eventFrameMapping.Keys.ToList().Find(s => s.Reference != null && s.Reference == instructionID && s.Type == eventName);

            if (mmuEvent != null)
            {
                List<long> occurences = this.eventFrameMapping[mmuEvent];

                //Compute the passed time
                float elapsed = this.frameTimes[this.FrameNumber] - this.frameTimes[occurences.Last()];

                //Return false if time which has been elapsed is not sufficient
                return (elapsed >= timeOffset);
            }

            return false;
        }


        /// <summary>
        /// Evaluates whether the condition is fulfilled
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        protected virtual bool EvaluateCondition(SynchronizationCondition condition)
        {
            switch (condition.ConditionType)
            {
                case ConditionType.Event:

                    //Check if the specific event has been raised
                    if (!EventAvailable(condition.Reference, condition.EventName))
                        return false;

                    //Do further checks if a time offset is defined and the condition is fulfilled
                    if (condition.TimeOffset > 0)
                    {
                        //Check if time offset is fulfilled -> if not directly return false
                        if (!this.TimeOffsetFulfilled(condition.Reference, condition.EventName, condition.TimeOffset))
                            return false;
                    }
                    break;

                case ConditionType.Expression:

                    //Evaluate the expression
                    if (!condition.Expression())
                        return false;
                    break;
            }

            //Recursively evaluate the subconditions
            if (condition.SubConditions != null)
            {
                foreach (SynchronizationCondition subcondition in condition.SubConditions)
                {
                    if (!this.EvaluateCondition(subcondition))
                        return false;
                }
            }

            //If not returned false until here -> the expression tree is valid
            return true;
        }


        /// <summary>
        /// Sort the MMUs according to the defined priorities
        /// </summary>
        protected virtual void SortMMUPriority()
        {
            //Add the mmu instances
            foreach (MMUContainer mmu in this.mmuContainers)
            {
                float priority = 1;
                if (!this.priorities.TryGetValue(mmu.Description.MotionType, out priority))
                    priority = 1;

                mmu.Priority = priority;
            }

            //Order with ascending priority
            this.mmuContainers = this.mmuContainers.OrderBy(s => s.Priority).ToList();
        }

        
        /// <summary>
        /// Handles optionally defined actions which should be executed in case of starting the motion task/instruction
        /// </summary>
        /// <param name="task"></param>
        protected virtual void HandleStartActions(MotionTask task)
        {
            List<String> actionStrings = new List<String>();

            //Utilize the action field of the instruction
            //To do provide multiple actions in future
            if (task.Instruction.Action != null && task.Instruction.Action.Length > 0)
            {
                //Example action string:
                //OnStart->8315395235:StartInstruction,OnEnd->3523534:StartInstruction
                
                //Get all subactions
                string[] subActions = task.Instruction.Action.Split(',');

                foreach(string subAction in subActions)
                {
                    //Example: OnStart->8315395235:StartInstruction

                    //Create the body of the subactions
                    string[] body = subAction.Split(new string[] { "->" },StringSplitOptions.RemoveEmptyEntries);

                    foreach(string b in body)
                    {
                        Console.WriteLine("Body: " + b);
                    }

                    if (body.Length >= 2)
                    {
                        //OnStart -> Remove whitespaces
                        string coSimTopic = body[0].Replace(" ", String.Empty);

                        if(coSimTopic == CoSimTopic.OnStart)
                        {
                            //8315395235:StartInstruction -> Remove whitespaces
                            actionStrings.Add(body[1].Replace(" ", String.Empty));
                        }
                    }
                }

            }

            //Check if instruction has a start action
            if (task.Instruction.Properties != null && task.Instruction.Properties.ContainsKey(CoSimTopic.OnStart))
                actionStrings.Add(task.Instruction.Properties[CoSimTopic.OnStart]);


            //Parse and schedule the actions
            this.ParseAndScheduleActions(actionStrings);   
        }


        /// <summary>
        /// Handles optionally defined actions which should be executed in case of finishing the motion task/instruction
        /// </summary>
        /// <param name="task"></param>
        protected virtual void HandleEndActions(MotionTask task)
        {
            List<String> actionStrings = new List<String>();

            //Utilize the action field of the instruction
            //To do provide multiple actions in future
            if (task.Instruction.Action != null && task.Instruction.Action.Length > 0)
            {
                //Example action string:
                //OnStart->8315395235:StartInstruction,OnEnd->3523534:StartInstruction

                //Get all subactions
                string[] subActions = task.Instruction.Action.Split(',');

                foreach (string subAction in subActions)
                {
                    //Example: OnStart->8315395235:StartInstruction

                    //Create the body of the subactions 
                    string[] body = subAction.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);


                    if (body.Length >= 2)
                    {
                        //OnStart -> Remove whitespaces
                        string coSimTopic = body[0].Replace(" ", String.Empty);

                        if (coSimTopic == CoSimTopic.OnEnd)
                        {
                            //8315395235:StartInstruction -> Remove whitespaces
                            actionStrings.Add(body[1].Replace(" ", String.Empty));
                        }
                    }
                }

            }

            //Check if instruction has a start action
            if (task.Instruction.Properties != null && task.Instruction.Properties.ContainsKey(CoSimTopic.OnEnd))
                actionStrings.Add(task.Instruction.Properties[CoSimTopic.OnEnd]);



            //Parse and schedule the actions
            this.ParseAndScheduleActions(actionStrings);         
        }


        /// <summary>
        /// Method parses and consecutively schedules the actions
        /// </summary>
        /// <param name="actionStrings"></param>
        protected virtual void ParseAndScheduleActions(List<String> actionStrings)
        {
            //Handle each individual action
            foreach (string actionString in actionStrings)
            {
                //Split the action into insruction id and the respectic CoSimAction
                string[] split = actionString.Split(':');

                //Check if the string can be split
                if (split.Length >= 2)
                {
                    //Extract the id 
                    string id = split[0];

                    //Extract the respective action
                    string action = split[1];

                    //Perform action with specified id 
                    switch (action)
                    {
                        case CoSimAction.StartInstruction:
                            //Start the instruction
                            toStart.Add(id);
                            break;

                        case CoSimAction.EndInstruction:
                            //Abort the instruction 
                            this.toAbort.Add(id);
                            break;
                    }

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Problem parsing action");
                }

            }
        }

        /// <summary>
        /// Evaluates the given string expression based on a lookup function.
        /// Overall expression such as (test:MotionFinished + 0.5 && test2:MotionFinished) || test:MotionFinished can be interpeted.
        /// The individual expression such as test:MotionFinished are resolved by the variableLookupFunction.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="variableLookupFunction"></param>
        /// <returns></returns>
        protected virtual bool EvaluateExpression(string expression, Func<string, bool> variableLookupFunction)
        {
            //Create a formated expression for evaluation using the data table
            string formatedExpression = expression.Replace("&&", "AND");
            formatedExpression = formatedExpression.Replace("||", "OR");

            //Remove the brackets
            expression = expression.Replace('(', ' ');
            expression = expression.Replace(')', ' ');

            //Get all variables/subexpressions which have to be evaluated
            string[] variables = expression.Split(new string[] { "&&", "||" }, StringSplitOptions.RemoveEmptyEntries);

            //Validate the respective variable/subexpression
            foreach (string variable in variables)
            {
                string trimmed = variable.Trim();
                bool result = variableLookupFunction(trimmed);
                formatedExpression = formatedExpression.Replace(trimmed, result.ToString());
            }



            bool expressionResult = false;

            //Evaluate the expression
            try
            {
                expressionResult = (bool)new DataTable().Compute(formatedExpression, "");

            }
            catch (Exception)
            {
                Console.WriteLine("(Co-Simulator) Failed to evaluate the expression: " + formatedExpression);
            }

            return expressionResult;
        }


        /// <summary>
        /// Method evaluates the single timing expression (either if event is raised or sufficient amount of time is elapsed)
        /// </summary>
        /// <param name="singleExpression"></param>
        /// <returns></returns>
        protected virtual bool EvaluateTimingExpression(string singleExpression)
        {
            //Split the statemenet
            if (singleExpression.Contains(":"))
            {
                string reference = singleExpression.Split(':')[0].Trim();
                string eventName = singleExpression.Split(':')[1].Trim();

                if (eventName.Contains("+"))
                {
                    float offset = float.Parse(eventName.Split('+')[1].Trim(), CultureInfo.InvariantCulture);

                    //Check if offset is fulfilled
                    eventName = eventName.Split('+')[0].Trim();

                    //Check if the time offset is fulfilled
                    return this.TimeOffsetFulfilled(reference, eventName, offset);
                }

                //Only check if event is available
                return this.EventAvailable(reference, eventName);
            }

            return false;
        }


        /// <summary>
        /// Evaluates the given string expression based on the assumption that the specified instruction is finished
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        protected virtual bool EvaluateExpressionHypthothetical(string expression, MInstruction instruction)
        {
            //Create a formated expression for evaluation using the data table
            string formatedExpression = expression.Replace("&&", "AND");
            formatedExpression = formatedExpression.Replace("||", "OR");

            //Remove the brackets
            expression = expression.Replace('(', ' ');
            expression = expression.Replace(')', ' ');

            //Get all variables/subexpressions which have to be evaluated
            string[] variables = expression.Split(new string[] { "&&", "||" }, System.StringSplitOptions.RemoveEmptyEntries);

            //Validate the respective variable/subexpression
            foreach (string variable in variables)
            {
                string singleExpression = variable.Trim();

                bool result = false;

                //Split the statemenet
                if (singleExpression.Contains(":"))
                {
                    string reference = singleExpression.Split(':')[0].Trim();
                    string eventName = singleExpression.Split(':')[1].Trim();

                    if (reference == instruction.ID && eventName == mmiConstants.MSimulationEvent_End)
                    {
                        result = true;
                    }
                }

                formatedExpression = formatedExpression.Replace(singleExpression, result.ToString());
            }

            bool expressionResult = false;


            //Evaluate the expression
            try
            {
                expressionResult = (bool)new DataTable().Compute(formatedExpression, "");

            }
            catch (Exception)
            {
                Console.WriteLine("(Co-Simulator) Failed to evaluate the expression: " + formatedExpression);
            }

            return expressionResult;
        }


        /// <summary>
        /// Finds a motion task given an instruction id
        /// </summary>
        /// <param name="instructionID"></param>
        /// <returns></returns>
        protected virtual MotionTask FindMotionTask(string instructionID)
        {
            return this.mmuContainers.Where(s => s.CurrentTasks != null && s.CurrentTasks.Count > 0).Select(s => s.CurrentTasks).SelectMany(s => s).ToList().Find(s => s.Instruction.ID != null && s.Instruction.ID == instructionID);
        }

        #endregion

        #region MMU compatibility



        /// <summary>
        /// Returns the boundary constraints to start the instruction (just for compatibility with MMU interface)
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public virtual List<MConstraint> GetBoundaryConstraints(MInstruction instruction)
        {
            return new List<MConstraint>();
        }


        /// <summary>
        /// Executes a function by name and generic parameters -> Could be implemented for debugging purposes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> ExecuteFunction(string name, Dictionary<string, string> parameters)
        {
            //Nothing to do in here
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Returns the description of the co-simulator
        /// </summary>
        /// <returns></returns>
        public virtual MMUDescription GetDescription()
        {
            return this.Description;
        }


        /// <summary>
        /// Checks whether the prerequisites for the instruction are fulfilled
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public virtual MBoolResponse CheckPrerequisites(MInstruction instruction)
        {
            //This could be overloaded in future to test whether a compatible MMU is available

            if (this.mmuInstances != null)
            {
                //Iterate over each MMU and check whether the prerequisites are fulfilled
                foreach (IMotionModelUnitAccess mmu in this.mmuInstances)
                {
                    //Return true if at least one MMU's prerequisites are fulfilled
                    if (mmu.CheckPrerequisites(instruction).Successful)
                    {
                        return new MBoolResponse(true);
                    }
                }
            }

            return new MBoolResponse(false);



        }

        /// <summary>
        /// Method is called if the co-simulation should be disposed (over network).
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual MBoolResponse Dispose(Dictionary<string, string> parameters)
        {
            //Abort the co-simulator
            this.Abort();
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Creates a checkpoint of the co-simulation
        /// </summary>
        /// <returns></returns>
        public virtual byte[] CreateCheckpoint()
        {
            SerializableCoSimulationState checkpoint = new SerializableCoSimulationState()
            {
                FrameNumber = this.FrameNumber,
                Time = this.Time,
                AvatarState = this.SimulationState,
                EventDictionary = this.eventDictionary,
                FrameTimes = this.frameTimes,
                EventFrameMapping = this.eventFrameMapping
            };


            //Add all the mmu data
            foreach (IMotionModelUnitAccess mmu in this.mmuInstances)
            {
                checkpoint.MMUData.Add(mmu.ID, mmu.CreateCheckpoint());
            }

            //Add all motion tasks which are already contained in the containers
            foreach (MMUContainer container in this.mmuContainers)
            {
                if (container.CurrentTasks != null)
                {
                    foreach (MotionTask task in container.CurrentTasks)
                        if (!checkpoint.MotionTasks.ContainsKey(task.ID))
                            checkpoint.MotionTasks.Add(task.ID, task.GetAsSerializable());
                }

                foreach (MotionTask task in container.History)
                {
                    if (!checkpoint.MotionTasks.ContainsKey(task.ID))
                        checkpoint.MotionTasks.Add(task.ID, task.GetAsSerializable());
                }
            }

            //Add the tasks within the buffer
            foreach (MotionTask task in this.taskBuffer)
            {
                checkpoint.TaskBuffer.Add(task.ID);

                if (!checkpoint.MotionTasks.ContainsKey(task.ID))
                    checkpoint.MotionTasks.Add(task.ID, task.GetAsSerializable());
            }


            //Add all containers
            foreach (MMUContainer mmuContainer in this.mmuContainers)
            {
                checkpoint.MMUContainers.Add(mmuContainer.GetAsSerializable());
            }


            //Serialize
            return Serialization.SerializeBinary(checkpoint);
        }


        /// <summary>
        /// Restores the checkpoint of the co-simulation
        /// </summary>
        /// <param name="data"></param>
        public virtual MBoolResponse RestoreCheckpoint(byte[] data)
        {
            //Get the loaded state from byte array
            SerializableCoSimulationState loadedState = Serialization.DeserializeBinary<SerializableCoSimulationState>(data);

            //Restore the event dictionary
            this.eventDictionary = loadedState.EventDictionary;
            this.SimulationState = loadedState.AvatarState;
            this.Time = loadedState.Time;
            this.FrameNumber = loadedState.FrameNumber;
            this.eventDictionary = loadedState.EventDictionary;
            this.eventFrameMapping = loadedState.EventFrameMapping;
            this.frameTimes = loadedState.FrameTimes;


            //Dictionary which contains MMU container id <-> Motion Task
            Dictionary<string, List<MotionTask>> mTasks = new Dictionary<string, List<MotionTask>>();
            List<MotionTask> taskList = new List<MotionTask>();


            //Add all tasks 
            foreach (SerializableMotionTask sTask in loadedState.MotionTasks.Values)
            {
                if (sTask.ContainerID == null)
                    sTask.ContainerID = "not assigned " + System.Guid.NewGuid().ToString();


                if (!mTasks.ContainsKey(sTask.ContainerID))
                    mTasks.Add(sTask.ContainerID, new List<MotionTask>());

                MotionTask task = new MotionTask(sTask);

                mTasks[sTask.ContainerID].Add(task);
                taskList.Add(task);
            }



            //Clear the container list
            this.mmuContainers.Clear();

            //Add all MMU Container
            foreach (SerializableMMUContainer sContainer in loadedState.MMUContainers)
            {
                if (mTasks.ContainsKey(sContainer.ID))
                {
                    this.mmuContainers.Add(new MMUContainer(sContainer, this.mmuInstances, mTasks[sContainer.ID]));
                }
                else
                {
                    this.mmuContainers.Add(new MMUContainer(sContainer, this.mmuInstances, new List<MotionTask>()));
                }
            }

            //Link the instance to the tasks
            foreach (var entry in mTasks)
            {
                foreach (MotionTask mtask in entry.Value)
                {
                    mtask.MMUContainer = this.mmuContainers.Find(s => s.ID == entry.Key);
                }
            }


            //Refill the task buffer
            this.taskBuffer.Clear();

            //Refill the task buffer
            foreach (string taskId in loadedState.TaskBuffer)
            {

                MotionTask motionTask = taskList.Find(s => s.ID == taskId);

                if (motionTask != null)
                    taskBuffer.Add(motionTask);
            }


            //Add all the mmu data
            foreach (var kvp in loadedState.MMUData)
            {
                IMotionModelUnitAccess match = this.mmuInstances.Find(s => s.ID == kvp.Key);
                match.RestoreCheckpoint(kvp.Value);
            }

            return new MBoolResponse(true);
        }

#endregion

    }
}
