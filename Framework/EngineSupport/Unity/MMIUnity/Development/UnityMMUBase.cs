// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Common;
using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMIUnity
{
    /// <summary>
    /// Base class for all MMUs
    /// </summary>
    public class UnityMMUBase : UnityAvatarBase, IMotionModelUnitDev
    {
        /// <summary>
        /// The provided service access
        /// </summary>
        public IServiceAccess ServiceAccess
        {
            get;
            set;
        }

        /// <summary>
        /// Access to the scene
        /// </summary>
        public MSceneAccess.Iface SceneAccess
        {
            get;
            set;
        }

        /// <summary>
        /// Access to skeleton manipulation functionality
        /// </summary>
        public MSkeletonAccess.Iface SkeletonAccess
        {
            get;
            set;
        }

        public string ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string MotionType
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

        /// <summary>
        /// The assigned avatar description
        /// </summary>
        protected MAvatarDescription AvatarDescription;



        /// <summary>
        /// Executes the content on the main thread
        /// </summary>
        /// <param name="function">The function which should be executed</param>
        public void ExecuteOnMainThread(Action function)
        {
            if(MainThreadDispatcher.Instance == null)
            {
                UnityEngine.Debug.Log("Cannot execute on main thread, Main thread dispatcher not available");
            }

            //Execute using MainThreadDispatcher
            MainThreadDispatcher.Instance.ExecuteBlocking(function);
        }


        /// <summary>
        /// Basic initialization method. In order to setup the retargeting the base class method should be executed by the child class.
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public virtual MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string,string> properties)
        {
            //Assign the avatar description
            this.AvatarDescription = avatarDescription;

            //Setup the retargeting already in the base classe
            this.SetupRetargeting(AvatarDescription.AvatarID);

            //Assign the skeleton access
            this.SkeletonAccess = this.GetSkeletonAccess();

            return new MBoolResponse(true);
        }



        public virtual MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState state)
        {
            return new MBoolResponse(true);
        }

        public virtual MSimulationResult DoStep(double time, MSimulationState avatarState)
        {
            return new MSimulationResult();
        }


        public virtual byte[] CreateCheckpoint()
        {
            return new byte[0];
        }

        public virtual MBoolResponse RestoreCheckpoint(byte[] data)
        {
            return new MBoolResponse(true);
        }


        public virtual List<MConstraint> GetBoundaryConstraints(MInstruction instruction)
        {
            return new List<MConstraint>();
        }


        public virtual MBoolResponse CheckPrerequisites(MInstruction instruction)
        {
            return new MBoolResponse(true);
        }

        public virtual MBoolResponse Abort(string instructionId)
        {
            return new MBoolResponse(true);
        }

        public virtual MBoolResponse Dispose(Dictionary<string, string> parameters)
        {
            return new MBoolResponse(true);
        }


        public virtual Dictionary<string, string> ExecuteFunction(string name, Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }
    }





}
