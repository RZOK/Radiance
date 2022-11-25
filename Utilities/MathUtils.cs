using System;
using Terraria;

namespace Radiance.Utilities
{
    static partial class RadianceUtils
    {
        public static double SineTiming(float sineTime) => Math.Sin(Main.GameUpdateCount / sineTime);
        public static double EaseInOutQuart(float x) => x < 0.5 ? 8 * Math.Pow(x, 4) : 1 - Math.Pow(-2 * x + 2, 4) / 2;
        public static double EaseOutCirc(float x) => Math.Sqrt(1 - Math.Pow(x - 1, 2));
        public static double EaseOutElastic(float x) => x == 0
              ? 0
              : x == 1
              ? 1
              : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * (2 * Math.PI / 3)) + 1;
    }
}