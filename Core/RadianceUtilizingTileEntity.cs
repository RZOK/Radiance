using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Radiance.Core
{
    public abstract class RadianceUtilizingTileEntity : ImprovedTileEntity
    {
        public float maxRadiance;
        public readonly List<int> inputTiles;
        public readonly List<int> outputTiles;

        public float currentRadiance = 0;

        public RadianceUtilizingTileEntity(int parentTile, float maxRadiance, List<int> inputTiles, List<int> outputTiles, float updateOrder = 1, bool usesStability = false) : base(parentTile, updateOrder, usesStability) 
        {
            this.maxRadiance = maxRadiance;
            this.inputTiles = inputTiles;
            this.outputTiles = outputTiles;
        }
        public sealed override void SaveData(TagCompound tag)
        {
            if (currentRadiance > 0)
                tag["CurrentRadiance"] = currentRadiance;
            tag["Enabled"] = enabled;
            SaveExtraData(tag);
        }
        public virtual void SaveExtraData(TagCompound tag) { }
        public sealed override void LoadData(TagCompound tag)
        {
            currentRadiance = tag.GetFloat("CurrentRadiance");
            enabled = tag.GetBool("Enabled");
            LoadExtraData(tag);
        }
        public virtual void LoadExtraData(TagCompound tag) { }
    }
}
