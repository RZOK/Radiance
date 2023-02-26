using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Core;
using static Terraria.Player;

namespace Radiance.Utilities
{
    partial class RadianceUtils
    {
        public static Item GetPlayerHeldItem() => Main.mouseItem.type == ItemID.None ? Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] : Main.mouseItem;
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
    }

}
