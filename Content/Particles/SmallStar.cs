using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class SmallStar : Particle
    {
        public override string Texture => "Radiance/Content/ExtraTextures/ThinStarFlare";
        public readonly float initialScale;
        public SmallStar(Vector2 position, Vector2 velocity, int maxTime, Color color, float scale, float rotation = 0)
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
            if(Progress >= scaleStart)
                scale = Lerp(initialScale, 0f, EaseOutCirc((Progress - scaleStart) / (1f - scaleStart)));

            velocity *= 0.9f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float colorMod = Min(1f, Progress * 10f) * 0.8f;

            Main.spriteBatch.Draw(ModContent.Request<Texture2D>("Radiance/Content/Particles/StarFlare").Value, drawPos, null, this.color * colorMod, rotation, texture.Size() / 2, this.scale, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.8f * colorMod, rotation, texture.Size() / 2, this.scale * 0.8f, SpriteEffects.None, 0);
        }
    }
}