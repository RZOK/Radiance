using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items
{
    public class EssenceOfFlight : ModItem, ITransmutationRecipe
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Manifested Sky");
            Tooltip.SetDefault("'Coalesced essence of the heavens'");
            Item.ResearchUnlockCount = 50;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 3, 0);
            Item.rare = ItemRarityID.Pink;
        }
        public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 150);
        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { Type };
            recipe.requiredRadiance = 40;
            recipe.outputItem = ItemID.SoulofFlight;
            recipe.unlock = UnlockSystem.UnlockBoolean.hardmode;
            recipe.outputStack = 2;
        }
    }
}