// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;

namespace MMICoSimulation
{
    public static class Utils
    {
        /// <summary>
        /// Creates a condition string based on the instruction and the event name
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public static string CreateConditionString(MInstruction instruction, string eventName)
        {
            return instruction.ID + ":" + eventName;
        }
    }
}
