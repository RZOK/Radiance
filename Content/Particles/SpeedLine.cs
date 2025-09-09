using Radiance.Core.Systems;

namespace Radiance.Content.Particles
{
    public class SpeedLine : Particle
    {
        public override string Texture => "Radiance/Content/Particles/BigLine";
        private readonly float initialTrailLength;
        private float trailLength;

        public SpeedLine(Vector2 position, Vector2 velocity, int maxTime, Color color, float rotation, float trailLength, float scale = 1)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.color = color;
            this.scale = scale;
            specialDraw = true;
            this.rotation = rotation;
            mode = ParticleSystem.DrawingMode.Additive;
            initialTrailLength = this.trailLength = trailLength;
        }

        public override void Update()
        {
            velocity *= 0.9f;
            trailLength = Lerp(initialTrailLength, 0, EaseOutExponent(Progress, 5f));
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 offset = new Vector2(tex.Width / 2, 0);
            spriteBatch.Draw(tex, drawPos, null, color, rotation + PiOver2, offset, new Vector2(0.4f, trailLength / tex.Height) * scale, 0, 0);
            spriteBatch.Draw(tex, drawPos, null, Color.White * 0.5f, rotation + PiOver2, offset, new Vector2(0.4f, trailLength / tex.Height) * scale * 0.8f, 0, 0);

            //Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/Particles/SpeedLine").Value;

            ////front drawing
            //Rectangle frontFrame = new Rectangle(0, 0, 6, 6);
            //spriteBatch.Draw(tex, position - Main.screenPosition, frontFrame, color * ((255 - alpha) / 255), rotation + PiOver2, frontFrame.Size() / 2, scale, 0, 0);

            ////middle drawing
            //Rectangle middleFrame = new Rectangle(0, 4, 6, 2);
            //spriteBatch.Draw(tex, position - (Vector2.UnitX * frontFrame.Width / 2).RotatedBy(rotation) * scale - Main.screenPosition, middleFrame, color * ((255 - alpha) / 255), rotation + PiOver2, Vector2.UnitX * middleFrame.Width / 2 * scale, new Vector2(1, trailLength), 0, 0);

            ////middle drawing
            //Rectangle endFrame = new Rectangle(0, 6, 6, 4);
            //spriteBatch.Draw(tex, position - (Vector2.UnitX * (frontFrame.Width / 2 + trailLength * 2)).RotatedBy(rotation) * scale - Main.screenPosition, endFrame, color * ((255 - alpha) / 255), rotation + PiOver2, Vector2.UnitX * endFrame.Width / 2 * scale, scale, 0, 0);

        }
    }
}