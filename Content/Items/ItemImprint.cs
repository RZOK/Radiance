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
            Tooltip.SetDefault("Stores the imprints of items\nRight click on a valid Apparatus to apply this imprint to it\nHold Shift and Right click in your inventory to clear the most recently added item instead");
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
                    if (i == 0)
                        itemDisplayLine.Text = new String('M', 2 * realAmountToDraw + 3) + i;
                    else
                        itemDisplayLine.Text = ".";
                    tooltips.Add(itemDisplayLine);
                }
            }
        }
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "ItemImprintItems0")
            {
                List<Item> items = new List<Item>();
                Texture2D bgTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/ItemImprintBackground{(imprintData.blacklist ? "Blacklist" : string.Empty)}").Value;
                foreach (string item in imprintData.imprintedItems)
                {
                    if(TryGetItemTypeFromFullName(item, out int type))
                    {
                        items.Add(new Item(type));
                    }
                }
                RadianceDrawing.DrawItemGrid(items, new Vector2(line.X, line.Y), bgTex, 16);
            }

            return !line.Name.StartsWith("ItemImprintItems");
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
        public bool ImprintAcceptsItem(Item item)
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