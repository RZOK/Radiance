using Microsoft.Xna.Framework;
using Radiance.Content.Tiles;

namespace Radiance.Core.Interfaces
{
    public interface IPedestalItem
    {
        Color aoeCircleColor { get; }
        float aoeCircleRadius { get; }
        void PedestalEffect(PedestalTileEntity pte);
    }
}
