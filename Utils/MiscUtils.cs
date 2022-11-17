using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Utils
{
    public class MiscUtils
    {
        public static Item GetPlayerHeldItem()
        {
            Player player = Main.LocalPlayer;
            return Main.mouseItem.type == ItemID.None ? player.inventory[player.selectedItem] : Main.mouseItem;
        }
    }
}
