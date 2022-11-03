using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Radiance.Content.Tiles;

namespace Radiance.Content.Items.TileItems
{
    public class PedestalItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pedestal");
            Tooltip.SetDefault("Right click with an item in hand to place it on the pedestal");
        }
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(0, 0, 5, 0);
            Item.rare = ItemRarityID.Blue;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Pedestal>();
        }
    }
}