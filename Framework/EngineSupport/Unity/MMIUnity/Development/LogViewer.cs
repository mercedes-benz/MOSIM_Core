// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.

using System.Collections;
using UnityEngine;

namespace MMIUnity.Development
{
    public class LogViewer : MonoBehaviour
    {
        private string logString;
        private Queue logQueue = new Queue();

        void Start()
        {

        }

        void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            this.logString = logString;
            string newString = "\n [" + type + "] : " + this.logString;
            logQueue.Enqueue(newString);

            if (type == LogType.Exception)
            {
                newString = "\n" + stackTrace;
                logQueue.Enqueue(newString);
            }
            this.logString = string.Empty;
            foreach (string mylog in logQueue)
            {
                this.logString += mylog;
            }
        }

        void OnGUI()
        {
            GUILayout.Label(logString);
        }
    }

}
