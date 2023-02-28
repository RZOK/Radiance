using Microsoft.Xna.Framework;
using Radiance.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
            List<Item> items = inv.inventory.ToList();
            var tagItems = tag["Items"];
            if (items != null && inv.inventory.Length > 0)
                tag["Items"] = items;
        }
        public static void LoadInventory(this IInventory inv, ref TagCompound tag, byte size)
        {
            inv.ConstructInventory(size);
            inv.inventory = ((List<Item>)tag.GetList<Item>("Items")).ToArray();
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
        public static void SafeInsertItemIntoSlot(this IInventory inv, byte slot, ref Item item, out bool success)
        {
            success = false;
            Item slotItem = inv.inventory[slot];
            Item newItem = item.Clone();
            if (newItem.type != ItemID.None)
            {
                if (slotItem != null && slotItem.type == item.type && slotItem.stack < slotItem.maxStack)
                {
                    if (newItem.stack + slotItem.stack <= slotItem.maxStack)
                        item.TurnToAir();
                    else
                        item.stack -= slotItem.maxStack - slotItem.stack;
                    slotItem.stack = Math.Min(newItem.stack + slotItem.stack, slotItem.maxStack);
                }
                else
                {
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
                Item item = Main.item[i];
                if (item != null && item.type != ItemID.None)
                    inv.DropItem(i, pos, source);
            }
        }
        public static void DropItem(this IInventory inv, byte slot, Vector2 pos, IEntitySource source)
        {
            if (inv.inventory[slot] != null && inv.inventory[slot].type != ItemID.None)
            {
                int num = Item.NewItem(source, (int)pos.X, (int)pos.Y, 1, 1, inv.inventory[slot].Clone().type, inv.inventory[slot].stack, false, 0, false, false);
                Item item = Main.item[num];

                item = inv.inventory[slot].Clone();
                item.velocity.Y = Main.rand.NextFloat(-4, -2);
                item.velocity.X = Main.rand.NextFloat(-2, 2);
                item.newAndShiny = true;
                inv.inventory[slot].TurnToAir();

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                }
            }
        }
    }
}
