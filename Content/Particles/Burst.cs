using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class Burst : Particle
    {
        private Color color1;
        private Color color2;
        private float targetScale;
        public override string Texture => "Radiance/Content/Particles/Burst";

        public Burst(Vector2 position, int maxTime, Color color1, Color color2, float scale = 1)
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
            color = Color.Lerp(color1, color2, Progress);
            scale = Lerp(0f, targetScale, Progress);
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, drawPos, null, color * (1f - Progress), rotation, tex.Size() / 2, scale, 0, 0);
        }
    }
}