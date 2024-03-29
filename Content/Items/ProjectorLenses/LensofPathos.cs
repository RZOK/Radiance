namespace Radiance.Content.Items.ProjectorLenses
{
    public class LensofPathos : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lens of Pathos");
            Tooltip.SetDefault("Allows you to perform transmutations involving the essence of emotions when slotted into a Projector");
            Item.ResearchUnlockCount = 1;

            RadianceSets.ProjectorLensID[Type] = (int)ProjectorLensID.Pathos;
            RadianceSets.ProjectorLensDust[Type] = DustID.CrimsonTorch;
            RadianceSets.ProjectorLensTexture[Type] = Texture + "_Transmutator";
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 1, 0);
            Item.rare = ItemRarityID.Pink;
        }
    }
}