using Radiance.Content.Items.BaseItems;
using System.Reflection;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Radiance.Utilities
{
    public static partial class RadianceUtils
    {
        public static Item GetPlayerHeldItem() => Main.mouseItem.IsAir ? Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] : Main.mouseItem;

        public static Item GetPlayerHeldItem(this Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                return GetPlayerHeldItem();
            return null;
        }

        public static Item GetItem(int type) => type < ItemID.Count ? ContentSamples.ItemsByType[type] : ItemLoader.GetItem(type).Item;

        public static string GetBuffName(int type) => type < BuffID.Count ? BuffID.Search.GetName(type) : BuffLoader.GetBuff(type).Name;

        public static float GetSmoothTileRNG(this Point tilePos, int shift = 0) => (float)(MathF.Sin(tilePos.X * 17.07947f + shift * 36f) + Math.Sin(tilePos.Y * 25.13274)) * 0.25f + 0.5f;
        public static float GetSmoothIntRNG(int number, int shift = 0) => (float)MathF.Sin(number * 17.07947f + shift * 36f) * 0.5f + 0.5f;

        public static bool IsCCd(this Player player) => player.CCed || player.frozen || player.noItems || !player.active || player.dead;

        public static Vector2 tileDrawingZero => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        public static bool OnScreen(Rectangle rectangle) => rectangle.Intersects(new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenWidth));

        public static void GetPrefixStats(int prefix, out int defense, out int mana, out int crit, out float damage, out float moveSpeed, out float meleeSpeed)
        {
            defense = 0;
            mana = 0;
            crit = 0;
            damage = 0;
            moveSpeed = 0;
            meleeSpeed = 0;
            switch (prefix)
            {
                case 62:
                    defense += 1;
                    break;

                case 63:
                    defense += 2;
                    break;

                case 64:
                    defense += 3;
                    break;

                case 65:
                    defense += 4;
                    break;

                case 66:
                    mana += 20;
                    break;

                case 67:
                    crit += 2;
                    break;

                case 68:
                    crit += 4;
                    break;

                case 69:
                    damage += 0.01f;
                    break;

                case 70:
                    damage += 0.02f;
                    break;

                case 71:
                    damage += 0.03f;
                    break;

                case 72:
                    damage += 0.04f;
                    break;

                case 73:
                    moveSpeed += 0.01f;
                    break;

                case 74:
                    moveSpeed += 0.02f;
                    break;

                case 75:
                    moveSpeed += 0.03f;
                    break;

                case 76:
                    moveSpeed += 0.04f;
                    break;

                case 77:
                    meleeSpeed += 0.01f;
                    break;

                case 78:
                    meleeSpeed += 0.02f;
                    break;

                case 79:
                    meleeSpeed += 0.03f;
                    break;

                case 80:
                    meleeSpeed += 0.04f;
                    break;
            }
        }

        public static void LogIlError(string name, string reason)
        {
            Radiance.Instance.Logger.Warn($"IL edit \"{name}\" failed! {reason}");
            SoundEngine.PlaySound(SoundID.DoorClosed);
        }

        public static Texture2D GetItemTexture(int type)
        {
            Main.instance.LoadItem(type);
            if (type >= ItemID.Count)
                return ModContent.Request<Texture2D>(ItemLoader.GetItem(type).Texture).Value;

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
            Main.item[targetIndex].favorited = false;

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

        public static Color MulticolorLerp(float increment, params Color[] colors)
        {
            increment %= 0.999f;
            int currentColorIndex = (int)(increment * colors.Length);
            Color currentColor = colors[currentColorIndex];
            Color nextColor = colors[(currentColorIndex + 1) % colors.Length];
            return Color.Lerp(currentColor, nextColor, increment * colors.Length % 1f);
        }

        public static void GetRadianceFromItem(this IInterfaceableRadianceCell obj)
        {
            if (obj is RadianceUtilizingTileEntity entity && entity is IInventory inventory && inventory.inventory != null)
            {
                BaseContainer container = obj.ContainerPlaced;
                if (container != null)
                {
                    entity.maxRadiance = container.maxRadiance;
                    entity.currentRadiance = container.currentRadiance;
                }
                else
                    entity.maxRadiance = entity.currentRadiance = 0;
            }
        }

        public static void SetCursorItem(this Player player, int id)
        {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = id;
        }

        public static void SpawnDebugDust(this Vector2 position, float scale = 1)
        {
            Dust d = Dust.NewDustPerfect(position, DustID.RedTorch);
            d.noGravity = true;
            d.scale = scale;
            d.velocity = Vector2.Zero;
        }

        public static void DrawRadianceInvBG(SpriteBatch spriteBatch, int x, int y, int width, int height)
        {
            Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/LightArrayInventorySlot").Value;
            Rectangle cornerFrame = new Rectangle(0, 0, 16, 16);
            Rectangle edgeFrame = new Rectangle(16, 0, 1, 16);
            Rectangle innerFrame = new Rectangle(16, 16, 1, 1);
            Color color = Color.White * 0.9f;

            // corners
            spriteBatch.Draw(texture, new Vector2(x, y), cornerFrame, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(x + width, y), cornerFrame, color, MathHelper.PiOver2, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(x + width, y + height), cornerFrame, color, MathHelper.Pi, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(x, y + height), cornerFrame, color, MathHelper.PiOver2 * 3, Vector2.Zero, 1, SpriteEffects.None, 0);

            // edges
            spriteBatch.Draw(texture, new Vector2(x + cornerFrame.Width, y), edgeFrame, color, 0, Vector2.Zero, new Vector2((width - cornerFrame.Width * 2), 1), SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(x + width, y + cornerFrame.Height), edgeFrame, color, MathHelper.PiOver2, Vector2.Zero, new Vector2(height - cornerFrame.Height * 2, 1), SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(x + width - cornerFrame.Width, y + height), edgeFrame, color, MathHelper.Pi, Vector2.Zero, new Vector2(width - cornerFrame.Width * 2, 1), SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(x, y + height - cornerFrame.Height), edgeFrame, color, MathHelper.PiOver2 * 3, Vector2.Zero, new Vector2(height - cornerFrame.Height * 2, 1), SpriteEffects.None, 0);

            spriteBatch.Draw(texture, new Vector2(x + cornerFrame.Width, y + cornerFrame.Height), innerFrame, color, 0, Vector2.Zero, new Vector2(width - cornerFrame.Width * 2, height - cornerFrame.Height * 2), SpriteEffects.None, 0);
        }

        public static void DrawFakeItemHover(SpriteBatch spriteBatch, string[] strings, Color? color = null, bool fancy = false)
        {
            if (!color.HasValue)
                color = new Color(23, 25, 81, 255) * 0.925f;

            var font = FontAssets.MouseText.Value;
            float boxWidth;
            float boxHeight = -16;
            Vector2 pos = Main.MouseScreen + new Vector2(30, 30);
            
            string widest = strings.OrderBy(n => ChatManager.GetStringSize(font, n, Vector2.One).X).Last();
            boxWidth = ChatManager.GetStringSize(font, widest, Vector2.One).X + 20;

            foreach (string str in strings)
            {
                boxHeight += ChatManager.GetStringSize(font, str, Vector2.One).Y;
            }

            if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
            {
                pos.X += 8;
                pos.Y += 2;
            }

            if (pos.X + ChatManager.GetStringSize(font, widest, Vector2.One).X > Main.screenWidth)
                pos.X = (int)(Main.screenWidth - boxWidth);

            if (pos.Y + ChatManager.GetStringSize(font, widest, Vector2.One).Y > Main.screenHeight)
                pos.Y = (int)(Main.screenHeight - boxHeight);

            if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
                Utils.DrawInvBG(spriteBatch, new Rectangle((int)pos.X - 14, (int)pos.Y - 10, (int)boxWidth + 6, (int)boxHeight + 28), color.Value);

            foreach (string str in strings)
            {
                Utils.DrawBorderString(spriteBatch, str, pos, Color.White);

                pos.Y += ChatManager.GetStringSize(font, str, Vector2.One).Y;
            }
        }

        public static bool IsSameAs(this Item item, Item matchingItem)
        {
            if(item.netID == matchingItem.netID)
                return item.type == matchingItem.type;
            return false;
        }

        #region Reflection

        public static FieldInfo ReflectionGrabField(this object obj, string name, BindingFlags flags) => obj.GetType().GetField(name, flags);

        public static object ReflectionGetValue(this object obj, string name, BindingFlags flags) => obj.ReflectionGrabField(name, flags).GetValue(obj);

        public static void ReflectionSetValue(this object obj, string name, object value, BindingFlags flags) => obj.ReflectionGrabField(name, flags).SetValue(obj, value);

        public static MethodInfo ReflectionGetMethod(this object obj, string name, BindingFlags flags) => obj.GetType().GetMethod(name, flags);

        public static MethodInfo ReflectionGetMethodFromType(this Type type, string name, BindingFlags flags) => type.GetMethod(name, flags);

        public static MethodInfo ReflectionGetMethodFromType(this Type type, string name, BindingFlags flags, Type[] types) => type.GetMethod(name, flags, types);

        public static object ReflectionInvokeMethod(this object obj, string name, BindingFlags flags, params object[] parameters) => obj.ReflectionGetMethod(name, flags).Invoke(obj, parameters);

        #endregion Reflection
    }
}