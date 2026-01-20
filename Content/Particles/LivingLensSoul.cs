using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class LivingLensSoul : Particle
    {
        public override string Texture => "Radiance/Content/Particles/LivingLensSoul";

        public int variant;

        public LivingLensSoul(Vector2 position, int maxTime, float scale = 1)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            rotation = Main.rand.NextFloat(Pi);
            color = Color.White;
        }

        public override void Update()
        {
            scale = Lerp(2f, 0.2f, MathF.Pow(Progress, 2f));
            velocity.Y -= 0.07f * Lerp(1f, 0f, MathF.Pow(Progress, 2f));
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, drawPos, null, color, rotation, tex.Size() / 2, scale, 0, 0);
        }
    }
}