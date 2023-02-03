using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.RadianceCells
{
    public class OverchargedRadianceCell : BaseContainer
    {
        public override Texture2D RadianceAdjustingTexture => ModContent.Request<Texture2D>("Radiance/Content/Items/RadianceCells/OverchargedRadianceCellGlow").Value;
        public override float MaxRadiance => 125;
        public override ContainerModeEnum ContainerMode => ContainerModeEnum.InputOutput;
        public override ContainerQuirkEnum ContainerQuirk => ContainerQuirkEnum.Absorbing;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overcharging Radiance Cell");
            Tooltip.SetDefault("Stores an ample amount of Radiance\nAbsorbed resources produce 25% more Radiance than usual");
            SacrificeTotal = 1;
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