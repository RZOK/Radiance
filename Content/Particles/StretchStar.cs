using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class StretchStar : Particle
    {
        private Rectangle frame;
        public override string Texture => "Radiance/Content/Particles/StarSmall";

        public StretchStar(Vector2 position, Vector2 velocity, int maxTime, Color color, float scale = 1)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.color = color;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void Update()
        {
            velocity *= 0.825f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            float scaleLerp = Lerp(1.7f, 0.6f, Math.Clamp(Progress * 1.5f, 0, 1));
            Vector2 stretchedScale = new Vector2(1f * (2f - scaleLerp), scaleLerp) * scale;
            float randomScale = Main.rand.NextFloat(0.95f, 1.1f);
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            spriteBatch.Draw(softGlow, drawPos, null, color * EaseOutExponent(1f - Progress, 2f), 0, softGlow.Size() / 2, stretchedScale / 2.5f * randomScale, 0, 0);

            spriteBatch.Draw(tex, drawPos, null, color * EaseOutExponent(1f - Progress, 2f), rotation, tex.Size() / 2, stretchedScale * randomScale, 0, 0);
            spriteBatch.Draw(tex, drawPos, null, Color.White * EaseOutExponent(1f - Progress, 2f) * 0.8f, rotation, tex.Size() / 2, stretchedScale * randomScale * 0.7f, 0, 0);
        }
    }
}