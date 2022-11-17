using Microsoft.Xna.Framework;
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

        public static Vector2 MultitileCenterWorldCoords(int i, int j)
        {
            return new Vector2(
            i * 16,
            j * 16
            ) -
            new Vector2(
            Main.tile[i, j].TileFrameX - (2 * Main.tile[i, j].TileFrameX / 18),
            Main.tile[i, j].TileFrameY - (2 * Main.tile[i, j].TileFrameY / 18)
            );
        }
        public static double EaseInOutQuart(float x)
        {
            return x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2;
        }
        public static double EaseOutCirc(float x)
        {
            return Math.Sqrt(1 - Math.Pow(x - 1, 2));
        }
        public static double EaseOutElastic(float x)
        {
            double c4 = (2 * Math.PI) / 3;

            return x == 0
              ? 0
              : x == 1
              ? 1
              : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;

        }
    }
}