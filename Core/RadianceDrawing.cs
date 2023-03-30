using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Radiance.Utilities;

namespace Radiance.Core
{
    public class RadianceDrawing
    {
        public enum DrawingMode
        {
            Default,
            Item,
            Tile,
            NPC,
            Projectile,
            UI,
            Beam,
            MPAoeCircle
        }
        public static void DrawHorizontalRadianceBar(Vector2 position, float maxRadiance, float currentRadiance, RadianceBarUIElement ui = null)
        {
            Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeter").Value;
            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeterBar").Value;
            Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeterOverlay").Value;
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

            Main.spriteBatch.Draw(
                overlayTexture,
                new Vector2(position.X, position.Y) - Vector2.UnitY * 2,
                null,
                Color.White * 0.5f,
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
        public static void DrawHoverableItem(SpriteBatch spriteBatch, int type, Vector2 pos, int stack, Color? color = null)
        {
            color ??= Color.White; //no compile-time-constant colors :(
            Main.instance.LoadItem(type);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            spriteBatch.Draw(TextureAssets.Item[type].Value, pos, new Rectangle?(Item.GetDrawHitbox(type, null)), (Color)color, 0, new Vector2(Item.GetDrawHitbox(type, null).Width, Item.GetDrawHitbox(type, null).Height) / 2, 1, SpriteEffects.None, 0);
            if (stack > 1)
                Utils.DrawBorderStringFourWay(spriteBatch, font, stack.ToString(), pos.X - Item.GetDrawHitbox(type, null).Width / 2, pos.Y + Item.GetDrawHitbox(type, null).Height / 2, (Color)color, Color.Black, font.MeasureString(stack.ToString()) / 2);
            
            Rectangle itemFrame = new Rectangle((int)(pos.X - Item.GetDrawHitbox(type, null).Width / 2), (int)pos.Y - Item.GetDrawHitbox(type, null).Height / 2, Item.GetDrawHitbox(type , null).Width, Item.GetDrawHitbox(type, null).Height);
            if (itemFrame.Contains(Main.MouseScreen.ToPoint()))
            {
                Item item = new();
                item.SetDefaults(type, false);
                item.stack = stack;
                Main.hoverItemName = item.Name;
                Main.HoverItem = item;
            }
        }
        public static void DrawBeam(Vector2 worldCoordsStart, Vector2 worldCoordsEnd, Vector4 color, float threshold, int thickness, DrawingMode mode, bool spike = false) 
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

            Effect rayEffect = Terraria.Graphics.Effects.Filters.Scene["Beam"].GetShader().Shader;
            if(spike) rayEffect = Terraria.Graphics.Effects.Filters.Scene["Spike"].GetShader().Shader;
            rayEffect.Parameters["startPos"].SetValue(pos);
            rayEffect.Parameters["endPos"].SetValue((worldCoordsEnd - Main.screenPosition));
            rayEffect.Parameters["threshold"].SetValue(threshold);
            rayEffect.Parameters["color"].SetValue(color);
            rayEffect.Parameters["thickness"].SetValue(height);
            rayEffect.Parameters["scale"].SetValue(1);

            SamplerState samplerState = mode == DrawingMode.UI ? SamplerState.LinearClamp : Main.DefaultSamplerState;
            DepthStencilState depthStencilState = DepthStencilState.None;
            RasterizerState rasterizerState = mode == DrawingMode.Tile ? RasterizerState.CullNone : mode == DrawingMode.Item || mode == DrawingMode.Projectile ? RasterizerState.CullCounterClockwise : Main.Rasterizer;
            Matrix matrix = mode == DrawingMode.Item || mode == DrawingMode.Projectile ? Main.GameViewMatrix.ZoomMatrix : mode == DrawingMode.UI ? Main.UIScaleMatrix : mode == DrawingMode.Beam ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, samplerState, depthStencilState, rasterizerState, rayEffect, matrix);

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
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.Rasterizer, null, matrix);
        }
        public static void DrawSoftGlow(Vector2 worldCoords, Color color, float scale, DrawingMode mode)
        {
            SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
            BlendState blendState = BlendState.AlphaBlend;
            SamplerState samplerState = Main.DefaultSamplerState;
            DepthStencilState depthStencilState = DepthStencilState.None;
            RasterizerState rasterizerState = mode == DrawingMode.Tile ? RasterizerState.CullCounterClockwise : mode == DrawingMode.Item || mode == DrawingMode.Projectile ? RasterizerState.CullCounterClockwise : Main.Rasterizer;
            Matrix matrix = mode == DrawingMode.MPAoeCircle ? Main.GameViewMatrix.ZoomMatrix : mode == DrawingMode.NPC || mode == DrawingMode.Item || mode == DrawingMode.Projectile ? Main.GameViewMatrix.ZoomMatrix : mode == DrawingMode.UI ? Main.UIScaleMatrix : mode == DrawingMode.Beam ? Main.GameViewMatrix.TransformationMatrix :  Matrix.Identity;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, samplerState, depthStencilState, rasterizerState, null, matrix);

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
            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, null, matrix);
        }
        public static void DrawCircle(Vector2 worldCoords, Color color, float radius, DrawingMode mode)
        {
            Main.spriteBatch.End();

            Texture2D circleTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            Vector2 pos = worldCoords - Main.screenPosition;

            Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["Circle"].GetShader().Shader;
            circleEffect.Parameters["color"].SetValue(color.ToVector4());
            circleEffect.Parameters["radius"].SetValue(radius);

            SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
            BlendState blendState = BlendState.AlphaBlend;
            SamplerState samplerState = Main.DefaultSamplerState;
            DepthStencilState depthStencilState = DepthStencilState.None;
            RasterizerState rasterizerState = mode == DrawingMode.Tile ? RasterizerState.CullNone : mode == DrawingMode.Item || mode == DrawingMode.Projectile ? RasterizerState.CullCounterClockwise : Main.Rasterizer;
            Matrix matrix = mode == DrawingMode.MPAoeCircle || mode == DrawingMode.Item || mode == DrawingMode.Projectile ? Main.GameViewMatrix.TransformationMatrix : mode == DrawingMode.UI ? Main.UIScaleMatrix : mode == DrawingMode.Beam ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, samplerState, depthStencilState, rasterizerState, circleEffect, matrix);

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
            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, null, matrix);
        }
        public static void DrawSquare(Vector2 worldCoords, Color color, float halfWidth, DrawingMode mode)
        {
            Main.spriteBatch.End();

            Texture2D circleTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            Vector2 pos = worldCoords - Main.screenPosition;

            Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["Square"].GetShader().Shader;
            circleEffect.Parameters["color"].SetValue(color.ToVector4());
            circleEffect.Parameters["halfWidth"].SetValue(halfWidth);

            SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
            BlendState blendState = BlendState.AlphaBlend;
            SamplerState samplerState = Main.DefaultSamplerState;
            DepthStencilState depthStencilState = DepthStencilState.None;
            RasterizerState rasterizerState = mode == DrawingMode.Tile ? RasterizerState.CullNone : mode == DrawingMode.Item || mode == DrawingMode.Projectile ? RasterizerState.CullCounterClockwise : Main.Rasterizer;
            Matrix matrix = mode == DrawingMode.MPAoeCircle || mode == DrawingMode.Item || mode == DrawingMode.Projectile ? Main.GameViewMatrix.TransformationMatrix : mode == DrawingMode.UI ? Main.UIScaleMatrix : mode == DrawingMode.Beam ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, samplerState, depthStencilState, rasterizerState, circleEffect, matrix);

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
            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, null, matrix);
        }
    }
}