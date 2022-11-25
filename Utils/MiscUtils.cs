﻿using Terraria;
using Terraria.ID;

namespace Radiance.Utils
{
    partial class RadianceUtils
    {
        public static Item GetPlayerHeldItem()
        {
            Player player = Main.LocalPlayer;
            return Main.mouseItem.type == ItemID.None ? player.inventory[player.selectedItem] : Main.mouseItem;
        }
    }
}
