using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.RadianceCells
{
    public class PoorRadianceCell : BaseContainer
    {
        public PoorRadianceCell() : base(
            new Dictionary<string, string>()
            {
                ["Mini"] = "Radiance/Content/Items/RadianceCells/PoorRadianceCellMini",
                ["RadianceAdjusting"] = "Radiance/Content/Items/RadianceCells/PoorRadianceCellGlow"
            },
            1000,
            true)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Poor Radiance Cell");
            Tooltip.SetDefault("Passively leaks a small amount of Radiance into the atmosphere");
            Item.ResearchUnlockCount = 1;
            RadianceSets.SetPedestalStability[Type] = BASE_CONTAINER_REQUIRED_STABILITY;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 26;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateContainer(IInterfaceableRadianceCell entity)
        {
            if (storedRadiance > 0)
                storedRadiance = Math.Max(storedRadiance - 0.002f, 0);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Lens, 2)
                .AddIngredient(ItemID.Glass, 4)
                .AddIngredient(ItemID.FallenStar, 2)
                .AddRecipeGroup(RecipeGroupID.IronBar, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}