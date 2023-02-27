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
    public interface IInventory
    {
        Item[] inventory { get; set; } //actual inventory array
        byte inventorySize { get; }
        byte[] inputtableSlots { get; }
        byte[] outputtableSlots { get; }

        void ConstructInventory(byte size, byte[] inputs, byte[] outputs); //set the inventory size, inputs, and outputs
        void SaveInventory(ref TagCompound tag); //save inventory contents in SaveData()
        void LoadInventory(ref TagCompound tag, byte size, byte[] inputs, byte[] outputs); //load inventory contents in LoadData()
        Item GetSlot(byte slot); //get the item in the parameter slot
        void InsertHeldItem(Player player, byte slot); //call InsertItemFromPlayerSlot from here
        void InsertItemFromPlayerSlot(Player player, int playerSlot, byte depositingSlot, out bool success); //safely insert the player's held item into the inventory
        void SafeInsertItemIntoSlot(byte slot, ref Item item, out bool success); //safely insert an item into an inventory, meaning it will try to stack items together before deleting or setting
        void SetItemInSlot(byte slot, Item item); //UNSAFELY set the item in a slot to the item parameter
        void DropAllItems(Vector2 pos, IEntitySource source); //drop all items. loop through and call DropItem
        void DropItem(byte slot, Vector2 pos, IEntitySource source); //drop item from slot into the world
    }
}