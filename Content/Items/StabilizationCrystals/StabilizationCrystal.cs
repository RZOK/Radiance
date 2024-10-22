using Radiance.Content.Items.Materials;
using Radiance.Core.Systems;

namespace Radiance.Content.Items.StabilizationCrystals
{
    public class StabilizationCrystal : ModItem, IStabilizationCrystal, ITransmutationRecipe
    {
        public string PlacedTexture => "Radiance/Content/Items/StabilizationCrystals/StabilizationCrystalPlaced";
        public int DustID => Terraria.ID.DustID.BlueCrystalShard;
        public int StabilizationRange => 10;
        public int StabilizationLevel => 15;
        public StabilizeType StabilizationType => StabilizeType.Basic;
        public Color CrystalColor => new Color(0, 150, 255);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stabilization Crystal");
            Tooltip.SetDefault("Stabilizes nearby Apparatuses when placed atop a Stabilization Apparatus");
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