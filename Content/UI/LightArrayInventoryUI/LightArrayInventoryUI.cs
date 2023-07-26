using Radiance.Content.Items.BaseItems;
using Radiance.Items.Accessories;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;
using Terraria.GameContent.Creative;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Gamepad;
using static Radiance.Content.Items.BaseItems.BaseLightArray;

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

        public static float SlotColorMult => Math.Max(0.3f, 1f - Math.Min(1f, EaseOutExponent((float)Main.LocalPlayer.LightArrayConfigTimer() * 4 / Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimerMax, 3)));

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

            if (Main.LocalPlayer.LightArrayConfigOpen())
            {
                if (Main.LocalPlayer.LightArrayConfigTimer() < Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimerMax)
                    Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer++;
            }
            else
                Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer = 0;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.LocalPlayer.HasActiveArray())
            {
                DrawMainSlots(spriteBatch);
                if(Main.LocalPlayer.LightArrayConfigOpen())
                    DrawConfigSlots(spriteBatch);

                Texture2D tex = TextureAssets.InventoryBack.Value;
                Vector2 offset = tex.Size() / 2 * Main.inventoryScale;
                Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
                Vector2 slotPosition = screenCenter - offset;
                Rectangle centerSlotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * Main.inventoryScale), (int)(tex.Height * Main.inventoryScale));
                Item tempItem = currentActiveArray.Item;
                ItemSlot.Draw(spriteBatch, ref tempItem, ItemSlotContext, slotPosition);

                if (centerSlotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentFakeHoverText = 
                        "[c/FF67AA:Left click to close]\n" + 
                        "Right Click to configure";

                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        Main.LocalPlayer.ResetActiveArray();
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
            for (int i = 0; i < currentActiveArray.inventorySize; i++)
            {
                Vector2 newSlotPosition = slotPosition;
                switch (currentActiveArray.currentOrientation)
                {
                    default:

                        int amountDrawnSoFar = fancyRows * (fancyRows + 1) / 2 * FANCY_MAX_SLOTS_PER_ROW;
                        if (i > 0 && i % amountDrawnSoFar == 0)
                            fancyRows += 1;
                        amountDrawnSoFar = fancyRows * (fancyRows + 1) / 2 * FANCY_MAX_SLOTS_PER_ROW;

                        int amountInCurrentRow = 8 * fancyRows;
                        int realAmountToDrawInRow = Math.Min(amountInCurrentRow, currentActiveArray.inventorySize - (amountDrawnSoFar - amountInCurrentRow));
                        float rotation = i % (float)realAmountToDrawInRow / realAmountToDrawInRow;
                        float distance = fancyRows * ease * FANCY_DISTANCE_BETWEEN_SLOTS;
                        newSlotPosition += Vector2.UnitX.RotatedBy(rotation * ease * TwoPi - PiOver2 * ((fancyRows % 2 == 1) ? 1 : -1)) * distance;

                        break;
                    case PossibleUIOrientations.Compact:

                        ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 7f + 5f * GetSmoothIntRNG(Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArraySlotSeed, i));
                        Main.inventoryScale = 0.9f * ease;
                        offset = tex.Size() / 2 * Main.inventoryScale;

                        float x = 
                            -COMPACT_DISTANCE_BETWEEN_SLOTS * (i % COMPACT_MAX_SLOTS_PER_ROW) + 
                            (Math.Min(COMPACT_MAX_SLOTS_PER_ROW, (int)currentActiveArray.inventorySize) / 2 //proper positioning with less than 8 slots
                            * COMPACT_DISTANCE_BETWEEN_SLOTS - COMPACT_DISTANCE_BETWEEN_SLOTS / 2 //centering
                            );
                        float y = 
                            COMPACT_DISTANCE_BETWEEN_SLOTS * 
                            ((currentActiveArray.inventorySize - 1) / COMPACT_MAX_SLOTS_PER_ROW - i / COMPACT_MAX_SLOTS_PER_ROW) //keep the first slots at the top
                            + COMPACT_DISTANCE_BETWEEN_SLOTS; //stay above the center slot
                        newSlotPosition = Vector2.Lerp(screenCenter - offset, screenCenter - offset - new Vector2(x, y), ease);
                        break;

                    case PossibleUIOrientations.CompactRight:

                        ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 7f + 5f * GetSmoothIntRNG(Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArraySlotSeed, i));
                        Main.inventoryScale = 0.9f * ease;
                        offset = tex.Size() / 2 * Main.inventoryScale;

                        float xSide =
                            COMPACT_DISTANCE_BETWEEN_SLOTS * -(i % COMPACT_SIDE_MAX_SLOTS_PER_ROW)
                            - COMPACT_DISTANCE_BETWEEN_SLOTS
                            ;
                        float ySide =
                            COMPACT_DISTANCE_BETWEEN_SLOTS *
                            ((currentActiveArray.inventorySize - 1) / COMPACT_SIDE_MAX_SLOTS_PER_ROW - i / COMPACT_SIDE_MAX_SLOTS_PER_ROW) //keep the first slots at the top
                            - currentActiveArray.inventorySize / COMPACT_SIDE_MAX_SLOTS_PER_ROW * COMPACT_DISTANCE_BETWEEN_SLOTS / 2; //center
                            ;
                        newSlotPosition = Vector2.Lerp(screenCenter - offset, screenCenter - offset - new Vector2(xSide, ySide), ease);
                        break;
                }
                Rectangle slotRectangle = new Rectangle((int)newSlotPosition.X, (int)newSlotPosition.Y, (int)(tex.Width * Main.inventoryScale), (int)(tex.Height * Main.inventoryScale));
                if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    if (!Main.LocalPlayer.LightArrayConfigOpen())
                    {
                        Main.LocalPlayer.mouseInterface = true;
                        ItemSlot.OverrideHover(currentActiveArray.inventory, ItemSlotContext, i);
                        if (IsValidForLightArray(Main.mouseItem))
                        {
                            ItemSlot.LeftClick(currentActiveArray.inventory, ItemSlotContext, i);
                            ItemSlot.RightClick(currentActiveArray.inventory, ItemSlotContext, i);
                        }
                        if (Main.mouseLeftRelease && Main.mouseLeft)
                            Recipe.FindRecipes();

                        ItemSlot.MouseHover(currentActiveArray.inventory, ItemSlotContext, i);
                    }
                }
                ItemSlot.Draw(spriteBatch, currentActiveArray.inventory, ItemSlotContext, i, newSlotPosition, Color.White * SlotColorMult);
            }
        }

        private void DrawConfigSlots(SpriteBatch spriteBatch)
        {
            float ease = EaseOutExponent((float)Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer / Main.LocalPlayer.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimerMax, 9);
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

                    switch(currentOption)
                    {
                        case LightArrayConfigOptions.Orientation:
                            strings =
                                "[c/FF0067:Inventory UI Orientation]\n" + 
                                $"Current Selection: {(
                                currentActiveArray.currentOrientation == PossibleUIOrientations.Fancy ? "Fancy" :
                                currentActiveArray.currentOrientation == PossibleUIOrientations.Compact ? "Compact" :
                                "Compact Side")}";
                            break;
                        case LightArrayConfigOptions.AutoPickup:
                            strings = 
                                "[c/FF0067:Automatic Item Pickup]\n" + 
                                "When enabled, items that are picked up will automatically be placed into this Light Array\n" + 
                                $"Current Selection: {GetAutoPickupString(currentActiveArray, "AutoPickup")}";
                            break;
                        case LightArrayConfigOptions.AutoPickupExistingItems:
                            strings =
                                "[c/FF0067:Conditional Automatic Item Pickup]\n" +
                                "When enabled, items that are picked up will automatically be placed into this Light Array if an item of the same type already exists inside of it\n" +
                                $"Current Selection: {GetAutoPickupString(currentActiveArray, "AutoPickupCurrentItems")}";
                            break;
                    }
                    if(Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        switch (currentOption)
                        {
                            case LightArrayConfigOptions.Orientation:
                                currentActiveArray.currentOrientation = (PossibleUIOrientations)(((int)currentActiveArray.currentOrientation + 1) % Enum.GetValues(typeof(PossibleUIOrientations)).Length);
                                break;
                            case LightArrayConfigOptions.AutoPickup:
                                currentActiveArray.optionsDictionary["AutoPickup"] = (currentActiveArray.optionsDictionary["AutoPickup"] + 1) % Enum.GetValues(typeof(AutoPickupModes)).Length;
                                break;
                            case LightArrayConfigOptions.AutoPickupExistingItems:
                                currentActiveArray.optionsDictionary["AutoPickupCurrentItems"] = (currentActiveArray.optionsDictionary["AutoPickupCurrentItems"] + 1) % Enum.GetValues(typeof(AutoPickupModes)).Length;
                                break;
                        }
                    }
                    Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentFakeHoverText = strings;
                }
                int item = ModContent.ItemType<DebugAccessory>();
                switch(currentOption)
                {
                    case LightArrayConfigOptions.Orientation:
                        switch(currentActiveArray.currentOrientation)
                        {
                            case PossibleUIOrientations.Fancy:
                                item = ItemID.MulticolorWrench;
                                break;
                            case PossibleUIOrientations.Compact:
                                item = ItemID.Wrench;
                                break;
                            case PossibleUIOrientations.CompactRight:
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
        private static string GetAutoPickupString(BaseLightArray array, string key) => (AutoPickupModes)array.optionsDictionary[key] switch
        {
            AutoPickupModes.Disabled => "Disabled",
            AutoPickupModes.Enabled => "Enabled",
            AutoPickupModes.IfInventoryIsFull => "Only if Inventory is Full",
            _ => ""
        };
    }

    public static class LightArrayPlayerExtensions
    {
        public static int LightArrayTimer(this Player player) => player.GetModPlayer<LightArrayPlayer>().lightArrayUITimer;
        public static int LightArrayConfigTimer(this Player player) => player.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer;
        public static bool LightArrayConfigOpen(this Player player) => player.GetModPlayer<LightArrayPlayer>().lightArrayConfigOpen;

        public static BaseLightArray SetActiveArray(this Player player, BaseLightArray array) => player.GetModPlayer<LightArrayPlayer>().currentlyActiveArray = array;

        public static BaseLightArray CurrentActiveArray(this Player player) => player.GetModPlayer<LightArrayPlayer>().currentlyActiveArray;

        public static bool HasActiveArray(this Player player) => player.CurrentActiveArray() != null;

        public static void ResetActiveArray(this Player player)
        {
            player.SetActiveArray(null);
            player.GetModPlayer<LightArrayPlayer>().lightArrayConfigOpen = false;
            player.GetModPlayer<LightArrayPlayer>().lightArrayUITimer = player.GetModPlayer<LightArrayPlayer>().lightArrayConfigTimer = 0;
        }
    }

    public class LightArrayPlayer : ModPlayer
    {
        public int lightArrayUITimer = 0;
        public int lightArrayUITimerMax = 120;

        public bool lightArrayConfigOpen = false;
        public int lightArrayConfigTimer = 0;
        public int lightArrayConfigTimerMax = 60;
        public int lightArraySlotSeed = 0;

        public BaseLightArray currentlyActiveArray = null;

        public override void PostUpdateMiscEffects()
        {
             lightArrayConfigTimerMax = 60;
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