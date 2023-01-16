using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Tiles;
using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;
using Radiance.Utilities;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Radiance.Core.Interface
{
    public static class InterfaceDrawer
    {
        public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < layers.Count; k++)
            {
                if (layers[k].Name == "Vanilla: Interface Logic 1")
                {
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance I/O Tile Display", DrawRadianceIO, InterfaceScaleType.Game));
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Tile AOE Effect Display", DrawTileAOECircle, InterfaceScaleType.Game));
                }
                if (layers[k].Name == "Vanilla: Emote Bubbles")
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Ray Display", DrawRay, InterfaceScaleType.Game));
                if (layers[k].Name == "Vanilla: Mouse Text")
                {
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance Item/Tile Display", DrawRadianceOverTile, InterfaceScaleType.UI));
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance Tile Special Text Display", DrawTileSpecialText, InterfaceScaleType.UI));
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance Transmutator IO", DrawTransmutatorIO, InterfaceScaleType.UI));
                }
                if (layers[k].Name == "Vanilla: Player Chat")
                {
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Incomplete Entry Text", DrawIncompleteText, InterfaceScaleType.UI));
                }
            }
        }

        public static bool DrawRadianceOverTile()
        {
            Player player = Main.player[Main.myPlayer];
            Vector2 hoverCoords = player.GetModPlayer<RadianceInterfacePlayer>().radianceContainingTileHoverOverCoords;
            if (hoverCoords != new Vector2(-1, -1) && RadianceUtils.TryGetTileEntityAs((int)hoverCoords.X, (int)hoverCoords.Y, out RadianceUtilizingTileEntity entity))
            {
                if (entity.MaxRadiance > 0)
                {
                    RadianceDrawing.DrawHorizontalRadianceBar(
                        RadianceUtils.MultitileCenterWorldCoords((int)hoverCoords.X, (int)hoverCoords.Y) - Main.screenPosition,
                        "Tile2x2",
                        entity);
                }
            }
            return true;
        }

        public static bool DrawRay()
        {
            Player player = Main.player[Main.myPlayer];
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                for (int i = 0; i < Radiance.maxRays; i++)
                {
                    if (Radiance.radianceRay[i] != null && Radiance.radianceRay[i].active)
                    {
                        RadianceRay ray = Radiance.radianceRay[i];
                        RadianceDrawing.DrawRayBetweenTwoPoints(ray);
                    }
                }
            }
            return true;
        }

        public static bool DrawRadianceIO()
        {
            Player player = Main.player[Main.myPlayer];
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                foreach (var (i, j) in RadianceTransferSystem.Instance.Coords)
                {
                    if (RadianceUtils.TryGetTileEntityAs(i, j, out RadianceUtilizingTileEntity entity))
                    {
                        int currentPos = 0;
                        for (int x = 0; x < entity.Width * entity.Height; x++)
                        {
                            currentPos++;
                            string type = "";
                            if (entity.InputTiles.Contains(currentPos))
                                type = "Input";
                            else if (entity.OutputTiles.Contains(currentPos))
                                type = "Output";
                            if (type != "")
                            {
                                Vector2 pos = new Vector2(i + x % entity.Width, j + (float)Math.Floor((double)x / entity.Width)) * 16 + new Vector2(8, 8);
                                RadianceDrawing.DrawSoftGlow(pos, type == "Input" ? Color.Blue : Color.Red, Math.Max(0.2f * (float)Math.Abs(RadianceUtils.SineTiming(60)), 0.16f), Main.GameViewMatrix.TransformationMatrix);
                                RadianceDrawing.DrawSoftGlow(pos, Color.White, Math.Max(0.15f * (float)Math.Abs(RadianceUtils.SineTiming(60)), 0.10f), Main.GameViewMatrix.TransformationMatrix);
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static bool DrawTileAOECircle()
        {
            Player player = Main.player[Main.myPlayer];
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (mp.aoeCirclePosition != new Vector2(-1, -1))
                RadianceDrawing.DrawCircle(mp.aoeCirclePosition, new Vector4(mp.aoeCircleColor.X, mp.aoeCircleColor.Y, mp.aoeCircleColor.Z, 1 * Math.Max(0.2f, (mp.aoeCircleAlphaTimer * 3) / 255)), mp.aoeCircleScale * RadianceUtils.EaseOutCirc(mp.aoeCircleAlphaTimer / 20) + (float)(RadianceUtils.SineTiming(30) * mp.aoeCircleScale / 250), mp.aoeCircleMatrix);

            return true;
        }
        public static bool DrawIncompleteText()
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
        public static bool DrawTileSpecialText()
        {
            Player player = Main.player[Main.myPlayer];
            Vector2 hoverCoords = player.GetModPlayer<RadianceInterfacePlayer>().hoveringOverSpecialTextTileCoords;
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            if (mp.hoveringOverSpecialTextTileCoords != new Vector2(-1, -1) && RadianceUtils.TryGetTileEntityAs((int)hoverCoords.X, (int)hoverCoords.Y, out RadianceUtilizingTileEntity entity))
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    font,
                    mp.hoveringOverSpecialTextTileString,
                    RadianceUtils.MultitileCenterWorldCoords((int)hoverCoords.X, (int)hoverCoords.Y) - Main.screenPosition - new Vector2(-entity.Width * 8, entity.Height * 8 * RadianceUtils.EaseInOutQuart(Math.Clamp(player.GetModPlayer<RadianceInterfacePlayer>().hoveringOverSpecialTextTileAlphaTimer / 20 + 0.5f, 0.5f, 1))),
                    mp.hoveringOverSpecialTextTileColor,
                    0,
                    font.MeasureString(mp.hoveringOverSpecialTextTileString) / 2,
                    Vector2.One
                    );
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    font,
                    mp.hoveringOverSpecialTextTileItemTagString,
                    RadianceUtils.MultitileCenterWorldCoords((int)hoverCoords.X, (int)hoverCoords.Y) - Main.screenPosition - 
                    new Vector2(
                        -entity.Width * 8, 
                        entity.Height * 16 * RadianceUtils.EaseInOutQuart(Math.Clamp(player.GetModPlayer<RadianceInterfacePlayer>().hoveringOverSpecialTextTileAlphaTimer / 20 + 0.5f, 0.5f, 1))) - 
                        (Vector2.UnitX * (font.MeasureString(mp.hoveringOverSpecialTextTileString).X / 2 + 24)),
                    mp.hoveringOverSpecialTextTileColor,
                    0,
                    Vector2.Zero,
                    Vector2.One
                    );
            }
            return true;
        }
        public static bool DrawTransmutatorIO()
        {
            Player player = Main.player[Main.myPlayer];
            RadianceInterfacePlayer mp = player.GetModPlayer<RadianceInterfacePlayer>();
            Vector2 hoverCoords = mp.transmutatorIOCoords;
            float easedTimer = RadianceUtils.EaseOutCirc(mp.transmutatorIOTimer / 20);
            if (hoverCoords != new Vector2(-1, -1) && RadianceUtils.TryGetTileEntityAs((int)hoverCoords.X, (int)hoverCoords.Y, out TransmutatorTileEntity entity))
            {
                Vector2 outputCoords = RadianceUtils.MultitileCenterWorldCoords((int)hoverCoords.X, (int)hoverCoords.Y) + new Vector2(16) + Vector2.UnitX * easedTimer * 48;
                Vector2 inputCoords = RadianceUtils.MultitileCenterWorldCoords((int)hoverCoords.X, (int)hoverCoords.Y) + new Vector2(16) - Vector2.UnitX * easedTimer * 48;

                RadianceDrawing.DrawSoftGlow(outputCoords, Color.Red * easedTimer, Math.Max(0.4f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.35f), Matrix.Identity);
                RadianceDrawing.DrawSoftGlow(outputCoords, Color.White * easedTimer, Math.Max(0.2f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.27f), Matrix.Identity);
                
                RadianceDrawing.DrawSoftGlow(inputCoords, Color.Blue * easedTimer, Math.Max(0.4f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.35f), Matrix.Identity);
                RadianceDrawing.DrawSoftGlow(inputCoords, Color.White * easedTimer, Math.Max(0.2f * (float)Math.Abs(RadianceUtils.SineTiming(100)), 0.27f), Matrix.Identity);

                RadianceDrawing.DrawHoverableItem(Main.spriteBatch, entity.inputItem.type, inputCoords - Main.screenPosition, entity.inputItem.stack);
                RadianceDrawing.DrawHoverableItem(Main.spriteBatch, entity.outputItem.type, outputCoords - Main.screenPosition, entity.outputItem.stack);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
            }
            return true;
        }
    }
}