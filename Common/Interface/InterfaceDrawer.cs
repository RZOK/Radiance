using Microsoft.Xna.Framework;
using Radiance.Content.Tiles;
using Radiance.Core.Systems;
using Radiance.Utils;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;

namespace Radiance.Common.Interface
{
    public static class InterfaceDrawer
    {
        public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < layers.Count; k++)
            {
                if (layers[k].Name == "Vanilla: Mouse Text")
                {
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance Item/Tile Display", DrawRadianceOverTile, InterfaceScaleType.UI));
                }
                if (layers[k].Name == "Vanilla: Interface Logic 1")
                {
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Radiance I/O Tile Display", DrawRadianceIO, InterfaceScaleType.Game));
                }
                if (layers[k].Name == "Vanilla: Emote Bubbles")
                {
                    layers.Insert(k + 1, new LegacyGameInterfaceLayer("Radiance: Ray Display", DrawRay, InterfaceScaleType.Game));
                }
            }
        }

        private static bool DrawRadianceOverTile()
        {
            Player player = Main.player[Main.myPlayer];
            if (player.GetModPlayer<RadiancePlayer>().hoveringOverRadianceContainingTile)
            {
                Vector2 hoverCoords = player.GetModPlayer<RadiancePlayer>().radianceContainingTileHoverOverCoords;
                if (TileUtils.TryGetTileEntityAs((int)hoverCoords.X, (int)hoverCoords.Y, out RadianceUtilizingTileEntity entity))
                {
                    if (entity.MaxRadiance > 0)
                    {
                        RadianceDrawing.DrawHorizontalRadianceBar(
                            new Vector2(
                                hoverCoords.X * 16,
                                hoverCoords.Y * 16
                                ) - Main.screenPosition -
                            new Vector2(
                                Main.tile[(int)hoverCoords.X, (int)hoverCoords.Y].TileFrameX - (2 * Main.tile[(int)hoverCoords.X, (int)hoverCoords.Y].TileFrameX / 18),
                                Main.tile[(int)hoverCoords.X, (int)hoverCoords.Y].TileFrameY - (2 * Main.tile[(int)hoverCoords.X, (int)hoverCoords.Y].TileFrameY / 18)
                                ),
                            "Tile2x2",
                            entity);
                    }
                }
            }
            return true;
        }

        private static bool DrawRay()
        {
            Player player = Main.player[Main.myPlayer];
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                foreach (var item in RadianceTransferSystem.Instance.connectionsDictionary)
                {
                    RadianceDrawing.DrawRayBetweenTwoPoints(item.Key, item.Value);
                }
            }
            return true;
        }

        private static bool DrawRadianceIO()
        {
            Player player = Main.player[Main.myPlayer];
            if (player.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                for (int i = 0; i < Radiance.maxRadianceUtilizingTileEntities; i++)
                {
                    RadianceUtilizingTileEntity indexedTile = Radiance.radianceUtilizingTileEntityIndex[i];
                    if(RadianceTransferSystem.Instance.IsTileEntityReal(indexedTile))
                    { 
                            int currentPos = 0;
                            for (int y = 0; y < indexedTile.Height; y++)
                            {
                                for (int x = 0; x < indexedTile.Width; x++)
                                {
                                    currentPos++;
                                    string type = "";
                                    if (indexedTile.InputTiles.Contains(currentPos))
                                        type = "Input";
                                    else if (indexedTile.OutputTiles.Contains(currentPos))
                                        type = "Output";
                                    if (type != "") RadianceDrawing.DrawIOOnTile(new Vector2(indexedTile.Position.X + x - 1, indexedTile.Position.Y + y - 1) * 16 + new Vector2(2, 2), type);
                                }
                        }
                    }
                }
            }
            return true;
        }
    }
}