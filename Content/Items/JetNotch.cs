using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Content.Items
{
    public class JetNotch : BaseNotch
    {
        public JetNotch() : base(new Color(193, 94, 255)) 
        {
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jet Notch");
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