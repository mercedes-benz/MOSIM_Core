// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MMICoSimulation
{
    /// <summary>
    /// Container class which encapsualtes individual co-simulation frames and the corresponding instructions
    /// </summary>
    [Serializable]
    public class CoSimulationRecord:IEnumerable<Tuple<CoSimulationFrame, MInstruction>>, ICloneable
    {
        /// <summary>
        /// The avaiable instructions
        /// </summary>
        public List<MInstruction> Instructions;

        /// <summary>
        /// The assigned frames
        /// </summary>
        public List<CoSimulationFrame> Frames;


        public CoSimulationRecord()
        {
            this.Instructions = new List<MInstruction>();
            this.Frames = new List<CoSimulationFrame>();
        }


        public CoSimulationRecord(List<CoSimulationFrame> frames, List<MInstruction> instructions)
        {
            this.Instructions = instructions;
            this.Frames = frames;
        }



        public IEnumerator GetEnumerator()
        {
            return new CoSimulationRecordEnumerator(this);
        }

        IEnumerator<Tuple<CoSimulationFrame, MInstruction>> IEnumerable<Tuple<CoSimulationFrame, MInstruction>>.GetEnumerator()
        {
            return new CoSimulationRecordEnumerator(this);
        }

        public object Clone()
        {
            //To do
            CoSimulationRecord clone = new CoSimulationRecord();
            return clone;
        }


    }


    /// <summary>
    /// Class allow to playback stored CoSimulation frames
    /// </summary>
    public class CoSimulationRecordEnumerator : IEnumerator<Tuple<CoSimulationFrame,MInstruction>>
    {
        protected CoSimulationRecord record;
        protected int currentIndex;

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public Tuple<CoSimulationFrame, MInstruction> Current
        {
            get
            {
                return new Tuple<CoSimulationFrame, MInstruction>(this.record.Frames[this.currentIndex], this.record.Instructions[this.currentIndex]);
            }
        }

        public CoSimulationRecordEnumerator(CoSimulationRecord record)
        {
            this.record = record;
            this.currentIndex = 0;
        }


        /// <summary>
        /// Moves to the next frame
        /// </summary>
        public bool MoveNext()
        {
            currentIndex++;
            return (currentIndex < record.Frames.Count);
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

        public void Dispose()
        {
            //Nothing to do
        }
    }


}
