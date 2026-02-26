namespace Radiance.Content.Items.Materials
{
    public class Dynaglass : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dynaglass");
            Tooltip.SetDefault("'Thrums to the beat of your heart'");
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 34;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 2, 50);
            Item.rare = ItemRarityID.Green;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * 0.9f;
    }
}