using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Content.Items
{
    public class AlabasterNotch : ModItem
    {
        public override void Load()
        {
           
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alabaster Notch");
            Tooltip.SetDefault("Placeholder Text");
            Item.ResearchUnlockCount = 0;

            LookingGlassNotchData.LoadNotchData
               (
               Type,
               new Color(166, 255, 227),
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Pylon",
               $"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass/LookingGlass_Pylon_Small",
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

        }

        public int RadianceCost(Player player, LookingGlass lookingGlass, int identicalCount)
        {
            return 10;
        }
    }
}