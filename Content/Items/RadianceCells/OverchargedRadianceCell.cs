using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.RadianceCells
{
    public class OverchargedRadianceCell : BaseContainer
    {
        public OverchargedRadianceCell() : base(
            new Dictionary<BaseContainer_TextureType, string>() 
            {
                [BaseContainer_TextureType.Mini] = "Radiance/Content/Items/RadianceCells/OverchargedRadianceCellMini",
                [BaseContainer_TextureType.RadianceAdjusting] = "Radiance/Content/Items/RadianceCells/OverchargedRadianceCellGlow"
            },
            125,
            true,
            .25f) 
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overcharging Radiance Cell");
            Tooltip.SetDefault("Converted resources produce 25% more Radiance than usual");
            Item.ResearchUnlockCount = 1;
            RadianceSets.SetPedestalStability[Type] = BASE_CONTAINER_REQUIRED_STABILITY;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }
    }
}