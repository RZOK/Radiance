using Radiance.Content.Items.BaseItems;

namespace Radiance.Core.TileEntities
{
    public abstract class StabilizerTileEntity : ImprovedTileEntity
    {
        public StabilizerTileEntity(int parentTile) : base(parentTile, 2, false) { }
        public virtual int StabilizerRange { get; }
        public virtual int StabilityLevel { get; }
        public virtual BaseStabilizationCrystal.StabilizeType StabilizationType { get; }
    }
}
