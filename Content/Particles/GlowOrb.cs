using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core.Systems;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Content.Particles
{
    public class GlowOrb : Particle
    {
        private Color innerColor;
        private Color outerColor;

        private float innerRadius;
        private float outerRadius;

        private readonly bool slowDown;
        public override string Texture => "Radiance/Content/ExtraTextures/SoftGlow";

        public GlowOrb(Vector2 position, Vector2 velocity, int maxTime, float innerRadius, float outerRadius, float alpha, Color outerColor, Color innerColor, bool slowDown = true)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.innerRadius = innerRadius;
            this.outerRadius = outerRadius;
            this.alpha = alpha;
            this.innerColor = innerColor;
            this.outerColor = outerColor;
            specialDraw = true;
            this.slowDown = slowDown;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void Update()
        {
            alpha += 255 / maxTime;
            if (slowDown)
                velocity *= 0.8f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D softGlow = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(softGlow, position - Main.screenPosition, null, outerColor * ((255 - alpha) / 255), 0, softGlow.Size() / 2, outerRadius / (softGlow.Width / 2), 0, 0);
            spriteBatch.Draw(softGlow, position - Main.screenPosition, null, innerColor * ((255 - alpha) / 255), 0, softGlow.Size() / 2, innerRadius / (softGlow.Width / 2), 0, 0);
        }
    }
}