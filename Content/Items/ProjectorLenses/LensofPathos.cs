namespace Radiance.Content.Items.ProjectorLenses
{
    public class LensofPathos : ModItem, IProjectorLens
    {
        public ProjectorLensID ID => ProjectorLensID.Pathos;
        public int DustID => Terraria.ID.DustID.CrimsonTorch;
        public string LensTexture => Texture + "_Transmutator";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lens of Pathos");
            Tooltip.SetDefault("Allows you to perform transmutations involving the essence of emotions when slotted into a Projector");
            Item.ResearchUnlockCount = 1;
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