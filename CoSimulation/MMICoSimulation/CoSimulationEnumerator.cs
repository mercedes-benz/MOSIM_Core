// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections;

namespace MMICoSimulation
{


    public class CoSimulationFrameEnumerator : IEnumerator
    {
        public CoSimulationFrame Frame;
        protected int currentIndex;

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public MSimulationResult Current
        {
            get
            {
                return this.Frame.Results[this.currentIndex];
            }
        }


        public CoSimulationFrameEnumerator(CoSimulationFrame frame)
        {
            this.Frame = frame;
            this.currentIndex = 0;
        }

        /// <summary>
        /// Moves to the next frame
        /// </summary>
        public bool MoveNext()
        {
            currentIndex++;

            return (currentIndex < this.Frame.Results.Count);
        }


        public bool MovePrevious()
        {
            currentIndex--;
            return currentIndex >= 0;
        }

        public void Reset()
        {
            this.currentIndex = -1;
        }     
    }    
}
