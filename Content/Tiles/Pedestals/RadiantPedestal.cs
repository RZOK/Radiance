using Radiance.Content.Items.ProjectorLenses;

namespace Radiance.Content.Tiles.Pedestals
{
    public class RadiantPedestal : PedestalTile<RadiantPedestalTileEntity>
    {
        public RadiantPedestal() : base(ModContent.ItemType<RadiantPedestalItem>(), new Vector2(-16, -17))
        {
        }
    }

    public class RadiantPedestalTileEntity : PedestalTileEntity
    {
        public RadiantPedestalTileEntity() : base(ModContent.TileType<RadiantPedestal>())
        {
        }
    }

    public class RadiantPedestalItem : BasePedestalItem
    {
        public RadiantPedestalItem() : base(nameof(RadiantPedestalItem), "Ornate Pedestal", nameof(RadiantPedestal))
        {
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ShimmeringGlass>(2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}