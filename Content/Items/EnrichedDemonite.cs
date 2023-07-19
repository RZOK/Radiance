namespace Radiance.Content.Items
{
    public class EnrichedDemonite : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enriched Demonite");
            Tooltip.SetDefault("'Sinister!'");
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 2, 50);
            Item.rare = ItemRarityID.Blue;
        }
    }
}