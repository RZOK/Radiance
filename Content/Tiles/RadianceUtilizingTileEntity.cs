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

        public bool active = false;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            if (tile.HasTile && tile.TileType == ParentTile)
            {
                active = true;
                return true;
            }
            active = false;
            return false;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }
        public void AddToIndex()
        {
            for (int i = 0; i < Radiance.maxRadianceUtilizingTileEntities + 1; i++)
            {
                RadianceUtilizingTileEntity indexedTile = Radiance.radianceUtilizingTileEntityIndex[i];
                if(!RadianceTransferSystem.Instance.IsTileEntityReal(indexedTile))
                {
                    Main.NewText(i);
                    Radiance.radianceUtilizingTileEntityIndex[i] = this;
                    break;
                }
            }
        }
    }
}
