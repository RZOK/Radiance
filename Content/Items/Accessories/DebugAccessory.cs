using Radiance.Core.Systems;

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
            RadianceTransferSystem.rays.Clear();
            RadianceTransferSystem.byPosition.Clear();
            return true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadiancePlayer>().debugMode = true;
            player.GetModPlayer<RadiancePlayer>().canSeeRays = true;
            if (TileEntitySystem.orderedEntities is not null)
            {
                foreach (ImprovedTileEntity ite in TileEntitySystem.orderedEntities)
                {
                    if (ite.TileEntityWorldCenter().Distance(player.Center) < 100)
                        ite.AddHoverUI();
                }
            }
        }
    }
}