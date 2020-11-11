// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System.Collections.Generic;
using System;

namespace MMICSharp.Common
{
    /// <summary>
    /// Base class for c# MMU development.
    /// Novel MMU can inherit from this class.
    /// </summary>
    public class MMUBase : IMotionModelUnitDev
    {
        #region properties

        /// <summary>
        /// The motion type of the MMU
        /// </summary>
        public string MotionType
        {
            get;
            set;
        }

        /// The name of the MMU
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The access to the scene
        /// </summary>
        public MSceneAccess.Iface SceneAccess
        {
            get;
            set;
        }

        public MSkeletonAccess.Iface SkeletonAccess
        {
            get;
            set;
        }

        /// <summary>
        /// Access to the services which are by default shipped with the MMI standard
        /// </summary>
        public IServiceAccess ServiceAccess
        {
            get;
            set;
        }

        /// <summary>
        /// Instance of the adapter to be used optionally (if accessing other MMUs via Abstraction)
        /// </summary>
        public AdapterEndpoint AdapterEndpoint
        {
            get;
            set;
        }


        public string ID
        {
            get;
            set;
        }


        #endregion

        /// <summary>
        /// The assigned avatar description
        /// </summary>
        protected MAvatarDescription AvatarDescription;

        /// <summary>
        /// Basic initialization method
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string,string> properties)
        {
            //Set the avatar description
            this.AvatarDescription = avatarDescription;
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Method to assign a specific instruction
        /// </summary>
        /// <param name="MInstruction"></param>
        /// <param name="avatarState"></param>
        /// <returns></returns>
        public virtual MBoolResponse AssignInstruction(MInstruction instruction,  MSimulationState simulationState)
        {
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Method to check whether the instruction can be executed given the present state of the scene
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public virtual MBoolResponse CheckPrerequisites(MInstruction instruction)
        {
            return new MBoolResponse(true);
        }

        /// <summary>
        /// Returns the boundary constraint for the given instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public virtual List<MConstraint> GetBoundaryConstraints(MInstruction instruction, MSimulationState simulationState)
        {
            return new List<MConstraint>();
        }



        public virtual byte[] CreateCheckpoint()
        {
            return new byte[0];
        }

        public virtual MBoolResponse RestoreCheckpoint(byte[] data)
        {
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Aborts the specified instruction
        /// </summary>
        /// <param name="instructionID"></param>
        /// <returns></returns>
        public virtual MBoolResponse Abort(string instructionID = null)
        {
            return new MBoolResponse(true);
        }

        /// <summary>
        /// Disposes the MMU
        /// </summary>
        /// <returns></returns>
        public virtual MBoolResponse Dispose(Dictionary<string,string> parameters)
        {
            return new MBoolResponse(true);
        }


        /// <summary>
        /// Can be optionally provided by the MMU
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> ExecuteFunction(string name, Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }





        public virtual List<MConstraint> GetBoundaryConstraints(MInstruction instruction)
        {
            return new List<MConstraint>();
        }

        public virtual MSimulationResult DoStep(double time, MSimulationState simulationState)
        {
            throw new NotImplementedException();
        }
    }

}
