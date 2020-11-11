// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Janis Sprenger

using MMIStandard;
using System.Collections.Generic;

namespace MMICSharp.Common
{       
    /// <summary>
    ///  Constants describing the standardized Meta Skeleton
    /// </summary>
    public class ISDescription

    {
        public static MQuaternion identityQ = new MQuaternion(0, 0, 0, 1);
        public static List<MChannel> defaultJointChannels = new List<MChannel>() { MChannel.WRotation, MChannel.XRotation, MChannel.YRotation, MChannel.ZRotation};
        public static List<MChannel> defaultRootChannels = new List<MChannel>() { MChannel.XOffset, MChannel.YOffset, MChannel.ZOffset, MChannel.WRotation, MChannel.XRotation, MChannel.YRotation, MChannel.ZRotation };
        public static List<MChannel> zeroChannels = new List<MChannel>();
        private static MQuaternion qRotY180 = new MQuaternion(0, 1, 0, 0);
        private static MQuaternion qRotYM180 = new MQuaternion(0, -1, 0, 0);

        private static MQuaternion qRotY90 = new MQuaternion(0, 0.707106769, 0, 0.707106769);
        private static MQuaternion qRotYM90 = new MQuaternion(0, -0.707106769, 0, 0.707106769);

        private static MQuaternion qRotX90 = new MQuaternion(0.707106769, 0, 0, 0.707106769);
        private static MQuaternion qRotXM90 = new MQuaternion(-0.707106769, 0, 0, 0.707106769);
        private static MQuaternion qRotX180 = new MQuaternion(1, 0, 0, 0);
        private static MQuaternion qRotXM180 = new MQuaternion(-1, 0, 0, 0);

        private static MQuaternion qRotZ90 = new MQuaternion(0, 0, 0.707106769, 0.707106769);
        private static MQuaternion qRotZM90 = new MQuaternion(0, 0, -0.707106769, 0.707106769);
        private static MQuaternion qRotZ180 = new MQuaternion(0, 0, 1, 0);
        private static MQuaternion qRotZM180 = new MQuaternion(0, 0, -1, 0);


