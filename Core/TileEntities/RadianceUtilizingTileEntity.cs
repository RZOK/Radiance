﻿namespace Radiance.Core.TileEntities
{
    /// <summary>
    /// An ImprovedTileEntity that is also a Radiance container.
    /// </summary>
    public abstract class RadianceUtilizingTileEntity : ImprovedTileEntity, IRadianceContainer
    {
        public readonly List<int> inputTiles;
        public readonly List<int> outputTiles;
        public float storedRadiance { get; set; }
        public float maxRadiance;

        public RadianceUtilizingTileEntity(int parentTile, float maxRadiance, List<int> inputTiles, List<int> outputTiles, float updateOrder = 1, bool usesStability = false) : base(parentTile, updateOrder, usesStability) 
        {
            this.maxRadiance = maxRadiance;
            this.inputTiles = inputTiles;
            this.outputTiles = outputTiles;
        }
        public sealed override void SaveExtraData(TagCompound tag)
        {
            if (storedRadiance > 0)
                tag[nameof(storedRadiance)] = storedRadiance;

            SaveExtraExtraData(tag);
        }
        public virtual void SaveExtraExtraData(TagCompound tag) { }
        public sealed override void LoadExtraData(TagCompound tag)
        {
            storedRadiance = tag.GetFloat(nameof(storedRadiance));
            LoadExtraExtraData(tag);
        }
        public virtual void LoadExtraExtraData(TagCompound tag) { }
    }
}
