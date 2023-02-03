using Microsoft.Xna.Framework;
using Radiance.Core;
using static Radiance.Utilities.CommonColors;

namespace Radiance.Utilities
{
    static class CommonSnippets
    {
        public static CustomTextSnippet radianceSnippet = new CustomTextSnippet("Radiance", RadianceColor1, RadianceColor1.GetDarkColor());
        public static CustomTextSnippet radianceSnippetPeriod = new CustomTextSnippet("Radiance.", RadianceColor1, RadianceColor1.GetDarkColor());

        public static CustomTextSnippet influencingSnippet = new CustomTextSnippet("Influencing", InfluencingColor, InfluencingColor.GetDarkColor());
        public static CustomTextSnippet influencingSnippetPeriod = new CustomTextSnippet("Influencing.", InfluencingColor, InfluencingColor.GetDarkColor());
        public static CustomTextSnippet transmutationSnippet = new CustomTextSnippet("Transmutation", TransmutationColor, TransmutationColor.GetDarkColor());
        public static CustomTextSnippet transmutationSnippetPeriod = new CustomTextSnippet("Transmutation.", TransmutationColor, TransmutationColor.GetDarkColor());
        public static CustomTextSnippet apparatusesSnippet = new CustomTextSnippet("Apparatuses", ApparatusesColor, ApparatusesColor.GetDarkColor());
        public static CustomTextSnippet apparatusesSnippetPeriod = new CustomTextSnippet("Apparatuses.", ApparatusesColor, ApparatusesColor.GetDarkColor());
        public static CustomTextSnippet instrumentsSnippet = new CustomTextSnippet("Instruments", InstrumentsColor, InstrumentsColor.GetDarkColor());
        public static CustomTextSnippet instrumentsSnippetPeriod = new CustomTextSnippet("Instruments.", InstrumentsColor, InstrumentsColor.GetDarkColor());
        public static CustomTextSnippet pedestalworksSnippet = new CustomTextSnippet("Pedestalworks", PedestalworksColor, PedestalworksColor.GetDarkColor());
        public static CustomTextSnippet pedestalworksSnippetPeriod = new CustomTextSnippet("Pedestalworks.", PedestalworksColor, PedestalworksColor.GetDarkColor());
        public static CustomTextSnippet phenomenaSnippet = new CustomTextSnippet("Phenomena", PhenomenaColor, PhenomenaColor.GetDarkColor());
        public static CustomTextSnippet phenomenaSnippetPeriod = new CustomTextSnippet("Phenomena.", PhenomenaColor, PhenomenaColor.GetDarkColor());

        public static CustomTextSnippet BWSnippet(string text) => new(text, Color.White, Color.Black);
    }
}
