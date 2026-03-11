using Radiance.Content.Tiles.Pedestals;

namespace Radiance.Core.Interfaces
{
    public interface IPedestalItem
    {
        /// <summary>
        /// Runs every tick, before all tile entities have their regular update, while the item is placed in a pedestal.
        /// <br></br>
        /// <br></br>
        /// Wire-enabledness must be added manually!
        /// </summary>
        /// <param name="pedestal">The pedestal tile entity the item is in.</param>
        public void PreUpdatePedestal(PedestalTileEntity pedestal);

        /// <summary>
        /// Runs every tick while the item is placed in a pedestal.
        /// <br></br>
        /// <br></br>
        /// Wire-enabledness must be added manually!
        /// </summary>
        /// <param name="pedestal">The pedestal tile entity the item is in.</param>
        public void UpdatePedestal(PedestalTileEntity pedestal);

        /// <summary>
        /// Adds any desired HoverUIElements to the pedestal.
        /// </summary>
        /// <param name="pedestal">The pedestal tile entity the item is in.</param>
        /// <returns>A list of HoverUIElements to add to the pedestal.</returns>
        public List<HoverUIElement> GetHoverData(PedestalTileEntity pedestal);
    }
}