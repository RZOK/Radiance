using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.RadianceCells
{
    public class OverchargedRadianceCell : BaseContainer
    {
        public OverchargedRadianceCell() : base(
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/OverchargedRadianceCellGlow").Value,
            ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/StandardRadianceCellMini").Value,
            1000,
            ContainerMode.InputOutput,
            ContainerQuirk.Absorbing)
        { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overcharging Radiance Cell");
            Tooltip.SetDefault("Absorbed resources produce 25% more Radiance than usual");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Lens, 2)
                .AddIngredient(ItemID.Glass, 4)
                .AddIngredient(ItemID.FallenStar, 2)
                .AddRecipeGroup(RecipeGroupID.IronBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}