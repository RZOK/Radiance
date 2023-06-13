using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.LightArrays
{
    public class PrimitiveLightArray : BaseLightArray
    {
        public PrimitiveLightArray() : base(24) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Primitive Light Array Tablet");
            Tooltip.SetDefault("Right click to open the tablet's inventory");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetExtraDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }
    }
}