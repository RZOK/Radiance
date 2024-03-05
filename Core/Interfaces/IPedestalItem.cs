using Radiance.Content.Tiles.Pedestals;

namespace Radiance.Core.Interfaces
{
    public interface IPedestalItem
    {
        /// <summary>
        /// Runs whenever a pedestal this item is placed in pre-updates. Wire-enabledness must be added manually!
        /// </summary>
        /// <param name="pte">The pedestal tile entity the item is in.</param>
        public void PreUpdatePedestal(PedestalTileEntity pte);
        /// /// <summary>
        /// Runs whenever a pedestal this item is placed in updates. Wire-enabledness must be added manually!
        /// </summary>
        /// <param name="pte">The pedestal tile entity the item is in.</param>
        public void UpdatePedestal(PedestalTileEntity pte);
    }
}