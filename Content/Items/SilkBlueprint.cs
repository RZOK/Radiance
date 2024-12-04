using Microsoft.Xna.Framework.Input;
using Radiance.Core.Config;
using Radiance.Core.Loaders;

namespace Radiance.Content.Items
{
    public class SilkBlueprint : ModItem, IPlayerUIItem
    {
        public AutoloadedBlueprint blueprint;
        public float progress = 0;

        public BlueprintRequirement requirement;
        public BlueprintRequirement condition;

        public string SlotTexture => "Radiance/Content/UI/BlueprintUI/BlueprintSlot";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Silkprint");
            Tooltip.SetDefault("Right Click to begin creating plans for an Apparatus");
            Item.ResearchUnlockCount = 3;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
        public override bool CanRightClick() => !Main.keyState.IsKeyDown(Keys.LeftShift) && !Main.keyState.IsKeyDown(Keys.RightShift);
        public override bool ConsumeItem(Player player) => false;
        public override void RightClick(Player player)
        {
            if (player.GetCurrentActivePlayerUIItem() != this)
            {
                player.ResetActivePlayerUI();
                player.SetCurrentlyActivePlayerUIItem(this);
            }
            else
                player.ResetActivePlayerUI();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Silk, 6)
                .AddTile(TileID.Loom)
                .Register();
        }

        public void OnOpen()
        {
            Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintSlotSeed = Main.rand.Next(10000);
            Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintUITimer = 0;
        }

        public void OnClose()
        {
            Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintUITimer = 0;
        }
    }
    public class BlueprintUIPlayer : ModPlayer
    {
        public int blueprintSlotSeed;
        public int blueprintUITimer;
        public const int BLUEPRINT_UI_TIMER_MAX = 150;

        public override void PostUpdateMiscEffects()
        {
            Item item = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem;
            if (RadianceConfig.ReducedMotion)
                blueprintUITimer = BLUEPRINT_UI_TIMER_MAX;
            else if (item is not null && (item.ModItem is SilkBlueprint || item.ModItem is BlueprintCase) && blueprintUITimer < BLUEPRINT_UI_TIMER_MAX)
                blueprintUITimer++;
        }
    }
}   