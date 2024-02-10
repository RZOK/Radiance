using Radiance.Content.Tiles.Transmutator;

namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static SetFactory ItemFactory = new SetFactory(ItemLoader.ItemCount);
        public static SetFactory TilesFactory = new SetFactory(TileLoader.TileCount);

        public static int[] RadianceProjectorLensID = ItemFactory.CreateCustomSet(0);
        public static int[] RadianceProjectorLensDust = ItemFactory.CreateCustomSet(0);
        public static string[] RadianceProjectorLensTexture = ItemFactory.CreateCustomSet(string.Empty);
        public static SoundStyle[] RadianceProjectorLensSound = ItemFactory.CreateCustomSet(Radiance.ProjectorLensTink);
        public static Action<ProjectorTileEntity>[] RadianceProjectorLensPreOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);
        public static Action<ProjectorTileEntity>[] RadianceProjectorLensOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);

        public static int[] SetPedestalStability = ItemFactory.CreateIntSet(0);

        public static bool[] DrawWindSwayTiles = TilesFactory.CreateBoolSet(false);

    }
}
