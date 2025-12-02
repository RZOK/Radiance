using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Content.Items
{
    public class AureolinNotch : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aureolin Notch");
            Tooltip.SetDefault("Placeholder Text");
            Item.ResearchUnlockCount = 0;
            LookingGlassNotchData.LoadNotchData
               (
               Type,
               new Color(255, 185, 102),
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Point",
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Point_Small",
               MirrorUse,
               ChargeCost
               );
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Orange;
        }
        public void MirrorUse(Player player, LookingGlass lookingGlass)
        {

        }

        public int ChargeCost(int identicalCount)
        {
            return 10;
        }
    }
}