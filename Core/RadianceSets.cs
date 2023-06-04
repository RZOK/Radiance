using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;

namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static SetFactory ItemFactory = new SetFactory(ItemLoader.ItemCount);
        public static SetFactory TilesFactory = new SetFactory(TileLoader.TileCount);

        public static bool[] DrawWindSwayTiles = TilesFactory.CreateBoolSet(false);
    }
}
