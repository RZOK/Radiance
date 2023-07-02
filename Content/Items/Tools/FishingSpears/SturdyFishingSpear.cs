using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.Tools.FishingSpears
{
    public class SturdyFishingSpear : BaseFishingSpear
    {
        public SturdyFishingSpear() : base(ModContent.ProjectileType<SturdyFishingSpearProjectile>(), 400)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sturdy Fishing Spear");
            Tooltip.SetDefault("");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetExtraDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.sellPrice(0, 0, 20, 0);
            Item.rare = ItemRarityID.Green;
        }
    }
    public class SturdyFishingSpearProjectile : BaseFishingSpearProjectile
    {
        public SturdyFishingSpearProjectile() : base("Radiance/Content/Items/Tools/FishingSpears/SturdyFishingSpear", ModContent.ItemType<SturdyFishingSpear>()) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sturdy Fishing Spear");
        }

        public override void SetExtraDefaults() 
        { 
        
        }
    }
}