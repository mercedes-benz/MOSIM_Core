using System.Collections.Generic;
using MMIStandard;
using System.IO;


namespace MMIStandard
{
    public static class MAvatarPostureExtensions
    {

        public static MAvatarPosture LoadMAvatarPostureFromFile(string filepath, string AvatarID)
        {
            string[] lines = (string[])File.ReadLines(filepath);
            List<MJoint> jointList = new List<MJoint>();
            int line_counter = 0;
            ParseJoint(lines, ref line_counter, jointList);

            MAvatarPosture ret = new MAvatarPosture(AvatarID, jointList);
            return ret;
        }

        public static void StorePostureToFile(this MAvatarPosture p, string filepath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
            }
        }

        public static string WriteJoint(this MJoint joint, string tap)
        {
            string desc = "";
            string nl = System.Environment.NewLine;

            if (joint.Parent == null)
            {
                desc += "HIERARCHY" + nl + "ROOT ";
            }
            else
            {
                desc += tap + "JOINT ";
            }
            desc += joint.ID;
            desc += nl + tap + "{" + nl;
            tap += "  ";
            desc += tap;
            desc += "OFFSET " + joint.Position.X + " " + joint.Position.Y + " " + joint.Position.Z + nl + tap;
            desc += "ROTATION " + joint.Rotation.W + " " + joint.Rotation.X + " " + joint.Rotation.Y + " " + joint.Rotation.Z + nl + tap;
            desc += "CHANNELS ";
            if (joint.Channels != null)
            {
                foreach (MChannel c in joint.Channels)
                {
                    desc += c.ToString() + " ";
                }
            }
            desc += nl;
            return desc;
        }

        private static MJoint ParseJoint(string[] mos, ref int line_counter, List<MJoint> mjointList)
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
                MJoint child = ParseJoint(mos, ref line_counter, mjointList);
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
        private static float[] parseFloatParameter(string[] floats, int end)
        {
            float[] flts = new float[end];
            for (int i = 1; i < end + 1; i++)
            {
                flts[i - 1] = float.Parse(floats[i], System.Globalization.CultureInfo.InvariantCulture);
            }
            return flts;
        }

        private static List<MChannel> MapChannels(string[] channels)
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
