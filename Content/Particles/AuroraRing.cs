using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core.Systems;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Content.Particles
{
    public class AuroraRing : Particle
    {
        private float radius;
        private float distance;
        public override string Texture => "Radiance/Content/ExtraTextures/SoftGlow";

        public AuroraRing(Vector2 position, Vector2 velocity, int maxTime, float radius, float distance, float alpha, Color outerColor, Color innerColor, bool slowDown = true)
        {
            this.position = position;
            this.velocity = velocity;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            this.radius = radius;
            this.distance = distance;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Additive;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Effect auroraEffect = Terraria.Graphics.Effects.Filters.Scene["AuroraRing"].GetShader().Shader;

            auroraEffect.Parameters["color"].SetValue(new Color(77, 255, 139).ToVector4());
            auroraEffect.Parameters["radius"].SetValue(30);
            auroraEffect.Parameters["distance"].SetValue(new Color(77, 255, 139).ToVector4());


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, default, default, default, auroraEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(tex, position - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, radius, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, default, default, default, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}