namespace Radiance.Core.Interfaces
{
    public interface IInventory
    {
        Item[] inventory { get; set; }
        byte[] inputtableSlots { get; }
        byte[] outputtableSlots { get; }
    }
}

namespace Radiance.Utilities
{
    public static class InventoryUtils
    {
        public static void ConstructInventory(this IInventory inv, byte size)
        {
            if (inv.inventory == null || inv.inventory.Length == 0)
                inv.inventory = Enumerable.Repeat(new Item(0), size).ToArray();
        }

        public static void SaveInventory(this IInventory inv, TagCompound tag)
        {
            Item[] realInventory = new Item[inv.inventory.Length];
            for (int i = 0; i < inv.inventory.Length; i++)
            {
                Item item = inv.inventory[i] ?? GetItem(0);
                realInventory[i] = item;
            }
            tag.Add("Inventory", realInventory);
        }

        public static void LoadInventory(this IInventory inv, TagCompound tag, byte size)
        {
            inv.ConstructInventory(size);
            inv.inventory = tag.Get<Item[]>("Inventory");
            if (inv.inventory.Length != size)
            {
                var tempInventory = inv.inventory;
                Array.Resize(ref tempInventory, size);
                inv.inventory = tempInventory;
            }
            for (int i = 0; i < inv.inventory.Length; i++)
            {
                if (inv.inventory[i] is null)
                    inv.inventory[i] = new Item(0);
            }
        }

        public static Item GetSlot(this IInventory inv, byte slot) => inv.inventory[slot] ?? ContentSamples.ItemsByType[ItemID.None];

        public static bool GetFirstSlotWithItem(this IInventory inv, out byte currentSlot)
        {
            currentSlot = 0;
            while (currentSlot < inv.inventory.Length)
            {
                if (!inv.GetSlot(currentSlot).IsAir)
                    return true;

                currentSlot++;
            }
            return false;
        }

        public static bool CanInsertItemIntoInventory(this IInventory inv, Item item, bool overrideValidInputs = false, bool requireExistingItemType = false)
        {
            if (requireExistingItemType && !inv.inventory.Where(x => x.IsSameAs(item)).Any())
                return false;

            for (int i = 0; i < inv.inventory.Length; i++)
            {
                if (!overrideValidInputs && !Array.Exists(inv.inputtableSlots, x => x == i))
                    continue;

                Item currentItem = inv.inventory[i];
                if (currentItem.IsAir || (inv is not ISpecificStackSlotInventory && (currentItem.IsSameAs(item) && currentItem.stack < currentItem.maxStack)))
                    return true;
            }
            return false;
        }

        public static void SafeInsertItemIntoInventory(this IInventory inv, Item item, bool overrideValidInputs = false)
        {
            if (item.IsAir)
                return;

            for (byte i = 0; i < inv.inventory.Length; i++)
            {
                if (!overrideValidInputs && !Array.Exists(inv.inputtableSlots, x => x == i))
                    continue;

                inv.SafeInsertItemIntoSlot(i, ref item, out var _);
                if (item.stack <= 0)
                    return;
            }
        }

        public static List<byte> GetSlotsWithItems(this IInventory inv, byte start = 0, int end = -1)
        {
            if (end == -1)
                end = inv.inventory.Length;

            List<byte> slots = new List<byte>();
            for (byte i = start; i < end; i++)
            {
                if (!inv.GetSlot(i).IsAir)
                    slots.Add(i);
            }
            return slots;
        }

        public static void InsertHeldItem(this IInventory inv, Player player, byte slot, out bool success)
        {
            success = false;
            if (player.whoAmI == Main.myPlayer)
            {
                Item item = GetPlayerHeldItem();
                inv.SafeInsertItemIntoSlot(slot, ref item, out success);
            }
        }

        public static void InsertItemFromPlayerSlot(this IInventory inv, Player player, int playerSlot, byte depositingSlot, out bool success) => inv.SafeInsertItemIntoSlot(depositingSlot, ref player.inventory[playerSlot], out success);

        public static void SafeInsertItemIntoSlot(this IInventory inv, byte slot, ref Item originalItem, out bool success)
        {
            success = false;
            Item itemInSlotBeingInsertedInto = inv.inventory[slot];
            Item itemBeingInserted = originalItem.Clone();
            if (!itemBeingInserted.IsAir)
            {
                int maxStack = itemBeingInserted.maxStack;
                if (inv is ISpecificStackSlotInventory specificStackSlotInventory && specificStackSlotInventory.allowedStackPerSlot.TryGetValue(slot, out int newStack))
                    maxStack = itemBeingInserted.stack = newStack;

                if (itemInSlotBeingInsertedInto.IsAir)
                {
                    inv.SetItemInSlot(slot, itemBeingInserted);

                    originalItem.stack -= itemBeingInserted.stack;
                    if (originalItem.stack <= 0)
                        originalItem.TurnToAir();
                    success = true;
                    return;
                }
                if (itemInSlotBeingInsertedInto.IsSameAs(itemBeingInserted) && itemInSlotBeingInsertedInto.stack < maxStack)
                {
                    int difference = Math.Min(Math.Max(maxStack - itemInSlotBeingInsertedInto.stack, 0), itemInSlotBeingInsertedInto.stack + itemBeingInserted.stack);

                    itemInSlotBeingInsertedInto.stack += difference;
                    originalItem.stack -= difference;

                    if (originalItem.stack <= 0)
                        originalItem.TurnToAir();

                    if (difference != 0)
                        success = true;
                }
            }
        }

        /// <summary>
        /// 99% of the time you shouldn't use this and should instead just use SafeInsetItemIntoSlot() instead, but it is remaining public in case you DO need to use it for whatever reason.
        /// </summary>
        public static void SetItemInSlot(this IInventory inv, byte slot, Item item) => inv.inventory[slot] = item;

        public static void DropItem(this IInventory inv, byte slot, Vector2 pos, out bool success)
        {
            success = false;
            if (inv.inventory[slot] != null && !inv.inventory[slot].IsAir)
            {
                int i = NewItemSpecific(pos, inv.inventory[slot].Clone());
                Item item = Main.item[i];
                item.GetGlobalItem<RadianceGlobalItem>().formationPickupTimer = 90;
                item.velocity.Y = Main.rand.NextFloat(-4, -2);
                item.velocity.X = Main.rand.NextFloat(-2, 2);
                inv.inventory[slot].TurnToAir();
                success = true;
            }
        }

        public static void DropAllItems(this IInventory inv, Vector2 pos)
        {
            for (byte i = 0; i < inv.inventory.Length; i++)
            {
                Item item = inv.inventory[i];
                if (item != null && !item.IsAir)
                    inv.DropItem(i, pos, out _);
            }
        }
    }
}