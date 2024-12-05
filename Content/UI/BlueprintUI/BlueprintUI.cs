using Radiance.Content.Items;
using Radiance.Core.Loaders;
using Steamworks;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Radiance.Content.UI.BlueprintUI
{
    internal class BlueprintUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        
        private const int DISTANCE_BETWEEN_SLOTS = 52;
        public static int SlotCount => BlueprintLoader.loadedBlueprints.Count;

        public static SilkBlueprint CurrentActiveBlueprint => Main.LocalPlayer.GetCurrentActivePlayerUIItem() as SilkBlueprint;
        public ref int timer => ref Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintUITimer;
        public static int timerMax => BlueprintUIPlayer.BLUEPRINT_UI_TIMER_MAX;
        public override bool Visible => CurrentActiveBlueprint is not null && Main.playerInventory && Main.LocalPlayer.active && !Main.LocalPlayer.dead;

        public UIElement centerIcon = new();

        public override void Update(GameTime gameTime)
        {
            bool flag = false;
            if (CurrentActiveBlueprint is not null)
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Main.LocalPlayer.inventory[i].ModItem == CurrentActiveBlueprint)
                        flag = true;
                }
                if (!flag)
                    Main.LocalPlayer.ResetActivePlayerUI();
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentActiveBlueprint is not null)
            {
                DrawMainSlots(spriteBatch);
                DrawCenterSlot(spriteBatch);
            }
        }
        private void DrawCenterSlot(SpriteBatch spriteBatch)
        {
            if (CurrentActiveBlueprint is null)
                return;

            Texture2D tex = ModContent.Request<Texture2D>((CurrentActiveBlueprint as IPlayerUIItem).SlotTexture).Value;
            float ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 9);
            float scale = 0.9f * ease;
            Vector2 offset = tex.Size() / 2 * scale;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            Vector2 slotPosition = screenCenter - offset;
            Rectangle centerSlotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * scale), (int)(tex.Height * scale));

            DrawSlot(spriteBatch, tex, GetItemTexture(CurrentActiveBlueprint.Type), slotPosition, Color.White, offset, scale);
            if (centerSlotRectangle.Contains(Main.MouseScreen.ToPoint()))
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().currentFakeHoverText = "[c/FF67AA:Left Click to close]";

                if (Main.mouseLeftRelease && Main.mouseLeft)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    Main.LocalPlayer.ResetActivePlayerUI();
                }
            }
        }
        private void DrawMainSlots(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>((CurrentActiveBlueprint as IPlayerUIItem).SlotTexture).Value;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            for (int i = 0; i < SlotCount; i++)
            {
                BlueprintData currentBlueprint = BlueprintLoader.loadedBlueprints[i];
                bool unlocked = currentBlueprint.unlockedCondition.condition();

                float ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 5f);
                float scale = 0.9f * ease;
                Vector2 offset = tex.Size() / 2 * scale;
                float rotation = TwoPi * (i / (float)SlotCount) + TwoPi * 0.75f * ease;
                Vector2 realPosition = screenCenter - offset;
                float distance = DISTANCE_BETWEEN_SLOTS;
                if (SlotCount > 6)
                    distance += (SlotCount - 6) * 10f;
                
                Color color = Color.White;
                if (!unlocked)
                    color = Color.Black;

                Vector2 slotPosition = realPosition + Vector2.UnitX.RotatedBy(rotation) * distance * ease;
                DrawSlot(spriteBatch, tex, GetItemTexture(currentBlueprint.tileItemType), slotPosition, color, offset, scale);

                Rectangle slotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * scale), (int)(tex.Height * scale));
                if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    if (!unlocked)
                        UICommon.TooltipMouseText($"[c/{CommonColors.LockedColor.Hex3()}:There is more yet to be revealed...]");
                    else
                    {
                        Item tempItem = new Item(currentBlueprint.tileItemType);
                        Main.HoverItem = tempItem;
                        Main.hoverItemName = tempItem.Name;
                        tempItem.GetGlobalItem<RadianceGlobalItem>().blueprintDummy = true;

                        if (Main.mouseLeftRelease && Main.mouseLeft)
                        {
                            Item newBlueprintItem = new Item(ModContent.ItemType<IncompleteBlueprint>());
                            IncompleteBlueprint newBlueprint = newBlueprintItem.ModItem as IncompleteBlueprint;
                            newBlueprint.blueprint = GetItem(currentBlueprint.blueprintType).ModItem as AutoloadedBlueprint;

                            int reqTier = newBlueprint.blueprint.blueprintData.tier;
                            int condTier = newBlueprint.blueprint.blueprintData.tier;

                            while (reqTier > 1)
                            {
                                if (Main.rand.NextBool(3))
                                    reqTier--;
                                else
                                    break;
                            }
                            while (condTier > 1)
                            {
                                if (Main.rand.NextBool(3))
                                    condTier--;
                                else
                                    break;
                            }

                            newBlueprint.requirement = BlueprintRequirement.weightedRequirementsByTier[reqTier].Get();
                            newBlueprint.condition = BlueprintCondition.weightedConditionsByTier[reqTier].Get();
                            Main.LocalPlayer.QuickSpawnItem(new EntitySource_ItemUse(Main.LocalPlayer, CurrentActiveBlueprint.Item), newBlueprintItem);

                            CurrentActiveBlueprint.Item.stack--;
                            if (CurrentActiveBlueprint.Item.stack <= 0)
                                CurrentActiveBlueprint.Item.TurnToAir();

                        }
                    }
                }
            }
        }
        private static void DrawSlot(SpriteBatch spriteBatch, Texture2D slotTex, Texture2D itemTex, Vector2 position, Color color, Vector2 offset, float scale)
        {
            float scale2 = 1f;
            if ((float)itemTex.Width > 32f || (float)itemTex.Height > 32f)
                scale2 = ((itemTex.Width <= itemTex.Height) ? (32f / (float)itemTex.Height) : (32f / (float)itemTex.Width));
            
            spriteBatch.Draw(slotTex, position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(itemTex, position + offset, null, color, 0, itemTex.Size() / 2f, scale * scale2, SpriteEffects.None, 0);
        }
    }
}
