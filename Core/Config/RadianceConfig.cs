using System.ComponentModel;
using System.Text.Json.Serialization;
using Terraria.ModLoader.Config;

namespace Radiance.Core.Config
{
    [BackgroundColor(181, 113, 25, 100)]
    public partial class RadianceConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [BackgroundColor(255, 130, 40, 0)]
        [DefaultValue(true)]
        [Label("Enable Vine Sway")]
        [Tooltip("While false, any Radiance tiles that sway in the wind (banners, vines) will no longer do such. Minimal performance impact as long as you don't have too many swaying tiles placed in the world.")]
        public bool EnableVineSway;
    }
}