using Radiance.Core.Loaders;
using Terraria.ModLoader.Config;

namespace Radiance.Content.Items
{
    public class SilkBlueprint : ModItem, IPlayerUIItem
    {
        public AutoloadedBlueprint blueprint;
        public float progress = 0;

        public BlueprintRequirement requirement;
        public BlueprintRequirement condition;

        public string SlotTexture => "Radiance/Content/UI/Blueprints/BlueprintSlot";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Silk Blueprint");
            Tooltip.SetDefault("Right Click to begin creating plans for an Apparatus");
            Item.ResearchUnlockCount = 3;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
        public override bool CanRightClick() => true;
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

        public void OnOpen()
        {
            Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintSlotSeed = Main.rand.Next(10000);
            Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintUITimer = 0;
        }

        public void OnClear()
        {
            Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintUITimer = 0;
        }
    }
    public class BlueprintUIPlayer : ModPlayer
    {
        public int blueprintSlotSeed;
        public int blueprintUITimer;
        public const int BLUEPRINT_UI_TIMER_MAX = 120;

        public override void PostUpdateMiscEffects()
        {
            Item item = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem;
            if (item is not null && item.ModItem is SilkBlueprint && blueprintUITimer < BLUEPRINT_UI_TIMER_MAX)
                blueprintUITimer++;
        }
    }
}   