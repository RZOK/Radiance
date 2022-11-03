using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common.Globals;
using Radiance.Content.Tiles;
using Radiance.Utils;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
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
            }
        }

        private static bool DrawRadianceOverTile()
        {
            Player player = Main.player[Main.myPlayer];
            if(player.GetModPlayer<RadiancePlayer>().hoveringOverRadianceContainingTile)
            {
                Vector2 hoverCoords = player.GetModPlayer<RadiancePlayer>().radianceContainingTileHoverOverCoords;
                if (TileUtils.TryGetTileEntityAs((int)hoverCoords.X, (int)hoverCoords.Y, out RadianceUtilizingTileEntity entity))
                {
                    if (entity.MaxRadiance > 0)
                    {
                        RadianceBarDrawer.DrawHorizontalRadianceBar(
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
    }
}