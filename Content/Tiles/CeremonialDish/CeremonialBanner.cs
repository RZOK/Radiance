using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Radiance.Core.Systems;
using Radiance.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.CeremonialDish
{
    public class CeremonialBanner : ModTile
    {
        private static bool HasGoop(int i, int j)
        {
            Point16 tileOrigin = RadianceUtils.GetTileOrigin(i, j);
            Tile tile = Framing.GetTileSafely(tileOrigin.X, tileOrigin.Y);
            return tile.TileType == ModContent.TileType<CeremonialBanner>() && tile.TileFrameX == 0 && tile.TileFrameY == 72;
        }
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Ceremonial Banner");
            AddMapEntry(new Color(255, 211, 21), name);

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);

            RadianceSets.DrawWindSwayTiles[Type] = true;

            TileObjectData.addTile(Type);

            DustType = -1;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameY == 0 && tile.TileFrameX == 0)
            {
                VineSwaySystem.AddToPoints(new Point(i, j));
            }
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameY == 72 && tile.TileFrameX == 0)
            {
                Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CeremonialDish/CeremonialBannerGoop").Value;
                spriteBatch.Draw(texture, new Vector2(i, j) * 16, null, Color.White * 0.8f, 0, texture.Size() / 2, 1, SpriteEffects.None, 0);
            }
        }
    }

    public class CeremonialBannerItem : BaseTileItem
    {
        public CeremonialBannerItem() : base("CeremonialBannerItem", "Ceremonial Banner", "", "CeremonialBanner", 3, Item.sellPrice(0, 0, 1, 0)) { }
    }
}