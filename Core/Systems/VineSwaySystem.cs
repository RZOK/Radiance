using System.Reflection;
using Terraria.GameContent.Drawing;
using Terraria.ObjectData;
using Radiance.Core.Config;

namespace Radiance.Core.Systems
{
    public class VineSwaySystem : ModSystem
    {
        private static List<Point> placesToDraw;
        private TileDrawing TileDrawer => Main.instance.TilesRenderer;

        private Func<int, int, int, int, int, float, int, bool, float> GetHighestWindGridPushComplex;
        private delegate void DrawAnimatedTileAdjustForVisionChangersDelegate(int i, int j, Tile tile, ushort type, short frameX, short frameY, ref Color tileLight, bool canDoDust);
        private DrawAnimatedTileAdjustForVisionChangersDelegate DrawAnimatedTileAdjustForVisionChangers;
        private Func<Tile, bool> IsVisible;
        private Func<int, int, Tile, ushort, short, short, Color, Color> DrawTilesGetLightOverride;
        private FieldInfo sunflowerWindCounterField;
        private double sunflowerWindCounter => (double)sunflowerWindCounterField.GetValue(TileDrawer);

        public override void Load()
        {
            if (Main.dedServ)
                return;

            GetHighestWindGridPushComplex = (Func<int, int, int, int, int, float, int, bool, float>)Delegate.CreateDelegate(typeof(Func<int, int, int, int, int, float, int, bool, float>), TileDrawer, TileDrawer.ReflectionGetMethod("GetHighestWindGridPushComplex", BindingFlags.Instance | BindingFlags.NonPublic));
            DrawAnimatedTileAdjustForVisionChangers = (DrawAnimatedTileAdjustForVisionChangersDelegate)Delegate.CreateDelegate(typeof(DrawAnimatedTileAdjustForVisionChangersDelegate), TileDrawer, TileDrawer.ReflectionGetMethod("DrawAnimatedTile_AdjustForVisionChangers", BindingFlags.Instance | BindingFlags.NonPublic));
            IsVisible = (Func<Tile, bool>)Delegate.CreateDelegate(typeof(Func<Tile, bool>), TileDrawer, TileDrawer.ReflectionGetMethod("IsVisible", BindingFlags.Instance | BindingFlags.NonPublic));
            DrawTilesGetLightOverride = (Func<int, int, Tile, ushort, short, short, Color, Color>)Delegate.CreateDelegate(typeof(Func<int, int, Tile, ushort, short, short, Color, Color>), TileDrawer, TileDrawer.ReflectionGetMethod("DrawTiles_GetLightOverride", BindingFlags.Instance | BindingFlags.NonPublic));
            sunflowerWindCounterField = TileDrawer.ReflectionGrabField("_sunflowerWindCounter", BindingFlags.Instance | BindingFlags.NonPublic);

            On_TileDrawing.DrawMultiTileVines += PostDrawMultiTileVinesHook;
            placesToDraw = new List<Point>();
        }

