using System;
using Terraria;

namespace Radiance.Utilities
{
    static partial class RadianceUtils
    {
        public static float SineTiming(float sineTime) => (float)Math.Sin(Main.GameUpdateCount / sineTime);
        public static float EaseInOutQuart(float x) => (float)(x < 0.5 ? 8 * Math.Pow(x, 4) : 1 - Math.Pow(-2 * x + 2, 4) / 2);
        public static float EaseOutCirc(float x) => (float)Math.Sqrt(1 - Math.Pow(x - 1, 2));
        public static float EaseOutElastic(float x) => (float)(x == 0
              ? 0
              : x == 1
              ? 1
              : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * (2 * Math.PI / 3)) + 1);
    }
}