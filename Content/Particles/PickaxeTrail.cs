using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class PickaxeTrail : Particle
    {
        private readonly Texture2D pickaxe;
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public PickaxeTrail(Vector2 position, Texture2D pickaxe, int maxTime, float rotation, Color color, float scale = 1)
        {
            this.position = position;
            this.pickaxe = pickaxe;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.color = color;
            this.scale = scale;
            this.rotation = rotation;

            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            if(timeLeft < maxTime)
                spriteBatch.Draw(pickaxe, drawPos, null, color * (1f - Progress), rotation, pickaxe.Size() * scale / 2, scale, 0, 0);
        }
    }
}