        private void PostDrawMultiTileVinesHook(On_TileDrawing.orig_DrawMultiTileVines orig, TileDrawing self)
        {
            orig(self);
            PostDrawMultitileVines();
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            GetHighestWindGridPushComplex = null;
            On_TileDrawing.DrawMultiTileVines -= PostDrawMultiTileVinesHook;
            placesToDraw = null;
        }
        public static bool AddToPoints(Point point)
        {
            bool flag = false;
            if(!placesToDraw.Contains(point))
            {
                placesToDraw.Add(point);
                flag = true;
            }
            return flag;
        }
        private void PostDrawMultitileVines()
        {
            if (ModContent.GetInstance<RadianceConfig>().EnableVineSway && Main.SettingsEnabled_TilesSwayInWind)
            {
                List<Point> pointsToRemove = new();
                foreach (Point location in placesToDraw)
                {
                    Tile tile = Framing.GetTileSafely(location);
                    if (tile.HasTile && RadianceSets.DrawWindSwayTiles[tile.TileType])
                        DrawSwaying(tile, location.X, location.Y);
                    else
                        pointsToRemove.Add(location);
                }
                placesToDraw.RemoveAll(pointsToRemove.Contains);
            }
            else
                placesToDraw.Clear();
        }
        public void DrawSwaying(Tile tile, int tileX, int tileY)
        {
            Vector2 screenPosition = Main.Camera.UnscaledPosition;
            int type = tile.TileType;
            int width = TileObjectData.GetTileData(tile).Width;
            int height = TileObjectData.GetTileData(tile).Height;

            int totalPushTime = 60;
            float pushForcePerFrame = 1.26f;
            float highestWindGridPushComplex = GetHighestWindGridPushComplex(tileX, tileY, width, height, totalPushTime, pushForcePerFrame, 3, true);
            
            float windCycle = TileDrawer.GetWindCycle(tileX, tileY, sunflowerWindCounter);
            windCycle += highestWindGridPushComplex;
            
            Vector2 originTilePosition = new Vector2(tileX * 16 + width * 8, tileY * 16 - 2) - screenPosition;

            if (!WorldGen.InAPlaceWithWind(tileX, tileY, width, height))
                windCycle = 0;

            for (int i = tileX; i < tileX + width; i++)
            {
                for (int j = tileY; j < tileY + height; j++)
                {
                    Tile tile2 = Main.tile[i, j];
                    ushort type2 = tile2.TileType;
                    if (type2 != type || !IsVisible(tile2))
                        continue;
                    
                    short tileFrameX = tile2.TileFrameX;
                    short tileFrameY = tile2.TileFrameY;
                    float num = (float)(j - tileY + 1) / height;
                    if (num == 0f)
                        num = 0.1f;

                    TileDrawer.GetTileDrawData(i, j, tile2, type2, ref tileFrameX, ref tileFrameY, out int tileWidth, out int tileHeight, out int tileTop, out int halfBrickHeight, out int addFrX, out int addFrY, out SpriteEffects tileSpriteEffect, out var _, out var _, out var _);
                    Color tileLight = Lighting.GetColor(i, j);
                    DrawAnimatedTileAdjustForVisionChangers(i, j, tile2, tile2.TileType, tileFrameX, tileFrameY, ref tileLight, Main.rand.NextBool(4));
                    tileLight = DrawTilesGetLightOverride(i, j, tile2, tile2.TileType, tileFrameX, tileFrameY, tileLight);

                    Vector2 lowerTilePosition = new Vector2(i * 16, j * 16 + tileTop - 2) - screenPosition;
                    Vector2 windModifier = new Vector2(windCycle, Math.Abs(windCycle) * -4f * num);
                    Vector2 lowerTileDifference = originTilePosition - lowerTilePosition;
                    Texture2D tileDrawTexture = TileDrawer.GetTileDrawTexture(tile2, i, j);
                    if (tileDrawTexture != null)
                    {
                        Vector2 vector6 = originTilePosition + Vector2.UnitY * windModifier;
                        Rectangle rectangle = new Rectangle(tileFrameX + addFrX, tileFrameY + addFrY, tileWidth, tileHeight - halfBrickHeight);
                        float rotation = windCycle * -0.15f * num;
                        Main.spriteBatch.Draw(tileDrawTexture, vector6, rectangle, tileLight, rotation, lowerTileDifference, 1f, tileSpriteEffect, 0f);
                        if (TileLoader.GetTile(type) is IGlowmaskTile glowmask && glowmask.ShouldDisplayGlowmask(tileX, tileY) && glowmask.glowmaskTexture != string.Empty )
                        {
                            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(glowmask.glowmaskTexture).Value, vector6, rectangle, glowmask.glowmaskColor, rotation, lowerTileDifference, 1f, tileSpriteEffect, 0f);
                        }
                    }
                }
            }
        }
    }
}
