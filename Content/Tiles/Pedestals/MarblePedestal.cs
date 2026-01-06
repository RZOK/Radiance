using Radiance.Content.Items.ProjectorLenses;

namespace Radiance.Content.Tiles.Pedestals
{
    public class MarblePedestal : PedestalTile<MarblePedestalTileEntity>
    {
        public MarblePedestal() : base(ModContent.ItemType<MarblePedestalItem>(), new Vector2(-16, -20))
        {
        }
    }

    public class MarblePedestalTileEntity : PedestalTileEntity
    {
        public MarblePedestalTileEntity() : base(ModContent.TileType<MarblePedestal>())
        {
        }
    }

    public class MarblePedestalItem : BasePedestalItem
    {
        public MarblePedestalItem() : base(nameof(MarblePedestalItem), "Marble Pedestal", nameof(MarblePedestal))
        {
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Marble, 10)
                .AddIngredient<ShimmeringGlass>(2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}