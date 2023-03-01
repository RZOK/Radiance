using Microsoft.Xna.Framework;
using Radiance.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Radiance.Utilities
{
    public static class InventoryUtils
    {
        public static void ConstructInventory(this IInventory inv, byte size)
        {
            if (inv.inventory == null || inv.inventory.Length == 0)
                inv.inventory = new Item[size];
        }

        public static void SaveInventory(this IInventory inv, ref TagCompound tag)
        {
            Item[] realInventory = new Item[inv.inventory.Length];
            for (int i = 0; i < inv.inventory.Length; i++)
            {
                Item item = inv.inventory[i] ?? new Item(0);
                realInventory[i] = item;
            }
            tag.Add("Inventory", realInventory);
        }
        public static void LoadInventory(this IInventory inv, ref TagCompound tag, byte size)
        {
            inv.ConstructInventory(size);
            inv.inventory = tag.Get<Item[]>("Inventory");
        }
        public static Item GetSlot(this IInventory inv, byte slot) => inv.inventory[slot] ?? new Item(ItemID.None);
        public static void InsertHeldItem(this IInventory inv, Player player, byte slot, out bool success)
        {
            success = false;
            if (player.whoAmI == Main.myPlayer)
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                inv.SafeInsertItemIntoSlot(slot, ref item, out bool success2);
                success = success2;
            }
        }
        public static void InsertItemFromPlayerSlot(this IInventory inv, Player player, int playerSlot, byte depositingSlot, out bool success)
        {
            inv.SafeInsertItemIntoSlot(depositingSlot, ref player.inventory[playerSlot], out bool success2);
            success = success2;
        }
        public static void SafeInsertItemIntoSlot(this IInventory inv, byte slot, ref Item item, out bool success, int stack = -1)
        {
            success = false;
            Item slotItem = inv.inventory[slot];
            Item newItem = item.Clone();
            int maxStack = newItem != null ? newItem.maxStack : 999;

            if (stack != -1)
            {
                maxStack = stack;
                newItem.stack = stack;
            }
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
                    if(item.stack <= 0)
                        item.TurnToAir();
                    inv.SetItemInSlot(slot, newItem);
                }
                success = true;
            }
        }

        public static void SetItemInSlot(this IInventory inv, byte slot, Item item)
        {
            inv.inventory[slot] = item;
        }
        public static void DropAllItems(this IInventory inv, Vector2 pos, IEntitySource source)
        {
            for (byte i = 0; i < inv.inventory.Length; i++)
            {
                Item item = inv.inventory[i];
                if (item != null && item.IsAir)
                    inv.DropItem(i, pos, source);
            }
        }
        public static void DropItem(this IInventory inv, byte slot, Vector2 pos, IEntitySource source)
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
    }
}
