// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

namespace MMIStandard
{
    public static class MRotationConstraintExtensions
    {
        public static double X(this MRotationConstraint rConstraint)
        {
            return rConstraint.Limits.X();
        }

        public static double Y(this MRotationConstraint rConstraint)
        {
            return rConstraint.Limits.Y();
        }

        public static double Z(this MRotationConstraint rConstraint)
        {
            return rConstraint.Limits.Z();
        }

        public static MVector3 GetVector3(this MRotationConstraint rConstraint)
        {
            return new MVector3(rConstraint.X(), rConstraint.Y(), rConstraint.Z());
        }

        public static MQuaternion GetQuaternion(this MRotationConstraint rConstraint)
        {
            return MQuaternionExtensions.FromEuler(rConstraint.GetVector3());
        }
    }
}
