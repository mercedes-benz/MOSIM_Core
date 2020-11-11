// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using UnityEngine;

namespace UnityLocomotionMMU
{
    /// <summary>
    /// Represents a two dimensional position with a corresponding time. This kan be used as key-frame.
    /// </summary>
    public class TimedPose2D
    {
        /// <summary>
        /// The (absolute) time of the frame
        /// </summary>
        public TimeSpan Time;

        /// <summary>
        /// The position in 2D space
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Basic constructor with explicit time assignment
        /// </summary>
        /// <param name="position"></param>
        /// <param name="time"></param>
        public TimedPose2D(Vector2 position, TimeSpan time)
        {
            this.Time = time;
            this.Position = position;
        }

        /// <summary>
        /// Basic constructor with implicit time assignment
        /// </summary>
        /// <param name="position"></param>
        public TimedPose2D(Vector2 position)
        {
            this.Position = position;
            this.Time = System.TimeSpan.Zero;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TimedPose2D()
        {
        }

        /// <summary>
        /// Interpolates between two timed poses
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="toWeight"></param>
        /// <returns></returns>
        public static TimedPose2D Interpolate(TimedPose2D from, TimedPose2D to, float toWeight)
        {
            return new TimedPose2D(Vector2.Lerp(from.Position, to.Position, toWeight), TimeSpan.FromSeconds(from.Time.TotalSeconds + (to.Time.TotalSeconds - from.Time.TotalSeconds) * toWeight));
        }

    }

}
