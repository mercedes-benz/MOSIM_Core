using Microsoft.VisualStudio.TestTools.UnitTesting;
using MMICSharp.Common;
using MMIStandard;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class IntermediateSkeletonTest
    {
        [TestMethod]
        public void TestSkeletonGeneration()
        {
            MAvatarDescription desc = IntermediateSkeleton.GenerateFromDescriptionFile("TestAvatar");
            Assert.AreEqual(desc.AvatarID, "TestAvatar");
            Assert.AreEqual(desc.ZeroPosture.Joints.Count, 19);

            IntermediateSkeleton skeleton = new IntermediateSkeleton();
            skeleton.InitializeAnthropometry(desc);
            Assert.AreEqual(desc, skeleton.GetAvatarDescription(desc.AvatarID));

            string[] jointnames = new string[] { "S1L5Joint", "T12L12Joint", "T1T2Joint", "C4C5Joint", "HeadJoint", "LeftShoulder", "LeftElbow", "LeftWrist", "RightShoulder", "RightElbow", "RightWrist", "LeftHip", "LeftKnee", "LeftAnkle", "LeftBall", "RightHip", "RightKnee", "RightAnkle", "RightBall"};
            for(int i = 0; i< desc.ZeroPosture.Joints.Count; i++)
            {
                MJoint joint = desc.ZeroPosture.Joints[i];
                Assert.AreEqual(joint.ID.ToString(), jointnames[i]);
            }

        }

        /*
        [TestMethod]
        public void TestApplyRotation()
        {
            MAvatarDescription desc = IntermediateSkeleton.GenerateFromDescriptionFile("TestAvatar", "Resources/SkeletonConfig.mos");
            IntermediateSkeleton skeleton = new IntermediateSkeleton();
            skeleton.InitializeAnthropometry(desc);
            List<double> rotationValues = new List<double>() { -0.93049, -0.02226, -0.00419, 5.10551, 0.99665, 81.87754, -0.99090, 3.61048, -1.90444, -0.25513, 0.93138, -2.06351, -0.01675, 0.25346, 1.53748, 2.12043, -3.96562, 9.42146, 84.43787, 0.78780, -13.86452, 8.89475, 8.81555, 9.52864, -4.82006, -1.86513, 13.40333, -85.71552, -0.52606, 24.30049, -6.17107, 5.60742, -8.03474, 1.99413, -4.31839, 10.24155, -0.63749, -4.59591, 7.32006, -0.09880, 24.72116, 0.02657, 5.04545, -14.48847, 4.43608, 0.00000, 0.00000, 0.00000, 1.81351, -3.53038, 0.27087, 0.00121, 25.37152, -0.00054, -3.37810, -14.86273, 0.56665, 0.00000, 0.00000, 0.00000};
            MAvatarPostureValues values = new MAvatarPostureValues(desc.AvatarID, rotationValues);

            skeleton.SetChannelData(values);
            for(int i = 0; i < desc.ZeroPosture.Joints.Count; i++)
            {
                MJoint joint = desc.ZeroPosture.Joints[i];
                MQuaternion val = skeleton.GetLocalJointRotation(desc.AvatarID, joint.Type);
                MVector3 referece = new MVector3(rotationValues[3 + i * 3 + 1], rotationValues[3 + i * 3 + 2], rotationValues[3 + i * 3 + 0]);
                Assert.IsTrue((val.X - referece.X) < 0.001 && (val.Y - referece.Y) < 0.001 && (val.Z - referece.Z) < 0.001, val + " not equal to " + referece + " for joint " + joint.Type);
            }

        }*/
        
        [TestMethod] 
        public void TestRootPosition()
        {
            MAvatarDescription desc = IntermediateSkeleton.GenerateFromDescriptionFile("TestAvatar");
            IntermediateSkeleton skeleton = new IntermediateSkeleton();
            skeleton.InitializeAnthropometry(desc);
            List<double> rotationValues = new List<double>() { 0,0,0, 0.86041, -0.01303, 0.50383, 0.07533, //S1L5
                0.00000, 0.00000, 0.00000, 1.00000, 0.00000, 0.00000, 0.00000, // T12L12
                0.00000, 0.00000, 0.00000, 1.00000, 0.00000, 0.00000, 0.00000, // T1T2
                0.00000, 0.00000, 0.00000, 1.00000, 0.00000, 0.00000, 0.00000, //C4C5
                0.00000, 0.00000, 0.00000, 0.98890, 0.04908, -0.13945, -0.01508, // Head
                0.00000, 0.00000, 0.00000, 0.74914, -0.35251, 0.02895, 0.56007, // LeftShoulder
                0.98560, 0.11136, -0.00962, 0.12689, // Left ELbow
                0.96542, -0.01250, 0.25953, 0.02139, // Left Wrist
                0.00000, 0.00000, 0.00000, 0.74411, 0.10420, 0.26279, -0.60530,
                0.95158, 0.28073, 0.07735, -0.09850, // Right Elbow
                0.99256, -0.00379, 0.11897, -0.02548, // right wrist
                0.94999, -0.28306, 0.12805, 0.03154, // Left hip
                0.97503, 0.22205, 0.00000, -0.00001, // Knee
                0.99439, -0.07404, 0.06580, 0.03709, // Ankle
                1.00000, 0.00000, 0.00000, 0.00000, // Toes
                0.99694, 0.07053, -0.02371, 0.02406, // Right Hip
                0.91716, 0.39852, 0.00000, 0.00000, // Knee
                0.99076, -0.12976, 0.02501, 0.03048, // Ankle
                1.00000, 0.00000, 0.00000, 0.00000 }; // Toes
            MAvatarPostureValues values = new MAvatarPostureValues(desc.AvatarID, rotationValues);

            skeleton.SetChannelData(values);
            MVector3 pos = skeleton.GetGlobalJointPosition(desc.AvatarID, MJointType.S1L5Joint);
            MVector3 gt = skeleton.GetRoot(desc.AvatarID).GetMJoint().Position;
            gt = new MVector3(gt.X + rotationValues[0], gt.Y + rotationValues[1], gt.Z + rotationValues[2]);
            System.Console.WriteLine("pos: {0}, {1}, {2}", pos.X, pos.Y, pos.Z);
            System.Console.WriteLine("gt: {0}, {1}, {2}", gt.X, gt.Y, gt.Z);
            Assert.IsTrue(System.Math.Abs(pos.X - gt.X) < 0.001);
            Assert.IsTrue(System.Math.Abs(pos.Y - gt.Y) < 0.001);
            Assert.IsTrue(System.Math.Abs(pos.Z -  gt.Z) < 0.001);


        }
    }
}
