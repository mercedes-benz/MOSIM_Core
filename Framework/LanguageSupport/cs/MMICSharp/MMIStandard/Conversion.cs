// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

namespace MMIStandard
{
    public static class Conversion
    {
        public static MEndeffectorType ToEndeffectorType(this MJointType jointType)
        {
            switch (jointType)
            {
                case MJointType.LeftWrist:
                    return MEndeffectorType.LeftHand;
                case MJointType.RightWrist:
                    return MEndeffectorType.RightHand;
                case MJointType.LeftBall:
                    return MEndeffectorType.LeftFoot;
                case MJointType.RightBall:
                    return MEndeffectorType.RightHand;
                case MJointType.PelvisCentre:
                    return MEndeffectorType.Root;

            }
            return MEndeffectorType.Root;
        }
    }
}
