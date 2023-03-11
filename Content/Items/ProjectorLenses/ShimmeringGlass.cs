using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.ProjectorLenses
{
    public class ShimmeringGlass : ModItem, IProjectorLens
    {
        ProjectorLensID IProjectorLens.ID => ProjectorLensID.Flareglass;
        int IProjectorLens.DustID => DustID.GoldFlame;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flareglass");
            Tooltip.SetDefault("'Glimmers in the light'");
            SacrificeTotal = 20;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 4);
            Item.rare = ItemRarityID.Blue;
        }
        Vector2 offset = Vector2.Zero;
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