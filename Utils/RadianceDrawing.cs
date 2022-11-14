﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
using Radiance.Core;
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
                        return;
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
            Vector2 pos = position + new Vector2(6, 6);
            DrawSoftGlow(pos, type == "Input" ? Color.Blue : Color.Red, Math.Max(0.2f * (float)Math.Abs(MathUtils.sineTiming(60)), 0.15f));
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            DrawSoftGlow(pos, Color.White, Math.Max(0.15f * (float)Math.Abs(MathUtils.sineTiming(60)), 0.10f));
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void DrawRayBetweenTwoPoints(RadianceRay ray)
        {
            Color color = Radiance.RadianceColor1;
            if (ray.pickedUp)
                color = Color.Lerp(Radiance.RadianceColor1, Radiance.RadianceColor2, (float)MathUtils.sineTiming(5));
            else if (ray.interferred)
                color = Color.Red;

            for (int i = 0; i < 2; i++)
            {
                DrawBeam(ray.startPos, ray.endPos, i == 1 ? new Color(255, 255, 255, 150).ToVector4() : color.ToVector4(), 0.2f, i == 1 ? 4 : 8);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D starTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Star").Value;
            //for (int i = 0; i < 2; i++)
            //    Main.spriteBatch.Draw(
            //    starTexture,
            //    ((ray.startPos - Main.screenPosition) + (ray.endPos - Main.screenPosition)) / 2,
            //    null,
            //    i == 0 ? new Color(255, 255, 255) : color,
            //    0,
            //    starTexture.Size() / 2,
            //    i == 0 ? 0.25f : 0.2f,
            //    SpriteEffects.None,
            //    0);
            for (int i = 0; i < 2; i++)
            {
                DrawSoftGlow(i == 0 ? ray.endPos : ray.startPos, color, 0.2f);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                DrawSoftGlow(i == 0 ? ray.endPos : ray.startPos, Color.White, 0.15f);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        public static void DrawBeam(Vector2 worldCoordsStart, Vector2 worldCoordsEnd, Vector4 color, float threshold, int thickness) //ALWAYS BEGIN SPRITEBATCH AGAIN AFTER CALLING THIS ‼‼
        {
            Main.spriteBatch.End();
            float num = Math.Clamp(Vector2.Distance(worldCoordsStart, worldCoordsEnd), 1, float.MaxValue);
            Vector2 vector = (worldCoordsEnd - worldCoordsStart) / num;
            float rotation = vector.ToRotation();

            Texture2D rayTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            Vector2 pos = worldCoordsStart - Main.screenPosition;
            int width = (int)Vector2.Distance(worldCoordsStart, worldCoordsEnd);
            int height = thickness;
            Vector2 adjustedPos = pos - new Vector2(0, height / 2).RotatedBy(rotation);
            var drawRect = new Rectangle((int)adjustedPos.X, (int)adjustedPos.Y, width, height);

            Effect rayEffect = Terraria.Graphics.Effects.Filters.Scene["CoolBeam"].GetShader().Shader;
            rayEffect.Parameters["startPos"].SetValue(pos);
            rayEffect.Parameters["endPos"].SetValue((worldCoordsEnd - Main.screenPosition));
            rayEffect.Parameters["threshold"].SetValue(threshold);
            rayEffect.Parameters["color"].SetValue(color);
            rayEffect.Parameters["thickness"].SetValue(height);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, rayEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(
            rayTexture,
            drawRect,
            null,
            Color.White,
            rotation,
            Vector2.Zero,
            SpriteEffects.None,
            0
            );

            Main.spriteBatch.End();
        }
        public static void DrawSoftGlow(Vector2 worldCoords, Color color, float scale) //ALWAYS BEGIN SPRITEBATCH AGAIN AFTER CALLING THIS ‼‼
        {
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            Main.spriteBatch.Draw(
                softGlow, 
                worldCoords - Main.screenPosition, 
                null, 
                color, 
                0, 
                softGlow.Size() / 2, 
                scale, 
                0, 
                0
                );

            Main.spriteBatch.End();
        }
    }
}