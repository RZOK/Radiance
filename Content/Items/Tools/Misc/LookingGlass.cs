using Radiance.Content.Items.Materials;
using System.Collections.ObjectModel;
using Terraria.ModLoader.Config;

namespace Radiance.Content.Items.Tools.Misc
{
    public class LookingGlass : ModItem
    {
        private Vector2 visualPosition;
        private int visualTimer = 0;
        private const int MAX_SLOTS = 6;
        private static int VISUAL_TIMER_MAX = 45;
        private ItemDefinition[] 
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Looking Glass");
            Tooltip.SetDefault("Allows you to view applied Item Imprints and remove them");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 90;
            Item.UseSound = SoundID.Item6;
            Item.useAnimation = 90;
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            visualPosition = position;
            if (Main.LocalPlayer.GetModPlayer<RadianceInterfacePlayer>().realHoveredItem == Item && GetPlayerHeldItem() != Item && Main.mouseItem.IsAir)
            {
                if (visualTimer < VISUAL_TIMER_MAX && !Main.gamePaused && Main.hasFocus)
                    visualTimer++;
            }
            else
                visualTimer = 0;

            return true;
        }
        public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                DrawNotchSlot(Main.spriteBatch, visualPosition, i);
            }
        }
        public void DrawNotchSlot(SpriteBatch spriteBatch, Vector2 position, int index)
        {
            Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/ExtraTextures/LookingGlass_Slot").Value;
            float completion = visualTimer / (float)VISUAL_TIMER_MAX;
            float adjustedCompletion = EaseOutExponent(completion, 6f);
            float alphaCompletion = EaseOutExponent(completion, 3f);
            float startingAngle = Pi - PiOver2;
            float endingAngle = TwoPi;
            float currentAngle = Lerp(startingAngle, endingAngle, index / (MAX_SLOTS - 1f));

            Vector2 floating = new Vector2(2 * SineTiming(45 + index, MathF.Pow(index + 1f, index + 1f)), 2 * SineTiming(45 + index, 30f + MathF.Pow(index + 1f, index + 1f)));
            floating.X -= floating.X % 0.5f;
            floating.Y -= floating.Y % 0.5f;
            int distance = (int)(48f * adjustedCompletion);
            int underDistance = (int)(44f * adjustedCompletion);
            Vector2 drawPos = position + Vector2.UnitX.RotatedBy(currentAngle) * distance + floating;
            Vector2 underDrawPos = position + Vector2.UnitX.RotatedBy(currentAngle) * underDistance + floating;
            float scale = Math.Clamp(adjustedCompletion + 0.3f, 0.3f, 1);

            spriteBatch.Draw(tex, underDrawPos, null, Color.White * alphaCompletion * 0.25f, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, Color.White * alphaCompletion, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }
}