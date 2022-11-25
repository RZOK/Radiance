using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Radiance.Content.Tiles.Transmutator;

namespace Radiance.Content.Items.TileItems
{
    public class TransmutatorItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiance Transmutator");
            Tooltip.SetDefault("Uses concentrated Radiance to convert items into other items\nRequires a functioning Projector below it to work");
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
            Item.createTile = ModContent.TileType<Transmutator>();
        }
    }
}