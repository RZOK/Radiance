using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core.Systems;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Content.Particles
{
    public class Sprinkle : Particle
    {
        private readonly int variant;
        private Rectangle drawFrame => new Rectangle(0, variant * 8, 6, 6);
        public override string Texture => "Radiance/Content/Particles/Sprinkle";

        public Sprinkle(Vector2 position, Vector2 velocity, int maxTime, float alpha, Color color, float scale = 1)
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
            variant = Main.rand.Next(3);
        }

        public override void Update()
        {
            alpha += 255 / maxTime;
            velocity *= 0.92f;
            velocity.Y += Main.rand.NextFloat(0.02f, 0.05f);
            rotation += velocity.Length() / 10;
            Point tileCoords = position.ToTileCoordinates();
            if (WorldGen.SolidTile(tileCoords))
                velocity *= 0f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, drawFrame, color * ((255 - alpha) / 255), rotation, drawFrame.Size() / 2, scale, 0, 0);

            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            spriteBatch.Draw(softGlow, position - Main.screenPosition, null, color * ((255 - alpha) / 255) * 0.5f, 0, softGlow.Size() / 2, scale / 5f, 0, 0);
        }
    }
}