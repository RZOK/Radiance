using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Core;
using Radiance.Core.Systems;
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
            Item.width = 22;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;
        }
        public override void AddRecipes()
        {
            //Two recipes for letting people use their now-obsolete poor cells
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PoorRadianceCell>(), 1)
                .AddIngredient(ModContent.ItemType<ShimmeringGlass>(), 1)
                .AddIngredient(ModContent.ItemType<PetrifiedCrystal>(), 6)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 5)
                .AddIngredient(ModContent.ItemType<ShimmeringGlass>(), 1)
                .AddIngredient(ModContent.ItemType<PetrifiedCrystal>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}