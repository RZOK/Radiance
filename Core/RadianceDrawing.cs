using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Radiance.Utilities;
using Terraria.UI;
using System.Reflection;
using static Radiance.Core.RadianceDrawing;

namespace Radiance.Core
{
    public static class RadianceDrawingExtensions
    {
        public static void GetSpritebatchDetails(this SpriteBatch spriteBatch, out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Matrix matrix)
        {
            spriteSortMode = (SpriteSortMode)spriteBatch.ReflectionGetValue("sortMode", BindingFlags.NonPublic | BindingFlags.Instance);
            blendState = (BlendState)spriteBatch.ReflectionGetValue("blendState", BindingFlags.NonPublic | BindingFlags.Instance);
            samplerState = (SamplerState)spriteBatch.ReflectionGetValue("samplerState", BindingFlags.NonPublic | BindingFlags.Instance);
            depthStencilState = (DepthStencilState)spriteBatch.ReflectionGetValue("depthStencilState", BindingFlags.NonPublic | BindingFlags.Instance);
            rasterizerState = (RasterizerState)spriteBatch.ReflectionGetValue("rasterizerState", BindingFlags.NonPublic | BindingFlags.Instance);
            matrix = (Matrix)spriteBatch.ReflectionGetValue("transformMatrix", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static void BeginSpriteBatchFromTemplate(this SpriteBatchData data, BlendState blendState = null, Effect effect = null)
        {
            switch(data)
            {
                case SpriteBatchData.WorldDrawingData:
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.Transform);
                    break;
                case SpriteBatchData.UIDrawingDataScale:
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState, null, null, null, effect, Main.UIScaleMatrix);
                    break;
                case SpriteBatchData.UIDrawingDataGame:
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState, null, null, null, effect, Main.GameViewMatrix.ZoomMatrix);
                    break;
                case SpriteBatchData.UIDrawingDataNone:
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendState, null, null, null, effect, Matrix.Identity);
                    break;
            }
        }
    }
    public class RadianceDrawing
    {
        public enum SpriteBatchData
        {
            WorldDrawingData,
            UIDrawingDataScale,
            UIDrawingDataGame,
            UIDrawingDataNone
        }
        public static void DrawHorizontalRadianceBar(Vector2 position, float maxRadiance, float currentRadiance, RadianceBarUIElement ui = null)
        {
            Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeter").Value;
            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeterBar").Value;
            float alpha = ui != null ? ui.timerModifier : 1; 

            int meterWidth = meterTexture.Width;
            int meterHeight = meterTexture.Height;
            Vector2 padding = (meterTexture.Size() - barTexture.Size()) / 2;
            int barWidth = (int)(meterWidth - 2 * padding.X);
            int barHeight = barTexture.Height;

            float radianceCharge = Math.Min(currentRadiance, maxRadiance);
            float fill = radianceCharge / maxRadiance;
            float scale = Math.Clamp(alpha + 0.7f, 0.7f, 1);

            Main.spriteBatch.Draw(
                meterTexture,
                position,
                null,
                Color.White * alpha,
                0,
                new Vector2(meterWidth / 2, meterHeight / 2),
                scale,
                SpriteEffects.None,
                0);

            Main.spriteBatch.Draw(
                barTexture,
                new Vector2(position.X, position.Y) - Vector2.UnitY * 2,
                new Rectangle(0, 0, (int)(fill * barWidth), barHeight),
                Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, fill * RadianceUtils.SineTiming(5)) * alpha,
                0,
                new Vector2(meterWidth / 2, meterHeight / 2) - padding * scale,
                scale,
                SpriteEffects.None,
                0);

            if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().debugMode)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    font,
                    currentRadiance + " / " + maxRadiance,
                    position,
                    Color.Lerp(new Color(255, 150, 0), new Color(255, 255, 192), fill),
                    0,
                    font.MeasureString(currentRadiance + " / " + maxRadiance) / 2,
                    Vector2.One
                    );
            }
        }
        public static void DrawHoverableItem(SpriteBatch spriteBatch, int type, Vector2 pos, int stack, Color? color = null, bool hoverable = true)
        {
            color ??= Color.White; //no compile-time-constant colors :(
            Item itemToDraw = RadianceUtils.GetItem(type);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            ItemSlot.DrawItemIcon(itemToDraw, 0, spriteBatch, pos, itemToDraw.scale, 256, color.Value);
            if (stack > 1)
                Utils.DrawBorderStringFourWay(spriteBatch, font, stack.ToString(), pos.X - Item.GetDrawHitbox(type, null).Width / 2, pos.Y + Item.GetDrawHitbox(type, null).Height / 2 + Math.Max(0, 20 - Item.GetDrawHitbox(type, null).Height), (Color)color, Color.Black, font.MeasureString(stack.ToString()) / 2);
            
            if (hoverable)
            {
                Rectangle itemFrame = new Rectangle((int)(pos.X - Item.GetDrawHitbox(type, null).Width / 2), (int)pos.Y - Item.GetDrawHitbox(type, null).Height / 2, Item.GetDrawHitbox(type, null).Width, Item.GetDrawHitbox(type, null).Height);
                if (itemFrame.Contains(Main.MouseScreen.ToPoint()))
                {
                    Item item = new();
                    item.SetDefaults(type, false);
                    item.stack = stack;
                    Main.hoverItemName = item.Name;
                    Main.HoverItem = item;
                }
            }
        }
        public static void DrawBeam(Vector2 worldCoordsStart, Vector2 worldCoordsEnd, Vector4 color, float threshold, int thickness, SpriteBatchData data, bool spike = false) 
        {
            float num = Math.Clamp(Vector2.Distance(worldCoordsStart, worldCoordsEnd), 1, float.MaxValue);
            Vector2 vector = (worldCoordsEnd - worldCoordsStart) / num;
            float rotation = vector.ToRotation();

            Texture2D rayTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            Vector2 pos = worldCoordsStart - Main.screenPosition;
            int width = (int)Vector2.Distance(worldCoordsStart, worldCoordsEnd);
            int height = thickness;
            Vector2 adjustedPos = pos - new Vector2(0, height / 2).RotatedBy(rotation);
            var drawRect = new Rectangle((int)adjustedPos.X, (int)adjustedPos.Y, width, height);

            Effect rayEffect = Terraria.Graphics.Effects.Filters.Scene["Beam"].GetShader().Shader;
            if(spike) rayEffect = Terraria.Graphics.Effects.Filters.Scene["Spike"].GetShader().Shader;
            rayEffect.Parameters["startPos"].SetValue(pos);
            rayEffect.Parameters["endPos"].SetValue((worldCoordsEnd - Main.screenPosition));
            rayEffect.Parameters["threshold"].SetValue(threshold);
            rayEffect.Parameters["color"].SetValue(color);
            rayEffect.Parameters["thickness"].SetValue(height);
            rayEffect.Parameters["scale"].SetValue(1);

            Main.spriteBatch.End();
            data.BeginSpriteBatchFromTemplate(BlendState.Additive, effect: rayEffect);

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
            data.BeginSpriteBatchFromTemplate();
        }
        public static void DrawSoftGlow(Vector2 worldCoords, Color color, float scale, SpriteBatchData? data = null)
        {
            bool hasData = data != null;
            if (hasData)
            {
                Main.spriteBatch.End();
                data.Value.BeginSpriteBatchFromTemplate(BlendState.Additive);
            }

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

            if (hasData)
            {
                Main.spriteBatch.End();
                data.Value.BeginSpriteBatchFromTemplate();
            }
        }
        public static void DrawCircle(Vector2 worldCoords, Color color, float radius, SpriteBatchData data)
        {
            Texture2D circleTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            Vector2 pos = worldCoords - Main.screenPosition;

            Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["Circle"].GetShader().Shader;
            circleEffect.Parameters["color"].SetValue(color.ToVector4());
            circleEffect.Parameters["radius"].SetValue(radius);

            Main.spriteBatch.End();
            data.BeginSpriteBatchFromTemplate(BlendState.Additive, circleEffect);

            Main.spriteBatch.Draw(
            circleTexture,
            pos,
            null,
            color,
            0,
            new Vector2(0.5f, 0.5f),
            radius * 2.22f,
            SpriteEffects.None,
            0
            );

            Main.spriteBatch.End();
            data.BeginSpriteBatchFromTemplate();
        }
        //public static void DrawStabilityCircle(Vector2 worldCoords, float radius, float progress, Vector4 color, float rotation)
        //{
        //    Main.spriteBatch.End();

        //    Texture2D circleTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
        //    Vector2 pos = worldCoords - Main.screenPosition;

        //    Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["StabilityCircle"].GetShader().Shader;
        //    circleEffect.Parameters["color"].SetValue(color);
        //    circleEffect.Parameters["maxProgress"].SetValue(progress);

        //    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, circleEffect, Main.GameViewMatrix.TransformationMatrix);

        //    Main.spriteBatch.Draw(
        //    circleTexture,
        //    pos,
        //    null,
        //    Color.White,
        //    rotation,
        //    new Vector2(0.5f, 0.5f),
        //    radius * 2.22f,
        //    SpriteEffects.FlipVertically,
        //    0
        //    );

        //    Main.spriteBatch.End();
        //    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        //}
        public static void DrawSquare(Vector2 worldCoords, Color color, float halfWidth, SpriteBatchData data)
        {
            Texture2D circleTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            Vector2 pos = worldCoords - Main.screenPosition;

            Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["Square"].GetShader().Shader;
            circleEffect.Parameters["color"].SetValue(color.ToVector4());
            circleEffect.Parameters["halfWidth"].SetValue(halfWidth);

            Main.spriteBatch.End();
            data.BeginSpriteBatchFromTemplate(BlendState.Additive, circleEffect);

            Main.spriteBatch.Draw(
            circleTexture,
            pos,
            null,
            color,
            0,
            new Vector2(0.5f, 0.5f),
            halfWidth * 2.22f,
            SpriteEffects.None,
            0
            );

            Main.spriteBatch.End();
            data.BeginSpriteBatchFromTemplate();
        }
    }
}