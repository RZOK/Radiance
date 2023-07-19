using Radiance.Core.Systems;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;

namespace Radiance.Content.Particles
{
    public class Burst : Particle
    {
        private Color color1;
        private Color color2;
        private float targetScale;
        public override string Texture => "Radiance/Content/Particles/Burst";

        public Burst(Vector2 position, int maxTime, float alpha, Color color1, Color color2, float scale = 1)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.alpha = alpha;
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
            alpha += 255f / maxTime;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, null, color * ((255 - alpha) / 255), rotation, tex.Size() / 2, scale, 0, 0);
        }
    }
}