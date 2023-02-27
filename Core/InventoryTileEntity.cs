using Microsoft.Xna.Framework;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Radiance.Core
{
    public abstract class InventoryTileEntity : ModTileEntity, IInventory
    {
        public Item[] inventory { get; set; }
        public byte inventorySize { get; }
        public byte[] inputtableSlots { get; set; }
        public byte[] outputtableSlots { get; set; }

        public void ConstructInventory(byte size, byte[] inputs, byte[] outputs)
        {
            if (inventory == null || inventory.Length == 0)
            {
                inventory = new Item[size];
                inputtableSlots = inputs;
                outputtableSlots = outputs;
            }
        }

        public void SaveInventory(ref TagCompound tag)
        {
            if (inventory.Length > 0)
                tag["Inventory"] = inventory.ToList();
        }

        public void LoadInventory(ref TagCompound tag, byte size, byte[] inputs, byte[] outputs)
        {
            ConstructInventory(size, inputs, outputs);
            inventory = tag.Get<List<Item>>("Inventory").ToArray();
        }

        public Item GetSlot(byte slot) => inventory[slot] ?? new Item(ItemID.None);
        public void InsertHeldItem(Player player, byte slot)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                SafeInsertItemIntoSlot(slot, ref item, out bool success);
            }
        }

        public void InsertItemFromPlayerSlot(Player player, int playerSlot, byte depositingSlot, out bool success)
        {
            SafeInsertItemIntoSlot(depositingSlot, ref player.inventory[playerSlot], out bool success2);
            success = success2;
        }

        public void SafeInsertItemIntoSlot(byte slot, ref Item item, out bool success)
        {
            success = false;
            Item slotItem = inventory[slot];
            Item newItem = item.Clone();
            if (newItem.type != ItemID.None)
            {
                if (slotItem != null && slotItem.type == item.type && slotItem.stack < slotItem.maxStack)
                {
                    if (newItem.stack + slotItem.stack <= slotItem.maxStack)
                        item.TurnToAir();
                    else
                        item.stack -= slotItem.maxStack - slotItem.stack;
                    success = true;
                    slotItem.stack = Math.Min(newItem.stack + slotItem.stack, slotItem.maxStack);
                }
                else
                {
                    item.TurnToAir();
                    SetItemInSlot(slot, newItem);
                }
            }
        }

        public void SetItemInSlot(byte slot, Item item)
        {
            inventory[slot] = item;
        }
        public void DropAllItems(Vector2 pos, IEntitySource source)
        {
            for (byte i = 0; i < inventory.Length; i++)
            {
                Item item = Main.item[i];
                if (item.type != ItemID.None && item != null)
                    DropItem(i, pos, source);
            }
        }
        public void DropItem(byte slot, Vector2 pos, IEntitySource source)
        {
            if (inventory[slot].type != ItemID.None && inventory[slot] != null)
            {
                int num = Item.NewItem(source, (int)pos.X, (int)pos.Y, 1, 1, inventory[slot].Clone().type, inventory[slot].stack, false, 0, false, false);
                Item item = Main.item[num];

                item = inventory[slot].Clone();
                item.velocity.Y = Main.rand.NextFloat(-4, -2);
                item.velocity.X = Main.rand.NextFloat(-2, 2);
                item.newAndShiny = true;
                inventory[slot].TurnToAir();

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
                }
            }
        }
    }
}