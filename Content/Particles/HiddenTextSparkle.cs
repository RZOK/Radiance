using Radiance.Core.Systems;
using rail;

namespace Radiance.Content.Particles
{
    public class HiddenTextSparkle : Particle
    {
        public override string Texture => "Radiance/Content/Particles/HiddenTextSparkle";
        private readonly float rotationDir;
        private static readonly int TICKS_TO_FADEIN = 120;
        private static readonly int TICKS_BEFORE_FADING_OUT = 120;

        public HiddenTextSparkle(Vector2 position, int maxTime, float scale)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            rotation = Main.rand.NextFloat(Pi);
            rotationDir = Main.rand.NextFloat(-1.5f, 1.5f);
            alpha = 255;
        }

        public override void Update()
        {
            if (timeLeft >= maxTime - TICKS_TO_FADEIN)
                alpha = Lerp(255, 50, EaseInExponent((float)(maxTime - timeLeft) / TICKS_TO_FADEIN, 3));

            int timeModifier = maxTime - TICKS_TO_FADEIN - TICKS_BEFORE_FADING_OUT;
            if (timeLeft < timeModifier)
            {
                scale -= 0.2f / timeModifier;
                alpha = Lerp(50, 255, EaseOutExponent(1f - ((float)timeLeft / timeModifier), 3));
                rotation += 0.001f * rotationDir * (1f - ((float)timeLeft / timeModifier));
            }
            rotation += 0.001f * rotationDir;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position, null, Color.White * ((255 - alpha) / 255), rotation, tex.Size() / 2, scale, 0, 0);

        }
    }
}