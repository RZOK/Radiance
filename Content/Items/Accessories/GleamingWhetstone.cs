using Radiance.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Accessories
{
    public class GleamingWhetstone : ModItem, IOnTransmutateEffect
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gleaming Whetstone");
            Tooltip.SetDefault("Can be Transmutated endlessly to reforge its prefix");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 1, 25);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public void OnTransmutate()
        {
            Item.Prefix(-2);
        }
    }
}