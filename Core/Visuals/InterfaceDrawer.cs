using Radiance.Core.Systems;
using System.Collections.Specialized;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Radiance.Core.Visuals
{
    public class InterfaceDrawer : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < layers.Count; k++)
            {
                if (layers[k].Name == "Vanilla: Interface Logic 1")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance I/O Tile Display", DrawRadianceIO, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: MP Player Names")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Hover UI Data", DrawHoverUIData, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Emote Bubbles")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Ray Display", DrawRays, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Interface Logic 4")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Fake Mouse Text", DrawFakeMouseText, InterfaceScaleType.UI));
                if (layers[k].Name == "Vanilla: Entity Health Bars")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Player Meters", DrawPlayerMeters, InterfaceScaleType.Game));
            }
        }
        private static bool DrawFakeMouseText()
        {
            RadianceInterfacePlayer mp = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
            if (mp.currentFakeHoverText != string.Empty && Main.mouseItem.IsAir)
            {
                string[] strings = mp.currentFakeHoverText.Split("\n");

                var font = FontAssets.MouseText.Value;
                float boxWidth;
                float boxHeight = -16;
                Vector2 pos = Main.MouseScreen + new Vector2(30, 30);

                string widest = strings.OrderBy(n => ChatManager.GetStringSize(font, n, Vector2.One).X).Last();
                boxWidth = ChatManager.GetStringSize(font, widest, Vector2.One).X + 20;

                foreach (string str in strings)
                {
                    boxHeight += ChatManager.GetStringSize(font, str, Vector2.One).Y;
                }

                if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
                {
                    pos.X += 8;
                    pos.Y += 2;
                }

                if (pos.X + ChatManager.GetStringSize(font, widest, Vector2.One).X > Main.screenWidth)
                    pos.X = (int)(Main.screenWidth - boxWidth);

                if (pos.Y + ChatManager.GetStringSize(font, widest, Vector2.One).Y > Main.screenHeight)
                    pos.Y = (int)(Main.screenHeight - boxHeight);

                if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
                {
                    RadianceDrawing.DrawInventoryBackground(Main.spriteBatch, mp.hoverTextBGTexture, (int)pos.X - 14, (int)pos.Y - 10, (int)boxWidth + 6, (int)boxHeight + 28, mp.hoverTextBGColor);
                }
                foreach (string str in strings)
                {
                    Utils.DrawBorderString(Main.spriteBatch, str, pos, Color.White);
                    pos.Y += ChatManager.GetStringSize(font, str, Vector2.One).Y;
                }
            }
            return true;
        }

        private static bool DrawHoverUIData()
        {
            RadianceInterfacePlayer mp = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
           
            Main.spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(spriteSortMode, blendState, SamplerState.PointWrap, depthStencilState, rasterizerState, effect, matrix); //pointwrap so its not blurry when zoomed

            foreach (HoverUIData data in mp.activeHoverData)
            {
                foreach (HoverUIElement element in data.elements)
                {
                    element.Update();
                    element.Draw(Main.spriteBatch);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
            return true;
        }

        private static bool DrawRays()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadianceInterfacePlayer>().canSeeRays)
            {
                foreach (RadianceRay ray in RadianceTransferSystem.rays)
                {
                    ray.DrawRay();
                }
                Main.spriteBatch.GetSpritebatchDetails(out SpriteSortMode spriteSortMode, out BlendState blendState, out SamplerState samplerState, out DepthStencilState depthStencilState, out RasterizerState rasterizerState, out Effect effect, out Matrix matrix);
                Main.spriteBatch.End();

                Effect circleEffect = Terraria.Graphics.Effects.Filters.Scene["HorizEdgeSoften"].GetShader().Shader;
                circleEffect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/RayTiling2").Value);
                circleEffect.Parameters["fadeThreshold"].SetValue(0.92f);
                circleEffect.Parameters["color"].SetValue(new Color(247, 136, 125).ToVector4() * 0.5f);

                Main.spriteBatch.Begin(spriteSortMode, BlendState.Additive, samplerState, depthStencilState, rasterizerState, circleEffect, matrix);
                foreach (RadianceRay ray in RadianceTransferSystem.rays)
                {
                    if ((ray.hasIoAtEnds[0] && ray.hasIoAtEnds[1]) || (ray.hasIoAtEnds[2] && ray.hasIoAtEnds[3]))
                        ray.DrawRayOverlay();
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, matrix);
            }
            return true;
        }
        public enum RadianceIOIndicatorMode
        {
            None,
            Input,
            Output
        }
        private static bool DrawRadianceIO()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadianceInterfacePlayer>().canSeeRays)
            {
                foreach (RadianceUtilizingTileEntity entity in TileEntity.ByID.Values.Where(x => x as RadianceUtilizingTileEntity != null))
                {
                    if (OnScreen(new Rectangle(entity.Position.X * 16, entity.Position.Y * 16, entity.Width * 16, entity.Height * 16)))
                    {
                        int currentPos = 0;
                        for (int x = 0; x < entity.Width * entity.Height; x++)
                        {
                            currentPos++;
                            RadianceIOIndicatorMode type = RadianceIOIndicatorMode.None;
                            if (entity.inputTiles.Contains(currentPos))
                                type = RadianceIOIndicatorMode.Input;
                            else if (entity.outputTiles.Contains(currentPos))
                                type = RadianceIOIndicatorMode.Output;

                            if (type != RadianceIOIndicatorMode.None)
                            {
                                Vector2 pos = new Vector2(entity.Position.X + x % entity.Width, entity.Position.Y + (x - x % entity.Width) / entity.Width) * 16 + Vector2.One * 8;
                                RadianceDrawing.DrawRadianceIOSlot(type, pos);
                            }
                        }
                    }
                }
            }
            return true;
        }
        private static bool DrawPlayerMeters()
        {
            Player player = Main.LocalPlayer;
            float yOffset = 30;
            float xOffset = 30;
            float yDist = 0;
            Texture2D backgroundTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/MeterBackground").Value;
            OrderedDictionary meters = player.GetModPlayer<MeterPlayer>().activeMeters; 
            foreach (MeterInfo info in meters.Keys)
            {;
                MeterVisual visual = (MeterVisual)meters[info];

                float idealY = yDist;
                if(!visual.position.HasValue)
                    visual.position = Vector2.UnitY * (30 + idealY);

                Vector2 position = Main.LocalPlayer.MountedCenter - Main.screenPosition + Vector2.UnitY * (yOffset + Main.LocalPlayer.gfxOffY) + visual.position.Value;

                float current = info.current();
                float max = info.max();
                float drawPercent = current / max;
                int lowerDrawPercent = (int)drawPercent; 
                int upperDrawPercent = (int)MathF.Ceiling(drawPercent);

                Color color = info.colorFunction(drawPercent);
                float alpha = visual.timer / MeterVisual.METER_VISUAL_TIMER_MAX;

                Main.spriteBatch.Draw(backgroundTex, position - Vector2.UnitX * xOffset, null, color * alpha, 0, backgroundTex.Size() / 2f, 1f, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(info.tex, position - Vector2.UnitX * xOffset, null, Color.White * alpha, 0, info.tex.Size() / 2f, 1f, SpriteEffects.None, 0);
                if (lowerDrawPercent > 0)
                {
                    Color lowerColor = info.colorFunction(lowerDrawPercent);
                    RadianceDrawing.DrawMeter(position, 1f, lowerColor * alpha, 1f);
                    RadianceDrawing.DrawMeter(position, drawPercent % 1f, color * alpha, 1f, false);
                }
                else
                    RadianceDrawing.DrawMeter(position, drawPercent, color * alpha, 1f);

                visual.position = Vector2.Lerp(visual.position.Value, Vector2.UnitY * idealY, 0.3f);
                yDist += 18f;
            }
            return true;
        }
    }
}