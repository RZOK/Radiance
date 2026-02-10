using Radiance.Core.Systems;
using Radiance.Core.Visuals.Primitives;
using Steamworks;
using System.ComponentModel.DataAnnotations;
using Terraria.Graphics.Effects;

namespace Radiance.Content.Particles
{
    public class PearlFlow : Particle
    {
        private const int POINT_COUNT = 30;
        private List<Vector2> cache;
        internal PrimitiveTrail TrailDrawer;
        private int offset;
        public override string Texture => "Radiance/Content/Particles/HiddenTextSparkle";

        public PearlFlow(Vector2 startPosition, int maxTime)
        {
            position = startPosition;
            this.maxTime = timeLeft = maxTime;
            specialDraw = true;
            drawPixelated = true;
            mode = ParticleSystem.DrawingMode.Additive;
            offset = Main.rand.Next(120);
        }

        public override void Update()
        {
            TrailDrawer ??= new PrimitiveTrail(POINT_COUNT, TrailWidth, TrailColor, new NoTip());
            ManageCache();

            position.X += SineTiming(5, offset) * MathF.Pow(Progress, 0.7f) * 4f;
            velocity.Y = -Lerp(1.5f + offset / 90f, 0f, MathF.Pow(Progress, 0.7f));

            TrailDrawer.SetPositionsSmart(cache, position, SmoothBezierPointRetreivalFunction);
        }

        private float TrailWidth(float factorAlongTrail)
        {
            return 10f * factorAlongTrail * (1f - Progress);
        }

        private Color TrailColor(float factorAlongTrail)
        {
            return Color.Lerp(Color.LightPink, Color.LightSteelBlue, factorAlongTrail) * (1f - Progress) * 0.6f;
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
            effect.Parameters["trailTexture"].SetValue(ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value);

            TrailDrawer?.Render(effect, -Main.screenPosition);
        }
    }
}