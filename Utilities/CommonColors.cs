using Microsoft.Xna.Framework;

namespace Radiance.Utilities
{
    public static class CommonColors
    {
        public static readonly Color RadianceColor1 = new(255, 192, 66);
        public static readonly Color RadianceColor2 = new(200, 150, 60);

        public static readonly Color LockedColor = new(150, 150, 150);
        public static readonly Color ContextColor = new(73, 134, 214);

        public static readonly Color InfluencingColor = new (255, 0, 103);
        public static readonly Color TransmutationColor = new (103, 255, 0);
        public static readonly Color ApparatusesColor = new(0, 103, 255);
        public static readonly Color InstrumentsColor = new(255, 103, 0);
        public static readonly Color PedestalworksColor = new (103, 0, 255);
        public static readonly Color PhenomenaColor = new(0, 255, 103);

        public static readonly Color ScarletColor = new(245, 48, 66);
        public static readonly Color CeruleanColor = new(66, 171, 227);
        public static readonly Color VerdantColor = new(58, 179, 58);
        public static readonly Color MauveColor = new(166, 0, 255);

        public static readonly Color ColdHLColor = new(0, 255, 221);
        public static readonly Color SearingHLColor = new(255, 119, 0);

        public static Color GetDarkColor(this Color color, float divisor = 5) => new Color(color.R / divisor, color.G / divisor, color.B / divisor, color.A);
    }
}
