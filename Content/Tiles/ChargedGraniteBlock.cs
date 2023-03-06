using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace Radiance.Content.Tiles
{
	public class ChargedGraniteBlock : ModTile
	{
		public override string Texture => "Terraria/Images/Tile_" + TileID.GraniteBlock;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;

			DustType = DustID.Granite;
			ItemDrop = ItemID.GraniteBlock;

			AddMapEntry(new Color(200, 200, 200));
		}
	}
	public class ChargedGraniteBlockItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.GraniteBlock;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Charged Granite Block");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
        }
        public override void SetDefaults()
        {
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.placeStyle = 0;
            Item.width = 16;
            Item.height = 16;
            Item.createTile = ModContent.TileType<HangingGlowtus>();
        }
        //public override void AddRecipes()
        //{
        //    CreateRecipe()
        //        .AddIngredient(ItemID.PotSuspended)
        //        .AddIngredient<GlowtusItem>()
        //        .Register();
        //}
    }
}