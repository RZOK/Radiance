using Microsoft.Xna.Framework;
using Radiance.Content.Tiles;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Core
{
    public class RadianceRay : TagSerializable
    {
        public int id = 0;
        public Vector2 startPos = Vector2.Zero;
        public Vector2 endPos = Vector2.Zero;
        public float transferRate = 2;
        public bool interferred = false;
        public bool active = false;
        public bool pickedUp = false;
        public int pickedUpTimer = 0;
        public bool disappearing = false;
        public float disappearTimer = 0;

        public enum IOEnum
        {
            None,
            Input,
            Output
        }

        #region Utility Methods

        public static int NewRadianceRay(Vector2 startPosition, Vector2 endPosition)
        {
            int num = Radiance.maxRays;
            for (int i = 0; i < Radiance.maxRays; i++)
            {
                if (Radiance.radianceRay[i] == null || !Radiance.radianceRay[i].active)
                {
                    num = i;
                    break;
                }
            }
            if (num == 1000)
            {
                //todo
            }
            Radiance.radianceRay[num] = new RadianceRay();
            RadianceRay radianceRay = Radiance.radianceRay[num];
            radianceRay.startPos = startPosition;
            radianceRay.endPos = endPosition;
            radianceRay.active = true;
            RadianceTransferSystem.Instance.rayList.Add(radianceRay);
            return num;
        }

        public static Vector2 SnapToCenterOfTile(Vector2 input)
        {
            return new Vector2((int)(Math.Floor(input.X / 16) * 16), (int)(Math.Floor(input.Y / 16) * 16)) + new Vector2(8, 8);
        }

        public static RadianceRay FindRay(Vector2 pos)
        {
            for (int i = 0; i < Radiance.maxRays; i++)
            {
                RadianceRay ray = Radiance.radianceRay[i];
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
                if (disappearTimer >= 60) Kill();
            }
            else

            if (pickedUpTimer > 0)
                pickedUpTimer--;
            if (pickedUpTimer == 0)
                pickedUp = false;
            
            if (!pickedUp && startPos == endPos)
                Kill();

            SnapToPosition(startPos, endPos);
            if (!pickedUp)
                MoveRadiance(GetIO(startPos), GetIO(endPos));
            if (Main.GameUpdateCount % 60 == 0)
                if(HasIntersection())
                    interferred = true;
                else
                    interferred = false;

        }
        public static bool OnSegment(Vector2 p, Vector2 q, Vector2 r) => q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) && q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y);
        public static int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            float val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0) return 0;
            return (val > 0) ? 1 : 2; 
        }
        public bool HasIntersection() 
        {
            for (int i = 0; i < Radiance.maxRays; i++)
            {
                if (Radiance.radianceRay[i] != null && Radiance.radianceRay[i].active && Radiance.radianceRay[i] != this)
                {
                    RadianceRay ray = Radiance.radianceRay[i];

                    int o1 = Orientation(startPos, endPos, ray.startPos);
                    int o2 = Orientation(startPos, endPos, ray.endPos);
                    int o3 = Orientation(ray.startPos, ray.endPos, startPos);
                    int o4 = Orientation(ray.startPos, ray.endPos, endPos);

                    if (o1 != o2 && o3 != o4)
                        return true;

                    if (o1 == 0 && OnSegment(startPos, ray.startPos, endPos)) return true;
                    if (o2 == 0 && OnSegment(startPos, ray.endPos, endPos)) return true;
                    if (o3 == 0 && OnSegment(ray.startPos, startPos, ray.endPos)) return true;
                    if (o4 == 0 && OnSegment(ray.startPos, endPos, ray.endPos)) return true;

                }
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

#nullable enable
        public (RadianceUtilizingTileEntity?, IOEnum) GetIO(Vector2 pos) //Returns a tuple of a RUTE and an IOEnum to grab a tile entity and if it should be inputted or outputted from
#nullable disable
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

        public void Kill()
        {
            if (!active)
                return;
            active = false;
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
                ["Active"] = active
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