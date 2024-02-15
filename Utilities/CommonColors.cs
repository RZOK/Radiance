namespace Radiance.Utilities
{
    public static class CommonColors
    {
        public static readonly Color RadianceColor1 = new(255, 192, 66);
        public static readonly Color RadianceColor2 = new(200, 150, 60);

        public static readonly Color LockedColor = new(150, 150, 150);
        public static readonly Color ContextColor = new(63, 222, 177);
        public static readonly Color EncycloradiaHoverColor = new(0, 255, 234);
        public static readonly Color EncycloradiaHiddenColor = new(165, 120, 191);

        public static readonly Color InfluencingColor = new (255, 0, 103);
        public static readonly Color TransmutationColor = new (103, 255, 0);
        public static readonly Color ApparatusesColor = new(0, 103, 255);
        public static readonly Color InstrumentsColor = new(255, 103, 0);
        public static readonly Color PedestalworksColor = new (103, 0, 255);
        public static readonly Color PhenomenaColor = new(0, 255, 103);

        public static readonly Color ScarletColor = new(255, 74, 90);
        public static readonly Color CeruleanColor = new(97, 200, 255);
        public static readonly Color VerdantColor = new(105, 255, 105);
        public static readonly Color MauveColor = new(204, 110, 255);

        public static Color GetDarkColor(this Color color, float divisor = 5) => new Color((int)(color.R / divisor), (int)(color.G / divisor), (int)(color.B / divisor), color.A);
    }
}
