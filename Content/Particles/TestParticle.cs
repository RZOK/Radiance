using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class TestParticle : Particle
    {
        public override string Texture => "Radiance/Content/Particles/TestParticle";
        internal PrimitiveTrail TrailDrawer;
        private List<Vector2> pointCache;

        private Vector2 endPosition;
        private int pointCount;

        public TestParticle(Vector2 position, Vector2 velocity, int maxTime)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            scale = 1;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            endPosition = position + Vector2.UnitX * 500f;
            drawPixelated = true;
            pointCount = (int)(Vector2.Distance(position, endPosition) / 16f);
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
                for (int i = 0; i < pointCount; i++)
                {
                    pointCache[i] += Main.rand.NextVector2Circular(16f, 16f) * MathF.Pow(Progress, 0.2f);
                }
            }
        }
        private Color TrailColor(float factor)
        {
            return color * MathF.Pow(1f - Progress, 0.4f);
        }

        private float TrailWidth(float factor)
        {
            return 2f + MathF.Sin(Pi * factor) * MathF.Pow(1f - Progress, 0.5f);
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            color = Color.Red with {A = 255};
            for (int i = 0; i < 4; i++)
            {
                TrailDrawer?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(TwoPi * (i / 4f)) * 2f);
            }
            color = Color.White with { A = 255 };
            TrailDrawer?.Render(null, -Main.screenPosition);
        }
    }
}
