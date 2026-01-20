using Radiance.Core.Systems;
using Steamworks;
using System.ComponentModel.DataAnnotations;
using Terraria.Graphics.Effects;

namespace Radiance.Content.Particles
{
    public class PearlFlow : Particle
    {
        private const int POINT_COUNT = 30;

        private Vector2 startPosition;
        private Vector2 endPosition;
        private List<Vector2> curve;
        private List<Vector2> cache;
        internal PrimitiveTrail TrailDrawer;
        public override string Texture => "Radiance/Content/Particles/HiddenTextSparkle";

        public PearlFlow(Vector2 startPosition, Vector2 endPosition, int maxTime)
        {
            this.startPosition = position = startPosition;
            this.endPosition = endPosition;
            this.maxTime = timeLeft = maxTime;
            specialDraw = true;
            drawPixelated = true;
            mode = ParticleSystem.DrawingMode.Additive;

            curve = new BezierCurve(position, new Vector2((position.X + endPosition.X) / 2f, position.Y - 160f), endPosition).GetEvenlySpacedPoints(maxTime);
        }

        public override void Update()
        {
            TrailDrawer ??= new PrimitiveTrail(POINT_COUNT, TrailWidth, TrailColor, new NoTip());
            ManageCache();
            position = curve[maxTime - timeLeft];
            TrailDrawer.SetPositionsSmart(cache, position, SmoothBezierPointRetreivalFunction);
        }

        private float TrailWidth(float factorAlongTrail)
        {
            return 5f * factorAlongTrail;
        }

        private Color TrailColor(float factorAlongTrail)
        {
            return Color.White;
        }
        public void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < POINT_COUNT; i++)
                {
                    cache.Add(position);
                }
            }
            cache.Add(position);
            while (cache.Count > POINT_COUNT)
            {
                cache.RemoveAt(0);
            }
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Effect effect = Filters.Scene["FadedUVMapStreak"].GetShader().Shader;
            effect.Parameters["time"].SetValue(0f);
            effect.Parameters["fadeDistance"].SetValue(0f);
            effect.Parameters["fadePower"].SetValue(0f);
            effect.Parameters["trailTexture"].SetValue(ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value);

            TrailDrawer?.Render(effect, -Main.screenPosition);
        }
    }
}