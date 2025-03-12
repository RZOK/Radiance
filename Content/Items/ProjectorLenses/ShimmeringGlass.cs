using Radiance.Core.Systems;

namespace Radiance.Content.Items.ProjectorLenses
{
    public class ShimmeringGlass : ModItem, ITransmutationRecipe
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flareglass");
            Tooltip.SetDefault("'Glimmers in the light'");
            Item.ResearchUnlockCount = 20;

            RadianceSets.ProjectorLensID[Type] = (int)ProjectorLensID.Flareglass;
            RadianceSets.ProjectorLensDust[Type] = DustID.GoldFlame;
            RadianceSets.ProjectorLensTexture[Type] = Texture + "_Transmutator";
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 4);
            Item.rare = ItemRarityID.Blue;
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = CommonItemGroups.Gems;
            recipe.requiredRadiance = 10;
        }
    }
}