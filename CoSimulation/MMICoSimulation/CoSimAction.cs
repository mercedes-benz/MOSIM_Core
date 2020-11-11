// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

namespace MMICoSimulation
{
    /// <summary>
    /// String for formulating co-simulation related actions.
    /// The action specifies what should happen if the source/topic is fulfilled.
    /// </summary>
    public static class CoSimAction
    {
        public const string StartInstruction = "StartInstruction";
        public const string EndInstruction = "EndInstruction";
    }
}
