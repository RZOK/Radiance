using Radiance.Core.Systems;

namespace Radiance.Core.TileEntities
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

        public RadianceUtilizingTileEntity(int parentTile, float maxRadiance, List<int> inputTiles, List<int> outputTiles, float updateOrder = 1, bool usesItemImprints = false) : base(parentTile, updateOrder, usesItemImprints)
        {
            this.maxRadiance = maxRadiance;
            this.inputTiles = inputTiles;
            this.outputTiles = outputTiles;
        }

        public override sealed void SaveExtraData(TagCompound tag)
        {
            if (storedRadiance > 0)
                tag[nameof(storedRadiance)] = storedRadiance;

            SaveExtraExtraData(tag);
        }

        public virtual void SaveExtraExtraData(TagCompound tag)
        { }

        public override sealed void LoadExtraData(TagCompound tag)
        {
            storedRadiance = tag.GetFloat(nameof(storedRadiance));
            LoadExtraExtraData(tag);
        }

        public virtual void LoadExtraExtraData(TagCompound tag)
        { }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            RadianceTransferSystem.shouldUpdateRays = true;
            return base.Hook_AfterPlacement(i, j, type, style, direction, alternate);
        }

        public override void OnKill()
        {
            RadianceTransferSystem.shouldUpdateRays = true;
            base.OnKill();
        }
    }
}