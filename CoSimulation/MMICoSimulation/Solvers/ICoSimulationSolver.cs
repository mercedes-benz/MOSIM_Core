// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;
using MMIStandard;

namespace MMICoSimulation.Solvers
{
    /// <summary>
    /// Basic interface for a solver which can be utilized for processing the results of MMUs
    /// </summary>
    public interface ICoSimulationSolver
    {

        /// <summary>
        /// Indicates whether a solving is required.
        /// E.g. posture is already solved and satisfies the constraints
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        bool RequiresSolving(MSimulationResult result, float timeSpan);


        /// <summary>
        /// Solve method is responsible for processing the results obtainted by MMUs
        /// </summary>
        /// <param name="currentResult">The current (merged) result</param>
        /// <param name="mmuResults">The raw result data of the mmus</param>
        /// <returns>A simulation result which is merged based on the given input </returns>
        MSimulationResult Solve(MSimulationResult currentResult, List<MSimulationResult> mmuResults, float timeSpan);
    }
}
