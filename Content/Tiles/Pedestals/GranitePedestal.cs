using Radiance.Content.Items.ProjectorLenses;

namespace Radiance.Content.Tiles.Pedestals
{
    public class GranitePedestal : PedestalTile<GranitePedestalTileEntity>
    {
        public GranitePedestal() : base(ModContent.ItemType<GranitePedestalItem>(), new Vector2(-16, -17))
        {
        }
    }

    public class GranitePedestalTileEntity : PedestalTileEntity
    {
        public GranitePedestalTileEntity() : base(ModContent.TileType<GranitePedestal>())
        {
        }
    }

    public class GranitePedestalItem : BasePedestalItem
    {
        public GranitePedestalItem() : base(nameof(GranitePedestalItem), "Granite Pedestal", nameof(GranitePedestal))
        {
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Granite, 10)
                .AddIngredient<ShimmeringGlass>(2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}