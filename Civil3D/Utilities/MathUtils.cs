using System;

namespace Civil3D.Utilities
{
    public static class MathUtils
    {
        public static double RoundToNearest(this double value, double step)
        {
            return Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
        }
    }
}