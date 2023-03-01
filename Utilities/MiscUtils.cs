using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Core;
using static Terraria.Player;
using Terraria.UI;

namespace Radiance.Utilities
{
    partial class RadianceUtils
    {
        public static Item GetPlayerHeldItem() => Main.mouseItem.IsAir ? Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] : Main.mouseItem;
        public static Item GetItem(int type) => type < ItemID.Count ? ContentSamples.ItemsByType[type] : ItemLoader.GetItem(type).Item;
        public static string GetBuffName(int type) => type < BuffID.Count ? BuffID.Search.GetName(type) : BuffLoader.GetBuff(type).Name;
        public static float GetSmoothTileRNG(this Point tilePos, int shift = 0) => (float)(Math.Sin(tilePos.X * 17.07947 + shift * 36) + Math.Sin(tilePos.Y * 25.13274)) * 0.25f + 0.5f;
        public static bool IsCCd(this Player player) => player.CCed || player.frozen || player.noItems || !player.active || player.dead;
        public static Texture2D GetItemTexture(int type)
        {
            if (type < ItemID.Count)
                return null;
            Main.instance.LoadItem(type);
            return TextureAssets.Item[type].Value;
        }
        public static Vector3 Vec3(this Vector2 vector) => new Vector3(vector.X, vector.Y, 0);
        public static T[] FastUnion<T>(this T[] front, T[] back)
        {
            T[] combined = new T[front.Length + back.Length];

            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }
        public static int NewItemSpecific(Vector2 position, Item Item) 
        {
            int targetIndex = 400;
            Main.item[400] = new Item(); 

            if (Main.netMode != NetmodeID.MultiplayerClient) 
            {
                for (int j = 0; j < 400; j++)
                {
                    if (!Main.item[j].active && Main.timeItemSlotCannotBeReusedFor[j] == 0)
                    {
                        targetIndex = j;
                        break;
                    }
                }
            }
            if (targetIndex == 400 && Main.netMode != NetmodeID.MultiplayerClient) 
            {
                int num2 = 0;
                for (int k = 0; k < 400; k++)
                {
                    if (Main.item[k].timeSinceItemSpawned - Main.timeItemSlotCannotBeReusedFor[k] > num2)
                    {
                        num2 = Main.item[k].timeSinceItemSpawned - Main.timeItemSlotCannotBeReusedFor[k];
                        targetIndex = k;
                    }
                }
            }

            Main.item[targetIndex] = Item;
            Main.item[targetIndex].position = position;

            if (ItemSlot.Options.HighlightNewItems && Item.type >= ItemID.None && !ItemID.Sets.NeverAppearsAsNewInInventory[Item.type])
                Item.newAndShiny = true;
            if (Main.netMode == NetmodeID.Server) 
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, targetIndex, 0, 0f, 0f, 0, 0, 0);
                Item.FindOwner(Item.whoAmI);
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
                Item.playerIndexTheItemIsReservedFor = Main.myPlayer;
            return targetIndex;
        }
    }

}
