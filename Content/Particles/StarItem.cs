using Radiance.Core.Config;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;

namespace Radiance.Content.Particles
{
    public class StarItem : Particle
    {
        public override string Texture => "Radiance/Content/ExtraTextures/ThinStarFlare";
        private readonly Item item;
        private readonly Vector2 idealPosition;
        private readonly Vector2 initialPosition;
        private readonly float initialScale;
        private readonly Vector2 curveOffset;
        private float Distance => Math.Max(64, initialPosition.Distance(idealPosition));
        private Vector2 CurvePoint => ((idealPosition - initialPosition) / 2f) + curveOffset;
        private float ScaleMod => Math.Min(2f, Item.GetDrawHitbox(item.type, null).Width / 32f);
        public StarItem(Vector2 position, Vector2 idealPosition, int maxTime, Color color, Item item, float scale)
        {
            this.position = initialPosition = position;
            this.idealPosition = idealPosition;
            this.maxTime = timeLeft = maxTime;
            this.color = color;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
            this.item = item;
            curveOffset = Main.rand.NextVector2CircularEdge(Distance, Distance);
            initialScale = scale * ScaleMod;
            this.scale = initialScale;
        }

        public override void Update()
        {
            float scaleStart = 0.75f;
            if (Progress >= scaleStart)
                scale = Lerp(initialScale, 0f, EaseOutExponent((Progress - scaleStart) / (1f - scaleStart), 2f));

            if (timeLeft % 10 == 0)
                WorldParticleSystem.system.DelayedAddParticle(new SmallStar(position, Main.rand.NextVector2CircularEdge(1f, 1f), 30, CommonColors.RadianceColor1, scale * 0.4f));

            position = Vector2.Hermite(initialPosition, CurvePoint, idealPosition, -CurvePoint, EaseInOutExponent(Progress, 4f));
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        { 
            Texture2D fullFlareTex = ModContent.Request<Texture2D>("Radiance/Content/Particles/StarFlare").Value;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D itemTex = GetItemTexture(item.type);
            float colorMod = Min(1f, Progress * 10f);

            if (ModContent.GetInstance<RadianceConfig>().FormationCoreAdaptiveColoring)
            {
                Main.instance.GraphicsDevice.Textures[1] = itemTex;
                Effect getColorEffect = Terraria.Graphics.Effects.Filters.Scene["StarColoring"].GetShader().Shader;
                getColorEffect.Parameters["alpha"].SetValue((Color.White * colorMod).ToVector4());

                spriteBatch.End();
                RadianceDrawing.SpriteBatchData.AdditiveParticleDrawing.BeginSpriteBatchFromTemplate(effect: getColorEffect);
            }

            Main.spriteBatch.Draw(fullFlareTex, drawPos, null, color * colorMod, rotation, texture.Size() / 2, scale, SpriteEffects.None, 0);
            
            if (ModContent.GetInstance<RadianceConfig>().FormationCoreAdaptiveColoring)
            {
                spriteBatch.End();
                RadianceDrawing.SpriteBatchData.AdditiveParticleDrawing.BeginSpriteBatchFromTemplate();
            }

            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * colorMod, rotation, texture.Size() / 2, scale * 0.8f, SpriteEffects.None, 0);
        }
    }
}