        private static MJoint NewMJoint(string id, MJointType type, MVector3 offset, MQuaternion rotation, string parentID, List<MChannel>channels)
        {
            MJoint j = new MJoint(id, type, offset, rotation);
            j.Parent = parentID;
            j.Channels = channels;
            return j;
        }
        public static List<MJoint> GetDefaultJointList()
        {
            List<MJoint> defaultJoints = new List<MJoint>() {
            // 7 joints along the spine (6 animated, 39 channels)
            NewMJoint("Root", MJointType.Root, new MVector3(0,0,0), identityQ, null, defaultRootChannels),
            NewMJoint("PelvisCenter", MJointType.PelvisCentre, new MVector3(0, 0, 0), qRotY90, "Root",defaultRootChannels),
            NewMJoint("S1L5Joint", MJointType.S1L5Joint, new MVector3(0, 0.18, 0), identityQ,"PelvisCenter",defaultRootChannels),
            NewMJoint("T12L1Joint", MJointType.T12L1Joint, new MVector3(0, 0.15, 0), identityQ, "S1L5Joint",defaultRootChannels), // 0.33 - 0.18
            NewMJoint("T1T2Joint", MJointType.T1T2Joint, new MVector3(0, 0.43, 0), identityQ, "T12L1Joint",defaultRootChannels), // 0.76 - 0.33
            NewMJoint("C4C5Joint", MJointType.C4C5Joint, new MVector3(0, 0.11, 0), identityQ, "T1T2Joint",defaultRootChannels), // 0.87 - 0.76
            NewMJoint("HeadJoint", MJointType.HeadJoint, new MVector3(0, 0.13, 0), qRotYM180, "C4C5Joint",defaultJointChannels), // 1.0 - 0.87
            NewMJoint("HeadTip", MJointType.HeadTip, new MVector3(0, 0.16, 0), identityQ, "HeadJoint", zeroChannels),


            // Left Arm: 3 joints (3 animated, 15 channels)
            NewMJoint("LeftShoulder", MJointType.LeftShoulder, new MVector3(0, 0, -1), qRotXM90.Multiply(qRotYM90), "T1T2Joint",defaultRootChannels),
            NewMJoint("LeftElbow", MJointType.LeftElbow, new MVector3(0, 1, 0), qRotY90, "LeftShoulder",defaultJointChannels),
            NewMJoint("LeftWrist", MJointType.LeftWrist, new MVector3(0, 1, 0), qRotY90, "LeftElbow",defaultJointChannels),

            //left hand: 20 joints (16 animated, 75 channels)
            NewMJoint("LeftMiddleProximal", MJointType.LeftMiddleProximal, new MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftMiddleMeta", MJointType.LeftMiddleMeta, new MVector3(0, 1, 0), identityQ, "LeftMiddleProximal",defaultJointChannels),
            NewMJoint("LeftMiddleDistal", MJointType.LeftMiddleDistal, new MVector3(0, 1, 0), identityQ, "LeftMiddleMeta",defaultJointChannels),
            NewMJoint("LeftMiddleTip", MJointType.LeftMiddleTip, new MVector3(0, 0.03, 0), identityQ, "LeftMiddleDistal", zeroChannels),

            NewMJoint("LeftIndexProximal", MJointType.LeftIndexProximal, new MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftIndexMeta", MJointType.LeftIndexMeta, new MVector3(0, 1, 0), identityQ, "LeftIndexProximal",defaultJointChannels),
            NewMJoint("LeftIndexDistal", MJointType.LeftIndexDistal, new MVector3(0, 1, 0), identityQ, "LeftIndexMeta",defaultJointChannels),
            NewMJoint("LeftIndexTip", MJointType.LeftIndexTip, new MVector3(0, 0.03, 0), identityQ, "LeftIndexDistal", zeroChannels),

            NewMJoint("LeftRingProximal", MJointType.LeftRingProximal, new MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftRingMeta", MJointType.LeftRingMeta, new MVector3(0, 1, 0), identityQ, "LeftRingProximal",defaultJointChannels),
            NewMJoint("LeftRingDistal", MJointType.LeftRingDistal, new MVector3(0, 1, 0), identityQ, "LeftRingMeta",defaultJointChannels),
            NewMJoint("LeftRingTip", MJointType.LeftRingTip, new MVector3(0, 0.03, 0), identityQ, "LeftRingDistal", zeroChannels),

            NewMJoint("LeftLittleProximal", MJointType.LeftLittleProximal, new MVector3(0, 1, 0), identityQ, "LeftWrist",defaultRootChannels),
            NewMJoint("LeftLittleMeta", MJointType.LeftLittleMeta, new MVector3(0, 1, 0), identityQ, "LeftLittleProximal",defaultJointChannels),
            NewMJoint("LeftLittleDistal", MJointType.LeftLittleDistal, new MVector3(0, 1, 0), identityQ, "LeftLittleMeta",defaultJointChannels),
            NewMJoint("LeftLittleTip", MJointType.LeftLittleTip, new MVector3(0, 0.03, 0), identityQ, "LeftLittleDistal", zeroChannels),

            NewMJoint("LeftThumbMid", MJointType.LeftThumbMid, new MVector3(0, 1, 0), new MQuaternion(-0.3826834559440613, 0, 0,  0.9238795042037964), "LeftWrist",defaultRootChannels),//0.53730, 0, 0, 0.84339
            NewMJoint("LeftThumbMeta", MJointType.LeftThumbMeta, new MVector3(0, 1, 0), identityQ, "LeftThumbMid",defaultJointChannels),
            NewMJoint("LeftThumbCarpal", MJointType.LeftThumbCarpal, new MVector3(0, 1, 0), identityQ, "LeftThumbMeta",defaultJointChannels),
            NewMJoint("LeftThumbTip", MJointType.LeftThumbTip, new MVector3(0, 0.03, 0), identityQ, "LeftThumbCarpal", zeroChannels),


            // Right Arm: 3 joints (3 animated, 15 channels)
            NewMJoint("RightShoulder", MJointType.RightShoulder, new MVector3(0, 0, 1), qRotX90.Multiply(qRotY90), "T1T2Joint",defaultRootChannels),
            NewMJoint("RightElbow", MJointType.RightElbow, new MVector3(0, 1, 0), qRotYM90, "RightShoulder",defaultJointChannels),
            NewMJoint("RightWrist", MJointType.RightWrist, new MVector3(0, 1, 0), qRotYM90, "RightElbow",defaultJointChannels),

            //left hand: 20 joints (16 animated, 75 channels)
            NewMJoint("RightMiddleProximal", MJointType.RightMiddleProximal, new MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightMiddleMeta", MJointType.RightMiddleMeta, new MVector3(0, 1, 0), identityQ, "RightMiddleProximal",defaultJointChannels),
            NewMJoint("RightMiddleDistal", MJointType.RightMiddleDistal, new MVector3(0, 1, 0), identityQ, "RightMiddleMeta",defaultJointChannels),
            NewMJoint("RightMiddleTip", MJointType.RightMiddleTip, new MVector3(0, 0.03, 0), identityQ, "RightMiddleDistal", zeroChannels),

            NewMJoint("RightIndexProximal", MJointType.RightIndexProximal, new MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightIndexMeta", MJointType.RightIndexMeta, new MVector3(0, 1, 0), identityQ, "RightIndexProximal",defaultJointChannels),
            NewMJoint("RightIndexDistal", MJointType.RightIndexDistal, new MVector3(0, 1, 0), identityQ, "RightIndexMeta",defaultJointChannels),
            NewMJoint("RightIndexTip", MJointType.RightIndexTip, new MVector3(0, 0.03, 0), identityQ, "RightIndexDistal", zeroChannels),

            NewMJoint("RightRingProximal", MJointType.RightRingProximal, new MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightRingMeta", MJointType.RightRingMeta, new MVector3(0, 1, 0), identityQ, "RightRingProximal",defaultJointChannels),
            NewMJoint("RightRingDistal", MJointType.RightRingDistal, new MVector3(0, 1, 0), identityQ, "RightRingMeta",defaultJointChannels),
            NewMJoint("RightRingTip", MJointType.RightRingTip, new MVector3(0, 0.03, 0), identityQ, "RightRingDistal", zeroChannels),

            NewMJoint("RightLittleProximal", MJointType.RightLittleProximal, new MVector3(0, 1, 0), identityQ, "RightWrist",defaultRootChannels),
            NewMJoint("RightLittleMeta", MJointType.RightLittleMeta, new MVector3(0, 1, 0), identityQ, "RightLittleProximal",defaultJointChannels),
            NewMJoint("RightLittleDistal", MJointType.RightLittleDistal, new MVector3(0, 1, 0), identityQ, "RightLittleMeta",defaultJointChannels),
            NewMJoint("RightLittleTip", MJointType.RightLittleTip, new MVector3(0, 0.03, 0), identityQ, "RightLittleDistal", zeroChannels),

            NewMJoint("RightThumbMid", MJointType.RightThumbMid, new MVector3(0, 1, 0), new MQuaternion(0.3826834559440613, 0, 0,  0.9238795042037964), "RightWrist",defaultRootChannels),//0.53730, 0, 0, 0.84339
            NewMJoint("RightThumbMeta", MJointType.RightThumbMeta, new MVector3(0, 1, 0), identityQ, "RightThumbMid",defaultJointChannels),
            NewMJoint("RightThumbCarpal", MJointType.RightThumbCarpal, new MVector3(0, 1, 0), identityQ, "RightThumbMeta",defaultJointChannels),
            NewMJoint("RightThumbTip", MJointType.RightThumbTip, new MVector3(0, 0.03, 0), identityQ, "RightThumbCarpal", zeroChannels),

            // Left leg: 5 joints (4 animated, 16 channels)
            NewMJoint("LeftHip", MJointType.LeftHip, new MVector3(0, 0, -1), qRotZ180, "PelvisCenter",defaultJointChannels),
            NewMJoint("LeftKnee", MJointType.LeftKnee, new MVector3(0, 1, 0), identityQ, "LeftHip",defaultJointChannels),
            NewMJoint("LeftAnkle", MJointType.LeftAnkle, new MVector3(0, 1, 0), new MQuaternion(0, 0, -0.53730, 0.84339), "LeftKnee",defaultJointChannels),
            NewMJoint("LeftBall", MJointType.LeftBall, new MVector3(0, 1, 0), new MQuaternion(0, 0, -0.2164396196603775,  0.9762960076332092), "LeftAnkle",defaultJointChannels),
            NewMJoint("LeftBallTip", MJointType.LeftBallTip, new MVector3(0, 0.08, 0), identityQ, "LeftBall", zeroChannels),

            // Right leg: 5 joints (4 animated, 16 channels)
            NewMJoint("RightHip", MJointType.RightHip, new MVector3(0, 0, 1), qRotZ180, "PelvisCenter",defaultJointChannels),
            NewMJoint("RightKnee", MJointType.RightKnee, new MVector3(0, 1, 0), identityQ, "RightHip",defaultJointChannels),
            NewMJoint("RightAnkle", MJointType.RightAnkle, new MVector3(0, 1, 0), new MQuaternion(0, 0, -0.53730, 0.84339), "RightKnee",defaultJointChannels),
            NewMJoint("RightBall", MJointType.RightBall, new MVector3(0, 1, 0), new MQuaternion(0, 0, -0.2164396196603775,  0.9762960076332092), "RightAnkle",defaultJointChannels),
            NewMJoint("RightBallTip", MJointType.RightBallTip, new MVector3(0, 0.08, 0), identityQ, "RightBall", zeroChannels)
            };
            return defaultJoints;
        }



        public static List<MJoint> GetJointListFromFile(string filepath)
        {
            string[] mos = System.IO.File.ReadAllLines(filepath);
            for (int i = 0; i < mos.Length; i++)
            {
                mos[i] = mos[i].Trim();
            }
            int line_counter = 0;

            if (mos[line_counter] == "HIERARCHY")
            {
                line_counter += 1;
            }
            List<MJoint> MJointList = new List<MJoint>();
            ParseJoint(mos, ref line_counter, ref MJointList);
            return MJointList;
            //return defaultJoints;
        }

        private static MJoint ParseJoint(string[] mos, ref int line_counter, ref List<MJoint> mjointList)
        {
            // parse lines for current joint
            // Todo: Improve parser to consider empty lines and comments
            string name = mos[line_counter].Split(' ')[1];
            float[] off = parseFloatParameter(mos[line_counter + 2].Split(' '), 3);
            MVector3 offset = new MVector3(off[0], off[1], off[2]);
            float[] quat = parseFloatParameter(mos[line_counter + 3].Split(' '), 4);
            MQuaternion rotation = new MQuaternion(quat[1], quat[2], quat[3], quat[0]);
            string[] channels = mos[line_counter + 4].Replace("CHANNELS", "").Split(' ');
            List<MChannel> mchannels = MapChannels(channels);


            MJoint mjoint = new MJoint(name, MJointTypeMap[name], offset, rotation);
            mjoint.Channels = mchannels;
            mjointList.Add(mjoint);

            line_counter += 5;
            while (!mos[line_counter].Contains("}"))
            {
                MJoint child = ParseJoint(mos, ref line_counter, ref mjointList);
                child.Parent = mjoint.ID;
            }
            line_counter += 1;

            return mjoint;
        }

        /// <summary>
        /// Helper function to parse floats from strings. 
        /// </summary>
        /// <param name="floats"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static float[] parseFloatParameter(string[] floats, int end)
        {
            float[] flts = new float[end];
            for (int i = 1; i < end + 1; i++)
            {
                flts[i - 1] = float.Parse(floats[i], System.Globalization.CultureInfo.InvariantCulture);
            }
            return flts;
        }

        public static List<MChannel> MapChannels(string[] channels)
        {
            List<MChannel> mchannels = new List<MChannel>();

            foreach (string c in channels)
            {
                if (c == "XTranslation")
                {
                    mchannels.Add(MChannel.XOffset);
                }
                else if (c == "YTranslation")
                {
                    mchannels.Add(MChannel.YOffset);
                }
                else if (c == "ZTranslation")
                {
                    mchannels.Add(MChannel.ZOffset);
                }
                else if (c == "Wrotation")
                {
                    mchannels.Add(MChannel.WRotation);
                }
                else if (c == "Xrotation")
                {
                    mchannels.Add(MChannel.XRotation);
                }
                else if (c == "Yrotation")
                {
                    mchannels.Add(MChannel.YRotation);
                }
                else if (c == "Zrotation")
                {
                    mchannels.Add(MChannel.ZRotation);
                }
            }
            return mchannels;
        }

        private static Dictionary<string, MJointType> MJointTypeMap = new Dictionary<string, MJointType>()
    {
            {"Undefined", MJointType.Undefined},
            {"Root", MJointType.Root },
{"LeftBallTip", MJointType.LeftBallTip},
{"LeftBall", MJointType.LeftBall},
{"LeftAnkle", MJointType.LeftAnkle},
{"LeftKnee", MJointType.LeftKnee},
{"LeftHip", MJointType.LeftHip},


{"RightBallTip", MJointType.RightBallTip},
{"RightBall", MJointType.RightBall},
{"RightAnkle", MJointType.RightAnkle},
{"RightKnee", MJointType.RightKnee},
{"RightHip", MJointType.RightHip},


{"PelvisCentre", MJointType.PelvisCentre},
{"S1L5Joint", MJointType.S1L5Joint},
{"T12L1Joint", MJointType.T12L1Joint},
{"T1T2Joint", MJointType.T1T2Joint},
{"C4C5Joint", MJointType.C4C5Joint},


{"HeadJoint", MJointType.HeadJoint},
{"HeadTip", MJointType.HeadTip},
{"MidEye", MJointType.MidEye},
{"LeftShoulder", MJointType.LeftShoulder},
{"LeftElbow", MJointType.LeftElbow},
{"LeftWrist", MJointType.LeftWrist},
{"RightShoulder", MJointType.RightShoulder},
{"RightElbow", MJointType.RightElbow},
{"RightWrist", MJointType.RightWrist},


{"LeftThumbMid", MJointType.LeftThumbMid},
{"LeftThumbMeta", MJointType.LeftThumbMeta},
{"LeftThumbCarpal", MJointType.LeftThumbCarpal},
{"LeftThumbTip", MJointType.LeftThumbTip},

{"LeftIndexMeta", MJointType.LeftIndexMeta},
{"LeftIndexProximal", MJointType.LeftIndexProximal},
{"LeftIndexDistal", MJointType.LeftIndexDistal},
{"LeftIndexTip", MJointType.LeftIndexTip},

{"LeftMiddleMeta", MJointType.LeftMiddleMeta},
{"LeftMiddleProximal", MJointType.LeftMiddleProximal},
{"LeftMiddleDistal", MJointType.LeftMiddleDistal},
{"LeftMiddleTip", MJointType.LeftMiddleTip},

{"LeftRingMeta", MJointType.LeftRingMeta},
{"LeftRingProximal", MJointType.LeftRingProximal},
{"LeftRingDistal", MJointType.LeftRingDistal},
{"LeftRingTip", MJointType.LeftRingTip},

{"LeftLittleMeta", MJointType.LeftLittleMeta},
{"LeftLittleProximal", MJointType.LeftLittleProximal},
{"LeftLittleDistal", MJointType.LeftLittleDistal},
{"LeftLittleTip", MJointType.LeftLittleTip},


{"RightThumbMid", MJointType.RightThumbMid},
{"RightThumbMeta", MJointType.RightThumbMeta},
{"RightThumbCarpal", MJointType.RightThumbCarpal},
{"RightThumbTip", MJointType.RightThumbTip},

{"RightIndexMeta", MJointType.RightIndexMeta},
{"RightIndexProximal", MJointType.RightIndexProximal},
{"RightIndexDistal", MJointType.RightIndexDistal},
{"RightIndexTip", MJointType.RightIndexTip},

{"RightMiddleMeta", MJointType.RightMiddleMeta},
{"RightMiddleProximal", MJointType.RightMiddleProximal},
{"RightMiddleDistal", MJointType.RightMiddleDistal},
{"RightMiddleTip", MJointType.RightMiddleTip},

{"RightRingMeta", MJointType.RightRingMeta},
{"RightRingProximal", MJointType.RightRingProximal},
{"RightRingDistal", MJointType.RightRingDistal},
{"RightRingTip", MJointType.RightRingTip},

{"RightLittleMeta", MJointType.RightLittleMeta},
{"RightLittleProximal", MJointType.RightLittleProximal},
{"RightLittleDistal", MJointType.RightLittleDistal},
{"RightLittleTip", MJointType.RightLittleTip }
    };

    }
}
