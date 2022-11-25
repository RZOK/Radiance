using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.ProjectorLenses
{
    public class ShimmeringGlass : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flareglass");
            Tooltip.SetDefault("'Glimmers in the light'");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 4);
            Item.rare = ItemRarityID.Blue;
        }
    }
}