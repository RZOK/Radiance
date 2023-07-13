using Radiance.Content.Tiles.Pedestals;

namespace Radiance.Core.Interfaces
{
    public interface IPedestalItem
    {
        public Color aoeCircleColor { get; }
        public float aoeCircleRadius { get; }

        public void PedestalEffect(PedestalTileEntity pte);
    }
}