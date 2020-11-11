// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Linq;
using System.Threading;

namespace MMICSharp.Adapter
{
    /// <summary>
    /// Class which optinally cleans the sessions based on a timeout
    /// </summary>
    public class SessionCleaner : IDisposable
    {
        public TimeSpan Timeout = TimeSpan.MaxValue;
        public TimeSpan UpdateTime = TimeSpan.FromMinutes(1);


        /// <summary>
        /// The utilized thread
        /// </summary>
        private readonly Thread thread;

        /// <summary>
        /// Task cancellation token used for terminating the thread
        /// </summary>
        private readonly CancellationTokenSource cts;

        /// <summary>
        /// The assigned session data which should be monitored
        /// </summary>
        private readonly SessionData SessionData;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="updateTime"></param>

        public SessionCleaner(SessionData sessionData)
        {
            this.thread = new Thread(new ThreadStart(this.ManageSessions));
            this.cts = new CancellationTokenSource();
            this.SessionData = sessionData;
        }



        /// <summary>
        /// Starts the thread
        /// </summary>
        public void Start()
        {
            //Only start if not already alive
            if (!this.thread.IsAlive)
            {
                this.thread.Start();
            }
        }

        /// <summary>
        /// Method which manages and cleans up the sessions
        /// </summary>
        private void ManageSessions()
        {
            //Do while no cancellation requested
            while (!this.cts.IsCancellationRequested)
            {
                //This can be done every n seconds
                Thread.Sleep(this.UpdateTime);

                //Check all sessions for timeout
                for (int i = SessionData.SessionContents.Count - 1; i >= 0; i--)
                {
                    SessionContent sessionContent = SessionData.SessionContents.ElementAt(i).Value;
                    string sessionID = SessionData.SessionContents.ElementAt(i).Key;

                    if (sessionContent.LastAccess != null && (DateTime.Now - sessionContent.LastAccess).Duration() > this.Timeout)
                    {
                        SessionData.SessionContents.TryRemove(sessionID, out sessionContent);

                        Logger.Log(Log_level.L_INFO, $"Session {sessionID} automatically removed due to timeout");
                    }
                }
            }
        }

        /// <summary>
        /// Basic dispose method which disposes the thread
        /// </summary>
        public void Dispose()
        {
            this.cts.Cancel();
            this.cts.Dispose();

        }
    }
}
