namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static SetFactory ItemFactory = new SetFactory(ItemLoader.ItemCount);
        public static SetFactory TilesFactory = new SetFactory(TileLoader.TileCount);

        public static int[] SetPedestalStability = ItemFactory.CreateIntSet(0);
        public static bool[] DrawWindSwayTiles = TilesFactory.CreateBoolSet(false);

    }
}
