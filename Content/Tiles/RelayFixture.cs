using Radiance.Content.Items.BaseItems;
using Radiance.Core.Systems;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class RelayFixture : BaseRelay
    {
        public override void SetExtraStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            TileObjectData.newTile.DrawFlipHorizontal = true;
            HitSound = SoundID.Item27;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Relay Fixture");
            AddMapEntry(new Color(241, 188, 91), name);
        }
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            if (i % 2 == 0)
                spriteEffects = SpriteEffects.FlipHorizontally;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            return true;
        }
        public override void PostDrawExtra(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/RelayFixture_Glow").Value;
            Vector2 offset = new Vector2(8, -5);
            Vector2 mainPosition = new Vector2(i, j) * 16f + TileDrawingZero;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (i % 2 == 0)
                spriteEffects = SpriteEffects.FlipHorizontally;

            if (tile.TileFrameX == 0 && tile.TileFrameY == 18)
                spriteBatch.Draw(glowTexture, mainPosition + offset - Main.screenPosition, null, Color.White * 0.3f, 0, glowTexture.Size() / 2f, 1f, spriteEffects, 0);
        }
        public override bool TileIsInput(Tile tile) => tile.HasTile && tile.TileFrameY == 18;
        public override bool TileIsOutput(Tile tile) => tile.HasTile && tile.TileFrameY == 0;

    } 
    public class RelayFixture_Item : BaseTileItem
    {
        public RelayFixture_Item() : base("RelayFixture_Item", "Relay Fixture", "Links rays together", "RelayFixture", 1, Item.sellPrice(0, 0, 1, 0), ItemRarityID.Blue) { }


    }
}