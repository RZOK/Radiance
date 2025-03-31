using Microsoft.Xna.Framework.Input;
using Radiance.Core.Loaders;
using Terraria.UI;

namespace Radiance.Content.Items
{
    public class BlueprintCase : ModItem, IPlayerUIItem
    {
        public string SlotTexture => "Radiance/Content/UI/BlueprintUI/BlueprintCaseSlot";
        public BlueprintData selectedData = null;
        public override void Load()
        {
            On_ItemSlot.RightClick_ItemArray_int_int += AddBlueprintToCase;
        }

        private void AddBlueprintToCase(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Main.mouseRight && Main.mouseRightRelease && !inv[slot].IsAir && !Main.LocalPlayer.ItemAnimationActive)
            {
                if (!Main.mouseItem.IsAir && Main.mouseItem.ModItem is AutoloadedBlueprint blueprint && !Main.LocalPlayer.GetModPlayer<BlueprintPlayer>().knownBlueprints.Contains(blueprint.blueprintData))
                {
                    Main.LocalPlayer.GetModPlayer<BlueprintPlayer>().knownBlueprints.Add(blueprint.blueprintData);
                    Main.mouseItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                    return;
                }
            }
            orig(inv, context, slot);
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Silkprint Case");
            Tooltip.SetDefault("Stores and allows placement of completed silkprints\nRight click a silkprint over the case to add it");
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
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.autoReuse = false;
                Item.useStyle = ItemUseStyleID.Swing;
                Item.createTile = -1;
                return true;
            }
            else
            {
                Item.autoReuse = true;
                Item.useAnimation = 15;
                Item.useTime = 10;
                Item.useStyle = ItemUseStyleID.Swing;
                if (selectedData is not null)
                {
                    Item.createTile = selectedData.TileType;
                    AssemblableTileEntity entity = selectedData.tileEntity;
                    int[] items = entity.stageMaterials[0].items;
                    Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
                    int amountLeft = entity.stageMaterials[0].stack;
                    for (int i = 0; i < 58; i++)
                    {
                        if (items.Contains(player.inventory[i].type))
                        {
                            slotsToPullFrom.Add(i, Math.Min(amountLeft, player.inventory[i].stack));
                            amountLeft -= Math.Clamp(amountLeft, 0, player.inventory[i].stack);
                        }
                    }
                    if (amountLeft == 0)
                        return true;
                }
            }
            return false;
        }
        public override bool? UseItem(Player player)
        {
            if(player.altFunctionUse == 2 && player.ItemAnimationJustStarted)
            {
                if(player.GetCurrentActivePlayerUIItem() != this)
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                    player.ResetActivePlayerUI();
                    player.SetCurrentlyActivePlayerUIItem(this);
                }
                if(!Main.playerInventory)
                {
                    Main.playerInventory = true;
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
                return true;
            }
            return base.UseItem(player);
        }
        public override void UpdateInventory(Player player)
        {
            if (!player.ItemAnimationActive) // this exists because otherwise there will be no tile display after player right clicks to open ui
            {
                if (selectedData is not null)
                    Item.createTile = selectedData.TileType;
                else
                    Item.createTile = -1;
            }
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
            // don't like to display it !
            TooltipLine placeableLine = tooltips.Find(x => x.Name == "Placeable");
            placeableLine?.Hide();

            string itemString;
            if (selectedData is not null)
            {
                Item item = GetItem(selectedData.tileItemType);
                itemString = $"{ItemRarityHex(item)}:{item.Name}";
            }
            else
                itemString = $"{CommonColors.LockedColor.Hex3()}:None";

            TooltipLine blueprintTileLine = new TooltipLine(Mod, "CurrentBlueprint", $"Currently selected schematic: [c/{itemString}]"); //todo: convert to localizedtext
            tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip0" && x.Mod == "Terraria") + 1, blueprintTileLine);
            
            if (selectedData is not null)
            {
                int itemType = selectedData.tileEntity.GetShiftingItemAtTier(0);
                string requirementString = $"[i/s{selectedData.tileEntity.stageMaterials[0].stack}:{itemType}]";
                if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
                    requirementString = $"[c/{ItemRarityHex(itemType)}:{GetItem(itemType).Name}] x{selectedData.tileEntity.stageMaterials[0].stack}";

                TooltipLine blueprintRequirementsLine = new TooltipLine(Mod, "MaterialRequirements", $"Required materials: {requirementString}");
                tooltips.Insert(tooltips.FindIndex(x => x.Name == "CurrentBlueprint" && x.Mod == nameof(Radiance)) + 1, blueprintRequirementsLine);
            }
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
}