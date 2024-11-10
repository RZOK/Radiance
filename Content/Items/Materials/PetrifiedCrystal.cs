namespace Radiance.Content.Items.Materials
{
    public class PetrifiedCrystal : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Petrified Crystal");
            Tooltip.SetDefault("'Straight from the mycelium'");
            Item.ResearchUnlockCount = 50;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(0, 0, 0, 50);
            Item.rare = ItemRarityID.White;
        }
    }
}