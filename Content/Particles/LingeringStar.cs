using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class LingeringStar : Particle
    {
        public override string Texture => "Radiance/Content/Particles/StarSmall";
        public readonly float initialScale;
        public readonly float rotationSpeed;

        public LingeringStar(Vector2 position, Vector2 velocity, int maxTime, Color color, float scale, float rotation = 0, float rotationSpeed = 0)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = timeLeft = maxTime;
            this.color = color;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            this.scale = initialScale = scale;
            this.rotation = rotation;
            this.rotationSpeed = rotationSpeed;
        }

        public override void Update()
        {
            float scaleStart = 0.8f;
            if (Progress >= scaleStart)
                scale = Lerp(initialScale, 0f, EaseOutCirc((Progress - scaleStart) / (1f - scaleStart)));

            velocity *= 0.9f;
            rotation += velocity.Length() * velocity.X.NonZeroSign() * 0.05f * rotationSpeed;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;

            float randomScale = Main.rand.NextFloat(0.95f, 1.1f);
            float colorMod = Min(1f, Progress * 10f);

            Main.spriteBatch.Draw(glowTexture, drawPos, null, color * colorMod * 0.5f, rotation, glowTexture.Size() / 2, scale * 0.5f * randomScale, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, color * colorMod, rotation, texture.Size() / 2, scale * randomScale, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * colorMod, rotation, texture.Size() / 2, scale * 0.7f * randomScale, SpriteEffects.None, 0);
        }
    }
}