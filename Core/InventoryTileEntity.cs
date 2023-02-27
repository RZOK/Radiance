using Microsoft.Xna.Framework;
using Radiance.Utilities;
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
        public abstract Item[] inventory { get; set; }
        public abstract int ParentTile { get; }
        public abstract byte inventorySize { get; }
        public abstract byte[] inputtableSlots { get; }
        public abstract byte[] outputtableSlots { get; }

        public void ConstructInventory()
        {
            inventory = new Item[inventorySize];
        }

        public void SaveInventory(ref TagCompound tag)
        {
            if (inventory.Length > 0)
                tag["Inventory"] = inventory.ToList();
        }

        public void LoadInventory(ref TagCompound tag)
        {
            inventory = tag.Get<List<Item>>("Inventory").ToArray();
        }

        public Item GetSlot(int slot) => inventory[slot];
        public void InsertHeldItem(Player player, int slot)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                SafeInsertItemIntoSlot(ref item, slot, out bool success);
            }
        }

        public void InsertItemFromPlayerSlot(Player player, int playerSlot, int inventorySlot)
        {
            SafeInsertItemIntoSlot(ref player.inventory[playerSlot], inventorySlot, out bool success);
        }

        public void SafeInsertItemIntoSlot(ref Item item, int slot, out bool success)
        {
            success = false;
            Item slotItem = inventory[slot];
            Item newItem = item.Clone();
            if (slotItem.type == item.type && slotItem.stack < slotItem.maxStack)
            {
                if (slotItem.maxStack - slotItem.stack < newItem.stack)
                {
                    success = true;
                    slotItem.stack += newItem.stack;
                    item.TurnToAir();
                }
                else
                {
                    success = true;
                    item.stack -= slotItem.maxStack - slotItem.stack;
                    slotItem.stack = slotItem.maxStack;
                }
            }
            else
            {
                success = true;
                SetItemInSlot(slot, newItem);
                item.TurnToAir();
            }
        }

        public void SetItemInSlot(int slot, Item item)
        {
            inventory[slot] = item;
        }
        public void DropAllItems(Vector2 pos, IEntitySource source)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                Item item = Main.item[i];
                if (item.type != ItemID.None && item != null)
                    DropItem(i, pos, source);
            }
        }
        public void DropItem(int slot, Vector2 pos, IEntitySource source)
        {
            int num = Item.NewItem(source, (int)pos.X, (int)pos.Y, 1, 1, inventory[slot].Clone().type, 1, false, 0, false, false);
            inventory[slot].TurnToAir();
            Item item = Main.item[num];

            item = inventory[slot].Clone();
            item.velocity.Y = Main.rand.NextFloat(-4, -2);
            item.velocity.X = Main.rand.NextFloat(-2, 2);
            item.newAndShiny = true;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
            }
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ParentTile;
        }
    }
}