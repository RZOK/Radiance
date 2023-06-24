using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.LightArrays
{
    public class TinyLightArray : BaseLightArray
    {
        public TinyLightArray() : base(13) { }
        public override string Texture => "Radiance/Content/Items/LightArrays/PrimitiveLightArray";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tiny Light Array Tablet");
            Tooltip.SetDefault("Holds five items within itself\nRight click to open the tablet's inventory");
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