using Terraria.ID;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Radiance.Content.Items.Tools.Misc;
using Radiance.Utilities;
using Terraria.GameContent;

namespace Radiance.Core
{
    public class PotionColors : GlobalItem
    {
        public override bool InstancePerEntity => true;

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
            BuffID.Spelunker
        };
        public static List<int> MauvePotions = new List<int>() //Unnatural abilities/Luck-related potions.
        {
            BuffID.AmmoReservation,
            BuffID.Crate,
            BuffID.Featherfall,
            BuffID.Fishing,
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

        #endregion
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
                        drawPos += new Vector2(TextureAssets.Item[item.type].Width() * scale / 2, TextureAssets.Item[item.type].Height() * scale / 2);
                        float slotScale = 0.7f;
                        slotScale *= Main.inventoryScale + 0.05f * RadianceUtils.SineTiming(60);
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, default, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
                        RadianceDrawing.DrawSoftGlow(Main.screenPosition + drawPos, new Color(color.R, color.G, color.B, (byte)(100 + 20 * RadianceUtils.SineTiming(20))), 0.5f, RadianceDrawing.DrawingMode.UI);
                        spriteBatch.Draw(texture, drawPos, null, new Color(color.R, color.G, color.B, (byte)(150 + 50 * RadianceUtils.SineTiming(20))), 0, texture.Size() / 2, slotScale, SpriteEffects.None, 0);
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, default, default, default, null, Main.UIScaleMatrix);
                    }
                }
            }
            return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
    }
}
