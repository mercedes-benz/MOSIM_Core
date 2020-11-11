// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;

namespace MMICSharp.Common
{
    /// <summary>
    /// Class contains general useful extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns the value of the dictionary using multiple keys.
        /// The method returns the first available result conisdering the order of the keys.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="result"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool GetValue(this Dictionary<string,string> dict, out string result, params string[] keys)
        {
            bool success = false;
            result = "";

            foreach(string key in keys)
            {
                if (dict.ContainsKey(key))
                {
                    success = true;
                    result = dict[key];
                    break;
                }
            }

            return success;
        }


        /// <summary>
        /// Returns the value of the dictionary using multiple keys.
        /// Directly converts to a boolean value.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="result"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool GetValue(this Dictionary<string, string> dict, out bool result, params string[] keys)
        {
            bool success = false;
            result = false;

            foreach (string key in keys)
            {
                if (dict.ContainsKey(key))
                {
                    if(bool.TryParse(dict[key], out result))
                    {
                        success = true;
                        break;
                    }
                }
            }

            return success;
        }


        /// <summary>
        /// Returns the value of the dictionary using multiple keys.
        /// Directly converts to a float value.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="result"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool GetValue(this Dictionary<string, string> dict, out float result, params string[] keys)
        {
            bool success = false;
            result = 0f;

            foreach (string key in keys)
            {
                if (dict.ContainsKey(key))
                {
                    if (float.TryParse(dict[key], out result))
                    {
                        success = true;
                        break;
                    }
                }
            }

            return success;
        }


        /// <summary>
        /// Returns the value of the dictionary using multiple keys.
        /// Directly converts to a float value.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="result"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool GetValue(this Dictionary<string, string> dict, out double result, params string[] keys)
        {
            bool success = false;
            result = 0;

            foreach (string key in keys)
            {
                if (dict.ContainsKey(key))
                {
                    if (double.TryParse(dict[key], out result))
                    {
                        success = true;
                        break;
                    }
                }
            }

            return success;
        }


        /// <summary>
        /// Returns the value of the dictionary using multiple keys.
        /// Directly converts to a float value.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="result"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool GetValue(this Dictionary<string, string> dict, out int result, params string[] keys)
        {
            bool success = false;
            result = 0;

            foreach (string key in keys)
            {
                if (dict.ContainsKey(key))
                {
                    if (int.TryParse(dict[key], out result))
                    {
                        success = true;
                        break;
                    }
                }
            }

            return success;
        }
    }
}
