using Radiance.Content.Items.BaseItems;
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

            RadianceSets.ProjectorLensID[Type] = (int)ProjectorLensID.Flareglass;
            RadianceSets.ProjectorLensDust[Type] = DustID.GoldFlame;
            RadianceSets.ProjectorLensTexture[Type] = Texture + "_Transmutator";
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 4);
            Item.rare = ItemRarityID.Blue;
        }
        private Vector2 offset = Vector2.Zero;

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = CommonItemGroups.Gems;
            recipe.requiredRadiance = 10;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            foreach (Item item in Main.item)
            {
                if (item.active && item.ModItem is BaseContainer container && item.Distance(Item.Center) < 100)
                {
                    float adjustedDist = (100f - item.Distance(Item.Center)) / 18;
                    if (Main.GameUpdateCount % 4 == 0)
                        offset = Main.rand.NextVector2Circular(-adjustedDist, adjustedDist);

                    spriteBatch.Draw(tex, Item.Center + offset - Main.screenPosition, null, Color.White * 0.3f, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
                    spriteBatch.Draw(tex, Item.Center + -offset - Main.screenPosition, null, Color.White * 0.3f, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
                }
            }
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White * 0.9f, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
            return false;
        }
    }
}