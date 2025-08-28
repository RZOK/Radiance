using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader.Config;

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
                    int type = inv[slot].type;
                    if (Main.mouseItem.ModItem is ItemImprint itemImprint && !itemImprint.imprintData.imprintedTypes.Contains(type))
                    {
                        if (inv[slot].type == Type && inv[slot].ModItem is ItemImprint hoveredImprint)
                            itemImprint.imprintData = hoveredImprint.imprintData;
                        else
                            itemImprint.imprintData.imprintedTypes.Add(type);

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
                if (imprintData.imprintedTypes.Count != 0)
                {
                    SoundEngine.PlaySound(SoundID.Grab);
                    if (imprintData.imprintedTypes.Count == 1)
                    {
                        Item.ChangeItemType(ModContent.ItemType<MemoryClay>());
                        Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().inventoryItemRightClickDelay = 10;
                    }
                    else
                        imprintData.imprintedTypes.Pop();
                }
            }
            else
                imprintData.blacklist = !imprintData.blacklist;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (imprintData.imprintedTypes.AnyAndExists())
            {
                for (int i = 0; i < Math.Ceiling((float)imprintData.imprintedTypes.Count / 16); i++)
                {
                    int realAmountToDraw = Math.Min(16, imprintData.imprintedTypes.Count - i * 16);
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
                string texString = "ItemImprintBackground";
                if (imprintData.blacklist)
                    texString = "ItemImprintBackgroundBlacklist";

                Texture2D bgTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/{texString}").Value;
                foreach (int item in imprintData.imprintedTypes)
                {
                    items.Add(new Item(item));
                }
                RadianceDrawing.DrawItemGrid(items, new Vector2(line.X, line.Y), bgTex, 16);
            }

            return !line.Name.StartsWith("ItemImprintItems");
        }

        public override void UpdateInventory(Player player)
        {
            if (!imprintData.imprintedTypes.AnyAndExists())
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
        public List<int> imprintedTypes = new List<int>();

        public ItemImprintData()
        { }

        public bool ImprintAcceptsItem(Item item)
        {
            if (!imprintedTypes.AnyAndExists())
                return true;

            if (blacklist)
                return !imprintedTypes.Contains(item.type);

            return imprintedTypes.Contains(item.type);
        }

        #region TagCompound

        public static readonly Func<TagCompound, ItemImprintData> DESERIALIZER = DeserializeData;

        public TagCompound SerializeData()
        {
            List<ItemDefinition> itemDefinitions = new List<ItemDefinition>();
            foreach (int type in imprintedTypes)
            {
                itemDefinitions.Add(new ItemDefinition(type));
            }
            return new TagCompound()
            {
                [nameof(blacklist)] = blacklist,
                [nameof(imprintedTypes)] = itemDefinitions,
            };
        }

        public static ItemImprintData DeserializeData(TagCompound tag)
        {
            List<int> types = new List<int>();
            foreach (ItemDefinition item in tag.GetList<ItemDefinition>(nameof(imprintedTypes)))
            {
                types.Add(item.Type);
            }
            ItemImprintData itemImprintData = new()
            {
                blacklist = tag.GetBool(nameof(blacklist)),
                imprintedTypes = types
            };
            return itemImprintData;
        }

        #endregion TagCompound
    }
}