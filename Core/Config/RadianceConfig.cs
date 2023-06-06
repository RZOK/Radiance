using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Radiance.Core.Config
{
    public partial class RadianceConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [DefaultValue(true)]
        [Label("Enable Vine Sway")]
        [Tooltip("While false, any Radiance tiles that sway in the wind (banners, vines) will no longer do such. Minimal performance impact as long as you don't have too many swaying tiles placed in the world.")]
        public bool EnableVineSway;
    }
}