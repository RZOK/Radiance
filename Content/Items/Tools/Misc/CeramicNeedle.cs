using Radiance.Content.Items.Materials;

namespace Radiance.Content.Items.Tools.Misc
{
    public class CeramicNeedle : ModItem
    {
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ceramic Needle");
            Tooltip.SetDefault("Allows you to view applied Item Imprints and remove them");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Blue;
            Item.useTurn = true;
            Item.useAnimation = 30;
            Item.useTime = 30;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("SilverGroup", 2)
                .AddIngredient<PetrifiedCrystal>(6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}