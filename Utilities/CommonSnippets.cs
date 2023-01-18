using Radiance.Core;
using static Radiance.Utilities.CommonColors;

namespace Radiance.Utilities
{
    static partial class RadianceUtils
    {
        public static CustomTextSnippet radianceSnippet = new CustomTextSnippet("Radiance", RadianceColor1, RadianceColorDark);
        public static CustomTextSnippet radianceSnippetPeriod = new CustomTextSnippet("Radiance.", RadianceColor1, RadianceColorDark);

        public static CustomTextSnippet influencingSnippet = new CustomTextSnippet("Influencing", InfluencingColor, InfluencingColorDark);
        public static CustomTextSnippet influencingSnippetPeriod = new CustomTextSnippet("Influencing.", InfluencingColor, InfluencingColorDark);
        public static CustomTextSnippet transmutationSnippet = new CustomTextSnippet("Transmutation", TransmutationColor, TransmutationColorDark);
        public static CustomTextSnippet transmutationSnippetPeriod = new CustomTextSnippet("Transmutation.", TransmutationColor, TransmutationColorDark);
        public static CustomTextSnippet apparatusesSnippet = new CustomTextSnippet("Apparatuses", ApparatusesColor, ApparatusesColorDark);
        public static CustomTextSnippet apparatusesSnippetPeriod = new CustomTextSnippet("Apparatuses.", ApparatusesColor, ApparatusesColorDark);
        public static CustomTextSnippet instrumentsSnippet = new CustomTextSnippet("Instruments", InstrumentsColor, InstrumentsColorDark);
        public static CustomTextSnippet instrumentsSnippetPeriod = new CustomTextSnippet("Instruments.", InstrumentsColor, InstrumentsColorDark);
        public static CustomTextSnippet pedestalworksSnippet = new CustomTextSnippet("Pedestalworks", PedestalworksColor, PedestalworksColorDark);
        public static CustomTextSnippet pedestalworksSnippetPeriod = new CustomTextSnippet("Pedestalworks.", PedestalworksColor, PedestalworksColorDark);
        public static CustomTextSnippet phenomenaSnippet = new CustomTextSnippet("Phenomena", PhenomenaColor, PhenomenaColorDark);
        public static CustomTextSnippet phenomenaSnippetPeriod = new CustomTextSnippet("Phenomena.", PhenomenaColor, PhenomenaColorDark);
    }
}
