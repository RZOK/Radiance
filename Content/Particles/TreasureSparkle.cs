using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class TreasureSparkle : Particle
    {
        private Rectangle frame;
        private int rotationDir = 1;
        private float idealAlpha;
        private bool fadeIn = true;
        public override string Texture => "Radiance/Content/Particles/Sparkle";

        public TreasureSparkle(Vector2 position, Vector2 velocity, int maxTime, float alpha, float scale, Color color)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.alpha = 255;
            idealAlpha = alpha;
            this.color = color;
            this.scale = scale;
            rotationDir = Main.rand.Next(new[] { 1, -1 });

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
            if (fadeIn)
            {
                alpha -= 10;
                if (alpha <= idealAlpha)
                    fadeIn = false;
            }
            else
                alpha += 255f / maxTime;

            velocity *= 0.999f;
            rotation += 0.005f * rotationDir;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, frame, color * ((255 - alpha) / 255), rotation, frame.Size() / 2, scale, 0, 0);

            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            spriteBatch.Draw(softGlow, position - Main.screenPosition, null, color * ((255 - alpha) / 255) * 0.7f, 0, softGlow.Size() / 2, scale / 2, 0, 0);
        }
    }
}