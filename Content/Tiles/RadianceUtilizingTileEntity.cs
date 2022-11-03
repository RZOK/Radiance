using Terraria;
using Terraria.ModLoader;

namespace Radiance.Content.Tiles
{
    public abstract class RadianceUtilizingTileEntity : ModTileEntity
    {
        public abstract float MaxRadiance { get; set; }
        public abstract float CurrentRadiance { get; set; }

    }
}
