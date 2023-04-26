using Microsoft.Xna.Framework;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Radiance.Core
{
    /// <summary>
    /// An 'improved' abstract ModTileEntity that comes with necessary placement methods, size properties, and stability support.
    /// </summary>
    public abstract class ImprovedTileEntity : ModTileEntity
    {
        public readonly int ParentTile;
        public bool isStabilized => idealStability > 0 && Math.Abs(1 - stability / idealStability) <= 0.1f;
        public bool usesStability = false;
        public float stability;
        public float idealStability;
        public bool enabled = true;
        public int Width => TileObjectData.GetTileData(ParentTile, 0).Width;
        public int Height => TileObjectData.GetTileData(ParentTile, 0).Height;

        public ImprovedTileEntity(int parentTile, bool usesStability = false)
        {
            ParentTile = parentTile;
            this.usesStability = usesStability;
        }
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ParentTile;
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            Point16 origin = RadianceUtils.GetTileOrigin(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, origin.X, origin.Y, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, origin.X, origin.Y, Type);
            }
            int placedEntity = Place(origin.X, origin.Y);
            StabilityHandler.ResetStabilizers();
            return placedEntity;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }
        public override void OnKill()
        {
            StabilityHandler.ResetStabilizers();
        }
    }
}