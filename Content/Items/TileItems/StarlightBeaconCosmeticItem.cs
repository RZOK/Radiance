using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Radiance.Content.Tiles.StarlightBeacon;

namespace Radiance.Content.Items.TileItems
{
    public class StarlightBeaconCosmeticItem : ModItem
    {
        public override string Texture => "Radiance/Content/Items/TileItems/StarlightBeaconItem";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starcatcher Beacon (Cosmetic)");
            Tooltip.SetDefault("Mimics the visuals of the Starcatcher Beacon without the functionality");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 22;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 0, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<StarlightBeaconCosmetic>();
        }
    }
}