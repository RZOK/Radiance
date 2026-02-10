using Radiance.Core.Systems;
using Radiance.Core.Visuals.Primitives;

namespace Radiance.Content.Particles
{
    public class TestParticle : Particle
    {
        public override string Texture => "Radiance/Content/Particles/TestParticle";
        internal PrimitiveCircle circle;
        private List<Vector2> pointCache;

        private Color colorToDraw;
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
            color = new Color(189, 106, 43);
            endPosition = position + Vector2.UnitX * 500f;
            drawPixelated = true;
            pointCount = 50;
        }

        public override void Update()
        {
            circle ??= new PrimitiveCircle(pointCount, TrailWidth, TrailColor);
            circle.SetPositions(position, 128f * MathF.Pow(1f - Progress, 5f) + 3f);   
        }
        private Color TrailColor(float factor)
        {
            return colorToDraw * 0.7f * MathF.Pow(Progress, 0.7f);
        }

        private float TrailWidth(float factor)
        {
            return 2f * MathF.Pow(Progress, 1.2f);
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            colorToDraw = color with { A = 255 };
            for (int i = 0; i < 4; i++)
            {
                circle?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(TwoPi * (i / 4f)) * 2f);
            }
            colorToDraw = Color.White with { A = 255 };
            circle?.Render(null, -Main.screenPosition);
        }
    }
}
