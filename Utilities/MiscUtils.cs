using Terraria;
using Terraria.ID;

namespace Radiance.Utilities
{
    partial class RadianceUtils
    {
        public static Item GetPlayerHeldItem() => Main.mouseItem.type == ItemID.None ? Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] : Main.mouseItem;
    }

}
