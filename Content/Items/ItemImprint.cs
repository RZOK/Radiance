using Terraria.UI;
using Microsoft.Xna.Framework.Input;

namespace Radiance.Content.Items
{
    public class ItemImprint : ModItem
    {
        public ItemImprintData imprintData;
        public override void Load()
        {
            On_ItemSlot.RightClick_ItemArray_int_int += AddItemToImprint;
        }

        public override void Unload()
        {
            On_ItemSlot.RightClick_ItemArray_int_int -= AddItemToImprint;
        }

        private void AddItemToImprint(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
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
                        return;
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
        public override bool ConsumeItem(Player player) => false;
        public override bool CanRightClick() => true;
        public override void RightClick(Player player)
        {
            if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
            {
                if (imprintData.imprintedItems.Count != 0)
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                    if (imprintData.imprintedItems.Count == 1)
                    {  
                        Item.ChangeItemType(ModContent.ItemType<MemoryClay>());
                        Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().inventoryItemRightClickDelay = 10; 
                    }
                    else
                        imprintData.imprintedItems.Pop();
                }
            }
            else
                imprintData.blacklist = !imprintData.blacklist;
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
                DrawRadianceInvBG(Main.spriteBatch, line.X - 8, line.Y - 8, width + 12, height + 8, drawMode: imprintData.blacklist ? RadianceInventoryBGDrawMode.ItemImprintBlacklist : RadianceInventoryBGDrawMode.ItemImprint);
            }
            if (line.Name.StartsWith("ItemImprintItems"))
            {
                int number = int.Parse(line.Name.Last().ToString());
                for (int i = number * 16; i < Math.Min((number + 1) * 16, imprintData.imprintedItems.Count); i++)
                {
                    if (TryGetItemTypeFromFullName(imprintData.imprintedItems[i], out int type)) //todo: replace with fullname
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

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(imprintData)] = imprintData;
        }
        public override void LoadData(TagCompound tag)
        {
            imprintData = tag.Get<ItemImprintData>(nameof(imprintData));
        }
    }
    public struct ItemImprintData : TagSerializable
    {
        public bool blacklist = false;
        public List<string> imprintedItems = new List<string>();
        public ItemImprintData() { }
        public bool IsItemValid(Item item)
        {
            if (!imprintedItems.AnyAndExists())
                return true;

            if (blacklist)
                return !imprintedItems.Contains(item.GetTypeOrFullNameFromItem());

            return imprintedItems.Contains(item.GetTypeOrFullNameFromItem());
        }
        #region TagCompound Stuff

        public static readonly Func<TagCompound, ItemImprintData> DESERIALIZER = DeserializeData;

        public TagCompound SerializeData()
        {
            return new TagCompound()
            {
                [nameof(blacklist)] = blacklist,
                [nameof(imprintedItems)] = imprintedItems,
            };
        }

        public static ItemImprintData DeserializeData(TagCompound tag)
        {
            ItemImprintData itemImprintData = new()
            {
                blacklist = tag.GetBool(nameof(blacklist)),
                imprintedItems = (List<string>)tag.GetList<string>(nameof(imprintedItems))
            };
            return itemImprintData;
        }

        #endregion TagCompound Stuff
    }
}