using Radiance.Core.Systems;
using Steamworks;

namespace Radiance.Content.Particles
{
    public class TravelSparkle : Particle
    {
        private Rectangle frame;
        private Vector2 startPosition;
        private Vector2 endPosition;
        private float alpha = 0;
        public override string Texture => "Radiance/Content/Particles/Sparkle";

        public TravelSparkle(Vector2 startPosition, Vector2 endPosition, int maxTime, Color color, float scale = 1)
        {
            this.startPosition = position = startPosition;
            this.endPosition = endPosition;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.color = color;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            rotation = Main.rand.NextFloat(Pi);
            switch (Main.rand.Next(4))
            {
                case 0:
                    frame = new Rectangle(0, 0, 14, 14);
                    break;

                case 1:
                    frame = new Rectangle(0, 16, 10, 10);
                    break;

                case 2:
                    frame = new Rectangle(0, 28, 12, 12);
                    break;

                case 3:
                    frame = new Rectangle(0, 42, 14, 14);
                    break;
            }
        }

        public override void Update()
        {
            float alphaTime = 180f;
            float positionProgression = EaseInExponent((1f - MathF.Pow(timeLeft / (float)maxTime, 2.5f)), 20f);

            float distance = Math.Max(64, startPosition.Distance(endPosition));
            Vector2 curvePoint = ((endPosition - startPosition) / 2f) + Vector2.UnitX.RotatedBy(Pi) * 60f;

            position = Vector2.Hermite(startPosition, curvePoint, endPosition, -curvePoint, positionProgression);
            //position = Vector2.Lerp(startPosition, endPosition, positionProgression);
            if (alpha < 1 && timeLeft > maxTime - alphaTime)
            {
                alpha += 1f / alphaTime;
            }
            if (timeLeft < alphaTime / 2f)
            {
                alpha -= 1f / (alphaTime / 2f);
            }
            //rotation += velocity.Length() / 10;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, drawPos, frame, color * alpha, rotation, frame.Size() / 2, scale, 0, 0);
            spriteBatch.Draw(tex, drawPos, frame, Color.White * 0.7f * alpha, rotation, frame.Size() / 2, scale * 0.7f, 0, 0);

            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            spriteBatch.Draw(softGlow, drawPos, null, color * alpha, 0, softGlow.Size() / 2, scale / 2.5f, 0, 0);
        }
    }
}