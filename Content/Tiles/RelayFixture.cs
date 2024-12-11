using Radiance.Content.Items.BaseItems;
using Radiance.Core.Systems;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class RelayFixture : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Item27;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Relay Fixture");
            AddMapEntry(new Color(241, 188, 91), name);

            RadianceSets.RayAnchorTiles[Type] = true;

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<RelayFixture>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
        }
        private int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            //todo: network
            RadianceTransferSystem.shouldUpdateRays = true;
            return 0;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {

            return true;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/RelayFixture_Glow").Value;
            Vector2 offset = new Vector2(8, -3);
            Vector2 mainPosition = new Vector2(i, j) * 16f + TileDrawingZero;
            if (tile.TileFrameX == 0 && tile.TileFrameY == 18)
                spriteBatch.Draw(glowTexture, mainPosition + offset - Main.screenPosition, null, Color.White * 0.3f, 0, glowTexture.Size() / 2f, 1f, SpriteEffects.None, 0);
            if (Main.LocalPlayer.GetModPlayer<RadiancePlayer>().canSeeRays)
            {
                if (tile.TileFrameY == 0)
                    RadianceDrawing.DrawRadianceIOSlot(InterfaceDrawer.RadianceIOIndicatorMode.Output, mainPosition + Vector2.One * 8f);
                if (tile.TileFrameY == 18)
                    RadianceDrawing.DrawRadianceIOSlot(InterfaceDrawer.RadianceIOIndicatorMode.Input, mainPosition + Vector2.One * 8f);
            }
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            RadianceTransferSystem.shouldUpdateRays = true;
        }
        public static bool TileIsInput(Tile tile) => tile.HasTile && tile.TileType == ModContent.TileType<RelayFixture>() && tile.TileFrameY == 18;
        public static bool TileIsOutput(Tile tile) => tile.HasTile && tile.TileType == ModContent.TileType<RelayFixture>() && tile.TileFrameY == 0;

    } 
    public class RelayFixture_Item : BaseTileItem
    {
        public RelayFixture_Item() : base("RelayFixture_Item", "Relay Fixture", "Allows rays to be linked together", "RelayFixture", 1, Item.sellPrice(0, 0, 1, 0), ItemRarityID.Blue) { }
    }
}