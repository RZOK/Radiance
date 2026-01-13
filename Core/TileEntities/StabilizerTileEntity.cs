using Radiance.Content.Items.BaseItems;

namespace Radiance.Core.TileEntities
{
    /// <summary>
    /// All StabilizerTileEntities are implied to be 1-wide with the 'source' of the stability at the top tile.
    /// </summary>
    public abstract class StabilizerTileEntity : ImprovedTileEntity
    {
        public StabilizerTileEntity(int parentTile) : base(parentTile, 2, false)
        {
        }

        public virtual int StabilizerRange { get; }
        public virtual int StabilityLevel { get; }
        public virtual BaseStabilizationCrystal.StabilizeType StabilizationType { get; }
    }
}