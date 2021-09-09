// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;
using MMICSharp.Common;

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


        /// <summary>
        /// Creates partial MAvatarPostureValues for the provided joint list. 
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="PartialJointList"></param>
        /// <returns></returns>
        public static MAvatarPostureValues MakePartial(this MAvatarPostureValues vals, List<MJointType> PartialJointList)
        {
            List<MJoint> defaultList = ISDescription.GetDefaultJointList();
            MAvatarPostureValues ret = new MAvatarPostureValues(vals.AvatarID, new List<double>());
            ret.PartialJointList = PartialJointList;
            int id = 0;
            foreach (MJoint joint in defaultList)
            {
                if (PartialJointList.Contains(joint.Type))
                {
                    foreach (var x in joint.Channels)
                    {
                        ret.PostureData.Add(vals.PostureData[id]);
                        id++;
                    }
                }
                else
                {
                    id += joint.Channels.Count;
                }
            }

            return ret;
        }

        /// <summary>
        /// Overwrites the values in this MAvatarPostureValues with the other values depending on the other values partial joint list. In case, the other does not have a partial joint list or the list is empty, nothing is transfered. 
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static MAvatarPostureValues OverwriteWithPartial(this MAvatarPostureValues vals, MAvatarPostureValues other)
        {
            List<MJoint> defaultList = ISDescription.GetDefaultJointList();
            MAvatarPostureValues ret = new MAvatarPostureValues(vals.AvatarID, new List<double>());

            if(other.PartialJointList == null || other.PartialJointList.Count == 0)
            {
                return vals;
            }

            int id = 0;
            int idPartial = 0;
            foreach (MJoint joint in defaultList)
            {
                if (other.PartialJointList.Contains(joint.Type))
                {
                    foreach (var x in joint.Channels)
                    {
                        ret.PostureData.Add(other.PostureData[idPartial]);
                        id++;
                        idPartial++;
                    }
                }
                else
                {
                    foreach (var x in joint.Channels)
                    {
                        ret.PostureData.Add(vals.PostureData[id]);
                        id++;
                    }
                }
            }

            return ret;

        }


    }
}
