using Radiance.Core.Systems;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Radiance.Core;

namespace Radiance.Content.Tiles
{
    public abstract class RadianceUtilizingTileEntity : ModTileEntity
    {
        public abstract float MaxRadiance { get; set; }
        public abstract float CurrentRadiance { get; set; }
        public abstract int ParentTile { get; set; }
        public abstract List<int> InputTiles { get; set; }
        public abstract List<int> OutputTiles { get; set; }
        public abstract int Width { get; set; }
        public abstract int Height { get; set; }

        public List<RadianceRay> inputsConnected = new();
        public List<RadianceRay> outputsConnected = new();
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ParentTile;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i - Math.Max(Width - 1, 0), j - Math.Max(Height - 1, 0));
            return placedEntity;
        }
        public void AddToCoordinateList()
        {
            if(!RadianceTransferSystem.Instance.Coords.Contains((Position.X, Position.Y)))
                RadianceTransferSystem.Instance.Coords.Add((Position.X, Position.Y));
        }
        public void RemoveFromCoordinateList()
        {
            if (RadianceTransferSystem.Instance.Coords.Contains((Position.X, Position.Y)))
                RadianceTransferSystem.Instance.Coords.Remove((Position.X, Position.Y));
        }
        public override void Update()
        {
            AddToCoordinateList();

            inputsConnected.Clear();
            outputsConnected.Clear();
        }
        public override void SaveData(TagCompound tag)
        {
            if (CurrentRadiance > 0)
                tag["CurrentRadiance"] = CurrentRadiance;
        }
        //public void SetRayConnections()
        //{
        //    Vector2 currentPos = new();
        //        for (int y = 0; y < Width * Height; y++)
        //        {
        //            if (currentPos.X >= Width)
        //            {
        //                currentPos.X = 0;
        //                currentPos.Y++;
        //            }
        //            int ioFinder = (int)(currentPos.X + (currentPos.Y * Width)) + 1;
        //            if (RadianceRay.FindRay(new Vector2(Position.X, Position.Y) + currentPos * 16 - new Vector2(8, 8), out RadianceRay ray))
        //            {
                        
        //            }
        //            currentPos.X++;
        //        }
        //    }
        public override void LoadData(TagCompound tag)
        {
            CurrentRadiance = tag.Get<float>("CurrentRadiance");
        }
        //public void AddToIndex()
        //{
        //    for (int i = 0; i < Radiance.maxRadianceUtilizingTileEntities; i++)
        //    {
        //        RadianceUtilizingTileEntity indexedTile = Radiance.radianceUtilizingTileEntityIndex[i];
        //        if(!RadianceTransferSystem.Instance.IsTileEntityReal(indexedTile))
        //        {
        //            Main.NewText(i);
        //            Radiance.radianceUtilizingTileEntityIndex[i] = this;
        //            break;
        //        }
        //    }
        //}
    }
}
