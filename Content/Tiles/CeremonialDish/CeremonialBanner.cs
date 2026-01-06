using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Materials;
using Radiance.Content.Particles;
using Radiance.Core.Config;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.CeremonialDish
{
    public class CeremonialBanner : ModTile, IGlowmaskTile
    {
        internal static bool HasGoop(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return tile.TileType == ModContent.TileType<CeremonialBanner>() && tile.TileFrameY >= 72;
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
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.StyleHorizontal = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            RegisterItemDrop(ModContent.ItemType<CeremonialBannerItem>());
            RadianceSets.DrawWindSwayTiles[Type] = true;
            TileObjectData.addTile(Type);
            DustType = -1;
        }

        public override bool RightClick(int i, int j)
        {
            if (HasGoop(i, j))
            {
                Point clickedTilePoint = new Point(i, j);
                Tile clickedTile = Framing.GetTileSafely(clickedTilePoint);
                TileObjectData data = TileObjectData.GetTileData(clickedTile);
                Point origin = clickedTilePoint.GetTileOrigin();
                for (int k = 0; k < data.Width; k++)
                {
                    for (int l = 0; l < data.Height; l++)
                    {
                        Framing.GetTileSafely(origin.X + k, origin.Y + l).TileFrameY -= (short)(data.Height * 18);
                    }
                }
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int soulCount = Main.rand.Next(2, 4);
                    for (int h = 0; h < soulCount; h++)
                    {
                        // need to sync this
                        Item.NewItem(new EntitySource_TileInteraction(Main.LocalPlayer, i, j), new Vector2(origin.X + 1, origin.Y + 2) * 16 + Main.rand.NextVector2Circular(14, 24), 0, 0, ModContent.ItemType<EssenceOfFlight>());
                    }
                    SoundEngine.PlaySound(SoundID.Item177, clickedTilePoint.ToWorldCoordinates());
                    return true;
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            if (HasGoop(i, j))
                Main.LocalPlayer.SetCursorItem(ModContent.ItemType<EssenceOfFlight>());
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => HasGoop(i, j);

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (HasGoop(i, j))
            {
                if (Main.rand.NextBool(600))
                {
                    Point clickedTilePoint = new Point(i, j);
                    Point origin = clickedTilePoint.GetTileOrigin();
                    WorldParticleSystem.system.AddParticle(new SoulofFlightJuice(new Vector2(origin.X + 1, origin.Y + 2) * 16 + Main.rand.NextVector2Circular(10, 20), 600));
                }
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            TileObjectData data = TileObjectData.GetTileData(tile);
            if (ModContent.GetInstance<RadianceConfig>().EnableVineSway && Main.SettingsEnabled_TilesSwayInWind)
            {
                if (tile.TileFrameY % (data.Height * 18) == 0 && tile.TileFrameX == 0)
                    VineSwaySystem.AddToPoints(new Point(i, j));
                return false;
            }
            return true;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if ((!ModContent.GetInstance<RadianceConfig>().EnableVineSway || !Main.SettingsEnabled_TilesSwayInWind) && HasGoop(i, j))
                Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>("Radiance/Content/Tiles/CeremonialDish/CeremonialBannerGoop").Value, new Vector2(i, j) * 16 - Main.screenPosition + TileDrawingZero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            if (HasGoop(i, j))
                yield return new Item(ModContent.ItemType<EssenceOfFlight>(), Main.rand.Next(2, 4));

            yield return new Item(ModContent.ItemType<CeremonialBannerItem>());
        }

        public bool GlowmaskInfo(int i, int j, out Texture2D tex, out Color color)
        {
            tex = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CeremonialDish/CeremonialBannerGoop").Value;
            color = Color.White;
            return HasGoop(i, j);
        }
    }

    public class CeremonialBannerItem : BaseTileItem
    {
        public CeremonialBannerItem() : base("CeremonialBannerItem", "Ceremonial Banner", "", "CeremonialBanner", 3, Item.sellPrice(0, 0, 1, 0))
        {
        }
    }
}