﻿using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class DynamicFixture : BaseRelay
    {
        public override void SetExtraStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Item27;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Dynamic Fixture");
            AddMapEntry(new Color(245, 103, 122), name);
            RegisterItemDrop(ModContent.ItemType<DynamicFixture_Item>());
        }
        public override void HitWire(int i, int j)
        {
            Point tilePoint = new Point(i, j);
            Tile tile = Framing.GetTileSafely(tilePoint);
            TileObjectData data = TileObjectData.GetTileData(tile);
            Point origin = tilePoint.GetTileOrigin();
            for (int k = 0; k < data.Width; k++)
            {
                for (int l = 0; l < data.Height; l++)
                {
                    Wiring.SkipWire(origin.X + k, origin.Y + l);
                    if (tile.TileFrameY <= 36)
                        Framing.GetTileSafely(origin.X + k, origin.Y + l).TileFrameY += (short)(data.Height * 18);
                    else
                        Framing.GetTileSafely(origin.X + k, origin.Y + l).TileFrameY -= (short)(data.Height * 18);
                }
            }
            RadianceTransferSystem.shouldUpdateRays = true;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            return true;
        }
        public override void PostDrawExtra(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            //TileObjectData data = TileObjectData.GetTileData(tile);

            Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/DynamicFixture_Glow").Value;
            Vector2 offset = new Vector2(8, -3);
            Vector2 mainPosition = new Vector2(i, j) * 16f + TileDrawingZero;
            if (tile.TileFrameX == 0 && tile.TileFrameY == 18)
                spriteBatch.Draw(glowTexture, mainPosition + offset - Main.screenPosition, null, Color.White * 0.3f, 0, glowTexture.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
        public override bool TileIsInput(Tile tile) => tile.HasTile && tile.TileFrameY % 36 == 18;
        public override bool TileIsOutput(Tile tile) => tile.HasTile && tile.TileFrameY % 36 == 0;
        public override bool Active(Tile tile) => tile.TileFrameY <= 36;

    } 
    public class DynamicFixture_Item : BaseTileItem
    {
        public DynamicFixture_Item() : base("DynamicFixture_Item", "Dynamic Fixture", "Links rays together\nCan be toggled with wires", "DynamicFixture", 1, Item.sellPrice(0, 0, 1, 0), ItemRarityID.Green) { }

    }
}