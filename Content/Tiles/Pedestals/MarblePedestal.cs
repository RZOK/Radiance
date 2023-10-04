using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;

namespace Radiance.Content.Tiles.Pedestals
{
    public class MarblePedestal : PedestalTile<MarblePedestalTileEntity>
    {
        public MarblePedestal() : base(ModContent.ItemType<MarblePedestalItem>(), new Vector2(-16, -20)) { }
    }
    public class MarblePedestalTileEntity : PedestalTileEntity
    {
        public MarblePedestalTileEntity() : base(ModContent.TileType<MarblePedestal>()) { }
    }

    public class MarblePedestalItem : BaseTileItem
    {
        public MarblePedestalItem() : base("MarblePedestalItem", "Marble Pedestal", "Right Click with an item in hand to place it on the pedestal", "MarblePedestal", 5, 0, ItemRarityID.Blue)
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