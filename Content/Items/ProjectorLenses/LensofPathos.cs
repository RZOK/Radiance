using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.ProjectorLenses
{
    public class LensofPathos : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lens of Pathos");
            Tooltip.SetDefault("Allows you to perform transmutations involving the essence of emotions when slotted into a Projector");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 1, 0);
            Item.rare = ItemRarityID.Pink;
        }
    }
}