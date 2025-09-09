using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class FadeDust : Particle
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        private Texture2D dustTexture;
        private Rectangle dustFrame;
        private Color dustColor;

        public FadeDust(Vector2 position, Vector2 velocity, int maxTime, int dustTextureID, Rectangle dustFrame, Color dustColor)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Regular;
            this.dustFrame = dustFrame;
            this.dustColor = dustColor;
            this.scale = 1f;

            if (dustTextureID >= DustID.Count)
                dustTexture = ModContent.Request<Texture2D>(DustLoader.GetDust(type).Texture).Value;
            else
                dustTexture = TextureAssets.Dust.Value;
        }

        public override void Update()
        {
            rotation += velocity.Length() / 10;
            velocity *= Lerp(0.95f, 0.8f, Progress);
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Point tileCoords = position.ToTileCoordinates();
            Color color = Lighting.GetColor(tileCoords);
            Color drawColor = color;
            if (dustColor != new Color(0, 0, 0, 0))
                drawColor = (dustColor.ToVector4() * color.ToVector4()).ToColor();

            drawColor *= 1f - EaseInExponent(Progress, 3f);

            Main.spriteBatch.Draw(dustTexture, drawPos, dustFrame, drawColor, rotation, dustFrame.Size() / 2f, scale, SpriteEffects.None, 0);
        }
    }
}