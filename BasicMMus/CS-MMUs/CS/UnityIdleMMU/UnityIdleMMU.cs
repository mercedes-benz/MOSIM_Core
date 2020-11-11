// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using MMIUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityIdleMMU
{
    /// <summary>
    /// Implementation of a MMU which just plays an idle animation
    /// </summary>
    public class UnityIdleMMU : UnityMMUBase
    {
        private Animator animator;
        MAvatarPosture initialPosture;


        /// <summary>
        /// Method is called on awake
        /// </summary>
        protected override void Awake()
        {
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;

            //We do not need to set the root transform
            this.MotionType = "Pose/Idle";
            this.Name = "UnityIdleMMU";

            this.RootTransform = this.transform;
            this.Pelvis = this.GetComponentsInChildren<Transform>().First(s => s.name == "pelvis");

            base.Awake();
        }


        /// <summary>
        /// Basic initialization methid
        /// </summary>
        /// <param name="avatarDescription"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public override MBoolResponse Initialize(MAvatarDescription avatarDescription, Dictionary<string, string> properties)
        {
            //Execute instructions on main thread
            this.ExecuteOnMainThread(() =>
            {
                //Call the base class initialization -> Retargeting is also set up in there
                base.Initialize(avatarDescription, properties);

                this.MotionType = "idle";
                this.animator = this.GetComponent<Animator>();

                //Set animation mode to always animate (even if not visible)
                this.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                this.animator.enabled = false;
                this.Name = "UnityIdleMMU";


                //Get the initial posture
                this.animator.Update(0.01f);

                //Get the initial posture
                this.initialPosture = this.GetZeroPosture();

            });
            return new MBoolResponse(true);

        }


        /// <summary>
        /// Method to assign an instruction
        /// </summary>
        /// <param name="motionCommand"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MBoolResponse AssignInstruction(MInstruction motionInstruction, MSimulationState simulationState)
        {
            //Execute instructions on main thread
            this.ExecuteOnMainThread(() =>
            {
                //Assign the posture
                this.AssignPostureValues(simulationState.Current);


                //this.transform.position = this.SkeletonAccess.GetRootPosition(this.AvatarDescription.AvatarID).ToVector3();
                //this.transform.rotation = this.SkeletonAccess.GetRootRotation(this.AvatarDescription.AvatarID).ToQuaternion();
                //Update the animator initially
                this.animator.Update(0.1f);
            });

            return new MBoolResponse(true);
        }




        /// <summary>
        /// Method to compute a new posture for the next frame
        /// </summary>
        /// <param name="time"></param>
        /// <param name="simulationState"></param>
        /// <returns></returns>
        public override MSimulationResult DoStep(Double time, MSimulationState simulationState)
        {
            //Create a result to store all the computed data
            MSimulationResult result = new MSimulationResult()
            {
                //Just forward the present constraints
                Constraints = simulationState.Constraints ?? new List<MConstraint>(),
                Posture = simulationState.Current,
                SceneManipulations = simulationState.SceneManipulations
            };


            //Execute instructions on main thread
            this.ExecuteOnMainThread(() =>
            {
                this.SkeletonAccess.SetChannelData(simulationState.Current);

                this.AssignPostureValues(simulationState.Current);

                //this.transform.position = this.SkeletonAccess.GetRootPosition(this.AvatarDescription.AvatarID).ToVector3();
                //this.transform.rotation = this.SkeletonAccess.GetRootRotation(this.AvatarDescription.AvatarID).ToQuaternion();

                this.animator.Update((float)time);

                result.Posture = this.GetRetargetedPosture();             
            });

            return result;
        }

    }
}
