using Microsoft.Xna.Framework;
using Radiance.Content.Tiles;
using Radiance.Core.Systems;
using Radiance.Utils;
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
        public int transferRate = 20;
        public bool isInterferred = false;
        public bool active = false;
        public bool pickedUp = false;
        public int pickedUpTimer = 0;

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
            }
            Main.NewText(num);
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

#nullable enable

        public static RadianceRay? FindRay(Vector2 pos)
#nullable disable
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
            if (pickedUpTimer > 0)
                pickedUpTimer--;
            if (pickedUpTimer == 0)
                pickedUp = false;

            if (Main.GameUpdateCount % 60 == 0)
                DetectIntersection();

            if (!pickedUp && startPos == endPos)
                Kill();

            SnapToPosition(startPos, endPos);
            MoveRadiance(GetIO(startPos), GetIO(endPos));
        }

        public void DetectIntersection()
        {
        }

        public void SnapToPosition(Vector2 start, Vector2 end)
        {
            Vector2 objectiveStartPosition = SnapToCenterOfTile(start);
            startPos = Vector2.Lerp(startPos, objectiveStartPosition, 0.5f);

            Vector2 objectiveEndPosition = SnapToCenterOfTile(end);
            endPos = Vector2.Lerp(endPos, objectiveEndPosition, 0.5f);
        }

        public void MoveRadiance((RadianceUtilizingTileEntity, IOEnum) start, (RadianceUtilizingTileEntity, IOEnum) end)
        {
            RadianceUtilizingTileEntity startEntity = start.Item1;
            IOEnum startMode = start.Item2;
            RadianceUtilizingTileEntity endEntity = end.Item1;
            IOEnum endMode = end.Item2;
            if (startEntity != null && endEntity != null)
            {
                if ((startEntity.MaxRadiance == 0 || endEntity.MaxRadiance == 0) || (startMode == endMode || (startMode == IOEnum.None || endMode == IOEnum.None)))
                {
                    return;
                }
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
        public void ActuallyMoveRadiance(RadianceUtilizingTileEntity source, RadianceUtilizingTileEntity destination, float amount)
        {
            if (TileUtils.TryGetTileEntityAs(source.Position.X, source.Position.Y, out PedestalTileEntity sourcePedestal) && sourcePedestal != null && sourcePedestal.containerPlaced != null)
                sourcePedestal.containerPlaced.CurrentRadiance -= Math.Min(source.CurrentRadiance, amount);
            else
                source.CurrentRadiance -= Math.Min(source.CurrentRadiance, amount);

            if (TileUtils.TryGetTileEntityAs(destination.Position.X, destination.Position.Y, out PedestalTileEntity destinationPedestal) && destinationPedestal != null && destinationPedestal.containerPlaced != null) 
                destinationPedestal.containerPlaced.CurrentRadiance += Math.Min(source.CurrentRadiance, amount);
            else
                destination.CurrentRadiance += Math.Min(source.CurrentRadiance, amount);
        }
#nullable enable

        public (RadianceUtilizingTileEntity?, IOEnum) GetIO(Vector2 pos)
#nullable disable
        {
            Tile posTile = Main.tile[(int)pos.X / 16, (int)pos.Y / 16];
            if (posTile != null && TileObjectData.GetTileData(posTile) != null)
            {
                Vector2 currentPos = new();
                int tePosX = (int)pos.X / 16 - posTile.TileFrameX % 36 / 18;
                int tePosY = (int)pos.Y / 16 - posTile.TileFrameY / 18;
                if (TileUtils.TryGetTileEntityAs(tePosX, tePosY, out RadianceUtilizingTileEntity entity))
                {
                    for (int y = 0; y < entity.Height * entity.Height; y++)
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
                                return (entity, IOEnum.Input);
                            }
                            else if (entity.OutputTiles.Contains(ioFinder))
                            {
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
            {
                return;
            }
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