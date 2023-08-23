using ReLogic.Graphics;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;

namespace Radiance.Content.Items
{
    public class ItemImprint : ModItem
    {
        public ItemImprintData imprintData;
        public override void Load()
        {
            On_ItemSlot.RightClick_ItemArray_int_int += ToggleAndAddItemToImprint;
        }

        public override void Unload()
        {
            On_ItemSlot.RightClick_ItemArray_int_int -= ToggleAndAddItemToImprint;
        }

        private void ToggleAndAddItemToImprint(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Main.mouseRight && Main.mouseRightRelease && !inv[slot].IsAir && !Main.LocalPlayer.ItemAnimationActive)
            {
                if (!Main.mouseItem.IsAir && Main.mouseItem.type == Type)
                {
                    string saveString = inv[slot].GetTypeOrFullNameFromItem();
                    if (Main.mouseItem.ModItem is ItemImprint itemImprint && !itemImprint.imprintData.imprintedItems.Contains(saveString))
                    {
                        if (inv[slot].type == Type && inv[slot].ModItem is ItemImprint hoveredImprint)
                            itemImprint.imprintData = hoveredImprint.imprintData;
                        else
                            itemImprint.imprintData.imprintedItems.Add(saveString);

                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }
                else if (inv[slot].type == Type && inv[slot].ModItem is ItemImprint itemImprint)
                {
                    if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
                    {
                        if (itemImprint.imprintData.imprintedItems.Any())
                        {
                            SoundEngine.PlaySound(SoundID.Grab);
                            if (itemImprint.imprintData.imprintedItems.Count == 1)
                            {
                                inv[slot].ChangeItemType(ModContent.ItemType<MemoryClay>());
                                Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().inventoryItemRightClickDelay = 10;
                                return;
                            }
                            else
                                itemImprint.imprintData.imprintedItems.Pop();
                        }
                    }
                    else
                    {
                        itemImprint.imprintData.blacklist = !itemImprint.imprintData.blacklist;
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }
            }
            orig(inv, context, slot);
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Item Imprint");
            Tooltip.SetDefault("Stores the imprints of items\nRight Click on a valid Apparatus to apply this imprint to it\nHold Shift and Right Click in your inventory to clear the most recently added item instead");
            Item.ResearchUnlockCount = 0;
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (imprintData.imprintedItems.AnyAndExists())
            {
                for (int i = 0; i < Math.Ceiling((float)imprintData.imprintedItems.Count / 16); i++)
                {
                    int realAmountToDraw = Math.Min(16, imprintData.imprintedItems.Count - i * 16);
                    TooltipLine itemDisplayLine = new(Mod, "ItemImprintItems" + i, "");
                    itemDisplayLine.Text = new String('M', 2 * realAmountToDraw + 3) + i;
                    tooltips.Add(itemDisplayLine);
                }
            }
        }
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (Main.SettingsEnabled_OpaqueBoxBehindTooltips && line.Name == "ItemImprintItems0")
            {
                int width = Math.Min(16, imprintData.imprintedItems.Count) * 36;
                int height = (int)Math.Ceiling((double)(imprintData.imprintedItems.Count / 16f)) * 28;
                DrawRadianceInvBG(Main.spriteBatch, line.X - 8, line.Y - 8, width + 10, height + 8, drawMode: imprintData.blacklist ? RadianceInventoryBGDrawMode.ItemImprintBlacklist : RadianceInventoryBGDrawMode.ItemImprint);
            }
            if (line.Name.StartsWith("ItemImprintItems"))
            {
                int number = int.Parse(line.Name.Last().ToString());
                for (int i = number * 16; i < Math.Min((number + 1) * 16, imprintData.imprintedItems.Count); i++)
                {
                    if (TryGetItemTypeFromFullName(imprintData.imprintedItems[i], out int type))
                    {
                        Item item = GetItem(type);
                        Vector2 pos = new Vector2(line.X + 16 + 36 * (i - number * 16), line.Y + 10);
                        ItemSlot.DrawItemIcon(item, 0, Main.spriteBatch, pos, 1f, 32, Color.White);
                    }
                }
                return false;
            }
            return true;
        }
        public override void UpdateInventory(Player player)
        {
            if (!imprintData.imprintedItems.AnyAndExists())
                Item.ChangeItemType(ModContent.ItemType<MemoryClay>());
        }
    }
}