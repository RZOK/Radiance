using Radiance.Core.Systems;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
