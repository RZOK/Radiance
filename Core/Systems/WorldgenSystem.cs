﻿using Radiance.Content.Tiles;
using Terraria.WorldBuilding;

namespace Radiance.Core.Systems
{
    public class WorldgenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int HerbIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Herbs"));
            if (HerbIndex == -1)
                return;

            tasks.Insert(HerbIndex + 1, new GlowstalkPass("Herbs", 237.4298f));
        }

        public override void PostUpdateWorld()
        {
            double tileX = (double)(0.00003f * (float)WorldGen.GetWorldUpdateRate());
            double num4 = Main.maxTilesX * Main.maxTilesY * tileX;
            int num5 = 151;
            int num6 = (int)Lerp(num5, num5 * 2.8f, Utils.Clamp((float)(Main.maxTilesX / 4200.0 - 1.0), 0, 1));
            int num7 = 0;
            while (num7 < num4)
            {
                if (Main.rand.NextBool(num6 * 200))
                    PlantGlowstalk();
                num7++;
            }
        }

        public static void PlantGlowstalk()
        {
            int tileX = WorldGen.genRand.Next(20, Main.maxTilesX - 20);
            int tileY = 0;
            if (Main.remixWorld)
            {
                tileY = WorldGen.genRand.Next(20, Main.maxTilesY - 20);
            }
            else
            {
                while (tileY < Main.worldSurface / 2 && !Main.tile[tileX, tileY].HasTile)
                {
                    tileY++;
                }
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
                        if (Main.tile[i, j].TileType == ModContent.TileType<Glowstalk>())
                            matchingHerbsInArea++;
                    }
                }
                if (matchingHerbsInArea < maxHerbsInArea)
                {
                    if (Main.tile[tileX, tileY].TileType == 2 || Main.tile[tileX, tileY].TileType == 109)
                    {
                        PlaceGlowstalk(tileX, tileY - 1);
                    }
                    if (Main.tile[tileX, tileY - 1].HasTile && Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, tileX, tileY - 1, TileChangeType.None);
                    }
                }
            }
        }

        public static bool PlaceGlowstalk(int i, int j)
        {
            if (!Main.tile[i, j].HasTile && Main.tile[i, j + 1].HasUnactuatedTile && !Main.tile[i, j + 1].IsHalfBlock && Main.tile[i, j + 1].Slope == SlopeType.Solid)
            {
                WorldGen.Place1x1(i, j, ModContent.TileType<Glowstalk>());
                return true;
            }
            return false;
        }
    }

    public class GlowstalkPass : GenPass
    {
        public GlowstalkPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Growing magical herbs";
            for (int k = 0; k < Main.maxTilesX / 4; k++)
            {
                WorldgenSystem.PlantGlowstalk();
            }
        }
    }
}