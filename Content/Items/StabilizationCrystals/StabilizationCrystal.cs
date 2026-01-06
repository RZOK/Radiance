using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.Materials;
using Radiance.Core.Systems;

namespace Radiance.Content.Items.StabilizationCrystals
{
    public class StabilizationCrystal : BaseStabilizationCrystal, ITransmutationRecipe
    {
        public StabilizationCrystal() : base(
            "Radiance/Content/Items/StabilizationCrystals/StabilizationCrystalPlaced",
            DustID.BlueCrystalShard,
            10,
            15,
            StabilizeType.Basic,
            new Color(0, 150, 255)
            )
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stabilization Crystal");
            Tooltip.SetDefault("Stabilizes nearby Apparatuses");
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 22;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 3);
            Item.rare = ItemRarityID.Blue;
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { ModContent.ItemType<PetrifiedCrystal>() };
            recipe.requiredRadiance = 20;
            recipe.inputStack = 5;
        }
    }
}