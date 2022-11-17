using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.RadianceCells
{
    public class FormationCore : BaseContainer
    {
        #region Fields

        private float maxRadiance = 10;
        private ContainerModeEnum containerMode = ContainerModeEnum.InputOutput;
        private ContainerQuirkEnum containerQuirk = ContainerQuirkEnum.CantAbsorb;

        public Texture2D radianceAdjustingTexture = null;

        #endregion

        #region Properties

#nullable enable
        public override Texture2D? RadianceAdjustingTexture
        {
            get => radianceAdjustingTexture;
            set => radianceAdjustingTexture = value;
        }
#nullable disable

        public override float MaxRadiance
        {
            get => maxRadiance;
            set => maxRadiance = value;
        }
        public override ContainerModeEnum ContainerMode
        {
            get => containerMode;
            set => containerMode = value;
        }
        public override ContainerQuirkEnum ContainerQuirk
        {
            get => containerQuirk;
            set => containerQuirk = value;
        }

        #endregion

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Formation Core");
            Tooltip.SetDefault("Warps nearby items when placed on a Pedestal\nItems will teleport to Pedestals that also have Formation Cores atop them that are linked with outputting rays");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Red;
        }
    }
}