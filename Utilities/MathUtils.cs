namespace Radiance.Utilities
{
    public static partial class RadianceUtils
    {
        public static float SineTiming(float sineTime, float offset = 0) => (float)Math.Sin((Main.GameUpdateCount + offset) / sineTime);
        public static float EaseInSine(float x) => 1 - (float)Math.Cos(x * Math.PI / 2);
        public static float EaseOutSine(float x) => (float)Math.Sin(x * Math.PI / 2);

        public static float EaseInExponent(float x, float y) => (float)Math.Pow(x, y);
        public static float EaseOutExponent(float x, float y) => 1f - MathF.Pow(1f - x, y);
        public static float EaseInOutExponent(float x, float y) => x < 0.5f ? 8 * MathF.Pow(x, y) : 1 - MathF.Pow(-2 * x + 2, y) / 2;

        public static float EaseInCirc(float x) => (float)(1 - Math.Sqrt(1 - Math.Pow(x, 2)));
        public static float EaseOutCirc(float x) => (float)Math.Sqrt(1 - Math.Pow(x - 1, 2));
        public static float EaseInOutCirc(float x) => (float)(x < 0.5 ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2 : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2);
        public static float EaseOutElastic(float x) => (float)(x == 0
              ? 0
              : x == 1
              ? 1
              : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * (2 * Math.PI / 3)) + 1);

        public static float EaseInElastic(float x) 
        {
            float c4 = (float)(2 * Math.PI) / 3;

            return (float)(x == 0
              ? 0
              : x == 1
              ? 1
              : -Math.Pow(2, 10 * x - 10) * Math.Sin((x* 10 - 10.75) * c4));
        }
        public static float AngleFromLawOfCosines(float a, float b, float c) => (float)Math.Acos((a * a + b * b - c * c) / (2 * a * b));
        public static bool AABBvCircle(Rectangle rectangle, Vector2 center, float radius) //robbed from fables :blush:
        {
            float nearestX = Math.Max(rectangle.X, Math.Min(center.X, rectangle.X + rectangle.Size().X));
            float nearestY = Math.Max(rectangle.Y, Math.Min(center.Y, rectangle.Y + rectangle.Size().Y));
            return new Vector2(center.X - nearestX, center.Y - nearestY).Length() < radius;
        }
        public static int NonZeroSign(this int n) => n == 0 ? 1 : Math.Sign(n);
        public static int NonZeroSign(this float n) => Math.Sign(n) >= 0 ? 1 : -1;
    }
}