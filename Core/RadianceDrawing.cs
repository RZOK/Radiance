using ReLogic.Graphics;
using Terraria.UI.Chat;
using Terraria.UI;
using static Radiance.Core.RadianceDrawing;
using System.Reflection;

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
        public static void BeginSpriteBatchFromTemplate(this SpriteBatchData data, BlendState blendState = null, Effect effect = null, SpriteSortMode spriteSortMode = SpriteSortMode.Deferred, SpriteBatch spriteBatch = null)
        {
            spriteBatch ??= Main.spriteBatch;
            switch(data)
            {
                case SpriteBatchData.WorldDrawingData:
                    spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.Transform);
                    break;
                case SpriteBatchData.TileDrawingData:
                    spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Matrix.Identity);
                    break;
                case SpriteBatchData.UIDrawingDataScale:
                    spriteBatch.Begin(spriteSortMode, blendState, null, null, null, effect, Main.UIScaleMatrix);
                    break;
                case SpriteBatchData.UIDrawingDataGame:
                    spriteBatch.Begin(spriteSortMode, blendState, null, null, null, effect, Main.GameViewMatrix.ZoomMatrix);
                    break;
                case SpriteBatchData.UIDrawingDataNone:
                    spriteBatch.Begin(spriteSortMode, blendState, null, null, null, effect, Matrix.Identity);
                    break;
            }
        }
    }
    public class RadianceDrawing
    {
        public enum SpriteBatchData
        {
            WorldDrawingData,
            TileDrawingData,
            UIDrawingDataScale,
            UIDrawingDataGame,
            UIDrawingDataNone
        }
        //public static List<(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects)> AdditiveTileFeatures = 
        //    new List<(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects)>();

        //public void Load(Mod mod)
        //{
        //    On_Main.DrawDust += DrawAdditiveTileFeatures;
        //}

        //public void Unload()
        //{
        //    On_Main.DrawDust -= DrawAdditiveTileFeatures;
        //}
        //private void DrawAdditiveTileFeatures(On_Main.orig_DrawDust orig, Main self)
        //{
        //    orig(self);
        //    Main.spriteBatch.End();
        //    SpriteBatchData.TileDrawingData.BeginSpriteBatchFromTemplate(BlendState.Additive)

        //    foreach (var data in AdditiveTileFeatures)
        //    {
                
        //    }
        //    AdditiveTileFeatures.Clear();
        //}

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
                Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, fill * SineTiming(5)) * alpha,
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
        public static void DrawHoverableItem(SpriteBatch spriteBatch, int type, Vector2 pos, int stack, Color? color = null, float scale = 1f, bool hoverable = true)
        {
            color ??= Color.White; //no compile-time-constant colors :(
            Item itemToDraw = GetItem(type);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            ItemSlot.DrawItemIcon(itemToDraw, 0, spriteBatch, pos, scale, 256, color.Value);
            if (stack > 1)
                Utils.DrawBorderStringFourWay(spriteBatch, font, stack.ToString(), pos.X - Item.GetDrawHitbox(type, null).Width / 2, pos.Y + Item.GetDrawHitbox(type, null).Height / 2 + Math.Max(0, 20 - Item.GetDrawHitbox(type, null).Height), (Color)color, Color.Black, font.MeasureString(stack.ToString()) / 2, scale);
            
            if (hoverable)
            {
                Rectangle itemFrame = new Rectangle((int)((pos.X - Item.GetDrawHitbox(type, null).Width / 2) * scale), (int)((pos.Y - Item.GetDrawHitbox(type, null).Height / 2) * scale), (int)(Item.GetDrawHitbox(type, null).Width * scale), (int)(Item.GetDrawHitbox(type, null).Height * scale));
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
        public static void DrawBeam(Vector2 worldCoordsStart, Vector2 worldCoordsEnd, Color color, float thickness) 
        {
            Texture2D rayTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value;
            float rotation = worldCoordsStart.AngleTo(worldCoordsEnd);
            Vector2 origin = new Vector2(0, rayTexture.Height) / 2;
            Color realColor = color * (color.A / 255f);
            realColor.A = 0;

            Main.spriteBatch.Draw(rayTexture, worldCoordsStart - Main.screenPosition, null, realColor, rotation, origin, new Vector2(Vector2.Distance(worldCoordsStart, worldCoordsEnd), thickness * 2f) / 200f, SpriteEffects.None, 0);
        }
        public static void DrawSoftGlow(Vector2 worldCoords, Color color, float scale)
        { 
            Texture2D softGlow = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SoftGlow").Value;
            Color realColor = color * (color.A / 255f);
            realColor.A = 0;

            Main.spriteBatch.Draw(
                softGlow, 
                worldCoords - Main.screenPosition, 
                null,
                realColor, 
                0, 
                softGlow.Size() / 2, 
                scale, 
                0, 
                0
                );
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