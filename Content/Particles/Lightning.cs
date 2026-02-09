using Radiance.Core.Systems;
using Radiance.Core.Visuals.Primitives;

namespace Radiance.Content.Particles
{
    public class Lightning : Particle
    {
        public override string Texture => "Radiance/Content/Particles/TestParticle";
        private int pointCount;
        private Color colorToDraw;
        private float width;
        private float intensity;

        internal PrimitiveTrail TrailDrawer;
        private List<Vector2> pointCache;

        public Lightning(List<Vector2> controlPoints, Color color, int maxTime, float width, float intensity = 1f)
        {
            if (controlPoints.Count < 2)
                throw new Exception($"Not enough control points input for lightning particle. Requires 2. Attempted to spawn with {controlPoints.Count}");

            this.position = controlPoints[0];
            this.maxTime = timeLeft = maxTime;
            this.color = color;
            this.width = width;
            this.intensity = intensity;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            drawPixelated = true;
            pointCount = (int)MathF.Max(10, (Vector2.Distance(position, controlPoints.Last()) / 16f));

            pointCache = new List<Vector2>();
            BezierCurve curve = new BezierCurve(controlPoints.ToArray());
            pointCache.AddRange(curve.GetEvenlySpacedPoints(pointCount));
        }

        public override void Update()
        {
            UpdateCache();

            TrailDrawer ??= new PrimitiveTrail(pointCount, TrailWidth, TrailColor);
            TrailDrawer.SetPositionsSmart(pointCache, position, RigidPointRetreivalFunction);
            TrailDrawer.NextPosition = position + velocity;
        }
        private void UpdateCache()
        {
            if (Main.GameUpdateCount % 2 == 0)
            {
                for (int i = 1; i < pointCount - 1f; i++)
                {
                    pointCache[i] += Main.rand.NextVector2Circular(12f, 12f) * MathF.Pow(Progress, 0.2f) * intensity;
                }
            }
        }
        private Color TrailColor(float factor)
        {
            return colorToDraw * MathF.Pow(1f - Progress, 0.4f);
        }

        private float TrailWidth(float factor)
        {
            return 0.5f + width * MathF.Sin(Pi * factor) * MathF.Pow(1f - Progress, 0.5f);
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            colorToDraw = color with { A = 255 };
            for (int i = 0; i < 4; i++)
            {
                TrailDrawer?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(TwoPi * (i / 4f)) * width);
            }
            colorToDraw = Color.White with { A = 255 };
            TrailDrawer?.Render(null, -Main.screenPosition);
        }
    }
}
