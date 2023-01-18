using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Utilities
{
    partial class RadianceUtils
    {
        public static Item GetPlayerHeldItem() => Main.mouseItem.type == ItemID.None ? Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] : Main.mouseItem;
        public static Item GetItem(int type) => type < ItemID.Count ? new Item(type) : ItemLoader.GetItem(type).Item;
        public static string GetBuffName(int type) => type < BuffID.Count ? BuffID.Search.GetName(type) : BuffLoader.GetBuff(type).Name;
    }

}
