using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Items.Materials;

namespace Radiance.Content.Items.RadianceCells
{
    public class StandardRadianceCell : BaseContainer
    {
        public StandardRadianceCell() : base(

            new Dictionary<string, string>()
            {
                ["Mini"] = "Radiance/Content/Items/RadianceCells/StandardRadianceCellMini",
                ["RadianceAdjusting"] = "Radiance/Content/Items/RadianceCells/StandardRadianceCellGlow"
            },
            4000,
            true,
            ContainerMode.InputOutput)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Standard Radiance Cell");
            Item.ResearchUnlockCount = 1;
            RadianceSets.SetPedestalStability[Type] = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }
        public override void AddRecipes()
        {
            //Two recipes for letting people use their now-obsolete poor cells
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PoorRadianceCell>(), 1)
                .AddIngredient(ModContent.ItemType<ShimmeringGlass>(), 1)
                .AddIngredient(ModContent.ItemType<PetrifiedCrystal>(), 5)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 3)
                .AddIngredient(ModContent.ItemType<ShimmeringGlass>(), 1)
                .AddIngredient(ModContent.ItemType<PetrifiedCrystal>(), 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}