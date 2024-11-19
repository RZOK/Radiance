using Radiance.Content.Items;
using Radiance.Content.Items.BaseItems;
using Radiance.Core.Loaders;
using Radiance.Items.Accessories;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;
using Terraria.GameContent.Creative;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Gamepad;
using static Radiance.Content.Items.BaseItems.BaseLightArray;

namespace Radiance.Content.UI.BlueprintUI
{
    internal class BlueprintUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

        private const int COMPACT_MAX_SLOTS_PER_ROW = 8;
        private const int DISTANCE_BETWEEN_SLOTS = 52;
        public static int SlotCount => BlueprintLoader.loadedBlueprints.Count;

        public static SilkBlueprint CurrentActiveBlueprint => (SilkBlueprint)(Main.LocalPlayer.GetCurrentActivePlayerUIItem() is SilkBlueprint ? Main.LocalPlayer.GetCurrentActivePlayerUIItem() : null);
        public ref int timer => ref Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintUITimer;
        public int timerMax => BlueprintUIPlayer.BLUEPRINT_UI_TIMER_MAX;
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
                DrawCenterSlot(spriteBatch);
                DrawMainSlots(spriteBatch);
            }
        }
        private void DrawCenterSlot(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>((CurrentActiveBlueprint as IPlayerUIItem).SlotTexture).Value;
            float ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 9);
            float scale = 0.9f * ease;
            Vector2 offset = tex.Size() / 2 * scale;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            Vector2 slotPosition = screenCenter - offset;
            Rectangle centerSlotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * scale), (int)(tex.Height * scale));

            DrawSlot(spriteBatch, tex, GetItemTexture(CurrentActiveBlueprint.Type), slotPosition, offset, scale);
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
            if (CurrentActiveBlueprint is null)
                return;

            Texture2D tex = ModContent.Request<Texture2D>((CurrentActiveBlueprint as IPlayerUIItem).SlotTexture).Value;
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2;
            for (int i = 0; i < SlotCount; i++)
            {
                float ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 5f);
                float scale = 0.9f * ease;
                Vector2 offset = tex.Size() / 2 * scale;
                float rotation = TwoPi * (i / (float)SlotCount) + TwoPi * 0.75f * ease;
                Vector2 realPosition = screenCenter - offset;
                float distance = DISTANCE_BETWEEN_SLOTS;
                if (SlotCount > 6)
                    distance += (SlotCount - 6) * 10f;

                Vector2 slotPosition = realPosition + Vector2.UnitX.RotatedBy(rotation) * distance * ease; 
                DrawSlot(spriteBatch, tex, GetItemTexture(BlueprintLoader.loadedBlueprints[i].tileItemType), slotPosition, offset, scale);

                Rectangle slotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * scale), (int)(tex.Height * scale));
                if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    Item tempItem = new Item(BlueprintLoader.loadedBlueprints[i].tileItemType);
                    Main.HoverItem = tempItem;
                    Main.hoverItemName = tempItem.Name;
                    tempItem.GetGlobalItem<RadianceGlobalItem>().blueprintDummy = true;

                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        Item newBlueprintItem = new Item(ModContent.ItemType<IncompleteBlueprint>());
                        IncompleteBlueprint newBlueprint = newBlueprintItem.ModItem as IncompleteBlueprint;
                        newBlueprint.blueprint = GetItem(BlueprintLoader.loadedBlueprints[i].blueprintType).ModItem as AutoloadedBlueprint;
                        newBlueprint.requirement = Main.rand.Next(BlueprintRequirement.loadedRequirements.Where(x => x.tier <= newBlueprint.blueprint.blueprintData.tier).ToList());
                        newBlueprint.condition = Main.rand.Next(BlueprintRequirement.loadedConditions.Where(x => x.tier <= newBlueprint.blueprint.blueprintData.tier).ToList());

                        Main.LocalPlayer.QuickSpawnItem(new EntitySource_ItemUse(Main.LocalPlayer, CurrentActiveBlueprint.Item), newBlueprintItem);

                        CurrentActiveBlueprint.Item.stack--;
                        if (CurrentActiveBlueprint.Item.stack <= 0)
                            CurrentActiveBlueprint.Item.TurnToAir();

                    }
                }
            }
        }
        private static void DrawSlot(SpriteBatch spriteBatch, Texture2D slotTex, Texture2D itemTex, Vector2 position, Vector2 offset, float scale)
        {
            float scale2 = 1f;
            if ((float)itemTex.Width > 32f || (float)itemTex.Height > 32f)
                scale2 = ((itemTex.Width <= itemTex.Height) ? (32f / (float)itemTex.Height) : (32f / (float)itemTex.Width));
            
            spriteBatch.Draw(slotTex, position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(itemTex, position + offset, null, Color.White, 0, itemTex.Size() / 2f, scale * scale2, SpriteEffects.None, 0);
        }
    }
}
