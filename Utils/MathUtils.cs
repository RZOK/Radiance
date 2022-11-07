using System;
using Terraria;

namespace Radiance.Utils
{
    public static class MathUtils
    {
        public static double sineTiming(float sineTime)
        {
            return Math.Sin(Main.GameUpdateCount / sineTime);
        }
    }
}
