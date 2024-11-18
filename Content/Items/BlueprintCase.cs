using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Particles;
using Radiance.Core.Loaders;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Tile_Entities;
using Terraria.ModLoader.Config;

namespace Radiance.Content.Items
{
    public class BlueprintCase : ModItem, IPlayerUIItem
    {
        public string SlotTexture => "Radiance/Content/UI/BlueprintUI/BlueprintCaseSlot";
        public BlueprintData selectedData = null;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blueprint Case");
            Tooltip.SetDefault("Stores completed blueprints");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 30;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
        }
        public override bool CanUseItem(Player player)
        {
            return selectedData is not null;
        }
        public override void UpdateInventory(Player player)
        {
            if (selectedData is not null)
                Item.createTile = selectedData.tileType;
            else
                Item.createTile = -1;
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
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string itemString;
            if (selectedData is not null)
            {
                Item item = GetItem(selectedData.tileItemType);
                itemString = $"{ItemRarityHex(item)}:{item.Name}";
            }
            else
                itemString = $"666666:None";
            TooltipLine blueprintTileLine = new TooltipLine(Mod, "CurrentBlueprint", $"Currently selected schematic: [c/{itemString}]"); //todo: convert to localizedtext
            tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria") + 1, blueprintTileLine);
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
}