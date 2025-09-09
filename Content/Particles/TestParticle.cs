using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class TestParticle : Particle
    {
        public override string Texture => "Radiance/Content/Particles/TestParticle";

        public TestParticle(Vector2 position, Vector2 velocity, int maxTime)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            color = Color.White;
            scale = 1;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void Update()
        {
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/RayTiling2").Value;
            float rotation = 0.5f;
            float speed = 5.5f;
            int tileWidth = 12;
            int tileHeight = 256;
            int totalHeight = 300;
            Color color = CommonColors.RadianceColor1;
            spriteBatch.DrawScrollingSprite(tex, drawPos, tileWidth, tileHeight, totalHeight, color, speed, rotation);
        }
    }
}