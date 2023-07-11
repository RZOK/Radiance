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
    }
}