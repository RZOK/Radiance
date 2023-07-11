using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class Cinder : Particle
    {
        private Color color1;
        private Color color2;
        public override string Texture => "Radiance/Content/Particles/Cinder";

        public Cinder(Vector2 position, Vector2 velocity, int maxTime, float alpha, Color color1, Color color2, float scale = 1)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.alpha = alpha;
            this.color1 = color1;
            this.color2 = color2;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            rotation = Main.rand.NextFloat(Pi);
        }

        public override void Update()
        {
            if (timeLeft > maxTime * 0.9f)
                velocity.X *= 1.05f;
            else
                velocity.X *= 0.95f;
            color = Color.Lerp(color1, color2, 1 - (float)timeLeft / maxTime);
            alpha += 255 / maxTime;
            velocity.Y += 0.08f;
            rotation += velocity.Length() / 10;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, null, color * ((255 - alpha) / 255), rotation, tex.Size() / 2, scale, 0, 0);
        }
    }
}