using ReLogic.Graphics;
using Terraria.UI.Chat;
using Terraria.UI;
using static Radiance.Core.Visuals.RadianceDrawing;
using System.Reflection;
using Radiance.Core.Config;
using static Radiance.Core.Config.RadianceConfig;
using Radiance.Core.Encycloradia;
using static Radiance.Core.Visuals.InterfaceDrawer;

namespace Radiance.Core.Visuals
{
    public static class RadianceDrawingExtensions
    {
        private static readonly FieldInfo SpriteSortMode_FieldInfo = typeof(SpriteBatch).GetField("sortMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo BlendState_FieldInfo = typeof(SpriteBatch).GetField("blendState", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo SamplerState_FieldInfo = typeof(SpriteBatch).GetField("samplerState", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo DepthStencilState_FieldInfo = typeof(SpriteBatch).GetField("depthStencilState", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo RasterizerState_FieldInfo = typeof(SpriteBatch).GetField("rasterizerState", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo Effect_FieldInfo = typeof(SpriteBatch).GetField("customEffect", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo TransformMatrix_FieldInfo = typeof(SpriteBatch).GetField("transformMatrix", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void GetSpritebatchDetails(this SpriteBatch spriteBatch, out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect,out Matrix matrix)
        {
            spriteSortMode = (SpriteSortMode)SpriteSortMode_FieldInfo.GetValue(spriteBatch);
            blendState = (BlendState)BlendState_FieldInfo.GetValue(spriteBatch);
            samplerState = (SamplerState)SamplerState_FieldInfo.GetValue(spriteBatch);
            depthStencilState = (DepthStencilState)DepthStencilState_FieldInfo.GetValue(spriteBatch);
            rasterizerState = (RasterizerState)RasterizerState_FieldInfo.GetValue(spriteBatch);
            effect = (Effect)Effect_FieldInfo.GetValue(spriteBatch);
            matrix = (Matrix)TransformMatrix_FieldInfo.GetValue(spriteBatch);
        }
        public static void BeginSpriteBatchFromTemplate(this SpriteBatchData data, BlendState blendState = null, SpriteSortMode spriteSortMode = SpriteSortMode.Deferred, Effect effect = null, SamplerState samplerState = null, SpriteBatch spriteBatch = null)
        {
            spriteBatch ??= Main.spriteBatch;
            switch(data)
            {
                case SpriteBatchData.WorldDrawingData:
                    spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend, samplerState ?? Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.Transform);
                    break;
                case SpriteBatchData.TileDrawingData:
                    spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend);
                    break;
                case SpriteBatchData.TileSpecialDrawingData:
                    spriteBatch.Begin(spriteSortMode, blendState ?? BlendState.AlphaBlend, samplerState ?? Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Matrix.Identity);
                    break;
                case SpriteBatchData.UIDrawingDataScale:
                    spriteBatch.Begin(spriteSortMode, blendState, samplerState, null, null, effect, Main.UIScaleMatrix);
                    break;
                case SpriteBatchData.UIDrawingDataGame:
                    spriteBatch.Begin(spriteSortMode, blendState, samplerState, null, null, effect, Main.GameViewMatrix.ZoomMatrix);
                    break;
                case SpriteBatchData.UIDrawingDataNone:
                    spriteBatch.Begin(spriteSortMode, blendState, samplerState, null, null, effect, Matrix.Identity);
                    break;
                case SpriteBatchData.AdditiveParticleDrawing:
                    spriteBatch.Begin(spriteSortMode, BlendState.Additive, samplerState ?? SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);
                    break;
            }
        }
    }
    public static class RadianceDrawing
    {
        public enum SpriteBatchData
        {
            WorldDrawingData,
            TileDrawingData,
            TileSpecialDrawingData,
            UIDrawingDataScale,
            UIDrawingDataGame,
            UIDrawingDataNone,
            AdditiveParticleDrawing
        }

        public static void DrawHorizontalRadianceBar(Vector2 position, float maxRadiance, float storedRadiance, RadianceBarUIElement ui = null)
        {
            Texture2D meterTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeter").Value;
            Texture2D barTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ItemRadianceMeterBar").Value;
            float alpha = ui != null ? ui.timerModifier : 1; 

            int meterWidth = meterTexture.Width;
            int meterHeight = meterTexture.Height;
            Vector2 padding = (meterTexture.Size() - barTexture.Size()) / 2;
            int barWidth = (int)(meterWidth - 2 * padding.X);
            int barHeight = barTexture.Height;

            float radianceCharge = Math.Min(storedRadiance, maxRadiance);
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
                    storedRadiance + " / " + maxRadiance,
                    position,
                    Color.Lerp(new Color(255, 150, 0), new Color(255, 255, 192), fill),
                    0,
                    font.MeasureString(storedRadiance + " / " + maxRadiance) / 2,
                    Vector2.One
                    );
            }
        }
        public static void DrawHoverableItem(SpriteBatch spriteBatch, int type, Vector2 pos, int stack, Color? color = null, float scale = 1f, bool hoverable = true, bool encycloradia = false)
        {
            color ??= Color.White; 
            Item itemToDraw = GetItem(type);
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            ItemSlot.DrawItemIcon(itemToDraw, 0, spriteBatch, pos, scale, 256, color.Value);
            if (stack > 1)
                Utils.DrawBorderStringFourWay(spriteBatch, font, stack.ToString(), pos.X - 10f, pos.Y + Item.GetDrawHitbox(type, null).Height / 2 + Math.Max(0, 20 - Item.GetDrawHitbox(type, null).Height), (Color)color, Color.Black, font.MeasureString(stack.ToString()) / 2, scale);
            
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
                    if (encycloradia && RadianceSets.EncycloradiaRelatedEntry[item.type] != string.Empty)
                    {
                        EncycloradiaEntry entry = EncycloradiaSystem.FindEntry(RadianceSets.EncycloradiaRelatedEntry[item.type]);
                        if (entry.unlockedStatus == UnlockedStatus.Unlocked)
                        {
                            item.GetGlobalItem<EncycloradiaRelatedEntryGlobalItem>().shouldLeadToRelevantEntry = true;
                            if (Main.mouseLeft && Main.mouseLeftRelease)
                            {
                                Encycloradia.Encycloradia encycloradiaInstance = EncycloradiaUI.Instance.encycloradia;
                                encycloradiaInstance.entryHistory.Add((encycloradiaInstance.currentEntry.internalName, encycloradiaInstance.leftPageIndex));
                                encycloradiaInstance.GoToEntry(entry);
                                SoundEngine.PlaySound(EncycloradiaUI.pageTurnSound);
                            }
                        }
                    }
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
        // todo: drawspike sucks balls because it's not a texture
        public static void DrawSpike(SpriteBatch spriteBatch, SpriteBatchData spriteBatchData, Vector2 startPosition, Vector2 endPosition, Color color, int thickness)
        {
            float rotation = (endPosition - startPosition).ToRotation();

            Texture2D rayTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            int width = (int)Vector2.Distance(startPosition, endPosition);
            int height = thickness;
            Vector2 adjustedPos = startPosition - new Vector2(0, height / 2).RotatedBy(rotation);
            var drawRect = new Rectangle((int)adjustedPos.X, (int)adjustedPos.Y, width, height);

            Effect spikeEffect = Terraria.Graphics.Effects.Filters.Scene["Spike"].GetShader().Shader;
            spikeEffect.Parameters["startPos"].SetValue(startPosition);
            spikeEffect.Parameters["endPos"].SetValue(endPosition);
            spikeEffect.Parameters["color"].SetValue(color.ToVector4());
            spikeEffect.Parameters["thickness"].SetValue(height);

            spriteBatch.End();
            spriteBatchData.BeginSpriteBatchFromTemplate(BlendState.Additive, effect:spikeEffect);

            Main.spriteBatch.Draw(rayTexture, drawRect, null, Color.White, rotation, Vector2.Zero, SpriteEffects.None, 0);

            spriteBatch.End();
            spriteBatchData.BeginSpriteBatchFromTemplate();
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
            Texture2D circleTexture = Radiance.notBlankTexture;
            color *= ModContent.GetInstance<RadianceConfig>().AreaOfEffectAlpha;
            Vector2 pos = worldCoords - Main.screenPosition;

            Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["Circle"].GetShader().Shader;
            circleEffect.Parameters["color"].SetValue(color.ToVector4());
            circleEffect.Parameters["radius"].SetValue(radius);

            Main.spriteBatch.End();
            data.BeginSpriteBatchFromTemplate(effect: circleEffect);

            Main.spriteBatch.Draw(
            circleTexture,
            pos,
            null,
            color,
            0,
            Vector2.One / 2f,
            radius * 2.22f,
            SpriteEffects.None,
            0
            );

            Main.spriteBatch.End();
            data.BeginSpriteBatchFromTemplate();
        }
        public static void DrawSquare(Vector2 worldCoords, Color color, float halfWidth, SpriteBatchData data, bool drawDetails = true)
        {
            Texture2D circleTexture = Radiance.notBlankTexture;
            color *= ModContent.GetInstance<RadianceConfig>().AreaOfEffectAlpha;
            Vector2 pos = worldCoords - Main.screenPosition;
            SquareAOEDrawingMode drawingMode = ModContent.GetInstance<RadianceConfig>().SquareAOEDrawingModeConfig;

            Effect squareEffect = Terraria.Graphics.Effects.Filters.Scene["Square"].GetShader().Shader;
            squareEffect.Parameters["color"].SetValue(color.ToVector4());
            squareEffect.Parameters["halfWidth"].SetValue(halfWidth);
            if (drawingMode != SquareAOEDrawingMode.DetailsOnly)
            {
                Main.spriteBatch.End();
                data.BeginSpriteBatchFromTemplate(effect: squareEffect);

                Main.spriteBatch.Draw(circleTexture, pos, null, color, 0, Vector2.One / 2f, halfWidth * 2f, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                data.BeginSpriteBatchFromTemplate();
            }
            if(drawingMode != SquareAOEDrawingMode.BoxOnly && drawDetails)
            {
                Texture2D starTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SquareExtraStar").Value;
                int cornerDistance = -5;
                int sideDistance = 7;
                int starDistance = 28;
                if (drawingMode == SquareAOEDrawingMode.DetailsOnly)
                {
                    cornerDistance = -16;
                    sideDistance = 2;
                    starDistance = 23;
                }
                Main.spriteBatch.Draw(starTexture, worldCoords + Vector2.UnitY * -(halfWidth + starDistance) - Main.screenPosition, null, color, 0, starTexture.Size() / 2, 1f, SpriteEffects.None, 0);
                for (int i = 0; i < 4; i++)
                {
                    Texture2D cornerTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SquareExtraCorner").Value;
                    Texture2D sideTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/SquareExtraSide").Value;
                    float rotation = TwoPi / 4f * i;
                    Vector2 cornerPosition = Vector2.One.RotatedBy(rotation) * -(halfWidth + cornerDistance);
                    Vector2 sidePosition = Vector2.UnitY.RotatedBy(rotation) * -(halfWidth + sideDistance);
                    Main.spriteBatch.Draw(cornerTexture, worldCoords + cornerPosition - Main.screenPosition, null, color, rotation, cornerTexture.Size() / 2, 1f, SpriteEffects.None, 0);
                    Main.spriteBatch.Draw(sideTexture, worldCoords + sidePosition - Main.screenPosition, null, color, rotation, sideTexture.Size() / 2, 1f, SpriteEffects.None, 0);
                }
            }        
        }
        public static void DrawRadianceIOSlot(RadianceIOIndicatorMode type, Vector2 position)
        {
            bool colorblindEnabled = ModContent.GetInstance<AccessibilityConfig>().ColorblindMode;
            Color color = type == RadianceIOIndicatorMode.Input ? ModContent.GetInstance<AccessibilityConfig>().radianceInputColor : ModContent.GetInstance<AccessibilityConfig>().radianceOutputColor;
            if (colorblindEnabled)
            {
                Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ColorblindShapes").Value;
                Rectangle frame = new Rectangle(0, 0, 14, 16);
                if (type == RadianceIOIndicatorMode.Output)
                    frame.X += 16;

                Main.spriteBatch.Draw(texture, position - Main.screenPosition, frame, color, 0, frame.Size() / 2, 1, SpriteEffects.None, 0);
            }
            else
            {
                RadianceDrawing.DrawSoftGlow(position, color, Math.Max(0.2f * (float)Math.Abs(SineTiming(90)), 0.16f));
                RadianceDrawing.DrawSoftGlow(position, Color.White, Math.Max(0.15f * (float)Math.Abs(SineTiming(90)), 0.10f));
            }
        }
    }
}