using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Core
{
    public abstract class RadianceUtilizingTileEntity : ModTileEntity
    {
        public readonly int parentTile;
        public float maxRadiance;
        public readonly List<int> inputTiles;
        public readonly List<int> outputTiles;
        public readonly int width;
        public readonly int height;

        public float currentRadiance = 0;
        public bool enabled = true;

        public RadianceUtilizingTileEntity(int parentTile, float maxRadiance, List<int> inputTiles, List<int> outputTiles, int width, int height)
        {
            this.parentTile = parentTile;
            this.maxRadiance = maxRadiance;
            this.inputTiles = inputTiles;
            this.outputTiles = outputTiles;
            this.width = width;
            this.height = height;
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
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            }
            int placedEntity = Place(i - width - 1, j - height - 1);
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
