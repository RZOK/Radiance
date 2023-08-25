using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class Ash : Particle
    {
        private readonly int variant;
        private Rectangle drawFrame => new Rectangle(0, variant * 8, 6, 6);
        public override string Texture => "Radiance/Content/Particles/Sprinkle";

        public Ash(Vector2 position, Vector2 velocity, int maxTime, float alpha, Color color, float scale = 1)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.alpha = alpha;
            this.color = color;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Regular;
            rotation = Main.rand.NextFloat(Pi);
            variant = Main.rand.Next(3);
        }

        public override void Update()
        {
            alpha += 255 / maxTime;
            velocity.Y += 0.08f;
            velocity.X += Main.windSpeedCurrent / 8f;
            rotation += velocity.Length() / 20;
            Point tileCoords = position.ToTileCoordinates();
            if (WorldGen.SolidTile(tileCoords))
                velocity *= 0f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, drawFrame, color * ((255 - alpha) / 255), rotation, drawFrame.Size() / 2, scale, 0, 0);
        }
    }
}