// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

using System;

namespace MMICSharp.Adapter
{
    /// <summary>
    /// Enum describes the log level utilized by the logger
    /// </summary>
    public enum Log_level
    {
        L_SILENT = 0,
        L_ERROR = 1,
        L_INFO = 2,
        L_DEBUG = 3,
    };

    /// <summary>
    /// Class providing for logging 
    /// </summary>
    public class Logger 
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static Logger Instance
        {
            get;
            set;
        } = new Logger();


        /// <summary>
        /// The assigned log level
        /// </summary>
        public Log_level Level
        {
            get;
            set;
        } = Log_level.L_SILENT;


        /// <summary>
        /// Creates a log
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="text"></param>
        public void CreateLog(Log_level logLevel, string text)
        {
            //Only log if the global log level allows it (e.g. if debug is desired -> all is logged)
            if (logLevel <= this.Level)
            {

                switch (logLevel)
                {
                    case Log_level.L_DEBUG:
                        this.LogDebug(text);
                        break;

                    case Log_level.L_INFO:
                        this.LogInfo(text);
                        break;

                    case Log_level.L_ERROR:
                        this.LogError(text);
                        break;
                }
            }
        }

        /// <summary>
        /// Method to log an error (must be overwritten by the child class)
        /// </summary>
        /// <param name="text"></param>
        protected virtual void LogError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now} ----> {text}");
            Console.ForegroundColor = ConsoleColor.White;

        }

        /// <summary>
        /// Method to log info messages (must be overwritten by the child class)
        /// </summary>
        /// <param name="text"></param>
        protected virtual void LogInfo(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{DateTime.Now} ----> {text}");
            Console.ForegroundColor = ConsoleColor.White;

        }

        /// <summary>
        /// Method to log deubg information (must be overwritten by the child class)
        /// </summary>
        /// <param name="text"></param>
        protected virtual void LogDebug(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{DateTime.Now} ----> {text}");
            Console.ForegroundColor = ConsoleColor.White;

        }


        /// <summary>
        /// Static logging method
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="text"></param>
        public static void Log(Log_level logLevel, string text)
        {
            //Call the assigned instance if defined
            Instance?.CreateLog(logLevel, text);
        }

    }
}
