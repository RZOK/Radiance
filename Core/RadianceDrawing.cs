using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles;
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
        public static void DrawHorizontalRadianceBar(Vector2 position, string mode, RadianceUtilizingTileEntity tileEntity = null)
        {
            float maxRadiance = 1;
            float currentRadiance = 0;

            Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeter").Value;
            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeterBar").Value;
            Texture2D overlayTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeterOverlay").Value;

            int meterWidth = meterTexture.Width;
            int meterHeight = meterTexture.Height;
            Vector2 padding = (meterTexture.Size() - barTexture.Size()) / 2;
            int barWidth = (int)(meterWidth - 2 * padding.X);
            int barHeight = barTexture.Height;

            Player player = Main.player[Main.myPlayer];
            RadiancePlayer mp = player.GetModPlayer<RadiancePlayer>();
            RadianceInterfacePlayer imp = player.GetModPlayer<RadianceInterfacePlayer>();

            switch (mode)
            {
                case "Item":
                    Item item = Main.HoverItem;
                    BaseContainer container = item.ModItem as BaseContainer;
                    player.GetModPlayer<RadianceInterfacePlayer>().radianceBarAlphaTimer = 20;
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
                    position -= new Vector2(2 * RadianceUtils.SineTiming(33), -2 * RadianceUtils.SineTiming(55)) - new Vector2(tileEntity.Width * 8 / Main.UIScale, (48 / (Main.UIScale * 0.8f) * RadianceUtils.EaseOutCirc(Math.Clamp(player.GetModPlayer<RadianceInterfacePlayer>().radianceBarAlphaTimer / 20 + 0.5f, 0.5f, 1))));
                    break;
            }
            float radianceCharge = Math.Min(currentRadiance, maxRadiance);
            float fill = radianceCharge / maxRadiance;

            Main.spriteBatch.Draw(
                meterTexture,
                position - Vector2.UnitY * 2,
                null,
                Color.White * ((imp.radianceBarAlphaTimer + 1) / 21),
                0,
                new Vector2(meterWidth / 2, meterHeight / 2),
                Math.Clamp((imp.radianceBarAlphaTimer + 1) / 21 + 0.7f, 0.7f, 1),
                SpriteEffects.None,
                0);

            Main.spriteBatch.Draw(
                barTexture,
                new Vector2(position.X + padding.X, position.Y + padding.Y) - Vector2.UnitY * 4,
                new Rectangle(0, 0, (int)(fill * barWidth), barHeight),
                Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, fill * RadianceUtils.SineTiming(5)) * ((imp.radianceBarAlphaTimer + 1) / 21),
                0,
                new Vector2(meterWidth / 2, meterHeight / 2),
                Math.Clamp((imp.radianceBarAlphaTimer + 1) / 21 + 0.7f, 0.7f, 1),
                SpriteEffects.None,
                0);

            Main.spriteBatch.Draw(
                overlayTexture,
                new Vector2(position.X + padding.X, position.Y + padding.Y) - Vector2.UnitY * 4,
                null,
                Color.White * 0.5f,
                0,
                new Vector2(meterWidth / 2, meterHeight / 2),
                Math.Clamp((imp.radianceBarAlphaTimer + 1) / 21 + 0.7f, 0.7f, 1),
                SpriteEffects.None,
                0);

            if (mp.debugMode)
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

        public static void DrawRayBetweenTwoPoints(RadianceRay ray)
        {
            Color color = CommonColors.RadianceColor1;
            if (ray.pickedUp)
                color = Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, RadianceUtils.SineTiming(5));
            else if (ray.interferred)
                color = Color.Red;
            for (int i = 0; i < 2; i++)
                DrawBeam(ray.startPos, ray.endPos, i == 1 ? new Color(255, 255, 255, 150).ToVector4() * (1 - ray.disappearTimer / 60) : color.ToVector4() * (1 - ray.disappearTimer / 30), 0.2f, i == 1 ? 4 : 8, DrawingMode.Beam);
            //Texture2D starTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Star").Value;
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
                DrawSoftGlow(i == 0 ? ray.endPos : ray.startPos, color * (1 - ray.disappearTimer / 30), 0.2f, RadianceDrawing.DrawingMode.Beam);
                DrawSoftGlow(i == 0 ? ray.endPos : ray.startPos, Color.White* (1 - ray.disappearTimer / 30), 0.16f, RadianceDrawing.DrawingMode.Beam);
            }
        }
        public static void DrawHoverableItem(SpriteBatch spriteBatch, int type, Vector2 pos, int stack)
        {
            Main.instance.LoadItem(type);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            spriteBatch.Draw(TextureAssets.Item[type].Value, pos, new Rectangle?(Item.GetDrawHitbox(type, null)), Color.White, 0, new Vector2(Item.GetDrawHitbox(type, null).Width, Item.GetDrawHitbox(type, null).Height) / 2, 1, SpriteEffects.None, 0);
            if (stack > 1)
                Utils.DrawBorderStringFourWay(spriteBatch, font, stack.ToString(), pos.X - Item.GetDrawHitbox(type, null).Width / 2, pos.Y + Item.GetDrawHitbox(type, null).Height / 2, Color.White, Color.Black, font.MeasureString(stack.ToString()) / 2);
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
        //todo: move all the ui stuff to a uistate
        public static void DrawSoftGlow(Vector2 worldCoords, Color color, float scale, DrawingMode mode)
        {
            SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
            BlendState blendState = BlendState.AlphaBlend;
            SamplerState samplerState = Main.DefaultSamplerState;
            DepthStencilState depthStencilState = DepthStencilState.None;
            RasterizerState rasterizerState = mode == DrawingMode.Tile ? RasterizerState.CullCounterClockwise : mode == DrawingMode.Item || mode == DrawingMode.Projectile ? RasterizerState.CullCounterClockwise : Main.Rasterizer;
            Matrix matrix = mode == DrawingMode.MPAoeCircle ? Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().aoeCircleMatrix : mode == DrawingMode.NPC || mode == DrawingMode.Item || mode == DrawingMode.Projectile ? Main.GameViewMatrix.ZoomMatrix : mode == DrawingMode.UI ? Main.UIScaleMatrix : mode == DrawingMode.Beam ? Main.GameViewMatrix.TransformationMatrix :  Matrix.Identity;

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
        public static void DrawCircle(Vector2 worldCoords, Vector4 color, float radius, DrawingMode mode)
        {
            Main.spriteBatch.End();

            Texture2D circleTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            Vector2 pos = worldCoords - Main.screenPosition;

            Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["Circle"].GetShader().Shader;
            circleEffect.Parameters["color"].SetValue(color);
            circleEffect.Parameters["distance"].SetValue(0.9f);

            SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
            BlendState blendState = BlendState.AlphaBlend;
            SamplerState samplerState = Main.DefaultSamplerState;
            DepthStencilState depthStencilState = DepthStencilState.None;
            RasterizerState rasterizerState = mode == DrawingMode.Tile ? RasterizerState.CullNone : mode == DrawingMode.Item || mode == DrawingMode.Projectile ? RasterizerState.CullCounterClockwise : Main.Rasterizer;
            Matrix matrix = mode == DrawingMode.Item || mode == DrawingMode.Projectile ? Main.GameViewMatrix.ZoomMatrix : mode == DrawingMode.UI ? Main.UIScaleMatrix : mode == DrawingMode.Beam ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, samplerState, depthStencilState, rasterizerState, circleEffect, matrix);

            Main.spriteBatch.Draw(
            circleTexture,
            pos,
            null,
            Color.White,
            0,
            new Vector2(0.5f, 0.5f),
            radius * 2.22f,
            SpriteEffects.None,
            0
            );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, null, matrix);
        }
    }
}