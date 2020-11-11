// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer


namespace MMIStandard
{
    public static class MIntervalExtensions
    {
        public static double X(this MInterval3 interval)
        {
            return (interval.X.Min+ interval.X.Max)/2f;
        }

        public static double Y(this MInterval3 interval)
        {
            return (interval.Y.Min + interval.Y.Max) / 2f;
        }

        public static double Z(this MInterval3 interval)
        {
            return (interval.Z.Min + interval.Z.Max) / 2f;
        }
    }
}
