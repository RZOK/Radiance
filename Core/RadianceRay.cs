using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Tiles;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Radiance.Core.Systems.RadianceTransferSystem;

namespace Radiance.Core
{
    public class RadianceRay : TagSerializable
    {
        public Vector2 startPos = Vector2.Zero;
        public Vector2 endPos = Vector2.Zero;
        public float transferRate = 2;
        public bool interferred = false;
        public bool active = false;
        public bool pickedUp = false;
        public int pickedUpTimer = 0;
        public bool disappearing = false;
        public float disappearTimer = 0;

        public RadianceUtilizingTileEntity inputTE;
        public RadianceUtilizingTileEntity outputTE;

        public float disappearProgress => 1 - disappearTimer / 30;

        #region Utility Methods

        public static RadianceRay NewRadianceRay(Vector2 startPosition, Vector2 endPosition)
        {
            RadianceRay radianceRay = new RadianceRay();
            radianceRay.startPos = startPosition;
            radianceRay.endPos = endPosition;
            radianceRay.active = true;
            rays.Add(radianceRay);
            return radianceRay;
        }

        public static Vector2 SnapToCenterOfTile(Vector2 input) => new Vector2(input.X - input.X % 16, input.Y - input.Y % 16) + new Vector2(8, 8);

        public static bool FindRay(Vector2 pos, out RadianceRay outRay)
        {
            outRay = rays.FirstOrDefault(x => x.active && (x.startPos == pos || x.endPos == pos));
            return outRay != null;
        }

        #endregion Utility Methods

        #region Ray Methods

        public void Update()
        {
            if (!pickedUp && inputTE == null && outputTE == null)
                disappearing = true;
            else
            {
                if (disappearTimer > 0)
                    disappearTimer -= Math.Min(disappearTimer, 6);
                disappearing = false;
            }

            if (disappearing)
            {
                disappearTimer++;
                if (disappearTimer >= 30) active = false;
            }
            else

            if (pickedUpTimer > 0)
                pickedUpTimer--;
            if (pickedUpTimer == 0)
                pickedUp = false;
            
            if (!pickedUp && startPos == endPos)
                active = false;

            SnapToPosition(startPos, endPos);
            if (!pickedUp)
            {
                TryGetIO(this, out inputTE, out outputTE);
                if (inputTE != null && outputTE != null)
                    ActuallyMoveRadiance(outputTE, inputTE, transferRate);
            }
            else
                inputTE = outputTE = null;

            if (Main.GameUpdateCount % 60 == 0)
                interferred = HasIntersection();
        }
        public bool HasIntersection()
        {
            foreach (RadianceRay ray in rays)
            {
                if (ray.startPos == startPos)
                    continue;

                if (Collision.CheckLinevLine(startPos, endPos, ray.startPos, ray.endPos).Length > 0)
                    return true;
            }
            return false;
        }
        public void SnapToPosition(Vector2 start, Vector2 end) //Snaps an endpoint to the center of the tile
        {
            startPos = Vector2.Lerp(startPos, SnapToCenterOfTile(start), 0.5f);
            endPos = Vector2.Lerp(endPos, SnapToCenterOfTile(end), 0.5f);
        }
        public static void TryGetIO(RadianceRay ray, out RadianceUtilizingTileEntity input, out RadianceUtilizingTileEntity output)
        {
            input = null;
            output = null;

            Point startCoords = Utils.ToTileCoordinates(ray.startPos);
            Point endCoords = Utils.ToTileCoordinates(ray.endPos);

            Tile startTile = Framing.GetTileSafely(startCoords.X, startCoords.Y);
            Tile endTile = Framing.GetTileSafely(endCoords.X, endCoords.Y);

            Vector2 startTEPos = new Vector2(startCoords.X - startTile.TileFrameX / 18, startCoords.Y - startTile.TileFrameY / 18);
            Vector2 endTEPos = new Vector2(endCoords.X - endTile.TileFrameX / 18, endCoords.Y - endTile.TileFrameY / 18);

            if (RadianceUtils.TryGetTileEntityAs((int)startTEPos.X, (int)startTEPos.Y, out RadianceUtilizingTileEntity entity))
            {
                int position1 = startTile.TileFrameX / 18 + (startTile.TileFrameY / 18) * entity.Width + 1;
                if (entity.inputTiles.Contains(position1))
                    input = entity;
                else if (entity.outputTiles.Contains(position1))
                    output = entity;
            }

            if (RadianceUtils.TryGetTileEntityAs((int)endTEPos.X, (int)endTEPos.Y, out RadianceUtilizingTileEntity entity2))
            {
                int position = endTile.TileFrameX / 18 + (endTile.TileFrameY / 18) * entity2.Width + 1;
                if (entity2.inputTiles.Contains(position))
                    input = entity2;
                else if (entity2.outputTiles.Contains(position))
                    output = entity2;
            }
        }
        public void ActuallyMoveRadiance(RadianceUtilizingTileEntity source, RadianceUtilizingTileEntity destination, float amount) //Actually manipulates Radiance values between source and destination
        {
            if (interferred) amount /= 500;
            float val = Math.Min(source.currentRadiance, destination.maxRadiance - destination.currentRadiance);
            if (source.currentRadiance < amount * source.outputTiles.Count)
            {
                amount /= source.outputTiles.Count;
            }
            float amountMoved = Math.Clamp(val, 0, amount);

            if (RadianceUtils.TryGetTileEntityAs(source.Position.X, source.Position.Y, out PedestalTileEntity sourcePedestal) && sourcePedestal != null)
            {
                if (sourcePedestal.containerPlaced != null)
                {
                    sourcePedestal.containerPlaced.CurrentRadiance -= amountMoved;
                    sourcePedestal.GetRadianceFromItem(sourcePedestal.containerPlaced);
                }
            }
            else
                source.currentRadiance -= amountMoved;

            if (RadianceUtils.TryGetTileEntityAs(destination.Position.X, destination.Position.Y, out PedestalTileEntity destinationPedestal) && destinationPedestal != null)
            {
                if (destinationPedestal.containerPlaced != null)
                {
                    destinationPedestal.containerPlaced.CurrentRadiance += amountMoved;
                    destinationPedestal.GetRadianceFromItem(destinationPedestal.containerPlaced);
                }
            }
            else
                destination.currentRadiance += amountMoved;
        }

