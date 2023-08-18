using Radiance.Content.Tiles.Pedestals;

namespace Radiance.Core.Interfaces
{
    public interface IPedestalItem
    {
        public Color aoeCircleColor { get; }

        public float aoeCircleRadius { get; }

        /// /// <summary>
        /// Runs whenever a pedestal this item is placed in is wire-enabled.
        /// </summary>
        /// <param name="pte">The pedestal tile entity the item is in.</param>
        public void PedestalEffect(PedestalTileEntity pte);
    }
}