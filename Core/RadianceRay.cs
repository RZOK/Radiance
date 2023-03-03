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
        public int id = -1;
        public Vector2 startPos = Vector2.Zero;
        public Vector2 endPos = Vector2.Zero;
        public float transferRate = 2;
        public bool interferred = false;
        public bool active = false;
        public bool pickedUp = false;
        public int pickedUpTimer = 0;
        public bool disappearing = false;
        public float disappearTimer = 0;

        public float disappearProgress => 1 - disappearTimer / 30;

        public enum IOEnum
        {
            None,
            Input,
            Output
        }

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

        public static RadianceRay FindRay(Vector2 pos)
        {
            foreach (RadianceRay ray in rays)
            { 
                if (ray != null && ray.active && (ray.startPos == pos || ray.endPos == pos))
                {
                    return ray;
                }
            }
            return null;
        }

        #endregion Utility Methods

        #region Ray Methods

        public void Update()
        {
            if (id == -1)
                id = rays.IndexOf(this);
            if (!pickedUp && GetIO(startPos).Item2 == IOEnum.None && GetIO(endPos).Item2 == IOEnum.None)
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
                MoveRadiance(GetIO(startPos), GetIO(endPos));

            if ((Main.GameUpdateCount + id) % 60 == 0)
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

        public void MoveRadiance((RadianceUtilizingTileEntity, IOEnum) start, (RadianceUtilizingTileEntity, IOEnum) end) //It's called MoveRadiance but it just returns an entity to grab from/to and if the side of the ray is input or output
        {
            RadianceUtilizingTileEntity startEntity = start.Item1;
            IOEnum startMode = start.Item2;
            RadianceUtilizingTileEntity endEntity = end.Item1;
            IOEnum endMode = end.Item2;
            if (startEntity != null && endEntity != null)
            {
                if (startEntity.MaxRadiance == 0 || endEntity.MaxRadiance == 0 || startMode == endMode || startMode == IOEnum.None || endMode == IOEnum.None)
                    return;
                switch (startMode)
                {
                    case IOEnum.Input:
                        ActuallyMoveRadiance(endEntity, startEntity, transferRate);
                        break;

                    case IOEnum.Output:
                        ActuallyMoveRadiance(startEntity, endEntity, transferRate);
                        break;
                }
            }
        }
        public void ActuallyMoveRadiance(RadianceUtilizingTileEntity source, RadianceUtilizingTileEntity destination, float amount) //Actually manipulates Radiance values between source and destination
        {
            if (interferred) amount /= 500;
            float val = Math.Min(source.CurrentRadiance, destination.MaxRadiance - destination.CurrentRadiance);
            if (source.CurrentRadiance < amount * source.OutputTiles.Count)
            {
                amount /= source.OutputTiles.Count;
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
                source.CurrentRadiance -= amountMoved;

            if (RadianceUtils.TryGetTileEntityAs(destination.Position.X, destination.Position.Y, out PedestalTileEntity destinationPedestal) && destinationPedestal != null)
            {
                if (destinationPedestal.containerPlaced != null)
                {
                    destinationPedestal.containerPlaced.CurrentRadiance += amountMoved;
                    destinationPedestal.GetRadianceFromItem(destinationPedestal.containerPlaced);
                }
            }
            else
                destination.CurrentRadiance += amountMoved;
        }

        public (RadianceUtilizingTileEntity, IOEnum) GetIO(Vector2 pos) //Returns a tuple of a RUTE and an IOEnum to grab a tile entity and if it should be inputted or outputted from
        {
            Tile posTile = Main.tile[(int)pos.X / 16, (int)pos.Y / 16];
            if (posTile != null && TileObjectData.GetTileData(posTile) != null)
            {
                Vector2 currentPos = new();
                int tePosX = (int)pos.X / 16 - posTile.TileFrameX / 18;
                int tePosY = (int)pos.Y / 16 - posTile.TileFrameY / 18;
                if (RadianceUtils.TryGetTileEntityAs(tePosX, tePosY, out RadianceUtilizingTileEntity entity))
                {
                    for (int y = 0; y < entity.Width * entity.Height; y++)
                    {
                        if (currentPos.X >= entity.Width)
                        {
                            currentPos.X = 0;
                            currentPos.Y++;
                        }
                        int ioFinder = (int)(currentPos.X + (currentPos.Y * entity.Width)) + 1;
                        if ((new Vector2(entity.Position.X, entity.Position.Y) + currentPos) * 16 == new Vector2((int)pos.X - 8, (int)pos.Y - 8))
                        {
                            if (entity.InputTiles.Contains(ioFinder))
                            {
                                if(!entity.inputsConnected.Contains(this))
                                    entity.inputsConnected.Add(this);
                                return (entity, IOEnum.Input);
                            }
                            else if (entity.OutputTiles.Contains(ioFinder))
                            {
                                if (!entity.outputsConnected.Contains(this))
                                    entity.outputsConnected.Add(this);
                                return (entity, IOEnum.Output);
                            }
                        }
                        currentPos.X++;
                    }
                }
            }
            return (null, IOEnum.None);
        }

        internal PrimitiveTrail RayPrimDrawer;
        internal PrimitiveTrail RayPrimDrawer2;
        public void DrawRay()
        {
            Color color = CommonColors.RadianceColor1;
            if (interferred)
                color = new Color(200, 0, 0);

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