using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Microsoft.Xna.Framework.Input;
using Radiance.Content.Particles;
using Radiance.Core.Loaders;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Tile_Entities;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace Radiance.Content.Items
{
    public class BlueprintCase : ModItem, IPlayerUIItem
    {
        public string SlotTexture => "Radiance/Content/UI/BlueprintUI/BlueprintCaseSlot";
        public BlueprintData selectedData = null;
        public override void Load()
        {
            On_ItemSlot.RightClick_ItemArray_int_int += AddItemToImprint;
            On_Player.PlaceThing_Tiles_PlaceIt_ConsumeFlexibleWandMaterial += ConsumeMaterialsToPlace;
        }

        private void ConsumeMaterialsToPlace(On_Player.orig_PlaceThing_Tiles_PlaceIt_ConsumeFlexibleWandMaterial orig, Player self)
        {
            Item item = self.inventory[self.selectedItem];
            if (item.ModItem is BlueprintCase blueprintCase)
            {
                BlueprintData selectedData = blueprintCase.selectedData;
                if (selectedData is not null)
                {
                    AssemblableTileEntity entity = selectedData.tileEntity;
                    int typeToConsume = entity.StageMaterials[0].item;
                    Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
                    int amountLeft = entity.StageMaterials[0].stack;
                    for (int i = 0; i < 58; i++)
                    {
                        if (self.inventory[i].type == typeToConsume)
                        {
                            slotsToPullFrom.Add(i, Math.Min(amountLeft, self.inventory[i].stack)); ;
                            amountLeft -= Math.Clamp(amountLeft, 0, self.inventory[i].stack);
                            if (amountLeft == 0)
                            {
                                foreach (var slot in slotsToPullFrom)
                                {
                                    //TODO: add consumed items for TE creation to itemsConsumed 
                                    //itemsConsumed[slot.Key] = slot.Value;
                                    self.inventory[slot.Key].stack -= slotsToPullFrom[slot.Key];
                                    if (self.inventory[slot.Key].stack <= 0)
                                        self.inventory[slot.Key].TurnToAir();
                                }
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        private void AddItemToImprint(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
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
            DisplayName.SetDefault("Blueprint Case");
            Tooltip.SetDefault("Stores completed blueprints and allows you to place them\nRight Click a completed blueprint over the case to permanently add it");
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
            if (selectedData is not null)
            {
                AssemblableTileEntity entity = selectedData.tileEntity;
                int item = entity.StageMaterials[0].item;
                Dictionary<int, int> slotsToPullFrom = new Dictionary<int, int>();
                int amountLeft = entity.StageMaterials[0].stack;
                for (int i = 0; i < 58; i++)
                {
                    if (player.inventory[i].type == item)
                    {
                        slotsToPullFrom.Add(i, Math.Min(amountLeft, player.inventory[i].stack));
                        amountLeft -= Math.Clamp(amountLeft, 0, player.inventory[i].stack);
                    }
                }
                if (amountLeft == 0)
                    return true;
            }
            return false;
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
            tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip1" && x.Mod == "Terraria") + 1, blueprintTileLine);
            
            if (selectedData is not null)
            {
                TooltipLine blueprintRequirementsLine = new TooltipLine(Mod, "MaterialRequirements", $"Required Materials: [i/s{selectedData.tileEntity.StageMaterials[0].stack}:{selectedData.tileEntity.StageMaterials[0].item}]");
                tooltips.Insert(tooltips.FindIndex(x => x.Name == "Tooltip1" && x.Mod == "Terraria") + 2, blueprintRequirementsLine);
            }
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