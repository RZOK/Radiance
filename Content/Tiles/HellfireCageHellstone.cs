using Microsoft.Xna.Framework;
using Radiance.Core;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Radiance.Content.Tiles
{
    public class HellfireCageHellstone : ModTile
    {
        public override string Texture => "Terraria/Images/Tiles_" + TileID.Hellstone;

        public override void SetStaticDefaults()
        {
            TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;
            TileID.Sets.HellSpecial[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.TouchDamageHot[Type] = true;
            TileID.Sets.OreMergesWithMud[Type] = true;
            TileID.Sets.Ore[Type] = true;
            Main.tileMerge[Type][TileID.Hellstone] = true;
            Main.tileMerge[TileID.Hellstone][Type] = true;

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Hellstone");
            AddMapEntry(new Color(0, 97, 255), name);

            MineResist = 2f;
            MinPick = 65;
            HitSound = SoundID.Tink;
            DustType = 6;
            ItemDrop = ItemID.Hellstone;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile tile = Main.tile[i, j];
            if (!fail)
            {
                tile.LiquidType = LiquidID.Lava;
                tile.LiquidAmount = 128;
            }
        }
    }

    //public class HellfireCageHellstoneItem : BaseTileItem
    //{
    //    public override string Texture => "Terraria/Images/Item_" + ItemID.Hellstone;
    //    public HellfireCageHellstoneItem() : base("HellfireCageHellstoneItem", "Hellfire Cage Hellstone", "Always drops lava on death", "HellfireCageHellstone", 0, 0, ItemRarityID.Green) { }
    //}
}