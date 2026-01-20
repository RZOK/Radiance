using Radiance.Content.Items.Tools.Misc;

namespace Radiance.Core
{
    public class PotionColors : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public enum InfluentialColor
        {
            Scarlet,
            Cerulean,
            Verdant,
            Mauve
        }
        public static Dictionary<InfluentialColor, List<int>> influentalColorMap;

        #region Potion Lists

        public static List<int> ScarletPotions = new List<int>() //Combat potions. Ones directly related to life/HP are not included here.
        {
            BuffID.Inferno,
            BuffID.Ironskin,
            BuffID.MagicPower,
            BuffID.Rage,
            BuffID.Thorns,
            BuffID.Wrath,
            BuffID.Battle,
            BuffID.Archery,
            BuffID.Endurance,
            BuffID.Titan
        };

        public static List<int> CeruleanPotions = new List<int>() //Vitality/Creativity/Peace-related potions.
        {
            BuffID.Calm,
            BuffID.Regeneration,
            BuffID.ManaRegeneration,
            BuffID.Warmth,
            BuffID.Builder,
            BuffID.Heartreach,
            BuffID.Lifeforce,
            BuffID.Summoning
        };

        public static List<int> VerdantPotions = new List<int>() //Wisdom/Insight/Knowledge-related potions.
        {
            BuffID.Dangersense,
            BuffID.Hunter,
            BuffID.NightOwl,
            BuffID.Sonar,
            BuffID.Spelunker,
            BuffID.Crate,
            BuffID.Fishing
        };

        public static List<int> MauvePotions = new List<int>() //Unnatural abilities/Luck-related potions.
        {
            BuffID.AmmoReservation,
            BuffID.Featherfall,
            BuffID.Flipper,
            BuffID.Gills,
            BuffID.Gravitation,
            BuffID.Lucky,
            BuffID.Invisibility,
            BuffID.ObsidianSkin,
            BuffID.Shine,
            BuffID.WaterWalking,
            BuffID.Swiftness
        };

        #endregion Potion Lists
        public override void Load()
        {
            influentalColorMap = new Dictionary<InfluentialColor, List<int>>();
            influentalColorMap[InfluentialColor.Scarlet] = ScarletPotions;
            influentalColorMap[InfluentialColor.Cerulean] = CeruleanPotions;
            influentalColorMap[InfluentialColor.Verdant] = VerdantPotions;
            influentalColorMap[InfluentialColor.Mauve] = MauvePotions;
        }
        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.LocalPlayer;
            if (item.buffType != 0)
            {
                if (Main.playerInventory && player.HasItem(ModContent.ItemType<AlchemicalLens>()) && Main.mouseItem != item)
                {
                    Vector2 drawPos = position;
                    Texture2D texture = null;
                    Color color = Color.White;
                    if (ScarletPotions.Contains(item.buffType))
                    {
                        color = CommonColors.ScarletColor;
                        texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/ScarletIcon").Value;
                    }
                    else if (CeruleanPotions.Contains(item.buffType))
                    {
                        color = CommonColors.CeruleanColor;
                        texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/CeruleanIcon").Value;
                    }
                    else if (VerdantPotions.Contains(item.buffType))
                    {
                        color = CommonColors.VerdantColor;
                        texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/VerdantIcon").Value;
                    }
                    else if (MauvePotions.Contains(item.buffType))
                    {
                        color = CommonColors.MauveColor;
                        texture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/MauveIcon").Value;
                    }
                    if (texture != null)
                    {
                        float slotScale = 0.7f;
                        slotScale *= Main.inventoryScale + 0.05f * SineTiming(180);
                        RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos, color * (0.4f + 0.2f * SineTiming(20)), 0.5f);
                        spriteBatch.Draw(texture, drawPos, null, color * (0.6f + 0.2f * SineTiming(120)), 0, texture.Size() / 2, slotScale, SpriteEffects.None, 0);
                    }
                }
            }
            return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
    }
}