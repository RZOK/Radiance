namespace Radiance.Content.Items.Materials
{
    public class Dynaglass : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dynaglass");
            Tooltip.SetDefault("'Softly thrumming'");
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 34;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 2, 50);
            Item.rare = ItemRarityID.Green;
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White * 0.9f, rotation, tex.Size() / 2, 1, SpriteEffects.None, 0);
            return false;
        }
    }
}