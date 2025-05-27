using Radiance.Content.Tiles.Transmutator;

namespace Radiance.Core
{
    public static class RadianceSets
    {

        public static string[] EncycloradiaRelatedEntry = ItemID.Sets.Factory.CreateCustomSet(string.Empty);
        public static Func<Item[], int, bool>[] RightClickMouseItemFunction = ItemID.Sets.Factory.CreateCustomSet<Func<Item[], int, bool>>(null);
        public static float[] SetPedestalStability = ItemID.Sets.Factory.CreateFloatSet(0);
        public static (float Amount, float Speed)[] RadianceCellAbsorptionStats = ItemID.Sets.Factory.CreateCustomSet<(float, float)>((0, 0));

        public static int[] ProjectorLensID = ItemID.Sets.Factory.CreateIntSet();
        public static int[] ProjectorLensDust = ItemID.Sets.Factory.CreateIntSet();
        public static string[] ProjectorLensTexture = ItemID.Sets.Factory.CreateCustomSet(string.Empty);
        public static SoundStyle[] ProjectorLensSound = ItemID.Sets.Factory.CreateCustomSet(default(SoundStyle));
        public static Action<ProjectorTileEntity>[] ProjectorLensPreOrderedUpdateFunction = ItemID.Sets.Factory.CreateCustomSet<Action<ProjectorTileEntity>>(null);
        public static Action<ProjectorTileEntity>[] ProjectorLensOrderedUpdateFunction = ItemID.Sets.Factory.CreateCustomSet<Action<ProjectorTileEntity>>(null);

        public static bool[] RayAnchorTiles = TileID.Sets.Factory.CreateBoolSet();
        public static bool[] DrawWindSwayTiles = TileID.Sets.Factory.CreateBoolSet(false);

    }
}
