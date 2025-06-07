using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class HiddenTextSparkle : Particle
    {
        public override string Texture => "Radiance/Content/Particles/HiddenTextSparkle";
        private const int TICKS_TO_FADEIN = 90;
        private const int TICKS_BEFORE_FADING_OUT = 90;

        public HiddenTextSparkle(Vector2 position, Vector2 velocity, int maxTime, float scale)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.scale = scale;
            specialDraw = true;
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
            }
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            for (int i = 0; i < 2; i++)
            {
                int dir = 1;
                if (i == 1)
                    dir = -1;

                spriteBatch.Draw(tex, drawPos + Vector2.UnitY * Progress * 4 * dir, null, Color.White * ((255 - alpha) / 255) * 0.3f, rotation, tex.Size() / 2, new Vector2(scale + 0.1f * -Progress, scale + 0.6f * Progress) * 1.3f, 0, 0);
            }

            spriteBatch.Draw(tex, drawPos, null, Color.White * ((255 - alpha) / 255), rotation, tex.Size() / 2, new Vector2(scale + 0.1f * -Progress, scale + 0.3f * Progress), 0, 0);

        }
    }
}