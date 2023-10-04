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
        public SturdyFishingSpearProjectile() : base("Radiance/Content/Items/Tools/FishingSpears/SturdyFishingSpear", ModContent.ItemType<SturdyFishingSpear>(), 10) { }
        public override List<FishingSpearPart> SetupParts()
        {
            return new List<FishingSpearPart>()
            {
                new FishingSpearPart(FishingSpearPart.FishingSpearPartType.Shaft, Vector2.UnitY * 11, Vector2.UnitY * 33),
                new FishingSpearPart(FishingSpearPart.FishingSpearPartType.Light, Vector2.UnitY * -36, color: Color.White),
                new FishingSpearPart(FishingSpearPart.FishingSpearPartType.Hook, new Vector2(-7, -42), Vector2.One * 4),
                new FishingSpearPart(FishingSpearPart.FishingSpearPartType.Hook, new Vector2(7, -42), Vector2.One * 4, true),
                new FishingSpearPart(FishingSpearPart.FishingSpearPartType.Extra, Vector2.UnitY * -48),
            };
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sturdy Fishing Spear");
        }

        public override void SetExtraDefaults() 
        { 
        
        }

    }
}