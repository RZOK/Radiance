using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class Ash : Particle
    {
        public override string Texture => "Radiance/Content/Particles/Ash";
        public Rectangle frame => variant switch
        {
            1 => new Rectangle(0, 8, 8, 8),
            2 => new Rectangle(0, 18, 8, 8),
            _ => new Rectangle(0, 0, 6, 6)
        };
        public int variant;

        public Ash(Vector2 position, int maxTime, float scale = 1)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Regular;
            variant = Main.rand.Next(3);
            rotation = Main.rand.NextFloat(Pi);
        }

        public override void Update()
        {
            alpha += 255 / maxTime;
            scale = Lerp(1.2f, 0.2f, Progress);
            velocity.Y -= 0.08f;
            velocity.X += Main.windSpeedCurrent / 10f;
            if (timeLeft % 5 == 0)
                variant = Main.rand.Next(3);

            color = Lighting.GetColor(position.ToTileCoordinates());
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, frame, color * ((255 - alpha) / 255), rotation, frame.Size() / 2, scale, 0, 0);
        }
    }
}