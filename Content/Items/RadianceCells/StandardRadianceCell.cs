using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.RadianceCells
{
    public class StandardRadianceCell : BaseContainer
    {
        #region Fields

        private float maxRadiance = 4000;
        private float currentRadiance = 0;
        private ContainerModeEnum containerMode = ContainerModeEnum.InputOutput;
        private ContainerQuirkEnum containerQuirk = ContainerQuirkEnum.Standard;

        public Texture2D radianceAdjustingTexture = ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/StandardRadianceCellGlow").Value;

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
        public override float CurrentRadiance
        {
            get => currentRadiance;
            set => currentRadiance = value;
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