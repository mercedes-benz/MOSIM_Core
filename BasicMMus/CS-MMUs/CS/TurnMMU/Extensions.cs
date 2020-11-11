// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;

namespace TurnMMU
{
    public static class Extensions
    {

        public static String GetValue(this Dictionary<string, string> dict, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (dict.ContainsKey(key))
                    return dict[key];
            }

            return null;
        }

    }

}
