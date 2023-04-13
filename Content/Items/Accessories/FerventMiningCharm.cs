﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Radiance.Utilities;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Accessories
{
    public class FerventMiningCharm : BaseAccessory, IInstrument
    {
        public float consumeAmount => 0.01f;

        public override void Load()
        {
            RadiancePlayer.MeleeEffectsEvent += DrawParticlesAroundPickaxe;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem += DrawOutline;
            IL_Player.PickTile += DetectTileBreak;
        }

        public override void Unload()
        {
            RadiancePlayer.MeleeEffectsEvent -= DrawParticlesAroundPickaxe;
            On_PlayerDrawLayers.DrawPlayer_27_HeldItem -= DrawOutline;
            IL_Player.PickTile -= DetectTileBreak;
        }

        private void DrawParticlesAroundPickaxe(Player player, Item item, Rectangle hitbox)
        {
            float totalBoost = player.GetModPlayer<FerventMiningCharmPlayer>().totalBoost;
            if (totalBoost > 0)
            {
                if (player.Equipped<FerventMiningCharm>() && player.ItemAnimationActive)
                {
                    if (Main.rand.NextBool(31 - (int)totalBoost))
                    {
                        int a = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.GoldFlame, player.velocity.X * 0.2f + (float)(player.direction * 3), player.velocity.Y * 0.2f, 100, default(Color), 0.75f);
                        Main.dust[a].noGravity = true;
                    }
                    if (Main.rand.NextBool(31 - (int)totalBoost))
                    {
                        ParticleSystem.AddParticle(new Sparkle(new Vector2(hitbox.X, hitbox.Y) + new Vector2(Main.rand.NextFloat(hitbox.Width), Main.rand.NextFloat(hitbox.Height)), new Vector2(player.velocity.X * 0.2f + player.direction, player.velocity.Y * 0.2f), 30, 0, new Color(200, 180, 100), 0.6f));
                    }
                }
            }
        }

        private void DrawOutline(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawinfo)
        {
            Player player = drawinfo.drawPlayer;
            float totalBoost = player.GetModPlayer<FerventMiningCharmPlayer>().totalBoost;
            if (totalBoost > 10)
            {
                if (player.active && player.Equipped<FerventMiningCharm>() && player.ItemAnimationActive)
                {
                    float rotation = player.itemRotation;
                    if (player.direction == -1)
                        rotation -= MathHelper.PiOver2;
                    ParticleSystem.AddParticle(new PickaxeTrail(drawinfo.ItemLocation + new Vector2(player.itemWidth / 2, -player.itemHeight / 2).RotatedBy(rotation), TextureAssets.Item[player.GetPlayerHeldItem().type].Value, 12, rotation, new Color(200, 180, 100), 255 - (totalBoost - 10) / 20 * 255, drawinfo.heldItem.scale));
                }
            }
            orig(ref drawinfo);
        }

        private void DetectTileBreak(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<Player>("hitTile"),
                i => i.MatchLdloc(0),
                i => i.MatchLdloc(2),
                i => i.MatchLdcI4(1),
                i => i.MatchCallvirt<HitTile>("AddDamage"),
                i => i.MatchLdcI4(100),
                i => i.MatchBlt(out var _)
                ))
            {
                RadianceUtils.LogIlError("Fervent Mining Charm tile break detection", "Couldn't navigate to after if(hitTile.AddDamage(num, num2) >= 100)");
                return;
            }
            cursor.EmitDelegate(AddToStack);
        }

        private void AddToStack()
        {
            Player player = Main.LocalPlayer;
            if (player.active && player.Equipped<FerventMiningCharm>())
            {
                int tileType = Main.tile[Player.tileTargetX, Player.tileTargetY].TileType;
                if (TileID.Sets.Ore[tileType])
                {
                    player.GetModPlayer<FerventMiningCharmPlayer>().stackTimer -= 30;
                    if (player.GetModPlayer<FerventMiningCharmPlayer>().miningStack.Count < 3 || player.GetModPlayer<FerventMiningCharmPlayer>().miningStack.Keys.Contains(tileType))
                    {
                        if (!player.GetModPlayer<FerventMiningCharmPlayer>().miningStack.Keys.Contains(tileType))
                            player.GetModPlayer<FerventMiningCharmPlayer>().miningStack.Add(tileType, 1);
                        else
                            player.GetModPlayer<FerventMiningCharmPlayer>().miningStack[tileType] = Math.Min(10, player.GetModPlayer<FerventMiningCharmPlayer>().miningStack[tileType] + 1);
                    }
                }
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fervent Mining Charm");
            Tooltip.SetDefault("Mining ores increases your mining speed up to 200%\nConsumes Radiance to keep the boost going");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.value = Item.sellPrice(0, 0, 50);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }
    }

    public class FerventMiningCharmPlayer : ModPlayer
    {
        internal Dictionary<int, int> miningStack;
        internal int stackTimer = 0;
        internal float totalBoost = 0;
        public FerventMiningCharmPlayer()
        {
            miningStack = new Dictionary<int, int>();
        }

        public override void Unload()
        {
            miningStack = null;
        }
        public override void ResetEffects()
        {
            totalBoost = 0;
        }
        public override void PostUpdateMiscEffects()
        {
            if (Player.Equipped<FerventMiningCharm>())
            {
                if (Player.GetPlayerHeldItem().pick > 0)
                {
                    miningStack.Values.ToList().ForEach(x => totalBoost += x);
                    Player.pickSpeed -= totalBoost / 60;
                    Player.GetAttackSpeed<MeleeDamageClass>() += totalBoost / 60;
                }
                if (miningStack.Count > 0)
                {
                    stackTimer++;
                    if (stackTimer == 600)
                    {
                        foreach (int item in miningStack.Keys)
                        {
                            miningStack[item]--;
                            if (miningStack[item] == 0)
                                miningStack.Remove(item);
                        }
                        if (!Player.ConsumeRadianceOnHand(0.01f * totalBoost))
                            miningStack.Clear();
                        stackTimer = 0;
                    }
                }
                else
                    stackTimer = 0;
            }
            else
            {
                miningStack.Clear();
                stackTimer = 0;
            }
        }
    }
}