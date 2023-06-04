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

namespace Radiance.Core.Systems
{
    public class VineSwaySystem : ModSystem //so much reflection :(
    {
        private static List<Point> placesToDraw;
        public TileDrawing tileDrawer => Main.instance.TilesRenderer;
        public double sunflowerWindCounter => (double)tileDrawer.ReflectionGrabValue("_sunflowerWindCounter", BindingFlags.Instance | BindingFlags.NonPublic);
        public override void Load()
        {
            IL_TileDrawing.PostDrawTiles += DrawSwayingTiles;
            placesToDraw = new List<Point>();
        }
        public override void Unload()
        {
            IL_TileDrawing.PostDrawTiles -= DrawSwayingTiles;
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
        private void DrawSwayingTiles(ILContext il)
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
            cursor.EmitDelegate(DrawAllSwayings);
        }
        public void DrawAllSwayings()
        {
            List<Point> pointsToRemove = new();
            foreach (Point location in placesToDraw)
            {
                Tile tile = Framing.GetTileSafely(location);
                if (RadianceSets.DrawWindSwayTiles[tile.TileType])
                    DrawSwaying(tile, location.X, location.Y);
                else
                    pointsToRemove.Add(location);
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
            float highestWindGridPushComplex = (float)tileDrawer.ReflectionInvokeMethod("GetHighestWindGridPushComplex", BindingFlags.Instance | BindingFlags.NonPublic, tileX, tileY, width, height, totalPushTime, pushForcePerFrame, 3, true);
            
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
                    if (type2 != type || !(bool)tileDrawer.ReflectionInvokeMethod("IsVisible", BindingFlags.Instance | BindingFlags.NonPublic, tile2))
                        continue;
                    
                    short tileFrameX = tile2.TileFrameX;
                    short tileFrameY = tile2.TileFrameY;
                    float num = (float)(j - tileY + 1) / height;
                    if (num == 0f)
                        num = 0.1f;

                    tileDrawer.GetTileDrawData(i, j, tile2, type2, ref tileFrameX, ref tileFrameY, out var tileWidth, out var tileHeight, out var tileTop, out var halfBrickHeight, out var addFrX, out var addFrY, out var tileSpriteEffect, out var _, out var _, out var _);
                    Color tileLight = Lighting.GetColor(i, j);
                    bool canDoDust = Main.rand.NextBool(4);
                    tileDrawer.ReflectionInvokeMethod("DrawAnimatedTile_AdjustForVisionChangers", BindingFlags.Instance | BindingFlags.NonPublic, i, j, tile2, type2, tileFrameX, tileFrameY, tileLight, canDoDust);
                    tileLight = (Color)tileDrawer.ReflectionInvokeMethod("DrawTiles_GetLightOverride", BindingFlags.Instance | BindingFlags.NonPublic, j, i, tile2, type2, tileFrameX, tileFrameY, tileLight);

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
                    }
                }
            }
        }
    }
}
