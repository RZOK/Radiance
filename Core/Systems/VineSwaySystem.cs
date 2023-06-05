using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.GameContent.Drawing;
using Terraria;
using Radiance.Utilities;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria.ObjectData;
using Radiance.Core.Interfaces;
using Terraria.ID;
using System.Diagnostics;
using System.Linq;

namespace Radiance.Core.Systems
{
    public class VineSwaySystem : ModSystem
    {
        private static List<Point> placesToDraw;
        private TileDrawing tileDrawer => Main.instance.TilesRenderer;

        private Func<int, int, int, int, int, float, int, bool, float> GetHighestWindGridPushComplex;
        private delegate void DrawAnimatedTileAdjustForVisionChangersDelegate(int i, int j, Tile tile, ushort type, short frameX, short frameY, ref Color tileLight, bool canDoDust);
        private DrawAnimatedTileAdjustForVisionChangersDelegate DrawAnimatedTileAdjustForVisionChangers;
        private Func<Tile, bool> IsVisible;
        private Func<int, int, Tile, ushort, short, short, Color, Color> DrawTilesGetLightOverride;
        private FieldInfo sunflowerWindCounterField;
        private double sunflowerWindCounter => (double)sunflowerWindCounterField.GetValue(tileDrawer);

        public override void Load()
        {
            GetHighestWindGridPushComplex = (Func<int, int, int, int, int, float, int, bool, float>)Delegate.CreateDelegate(typeof(Func<int, int, int, int, int, float, int, bool, float>), tileDrawer, tileDrawer.ReflectionGetMethod("GetHighestWindGridPushComplex", BindingFlags.Instance | BindingFlags.NonPublic));
            DrawAnimatedTileAdjustForVisionChangers = (DrawAnimatedTileAdjustForVisionChangersDelegate)Delegate.CreateDelegate(typeof(DrawAnimatedTileAdjustForVisionChangersDelegate), tileDrawer, tileDrawer.ReflectionGetMethod("DrawAnimatedTile_AdjustForVisionChangers", BindingFlags.Instance | BindingFlags.NonPublic));
            IsVisible = (Func<Tile, bool>)Delegate.CreateDelegate(typeof(Func<Tile, bool>), tileDrawer, tileDrawer.ReflectionGetMethod("IsVisible", BindingFlags.Instance | BindingFlags.NonPublic));
            DrawTilesGetLightOverride = (Func<int, int, Tile, ushort, short, short, Color, Color>)Delegate.CreateDelegate(typeof(Func<int, int, Tile, ushort, short, short, Color, Color>), tileDrawer, tileDrawer.ReflectionGetMethod("DrawTiles_GetLightOverride", BindingFlags.Instance | BindingFlags.NonPublic));
            sunflowerWindCounterField = tileDrawer.ReflectionGrabField("_sunflowerWindCounter", BindingFlags.Instance | BindingFlags.NonPublic);

            IL_TileDrawing.PostDrawTiles += PostDrawMultiTileVinesIL;
            placesToDraw = new List<Point>();
        }
        public override void Unload()
        {
            GetHighestWindGridPushComplex = null;
            IL_TileDrawing.PostDrawTiles -= PostDrawMultiTileVinesIL;
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
        private void PostDrawMultiTileVinesIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchCall<TileDrawing>("DrawMultiTileVines")
                ))
            {
                RadianceUtils.LogIlError("Vine sway system", "Couldn't navigate to after DrawMultiTileVines()");
                return;
            }
            cursor.EmitDelegate(PostDrawMultitileVines);
        }
        public static List<double> Last5Seconds = new(); 
        private void PostDrawMultitileVines()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Point> pointsToRemove = new();
            foreach (Point location in placesToDraw)
            {
                Tile tile = Framing.GetTileSafely(location);
                if (tile.HasTile && RadianceSets.DrawWindSwayTiles[tile.TileType])
                    DrawSwaying(tile, location.X, location.Y);
                else
                    pointsToRemove.Add(location);
            }
            stopwatch.Stop();
            Last5Seconds.Add(stopwatch.Elapsed.TotalMilliseconds);
            if(Main.GameUpdateCount % 300 == 0)
            {
                Console.WriteLine(Last5Seconds.Sum() / 300);
                Last5Seconds.Clear();
            }
            placesToDraw.RemoveAll(pointsToRemove.Contains);
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
            
            float windCycle = tileDrawer.GetWindCycle(tileX, tileY, sunflowerWindCounter);
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

                    tileDrawer.GetTileDrawData(i, j, tile2, type2, ref tileFrameX, ref tileFrameY, out int tileWidth, out int tileHeight, out int tileTop, out int halfBrickHeight, out int addFrX, out int addFrY, out SpriteEffects tileSpriteEffect, out var _, out var _, out var _);
                    Color tileLight = Lighting.GetColor(i, j);
                    DrawAnimatedTileAdjustForVisionChangers(i, j, tile2, tile2.TileType, tileFrameX, tileFrameY, ref tileLight, Main.rand.NextBool(4));
                    tileLight = DrawTilesGetLightOverride(i, j, tile2, tile2.TileType, tileFrameX, tileFrameY, tileLight);

                    Vector2 lowerTilePosition = new Vector2(i * 16, j * 16 + tileTop - 2) - screenPosition;
                    Vector2 windModifier = new Vector2(windCycle, Math.Abs(windCycle) * -4f * num);
                    Vector2 lowerTileDifference = originTilePosition - lowerTilePosition;
                    Texture2D tileDrawTexture = tileDrawer.GetTileDrawTexture(tile2, i, j);
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
