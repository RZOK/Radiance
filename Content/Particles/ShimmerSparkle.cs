using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class ShimmerSparkle : Particle
    {
        private Rectangle frame;
        public override string Texture => "Radiance/Content/Particles/Sparkle";

        public ShimmerSparkle(Vector2 position, Vector2 velocity, int maxTime, float alpha, Color color, float scale = 1)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.alpha = alpha;
            this.color = color;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            frame = new Rectangle(0, 42, 14, 14);
        }

        public override void Update()
        {
            alpha = Lerp(0, 255, EaseInExponent(Progress, 2));
            velocity *= 0.85f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            float scaleLerp = Lerp(1.7f, 0.6f, Math.Clamp(Progress * 1.5f, 0, 1));
            Vector2 stretchedScale = new Vector2(1f * (2f - scaleLerp), scaleLerp) * scale;
            spriteBatch.Draw(tex, drawPos, frame, color * ((255 - alpha) / 255), rotation, frame.Size() / 2, stretchedScale * Main.rand.NextFloat(0.75f, 1.1f), 0, 0);
            spriteBatch.Draw(tex, drawPos, frame, Color.White * ((255 - alpha) / 255) * 0.8f, rotation, frame.Size() / 2, stretchedScale * Main.rand.NextFloat(0.75f, 1.1f) * 0.7f, 0, 0);

            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            spriteBatch.Draw(softGlow, drawPos, null, color * ((255 - alpha) / 255), 0, softGlow.Size() / 2, stretchedScale / 2.5f * Main.rand.NextFloat(0.75f, 1.1f), 0, 0);
        }
    }
}