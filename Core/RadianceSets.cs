using Radiance.Content.Tiles.Transmutator;

namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static SetFactory ItemFactory = new SetFactory(ItemLoader.ItemCount);
        public static SetFactory TilesFactory = new SetFactory(TileLoader.TileCount);

        public static string[] EncycloradiaRelatedEntry = ItemFactory.CreateCustomSet(string.Empty);

        public static int[] ProjectorLensID = ItemFactory.CreateIntSet();
        public static int[] ProjectorLensDust = ItemFactory.CreateIntSet();
        public static string[] ProjectorLensTexture = ItemFactory.CreateCustomSet(string.Empty);
        public static SoundStyle[] ProjectorLensSound = ItemFactory.CreateCustomSet(default(SoundStyle));
        public static Action<ProjectorTileEntity>[] ProjectorLensPreOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);
        public static Action<ProjectorTileEntity>[] ProjectorLensOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);

        public static int[] SetPedestalStability = ItemFactory.CreateIntSet(0);

        public static bool[] DrawWindSwayTiles = TilesFactory.CreateBoolSet(false);

    }
}
