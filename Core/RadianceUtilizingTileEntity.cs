using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Radiance.Core
{
    public abstract class RadianceUtilizingTileEntity : ImprovedTileEntity
    {
        public float maxRadiance;
        public readonly List<int> inputTiles;
        public readonly List<int> outputTiles;

        public float currentRadiance = 0;
        public bool enabled = true;

        public RadianceUtilizingTileEntity(int parentTile, float maxRadiance, List<int> inputTiles, List<int> outputTiles, bool usesStability) : base(parentTile, usesStability) 
        {
            this.maxRadiance = maxRadiance;
            this.inputTiles = inputTiles;
            this.outputTiles = outputTiles;
        }
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ParentTile;
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
