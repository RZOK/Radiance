using Radiance.Content.Items.BaseItems;
using Terraria.ID;

namespace Radiance.Content.Items.RadianceCells
{
    public class StandardRadianceCell : BaseContainer
    {
        private float maxRadiance = 50000;
        private float currentRadiance = 5000;
        public override float MaxRadiance
        {
            get => maxRadiance;
            set => maxRadiance = value;
        }
        public override float CurrentRadiance
        {
            get => currentRadiance;
            set => currentRadiance = value;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Standard Radiance Cell");
            Tooltip.SetDefault("Stores an ample amount of Radiance");
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 26;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }
    }
}