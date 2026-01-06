using Radiance.Content.Items.BaseItems;
using Radiance.Core.Config;
using Radiance.Items.Accessories;
using System.Reflection;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Radiance.Content.UI.LightArrayInventoryUI
{
    internal class LightArrayInventoryUI : SmartUIState
    {
        public const int ItemSlotContext = 607;
        public static LightArrayInventoryUI Instance { get; set; }

        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

        public LightArrayInventoryUI()
        {
            Instance = this;
        }

        static LightArrayInventoryUI()
        {
            LeftClickToClose = Language.GetOrRegister($"{Radiance.COMMON_STRING_PREFIX}.{nameof(LeftClickToClose)}", () => "Left click to close");
            RightClickToConfigure = Language.GetOrRegister($"{Radiance.COMMON_STRING_PREFIX}.{nameof(RightClickToConfigure)}", () => "Right click to configure");
        }

        public enum LightArrayConfigOptions
        {
            Orientation,
            AutoPickup,
            AutoPickupExistingItems,
        }

        private const int FANCY_MAX_SLOTS_PER_ROW = 8;
        private const int FANCY_DISTANCE_BETWEEN_SLOTS = 68;
        private const int COMPACT_MAX_SLOTS_PER_ROW = 8;
        private const int COMPACT_DISTANCE_BETWEEN_SLOTS = 52;
        private const int COMPACT_SIDE_MAX_SLOTS_PER_ROW = 6;

        public static float SlotColorMult => Math.Max(0.3f, 1f - Math.Min(1f, EaseOutExponent((float)Main.LocalPlayer.LightArrayConfigTimer() * 4 / LightArrayPlayer.LIGHT_ARRAY_CONFIG_TIMER_MAX, 3)));
        public static BaseLightArray CurrentActiveArray => Main.LocalPlayer.GetCurrentUIItem() as BaseLightArray;
        public ref int timer => ref Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayUITimer;
        public int timerMax => LightArrayPlayer.LIGHT_ARRAY_UI_TIMER_MAX;
        public override bool Visible => CurrentActiveArray is not null && Main.playerInventory && Main.LocalPlayer.active && !Main.LocalPlayer.dead;
        public UIElement centerIcon = new();

        private static readonly LocalizedText LeftClickToClose;
        private static readonly LocalizedText RightClickToConfigure;

        public override void Update(GameTime gameTime)
        {
            bool flag = false;
            if (CurrentActiveArray is not null)
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Main.LocalPlayer.inventory[i].ModItem == CurrentActiveArray)
                        flag = true;
                }
                if (!flag)
                    Main.LocalPlayer.ResetActiveItemUI();
            }

            if (RadianceConfig.ReducedMotion)
            {
                timer = timerMax;
                if (Main.LocalPlayer.LightArrayConfigOpen())
                    Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer = LightArrayPlayer.LIGHT_ARRAY_CONFIG_TIMER_MAX;
                else
                    Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer = 0;
                return;
            }
            else
            {
                if (Main.LocalPlayer.LightArrayConfigOpen())
                {
                    if (Main.LocalPlayer.LightArrayConfigTimer() < LightArrayPlayer.LIGHT_ARRAY_CONFIG_TIMER_MAX)
                        Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer++;
                }
                else if (Main.LocalPlayer.LightArrayConfigTimer() > 0)
                    Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer -= 3;

                if (Main.LocalPlayer.LightArrayConfigTimer() < 0)
                    Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer = 0;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentActiveArray is not null)
            {
                DrawMainSlots(spriteBatch);
                if (Main.LocalPlayer.LightArrayConfigTimer() > 0)
                    DrawConfigSlots(spriteBatch);

                Texture2D tex = TextureAssets.InventoryBack.Value;
                Vector2 offset = tex.Size() / 2 * Main.inventoryScale;
                Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
                Vector2 slotPosition = screenCenter - offset;
                Rectangle centerSlotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * Main.inventoryScale), (int)(tex.Height * Main.inventoryScale));
                Item tempItem = CurrentActiveArray.Item;
                ItemSlot.Draw(spriteBatch, ref tempItem, ItemSlotContext, slotPosition);

                if (centerSlotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;

                    Main.LocalPlayer.SetFakeHoverText(
                        $"[c/FF67AA:{LeftClickToClose.Value}]\n" +
                        $"[c/FF67AA:{RightClickToConfigure.Value}]");

                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        Main.LocalPlayer.ResetActiveItemUI();
                        Recipe.FindRecipes();
                        timer = 0;
                    }
                    else if (Main.mouseRightRelease && Main.mouseRight)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigOpen = !Main.LocalPlayer.LightArrayConfigOpen();
                    }
                }
            }
        }

        private void DrawMainSlots(SpriteBatch spriteBatch)
        {
            float ease = EaseOutExponent((float)timer / timerMax, 9);
            Main.inventoryScale = 0.9f * ease;
            Texture2D tex = TextureAssets.InventoryBack.Value;
            Vector2 offset = tex.Size() / 2 * Main.inventoryScale;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            Vector2 slotPosition = screenCenter - offset;
            int fancyRows = 1;
            for (int i = 0; i < CurrentActiveArray.inventorySize; i++)
            {
                Vector2 newSlotPosition = slotPosition;
                switch (CurrentActiveArray.optionsDictionary["UIOrientation"])
                {
                    default:

                        int amountDrawnSoFar = fancyRows * (fancyRows + 1) / 2 * FANCY_MAX_SLOTS_PER_ROW;
                        if (i > 0 && i % amountDrawnSoFar == 0)
                            fancyRows += 1;
                        amountDrawnSoFar = fancyRows * (fancyRows + 1) / 2 * FANCY_MAX_SLOTS_PER_ROW;

                        int amountInCurrentRow = 8 * fancyRows;
                        int realAmountToDrawInRow = Math.Min(amountInCurrentRow, CurrentActiveArray.inventorySize - (amountDrawnSoFar - amountInCurrentRow));
                        float rotation = i % (float)realAmountToDrawInRow / realAmountToDrawInRow;
                        float distance = fancyRows * ease * FANCY_DISTANCE_BETWEEN_SLOTS;
                        newSlotPosition += Vector2.UnitX.RotatedBy(rotation * ease * TwoPi - PiOver2 * ((fancyRows % 2 == 1) ? 1 : -1)) * distance;

                        break;

                    case (int)BaseLightArray.PossibleUIOrientations.Compact:

                        ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 7f + 5f * GetSmoothIntRNG(Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArraySlotSeed, i));
                        Main.inventoryScale = 0.9f * ease;
                        offset = tex.Size() / 2 * Main.inventoryScale;

                        float x =
                            -COMPACT_DISTANCE_BETWEEN_SLOTS * (i % COMPACT_MAX_SLOTS_PER_ROW) +
                            (Math.Min(COMPACT_MAX_SLOTS_PER_ROW, (int)CurrentActiveArray.inventorySize) / 2 //proper positioning with less than 8 slots
                            * COMPACT_DISTANCE_BETWEEN_SLOTS - COMPACT_DISTANCE_BETWEEN_SLOTS / 2 //centering
                            );
                        float y =
                            COMPACT_DISTANCE_BETWEEN_SLOTS *
                            ((CurrentActiveArray.inventorySize - 1) / COMPACT_MAX_SLOTS_PER_ROW - i / COMPACT_MAX_SLOTS_PER_ROW) //keep the first slots at the top
                            + COMPACT_DISTANCE_BETWEEN_SLOTS; //stay above the center slot
                        newSlotPosition = Vector2.Lerp(screenCenter - offset, screenCenter - offset - new Vector2(x, y), ease);
                        break;

                    case (int)BaseLightArray.PossibleUIOrientations.CompactRight:

                        ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 7f + 5f * GetSmoothIntRNG(Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArraySlotSeed, i));
                        Main.inventoryScale = 0.9f * ease;
                        offset = tex.Size() / 2 * Main.inventoryScale;

                        float xSide =
                            COMPACT_DISTANCE_BETWEEN_SLOTS * -(i % COMPACT_SIDE_MAX_SLOTS_PER_ROW)
                            - COMPACT_DISTANCE_BETWEEN_SLOTS
                            ;
                        float ySide =
                            COMPACT_DISTANCE_BETWEEN_SLOTS *
                            ((CurrentActiveArray.inventorySize - 1) / COMPACT_SIDE_MAX_SLOTS_PER_ROW - i / COMPACT_SIDE_MAX_SLOTS_PER_ROW) //keep the first slots at the top
                            - CurrentActiveArray.inventorySize / COMPACT_SIDE_MAX_SLOTS_PER_ROW * COMPACT_DISTANCE_BETWEEN_SLOTS / 2; //center
                        ;
                        newSlotPosition = Vector2.Lerp(screenCenter - offset, screenCenter - offset - new Vector2(xSide, ySide), ease);
                        break;
                }
                Rectangle slotRectangle = new Rectangle((int)newSlotPosition.X, (int)newSlotPosition.Y, (int)(tex.Width * Main.inventoryScale), (int)(tex.Height * Main.inventoryScale));
                if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    if (Main.LocalPlayer.LightArrayConfigTimer() == 0)
                    {
                        Main.LocalPlayer.mouseInterface = true;
                        ItemSlot.OverrideHover(CurrentActiveArray.inventory, ItemSlotContext, i);
                        if (BaseLightArray.IsValidForLightArray(Main.mouseItem))
                        {
                            ItemSlot.LeftClick(CurrentActiveArray.inventory, ItemSlotContext, i);
                            ItemSlot.RightClick(CurrentActiveArray.inventory, ItemSlotContext, i);
                        }
                        if (Main.mouseLeftRelease && Main.mouseLeft)
                            Recipe.FindRecipes();

                        ItemSlot.MouseHover(CurrentActiveArray.inventory, ItemSlotContext, i);
                    }
                }
                ItemSlot.Draw(spriteBatch, CurrentActiveArray.inventory, ItemSlotContext, i, newSlotPosition, Color.White * SlotColorMult);
            }
        }

        private void DrawConfigSlots(SpriteBatch spriteBatch)
        {
            float ease = EaseOutExponent((float)Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer / LightArrayPlayer.LIGHT_ARRAY_CONFIG_TIMER_MAX, 9);
            if (!Main.LocalPlayer.LightArrayConfigOpen())
                ease = EaseOutExponent((float)Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer / LightArrayPlayer.LIGHT_ARRAY_CONFIG_TIMER_MAX, 4);

            Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/LightArrayInventorySlot").Value;
            Vector2 offset = tex.Size() / 2 * Main.inventoryScale;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            Vector2 slotPosition = screenCenter - offset;
            int rows = 1;
            for (int i = 0; i < Enum.GetValues(typeof(LightArrayConfigOptions)).Length; i++)
            {
                LightArrayConfigOptions currentOption = (LightArrayConfigOptions)i;
                int amountDrawnSoFar = rows * (rows + 1) / 2 * FANCY_MAX_SLOTS_PER_ROW;
                if (i > 0 && i % amountDrawnSoFar == 0)
                    rows += 1;
                amountDrawnSoFar = rows * (rows + 1) / 2 * FANCY_MAX_SLOTS_PER_ROW;

                int amountInCurrentRow = 8 * rows;
                int realAmountToDrawInRow = Math.Min(amountInCurrentRow, Enum.GetValues(typeof(LightArrayConfigOptions)).Length - (amountDrawnSoFar - amountInCurrentRow));
                float rotation = i % (float)realAmountToDrawInRow / realAmountToDrawInRow;
                float distance = rows * ease * FANCY_DISTANCE_BETWEEN_SLOTS;
                Vector2 newSlotPosition = slotPosition + Vector2.UnitX.RotatedBy(rotation * ease * TwoPi - PiOver2 * ((rows % 2 == 1) ? 1 : -1)) * distance;

                Rectangle slotRectangle = new Rectangle((int)newSlotPosition.X, (int)newSlotPosition.Y, (int)(tex.Width * Main.inventoryScale), (int)(tex.Height * Main.inventoryScale));
                if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    string strings = $"[c/FF0067:Error!]";

                    switch (currentOption)
                    {
                        case LightArrayConfigOptions.Orientation:
                            strings =
                                "[c/FF0067:Inventory UI Orientation]\n" +
                                $@"Current Selection: [c/FF67AA:{(
                                CurrentActiveArray.optionsDictionary["UIOrientation"] == (int)BaseLightArray.PossibleUIOrientations.Fancy ? "Fancy" :
                                CurrentActiveArray.optionsDictionary["UIOrientation"] == (int)BaseLightArray.PossibleUIOrientations.Compact ? "Compact" :
                                "Compact Side")}]";
                            break;

                        case LightArrayConfigOptions.AutoPickup:
                            strings =
                                "[c/FF0067:Automatic Item Pickup]\n" +
                                "When enabled, items that are picked up will automatically be placed into this Light Array\n" +
                                $"Current Selection: [c/FF67AA:{GetAutoPickupString(CurrentActiveArray, "AutoPickup")}]";
                            break;

                        case LightArrayConfigOptions.AutoPickupExistingItems:
                            strings =
                                "[c/FF0067:Conditional Automatic Item Pickup]\n" +
                                "When enabled, items that are picked up will automatically be placed into this Light Array if an item of the same type already exists inside of it\n" +
                                $"Current Selection: [c/FF67AA:{GetAutoPickupString(CurrentActiveArray, "AutoPickupCurrentItems")}]";
                            break;
                    }
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        switch (currentOption)
                        {
                            case LightArrayConfigOptions.Orientation:
                                CurrentActiveArray.optionsDictionary["UIOrientation"] = ((CurrentActiveArray.optionsDictionary["UIOrientation"] + 1) % Enum.GetValues(typeof(BaseLightArray.PossibleUIOrientations)).Length);
                                break;

                            case LightArrayConfigOptions.AutoPickup:
                                CurrentActiveArray.optionsDictionary["AutoPickup"] = (CurrentActiveArray.optionsDictionary["AutoPickup"] + 1) % Enum.GetValues(typeof(BaseLightArray.AutoPickupModes)).Length;
                                break;

                            case LightArrayConfigOptions.AutoPickupExistingItems:
                                CurrentActiveArray.optionsDictionary["AutoPickupCurrentItems"] = (CurrentActiveArray.optionsDictionary["AutoPickupCurrentItems"] + 1) % Enum.GetValues(typeof(BaseLightArray.AutoPickupModes)).Length;
                                break;
                        }
                    }
                    Main.LocalPlayer.SetFakeHoverText(strings);
                }
                int item = ModContent.ItemType<DebugAccessory>();
                switch (currentOption)
                {
                    case LightArrayConfigOptions.Orientation:
                        switch ((BaseLightArray.PossibleUIOrientations)CurrentActiveArray.optionsDictionary["UIOrientation"])
                        {
                            case BaseLightArray.PossibleUIOrientations.Fancy:
                                item = ItemID.MulticolorWrench;
                                break;

                            case BaseLightArray.PossibleUIOrientations.Compact:
                                item = ItemID.Wrench;
                                break;

                            case BaseLightArray.PossibleUIOrientations.CompactRight:
                                item = ItemID.BlueWrench;
                                break;
                        }
                        break;

                    case LightArrayConfigOptions.AutoPickup:
                        item = ItemID.Chest;
                        break;

                    case LightArrayConfigOptions.AutoPickupExistingItems:
                        item = ItemID.GoldChest;
                        break;
                }
                spriteBatch.Draw(tex, newSlotPosition, null, Color.White * Main.inventoryBack.A, 0, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0);
                ItemSlot.DrawItemIcon(GetItem(item), ItemSlotContext, spriteBatch, newSlotPosition + Main.inventoryScale * tex.Size() / 2, Main.inventoryScale, 32f, Color.White);
            }
        }

        private static string GetAutoPickupString(BaseLightArray array, string key) => (BaseLightArray.AutoPickupModes)array.optionsDictionary[key] switch
        {
            BaseLightArray.AutoPickupModes.Disabled => "Disabled",
            BaseLightArray.AutoPickupModes.Enabled => "Enabled",
            BaseLightArray.AutoPickupModes.IfInventoryIsFull => "Only if Inventory is Full",
            _ => ""
        };
    }

    public static class LightArrayPlayerExtensions
    {
        public static int LightArrayTimer(this Player player) => player.GetModPlayer<LightArrayPlayer>().lightArrayUITimer;

        public static int LightArrayConfigTimer(this Player player) => player.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer;

        public static bool LightArrayConfigOpen(this Player player) => player.GetModPlayer<LightArrayPlayer>().lightArrayConfigOpen;
    }

    public class LightArrayPlayer : ModPlayer
    {
        public int lightArrayUITimer = 0;
        public const int LIGHT_ARRAY_UI_TIMER_MAX = 120;

        public bool lightArrayConfigOpen = false;
        public int lightArrayConfigTimer = 0;
        public const int LIGHT_ARRAY_CONFIG_TIMER_MAX = 60;
        public int lightArraySlotSeed = 0;

        public override void PostUpdateMiscEffects()
        {
            Item item = Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentlyActiveUIItem;
            if (item is not null && item.ModItem is BaseLightArray && lightArrayUITimer < LIGHT_ARRAY_UI_TIMER_MAX)
                lightArrayUITimer++;
        }

        public static Func<Item[], int, int, int> GetGamepadPointForSlot;

        public override void Load()
        {
            GetGamepadPointForSlot = (Func<Item[], int, int, int>)Delegate.CreateDelegate(typeof(Func<Item[], int, int, int>), null, typeof(ItemSlot).GetMethod("GetGamepadPointForSlot", BindingFlags.Static | BindingFlags.NonPublic));
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