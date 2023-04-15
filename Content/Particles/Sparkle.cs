using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core.Systems;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Content.Particles
{
    public class Sparkle : Particle
    {
        private Rectangle frame;
        public override string Texture => "Radiance/Content/Particles/Sparkle";

        public Sparkle(Vector2 position, Vector2 velocity, int maxTime, float alpha, Color color, float scale = 1)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.alpha = alpha;
            this.color = color;
            this.scale = scale;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            rotation = Main.rand.NextFloat(MathHelper.Pi);
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
            alpha += 255 / maxTime;
            velocity *= 0.8f;
            rotation += velocity.Length() / 10;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, frame, color * ((255 - alpha) / 255), rotation, frame.Size() / 2, scale, 0, 0);

            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            spriteBatch.Draw(softGlow, position - Main.screenPosition, null, color * ((255 - alpha) / 255), 0, softGlow.Size() / 2, scale / 2.5f, 0, 0);
        }
    }
}