using Radiance.Content.Tiles.Transmutator;

namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static SetFactory ItemFactory = new SetFactory(ItemLoader.ItemCount);
        public static SetFactory TileFactory = new SetFactory(TileLoader.TileCount);

        public static string[] EncycloradiaRelatedEntry = ItemFactory.CreateCustomSet(string.Empty);
        public static Func<Item[], int, bool>[] RightClickMouseItemFunction = ItemFactory.CreateCustomSet<Func<Item[], int, bool>>(null);
        public static float[] SetPedestalStability = ItemFactory.CreateFloatSet(0);
        public static (float Amount, float Speed)[] RadianceCellAbsorptionStats = ItemFactory.CreateCustomSet<(float, float)>((0, 0));

        public static int[] ProjectorLensID = ItemFactory.CreateIntSet();
        public static int[] ProjectorLensDust = ItemFactory.CreateIntSet();
        public static string[] ProjectorLensTexture = ItemFactory.CreateCustomSet(string.Empty);
        public static SoundStyle[] ProjectorLensSound = ItemFactory.CreateCustomSet(default(SoundStyle));
        public static Action<ProjectorTileEntity>[] ProjectorLensPreOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);
        public static Action<ProjectorTileEntity>[] ProjectorLensOrderedUpdateFunction = ItemFactory.CreateCustomSet<Action<ProjectorTileEntity>>(null);

        public static bool[] RayAnchorTiles = TileFactory.CreateBoolSet();
        public static bool[] DrawWindSwayTiles = TileFactory.CreateBoolSet(false);

    }
}
