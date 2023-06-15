﻿using Radiance.Content.Items.BaseItems;
using System.Reflection;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;

namespace Radiance.Content.UI.LightArrayInventoryUI
{
    internal class LightArrayInventoryUI : SmartUIState
    {
        public const int ItemSlotContext = 607;
        public static LightArrayInventoryUI Instance { get; set; }

        public LightArrayInventoryUI()
        {
            Instance = this;
        }

        public int cursor = 0;

        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

        public BaseLightArray currentActiveArray => Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().currentlyActiveArray;
        public ref int timer => ref Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayUITimer;
        public int timerMax => Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayUITimerMax;
        public override bool Visible => currentActiveArray != null && Main.playerInventory && Main.LocalPlayer.active && !Main.LocalPlayer.dead;
        public UIElement centerIcon = new();

        public override void Update(GameTime gameTime)
        {
            bool flag = false;
            if (Main.LocalPlayer.HasActiveArray())
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Main.LocalPlayer.inventory[i].ModItem == Main.LocalPlayer.CurrentActiveArray())
                        flag = true;
                }
            }
            if (!flag)
                Main.LocalPlayer.ResetActiveArray();
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.LocalPlayer.HasActiveArray())
            {
                float ease = EaseOutExponent((float)timer / timerMax, 9);
                Main.inventoryScale = 0.9f * ease;
                Texture2D tex = TextureAssets.InventoryBack.Value;
                Vector2 offset = tex.Size() / 2 * Main.inventoryScale;
                Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
                Vector2 slotPosition = screenCenter - offset;
                int rows = 1;
                for (int i = 0; i < currentActiveArray.inventorySize; i++)
                {
                    int amountDrawnSoFar = rows * (rows + 1) / 2 * 8;
                    if (i > 0 && i % amountDrawnSoFar == 0)
                        rows += 1;
                    amountDrawnSoFar = rows * (rows + 1) / 2 * 8;

                    int amountInCurrentRow = 8 * rows;
                    int realAmountToDrawInRow = Math.Min(amountInCurrentRow, currentActiveArray.inventorySize - (amountDrawnSoFar - amountInCurrentRow)); 
                    float rotation = i % (float)realAmountToDrawInRow / realAmountToDrawInRow;
                    float distance = rows * ease * 68;
                    Vector2 newSlotPosition = slotPosition + Vector2.UnitX.RotatedBy(rotation * ease * MathHelper.TwoPi - MathHelper.PiOver2 * ((rows % 2 == 1) ? 1 : -1)) * distance;

                    Rectangle slotRectangle = new Rectangle((int)newSlotPosition.X, (int)newSlotPosition.Y, (int)(tex.Width * Main.inventoryScale), (int)(tex.Height * Main.inventoryScale));
                    if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                    {
                        Main.LocalPlayer.mouseInterface = true;
                        ItemSlot.OverrideHover(currentActiveArray.inventory, ItemSlotContext, i);
                        if (BaseLightArray.IsValidForLightArray(Main.mouseItem))
                        {
                            ItemSlot.LeftClick(currentActiveArray.inventory, ItemSlotContext, i);
                            ItemSlot.RightClick(currentActiveArray.inventory, ItemSlotContext, i);
                        }
                        if (Main.mouseLeftRelease && Main.mouseLeft)
                            Recipe.FindRecipes();
                        ItemSlot.MouseHover(currentActiveArray.inventory, ItemSlotContext, i);
                    }
                    ItemSlot.Draw(spriteBatch, currentActiveArray.inventory, ItemSlotContext, i, newSlotPosition);
                }
                Rectangle centerSlotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * Main.inventoryScale), (int)(tex.Height * Main.inventoryScale));
                Item tempItem = currentActiveArray.Item;
                ItemSlot.Draw(spriteBatch, ref tempItem, ItemSlotContext, slotPosition);

                if (centerSlotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    float boxWidth;
                    float boxHeight = -16;
                    Vector2 pos = Main.MouseScreen + new Vector2(34, 28);
                    var font = FontAssets.MouseText.Value;
                    string[] iconString =
                    {
                        "[c/FF67AA:Left click to close]",
                        "Right click to configure"
                    };
                    string widest2 = iconString.OrderBy(n => ChatManager.GetStringSize(font, n, Vector2.One).X).Last();
                    boxWidth = ChatManager.GetStringSize(font, widest2, Vector2.One).X + 20;
                    foreach (string str in iconString)
                    {
                        boxHeight += ChatManager.GetStringSize(font, str, Vector2.One).Y;
                    }

                    Utils.DrawInvBG(spriteBatch, new Rectangle((int)pos.X - 14, (int)pos.Y - 10, (int)boxWidth + 6, (int)boxHeight + 28), new Color(23, 25, 81, 255) * 0.925f);
                    foreach (string str in iconString)
                    {
                        Utils.DrawBorderString(spriteBatch, str, pos, Color.White);

                        pos.Y += ChatManager.GetStringSize(font, str, Vector2.One).Y;
                    }
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().currentlyActiveArray = null;
                        Recipe.FindRecipes();
                        timer = 0;
                    }
                }
            }
        }
    }
    public static class LightArrayPlayerExtensions
    {
        public static int LightArrayTimer(this Player player) => player.GetModPlayer<LightArrayPlayer>().lightArrayUITimer;

        public static BaseLightArray SetActiveArray(this Player player, BaseLightArray array) => player.GetModPlayer<LightArrayPlayer>().currentlyActiveArray = array;

        public static BaseLightArray CurrentActiveArray(this Player player) => player.GetModPlayer<LightArrayPlayer>().currentlyActiveArray;

        public static bool HasActiveArray(this Player player) => player.CurrentActiveArray() != null;

        public static void ResetActiveArray(this Player player)
        {
            player.SetActiveArray(null);
            player.GetModPlayer<LightArrayPlayer>().lightArrayUITimer = 0;
        }
    }
    public class LightArrayPlayer : ModPlayer
    {
        public int lightArrayUITimer = 0;
        public int lightArrayUITimerMax = 120;
        public BaseLightArray currentlyActiveArray = null;

        public override void PostUpdateMiscEffects()
        {
            if (currentlyActiveArray != null && lightArrayUITimer < lightArrayUITimerMax)
                lightArrayUITimer++;
        }

        public static Func<Item[], int, int, int> GetGamepadPointForSlot;

        public override void Load()
        {
            GetGamepadPointForSlot = (Func<Item[], int, int, int>)Delegate.CreateDelegate(typeof(Func<Item[], int, int, int>), null, typeof(ItemSlot).ReflectionGetMethodFromType("GetGamepadPointForSlot", BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            GetGamepadPointForSlot = null;
        }

        public override bool HoverSlot(Item[] inv, int context, int slot)
        {
            if (context == LightArrayInventoryUI.ItemSlotContext)
            {
                Item item = inv[slot];
                if (!PlayerInput.UsingGamepad)
                    UILinkPointNavigator.SuggestUsage(GetGamepadPointForSlot(inv, context, slot));

                bool shiftForcedOn = ItemSlot.ShiftForcedOn;
                if (ItemSlot.NotUsingGamepad && ItemSlot.Options.DisableLeftShiftTrashCan && !shiftForcedOn)
                {
                    if (ItemSlot.ControlInUse && !ItemSlot.Options.DisableQuickTrash)
                    {
                        if (!item.IsAir)
                            Main.cursorOverride = 6;
                    }
                    else if (ItemSlot.ShiftInUse)
                    {
                        bool flag = false;
                        if (Main.LocalPlayer.tileEntityAnchor.IsInValidUseTileEntity())
                            flag = Main.LocalPlayer.tileEntityAnchor.GetTileEntity().OverrideItemSlotHover(inv, context, slot);

                        if (!item.IsAir && !flag)
                        {
                            if (Main.player[Main.myPlayer].ItemSpace(item).CanTakeItemToPersonalInventory)
                                Main.cursorOverride = 8;

                            else if (Main.player[Main.myPlayer].chest != -1 && ChestUI.TryPlacingInChest(item, justCheck: true, context))
                                Main.cursorOverride = 9;
                        }
                    }
                }
                else if (ItemSlot.ShiftInUse)
                {
                    bool flag = false;
                    if (Main.LocalPlayer.tileEntityAnchor.IsInValidUseTileEntity())
                        flag = Main.LocalPlayer.tileEntityAnchor.GetTileEntity().OverrideItemSlotHover(inv, context, slot);

                    if (!item.IsAir && !flag)
                    {
                        if (Main.player[Main.myPlayer].ItemSpace(item).CanTakeItemToPersonalInventory)
                            Main.cursorOverride = 8;

                        else if (Main.player[Main.myPlayer].chest != -1 && ChestUI.TryPlacingInChest(item, justCheck: true, context))
                            Main.cursorOverride = 9;

                        else if (!ItemSlot.Options.DisableQuickTrash)
                            Main.cursorOverride = 6;
                    }
                }
                return true;
            }
            return false;
        }
    }
}