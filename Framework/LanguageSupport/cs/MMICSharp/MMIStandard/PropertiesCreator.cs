// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.Collections.Generic;

namespace MMICSharp.MMIStandard.Utils
{
    public class PropertiesCreator
    {
        public static Dictionary<string,string> Create(params string[] properties)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            for(int i=0; i< properties.Length; i += 2)
            {
                dict.Add(properties[i], properties[i + 1]);
            }

            return dict;
        }
    }
}
