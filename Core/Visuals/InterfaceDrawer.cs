﻿using Radiance.Core.Config;
using Radiance.Core.Systems;
using ReLogic.Graphics;
using Terraria.UI;

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
                if (layers[k].Name == "Vanilla: Town NPC House Banners")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Hover UI Data", DrawHoverUIData, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Emote Bubbles")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Ray Display", DrawRays, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Player Chat")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Incomplete Entry Text", DrawIncompleteText, InterfaceScaleType.UI));
                if (layers[k].Name == "Vanilla: Interface Logic 4")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Fake Mouse Text", DrawFakeMouseText, InterfaceScaleType.UI));
            }
        }
        public bool DrawFakeMouseText()
        {
            RadianceInterfacePlayer mp = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
            if(mp.currentFakeHoverText != string.Empty && Main.mouseItem.IsAir)
            {
                DrawFakeItemHover(Main.spriteBatch, mp.currentFakeHoverText.Split("\n"), fancy: mp.fancyHoverTextBackground);
            }
            return true;
        }

        public bool DrawHoverUIData()
        {
            RadianceInterfacePlayer mp = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>();
            foreach (HoverUIData data in mp.activeHoverData)
            {
                foreach (HoverUIElement element in data.elements)
                {
                    element.Update();
                    element.Draw(Main.spriteBatch);
                }
            }
            return true;
        }

        public bool DrawRays()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                foreach (RadianceRay ray in RadianceTransferSystem.rays)
                {
                    ray.DrawRay();
                }
            }
            return true;
        }
        public enum RadianceIOIndicatorMode
        {
            None,
            Input,
            Output
        }
        public static bool DrawRadianceIO()
        {
            Player player = Main.LocalPlayer;
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                bool colorblindEnabled = ModContent.GetInstance<AccessibilityConfig>().ColorblindMode;
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
                                Color color = type == RadianceIOIndicatorMode.Input ? ModContent.GetInstance<AccessibilityConfig>().radianceInputColor : ModContent.GetInstance<AccessibilityConfig>().radianceOutputColor;
                                if (colorblindEnabled)
                                {
                                    Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ColorblindShapes").Value;
                                    Rectangle frame = new Rectangle(0, 0, 14, 16);
                                    if (type == RadianceIOIndicatorMode.Output)
                                        frame.X += 16;

                                    Main.spriteBatch.Draw(texture, pos - Main.screenPosition, frame, color, 0, frame.Size() / 2, 1, SpriteEffects.None, 0);
                                }
                                else
                                {
                                    RadianceDrawing.DrawSoftGlow(pos, color, Math.Max(0.2f * (float)Math.Abs(SineTiming(90)), 0.16f));
                                    RadianceDrawing.DrawSoftGlow(pos, Color.White, Math.Max(0.15f * (float)Math.Abs(SineTiming(90)), 0.10f));
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool DrawIncompleteText()
        {
            Player player = Main.LocalPlayer;
            string str = player.GetModPlayer<RadianceInterfacePlayer>().incompleteEntryText;
            if (str != string.Empty)
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                pos.X = Math.Min(Main.screenWidth - font.MeasureString(str).X - 6, pos.X);
                Utils.DrawBorderStringFourWay(Main.spriteBatch, font, str, pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero);
            }
            return true;
        }
    }
}