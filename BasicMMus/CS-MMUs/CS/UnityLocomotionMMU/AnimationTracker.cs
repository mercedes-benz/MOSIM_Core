// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using UnityEngine;

namespace UnityLocomotionMMU
{
    /// <summary>
    /// Class allows to track a specific transform over time to analyze the velocity
    /// </summary>
    public class AnimationTracker
    {
        /// <summary>
        /// The current (directed) velocity
        /// </summary>
        public Vector3 VelocityVector = Vector3.zero;

        /// <summary>
        /// The current velocity (magnitude)
        /// </summary>
        public float Velocity = 0f;

        /// <summary>
        /// The current angular velocity
        /// </summary>
        public float AngularVelocity = 0f;


        /// <summary>
        /// The current position
        /// </summary>
        public Vector3 Position = Vector3.zero;

        /// <summary>
        /// The current rotation
        /// </summary>
        public Quaternion Rotation;


        #region private fields

        private Vector3 lastPosition;
        private Quaternion lastRotation;

        private bool initialized = false;

        #endregion

        /// <summary>
        /// Rests the logs
        /// </summary>
        public void Reset()
        {
            this.Velocity = 0f;
            this.AngularVelocity = 0f;
            this.VelocityVector = Vector3.zero;
            this.initialized = false;
        }

        /// <summary>
        /// Updates the tracker with the most recent transform
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="time"></param>
        public void UpdateStats(Transform transform, float time)
        {
            if (!initialized)
            {
                this.lastPosition = transform.position;

                this.lastRotation = transform.rotation;

                this.initialized = true;
            }

            this.Position = transform.position;
            this.Rotation = transform.rotation;

            this.VelocityVector = (transform.position - this.lastPosition) / time;
            this.AngularVelocity = (UnityEngine.Quaternion.Angle(this.lastRotation, this.Rotation));
            this.Velocity = this.VelocityVector.magnitude;

            this.lastPosition = transform.position;
            this.lastRotation = transform.rotation;
        }

    }
}
