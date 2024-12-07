using Terraria.UI;

namespace Radiance.Content.Items
{
    public class MemoryClay : ModItem
    {
        public override void Load()
        {
            On_ItemSlot.RightClick_ItemArray_int_int += CreateItemImprint;
        }

        public override void Unload()
        {
            On_ItemSlot.RightClick_ItemArray_int_int -= CreateItemImprint;
        }

        private void CreateItemImprint(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Main.mouseRight && Main.mouseRightRelease && !Main.mouseItem.IsAir && Main.mouseItem.type == Type && !inv[slot].IsAir && !Main.LocalPlayer.ItemAnimationActive)
            {
                Main.mouseItem.stack--;
                if (Main.mouseItem.stack <= 0)
                    Main.mouseItem.TurnToAir();

                Item createdItem = new Item(ModContent.ItemType<ItemImprint>());
                ItemImprint createdImprint = createdItem.ModItem as ItemImprint;

                if (inv[slot].type == ModContent.ItemType<ItemImprint>())
                    createdImprint.imprintData = (inv[slot].ModItem as ItemImprint).imprintData;
                else
                {
                    string saveString = inv[slot].GetTypeOrFullNameFromItem();
                    createdImprint.imprintData.imprintedItems = new List<string>() { saveString };
                }

                if (Main.mouseItem.stack > 0)
                    Main.LocalPlayer.QuickSpawnClonedItemDirect(new EntitySource_ItemUse(Main.LocalPlayer, Main.mouseItem), createdItem);
                else
                    Main.mouseItem = createdItem;

                SoundEngine.PlaySound(SoundID.Grab);
                return;
            }
            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().inventoryItemRightClickDelay == 0 || inv[slot].type != Type)
                orig(inv, context, slot);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Memory Clay");
            Tooltip.SetDefault("Right Click over another item to create an Item Imprint");
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 0, 10);
            Item.rare = ItemRarityID.Blue;
        }
        public override bool ConsumeItem(Player player) => false;
    }
}