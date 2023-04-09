using Microsoft.Xna.Framework;
using Radiance.Content.Particles;
using Radiance.Core;
using Radiance.Core.Systems;
using Radiance.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Items.Accessories
{
    public class DebugAccessory : ModItem
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Debug";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Accessory");
            Tooltip.SetDefault("Enables various debug features and information when equipped");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadiancePlayer>().debugMode = true;
            player.GetModPlayer<RadiancePlayer>().canSeeRays = true;
        }
    }
}