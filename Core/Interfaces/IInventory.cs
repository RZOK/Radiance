﻿using Radiance.Content.Items.PedestalItems;

namespace Radiance.Core.Interfaces
{
    public interface IInventory
    {
        public int inventorySize { get; set; }
        public Item[] inventory { get; set; }
        /// <summary>
        /// Slots of the inventory that can automatically be inserted into.
        /// </summary>
        public byte[] inputtableSlots { get; }
        /// <summary>
        /// Slots of the inventory that can be automatically extracted from.
        /// </summary>
        public byte[] outputtableSlots { get; }
        /// <summary>
        /// Whether an item can be inserted into a slot. Used for filtering certain slots and often time working with item imprints.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slot"></param>
        /// <param name="overrideValidInputs">Whether to ignore <see cref="IInventory.inputtableSlots"/>. You should always set this to true when inserting through manual means in order to preserve consistency.</param>
        /// <param name="ignoreItemImprint">Whether to ignore item imprints. You should always set this to true when inserting through manual means in order to preserve consistency.</param>
        /// <returns></returns>
        bool TryInsertItemIntoSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint);
    }
}

namespace Radiance.Utilities
{
    public static class InventoryUtils
    {
        public static void ConstructInventory(this IInventory inv)
        {
            if (inv.inventory == null || inv.inventory.Length == 0)
                inv.inventory = Enumerable.Repeat(new Item(0), inv.inventorySize).ToArray();
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

        public static void LoadInventory(this IInventory inv, TagCompound tag)
        {
            inv.ConstructInventory();
            inv.inventory = tag.Get<Item[]>("Inventory");
            if (inv.inventory.Length != inv.inventorySize)
            {
                var tempInventory = inv.inventory;
                Array.Resize(ref tempInventory, inv.inventorySize);
                inv.inventory = tempInventory;
            }
            for (int i = 0; i < inv.inventory.Length; i++)
            {
                if (inv.inventory[i] is null)
                    inv.inventory[i] = new Item(0);
            }
        }

        public static Item GetSlot(this IInventory inv, byte slot) => inv.inventory[slot] ?? new Item(0);

        /// <summary>
        /// Searches the inventory to find the first slot with an item in it.
        /// </summary>
        /// <param name="inv">The inventory to search.</param>
        /// <param name="currentSlot">The first slot that has an item in it.</param>
        /// <returns>Whether there is a slot with an item in it at all.</returns>
        public static bool GetFirstSlotWithItem(this IInventory inv, out byte currentSlot)
        {
            for (currentSlot = 0; currentSlot < inv.inventory.Length; currentSlot++)
            {
                if (!inv.GetSlot(currentSlot).IsAir)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets every slot in an inventory that has an item in it.
        /// </summary>
        /// <param name="inv">The inventory to search.</param>
        /// <param name="start">The slot to begin the search from (inclusive)./</param>
        /// <param name="end">The slot to end the search at (exclusive).</param>
        /// <returns>A list of all slots with items in them.</returns>
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
        /// <summary>
        /// Checks to see if an item would fit into an inventory. 
        /// </summary>
        /// <param name="inv">The inventory to fit the item into.</param>
        /// <param name="item">The item being check to see if it would fit.</param>
        /// <param name="overrideValidInputs">Whether to ignore <see cref="IInventory.inputtableSlots"/>.</param>
        /// <param name="requireExistingItemType">Whether it should require an item of the same type already in the inventory.</param>
        /// <param name="ignoreItemImprint">Whether to ignore item imprints. You should always set this to true when inserting through manual means in order to preserve consistency.</param>
        /// <returns>Whether <paramref name="item"/> can fit into <paramref name="inv"/>.</returns>
        public static bool CanInsertItemIntoInventory(this IInventory inv, Item item, bool overrideValidInputs = false, bool requireExistingItemType = false, bool ignoreItemImprint = false)
        {
            if (inv is null)
                return false;

            if (inv is IOverrideInputtableSlotsFlag)
                overrideValidInputs = true;

            for (int i = 0; i < inv.inventory.Length; i++)
            {
                if (requireExistingItemType && (inv.inventory[i].IsAir || !inv.inventory[i].IsSameAs(item)))
                    continue;

                Item currentItem = inv.inventory[i];
                int maxStack = currentItem.maxStack;
                if(inv is ISpecificStackSlotInventory specificStackSlotInventory)
                {
                    if (specificStackSlotInventory.allowedStackPerSlot.TryGetValue(i, out int newStack))
                        maxStack = newStack;
                }
                if ((currentItem.IsAir || (currentItem.IsSameAs(item) && currentItem.stack < maxStack)) && (ignoreItemImprint || inv.TryInsertItemIntoSlot(item, (byte)i, overrideValidInputs, ignoreItemImprint)))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Runs <see cref="SafeInsertItemIntoSlot(IInventory, byte, ref Item, out bool)"/> on an item for every available slot in an inventory.
        /// </summary>
        /// <param name="inv">The inventory being inserted into.</param>
        /// <param name="item">The item being inserted.</param>
        /// <param name="overrideValidInputs">Whether to ignore <see cref="IInventory.inputtableSlots"/>.</param>
        /// <param name="ignoreItemImprint">Whether to ignore item imprints. You should always set this to true when inserting through manual means in order to preserve consistency.</param>
        public static void SafeInsertItemIntoInventory(this IInventory inv, Item item, out bool success, bool overrideValidInputs = false, bool ignoreItemImprint = false)
        {
            success = false;

            if (item.IsAir)
                return;

            if (inv is IOverrideInputtableSlotsFlag)
                overrideValidInputs = true;

            for (byte i = 0; i < inv.inventory.Length; i++)
            {
                inv.SafeInsertItemIntoSlot(i, item, out success, overrideValidInputs, ignoreItemImprint);
                if (item.stack <= 0)
                    return;
            }
        }
        /// <summary>
        /// Safely inserts an item into an inventory.
        /// </summary>
        /// <param name="inv">The inventory being inserted into.</param>
        /// <param name="slot">The slot in the inventory being inserted into.</param>
        /// <param name="originalItem">The item being inserted.</param>
        /// <param name="success">Whether the item was sucessfully inserted or not.</param>
        public static void SafeInsertItemIntoSlot(this IInventory inv, byte slot, Item originalItem, out bool success, bool overrideValidInputs = false, bool ignoreItemImprint = false)
        {
            success = false;

            if (originalItem.IsAir)
                return;

            if (inv is IOverrideInputtableSlotsFlag)
                overrideValidInputs = true;

            if (!inv.TryInsertItemIntoSlot(originalItem, slot, overrideValidInputs, ignoreItemImprint))
                return;

            Item itemInSlotBeingInsertedInto = inv.inventory[slot];
            Item itemBeingInserted = originalItem.Clone();
            if (!itemBeingInserted.IsAir)
            {
                int maxStack = itemBeingInserted.maxStack;
                if (inv is ISpecificStackSlotInventory specificStackSlotInventory && specificStackSlotInventory.allowedStackPerSlot.TryGetValue(slot, out int newStack))
                    maxStack = itemBeingInserted.stack = newStack;

                // If the slot is empty
                if (itemInSlotBeingInsertedInto.IsAir)
                {
                    inv.SetItemInSlot(slot, itemBeingInserted);

                    originalItem.stack -= itemBeingInserted.stack;
                    if (originalItem.stack <= 0)
                    {
                        originalItem.active = false;
                        originalItem.TurnToAir();
                    }
                    success = true;
                    return;
                }

                // If the slot is not empty but has an item of the same type and isn't at max stack
                if (itemInSlotBeingInsertedInto.IsSameAs(itemBeingInserted) && itemInSlotBeingInsertedInto.stack < maxStack)
                {
                    int difference = Math.Min(Math.Max(maxStack - itemInSlotBeingInsertedInto.stack, 0), itemBeingInserted.stack);

                    itemInSlotBeingInsertedInto.stack += difference;
                    originalItem.stack -= difference;

                    if (originalItem.stack <= 0)
                    {
                        originalItem.active = false;
                        originalItem.TurnToAir();
                    }
                    if (difference != 0)
                        success = true;
                }
            }
        }
        /// <summary>
        /// Safely inserts an item from a player's slot into an inventory's slot.
        /// </summary>
        /// <param name="inv">The inventory being inserted into.</param>
        /// <param name="player">The player whose specified slot item should be pulled</param>
        /// <param name="playerSlot">The slot in the player's inventory to pull from.</param>
        /// <param name="depositingSlot">The slot in the inventory to insert into.</param>
        /// <param name="success">Whether the item was sucessfully inserted or not.</param>
        public static void InsertItemFromPlayerSlot(this IInventory inv, Player player, int playerSlot, byte depositingSlot, out bool success) => inv.SafeInsertItemIntoSlot(depositingSlot, player.inventory[playerSlot], out success, true, true);

       /// <summary>
       /// Directly sets a slot to an item.
       /// <para />
       /// This should almost never be used. Use <see cref="SafeInsertItemIntoSlot(IInventory, byte, ref Item, out bool)"/> instead.
       /// </summary>
       /// <param name="inv">The inventory being inserted into.</param>
       /// <param name="slot">The slot to insert into.</param>
       /// <param name="item">The item being inserted.</param>
        public static void SetItemInSlot(this IInventory inv, byte slot, Item item) => inv.inventory[slot] = item;
        /// <summary>
        /// Spawns an item from the inventory into the world.
        /// </summary>
        /// <param name="inv">The inventory to pull from.</param>
        /// <param name="slot">The slot to pull from.</param>
        /// <param name="pos">The world coordinates that the item should be dropped at.</param>
        /// <param name="success">Whether the item was sucessfully dropped or not.</param>
        public static void DropItem(this IInventory inv, byte slot, Vector2 pos, out bool success)
        {
            success = false;
            if (inv.inventory[slot] != null && !inv.inventory[slot].IsAir)
            {
                int i = NewItemSpecific(pos, inv.inventory[slot].Clone());
                Item item = Main.item[i];
                item.GetGlobalItem<FormationCoreGlobalItem>().formationPickupTimer = 90;
                item.velocity.Y = Main.rand.NextFloat(-4, -2);
                item.velocity.X = Main.rand.NextFloat(-2, 2);
                inv.inventory[slot].TurnToAir();
                success = true;
            }
        }
        /// <summary>
        /// Runs <see cref="DropItem(IInventory, byte, Vector2, out bool)"/> for every slot in the inventory.
        /// </summary>
        /// <param name="inv"></param>
        /// <param name="pos"></param>
        public static void DropAllItems(this IInventory inv, Vector2 pos)
        {
            for (byte i = 0; i < inv.inventory.Length; i++)
            {
                Item item = inv.inventory[i];
                if (item != null && !item.IsAir)
                    inv.DropItem(i, pos, out _);
            }
        }
        public static IInventory GetInventory(this IInventory inv)
        {
            if (inv is IRedirectInterfacableInventory rdi)
                return rdi.redirectedInventory;

            return inv;
        }
    }
}