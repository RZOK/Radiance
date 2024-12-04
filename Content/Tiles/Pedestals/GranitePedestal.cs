using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;

namespace Radiance.Content.Tiles.Pedestals
{
    public class GranitePedestal : PedestalTile<GranitePedestalTileEntity>
    {
        public GranitePedestal() : base(ModContent.ItemType<GranitePedestalItem>(), new Vector2(-16, -17)) { }
    }
    public class GranitePedestalTileEntity : PedestalTileEntity
    {
        public GranitePedestalTileEntity() : base(ModContent.TileType<GranitePedestal>()) { }
    }

    public class GranitePedestalItem : BaseTileItem
    {
        public GranitePedestalItem() : base("GranitePedestalItem", "Granite Pedestal", "Right Click with an item in hand to place it on the pedestal", "GranitePedestal", 5, 0, ItemRarityID.Blue)
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