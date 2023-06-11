﻿using Microsoft.Xna.Framework;
using Radiance.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

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
                inv.inventory = new Item[size];
        }

        public static void SaveInventory(this IInventory inv, TagCompound tag)
        {
            Item[] realInventory = new Item[inv.inventory.Length];
            for (int i = 0; i < inv.inventory.Length; i++)
            {
                Item item = inv.inventory[i] ?? new Item(0);
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
        public static List<byte> GetSlotsWithItems(this IInventory inv, byte start = 0, int end = -1)
        {
            if (end == -1)
                end = inv.inventory.Length;
            List<byte> slots = new List<byte>();
            for (byte i = start; i < end; i++)
            {
                if(!inv.GetSlot(i).IsAir)
                    slots.Add(i);
            }
            return slots.Any() ? slots : null;
        }
        public static void InsertHeldItem(this IInventory inv, Player player, byte slot, out bool success)
        {
            success = false;
            if (player.whoAmI == Main.myPlayer)
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                inv.SafeInsertItemIntoSlot(slot, ref item, out success);
            }
        }
        public static void InsertItemFromPlayerSlot(this IInventory inv, Player player, int playerSlot, byte depositingSlot, out bool success) => inv.SafeInsertItemIntoSlot(depositingSlot, ref player.inventory[playerSlot], out success);
        public static void SafeInsertItemIntoSlot(this IInventory inv, byte slot, ref Item item, out bool success, int stack = -1)
        {
            success = false;
            Item slotItem = inv.inventory[slot];
            Item newItem = item.Clone();
            int maxStack = newItem != null ? newItem.maxStack : Item.CommonMaxStack;

            if (stack != -1)
                maxStack = newItem.stack = stack;

            if (!newItem.IsAir)
            {
                if (slotItem != null && slotItem.type == item.type && slotItem.stack < maxStack)
                {
                    if (newItem.stack + slotItem.stack <= maxStack)
                        item.TurnToAir();
                    else
                        item.stack -= maxStack - slotItem.stack;
                    slotItem.stack = Math.Min(newItem.stack + slotItem.stack, maxStack);
                }
                else
                {
                    item.stack -= maxStack;
                    if (item.stack <= 0)
                        item.TurnToAir();
                    inv.SetItemInSlot(slot, newItem);
                }
                success = true;
            }
        }
        /// <summary>
        /// 99% of the time you shouldn't use this and should instead just use SafeInsetItemIntoSlot() instead, but it is remaining public in case you DO need to use it for whatever reason.
        /// </summary>
        public static void SetItemInSlot(this IInventory inv, byte slot, Item item) => inv.inventory[slot] = item;

        public static void DropItem(this IInventory inv, byte slot, Vector2 pos)
        {
            if (inv.inventory[slot] != null && !inv.inventory[slot].IsAir)
            {
                int i = RadianceUtils.NewItemSpecific(pos, inv.inventory[slot].Clone());
                Item item = Main.item[i];
                item.velocity.Y = Main.rand.NextFloat(-4, -2);
                item.velocity.X = Main.rand.NextFloat(-2, 2);
                inv.inventory[slot].TurnToAir();
            }
        }
        public static void DropAllItems(this IInventory inv, Vector2 pos)
        {
            for (byte i = 0; i < inv.inventory.Length; i++)
            {
                Item item = inv.inventory[i];
                if (item != null && !item.IsAir)
                    inv.DropItem(i, pos);
            }
        }
    }
}