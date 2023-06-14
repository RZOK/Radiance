using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.UI.LightArrayInventoryUI;
using ReLogic.Graphics;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Permissions;
using Terraria.UI;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseLightArray : ModItem, IInventory
    {
        public BaseLightArray(byte inventorySize)
        {
            this.inventorySize = inventorySize;
        }

        public byte inventorySize;
        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => Array.Empty<byte>();
        public byte[] outputtableSlots => Array.Empty<byte>();
        public Color arrayColor;

        public override bool ConsumeItem(Player player) => false;

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            if (!player.HasActiveArray())
            {
                arrayColor = new Color(255, 0, 103);
                player.SetActiveArray(this);
                return;
            }
            player.ResetActiveArray();
        }

        public override sealed void SetDefaults()
        {
            inventory = Enumerable.Repeat(new Item(0), inventorySize).ToArray();
            SetExtraDefaults();
        }

        public virtual void SetExtraDefaults()
        { }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            List<byte> slotsWithItems = this.GetSlotsWithItems();
            for (int i = 0; i < Math.Ceiling((float)slotsWithItems.Count / 16); i++)
            {
                int realAmountToDraw = Math.Min(16, slotsWithItems.Count - i * 16);
                TooltipLine itemDisplayLine = new(Mod, "LightArrayItems" + i, "");
                itemDisplayLine.Text = new String('M', 2 * realAmountToDraw + 3) + i;
                tooltips.Add(itemDisplayLine);
            }
        }
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            List<byte> slotsWithItems = this.GetSlotsWithItems();
            if (Main.SettingsEnabled_OpaqueBoxBehindTooltips && line.Name == "LightArrayItems0")
            {
                int width = Math.Min(16, slotsWithItems.Count) * 36;
                int height = (int)Math.Ceiling((double)(slotsWithItems.Count / 16f)) * 28;
                RadianceUtils.DrawRadianceInvBG(Main.spriteBatch, line.X - 8, line.Y - 8, width + 10, height + 8);
            }
            if (line.Name.StartsWith("LightArrayItems"))
            {
                int number = int.Parse(line.Name.Last().ToString());
                for (int i = number * 16; i < (number + 1) * 16; i++)
                {
                    Item item = inventory[slotsWithItems[i]]; 
                    Vector2 pos = new Vector2(line.X + 16 + 36 * (i - number * 16), line.Y + 10);
                    DynamicSpriteFont font = FontAssets.MouseText.Value;

                    ItemSlot.DrawItemIcon(item, 0, Main.spriteBatch, pos, 1f, 32, Color.White);
                    if (item.stack > 1)
                        Utils.DrawBorderStringFourWay(Main.spriteBatch, font, item.stack.ToString(), pos.X - 14, pos.Y + 12, Color.White, Color.Black, Vector2.UnitY * font.MeasureString(item.stack.ToString()).Y / 2, 0.85f);
                    
                    if (slotsWithItems[i] == slotsWithItems.Last())
                        break;
                }
                return false;
            }
            return true;
        }

        public override sealed void SaveData(TagCompound tag)
        {
            this.SaveInventory(tag);
            SaveExtraData(tag);
        }

        public virtual void SaveExtraData(TagCompound tag)
        { }

        public override sealed void LoadData(TagCompound tag)
        {
            this.LoadInventory(tag, inventorySize);
            LoadExtraData(tag);
        }

        public virtual void LoadExtraData(TagCompound tag)
        { }

        public static bool IsValidForLightArray(Item item)
        {
            if (item.ModItem != null && item.ModItem is BaseLightArray)
                return false;
            return true;
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

    public class LightArrayILEdits : ILoadable
    {
        private Action<Item[], int> CollectItems;

        private delegate bool ConsumeForCraftDelegate(Item item, Item requiredItem, ref int stackRequired);

        private ConsumeForCraftDelegate ConsumeForCraft;

        private delegate void CheckArraysForItemsDelegate(Item requiredItem, ref int stackRequried);

        private delegate void SetSlotColorDelegate(ref Texture2D texture, int context);

        public void Load(Mod mod)
        {
            IL_Recipe.CollectItemsToCraftWithFrom += IL_Recipe_CollectItemsToCraftWithFrom;
            IL_Recipe.Create += On_Recipe_Create;
            IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
            IL_ItemSlot.OverrideHover_ItemArray_int_int += IL_ItemSlot_OverrideHover_ItemArray_int_int;
            IL_ItemSlot.PickItemMovementAction += IL_ItemSlot_PickItemMovementAction;
            IL_ItemSlot.OverrideLeftClick += IL_ItemSlot_OverrideLeftClick;
            //TODO: EQUIPPABLE RIGHT CLICK COMPAT

            ConsumeForCraft = (ConsumeForCraftDelegate)Delegate.CreateDelegate(typeof(ConsumeForCraftDelegate), Main.recipe[Main.focusRecipe], Main.recipe[Main.focusRecipe].ReflectionGetMethod("ConsumeForCraft", BindingFlags.NonPublic | BindingFlags.Instance));
            CollectItems = (Action<Item[], int>)Delegate.CreateDelegate(typeof(Action<Item[], int>), null, typeof(Recipe).ReflectionGetMethodFromType("CollectItems", BindingFlags.Static | BindingFlags.NonPublic, new Type[] { typeof(Item[]), typeof(int) }));
        }

        public void Unload()
        {
            IL_Recipe.CollectItemsToCraftWithFrom -= IL_Recipe_CollectItemsToCraftWithFrom;
            IL_Recipe.Create -= On_Recipe_Create;
            IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
            IL_ItemSlot.OverrideHover_ItemArray_int_int -= IL_ItemSlot_OverrideHover_ItemArray_int_int;
            IL_ItemSlot.PickItemMovementAction -= IL_ItemSlot_PickItemMovementAction;
            IL_ItemSlot.OverrideLeftClick -= IL_ItemSlot_OverrideLeftClick;

            ConsumeForCraft = null;
            CollectItems = null;
        }

        #region Inventory Shift-Click compatability

        private void IL_ItemSlot_OverrideLeftClick(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.Index = cursor.Instrs.Count;
            if (!cursor.TryGotoPrev(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(2),
                i => i.MatchLdelemRef()))
            {
                RadianceUtils.LogIlError("Light Array Shift Clicking", "Couldn't navigate to before final return true");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(CheckForArrayOrChest);
            cursor.Emit(OpCodes.Ldc_I4_1);
            cursor.Emit(OpCodes.Ret);
            cursor.Emit(OpCodes.Ldarg_0);
        }

        private void CheckForArrayOrChest(Item item, int context)
        {
            if (Main.LocalPlayer.chest != -1 && ChestUI.TryPlacingInChest(item, true, context))
            {
                ChestUI.TryPlacingInChest(item, false, context);
                return;
            }
            if (Main.LocalPlayer.HasActiveArray() && Main.LocalPlayer.CurrentActiveArray().CanInsertItemIntoInventory(item, true))
            {
                Main.LocalPlayer.CurrentActiveArray().SafeInsertItemIntoSlots(item, true);
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }

        #endregion Inventory Shift-Click compatability

        #region Item Movement Action

        private void IL_ItemSlot_PickItemMovementAction(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel[] switchLabels = null;
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchSwitch(out switchLabels)))
            {
                RadianceUtils.LogIlError("Light Array Item Movement", "Couldn't navigate to after result initialization");
                return;
            }
            int offset = 0;
            if (cursor.Prev.Previous.MatchSub())
                cursor.Prev.Previous.Previous.MatchLdcI4(out offset);

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldc_I4, LightArrayInventoryUI.ItemSlotContext);
            cursor.Emit(OpCodes.Beq, switchLabels[3 - offset]);
        }

        #endregion Item Movement Action

        #region Modify Cursor Override with Light Arrays

        private void IL_ItemSlot_OverrideHover_ItemArray_int_int(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            ILLabel postSwitchStatementLabel = null;
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdsfld(typeof(Main), nameof(Main.player)),
                i => i.MatchLdsfld(typeof(Main), nameof(Main.myPlayer)),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(Player), nameof(Player.chest)),
                i => i.MatchLdcI4(-1),
                i => i.MatchBeq(out postSwitchStatementLabel)))
            {
                RadianceUtils.LogIlError("Light Array Vanilla Inventory ItemSlot Modifications", "Could not jump to first chest detection");
                return;
            }
            cursor.Index++;
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(FirstIsInChestOrArray);
            cursor.Emit(OpCodes.Brfalse, postSwitchStatementLabel);

            ILLabel cursorSetLabel = cursor.MarkLabel();
            cursor.Emit(OpCodes.Ldc_I4, 9);
            cursor.Emit<Main>(OpCodes.Stsfld, nameof(Main.cursorOverride));
            cursor.Emit(OpCodes.Br, postSwitchStatementLabel);

            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdsfld(typeof(Main), nameof(Main.player)),
                i => i.MatchLdsfld(typeof(Main), nameof(Main.myPlayer)),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(Player), nameof(Player.chest)),
                i => i.MatchLdcI4(-1),
                i => i.MatchBeq(out var _)))
            {
                RadianceUtils.LogIlError("Light Array Vanilla Inventory ItemSlot Modifications", "Could not jump to second chest detection");
                return;
            }
            cursor.Index++;
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(SecondIsInChestOrArray);
            cursor.Emit(OpCodes.Brtrue, cursorSetLabel);
            cursor.Emit<Main>(OpCodes.Ldsfld, nameof(Main.player));
        }

        private bool FirstIsInChestOrArray(Item item, int itemSlotContext) =>
            (Main.player[Main.myPlayer].chest != -1 && ChestUI.TryPlacingInChest(item, true, itemSlotContext)) ||
            (BaseLightArray.IsValidForLightArray(item) && Main.LocalPlayer.HasActiveArray() && Main.LocalPlayer.CurrentActiveArray().CanInsertItemIntoInventory(item, true));

        private bool SecondIsInChestOrArray(Item item, int itemSlotContext)
        {
            if (BaseLightArray.IsValidForLightArray(item) && Main.LocalPlayer.HasActiveArray() && Main.LocalPlayer.CurrentActiveArray().CanInsertItemIntoInventory(item, true))
                return true;

            if (Main.player[Main.myPlayer].chest != -1)
            {
                if (ChestUI.TryPlacingInChest(item, true, itemSlotContext))
                    return true;
            }
            return false;
        }

        #endregion Modify Cursor Override with Light Arrays

        #region ItemSlot Draw Texture

        private void IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdloc(7),
                i => i.MatchLdarg(out var _),
                i => i.MatchLdloca(24),
                i => i.MatchInitobj(out var _),
                i => i.MatchLdloc(24),
                i => i.MatchLdloc(8)
                ))
            {
                RadianceUtils.LogIlError("Light Array Item Slot Color", "Couldn't navigate to before draw function");
                return;
            }

            cursor.Emit(OpCodes.Ldloca, 7); //push texture onto stack
            cursor.Emit(OpCodes.Ldarg_2); //push context onto stack
            cursor.EmitDelegate<SetSlotColorDelegate>(SetTexture);
        }

        public void SetTexture(ref Texture2D texture, int context)
        {
            if (context == LightArrayInventoryUI.ItemSlotContext)
                texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/LightArrayInventorySlot").Value;
        }

        #endregion ItemSlot Draw Texture

        #region Recipe Consumption

        private void On_Recipe_Create(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(7),
                i => i.MatchLdcI4(58),
                i => i.MatchBlt(out var _)
                ))
            {
                RadianceUtils.LogIlError("Light Array Recipe Compatability", "Couldn't navigate to after inventory loop");
                return;
            }

            //cursor.Emit(OpCodes.Ldarg_0); //recipe instance
            //cursor.Emit(OpCodes.Ldloc_1); //item to eat
            cursor.Emit(OpCodes.Ldloc_3); //item required
            cursor.Emit(OpCodes.Ldloca, 4); //stack required
            cursor.EmitDelegate<CheckArraysForItemsDelegate>(CheckArraysForItems);
        }

        private void CheckArraysForItems(Item requiredItem, ref int stackRequired)
        {
            if (Main.LocalPlayer.HasActiveArray())
            {
                for (int i = 0; i < Main.LocalPlayer.CurrentActiveArray().inventorySize; i++)
                {
                    Item itemToConsume = Main.LocalPlayer.CurrentActiveArray().inventory[i];
                    ConsumeForCraft(itemToConsume, requiredItem, ref stackRequired);
                }
            }
        }

        #endregion Recipe Consumption

        #region Recipe Detection

        private void IL_Recipe_CollectItemsToCraftWithFrom(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchCall<Recipe>("CollectItems")))
            {
                RadianceUtils.LogIlError("Light Array Recipe Item Collection", "Couldn't navigate to after inventory check.");
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CollectItemsToCraftWith);
        }
        private void CollectItemsToCraftWith(Player player)
        {
            if (player.HasActiveArray())
                CollectItems(player.CurrentActiveArray().inventory, player.CurrentActiveArray().inventorySize);
        }

        #endregion Recipe Detection
    }
}