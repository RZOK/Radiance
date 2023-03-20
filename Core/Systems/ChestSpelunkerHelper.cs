using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Radiance.Content.Particles;

namespace Radiance.Core.Systems
{
    public class ChestSpelunkerHelper : ModSystem
    {
        public static ChestSpelunkerHelper Instance;

        private HashSet<Vector2> positionsChecked = new HashSet<Vector2>();

        private HashSet<Point> tilesChecked = new HashSet<Point>();

        private Rectangle clampBox;

        public ChestSpelunkerHelper()
        {
            Instance = this;
        }
        public override void Unload()
        {
            Instance = null;
        }
        public override void PreUpdateProjectiles()
        {
            clampBox = new Rectangle(2, 2, Main.maxTilesX - 2, Main.maxTilesY - 2);
            if (Main.GameUpdateCount % 10 == 0)
            {
                tilesChecked.Clear();
                positionsChecked.Clear();
            }
        }

        public void AddSpotToCheck(Vector2 spot)
        {
            if (positionsChecked.Add(spot))
                CheckSpot(spot);
        }

        private void CheckSpot(Vector2 Center)
        {
            int num = (int)Center.X / 16;
            int num2 = (int)Center.Y / 16;
            int num3 = Utils.Clamp(num - 30, clampBox.Left, clampBox.Right);
            int num4 = Utils.Clamp(num + 30, clampBox.Left, clampBox.Right);
            int num5 = Utils.Clamp(num2 - 30, clampBox.Top, clampBox.Bottom);
            int num6 = Utils.Clamp(num2 + 30, clampBox.Top, clampBox.Bottom);
            Point point;
            Vector2 position;
            for (int i = num3; i <= num4; i++)
            {
                for (int j = num5; j <= num6; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile != null && tile.HasTile && Main.IsTileSpelunkable(i, j))
                    {
                        Vector2 vector = new Vector2(num - i, num2 - j);
                        if (vector.Length() <= 30f)
                        {
                            point.X = i;
                            point.Y = j;
                            position = point.ToVector2() * 16;
                            if (tilesChecked.Add(point) && Main.rand.NextBool(4))
                            {
                                if (tile.TileType == 21 || tile.TileType == 467)
                                {
                                    Color color = new Color(255, 236, 173);
                                    if (tile.TileType == 21)
                                    {
                                        switch (tile.TileFrameX / 18)
                                        {
                                            case 0:
                                            case 1:
                                                color = new Color(242, 135, 78); //wood
                                                break;

                                            case 6:
                                            case 7:
                                            case 9:
                                            case 8:
                                                color = new Color(198, 78, 242); //shadow
                                                break;

                                            case 16:
                                            case 17:
                                            case 20:
                                            case 21:
                                            case 36:
                                            case 37:
                                            case 46:
                                            case 47:
                                                color = new Color(152, 242, 78); //mahogany + jungle + dungeon jungle
                                                break;

                                            case 22:
                                            case 23:
                                            case 44:
                                            case 45:
                                            case 54:
                                            case 55:
                                                color = new Color(78, 223, 242); //ice + dungeon ice
                                                break;

                                            case 24:
                                            case 25:
                                                color = new Color(182, 250, 105); //living
                                                break;

                                            case 30:
                                            case 31:
                                                color = new Color(175, 245, 174); //web
                                                break;

                                            case 32:
                                            case 33:
                                                color = new Color(247, 142, 106); //lihizahrd
                                                break;

                                            case 34:
                                            case 35:
                                                color = new Color(106, 134, 247); //water
                                                break;

                                            case 38:
                                            case 39:
                                            case 48:
                                            case 49:
                                                color = new Color(179, 106, 247); //dungeon corruption
                                                break;

                                            case 40:
                                            case 41:
                                            case 50:
                                            case 51:
                                                color = new Color(255, 0, 68); //dungeon crimson
                                                break;

                                            case 42:
                                            case 43:
                                            case 52:
                                            case 53:
                                                color = new Color(0, 255, 242); //dungeon hallow
                                                break;

                                            case 64:
                                            case 65:
                                                color = new Color(46, 105, 255); //glowing mushroom
                                                break;

                                            case 100:
                                            case 101:
                                                color = new Color(73, 71, 186); //granite
                                                break;

                                            case 102:
                                            case 103:
                                                color = new Color(255, 230, 191); //marble
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch(tile.TileFrameX / 18)
                                        {
                                            case 20:
                                            case 21:
                                            case 24:
                                            case 25:
                                            case 26:    
                                            case 27:
                                                color = new Color(255, 179, 128); //sandstone = dungeon desert
                                                break;
                                        }
                                    }

                                    if (Main.rand.NextBool(4))
                                        ParticleSystem.AddParticle(new Sparkle(position + Vector2.One * 8, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 3), 120, 100, color));
                                }
                                else
                                {
                                    Dust dust = Dust.NewDustDirect(position, 16, 16, DustID.TreasureSparkle, 0f, 0f, 150, default, 0.3f);
                                    dust.fadeIn = 0.75f;
                                    dust.velocity *= 0.1f;
                                    dust.noLight = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
