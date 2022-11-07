using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Common.Globals;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Radiance.Utils
{
    public class RadianceDrawing
    {
#nullable enable
        public static void DrawHorizontalRadianceBar(Vector2 position, string mode, RadianceUtilizingTileEntity? tileEntity = null)
#nullable disable
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
            Player player = Main.player[Main.myPlayer];

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
                    position += (meterTexture.Size() / 2);
                    maxRadiance = container.MaxRadiance;
                    currentRadiance = container.CurrentRadiance;
                    break;
                case "Tile2x2":
                    maxRadiance = tileEntity.MaxRadiance;
                    currentRadiance = tileEntity.CurrentRadiance;
                    position /= Main.UIScale;
                    position -= new Vector2(2 * (float)MathUtils.sineTiming(33), -(float)(2 * MathUtils.sineTiming(55))) - new Vector2(16 / Main.UIScale, 48 / (Main.UIScale * 0.8f));
                    break;
            }
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
                0);
            Main.spriteBatch.Draw(
                barTexture,
                new Vector2(position.X + padding.X, position.Y + padding.Y),
                new Rectangle(0, 0, (int)(fill * barWidth), barHeight),
                Color.Lerp(Radiance.RadianceColor1, Radiance.RadianceColor2, fill * (float)MathUtils.sineTiming(5)),
                0,
                new Vector2(meterWidth / 2, meterHeight / 2),
                1,
                SpriteEffects.None,
                0);

            if (player.GetModPlayer<RadiancePlayer>().debugMode)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    font,
                    Math.Round((double)currentRadiance) + " / " + maxRadiance,
                    position + new Vector2(0, 3),
                    Color.Lerp(new Color(255, 150, 0), new Color(255, 255, 192), fill), 
                    0,
                    font.MeasureString(Math.Round((double)currentRadiance) + " / " + maxRadiance) / 2,
                    Vector2.One
                    );
            }
        }
        public static void DrawIOOnTile(Vector2 position, string type)
        {
            Texture2D indicatorTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/" + type + "Indicator").Value;
            Main.spriteBatch.Draw(
            indicatorTexture,
            position,
            null,
            Color.White,
            0,
            Vector2.Zero,
            1,
            SpriteEffects.None,
            0);
        }
        public static void DrawRayBetweenTwoPoints(Vector2 startPos, Vector2 endPos, Vector2? controlPoint = null)
        {
            float num = Vector2.Distance(startPos, endPos);
            if(num == 0)
            {
                num = 1;
            }
            Vector2 vector = (endPos - startPos) / num;
            Vector2 value = startPos;
            Vector2 screenPosition = Main.screenPosition;
            float rotation = vector.ToRotation();
            for (float num2 = 0f; num2 <= num; num2 += 4f)
            {
                Main.spriteBatch.Draw(
                    TextureAssets.BlackTile.Value, 
                    value - screenPosition, 
                    null, 
                    Radiance.RadianceColor1, 
                    rotation,
                    TextureAssets.BlackTile.Value.Size() / 2,
                    0.25f, 
                    SpriteEffects.None, 
                    0f);
                value = startPos + num2 * vector;
            }
            for (int i = 0; i < 2; i++)
            {
                Texture2D indicatorTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/RayIndicator").Value;
                Main.spriteBatch.Draw(
                indicatorTexture,
                i == 1 ? endPos - screenPosition : startPos - screenPosition,
                null,
                new Color(100, 100, 100, 50),
                0,
                indicatorTexture.Size() / 2,
                1,
                SpriteEffects.None,
                0);
            }
        }
    }
}
