using System.Collections.Generic;

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
                tag[nameof(currentRadiance)] = currentRadiance;
            tag[nameof(enabled)] = enabled;
            SaveExtraData(tag);
        }
        public virtual void SaveExtraData(TagCompound tag) { }
        public sealed override void LoadData(TagCompound tag)
        {
            currentRadiance = tag.GetFloat(nameof(currentRadiance));
            enabled = tag.GetBool(nameof(enabled));
            LoadExtraData(tag);
        }
        public virtual void LoadExtraData(TagCompound tag) { }
    }
}