        internal PrimitiveTrail RayPrimDrawer;
        internal PrimitiveTrail RayPrimDrawer2;
        public void DrawRay()
        {
            Color color = CommonColors.RadianceColor1;
            if (interferred)
                color = new Color(200, 50, 50);

            for (int i = 0; i < 2; i++)
            {
                RadianceDrawing.DrawSoftGlow(i == 0 ? endPos : startPos, color * disappearProgress, 0.2f, RadianceDrawing.DrawingMode.Beam);
                RadianceDrawing.DrawSoftGlow(i == 0 ? endPos : startPos, Color.White * disappearProgress, 0.16f, RadianceDrawing.DrawingMode.Beam);
            }
            Effect effect = Filters.Scene["UVMapStreak"].GetShader().Shader;

            RayPrimDrawer = RayPrimDrawer ?? new PrimitiveTrail(2, w => 10 * disappearProgress, ColorFunction, new NoTip());
            RayPrimDrawer.SetPositionsSmart(new List<Vector2>() { startPos, endPos }, endPos, RadianceUtils.RigidPointRetreivalFunction);
            RayPrimDrawer.NextPosition = endPos;
            effect.Parameters["time"].SetValue(0);
            effect.Parameters["fadePower"].SetValue(5);
            effect.Parameters["colorPower"].SetValue(1.6f);
            Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value;
            RayPrimDrawer?.Render(effect, -Main.screenPosition);

            RayPrimDrawer2 = RayPrimDrawer2 ?? new PrimitiveTrail(2, w => 4 * disappearProgress, ColorFunction2, new NoTip());
            RayPrimDrawer2.SetPositionsSmart(new List<Vector2>() { startPos, endPos }, endPos, RadianceUtils.SmoothBezierPointRetreivalFunction);
            RayPrimDrawer2.NextPosition = endPos;
            effect.Parameters["time"].SetValue(0);
            effect.Parameters["fadePower"].SetValue(3);
            effect.Parameters["colorPower"].SetValue(1.6f);
            Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value;
            RayPrimDrawer2?.Render(effect, -Main.screenPosition);
        }
        internal Color ColorFunction(float completionRatio)
        {
            Color trailColor = CommonColors.RadianceColor1;
            if (interferred)
                trailColor = new Color(200, 50, 50);
            trailColor *= disappearProgress;
            return trailColor;
        }
        internal Color ColorFunction2(float completionRatio)
        {
            return Color.White * disappearProgress;
        }

        #endregion Ray Methods

        #region TagCompound Stuff

        public static readonly Func<TagCompound, RadianceRay> DESERIALIZER = DeserializeData;

        public TagCompound SerializeData()
        {
            return new TagCompound()
            {
                ["StartPos"] = startPos,
                ["EndPos"] = endPos,
                ["Active"] = active,
            };
        }

        public static RadianceRay DeserializeData(TagCompound tag)
        {
            RadianceRay radianceRay = new()
            {
                startPos = tag.Get<Vector2>("StartPos"),
                endPos = tag.Get<Vector2>("EndPos"),
                active = tag.Get<bool>("Active")
            };
            return radianceRay;
        }

        #endregion TagCompound Stuff
    }
}