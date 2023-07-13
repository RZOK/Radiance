using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.RadianceCells
{
    public class OverchargedRadianceCell : BaseContainer
    {
        public OverchargedRadianceCell() : base(
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/OverchargedRadianceCellGlow").Value,
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/OverchargedRadianceCellMini").Value,
            125,
            ContainerMode.InputOutput,
            ContainerQuirk.Standard,
            1.25f)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overcharging Radiance Cell");
            Tooltip.SetDefault("Absorbed resources produce 25% more Radiance");
            Item.ResearchUnlockCount = 1;
            RadianceSets.SetPedestalStability[Type] = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Lens, 2)
                .AddIngredient(ItemID.Glass, 4)
                .AddIngredient(ItemID.FallenStar, 2)
                .AddRecipeGroup(RecipeGroupID.IronBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}