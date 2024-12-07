using Radiance.Content.NPCs;
using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class SoulofFlightJuice : Particle
    {
        private readonly int variant;
        private readonly float initialScale;
        private readonly float targetScale;
        private readonly int idealYVelocity = 2;
        private readonly float fullGrownRatio;
        private readonly WyvernHatchlingSegment segment;
        private readonly Vector2 segmentOffset = Vector2.Zero;
        private Rectangle drawFrame => new Rectangle(0, variant * 8, 6, 6);
        public override string Texture => "Radiance/Content/Particles/SoulofFlightJuice";
        public SoulofFlightJuice(Vector2 position, int maxTime, float fullGrownRatio = 0.5f, float scale = 0.1f, float targetScale = 1, WyvernHatchlingSegment segment = null)
        {
            this.maxTime = timeLeft = maxTime;
            this.scale = initialScale = scale;
            this.targetScale = targetScale;
            this.fullGrownRatio = fullGrownRatio;
            mode = ParticleSystem.DrawingMode.Additive;
            specialDraw = true;
            variant = Main.rand.Next(3);
            this.segment = segment;
            if (segment != null)
            {
                segmentOffset = position;
                this.position = segment.position + segmentOffset.RotatedBy(segment.rotation);
            }
            else
                this.position = position;

        }

        public override void Update()
        {
            if (Progress <= fullGrownRatio)
            {
                scale = Lerp(initialScale, targetScale, Math.Min(Progress / fullGrownRatio, 1f));
                if(segment != null)
                    position = segment.position + segmentOffset.RotatedBy(segment.rotation);
            }
            else
            {
                velocity.Y = Lerp(0, idealYVelocity, 0.1f);
                scale = Lerp(targetScale, 0, Math.Max((Progress - 0.75f) * 4, 0));
            }
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, drawPos, drawFrame, Color.White, rotation, drawFrame.Size() / 2, scale, 0, 0);
        }
    }
}