using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Common.Globals;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Radiance.Utils
{
    public class RadianceBarDrawer
    {
        public static void DrawHorizontalRadianceBar(Vector2 position, string mode, RadianceUtilizingTileEntity? tileEntity = null)
        {
            float maxRadiance = 1;
            float currentRadiance = 0;

            Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeter").Value;
            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeterBar").Value;

            int meterWidth = meterTexture.Width;
            int meterHeight = meterTexture.Height;
            Vector2 padding = (meterTexture.Size() - barTexture.Size()) / 2;
            int barWidth = (int)(meterWidth - 2 * padding.X);
            int barHeight = barTexture.Height;

            switch (mode)
            {
                case "Item":
                    Item item = Main.HoverItem;
#nullable enable
                    BaseContainer? container = item.ModItem as BaseContainer;
#nullable disable
                    if (container == null || item.IsAir)
                    {
                        return;
                    }
                    RadianceGlobalItem GItem = item.GetGlobalItem<RadianceGlobalItem>();
                    position += (meterTexture.Size() / 2);
                    maxRadiance = container.MaxRadiance;
                    currentRadiance = container.CurrentRadiance;
                    break;
                case "Tile2x2":
                    maxRadiance = tileEntity.MaxRadiance;
                    currentRadiance = tileEntity.CurrentRadiance;
                    position -= new Vector2(2 * (float)MathUtils.sineTiming(33), -(float)(2 * MathUtils.sineTiming(55))) - new Vector2(16, 48);
                    break;
            }

            Player player = Main.player[Main.myPlayer];

            float radianceCharge = Math.Min(currentRadiance, maxRadiance);
            float fill = radianceCharge / maxRadiance;
            Main.spriteBatch.Draw(
                meterTexture,
                position,
                null,
                Color.White,
                0,
                new Vector2(meterWidth / 2, meterHeight / 2),
                1,
                SpriteEffects.None,
                0f);
            Main.spriteBatch.Draw(
                barTexture,
                new Vector2(position.X + padding.X, position.Y + padding.Y),
                new Rectangle(0, 0, (int)(fill * barWidth), barHeight),
                Color.Lerp(Color.Lerp(Color.Orange, Color.Yellow, fill), Color.White, fill * (float)MathUtils.sineTiming(10)),
                0,
                new Vector2(meterWidth / 2, meterHeight / 2),
                1,
                SpriteEffects.None,
                0);

            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    font,
                    currentRadiance + " / " + maxRadiance,
                    position + new Vector2(0, 3),
                    Color.DarkOrange, 
                    0,
                    font.MeasureString(currentRadiance + " / " + maxRadiance) / 2,
                    Vector2.One
                    );
            }
        }
    }
}
