using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Core
{
    public abstract class RadianceUtilizingTileEntity : ModTileEntity
    {
        public readonly int parentTile;
        public float maxRadiance;
        public readonly List<int> inputTiles;
        public readonly List<int> outputTiles;
        public int Width => TileObjectData.GetTileData(parentTile, 0).Width;
        public int Height => TileObjectData.GetTileData(parentTile, 0).Height;

        public float currentRadiance = 0;
        public bool enabled = true;

        public RadianceUtilizingTileEntity(int parentTile, float maxRadiance, List<int> inputTiles, List<int> outputTiles)
        {
            this.parentTile = parentTile;
            this.maxRadiance = maxRadiance;
            this.inputTiles = inputTiles;
            this.outputTiles = outputTiles;
        }
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == parentTile;
        }
        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, Width, Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i - Width - 1, j - Height - 1);
            return placedEntity;
        }
        public override void SaveData(TagCompound tag)
        {
            if (currentRadiance > 0)
                tag["CurrentRadiance"] = currentRadiance;
            tag["Enabled"] = enabled;
        }
        public override void LoadData(TagCompound tag)
        {
            currentRadiance = tag.GetFloat("CurrentRadiance");
            enabled = tag.GetBool("Enabled");
        }
    }
}
