using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class Lightning : Particle
    {
        private Vector2 destination;
        private SpriteEffects spriteEffects = Main.rand.Next(new[] { SpriteEffects.None, SpriteEffects.FlipVertically });
        public override string Texture => "Radiance/Content/ExtraTextures/Lightning";
        private string displayedTexture;

        public Lightning(Vector2 position, Vector2 destination, Color color, int maxTime)
        {
            this.position = position;
            this.destination = destination;
            this.color = color;
            this.maxTime = maxTime;
            timeLeft = maxTime;

            scale = 1;
            mode = ParticleSystem.DrawingMode.Additive;
            specialDraw = true;
            displayedTexture = "Radiance/Content/ExtraTextures/Lightning" + Main.rand.Next(5);
        }

        public override void Update()
        {
            if (timeLeft % 3 == 0)
            {
                spriteEffects = Main.rand.Next(new[] { SpriteEffects.None, SpriteEffects.FlipVertically });
                scale = Main.rand.NextFloat(0.9f, 1.2f);
                displayedTexture = "Radiance/Content/ExtraTextures/Lightning" + Main.rand.Next(5);
            }
            //alpha = Lerp(255, 0, EaseOutCirc((float)timeLeft / maxTime));
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(displayedTexture).Value;
            //spriteBatch.Draw(tex, position - Main.screenPosition, new Rectangle(0, 0, (int)(Vector2.Distance(position, destination) / scale), tex.Height), color * (1 - alpha / 255), (destination - position).ToRotation(), Vector2.UnitY * tex.Size().Y / 2, scale, spriteEffects, 0);
            spriteBatch.Draw(tex, position - Main.screenPosition, null, color * (1 - alpha / 255), (destination - position).ToRotation(), Vector2.UnitY * tex.Size().Y / 2, new Vector2(Vector2.Distance(position, destination) / tex.Width, 1.1f), spriteEffects, 0);
            spriteBatch.Draw(tex, position - Main.screenPosition + Main.rand.NextVector2Square(-4, 4), null, color * (1 - alpha / 255) * 0.7f, (destination - position).ToRotation(), Vector2.UnitY * tex.Size().Y / 2, new Vector2(Vector2.Distance(position, destination) / tex.Width, 1.1f), spriteEffects, 0);
        }
    }
}