using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class RadiantFire : Particle
    {
        public override string Texture => "Radiance/Content/Particles/RadiantFire";
        public int variant;
        public float initialScale;

        public RadiantFire(Vector2 position, int maxTime, float scale = 1)
        {
            this.position = position;
            this.maxTime = timeLeft = maxTime;
            this.scale = initialScale = scale;

            mode = ParticleSystem.DrawingMode.Additive;
            variant = Main.rand.Next(4);
        }

        public override void Update()
        {
            scale = Lerp(initialScale, 0.2f, MathF.Pow(Progress, 2f));
            velocity.Y -= 0.02f;
            if (timeLeft % 6 == 0)
                variant = Main.rand.Next(3);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = new Rectangle(0, 14 * variant, 6, 12);
            spriteBatch.Draw(tex, drawPos, frame, Color.White, rotation, frame.Size() / 2, scale, 0, 0);

            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            spriteBatch.Draw(softGlow, drawPos, null, CommonColors.RadianceColor1 * (1f - Progress) * 0.7f, 0, softGlow.Size() / 2, new Vector2(0.5f, 1f) * scale * 0.25f, 0, 0);
        }
    }
}