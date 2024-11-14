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

namespace Radiance.Content.UI.Blueprints
{
    internal class BlueprintUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

        private const int COMPACT_MAX_SLOTS_PER_ROW = 8;
        private const int COMPACT_DISTANCE_BETWEEN_SLOTS = 52;
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
                float ease = EaseOutExponent(Math.Min(1, (float)(timer * 2) / timerMax), 7f + 5f * GetSmoothIntRNG(Main.LocalPlayer.GetModPlayer<BlueprintUIPlayer>().blueprintSlotSeed, i));
                float scale = 0.9f * ease;
                Vector2 offset = tex.Size() / 2 * scale;

                float x =
                    -COMPACT_DISTANCE_BETWEEN_SLOTS * (i % COMPACT_MAX_SLOTS_PER_ROW) + 
                    ((float)Math.Min(COMPACT_MAX_SLOTS_PER_ROW, SlotCount) / 2 * COMPACT_DISTANCE_BETWEEN_SLOTS //proper positioning with less than 8 slots 
                     - COMPACT_DISTANCE_BETWEEN_SLOTS / 2);
                float y =
                    COMPACT_DISTANCE_BETWEEN_SLOTS *
                    ((SlotCount - 1) / COMPACT_MAX_SLOTS_PER_ROW - i / COMPACT_MAX_SLOTS_PER_ROW) //keep the first slots at the top
                    + COMPACT_DISTANCE_BETWEEN_SLOTS; //stay above the center slot 
                Vector2 slotPosition = Vector2.Lerp(screenCenter - offset, screenCenter - offset - new Vector2(x, y), ease);
                DrawSlot(spriteBatch, tex, GetItemTexture(BlueprintLoader.loadedBlueprints[i].tileItemType), slotPosition, offset, scale);

                Rectangle slotRectangle = new Rectangle((int)slotPosition.X, (int)slotPosition.Y, (int)(tex.Width * scale), (int)(tex.Height * scale));
                if (slotRectangle.Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                    Item tempItem = GetItem(BlueprintLoader.loadedBlueprints[i].tileItemType).Clone();
                    Main.HoverItem = tempItem;
                    Main.hoverItemName = tempItem.Name;
                    tempItem.GetGlobalItem<RadianceGlobalItem>().blueprintDummy = true;
                }
            }
        }
        private void DrawSlot(SpriteBatch spriteBatch, Texture2D slotTex, Texture2D itemTex, Vector2 position, Vector2 offset, float scale)
        {
            float scale2 = 1f;
            if ((float)itemTex.Width > 32f || (float)itemTex.Height > 32f)
                scale2 = ((itemTex.Width <= itemTex.Height) ? (32f / (float)itemTex.Height) : (32f / (float)itemTex.Width));
            
            spriteBatch.Draw(slotTex, position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(itemTex, position + offset, null, Color.White, 0, itemTex.Size() / 2f, scale * scale2, SpriteEffects.None, 0);
        }
    }
}
