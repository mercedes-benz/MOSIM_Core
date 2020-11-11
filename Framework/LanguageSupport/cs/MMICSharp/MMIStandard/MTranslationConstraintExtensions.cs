// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

namespace MMIStandard
{
    public static class MTranslationConstraintExtensions
    {
        public static double X(this MTranslationConstraint tConstraint)
        {
            return tConstraint.Limits.X();
        }

        public static double Y(this MTranslationConstraint tConstraint)
        {
            return tConstraint.Limits.Y();
        }

        public static double Z(this MTranslationConstraint tConstraint)
        {
            return tConstraint.Limits.Z();
        }

        public static MVector3 GetVector3(this MTranslationConstraint tConstraint)
        {
            return new MVector3(tConstraint.X(), tConstraint.Y(), tConstraint.Z());
        }
    }
}
