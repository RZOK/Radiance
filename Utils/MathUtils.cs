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
        public static double easeInOutQuart(float x)
        {
            return x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2;
        }
        public static double easeOutCirc(float x)
        {
            return Math.Sqrt(1 - Math.Pow(x - 1, 2));
        }
    }
}