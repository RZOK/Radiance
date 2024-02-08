using Radiance.Content.Tiles.Pedestals;

namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static SetFactory ItemFactory = new SetFactory(ItemLoader.ItemCount);
        public static SetFactory TilesFactory = new SetFactory(TileLoader.TileCount);

        public static int[] RadianceProjectorLensID = ItemFactory.CreateCustomSet(0);
        public static int[] RadianceProjectorLensDust = ItemFactory.CreateCustomSet(0);
        public static string[] RadianceProjectorLensTexture = ItemFactory.CreateCustomSet(string.Empty);
        public static Action<PedestalTileEntity>[] RadianceProjectorLensPreOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<PedestalTileEntity>>(null);
        public static Action<PedestalTileEntity>[] RadianceProjectorLensOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<PedestalTileEntity>>(null);

        public static int[] SetPedestalStability = ItemFactory.CreateIntSet(0);

        public static bool[] DrawWindSwayTiles = TilesFactory.CreateBoolSet(false);

    }
}
