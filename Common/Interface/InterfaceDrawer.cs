using Microsoft.Xna.Framework;
using Radiance.Content.Tiles;
using Radiance.Core;
using Radiance.Core.Systems;
using Radiance.Utils;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace Radiance.Common.Interface
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
                    //layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance Transmutator IO", DrawTransmutatorIO, InterfaceScaleType.UI));
                }
            }
        }

        public static bool DrawRadianceOverTile()
        {
            Player player = Main.player[Main.myPlayer];
            Vector2 hoverCoords = player.GetModPlayer<RadiancePlayer>().radianceContainingTileHoverOverCoords;
            if (player.GetModPlayer<RadiancePlayer>().radianceContainingTileHoverOverCoords != default && TileUtils.TryGetTileEntityAs((int)hoverCoords.X, (int)hoverCoords.Y, out RadianceUtilizingTileEntity entity))
            {
                if (entity.MaxRadiance > 0)
                {
                    RadianceDrawing.DrawHorizontalRadianceBar(
                        MathUtils.MultitileCenterWorldCoords((int)hoverCoords.X, (int)hoverCoords.Y) - Main.screenPosition,
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
                    if (TileUtils.TryGetTileEntityAs(i, j, out RadianceUtilizingTileEntity entity))
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
                            if (type != "") RadianceDrawing.DrawIOOnTile(new Vector2(i + x % entity.Width, j + (float)Math.Floor((double)x / entity.Width)) * 16 + new Vector2(2, 2), type);
                        }
                    }
                }
            }
            return true;
        }

        public static bool DrawTileAOECircle()
        {
            Player player = Main.player[Main.myPlayer];
            RadiancePlayer mp = player.GetModPlayer<RadiancePlayer>();
            if (mp.aoeCirclePosition != new Vector2(-1, -1))
                RadianceDrawing.DrawCircle(mp.aoeCirclePosition, new Vector4(mp.aoeCircleColor.X, mp.aoeCircleColor.Y, mp.aoeCircleColor.Z, 1 * (mp.aoeCircleAlphaTimer * 2) / 255), mp.aoeCircleScale * 1.11f * (float)MathUtils.EaseOutCirc(mp.aoeCircleAlphaTimer / 20) + (float)(MathUtils.sineTiming(30) * mp.aoeCircleScale / 250), mp.aoeCircleMatrix);

            return true;
        }

        public static bool DrawTileSpecialText()
        {
            Player player = Main.player[Main.myPlayer];
            Vector2 hoverCoords = player.GetModPlayer<RadiancePlayer>().hoveringOverSpecialTextTileCoords;
            RadiancePlayer mp = player.GetModPlayer<RadiancePlayer>();
            if (mp.hoveringOverSpecialTextTileCoords != new Vector2(-1, -1) && TileUtils.TryGetTileEntityAs((int)hoverCoords.X, (int)hoverCoords.Y, out RadianceUtilizingTileEntity entity))
            {
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    font,
                    mp.hoveringOverSpecialTextTileString,
                    MathUtils.MultitileCenterWorldCoords((int)hoverCoords.X, (int)hoverCoords.Y) - Main.screenPosition - Vector2.UnitY * (entity.Height * 16) * (float)MathUtils.EaseInOutQuart(Math.Clamp(player.GetModPlayer<RadiancePlayer>().hoveringOverSpecialTextTileAlphaTimer / 20 + 0.5f, 0.5f, 1)),
                    mp.hoveringOverSpecialTextTileColor * (mp.hoveringOverSpecialTextTileAlphaTimer / 20),
                    0,
                    Vector2.Zero,
                    Vector2.One
                    );
            }
            return true;
        }
    }
}