// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;

namespace MMIStandard
{
    public static class MAvatarPostureValuesExtensions
    {

        public static MAvatarPostureValues Copy (this MAvatarPostureValues old)
        {
            MAvatarPostureValues newValues = new MAvatarPostureValues();
            newValues.AvatarID = old.AvatarID;
            newValues.PostureData = new List<double>(old.PostureData);
            return newValues;
        }
    }
}
