using Radiance.Core.Systems;

namespace Radiance.Content.Items.Accessories
{
    public class RingofFrugality : ModItem, ITransmutationRecipe
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Band of Frugality");
            Tooltip.SetDefault("Reduces the amount of Radiance consumed by Instruments by 15%");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 1, 10);
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadiancePlayer>().radianceDiscount += 0.15f;
        }

        public void AddTransmutationRecipe(TransmutationRecipe recipe)
        {
            recipe.inputItems = new int[] { ItemID.BandofRegeneration, ItemID.BandofStarpower };
            recipe.requiredRadiance = 200;
        }
    }
}