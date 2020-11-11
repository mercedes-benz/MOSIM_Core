// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICoSimulation;
using MMICSharp.Access;
using MMICSharp.Common;
using MMIStandard;
using MMIUnity.TargetEngine.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMIUnity.TargetEngine
{
    /// <summary>
    /// Wrapper class to a remote cosimulation
    /// </summary>
    public class RemoteCoSimulation : MMICoSimulator
    {

        public override event EventHandler<MSimulationEvent> MSimulationEventHandler;

        #region private variables


        private readonly IMotionModelUnitAccess remoteCoSimulationMMU;

        /// <summary>
        /// The referenced avatar
        /// </summary>
        private readonly MMIAvatar avatar;


        /// <summary>
        /// The service access 
        /// </summary>
        private readonly IServiceAccess serviceAccess;

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="mmus"></param>
        /// <param name="avatar"></param>
        public RemoteCoSimulation(IMotionModelUnitAccess coSimulationMMU, IServiceAccess serviceAccess, MMIAvatar avatar, Dictionary<string, string> priorities) : base(new List<IMotionModelUnitAccess>() { coSimulationMMU })
        {
            this.avatar = avatar;
            this.serviceAccess = serviceAccess;

            this.remoteCoSimulationMMU = coSimulationMMU;
        }


        /// <summary>
        /// Co Simulator has to be initialized before usage
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            //Initialize the CoSimulator
            return this.remoteCoSimulationMMU.Initialize(avatarDescription, properties);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="avatarState"></param>
        public override MBoolResponse AssignInstruction(MInstruction instruction, MSimulationState avatarState)
        {
            return this.remoteCoSimulationMMU.AssignInstruction(instruction, avatarState);
        }

        public override MBoolResponse Abort(string instructionId)
        {
            return this.remoteCoSimulationMMU.Abort(instructionId);
        }

        public override MSimulationResult DoStep(double time, MSimulationState avatarState)
        {
            //Call the remote cosimulation
            MSimulationResult result = this.remoteCoSimulationMMU.DoStep(time, avatarState);

            //Fire events
            if (result != null && result.Events != null && result.Events.Count > 0)
            {
                foreach (MSimulationEvent simEvent in result.Events)
                {
                    this.MSimulationEventHandler?.Invoke(this, simEvent);
                }
            }

            try
            {
                this.avatar.AssignPostureValues(result.Posture);

            }
            catch (Exception)
            {
                Debug.LogError("Problem assigning posture using remote co-simulation");
            }
            return result;
        }

        public override byte[] CreateCheckpoint()
        {
            return this.remoteCoSimulationMMU.CreateCheckpoint();
        }



        public override MBoolResponse RestoreCheckpoint(byte[] data)
        {
            return this.remoteCoSimulationMMU.RestoreCheckpoint(data);
        }

    }
}
