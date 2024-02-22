using Radiance.Content.Tiles.Transmutator;

namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static SetFactory ItemFactory = new SetFactory(ItemLoader.ItemCount);
        public static SetFactory TilesFactory = new SetFactory(TileLoader.TileCount);

        public static int[] RadianceProjectorLensID = ItemFactory.CreateCustomSet(0);
        public static int[] RadianceProjectorLensDust = ItemFactory.CreateCustomSet(0);

        //i hate this. i can make it in a better way while being compatible with vanilla items
        public static string[] RadianceProjectorLensTexture = ItemFactory.CreateCustomSet(string.Empty);
        public static SoundStyle[] RadianceProjectorLensSound = ItemFactory.CreateCustomSet(Radiance.ProjectorLensTink);
        public static Action<ProjectorTileEntity>[] RadianceProjectorLensPreOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);
        public static Action<ProjectorTileEntity>[] RadianceProjectorLensOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);

        public static (float Amount, float Speed)[] RadianceCellAbsorptionStats = ItemFactory.CreateCustomSet<(float, float)>((0, 0));

        public static float[] SetPedestalStability = ItemFactory.CreateFloatSet(0);

        public static bool[] DrawWindSwayTiles = TilesFactory.CreateBoolSet(false);

    }
}
