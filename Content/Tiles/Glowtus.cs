using Radiance.Content.Items.BaseItems;
using System.Collections.Generic;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public enum PlantStage : byte
    {
        Planted,
        Grown,
        Blooming
    }

    public class Glowtus : ModTile
    {
        private const int FrameWidth = 18;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileLighted[Type] = true;

            TileID.Sets.ReplaceTileBreakUp[Type] = true;
            TileID.Sets.IgnoredInHouseScore[Type] = true;
            TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Glowtus");
            AddMapEntry(new Color(241, 226, 172), name);

            TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
            TileObjectData.newTile.CoordinateHeights = new int[] { 24 };
            TileObjectData.newTile.AnchorValidTiles = new int[] {
                TileID.Grass,
                TileID.HallowedGrass,
                TileID.GolfGrass,
                TileID.GolfGrassHallowed
            };
            TileObjectData.newTile.AnchorAlternateTiles = new int[] {
                TileID.ClayPot,
                TileID.PlanterBox
            };
            TileObjectData.addTile(Type);

            HitSound = SoundID.Grass;
            DustType = DustID.Grass;
        }
        public override bool CanPlace(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.HasTile)
            {
                int tileType = tile.TileType;
                Main.NewText(Type);
                Main.NewText(tileType);
                if (tileType == Type)
                {
                    return GetStage(i, j) == PlantStage.Blooming;
                }
                else
                {
                    if (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType] || tileType == TileID.WaterDrip || tileType == TileID.LavaDrip || tileType == TileID.HoneyDrip || tileType == TileID.SandDrip)
                    {
                        bool foliageGrass = tileType == TileID.Plants || tileType == TileID.Plants2;
                        bool moddedFoliage = tileType >= TileID.Count && (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType]);
                        bool harvestableVanillaHerb = Main.tileAlch[tileType] && WorldGen.IsHarvestableHerbWithSeed(tileType, tile.TileFrameX / 18);
                        if (foliageGrass || moddedFoliage || harvestableVanillaHerb)
                        {
                            WorldGen.KillTile(i, j);
                            if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
                            return true;
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            if (i % 2 == 0)
                spriteEffects = SpriteEffects.FlipHorizontally;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            PlantStage stage = GetStage(i, j);
            if (stage != PlantStage.Planted) 
            {
                Point point = new Point(i, j);
                float randomNumber = point.GetSmoothTileRNG();
                Tile tile = Framing.GetTileSafely(i, j);
                if ((Main.dayTime && randomNumber < 0.5f) || (!Main.dayTime && randomNumber >= 0.5f))
                {
                    if (stage == PlantStage.Grown)
                        tile.TileFrameX = FrameWidth * 2;
                }
                else if (stage == PlantStage.Blooming)
                    tile.TileFrameX = FrameWidth;
            }
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            PlantStage stage = GetStage(i, j);
            
            if (stage == PlantStage.Blooming)
            {
                if (Main.rand.NextBool(20))
                {
                    int d = Dust.NewDust(new Vector2(i * 16, j * 16 - 4), 14, 14, DustID.TreasureSparkle);
                    Main.dust[d].velocity *= 0f;
                    Main.dust[d].noGravity = true;
                }
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            PlantStage stage = GetStage(i, j);
            float strength = 0.2f * Math.Clamp(Math.Abs(SineTiming(120)), 0.6f, 1f);
            if (stage == PlantStage.Blooming)
            {
                r = 1f * strength;
                g = 0.9f * strength;
                b = 0.8f * strength;
            }
            else
                r = g = b = 0;
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = -6;
        public override bool CanDrop(int i, int j) => GetStage(i, j) != PlantStage.Planted;

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            PlantStage stage = GetStage(i, j);

            Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
            Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];

            int herbItemType = ModContent.ItemType<GlowtusItem>();
            int herbItemStack = 1;
            int seedItemType = ModContent.ItemType<GlowtusSeeds>();
            int seedItemStack = 0;
            if (nearestPlayer.active && nearestPlayer.HeldItem.type == ItemID.StaffofRegrowth)
            {
                herbItemStack = Main.rand.Next(1, 3);
                seedItemStack = Main.rand.Next(1, 6);
            }
            else if (stage == PlantStage.Blooming)
                seedItemStack = Main.rand.Next(1, 4);
             

            List<Item> itemDrops = new List<Item>();
            if (herbItemType > 0 && herbItemStack > 0)
                itemDrops.Add(new Item(herbItemType, herbItemStack));

            if (seedItemType > 0 && seedItemStack > 0)
                itemDrops.Add(new Item(seedItemType, seedItemStack));

            return itemDrops;
        }

        public override bool IsTileSpelunkable(int i, int j) => GetStage(i, j) == PlantStage.Blooming;

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            PlantStage stage = GetStage(i, j);

            if (stage == PlantStage.Planted)
            {
                Point point = new Point(i, j);
                float randomNumber = point.GetSmoothTileRNG();
                if (WorldGen.genRand.NextBool(30) || (WorldGen.genRand.NextBool(30) && ((Main.dayTime && randomNumber < 0.5f) || (!Main.dayTime && randomNumber >= 0.5f))))
                {
                    tile.TileFrameX = FrameWidth;
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendTileSquare(-1, i, j, 1);
                    }
                }
            }
        }

        private static PlantStage GetStage(int i, int j) => (PlantStage)(Framing.GetTileSafely(i, j).TileFrameX / FrameWidth);
    }

    public class GlowtusSeeds : BaseTileItem
    {
        public GlowtusSeeds() : base("GlowtusSeeds", "Glowtus Seeds", "", "Glowtus", 25, Item.sellPrice(0, 0, 0, 16)) { }
    }

    public class GlowtusItem : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glowtus");
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
            Item.width = 14;
            Item.height = 14;
            Item.value = Item.sellPrice(0, 0, 0, 25);
        }
    }
}