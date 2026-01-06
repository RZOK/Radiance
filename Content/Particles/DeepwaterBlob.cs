using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class DeepwaterBlob : Particle
    {
        public override string Texture => "Radiance/Content/Particles/DeepwaterBlob";
        private const int TICKS_TO_FADEIN = 10;
        private const int TICKS_FADING_OUT = 10;
        private float alpha;

        public DeepwaterBlob(Vector2 position, Vector2 velocity, int maxTime, float scale)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.scale = scale;
            specialDraw = true;
            alpha = 5;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void Update()
        {
            velocity *= 0.99f;
            if (timeLeft >= maxTime - TICKS_TO_FADEIN)
                alpha = Lerp(5, 1, EaseInExponent((float)(maxTime - timeLeft) / TICKS_TO_FADEIN, 3));

            if (timeLeft <= TICKS_FADING_OUT)
            {
                scale -= 0.8f / TICKS_FADING_OUT;
                alpha = Lerp(1, 5, EaseOutExponent(1f - ((float)timeLeft / TICKS_FADING_OUT), 3));
            }
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/SoftGlow").Value;
            Main.spriteBatch.Draw(glowTex, drawPos, null, Color.RoyalBlue * ((5 - alpha) / 5) * 0.5f, rotation, glowTex.Size() / 2, new Vector2(MathF.Pow(scale, 0.8f), MathF.Pow(1f / scale, 2f)) * 1.3f * Main.rand.NextFloat(0.75f, 1.1f) * 0.3f, SpriteEffects.None, 0);
            for (int i = -1; i < 1; i += 2)
            {
                spriteBatch.Draw(tex, drawPos + Vector2.UnitX * Progress * 8 * i, null, Color.White * ((5 - alpha) / 5) * 0.3f, rotation, tex.Size() / 2, new Vector2(MathF.Pow(scale, 0.8f), MathF.Pow(1f / scale, 2f)) * 1.3f, 0, 0);
            }
            spriteBatch.Draw(tex, drawPos, null, Color.White * ((5 - alpha) / 5), rotation, tex.Size() / 2, new Vector2(MathF.Pow(scale, 0.8f), MathF.Pow(1f / scale, 2f)), 0, 0);
        }
    }
}