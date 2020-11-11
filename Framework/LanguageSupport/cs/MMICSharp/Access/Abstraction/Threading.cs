// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MMICSharp.Access.Abstraction
{
    /// <summary>
    /// Further functions required for multithreading
    /// </summary>
    internal static class Threading
    {
        /// <summary>
        /// Helper function which executes multiple functions in parallel using the TPL and tasks.
        /// This method call is a blocking call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection of elements.</param>
        /// <param name="function">The function which should be executed.</param>
        /// <param name="timeout">The timeout after which the tasks should be aborted.</param>
        /// <returns></returns>
        public static bool ExecuteTasksParallel<T>(IEnumerable<T> collection, Action<T, CancellationTokenSource> function, TimeSpan timeout)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();

            foreach (var item in collection)
            {
                tasks.Add(Task.Run(() =>
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                    }

                    //Call the function
                    function(item, cts);
                }
                // all tasks have only one token
                , cts.Token));

            }
            // this will cancel all tasks after timeout from start
            bool finished = Task.WaitAll(tasks.ToArray(), timeout);

            //If there are still running tasks -> cancel all using the cancellation token
            if (!finished)
            {
                cts.Cancel();
            }

            //Dispose the token
            cts.Dispose();

            //Return whether all tasks have been finished
            return finished;
        }


        /// <summary>
        /// Executes the function as a new task
        /// </summary>
        /// <param name="function"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool ExecuteTask(Action<CancellationTokenSource> function, TimeSpan timeout)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task task = Task.Run(() =>
            {
                if (cts.Token.IsCancellationRequested)
                {
                    cts.Token.ThrowIfCancellationRequested();
                }

                //Call the function
                function(cts);
            } , cts.Token);

            bool finished = task.Wait(timeout);

            //If there are still running tasks -> cancel all using the cancellation token
            if (!finished)
            {
                cts.Cancel();
            }

            //Dispose the token
            cts.Dispose();

            //Return whether all tasks have been finished
            return finished;
        }
    }
}
