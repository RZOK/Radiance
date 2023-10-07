using System.Collections.Generic;

namespace Radiance.Core.TileEntities
{
    /// <summary>
    /// An ImprovedTileEntity that is also a Radiance container.
    /// </summary>
    public abstract class RadianceUtilizingTileEntity : ImprovedTileEntity
    {
        public readonly List<int> inputTiles;
        public readonly List<int> outputTiles;

        public float currentRadiance = 0;
        public float maxRadiance;

        public RadianceUtilizingTileEntity(int parentTile, float maxRadiance, List<int> inputTiles, List<int> outputTiles, float updateOrder = 1, bool usesStability = false) : base(parentTile, updateOrder, usesStability) 
        {
            this.maxRadiance = maxRadiance;
            this.inputTiles = inputTiles;
            this.outputTiles = outputTiles;
        }
        public sealed override void SaveExtraData(TagCompound tag)
        {
            if (currentRadiance > 0)
                tag[nameof(currentRadiance)] = currentRadiance;

            SaveExtraExtraData(tag);
        }
        public virtual void SaveExtraExtraData(TagCompound tag) { }
        public sealed override void LoadExtraData(TagCompound tag)
        {
            currentRadiance = tag.GetFloat(nameof(currentRadiance));
            LoadExtraExtraData(tag);
        }
        public virtual void LoadExtraExtraData(TagCompound tag) { }
    }
}
