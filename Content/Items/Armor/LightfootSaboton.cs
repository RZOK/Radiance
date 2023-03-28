using Radiance.Core;
using Radiance.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class LightfootSaboton : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lightfoot Saboton");
            Tooltip.SetDefault("Provides a unique bonus if your other two armor pieces are not in the same set");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 1, 25);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 7;
        }
    }
    public class LegsSystem : ModSystem
    {
        public static List<int> Legs;
        public override void Load()
        {
            Legs = new List<int>();
        }
        public override void OnWorldLoad()
        {
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                if (i <= 0 || i >= ItemLoader.ItemCount)
                    continue;

                Item item = RadianceUtils.GetItem(i);
                if ()

            }
        }
    }
}
