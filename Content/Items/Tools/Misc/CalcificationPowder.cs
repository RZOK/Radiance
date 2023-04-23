﻿using Microsoft.Xna.Framework;
using Radiance.Content.Items.StabilizationCrystals;
using Radiance.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Tools.Misc
{
    public class CalcificationPowder : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Calcification Powder");
            Tooltip.SetDefault("Petrifies plants when thrown");
            Item.ResearchUnlockCount = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Blue;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<CalcificationPowderDust>();
            Item.shootSpeed = 3;
            Item.consumable = true;
            Item.UseSound = SoundID.Item1;
        }
        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SyncPlayer>().mouseListener = true;
        }
        public override bool? UseItem(Player player)
        {
            for (int h = 0; h < 30; h++)
            { 
                Vector2 position = player.position + new Vector2(Main.rand.NextFloat(player.width), Main.rand.NextFloat(player.height));
                Vector2 velocity = Vector2.Normalize(player.GetModPlayer<SyncPlayer>().mouseWorld - position).RotatedByRandom(0.5f) * 8 * Main.rand.NextFloat(0.1f, 1);
                Dust dust = Dust.NewDustPerfect(position, DustID.PurificationPowder, velocity);
                dust.color = new Color(0, 255, 0);
                dust.scale = 1.1f;
            }
            return null;
        }
    }

    #region Projecitle

    public class CalcificationPowderDust : ModProjectile
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Calcification Powder");
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.98f;

            Rectangle clampBox = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);
            Point center = new Point((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            int radius = 2;
            int leftBound = Utils.Clamp(center.X - radius, clampBox.Left, clampBox.Right);
            int rightBound = Utils.Clamp(center.X + radius + 1, clampBox.Left, clampBox.Right);
            int topBound = Utils.Clamp(center.Y - radius, clampBox.Top, clampBox.Bottom);
            int bottomBound = Utils.Clamp(center.Y + radius + 1, clampBox.Top, clampBox.Bottom);

            for (int i = leftBound; i < rightBound; i++)
            {
                for (int j = topBound; j < bottomBound; j++)
                {
                    Tile tile = Framing.GetTileSafely(i, j);
                    if (tile.TileType == 71 && (tile.TileFrameX / 18) > 1)
                    {
                        Main.NewText(tile.TileFrameX);
                        WorldGen.KillTile(i, j, false, false, true);
                        for (int h = 0; h < 9; h++)
                        {
                            Dust dust = Main.dust[Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.BlueCrystalShard)];
                            dust.noGravity = true;
                            dust.velocity *= 0.3f;
                            dust.scale = 1.7f;
                            dust.fadeIn = 1.1f;
                        }
                        SoundEngine.PlaySound(SoundID.Tink, new Vector2(i * 16, j * 16));
                        Item.NewItem(new EntitySource_Misc("CalcificationPowder"), i * 16, j * 16, 16, 16, ModContent.ItemType<PetrifiedCrystal>());
                    }
                }
            }
        }
        public override bool? CanDamage() => false;
    }

    #endregion Spark Projectile
}