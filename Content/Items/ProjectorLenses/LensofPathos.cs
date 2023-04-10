using Radiance.Core.Interfaces;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.ProjectorLenses
{
    public class LensofPathos : ModItem, IProjectorLens
    {
        ProjectorLensID IProjectorLens.ID => ProjectorLensID.Pathos;
        int IProjectorLens.DustID => DustID.CrimsonTorch;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 1, 0);
            Item.rare = ItemRarityID.Pink;
        }
    }
}