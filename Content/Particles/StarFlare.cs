using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class StarFlare : Particle
    {
        private Color color1;
        private Color color2;
        private float targetScale;
        private float initialAlpha;
        public override string Texture => "Radiance/Content/Particles/StarFlare";

        public StarFlare(Vector2 position, int maxTime, Color color1, Color color2, float scale = 1)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.color1 = color1;
            this.color2 = color2;
            targetScale = scale;
            this.scale = 0f;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void Update()
        {
            float easedProgress = EaseInExponent(Progress, 8f);
            color = Color.Lerp(color1, color2, easedProgress);
            scale = Lerp(targetScale, 0f, easedProgress);
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, drawPos, null, color, rotation, tex.Size() / 2, new Vector2(targetScale * 2f, scale) * (1f - Progress) * Main.rand.NextFloat(0.9f, 1.1f), 0, 0);
            spriteBatch.Draw(tex, drawPos, null, Color.White * 0.7f, rotation, tex.Size() / 2, new Vector2(targetScale * 2f, scale) * (1f - Progress) * 0.8f * Main.rand.NextFloat(0.9f, 1.1f), 0, 0);
        }
    }
}