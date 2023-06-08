using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core;
using Radiance.Core.Config;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Radiance.Utilities;
using Steamworks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles.CeremonialDish
{
    public class CeremonialBanner : ModTile, IGlowmaskTile
    {
        public Color glowmaskColor => Color.White;
        public string glowmaskTexture { get; set; }

        public override void Load()
        {
            glowmaskTexture = "Radiance/Content/Tiles/CeremonialDish/CeremonialBannerGoop";
        }
        private static bool HasGoop(int i, int j)
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
            if(HasGoop(i, j))
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
                        Item.NewItem(new EntitySource_TileInteraction(Main.LocalPlayer, origin.X, origin.Y), (origin.X + k) * 16, (origin.Y + l) * 16, 16, 16, ItemID.SoulofFlight);
                    }
                }
                SoundEngine.PlaySound(SoundID.Item177, clickedTilePoint.ToWorldCoordinates());
                return true;
            }
            return false;
        }
        public override void MouseOver(int i, int j)
        {
            if(HasGoop(i, j))
                Main.LocalPlayer.SetCursorItem(ItemID.SoulofFlight);
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (HasGoop(i, j))
            {
                if (Main.rand.NextBool(600))
                {
                    Point clickedTilePoint = new Point(i, j);
                    Point origin = clickedTilePoint.GetTileOrigin(); //todo make tileorigin a point instead of point16 
                    ParticleSystem.AddParticle(new SoulofFlightJuice(new Vector2(origin.X + 1, origin.Y + 2) * 16 + Main.rand.NextVector2Circular(10, 20), 600));
                }
            }
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (ModContent.GetInstance<RadianceConfig>().EnableVineSway && Main.SettingsEnabled_TilesSwayInWind)
            {
                if(tile.TileFrameY == 0 && tile.TileFrameX == 0)
                    VineSwaySystem.AddToPoints(new Point(i, j));
                return false;
            }
            return true;
        }

        public bool ShouldDisplayGlowmask(int i, int j) => HasGoop(i, j);
    }

    public class CeremonialBannerItem : BaseTileItem
    {
        public CeremonialBannerItem() : base("CeremonialBannerItem", "Ceremonial Banner", "", "CeremonialBanner", 3, Item.sellPrice(0, 0, 1, 0)) { }
    }
}