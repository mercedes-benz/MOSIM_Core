// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;

namespace MMICSharp.Common.Tools
{
    /// <summary>
    /// Class realizes a transition modeling based on posture blending and user-defined in and output postures
    /// </summary>
    public class TransitionModeler
    {
        /// <summary>
        /// Defines the posture which should be used as source for the transition
        /// </summary>
        public Func<float, MSimulationState, MAvatarPostureValues> GetSourcePosture;

        /// <summary>
        /// Defines the posture which should be used as target for the transition
        /// </summary>
        public Func<float, MSimulationState, MAvatarPostureValues> GetTargetPosture;

        /// <summary>
        /// The function for determining the blend weight.
        /// Default a linear weight curve is utilized.
        /// Can be optinally specified.
        /// As input the total elapsed time is given.
        /// The output should be the blend weight
        /// </summary>
        public Func<float, float> WeightFunction;


        /// <summary>
        /// The asignet intermediate skeleton
        /// </summary>
        public IntermediateSkeleton Skeleton;

        /// <summary>
        /// Indicates whether the transition is finished
        /// </summary>
        public bool Finished
        {
            get;
            private set;
        }

        /// <summary>
        /// Flag which specifies whether the TransitionModler is active
        /// </summary>
        public bool Active
        {
            get;
            private set;
        }


        /// <summary>
        /// The utilized blending mask.
        /// As default the roottransform and all relative rotations are used.
        /// </summary>
        public Dictionary<MJointType, BlendProperty> Mask;


        #region protected variables

        /// <summary>
        /// The elapsed time of the current transition
        /// </summary>
        protected float elapsedTime = 0;

        /// <summary>
        /// The overall blend time
        /// </summary>
        protected float blendTime = 1.0f;

        #endregion


        /// <summary>
        /// Basic constructor
        /// </summary>
        public TransitionModeler(IntermediateSkeleton skeleton)
        {
            this.elapsedTime = 0f;
            this.blendTime = 1.0f;
            this.Finished = false;
            this.Active = false;

            //Set default to root transform and rotation
            this.Mask = BlendingMask.RootTransformAndRotations;

            //Set default linear weight function
            this.WeightFunction = (float time) =>
            {
                return this.elapsedTime / this.blendTime;
            };

            this.Skeleton = skeleton;
        }

        /// <summary>
        /// Activates the transition modeller
        /// </summary>
        public void Activate()
        {
            this.Active = true;
        }

        /// <summary>
        /// Deactivates the transition modeller
        /// </summary>
        public void Deactivate()
        {
            this.Active = false;
        }


        /// <summary>
        /// Resets the current transition modeling
        /// </summary>
        public void Reset()
        {
            this.elapsedTime = 0f;
            this.Finished = false;
            this.Active = false;
        }


        /// <summary>
        /// Updates the transition modeler
        /// </summary>
        /// <param name="time"></param>
        /// <param name="avatarState"></param>
        /// <returns></returns>
        public MAvatarPostureValues ComputeResult(float time, MSimulationState avatarState)
        {
            //Do nothing if inactive
            if (!this.Active)
                return avatarState.Current;

            //Increment the time
            this.elapsedTime += time;

            //Estimate the blend weight -> to do use animation curves
            float blendWeight = this.WeightFunction(elapsedTime);

            //Set finished flag
            if (blendWeight >= 1)
                this.Finished = true;

            //Perform the blend and return the result
            return Blending.PerformBlend(this.Skeleton,  this.GetSourcePosture(time, avatarState), this.GetTargetPosture(time, avatarState), blendWeight, this.Mask);
        }
    }
}
