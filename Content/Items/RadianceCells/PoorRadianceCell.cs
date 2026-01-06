using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.RadianceCells
{
    public class PoorRadianceCell : BaseContainer
    {
        public PoorRadianceCell() : base(
            new Dictionary<BaseContainer_TextureType, string>()
            {
                [BaseContainer_TextureType.Mini] = "Radiance/Content/Items/RadianceCells/PoorRadianceCellMini",
                [BaseContainer_TextureType.RadianceAdjusting] = "Radiance/Content/Items/RadianceCells/PoorRadianceCellGlow"
            },
            200,
            true)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Poor Radiance Cell");
            Tooltip.SetDefault("Passively leaks a small amount of Radiance when on the ground or atop a Pedestal");
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
            {
                storedRadiance -= 0.002f;
                if (storedRadiance < 0)
                    storedRadiance = 0;
            }
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