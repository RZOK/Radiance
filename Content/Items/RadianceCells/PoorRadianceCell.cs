using Radiance.Content.Items.BaseItems;
using Radiance.Content.Tiles.Pedestals;

namespace Radiance.Content.Items.RadianceCells
{
    public class PoorRadianceCell : BaseContainer
    {
        public PoorRadianceCell() : base(
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/PoorRadianceCellGlow").Value,
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/PoorRadianceCellMini").Value,
            1000,
            true,
            ContainerMode.InputOutput)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Poor Radiance Cell");
            Tooltip.SetDefault("Passively leaks a small amount of Radiance into the atmosphere");
            Item.ResearchUnlockCount = 1;
            RadianceSets.SetPedestalStability[Type] = 10;
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
            float leakValue = 0.002f;
            if (storedRadiance != 0)
                storedRadiance -= Math.Min(storedRadiance, leakValue);
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