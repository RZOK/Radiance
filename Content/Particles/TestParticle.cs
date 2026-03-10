using Radiance.Core.Systems;
using Radiance.Core.Visuals.Primitives;

namespace Radiance.Content.Particles
{
    public class TestParticle : Particle
    {
        public override string Texture => "Radiance/Content/Particles/TestParticle";
        internal PrimitiveCircle innerCircle;
        internal PrimitiveCircle outerCircle;

        private Color colorToDraw;
        private int pointCount;

        public TestParticle(Vector2 position, Vector2 velocity, int maxTime)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            
            mode = ParticleSystem.DrawingMode.Additive;
            color = CommonColors.RadianceColor1;
            drawPixelated = true;
            pointCount = 50;
        }

        public override void Update()
        {
            velocity *= 0.95f;
            float rotation = -velocity.ToRotation();
            if (velocity.X < 0)
                rotation += Pi;

            innerCircle ??= new PrimitiveCircle(pointCount, TrailWidth, TrailColor);
            innerCircle.SetPositionsEllipse(position - Vector2.UnitY.RotatedBy(rotation) * Lerp(-24, 24, MathF.Pow(Progress, 0.4f)), 20, 0, 0.2f, rotation, 0);

            outerCircle ??= new PrimitiveCircle(pointCount, TrailWidth, TrailColor);
            outerCircle.SetPositionsEllipse(position - Vector2.UnitY.RotatedBy(rotation) * Lerp(0, 12, MathF.Pow(Progress, 0.4f)), 36, 0, 0.2f, rotation, 0);
        }
        private Color TrailColor(float factor)
        {
            return colorToDraw * 0.7f * (1f - MathF.Pow(Progress, 1.8f));
        }

        private float TrailWidth(float factor)
        {
            return 1.5f * (1f - MathF.Pow(Progress, 1.8f));
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            drawPixelated = true;
            colorToDraw = color with { A = 255 };
            for (int i = 0; i < 4; i++)
            {
                innerCircle?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(TwoPi * (i / 4f)) * 2f);
            }
            colorToDraw = Color.White with { A = 255 };
            innerCircle?.Render(null, -Main.screenPosition);

            colorToDraw = color with { A = 255 };
            for (int i = 0; i < 4; i++)
            {
                outerCircle?.Render(null, -Main.screenPosition + Vector2.UnitX.RotatedBy(TwoPi * (i / 4f)) * 2f);
            }
            colorToDraw = Color.White with { A = 255 };
            outerCircle?.Render(null, -Main.screenPosition);
        }
    }
}
