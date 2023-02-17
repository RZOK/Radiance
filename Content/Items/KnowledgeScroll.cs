using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items
{
    public class KnowledgeScroll : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scroll of Knowledge");
            Tooltip.SetDefault("Placeholder Tooltip");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 28;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Blue;
        }
    }
}