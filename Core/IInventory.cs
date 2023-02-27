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
        public abstract Item[] inventory { get; set; } //actual inventory array
        public abstract byte inventorySize { get; }
        public abstract byte[] inputtableSlots { get; }
        public abstract byte[] outputtableSlots { get; }

        void ConstructInventory(); //set the inventory size on Load()

        void SaveInventory(ref TagCompound tag); //save inventory contents in SaveData()

        void LoadInventory(ref TagCompound tag); //load inventory contents in LoadData()

        Item GetSlot(int slot); //get the item in the parameter slot
        public void InsertHeldItem(Player player, int slot); //call InsertItemFromPlayerSlot from here

        public void InsertItemFromPlayerSlot(Player player, int playerSlot, int inventorySlot); //safely insert the player's held item into the inventory

        public void SafeInsertItemIntoSlot(ref Item item, int slot, out bool success); //safely insert an item into an inventory, meaning it will try to stack items together before deleting or setting

        public void SetItemInSlot(int slot, Item item); //UNSAFELY set the item in a slot to the item parameter
        public void DropAllItems(Vector2 pos, IEntitySource source); //drop all items. loop through and call DropItem
        public void DropItem(int slot, Vector2 pos, IEntitySource source); //drop item from slot into the world
    }
}