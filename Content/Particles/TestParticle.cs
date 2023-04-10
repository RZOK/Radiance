using Microsoft.Xna.Framework;
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
            alpha = 200;
        }

        public override void Update()
        {
            rotation += 0.1f;
        }
    }
}