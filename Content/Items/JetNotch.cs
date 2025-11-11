using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Content.Items
{
    public class JetNotch : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jet Notch");
            Tooltip.SetDefault("Placeholder Text");
            Item.ResearchUnlockCount = 0;
            LookingGlassNotchData.LoadNotchData
                (
                Type,
                new Color(193, 94, 255),
                $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Return",
                $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Return_Small",
                MirrorUse,
                RadianceCost
                );
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {
            player.DoPotionOfReturnTeleportationAndSetTheComebackPoint();
            lookingGlass.CreateRecallParticles(player);
        }

        public int RadianceCost(int identicalCount)
        {
            return 10;
        }
    }
}