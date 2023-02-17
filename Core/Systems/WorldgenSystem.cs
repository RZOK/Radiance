using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Radiance.Content.Tiles;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.IO;

namespace Radiance.Core.Systems
{
    public class WorldgenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int HerbIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Herbs"));
            if (HerbIndex == -1)
                return;

            tasks.Insert(HerbIndex + 1, new GlowtusPass("Herbs", 237.4298f));
        }
        public override void PostUpdateWorld()
        {
            double tileX = (double)(0.00003f * (float)WorldGen.GetWorldUpdateRate());
            double num4 = Main.maxTilesX * Main.maxTilesY * tileX;
            int num5 = 151;
            int num6 = (int)MathHelper.Lerp(num5, num5 * 2.8f, Utils.Clamp((float)(Main.maxTilesX / 4200.0 - 1.0), 0, 1));
            int num7 = 0;
            while (num7 < num4)
            {
                if (Main.rand.NextBool(num6 * 200))
                    PlantGlowtus();
                num7++;
            }
        }
        public static void PlantGlowtus()
        {
            int tileX = WorldGen.genRand.Next(20, Main.maxTilesX - 20);
            int tileY = 0;
            /*if (Main.remixWorld)
            {
                tileY = WorldGen.genRand.Next(20, Main.maxTilesY - 20);
            }
            else*/
            while (tileY < Main.worldSurface / 2 && !Main.tile[tileX, tileY].HasTile)
            {
                tileY++;
            }
            if (Main.tile[tileX, tileY].HasUnactuatedTile && !Main.tile[tileX, tileY - 1].HasTile && Main.tile[tileX, tileY - 1].LiquidType == 0)
            {
                int maxHerbsInArea = 5;
                int matchingHerbsInArea = 0;
                int range = (int)(15 * (Main.maxTilesX / 4200.0));

                int search1 = Utils.Clamp(tileX - range, 4, Main.maxTilesX - 4);
                int search2 = Utils.Clamp(tileX + range, 4, Main.maxTilesX - 4);
                int search3 = Utils.Clamp(tileY - range, 4, Main.maxTilesY - 4);
                int search4 = Utils.Clamp(tileY + range, 4, Main.maxTilesY - 4);
                for (int i = search1; i <= search2; i++)
                {
                    for (int j = search3; j <= search4; j++)
                    {
                        if (Main.tile[i, j].TileType == ModContent.TileType<Glowtus>())
                            matchingHerbsInArea++;
                    }
                }
                if (matchingHerbsInArea < maxHerbsInArea)
                {
                    if (Main.tile[tileX, tileY].TileType == 2 || Main.tile[tileX, tileY].TileType == 109)
                    {
                        PlaceGlowtus(tileX, tileY - 1);
                    }
                    if (Main.tile[tileX, tileY - 1].HasTile && Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, tileX, tileY - 1, TileChangeType.None);
                    }
                }
            }
        }
        public static bool PlaceGlowtus(int i, int j)
        {
            //if (Main.tile[i, j] == null)
            //{
            //    Main.tile[i, j] = new Tile();
            //}
            //if (Main.tile[i, j + 1] == null)
            //{
            //    Main.tile[i, j + 1] = new Tile();
            //}

            if (!Main.tile[i, j].HasTile && Main.tile[i, j + 1].HasUnactuatedTile && !Main.tile[i, j + 1].IsHalfBlock && Main.tile[i, j + 1].Slope == SlopeType.Solid)
            {
                //if (Main.tile[i, j + 1].TileType != 2 && Main.tile[i, j + 1].TileType != 477 && Main.tile[i, j + 1].TileType != 492 && Main.tile[i, j + 1].TileType != 78 && Main.tile[i, j + 1].TileType != 380 && Main.tile[i, j + 1].TileType != 109)
                //{
                WorldGen.Place1x1(i, j, ModContent.TileType<Glowtus>());
                return true;
                //}
            }
            return false;
        }
    }
    public class GlowtusPass : GenPass
    {
        public GlowtusPass(string name, float loadWeight) : base(name, loadWeight)
        {

        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Growing magical herbs";
            for (int k = 0; k < Main.maxTilesX / 4; k++)
            {
                WorldgenSystem.PlantGlowtus();
            }
        }
    }
}