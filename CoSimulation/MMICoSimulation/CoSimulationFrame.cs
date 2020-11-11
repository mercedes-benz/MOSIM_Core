// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;


namespace MMICoSimulation
{
    /// <summary>
    /// A container class representing an individual frame of the co-simulation
    /// </summary>
    [Serializable]
    public class CoSimulationFrame
    {
        /// <summary>
        /// The initial posture
        /// </summary>
        public MAvatarPostureValues Initial;

        /// <summary>
        /// The absolute simulation time
        /// </summary>
        public TimeSpan Time;

        /// <summary>
        /// The frame number
        /// </summary>
        public int FrameNumber;

        /// <summary>
        /// All individual results
        /// </summary>
        public List<MSimulationResult> Results;

        /// <summary>
        /// The merged result
        /// </summary>
        public MSimulationResult MergedResult;


        /// <summary>
        /// The results gathered form the cosimulation solvers
        /// </summary>
        public List<MSimulationResult> CoSimulationSolverResults;


        /// <summary>
        /// The ids of the corresponding instructions
        /// </summary>
        public List<string> Instructions;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <param name="time"></param>
        public CoSimulationFrame(int frameNumber, TimeSpan time)
        {
            this.FrameNumber = frameNumber;
            this.Time = time;
            this.Results = new List<MSimulationResult>();
            this.Instructions = new List<string>();
            this.CoSimulationSolverResults = new List<MSimulationResult>();
        }
    }
}
