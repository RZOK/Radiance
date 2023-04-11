using Radiance.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Accessories
{
    public class RingofFrugality : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Band of Frugality");
            Tooltip.SetDefault("Reduces the amount of Radiance that Instruments consume by 15%");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 1, 10);
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadiancePlayer>().radianceDiscount += 0.15f;
        }
    }
}