using Radiance.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Tools.Misc
{
    public class AlchemicalLens : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alchemical Lens");
            Tooltip.SetDefault("Reveals the Influental Colors of potions in your inventory");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateInventory(Player player)
        {
            Main.LocalPlayer.GetModPlayer<RadiancePlayer>().alchemicalLens = true;
        }
    }
}