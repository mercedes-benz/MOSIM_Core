// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;

namespace MMICoSimulation
{
    public class CoSimulationLogEvent:EventArgs
    {
        public string Message;

        public CoSimulationLogEvent(string message)
        {
            this.Message = message;
        }
    }
}
