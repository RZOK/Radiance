namespace Radiance.Core
{
    public static class RadianceSets
    {
        public static string[] EncycloradiaRelatedEntry = ItemID.Sets.Factory.CreateCustomSet(string.Empty);
        public static Func<Item[], int, bool>[] RightClickMouseItemFunction = ItemID.Sets.Factory.CreateCustomSet<Func<Item[], int, bool>>(null);
        public static float[] SetPedestalStability = ItemID.Sets.Factory.CreateFloatSet(0);
        public static (float Amount, float Speed)[] RadianceCellAbsorptionStats = ItemID.Sets.Factory.CreateCustomSet<(float, float)>((0, 0));

        public static bool[] RayAnchorTiles = TileID.Sets.Factory.CreateBoolSet();
        public static bool[] DrawWindSwayTiles = TileID.Sets.Factory.CreateBoolSet(false);
    }
}