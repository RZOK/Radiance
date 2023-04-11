using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.RadianceCells
{
    public class StandardRadianceCell : BaseContainer
    {
        public StandardRadianceCell() : base(
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/StandardRadianceCellGlow").Value,
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/StandardRadianceCellMini").Value,
            4000,
            ContainerMode.InputOutput,
            ContainerQuirk.Standard)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Standard Radiance Cell");
            Item.ResearchUnlockCount = 1;
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