// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace MMIUnity
{
    /// <summary>
    /// A main thread dispatcher to execute functions on the unity main thread
    /// </summary>
    public class MainThreadDispatcher:MonoBehaviour
    {
        /// <summary>
        /// The singleton instance
        /// </summary>
        public static MainThreadDispatcher Instance;

        #region private variables

        private ConcurrentQueue<Action> functionQueue = new ConcurrentQueue<Action>();
        private AutoResetEvent waitHandle = new AutoResetEvent(false);

        private bool frameFinished = false;
        private Timer timer;
        private Thread mainThread;


        #endregion

        /// <summary>
        /// Executes the specific function on the main thread and blocks until the execution is finished
        /// </summary>
        /// <param name="function"></param>
        public virtual void ExecuteBlocking(Action function)
        {
            //Check if already on main thread
            if (mainThread.Equals(Thread.CurrentThread))
            {
                //Directly execute the function and return afterwards
                function();
            }
            //Try to acquire main thread and wait until executed
            else
            {

                Mutex mutex = new Mutex(false);
                bool started = false;

                //Enque a new function
                this.functionQueue.Enqueue(delegate
                {
                    //Acquire mutex
                    mutex.WaitOne();

                    started = true;

                    function();

                    //Release mutex if function finished
                    mutex.ReleaseMutex();
                });

                //Set signal on main thread 
                this.waitHandle.Set();

                //Wait until started
                while (!started)
                {
                    //System.Threading.Thread.Sleep(0);
                }

                //Wait until the function is executed and finished 
                mutex.WaitOne();
            }
        }

        /// <summary>
        /// Exeuctes the function in a non blocking way
        /// </summary>
        /// <param name="function"></param>
        public virtual void ExecuteNonBlocking(Action function)
        {
            this.functionQueue.Enqueue(function);
            this.waitHandle.Set();
        }

        private void Awake()
        {
            Instance = this;

            //Create a new timer
            this.timer = new Timer(TimerCallback, "finished", 0, -1);

            //Set the main thread = Unity main thread, since the Awake is executed on the unity main thread
            this.mainThread = System.Threading.Thread.CurrentThread;

        }



        // Update is called once per frame
        protected virtual void Update()
        {
            frameFinished = false;

            //Estimate the prefered frametime in ms
            int prefered = Application.targetFrameRate == int.MaxValue ? 0 : (int)(1000.0f / Application.targetFrameRate);

            //Max time in ms
            int maxTime = Math.Max(prefered - 1, 0);

            //Create a new timer which fires after a specific time
            this.timer.Change((maxTime), -1);

            //Wait in frame
            while (!frameFinished)
            {
                //Wait unitl new event or no time left
                this.waitHandle.WaitOne();

                //Execute all functions
                while (functionQueue.Count > 0)
                {
                    Action function = null;
                    if (functionQueue.TryDequeue(out function))
                        function();
                }
            }
        }

        private void TimerCallback(object state)
        {
            frameFinished = true;
            waitHandle.Set();
        }
    }
}