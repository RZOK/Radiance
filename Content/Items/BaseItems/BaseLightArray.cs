﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Tiles;
using Radiance.Content.UI.LightArrayInventoryUI;
using ReLogic.Graphics;
using System.Reflection;
using Terraria.UI;
using static Radiance.Content.Items.BaseItems.BaseLightArray;
using static Terraria.Player;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseLightArray : ModItem, IInventory, IOverrideInputtableSlotsFlag
    {
        public BaseLightArray(byte inventorySize, string miniTexture)
        {
            this.inventorySize = inventorySize;
            this.miniTexture = miniTexture;
        }
        public string miniTexture;
        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => Array.Empty<byte>();
        public byte[] outputtableSlots => Array.Empty<byte>();
        public int inventorySize { get; set; }

        public ItemImprintData itemImprintData;
        public LightArrayBaseTileEntity currentBase;

        public enum PossibleUIOrientations
        {
            Fancy,
            Compact,
            CompactRight
        }
        public enum AutoPickupModes
        {
            Disabled,
            Enabled,
            IfInventoryIsFull
        }

        public Dictionary<string, int> optionsDictionary = new Dictionary<string, int>()
        {
            ["AutoPickup"] = (int)AutoPickupModes.Disabled,
            ["AutoPickupCurrentItems"] = (int)AutoPickupModes.Disabled,
            ["UIOrientation"] = (int)PossibleUIOrientations.Fancy,
        };

        public override bool ConsumeItem(Player player) => false;

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            if (!player.HasActiveArray())
            {
                player.GetModPlayer<LightArrayPlayer>().lightArraySlotSeed = Main.rand.Next(10000);
                player.SetActiveArray(this);
                return;
            }
            if (player.CurrentActiveArray() != this)
            {
                player.ResetActiveArray();
                player.GetModPlayer<LightArrayPlayer>().lightArraySlotSeed = Main.rand.Next(10000);
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

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.LocalPlayer.HasActiveArray() && Main.LocalPlayer.CurrentActiveArray() == this && Main.mouseItem != Item)
            {
                Texture2D texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/LightArrayInventorySlot").Value;
                spriteBatch.Draw(texture, position, null, Color.White * 0.8f, 0, texture.Size() / 2, Main.inventoryScale, SpriteEffects.None, 0);
            }
            return true;
        }
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
                DrawRadianceInvBG(Main.spriteBatch, line.X - 8, line.Y - 8, width + 10, height + 8);
            }
            if (line.Name.StartsWith("LightArrayItems"))
            {
                int number = int.Parse(line.Name.Last().ToString());
                for (int i = number * 16; i < Math.Min((number + 1) * 16, slotsWithItems.Count); i++)
                {
                    Item item = inventory[slotsWithItems[i]];
                    Vector2 pos = new Vector2(line.X + 16 + 36 * (i - number * 16), line.Y + 10);
                    DynamicSpriteFont font = FontAssets.MouseText.Value;

                    ItemSlot.DrawItemIcon(item, 0, Main.spriteBatch, pos, 1f, 32, Color.White);
                    if (item.stack > 1)
                        Utils.DrawBorderStringFourWay(Main.spriteBatch, font, item.stack.ToString(), pos.X - 14, pos.Y + 12, Color.White, Color.Black, Vector2.UnitY * font.MeasureString(item.stack.ToString()).Y / 2, 0.85f);
                }
                return false;
            }
            return true;
        }

        public override sealed void SaveData(TagCompound tag)
        {
            this.SaveInventory(tag);
            tag.Add("OptionKeys", optionsDictionary.Keys.ToList());
            tag.Add("OptionValues", optionsDictionary.Values.ToList());
            SaveExtraData(tag);
        }

        public virtual void SaveExtraData(TagCompound tag)
        { }

        public override sealed void LoadData(TagCompound tag)
        {
            this.LoadInventory(tag);
            List<string> optionKeys = (List<string>)tag.GetList<string>("OptionKeys");
            if (optionKeys.Any())
            {
                List<int> optionValues = (List<int>)tag.GetList<int>("OptionValues");
                optionsDictionary = optionKeys.Zip(optionValues, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
            }
            LoadExtraData(tag);
        }

        public virtual void LoadExtraData(TagCompound tag)
        { }

        public static bool IsValidForLightArray(Item item)
        {
            if (item.ModItem != null && item.ModItem is IInventory)
                return false;

            return true;
        }

        public bool TryInsertItemIntoSlot(Item item, byte slot, bool overrideValidInputs, bool ignoreItemImprint)
        {
            if (!IsValidForLightArray(item))
                return false;
            
            if (!ignoreItemImprint)
            {
                if (currentBase != null && currentBase.HasImprint)
                    return currentBase.itemImprintData.IsItemValid(item);
                else
                    return itemImprintData.IsItemValid(item);

            }
            return true;

        }
    }

    public class LightArrayGlobalItem : GlobalItem
    {
        public override bool ItemSpace(Item item, Player player)
        {
            List<BaseLightArray> validLightArrays = player.inventory.Where(x => x.ModItem is not null && x.ModItem is BaseLightArray array &&
                (array.optionsDictionary["AutoPickup"] != (int)AutoPickupModes.Disabled || array.optionsDictionary["AutoPickupCurrentItems"] != (int)AutoPickupModes.Disabled))
                .Select(x => x.ModItem as BaseLightArray).ToList();

            foreach (BaseLightArray lightArray in validLightArrays)
            {
                switch((AutoPickupModes)lightArray.optionsDictionary["AutoPickup"])
                {
                    case AutoPickupModes.Enabled:
                        if (lightArray.CanInsertItemIntoInventory(item, true))
                            return true;
                        break;

                    case AutoPickupModes.IfInventoryIsFull:
                        if (!HasSpaceInInventory(player, item).CanTakeItemToPersonalInventory && lightArray.CanInsertItemIntoInventory(item, true))
                            return true;

                        if (lightArray.optionsDictionary["AutoPickupCurrentItems"] == (int)AutoPickupModes.Enabled && lightArray.CanInsertItemIntoInventory(item, true, true))
                            return true;
                        break;
                }
                switch ((AutoPickupModes)lightArray.optionsDictionary["AutoPickupCurrentItems"])
                {
                    case AutoPickupModes.Enabled:
                        if (lightArray.CanInsertItemIntoInventory(item, true, true))
                            return true;
                        break;

                    case AutoPickupModes.IfInventoryIsFull:
                        if (!HasSpaceInInventory(player, item).CanTakeItemToPersonalInventory && lightArray.CanInsertItemIntoInventory(item, true, true))
                            return true;
                        break;
                }
            }
            return base.ItemSpace(item, player);
        }

        public override bool OnPickup(Item item, Player player)
        {
            List<BaseLightArray> validLightArrays = player.inventory.Where(x => x.ModItem is not null && x.ModItem is BaseLightArray array &&
                (array.optionsDictionary["AutoPickup"] != (int)AutoPickupModes.Disabled || array.optionsDictionary["AutoPickupCurrentItems"] != (int)AutoPickupModes.Disabled))
                .Select(x => x.ModItem as BaseLightArray).ToList();

            foreach (BaseLightArray lightArray in validLightArrays)
            {
                switch ((AutoPickupModes)lightArray.optionsDictionary["AutoPickup"])
                {
                    case AutoPickupModes.Enabled:
                        if (lightArray.CanInsertItemIntoInventory(item, overrideValidInputs: true))
                        {
                            MakePopupText(item);
                            lightArray.SafeInsertItemIntoInventory(item, out _, true);
                        }
                        break;
                    case AutoPickupModes.IfInventoryIsFull:
                        if (!HasSpaceInInventory(player, item).CanTakeItemToPersonalInventory && lightArray.CanInsertItemIntoInventory(item, true))
                        {
                            MakePopupText(item);
                            lightArray.SafeInsertItemIntoInventory(item, out _, true);
                        }
                        if (lightArray.optionsDictionary["AutoPickupCurrentItems"] == (int)AutoPickupModes.Enabled && lightArray.CanInsertItemIntoInventory(item, true, true))
                        {
                            MakePopupText(item);
                            lightArray.SafeInsertItemIntoInventory(item, out _, true);
                        }
                        break;
                }
                switch ((AutoPickupModes)lightArray.optionsDictionary["AutoPickupCurrentItems"])
                {
                    case AutoPickupModes.Enabled:
                        if (lightArray.CanInsertItemIntoInventory(item, true, true))
                        {
                            MakePopupText(item);
                            lightArray.SafeInsertItemIntoInventory(item, out _, true);
                        }
                        break;
                    case AutoPickupModes.IfInventoryIsFull:
                        if (!HasSpaceInInventory(player, item).CanTakeItemToPersonalInventory && lightArray.CanInsertItemIntoInventory(item, true, true))
                        {
                            MakePopupText(item);
                            lightArray.SafeInsertItemIntoInventory(item, out _, true);
                        }
                        break;
                }
            }
            return true;
        }
        private static void MakePopupText(Item item)
        {
            SoundEngine.PlaySound(SoundID.Grab);
            PopupText text = Main.popupText[PopupText.NewText((PopupTextContext)LightArrayInventoryUI.ItemSlotContext, item, item.stack)];
            text.color = new Color(255, 220, 130);
        }
        //have to remake itemspace without the itemloader run 
        private static ItemSpaceStatus HasSpaceInInventory(Player player, Item newItem)
        {
            if (ItemID.Sets.IsAPickup[newItem.type])
                return new ItemSpaceStatus(CanTakeItem: true);  

            if (newItem.uniqueStack && player.HasItem(newItem.type))
                return new ItemSpaceStatus(CanTakeItem: false);

            int slotsToCheck = 50;
            if (newItem.IsACoin)
                slotsToCheck = 54;

            for (int i = 0; i < slotsToCheck; i++)
            {
                if (player.CanItemSlotAccept(player.inventory[i], newItem))
                    return new ItemSpaceStatus(CanTakeItem: true);
            }
            if (newItem.ammo > 0 && !newItem.notAmmo)
            {
                for (int j = 54; j < 58; j++)
                {
                    if (player.CanGoIntoAmmoOnPickup(player.inventory[j], newItem))
                        return new ItemSpaceStatus(CanTakeItem: true);
                    
                }
            }
            for (int k = 54; k < 58; k++)
            {
                if (!player.inventory[k].IsAir && player.inventory[k].stack < player.inventory[k].maxStack && player.inventory[k].IsSameAs(newItem))
                    return new ItemSpaceStatus(CanTakeItem: true);
            }
            if (player.ItemSpaceForCofveve(newItem))
                return new ItemSpaceStatus(CanTakeItem: true, ItemIsGoingToVoidVault: true);

            return new ItemSpaceStatus(CanTakeItem: false);
        }
    }

    public class LightArrayILEdits : ILoadable
    {
        private Action<Item[], int> CollectItems;

        private delegate bool ConsumeForCraftDelegate(Item item, Item requiredItem, ref int stackRequired);

        private ConsumeForCraftDelegate ConsumeForCraft;

        private delegate void CheckArraysForItemsDelegate(Item requiredItem, ref int stackRequried);

        public void Load(Mod mod)
        {
            IL_Recipe.CollectItemsToCraftWithFrom += IL_Recipe_CollectItemsToCraftWithFrom;
            IL_Recipe.Create += On_Recipe_Create;

            IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
            IL_ItemSlot.OverrideHover_ItemArray_int_int += IL_ItemSlot_OverrideHover_ItemArray_int_int;
            IL_ItemSlot.PickItemMovementAction += IL_ItemSlot_PickItemMovementAction;
            IL_ItemSlot.OverrideLeftClick += IL_ItemSlot_OverrideLeftClick;
            IL_ItemSlot.RightClick_ItemArray_int_int += IL_ItemSlot_RightClick_ItemArray_int_int;

            IL_Main.DrawItemTextPopups += IL_Main_DrawItemTextPopups;

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
            IL_ItemSlot.RightClick_ItemArray_int_int -= IL_ItemSlot_RightClick_ItemArray_int_int;

            IL_Main.DrawItemTextPopups -= IL_Main_DrawItemTextPopups;

            ConsumeForCraft = null;
            CollectItems = null;
        }

        #region Right Click to equip compatibility

        private void IL_ItemSlot_RightClick_ItemArray_int_int(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.Index = cursor.Instrs.Count;

            if (!cursor.TryGotoPrev(MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(2),
                i => i.MatchLdelemRef(),
                i => i.MatchLdfld(typeof(Item), nameof(Item.maxStack)),
                i => i.MatchLdcI4(1),
                i => i.MatchBneUn(out var _),
                i => i.MatchLdsfld(typeof(Main), nameof(Main.mouseRightRelease)),
                i => i.MatchBrfalse(out var _),
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(1),
                i => i.MatchLdarg(2),
                i => i.MatchCall(out var _)))
            {
                LogIlError("Light Array Right Click", "Couldn't navigate to before max stack detection");
                return;
            }
            ILLabel beforeSwapLabel = cursor.MarkLabel();

            if (!cursor.TryGotoPrev(MoveType.Before,
                i => i.MatchCall(typeof(Math), nameof(Math.Abs)),
                i => i.MatchStloc(1),
                i => i.MatchLdloc(1),
                i => i.MatchLdcI4(12),
                i => i.MatchBgt(out var _)))
            {
                LogIlError("Light Array Right Click", "Couldn't navigate to before context switch statment");
                return;
            }
            cursor.Emit(OpCodes.Ldc_I4, LightArrayInventoryUI.ItemSlotContext);
            cursor.Emit(OpCodes.Beq, beforeSwapLabel);
            cursor.Emit(OpCodes.Ldarg_1);
        }

        #endregion

        #region Pickup Text Color
        private void IL_Main_DrawItemTextPopups(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(1),
                i => i.MatchLdfld(typeof(PopupText), nameof(PopupText.context)),
                i => i.MatchStloc(16)))
            {
                LogIlError("Light Array Pickup Text", "Couldn't navigate to after result initialization");
                return;
            }

            cursor.Emit(OpCodes.Ldloc, 16); //load context
            cursor.Emit(OpCodes.Ldloca, 12); //load color
            cursor.EmitDelegate(ChangeColor);
        }
        private void ChangeColor(PopupTextContext context, ref Color color)
        {
            if(context == (PopupTextContext)LightArrayInventoryUI.ItemSlotContext)
                color = CommonColors.RadianceColor2 * 0.4f;
        }
        #endregion

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
                LogIlError("Light Array Shift Clicking", "Couldn't navigate to before final return true");
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
                Main.LocalPlayer.CurrentActiveArray().SafeInsertItemIntoInventory(item, out _, overrideValidInputs: true);
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
                LogIlError("Light Array Item Movement", "Couldn't navigate to after result initialization");
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
                LogIlError("Light Array Vanilla Inventory ItemSlot Modifications", "Could not jump to first chest detection");
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
                LogIlError("Light Array Vanilla Inventory ItemSlot Modifications", "Could not jump to second chest detection");
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
            (IsValidForLightArray(item) && Main.LocalPlayer.HasActiveArray() && Main.LocalPlayer.CurrentActiveArray().CanInsertItemIntoInventory(item, true));

        private bool SecondIsInChestOrArray(Item item, int itemSlotContext)
        {
            if (IsValidForLightArray(item) && Main.LocalPlayer.HasActiveArray() && Main.LocalPlayer.CurrentActiveArray().CanInsertItemIntoInventory(item, true))
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
                LogIlError("Light Array Item Slot Color", "Couldn't navigate to before draw function");
                return;
            }

            cursor.Emit(OpCodes.Ldloca, 8); //push color onto stack
            cursor.Emit(OpCodes.Ldloca, 7); //push texture onto stack
            cursor.Emit(OpCodes.Ldarg_2); //push context onto stack
            cursor.EmitDelegate(SetTexture);
        }

        private void SetTexture(ref Color color, ref Texture2D texture, int context)
        {
            if (context == LightArrayInventoryUI.ItemSlotContext)
            {
                texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/LightArrayInventorySlot").Value;
                color *= LightArrayInventoryUI.SlotColorMult;
            }
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
                LogIlError("Light Array Recipe Compatability", "Couldn't navigate to after inventory loop");
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
                LogIlError("Light Array Recipe Item Collection", "Couldn't navigate to after inventory check.");
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