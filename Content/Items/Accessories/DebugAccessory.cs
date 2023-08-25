namespace Radiance.Items.Accessories
{
    public class DebugAccessory : ModItem
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Debug";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Accessory");
            Tooltip.SetDefault("Enables various debug features and information when equipped");
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.accessory = true;
        }
        public override bool? UseItem(Player player)
        {
            for (int i = 0; i < 4; i++)
            {
                Dictionary<int, int> map = new() 
                {
                    [0] = ItemID.CobaltBar,
                    [1] = ItemID.MythrilBar,
                    [2] = ItemID.AdamantiteBar,
                    [3] = ItemID.HallowedBar
                };
                int item = Item.NewItem(new EntitySource_ItemUse(player, Item), Main.MouseWorld - Vector2.UnitY * 16 * i, map[i]);
                Main.item[item].velocity *= 0f;
            }
            return true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadiancePlayer>().debugMode = true;
            player.GetModPlayer<RadiancePlayer>().canSeeRays = true;
        }
    }
}