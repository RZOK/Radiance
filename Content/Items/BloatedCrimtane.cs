namespace Radiance.Content.Items
{
    public class BloatedCrimtane : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bloated Crimtane");
            Tooltip.SetDefault("'Tumorous!'");
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