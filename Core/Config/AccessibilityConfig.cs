using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Radiance.Core.Config
{
    [BackgroundColor(158, 104, 51, 150)]
    public partial class AccessibilityConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(false)]
        [Label("Shaped Radiance I/O")]
        [Tooltip("While true, the shapes of Radiance input and output displays on tiles will be changed from identical circles in order to help those who may have trouble differentiating them.\nTriangles are inputs, while square are outputs.")]
        public bool ColorblindMode;

        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(typeof(Color), "0, 149, 230, 255")]
        [Label("Radiance Input Color")]
        [Tooltip("Determines the color that Apparatus Radiance input indicators will be.")]
        public Color radianceInputColor;

        [BackgroundColor(158, 121, 51, 0)]
        [DefaultValue(typeof(Color), "207, 46, 38, 255")]
        [Label("Radiance Output Color")]
        [Tooltip("Determines the color that Apparatus Radiance output indicators will be.")]
        public Color radianceOutputColor;
    }
}