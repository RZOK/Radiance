﻿using Radiance.Content.Items;
using Radiance.Content.Items.BaseItems;
using Radiance.Core.Loaders;
using Radiance.Items.Accessories;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;
using Terraria.GameContent.Creative;
using Terraria.GameInput;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;
using static Radiance.Content.Items.BaseItems.BaseLightArray;

namespace Radiance.Content.UI.BlueprintUI
{
    internal class BlueprintCaseUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

        private const int COMPACT_MAX_SLOTS_PER_ROW = 8;
        private const int COMPACT_DISTANCE_BETWEEN_SLOTS = 52;
        public static int SlotCount => BlueprintLoader.loadedBlueprints.Count;

        public static BlueprintCase CurrentActiveCase => (BlueprintCase)(Main.LocalPlayer.GetCurrentActivePlayerUIItem() is BlueprintCase ? Main.LocalPlayer.GetCurrentActivePlayerUIItem() : null);
        public ref int timer => ref Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintUITimer;
        public int timerMax => BlueprintUIPlayer.BLUEPRINT_UI_TIMER_MAX;
        public override bool Visible => CurrentActiveCase is not null && Main.playerInventory && Main.LocalPlayer.active && !Main.LocalPlayer.dead;

        public UIElement centerIcon = new();

        public override void Update(GameTime gameTime)
        {
            bool flag = false;
            if (CurrentActiveCase is not null)
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Main.LocalPlayer.inventory[i].ModItem == CurrentActiveCase)
                        flag = true;
                }
                if (!flag)
                    Main.LocalPlayer.ResetActivePlayerUI();
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentActiveCase is not null)
            {
                DrawCenterSlot(spriteBatch);
                DrawMainSlots(spriteBatch);
            }
        }
        private void DrawCenterSlot(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>((CurrentActiveCase as IPlayerUIItem).SlotTexture).Value;
            float ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 9);
            float scale = 0.9f * ease;
            Vector2 offset = tex.Size() / 2 * scale;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            Vector2 slotPosition = screenCenter - offset;
            Rectangle centerSlotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * scale), (int)(tex.Height * scale));

            DrawSlot(spriteBatch, tex, GetItemTexture(CurrentActiveCase.Type), slotPosition, offset, scale, Color.White, null);
            if (centerSlotRectangle.Contains(Main.MouseScreen.ToPoint()))
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentFakeHoverText = "[c/FF67AA:Left Click to close]";

                if (Main.mouseLeftRelease && Main.mouseLeft)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    Main.LocalPlayer.ResetActivePlayerUI();
                    timer = 0;
                }
            }
        }
        private void DrawMainSlots(SpriteBatch spriteBatch)
        {
            if (CurrentActiveCase is null)
                return;

            Texture2D slotTex = ModContent.Request<Texture2D>((CurrentActiveCase as IPlayerUIItem).SlotTexture).Value;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            for (int i = 0; i < SlotCount; i++)
            {
                BlueprintData currentData = BlueprintLoader.loadedBlueprints[i];
                bool unlocked = true;
                if (!Main.LocalPlayer.GetModPlayer<BlueprintPlayer>().knownBlueprints.Contains(currentData))
                    unlocked = false;

                float ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 7f + 5f * GetSmoothIntRNG(Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintSlotSeed, i));
                float scale = 0.9f * ease;
                Vector2 offset = slotTex.Size() / 2 * scale;

                float x =
                    -COMPACT_DISTANCE_BETWEEN_SLOTS * (i % COMPACT_MAX_SLOTS_PER_ROW) + 
                    ((float)Math.Min(COMPACT_MAX_SLOTS_PER_ROW, SlotCount) / 2 * COMPACT_DISTANCE_BETWEEN_SLOTS //proper positioning with less than 8 slots 
                     - COMPACT_DISTANCE_BETWEEN_SLOTS / 2);
                float y =
                    COMPACT_DISTANCE_BETWEEN_SLOTS *
                    ((SlotCount - 1) / COMPACT_MAX_SLOTS_PER_ROW - i / COMPACT_MAX_SLOTS_PER_ROW) //keep the first slots at the top
                    + COMPACT_DISTANCE_BETWEEN_SLOTS; //stay above the center slot 

                Vector2 slotPosition = Vector2.Lerp(screenCenter - offset, screenCenter - offset - new Vector2(x, y), ease);

                Texture2D iconTex = GetItemTexture(currentData.tileItemType);
                Color color = Color.White;
                Color? accentColor = null;
                if(CurrentActiveCase.selectedData == currentData)
                    accentColor = (GetItem(currentData.blueprintType).ModItem as AutoloadedBlueprint).color;

                if (!unlocked)
                {
                    color = Color.Black;
                    accentColor = null;
                }
                DrawSlot(spriteBatch, slotTex, iconTex, slotPosition, offset, scale, color, accentColor);

                Rectangle slotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(slotTex.Width * scale), (int)(slotTex.Height * scale));
                if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    if (unlocked)
                    {
                        Item tempItem = new Item(currentData.tileItemType);
                        Main.HoverItem = tempItem;
                        Main.hoverItemName = tempItem.Name;
                        tempItem.GetGlobalItem<RadianceGlobalItem>().blueprintCaseDummy = true;

                        if (Main.mouseLeftRelease && Main.mouseLeft)
                        {
                            if (CurrentActiveCase.selectedData != currentData)
                                CurrentActiveCase.selectedData = currentData;
                            else
                                CurrentActiveCase.selectedData = null;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                    }
                    else
                        UICommon.TooltipMouseText("[c/555555:There is more yet to be revealed...]");
                }
            }
        }
        private static void DrawSlot(SpriteBatch spriteBatch, Texture2D slotTex, Texture2D itemTex, Vector2 position, Vector2 offset, float scale, Color color, Color? accentColor)
        {
            Texture2D accentTex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/UI/BlueprintUI/BlueprintCaseSlot_Accent").Value;
            float scale2 = 1f;
            if ((float)itemTex.Width > 32f || (float)itemTex.Height > 32f)
                scale2 = ((itemTex.Width <= itemTex.Height) ? (32f / (float)itemTex.Height) : (32f / (float)itemTex.Width));
            
            spriteBatch.Draw(slotTex, position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            if(accentColor is not null)
                spriteBatch.Draw(accentTex, position, null, accentColor.Value, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

            spriteBatch.Draw(itemTex, position + offset, null, color, 0, itemTex.Size() / 2f, scale * scale2, SpriteEffects.None, 0);
        }
    }
}