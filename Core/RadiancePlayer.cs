using IL.Terraria.GameContent.ObjectInteractions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Radiance.Content.Items.Armor;
using Radiance.Content.Items.BaseItems;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public static class RadiancePlayerExtensionMethods
    {
        public static float GetRadianceDiscount(this Player player) => 1f - Math.Min(0.9f, player.GetModPlayer<RadiancePlayer>().radianceDiscount);   
        public static bool ConsumeRadianceOnHand(this Player player, float amount) => player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(amount);
        public static bool HasRadiance(this Player player, float consumeAmount) => player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand >= consumeAmount * player.GetRadianceDiscount();
    }
    public class RadiancePlayer : ModPlayer
    {
        public bool debugMode = false;
        public bool canSeeRays = false;
        public bool alchemicalLens = false;
        public float dashTimer = 0;
        /// <summary>
        /// Do NOT try to consume Radiance by changing currentRadianceOnHand directly. Use ConsumeRadianceOnHand(float consumedAmount) from RadiancePlayer.cs instead.
        /// </summary>
        public float currentRadianceOnHand { get; private set; }
        public float maxRadianceOnHand { get; private set; }
        /// <summary>
        /// Do NOT try to get Radiance discount by reading directly from radianceDiscount. Use player.GetRadianceDiscount() intead.
        /// </summary>
        public float radianceDiscount { internal get; set; }

        public override void ResetEffects()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            radianceDiscount = 0;
        }

        public override void UpdateDead()
        {
            debugMode = false;
            canSeeRays = false;
            alchemicalLens = false;
            radianceDiscount = 0;
        }
        public override void PreUpdate()
        {
            maxRadianceOnHand = 0;
            currentRadianceOnHand = 0;
            for (int i = 0; i < 58; i++)
            {
                if (Player.inventory[i].ModItem is BaseContainer cell && cell.mode != BaseContainer.ContainerMode.InputOnly)
                {
                    maxRadianceOnHand += cell.maxRadiance;
                    currentRadianceOnHand += cell.currentRadiance;
                }
            }
        }

        public bool ConsumeRadianceOnHand(float consumedAmount)
        {
            float radianceLeft = consumedAmount * Player.GetRadianceDiscount();
            if (currentRadianceOnHand >= radianceLeft)
            {
                for (int i = 0; i < 58; i++)
                {
                    if (Player.inventory[i].ModItem is BaseContainer cell && cell.currentRadiance > 0)
                    {
                        float minus = Math.Clamp(cell.currentRadiance, 0, radianceLeft);
                        cell.currentRadiance -= minus;
                        radianceLeft -= minus;
                    }
                    if (radianceLeft == 0)
                        return true;
                }
            }
            return false;
        }
        public override void FrameEffects()
        {
            if (dashTimer > 10)
            {
                Player.armorEffectDrawShadow = true;
            }
        }
        #region Events

        public delegate void PostUpdateEquipsDelegate(Player player);
        public static event PostUpdateEquipsDelegate PostUpdateEquipsEvent;
        public override void PostUpdateEquips()
        {
            if (dashTimer > 0)
                dashTimer--;
            PostUpdateEquipsEvent?.Invoke(Player);
        }
        public delegate bool CanUseItemDelegate(Player player, Item item);
        public static event CanUseItemDelegate CanUseItemEvent;
        public override bool CanUseItem(Item item)
        {
            if (CanUseItemEvent != null)
            {
                bool result = true;
                foreach (CanUseItemDelegate del in CanUseItemEvent.GetInvocationList())
                {
                    result &= del(Player, item);
                }
                return result;
            }
            return base.CanUseItem(item);
        }
        public delegate void PostHurtDelegate(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter);
        public static event PostHurtDelegate PostHurtEvent;
        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter)
        {
            PostHurtEvent?.Invoke(Player, pvp, quiet, damage, hitDirection, crit, cooldownCounter);
        }
        #endregion
        public override void Unload()
        {
            PostUpdateEquipsEvent = null;
            CanUseItemEvent = null;
        }
    }
}