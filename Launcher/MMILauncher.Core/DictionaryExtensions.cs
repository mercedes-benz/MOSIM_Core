// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;
using System.Text;

namespace MMILauncher.Core
{
    /// <summary>
    /// Class provides extensions for dcitionaries being used within launcher
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Provides a readable string which comprises all entries of the dictionary
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string GetReadableString(this Dictionary<string, string> dict)
        {

            if (dict == null)
                return "";

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var entry in dict)
            {
                stringBuilder.AppendLine(entry.Key + " : " + entry.Value);
            }

            return stringBuilder.ToString();
        }
    }
}
