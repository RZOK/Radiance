using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using Terraria.UI;

namespace Radiance.Content.Particles
{
    public class DisintegratingItem : Particle
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public Texture2D itemTexture;
        public Item item;
        public int direction;

        public DisintegratingItem(Vector2 position, Vector2 velocity, int maxTime, int direction, Item item, Texture2D tex)
        {
            this.direction = direction;
            this.position = position;
            this.velocity = velocity;
            this.velocity.X *= direction;
            this.maxTime = maxTime;
            timeLeft = maxTime;
            specialDraw = true;
            mode = ParticleSystem.DrawingMode.Regular;
            itemTexture = tex;
            this.item = item;
        }

        public override void Update()
        {
            if (timeLeft > maxTime / 1.7f)
            {
                velocity.X += Lerp(0.1f, 0, EaseInOutExponent(Progress * 0.5f + 0.5f, 3f)) * direction;
                rotation += (velocity.X / 50f) * (1f - Progress);
            }
            else
            {
                if (timeLeft < maxTime / 2f)
                    alpha = 255 * (1f - ((timeLeft - 1) * 2f / maxTime));

                Rectangle rect = Item.GetDrawHitbox(item.type, null);
                Rectangle ashRectangle = new Rectangle((int)position.X - rect.Width / 2, (int)position.Y - rect.Height / 2, rect.Width, rect.Height);
                WorldParticleSystem.system.DelayedAddParticle(new DisintegratingItemAsh(Main.rand.NextVector2FromRectangle(ashRectangle), 45, 1.2f));
            }
            velocity *= 0.92f;
        }

        public override void SpecialDraw(SpriteBatch spriteBatch, Vector2 drawPos)
        {
            Point tileCoords = position.ToTileCoordinates();
            Color color = Lighting.GetColor(tileCoords);
            Color alphaColor = item.GetAlpha(color);
            float scale = 1f;
            ItemSlot.GetItemLight(ref alphaColor, ref scale, item.type);

            Effect effect = Terraria.Graphics.Effects.Filters.Scene["Disintegration"].GetShader().Shader;

            effect.Parameters["sampleTexture"].SetValue(itemTexture);
            effect.Parameters["alpha"].SetValue(1f - (alpha / 255f));
            effect.Parameters["color1"].SetValue(alphaColor.ToVector4());
            effect.Parameters["color2"].SetValue(new Color(116, 75, 173).ToVector4());
            effect.Parameters["time"].SetValue(Progress * 0.8f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

            //Main.spriteBatch.Draw(itemTexture, drawPos, null, alphaColor, rotation, itemTexture.Size() / 2, scale, SpriteEffects.None, 0);

            ItemSlot.DrawItemIcon(item, 0, spriteBatch, drawPos, scale, 256, alphaColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}