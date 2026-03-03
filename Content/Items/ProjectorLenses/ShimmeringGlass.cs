using Radiance.Content.Tiles.Transmutator;
using Radiance.Core.Systems;

namespace Radiance.Content.Items.ProjectorLenses
{
    public class ShimmeringGlass : ModItem, ITransmutationRecipe
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flareglass");
            Tooltip.SetDefault("'Glimmers in the light'");
            Item.ResearchUnlockCount = 20;

            ProjectorLensData.AddProjectorLensData(Name, Type, DustID.GoldFlame, PreDrawLens);
        }
        private void PreDrawLens(ProjectorTileEntity entity, SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            Texture2D lensTex = ModContent.Request<Texture2D>(Texture + "_Transmutator").Value;
            Main.spriteBatch.Draw(lensTex, position, null, color, 0, lensTex.Size() / 2, 1, SpriteEffects.None, 0);
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 4);
            Item.rare = ItemRarityID.Blue;
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = CommonItemGroups.Gems;
            recipe.requiredRadiance = 10;
        }
    }
}