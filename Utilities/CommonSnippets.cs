using Microsoft.Xna.Framework;
using Radiance.Core;
using static Radiance.Utilities.CommonColors;

namespace Radiance.Utilities
{
    static class CommonSnippets
    {
        public static CustomTextSnippet radianceSnippet = new CustomTextSnippet("Radiance", RadianceColor1, RadianceColor1.GetDarkColor());
        public static CustomTextSnippet hardLightSnippet = new CustomTextSnippet("HL", RadianceColor1, RadianceColor1.GetDarkColor());

        public static CustomTextSnippet[] coldHardLightSnippet = { new CustomTextSnippet("C", ColdHLColor, ColdHLColor.GetDarkColor()), hardLightSnippet };
        public static CustomTextSnippet[] searingHardLightSnippet = { new CustomTextSnippet("S", SearingHLColor, SearingHLColor.GetDarkColor()), hardLightSnippet };

        public static CustomTextSnippet BWSnippet(this string text) => new(text, Color.White, Color.Black);
        public static CustomTextSnippet DarkColorSnippet(this string text, Color color) => new(text, color, color.GetDarkColor());
    }
}
