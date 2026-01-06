using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class GlowSpeck : Particle
    {
        public override string Texture => "Radiance/Content/Particles/Speck";
        public readonly float initialScale;

        public GlowSpeck(Vector2 position, Vector2 velocity, int maxTime, Color color, float scale, float rotation = 0)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = timeLeft = maxTime;
            this.color = color;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            this.scale = initialScale = scale;
            this.rotation = rotation;
        }

        public override void Update()
        {
            float scaleStart = 0.8f;
            if (Progress >= scaleStart)
                scale = Lerp(initialScale, 0f, EaseOutCirc((Progress - scaleStart) / (1f - scaleStart)));

            velocity *= 0.925f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;

            Vector2 stretchedScale = new Vector2(scale - 0.5f * Min(1, MathF.Abs(Min(5f, velocity.Y))), scale + 0.5f * MathF.Abs(Min(5f, velocity.Y)));

            Main.spriteBatch.Draw(glowTexture, drawPos, null, color * 0.5f, rotation, glowTexture.Size() / 2, stretchedScale * 0.2f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, color, rotation, texture.Size() / 2, stretchedScale, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White, rotation, texture.Size() / 2, stretchedScale * 0.75f, SpriteEffects.None, 0);
        }
    }
}