using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class ExtractinatorDust : Particle
    {
        private Texture2D itemTexture;
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public Rectangle centerFrame;

        public ExtractinatorDust(Vector2 position, int maxTime, Texture2D itemTexture, float scale = 1)
        {
            this.position = position;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            velocity = Vector2.UnitY * Main.rand.NextFloat(0.8f, 1.7f);
            this.scale = scale;
            this.itemTexture = itemTexture;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Regular;
            rotation = Main.rand.NextFloat(Pi);
            behindTiles = true;

            int centerWidthHeight = 4;
            int padding = 2;
            centerFrame = new Rectangle(2 + Main.rand.Next(itemTexture.Width - padding * 2 - centerWidthHeight), 2 + Main.rand.Next(itemTexture.Height - padding * 2 - centerWidthHeight), centerWidthHeight, centerWidthHeight);
    }

        public override void Update()
        {
            rotation += velocity.Length() / 10;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Color color = Lighting.GetColor(position.ToTileCoordinates());
            spriteBatch.Draw(itemTexture, position - Main.screenPosition, centerFrame, color, rotation, centerFrame.Size() / 2, scale, 0, 0);

            int padding = 2;
            Rectangle topFrame = new Rectangle(itemTexture.Width / 2 - padding, 0, 4, 2);
            Rectangle bottomFrame = new Rectangle(itemTexture.Width / 2, itemTexture.Height - padding, 4, 2);
            Rectangle leftFrame = new Rectangle(0, itemTexture.Height / 2 - padding, 2, 4);
            Rectangle rightFrame = new Rectangle(itemTexture.Width - padding, itemTexture.Height / 2 - padding, 2, 4);
            spriteBatch.Draw(itemTexture, position - Main.screenPosition - Vector2.UnitY.RotatedBy(rotation) * 3 * scale, topFrame, color, rotation, topFrame.Size() / 2, scale, 0, 0);
            spriteBatch.Draw(itemTexture, position - Main.screenPosition + Vector2.UnitY.RotatedBy(rotation) * 3 * scale, bottomFrame, color, rotation, bottomFrame.Size() / 2, scale, 0, 0);
            spriteBatch.Draw(itemTexture, position - Main.screenPosition - Vector2.UnitX.RotatedBy(rotation) * 3 * scale, leftFrame, color, rotation, leftFrame.Size() / 2, scale, 0, 0);
            spriteBatch.Draw(itemTexture, position - Main.screenPosition + Vector2.UnitX.RotatedBy(rotation) * 3 * scale, rightFrame, color, rotation, rightFrame.Size() / 2, scale, 0, 0);
        }
    }
}