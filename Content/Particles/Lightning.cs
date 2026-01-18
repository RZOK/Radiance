using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class Lightning : Particle
    {
        public override string Texture => "Radiance/Content/Particles/TestParticle";
        private Vector2 endPosition;
        private int pointCount;
        private Color colorToDraw;
        private float width;
        private float intensity;

        internal PrimitiveTrail TrailDrawer;
        private List<Vector2> pointCache;

        public Lightning(Vector2 position, Vector2 endPosition, Color color, int maxTime, float width, float intensity = 1f)
        {
            this.position = position;
            this.endPosition = endPosition;
            this.maxTime = timeLeft = maxTime;
            this.color = color;
            this.width = width;
            this.intensity = intensity;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            drawPixelated = true;
            pointCount = (int)MathF.Max(10, (Vector2.Distance(position, endPosition) / 16f));
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
            if (pointCache is null)
            {
                pointCache = new List<Vector2>();
                for (int i = 0; i < pointCount; i++)
                {
                    pointCache.Add(Vector2.Lerp(position, endPosition, i / (float)pointCount));
                }
            }
            if (Main.GameUpdateCount % 2 == 0)
            {
                for (int i = 1; i < pointCount - 1f; i++)
                {
                    pointCache[i] += Main.rand.NextVector2Circular(16f, 16f) * MathF.Pow(Progress, 0.2f) * intensity;
                }
            }
        }
        private Color TrailColor(float factor)
        {
            return colorToDraw * MathF.Pow(1f - Progress, 0.4f);
        }

        private float TrailWidth(float factor)
        {
            return width / 2f + width * MathF.Pow(MathF.Sin(Pi * factor), 2f) * MathF.Pow(1f - Progress, 0.5f) / 2f;
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
