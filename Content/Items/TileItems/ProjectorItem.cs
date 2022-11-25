using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Radiance.Content.Tiles.Transmutator;

namespace Radiance.Content.Items.TileItems
{
    public class ProjectorItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiance Projector");
            Tooltip.SetDefault("Provides Radiance to a Transmutator above\nRequires a Radiance-focusing lens to be installed in order to function");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Green;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Projector>();
        }
    }
}