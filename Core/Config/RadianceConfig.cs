using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Radiance.Core.Config
{
    [BackgroundColor(158, 104, 51, 150)]
    public partial class RadianceConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(true)]
        [Label("Enable Vine Sway")]
        [Tooltip("While true, any Radiance tiles that can sway in the wind (banners, vines) will do so.\nMinimal performance impact as long as there is not a large number of swaying tiles placed in the world.")]
        public bool EnableVineSway;

        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(false)]
        [Label("Colorblind Mode")]
        [Tooltip("While true, the shapes of Radiance input and output displays on tiles will be changed from identical circles in order to help those who may have trouble differentiating them.\nTriangles are inputs, while square are outputs.")]
        public bool ColorblindMode;

        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(true)]
        [Label("Preload Assets")]
        [Tooltip("While true, a number of assets for the mod will be loaded into memory along with the mod loading.\nMay reduce mod loading times and memory usage in worlds that have not progressed as far yet, but certain sprites may flicker upon being initially displayed.")]
        public bool PreloadAssets;

        public enum SquareAOEDrawingMode
        {
            Standard,
            DetailsOnly,
            BoxOnly,
        }
        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(SquareAOEDrawingMode.Standard)]
        [Label("Area of Effect Square Style")]
        [Tooltip("Sets the style of drawing for square area-of-effect indicators.")]
        public SquareAOEDrawingMode SquareAOEDrawingModeConfig;

        [BackgroundColor(158, 121, 51, 0)]
        [Range(0f, 1f)]
        [Increment(.05f)]
        [DrawTicks]
        [DefaultValue(0.6f)]
        [Label("Area of Effect Indicator Alpha")]
        [Tooltip("Sets the alpha (transparency) of area-of-effect indicators. 0 will be fully transparent (not visible), while 1 will be entirely solid.")]
        public float AreaOfEffectAlpha;

        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(true)]
        [Label("Formation Core Adaptive Star Color")]
        [Tooltip("While true, the visual stars created by a Formation Core moving an item will adapt their color to the center color of the item.\nMinimal performance impact.")]
        public bool FormationCoreAdaptiveColoring;
    }
}