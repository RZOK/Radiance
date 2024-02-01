using Radiance.Content.Items.BaseItems;

namespace Radiance.Content.Items.RadianceCells
{
    public class OverchargedRadianceCell : BaseContainer
    {
        public OverchargedRadianceCell() : base(
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/OverchargedRadianceCellGlow").Value,
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/OverchargedRadianceCellMini").Value,
            125,
            true,
            ContainerMode.InputOutput,
            1.25f)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overcharging Radiance Cell");
            Tooltip.SetDefault("Converted resources produce 25% more Radiance than usual");
            Item.ResearchUnlockCount = 1;
            RadianceSets.SetPedestalStability[Type] = 10;
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