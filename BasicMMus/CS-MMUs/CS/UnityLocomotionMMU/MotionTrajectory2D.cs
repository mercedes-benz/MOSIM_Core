// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityLocomotionMMU
{
    /// <summary>
    ///Implementation of a two dimensional motion trajectory
    /// </summary>
    public class MotionTrajectory2D
    {
        /// <summary>
        /// The corresponding poses/key frames
        /// </summary>
        public List<TimedPose2D> Poses;

        /// <summary>
        /// The overall duration of the motion
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (this.Poses.Count > 0)
                    return this.Poses.Last().Time;
                else
                    return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// The overall length [m] of the motion
        /// </summary>
        public float Length
        {
            get
            {
                float length = 0;
                for (int i = 0; i < this.Poses.Count - 1; i++)
                {
                    length += (this.Poses[i].Position - this.Poses[i + 1].Position).magnitude;
                }

                return length;
            }
        }


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="poses"></param>
        public MotionTrajectory2D(List<TimedPose2D> poses)
        {
            this.Poses = new List<TimedPose2D>(poses);
        }




        /// <summary>
        /// Creates a motion trajectory from MTransforms
        /// </summary>
        /// <param name="computedPath"></param>
        /// <returns></returns>
        public MotionTrajectory2D(List<MTransform> computedPath, float velocity):this(computedPath.Select(s => new MVector2(s.Position.X, s.Position.Z)).ToList(), velocity)
        {

        }

        /// <summary>
        /// Creates a motion trajectory from MTransforms
        /// </summary>
        /// <param name="computedPath"></param>
        /// <returns></returns>
        public MotionTrajectory2D(List<Vector2> computedPath, float velocity) : this(computedPath.Select(s => new MVector2(s.x, s.y)).ToList(), velocity)
        {

        }


        /// <summary>
        /// Createas motion trajectories from MVector2 list
        /// </summary>
        /// <param name="computedPath"></param>
        /// <returns></returns>
        public MotionTrajectory2D(List<MVector2> computedPath, float velocity)
        {
            //Extract the path
            List<Vector2> path = computedPath.Select(s => new Vector2((float)s.X, (float)s.Y)).ToList();

            this.Poses = new List<TimedPose2D>();

            foreach (Vector2 p in path)
            {
                //poseList.Add(new TimedPose(new Vector3D(p.x,0,p.y), IntelliAgent.Core.Math.Quaternion.Identity));
                this.Poses.Add(new TimedPose2D(p));
            }

            this.EstimateTimestamps(velocity);        
        }

        public void Append(List<Vector2> list)
        {
            foreach(Vector2 p in list)
            {
                this.Poses.Add(new TimedPose2D(p));
            }
        }

        /// <summary>
        /// Creates intermediate postures to achieve the desired frame rate 
        /// </summary>
        /// <param name="fps"></param>
        public void Fill(int fps)
        {
            this.Poses = this.SampleFrames(fps);
        }


        /// <summary>
        /// Returns frames according to the specified framerate
        /// </summary>
        /// <param name="fps"></param>
        /// <returns></returns>
        public List<TimedPose2D> SampleFrames(int fps)
        {
            List<TimedPose2D> poses = new List<TimedPose2D>();
            float frames = ((float)Duration.TotalSeconds * fps);
            float duration = (float)Duration.TotalSeconds;
            float delta = duration / frames;
            float value = 0;

            for (int f = 0; f < frames; f++)
            {
                TimedPose2D pose = this.GetPose(TimeSpan.FromSeconds(value));
                poses.Add(pose);
                value += delta;
            }

            return poses;
        }


        /// <summary>
        /// Returns an (interpolated) pose at the given time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TimedPose2D GetPose(TimeSpan time)
        {
            return this.ComputePose(this.Poses, time, InterpolationMode.CatmullRom);
        }


        /// <summary>
        /// Estimates the times of the individual poses/key frames based on a given velocity
        /// </summary>
        /// <param name="velocity"></param>
        public void EstimateTimestamps(float velocity)
        {
            if (this.Poses.Count == 0)
                return;

            float length = 0;

            //First pose has time zero
            this.Poses[0].Time = TimeSpan.Zero;

            //Iterate over the full path -> sum up the length
            for (int i = 1; i < this.Poses.Count; i++)
            {
                //Increment the length
                length += (this.Poses[i].Position - this.Poses[i - 1].Position).magnitude;

                //Estimate the required time
                float time = length / velocity;

                //Write back the newly estimated time
                this.Poses[i].Time = TimeSpan.FromSeconds(time);

                if (this.Poses[i].Time - this.Poses[i - 1].Time < TimeSpan.FromMilliseconds(1))
                {
                    this.Poses[i].Time += TimeSpan.FromMilliseconds(1);
                }
            }
        }


        /// <summary>
        /// Estimates the length of the trajectory from the specified position to the last point of the path
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="trajectory"></param>
        /// <returns></returns>
        public float GetTrajectoryLength(Vector2 currentPosition)
        {
            int index = GetClosetPosition(this.Poses.Select(s => s.Position).ToList(), currentPosition);

            if (index == -1)
            {
                return 0;
            }

            float length = 0;
            for (int i = index; i < this.Poses.Count - 1; i++)
            {
                length += (this.Poses[i].Position - this.Poses[i + 1].Position).magnitude;
            }

            return length;
        }


        #region protected methods

        /// <summary>
        /// Returns the pose at the given time.
        /// New implementation utilizes binary search for improved performance.
        /// </summary>
        /// <param name="poses">The input list of poses</param>
        /// <param name="time">The desired timestamp</param>
        /// <param name="interpolate">Specifies whether interpolation is used to generate a suitable pose at the timestamp </param>
        /// <returns></returns>
        protected TimedPose2D ComputePose(List<TimedPose2D> poses, System.TimeSpan time, InterpolationMode mode = InterpolationMode.Linear, bool extrapolate = true)
        {
            //The first index which is smaller than the actual time
            int from = 0;
            int to = 0;

            //Find the closest indices using binary search
            GetClosestIndices(poses, time, out from, out to);


            //First determine the weight
            float weight = 0;

            //Check if extrapoltion in positive direction is required
            if (poses[to].Time < time)
            {
                if (extrapolate)
                {
                    float deltaT = (float)(poses[to].Time - poses[from].Time).TotalSeconds;

                    //Use next point if both points are identical
                    if (deltaT == 0 && from > 0)
                    {
                        deltaT = (float)(poses[to].Time - poses[from - 1].Time).TotalSeconds;
                    }

                    float newDeltaT = (float)(time - poses[from].Time).TotalSeconds;


                    //Compute the weight
                    weight = newDeltaT / deltaT;
                }
                else
                {
                    weight = 1f;
                }
            }

            //Check if extrapolation in negative direction is required
            else if (poses[from].Time > time)
            {
                if (extrapolate)
                {
                    float deltaT = (float)(poses[from].Time - poses[to].Time).TotalSeconds;

                    //Use next point if both points are identical
                    if (deltaT == 0 && to < poses.Count - 1)
                    {
                        deltaT = (float)(poses[from].Time - poses[to + 1].Time).TotalSeconds;
                    }

                    float newDeltaT = (float)(poses[from].Time - time).TotalSeconds;

                    //Compute the weight
                    weight = newDeltaT / deltaT;
                }
                else
                {
                    weight = 0f;
                }
            }

            //Default behavior just interpolate between the two poses
            else
            {
                //Compute the weight
                float weightTo = (float)(time - poses[from].Time).Duration().TotalSeconds;
                float weightFrom = (float)(poses[to].Time - time).Duration().TotalSeconds;

                //Normalize the weight
                float sum = weightFrom + weightTo;

                //Check if sum is null 
                if (sum == 0)
                    weight = 0;
                else
                    weight = weightTo / sum;

            }

            //Check the speicifc interpolation mode
            switch (mode)
            {
                //Perform a linear interpolation
                case InterpolationMode.Linear:
                    return TimedPose2D.Interpolate(poses[from], poses[to], weight);

                //Perform a cutmull rom interpolation
                case InterpolationMode.CatmullRom:
                    //Only valid if 4 points are available
                    if (from > 0 && to < poses.Count - 1)
                    {
                        TimedPose2D result = new TimedPose2D();
                        result.Position = CatmullRom.ComputePoint(poses[from - 1].Position, poses[from].Position, poses[to].Position, poses[to + 1].Position, weight);
                        result.Time = System.TimeSpan.FromSeconds(CatmullRom.ComputePoint((float)poses[from - 1].Time.TotalSeconds, (float)poses[from].Time.TotalSeconds, (float)poses[to].Time.TotalSeconds, (float)poses[to + 1].Time.TotalSeconds, weight));
                        return result;
                    }
                    //Otherwise interpolate linear
                    else
                        return TimedPose2D.Interpolate(poses[from], poses[to], weight);


                //Get the closest point
                case InterpolationMode.None:
                    return (poses[from].Time - time).Duration() < (poses[to].Time - time).Duration() ? poses[from] : poses[to];

            }

            return poses.Last();
        }


        /// <summary>
        /// Returns the closest indices of the given list of TimedPoses.
        /// This information is useful for inter/extrapolation
        /// </summary>
        /// <param name="list"></param>
        /// <param name="time"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        protected static void GetClosestIndices(List<TimedPose2D> list, TimeSpan time, out int from, out int to)
        {
            int index = list.BinarySearch(new TimedPose2D(Vector2.zero, time), new TimedPoseComparer());
            if (index < 0)
            {
                index = ~index;
            }

            index = Mathf.Min(index, list.Count - 1);

            if (list[index].Time - time >= System.TimeSpan.Zero)
            {
                //The element is obviously bigger
                index = Mathf.Max(index - 1, 0);
            }

            from = index;
            to = Mathf.Min(list.Count - 1, from + 1);
        }


        /// <summary>
        /// Returns the index of the closest position of the trajectory
        /// </summary>
        /// <param name="trajectory"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        protected static int GetClosetPosition(List<Vector2> trajectory, Vector2 position)
        {
            float min = float.MaxValue;
            int index = -1;
            for (int i = 0; i < trajectory.Count; i++)
            {
                float dist = (trajectory[i] - position).magnitude;
                if (dist < min)
                {
                    min = dist;
                    index = i;
                }
            }
            return index;
        }

        protected class TimedPoseComparer : IComparer<TimedPose2D>
        {

            public int Compare(TimedPose2D x, TimedPose2D y)
            {
                if (x.Time > y.Time)
                    return 1;
                if (y.Time > x.Time)
                    return -1;

                return 0;
            }
        }

        #endregion
    }



}
