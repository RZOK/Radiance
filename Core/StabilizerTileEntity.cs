using Radiance.Core.Interfaces;

namespace Radiance.Core
{
    public abstract class StabilizerTileEntity : ImprovedTileEntity
    {
        public StabilizerTileEntity(int parentTile, bool usesStability = false) : base(parentTile, usesStability) { }
        public virtual int StabilizerRange { get; }
        public virtual int StabilityLevel { get; }
        public virtual StabilizeType StabilizationType { get; }
    }
}
