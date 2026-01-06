using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;
using System.Collections.ObjectModel;

namespace Radiance.Items.Accessories
{
    public class DebugAccessory : ModItem, IDrawOverInventory
    {
        private Vector2 visualPosition;
        private int visualTimer = 0;
        private const int MAX_SLOTS = 6;
        private const int VISUAL_TIMER_MAX = 30;
        public override string Texture => "Radiance/Content/ExtraTextures/Debug";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Accessory");
            Tooltip.SetDefault("Enables various debug features and information when equipped");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = 0;
            Item.rare = ItemRarityID.Blue;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.accessory = true;
        }

        public void DrawOverInventory(SpriteBatch spriteBatch)
        {
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
            float alphaCompletion = EaseOutExponent(completion, 2f);
            float startingAngle = Pi - PiOver2;
            float endingAngle = TwoPi;
            float currentAngle = Lerp(startingAngle, endingAngle, index / (MAX_SLOTS - 1f));
            float distance = 48f * adjustedCompletion;
            float underDistance = 44f * adjustedCompletion;
            distance -= distance % 1f;
            underDistance -= distance % 1f;

            Vector2 floating = new Vector2(2 * SineTiming(45 + index, MathF.Pow(index + 1f, index + 1f)), 2 * SineTiming(45 + index, 30f + MathF.Pow(index + 1f, index + 1f)));
            floating.X -= floating.X % 0.5f;
            floating.Y -= floating.Y % 0.5f;
            Vector2 drawPos = position + Vector2.UnitX.RotatedBy(currentAngle) * distance + floating;
            Vector2 underDrawPos = position + Vector2.UnitX.RotatedBy(currentAngle) * underDistance + floating;
            float scale = Math.Clamp(adjustedCompletion + 0.3f, 0.3f, 1);

            spriteBatch.Draw(tex, underDrawPos, null, Color.White * alphaCompletion * 0.25f, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, Color.White * alphaCompletion, 0, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }

        public override bool? UseItem(Player player)
        {
            WorldParticleSystem.system.AddParticle(new TestParticle(Main.MouseWorld, Vector2.Zero, 600));
            return true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RadiancePlayer>().debugMode = true;
            player.GetModPlayer<RadianceInterfacePlayer>().canSeeRays = true;
            if (TileEntitySystem.orderedEntities is not null)
            {
                foreach (ImprovedTileEntity ite in TileEntitySystem.orderedEntities)
                {
                    if (ite.TileEntityWorldCenter().Distance(player.Center) < 100)
                        ite.AddHoverUI();
                }
            }
        }
    }
}