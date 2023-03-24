using Microsoft.Xna.Framework;
using Radiance.Content.Tiles;

namespace Radiance.Core.Interfaces
{
    public interface IPedestalItem
    {
        public Color aoeCircleColor { get; }
        public float aoeCircleRadius { get; }
        public void PedestalEffect(PedestalTileEntity pte);
    }
}
