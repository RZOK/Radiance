﻿using IL.Terraria.IO;
using Microsoft.Xna.Framework;
using Radiance.Content.Particles;
using Radiance.Core;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class LightfootSaboton : ModItem
    {
        public override void Load()
        {
            RadiancePlayer.PostHurtEvent += AddCooldown;
            RadiancePlayer.PostUpdateEquipsEvent += LightfootDash;
        }
        public override void Unload()
        {
            RadiancePlayer.PostHurtEvent -= AddCooldown;
            RadiancePlayer.PostUpdateEquipsEvent -= LightfootDash;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lightfoot Sabaton");
            Tooltip.SetDefault("Reduces damage taken by 5%\nDouble tap a direction to perform a defensive dash");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 1, 25);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 7;
        }
        public override void UpdateEquip(Player player)
        {
            player.endurance += 0.05f;
        }
        private void AddCooldown(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter)
        {
            if (player.HasBuff<LightfootSabotonBuff>())
                player.AddBuff(ModContent.BuffType<LightfootSabotonDebuff>(), 1200);
        }
        private void LightfootDash(Player player)
        {
            if (player.armor[2].type == ModContent.ItemType<LightfootSaboton>())
            {
                player.dashType = ModContent.ItemType<LightfootSaboton>();
                RadiancePlayer rPlayer = player.GetModPlayer<RadiancePlayer>();

                if (player.dashTime > 0)
                    player.dashTime--;
                if (player.dashTime < 0)
                    player.dashTime++;

                if (!player.mount.Active && !player.stoned && !player.frozen)
                {
                    if (rPlayer.dashTimer > 0)
                    {
                        if(player.velocity.Y == 0f)
                            SpawnParticlesAtFeet(player, player.position + new Vector2(Main.rand.NextFloat(player.width), player.height + 4));
                        else
                            SpawnParticlesAtFeet(player, player.position + new Vector2(Main.rand.NextFloat(player.width / 2 - 8, player.width / 2 + 8), Main.rand.NextFloat(player.height / 2 - 8, player.height / 2 + 8)));

                        if (rPlayer.dashTimer < 25)
                            player.velocity.X *= 0.97f;
                    }
                    if(rPlayer.dashTimer == 0)
                    {
                        if (player.controlRight && player.releaseRight)
                        {
                            if (player.dashTime <= 0)
                            {
                                player.dashTime = 15;
                                return;
                            }
                            Dash(player);
                        }
                        else if (player.controlLeft && player.releaseLeft)
                        {
                            if (player.dashTime >= 0)
                            {
                                player.dashTime = -15;
                                return;
                            }
                            Dash(player);
                        }
                    }
                }
            }
        }
        private void Dash(Player player)
        {
            RadiancePlayer rPlayer = player.GetModPlayer<RadiancePlayer>();
            if (!player.HasBuff<LightfootSabotonDebuff>())
                player.AddBuff(ModContent.BuffType<LightfootSabotonBuff>(), 30);

            int dir = player.controlLeft ? -1 : 1;
            for (int i = 0; i < 24; i++)
            {
                SpawnParticlesAroundBody(player, dir);
            }
            rPlayer.dashTimer = 30;
            player.velocity.X = 16 * dir;
        }

        private void SpawnParticlesAtFeet(Player player, Vector2 position)
        {
            ParticleSystem.AddParticle(new Sparkle(position, Vector2.Zero, 30, 0, new Color(200, 180, 100), Main.rand.NextFloat(0.5f, 0.7f)));
        }
        private void SpawnParticlesAroundBody(Player player, int dir)
        {
            float bonusOffset = 8;
            Vector2 playerBody = player.position - new Vector2(bonusOffset) + new Vector2(Main.rand.NextFloat(player.width + bonusOffset * 2), Main.rand.NextFloat(player.height + bonusOffset * 2));
            ParticleSystem.AddParticle(new Sparkle(playerBody, Main.rand.NextVector2Circular(4, 4), Main.rand.Next(40, 80), 0, new Color(200, 180, 100), Main.rand.NextFloat(0.5f, 0.7f)));
        }
    }
    public class LightfootSabotonBuff : BaseBuff
    {
        public LightfootSabotonBuff() : base("Lightfoot Dodge", "50% reduced damage taken and immunity to knockback", false, false) { }
        public override void Update(Player player, ref int buffIndex)
        {
            player.noKnockback = true;
            player.endurance += 0.5f;
        }
    }
    public class LightfootSabotonDebuff : BaseBuff
    {
        public LightfootSabotonDebuff() : base("Lightfoot Falter", "Cannot gain Lightfoot Dodge", true) { }
    }
    //public class LegsSystem : ModSystem
    //{
    //    public static List<int> cachedLegs;
    //    public override void Load()
    //    {
    //        cachedLegs = new List<int>();
    //    }
    //    public override void Unload()
    //    {
    //        cachedLegs = null;
    //    }
    //    public override void OnWorldLoad()
    //    {
    //        for (int i = 0; i < ItemLoader.ItemCount; i++)
    //        {
    //            if (i <= 0 || i >= ItemLoader.ItemCount)
    //                continue;

    //            Item item = RadianceUtils.GetItem(i);
    //            if (item.legSlot == -1)
    //                continue;

    //            cachedLegs.Add(item.type);
    //        }
    //    }
    //}
}
