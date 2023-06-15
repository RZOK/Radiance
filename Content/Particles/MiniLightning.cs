using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class MiniLightning : Particle
    {
        private Vector2 destination;
        private Rectangle frame;
        private SpriteEffects spriteEffects = Main.rand.Next(new[] { SpriteEffects.None, SpriteEffects.FlipVertically });
        public override string Texture => "Radiance/Content/Particles/MiniLightning";

        public MiniLightning(Vector2 position, Vector2 destination, Color color, int maxTime)
        {
            this.position = position;
            this.destination = destination;
            this.color = color;
            this.maxTime = maxTime;
            timeLeft = maxTime;

            mode = ParticleSystem.DrawingMode.Additive;
            scale = 1;
            specialDraw = true;
            switch (Main.rand.Next(3))
            {
                case 0:
                    frame = new Rectangle(0, 0, 22, 18);
                    break;

                case 1:
                    frame = new Rectangle(24, 0, 22, 10);
                    break;

                case 2:
                    frame = new Rectangle(48, 0, 30, 10);
                    break;
            }
        }

        public override void Update()
        {
            if (timeLeft % 3 == 0)
            {
                switch (Main.rand.Next(3))
                {
                    case 0:
                        frame = new Rectangle(0, 0, 22, 18);
                        break;

                    case 1:
                        frame = new Rectangle(24, 0, 22, 10);
                        break;

                    case 2:
                        frame = new Rectangle(48, 0, 30, 10);
                        break;
                }
                spriteEffects = (SpriteEffects)Main.rand.Next(3);
                scale = Main.rand.NextFloat(0.9f, 1.2f);
            }
            alpha = MathHelper.Lerp(255, 0, EaseOutCirc((float)timeLeft / maxTime));
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            //spriteBatch.Draw(tex, position - Main.screenPosition, new Rectangle(0, 0, (int)(Vector2.Distance(position, destination) / scale), tex.Height), color * (1 - alpha / 255), (destination - position).ToRotation(), Vector2.UnitY * tex.Size().Y / 2, scale, spriteEffects, 0);
            spriteBatch.Draw(tex, position - Main.screenPosition, frame, color * (1 - alpha / 255), (destination - position).ToRotation(), Vector2.UnitY * frame.Size().Y / 2, new Vector2(Vector2.Distance(position, destination) / frame.Width, 1), spriteEffects, 0);
        }
    }
}