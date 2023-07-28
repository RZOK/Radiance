
namespace Radiance.Content.Items.Tools.Pickaxes
{
    public class SubjugationPickaxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Subjugation Pickaxe");
            Tooltip.SetDefault("Enriches mined Demonite and Crimtane");
        }

        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Melee;
            Item.width = 26;
            Item.height = 26;
            Item.useTime = 15;
            Item.useAnimation = 19;
            Item.pick = 100;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3;
            Item.value = Item.sellPrice(0, 0, 70, 0);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.tileBoost += 1;
            Item.scale = 1.15f;
        }
    }
}