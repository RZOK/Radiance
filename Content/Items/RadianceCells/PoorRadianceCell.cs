using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.RadianceCells
{
    public class PoorRadianceCell : BaseContainer
    {
        #region Fields

        private float maxRadiance = 1000;
        private ContainerModeEnum containerMode = ContainerModeEnum.InputOutput;
        private ContainerQuirkEnum containerQuirk = ContainerQuirkEnum.Leaking;

        public Texture2D radianceAdjustingTexture = ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/PoorRadianceCellGlow").Value;

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
            DisplayName.SetDefault("Poor Radiance Cell");
            Tooltip.SetDefault("Stores an ample amount of Radiance");
        }
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 34;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
    }
}