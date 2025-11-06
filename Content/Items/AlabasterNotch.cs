using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Content.Items
{
    public class AlabasterNotch : BaseNotch
    {
        public AlabasterNotch() : base(new Color(166, 255, 227)) 
        {
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alabaster Notch");
            Tooltip.SetDefault("Placeholder Text");
            Item.ResearchUnlockCount = 0;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
        public override void MirrorUse()
        {

        }

        public override int RadianceCost(LookingGlass lookingGlass, int identicalCount)
        {
            return 10;
        }
    }
}