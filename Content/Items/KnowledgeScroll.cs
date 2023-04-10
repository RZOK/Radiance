using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items
{
    public class KnowledgeScroll : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
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