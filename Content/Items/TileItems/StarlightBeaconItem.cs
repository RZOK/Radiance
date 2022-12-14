using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Radiance.Content.Tiles.StarlightBeacon;

namespace Radiance.Content.Items.TileItems
{
    public class StarlightBeaconItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starlight Beacon");
            Tooltip.SetDefault("Draws in all stars in a massive radius when deployed\nRequires a small amount of Radiance and Souls of Flight to operate");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 22;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 20, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<StarlightBeacon>();
        }
    }
}