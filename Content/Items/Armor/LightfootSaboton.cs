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
        public override void Load()
        {
            RadiancePlayer.PostUpdateEquipsEvent += LightfootDash;
        }
        public override void Unload()
        {
            RadiancePlayer.PostUpdateEquipsEvent -= LightfootDash;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lightfoot Sabaton");
            Tooltip.SetDefault("10% reduced damage\nDouble tap a direction to perform a defensive dash");
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
        public override void UpdateEquip(Player player)
        {
            player.endurance += 0.05f;
        }
        public void LightfootDash(Player player)
        {
            if (player.legs == ModContent.ItemType<LightfootSaboton>())
            {
                player.dashType = ModContent.ItemType<LightfootSaboton>();

                if (player.dashTime > 0)
                    player.dashTime--;
                if (player.controlRight && player.releaseRight || (player.controlLeft && player.releaseLeft))
                {
                    int dir = player.controlLeft ? -1 : 1;
                    if (player.dashTime <= 0)
                    {
                        player.dashTime = 15;
                        return;
                    }
                    
                }
            }
        }
    }
    //public class LegsSystem : ModSystem
    //{
    //    public static List<int> cachedLegs;
    //    public override void Load()
    //    {
    //        cachedLegs = new List<int>();
    //    }
    //    public override void Unload()
    //    {
    //        cachedLegs = null;
    //    }
    //    public override void OnWorldLoad()
    //    {
    //        for (int i = 0; i < ItemLoader.ItemCount; i++)
    //        {
    //            if (i <= 0 || i >= ItemLoader.ItemCount)
    //                continue;

    //            Item item = RadianceUtils.GetItem(i);
    //            if (item.legSlot == -1)
    //                continue;

    //            cachedLegs.Add(item.type);
    //        }
    //    }
    //}
}
