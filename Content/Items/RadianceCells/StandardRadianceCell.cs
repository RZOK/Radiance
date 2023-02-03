using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.RadianceCells
{
    public class StandardRadianceCell : BaseContainer
    {
        public override Texture2D RadianceAdjustingTexture => ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/StandardRadianceCellGlow").Value;
        public override float MaxRadiance => 4000;
        public override ContainerModeEnum ContainerMode => ContainerModeEnum.InputOutput;
        public override ContainerQuirkEnum ContainerQuirk => ContainerQuirkEnum.Standard;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Standard Radiance Cell");
            Tooltip.SetDefault("Stores an ample amount of Radiance");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }
    }
}