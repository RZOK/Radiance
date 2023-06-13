using Radiance.Content.Particles;
using System.Collections.Generic;

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
            Point spotPoint = (Center / 16).ToPoint();
            int leftBound = Utils.Clamp(spotPoint.X - 30, clampBox.Left, clampBox.Right);
            int rightBound = Utils.Clamp(spotPoint.X + 30, clampBox.Left, clampBox.Right);
            int topBound = Utils.Clamp(spotPoint.Y - 30, clampBox.Top, clampBox.Bottom);
            int bottomBound = Utils.Clamp(spotPoint.Y + 30, clampBox.Top, clampBox.Bottom);
            for (int i = leftBound; i <= rightBound; i++)
            {
                for (int j = topBound; j <= bottomBound; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile != null && tile.HasTile && Main.IsTileSpelunkable(i, j))
                    {
                        Vector2 vector = new Vector2(spotPoint.X - i, spotPoint.Y - j);
                        if (vector.Length() <= 30f)
                        {
                            Point point = new Point(i, j);
                            Vector2 position = point.ToVector2() * 16;
                            if (tilesChecked.Add(point) && Main.rand.NextBool(4))
                            {
                                if (tile.TileType == 21 || tile.TileType == 467 || tile.TileType == 12)
                                {
                                    if(tile.TileType == 12 && tile.TileFrameX == 0)
                                    {
                                        ParticleSystem.AddParticle(new TreasureSparkle(position + Vector2.One + Main.rand.NextVector2Square(0, 32), -Vector2.UnitY * Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(600, 1200), 50, Main.rand.NextFloat(0.35f, 0.65f), new Color(255, 100, 168)));
                                        continue;
                                    }
                                    if ((tile.TileFrameY == 0 || tile.TileFrameY == 36 || tile.TileFrameY == 72) && tile.TileFrameX / 18 % 2 == 0)
                                    {
                                        Color color = new Color(255, 236, 173);
                                        if (tile.TileType == 21)
                                        {
                                            switch (tile.TileFrameX / 18)
                                            {
                                                case 0:
                                                    color = new Color(242, 135, 78); //wood
                                                    break;

                                                case 6:
                                                case 8:
                                                    color = new Color(198, 78, 242); //shadow
                                                    break;

                                                case 16:
                                                case 20:
                                                case 36:
                                                case 46:
                                                    color = new Color(152, 242, 78); //mahogany + jungle + dungeon jungle
                                                    break;

                                                case 22:
                                                case 44:
                                                case 54:
                                                    color = new Color(78, 223, 242); //ice + dungeon ice
                                                    break;

                                                case 24:
                                                    color = new Color(182, 250, 105); //living
                                                    break;

                                                case 30:
                                                    color = new Color(175, 245, 174); //web
                                                    break;

                                                case 32:
                                                    color = new Color(247, 142, 106); //lihizahrd
                                                    break;

                                                case 34:
                                                    color = new Color(106, 134, 247); //water
                                                    break;

                                                case 38:
                                                case 48:
                                                    color = new Color(179, 106, 247); //dungeon corruption
                                                    break;

                                                case 40:
                                                case 50:
                                                    color = new Color(255, 0, 68); //dungeon crimson
                                                    break;

                                                case 42:
                                                case 52:
                                                    color = new Color(0, 255, 242); //dungeon hallow
                                                    break;

                                                case 64:
                                                    color = new Color(46, 105, 255); //glowing mushroom
                                                    break;

                                                case 100:
                                                    color = new Color(73, 71, 186); //granite
                                                    break;

                                                case 102:
                                                    color = new Color(255, 230, 191); //marble
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (tile.TileFrameX / 18)
                                            {
                                                case 20:
                                                case 24:
                                                case 26:
                                                    color = new Color(255, 179, 128); //sandstone + dungeon desert
                                                    break;
                                            }
                                        }
                                        ParticleSystem.AddParticle(new TreasureSparkle(position + Vector2.One + Main.rand.NextVector2Square(0, 32), -Vector2.UnitY * Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(600, 1200), 50, Main.rand.NextFloat(0.35f, 0.65f), color));
                                    }
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