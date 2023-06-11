using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Projectiles;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Weapons.Ranged
{
    #region Main Item

    public class FleshCatalyzer : ModItem, IInstrument
    {
        public float consumeAmount => 0.1f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flesh Catalyzer");
            Tooltip.SetDefault("Fires syringes that inject enemies with Radiance until they explode\n25% chance to not consume ammo");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.width = 62;
            Item.height = 32;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item108.WithPitchOffset(0.3f);
            Item.rare = ItemRarityID.Lime;
            Item.knockBack = 0.1f;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 7, 50);
            Item.shoot = ModContent.ProjectileType<FleshCatalyzerSyringeBullet>();
            Item.shootSpeed = 16f;
            Item.useAmmo = ModContent.ItemType<FleshCatalyzerSyringe>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));
            if (Collision.CanHit(position, 0, 0, position + velocity, 0, 0))
                position += velocity * 2;
            FleshCatalyzerSyringeBullet proj = Main.projectile[Projectile.NewProjectile(source, position, velocity, type, damage / 4, knockback, Main.myPlayer, 0, 0)].ModProjectile as FleshCatalyzerSyringeBullet;
            proj.shotFC = Item;
            proj.charged = player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(consumeAmount);
            return false;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => !Main.rand.NextBool(4);

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-9f, 0f);
        }
    }

    #endregion Main Item

    #region Syringe Item

    public class FleshCatalyzerSyringe : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Syringe");
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 10;
            Item.height = 18;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.value = Item.buyPrice(0, 0, 0, 7);
            Item.rare = ItemRarityID.White;
            Item.knockBack = 0.5f;
            Item.ammo = ModContent.ItemType<FleshCatalyzerSyringe>();
            Item.shootSpeed = 6f;
            Item.shoot = ModContent.ProjectileType<FleshCatalyzerSyringeBullet>();
        }
    }

    #endregion Syringe Item

    #region Syringe Bullet Projectile

    public class FleshCatalyzerSyringeBullet : ModProjectile
    {
        public bool charged = false;
        public Item shotFC = new(0, 1);
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Syringe");
        }
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 5;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.X -= 6; //hitbox size / 2 - 2
            hitbox.Y -= 6;
            hitbox.Width = hitbox.Height = 16;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                CameraSystem.Quake += 4;
                FleshCatalyzerSyringeProjectile proj = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<FleshCatalyzerSyringeProjectile>(), Projectile.damage, Projectile.knockBack, Main.myPlayer)].ModProjectile as FleshCatalyzerSyringeProjectile;
                proj.targetWhoAmI = target.whoAmI;
                proj.isStickingToTarget = true;
                proj.Projectile.velocity = (target.Center - Projectile.Center) * 0.75f;
                proj.Projectile.netUpdate = true;
                proj.Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                proj.maxRadianceContained = proj.radianceContained = (charged ? 18 : 0) * (hit.Crit ? 2 : 1);
                proj.isCrit = hit.Crit;
                target.GetGlobalNPC<FleshCatalyzerNPC>().shotFC = shotFC;
                for (int k = 0; k < proj.Projectile.oldPos.Length; k++)
                {
                    proj.Projectile.oldPos[k] = Projectile.position - Projectile.velocity * k;
                }
            }
            SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);

            Vector2 velocity = Projectile.velocity;
            if (Projectile.velocity.X != oldVelocity.X)
                velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                velocity.Y = -oldVelocity.Y;
            if (Main.netMode != NetmodeID.Server)
            {
                int goreType = Mod.Find<ModGore>("FleshCatalyzerSyringeGore").Type;
                int g = Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, velocity / 8, goreType);
                Main.gore[g].timeLeft = 1;
                Main.gore[g].sticky = false;
                Main.gore[g].rotation = Projectile.rotation;
            }
            int width;
            int height = width = 4;
            Collision.HitTiles(Projectile.Center - new Vector2(width, height) / 2, Projectile.velocity, width, height);

            return base.OnTileCollide(oldVelocity);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition - Vector2.Normalize(Projectile.velocity) * texture.Width, null, lightColor, Projectile.rotation, Vector2.Zero, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    #endregion Syringe Bullet Projectile

    #region Syringe Projectile

    public class FleshCatalyzerSyringeProjectile : ModProjectile
    {
        public Texture2D plungerTexture;
        public float maxRadianceContained = 18;
        public float radianceContained = 0;
        public bool isCrit = false;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Syringe");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 180;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.ai[0] == 1f)
            {
                int npcIndex = (int)Projectile.ai[1];
                if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active)
                {
                    if (Main.npc[npcIndex].behindTiles)
                        behindNPCsAndTiles.Add(index);
                    else
                        behindNPCs.Add(index);
                    return;
                }
            }
            behindProjectiles.Add(index);
        }

        public bool isStickingToTarget
        {
            get { return Projectile.ai[0] == 1f; }
            set { Projectile.ai[0] = value ? 1f : 0f; }
        }

        public int targetWhoAmI
        {
            get { return (int)Projectile.ai[1]; }
            set { Projectile.ai[1] = value; }
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D plungerTexture = ModContent.Request<Texture2D>("Radiance/Content/Items/Weapons/Ranged/FleshCatalyzerSyringeProjectilePlunger").Value;
            float fill = 1;
            if (maxRadianceContained > 0)
            {
                fill = radianceContained / maxRadianceContained;
                Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Items/Weapons/Ranged/FleshCatalyzerSyringeProjectileGlow").Value;
                Main.spriteBatch.Draw(
                    glowTexture,
                    new Vector2(Projectile.Center.X, Projectile.Center.Y) - Main.screenPosition,
                    new Rectangle(0, 0, glowTexture.Width, (int)(fill * glowTexture.Height)),
                    Color.Lerp(CommonColors.RadianceColor1, CommonColors.RadianceColor2, fill * RadianceUtils.SineTiming(5)),
                    Projectile.rotation,
                    new Vector2(glowTexture.Width / 2, glowTexture.Height / 2 - 4),
                    Projectile.scale,
                    SpriteEffects.None,
                    0);
            }
            Main.spriteBatch.Draw(
                plungerTexture,
                new Vector2(Projectile.Center.X, Projectile.Center.Y) - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                new Vector2(plungerTexture.Width / 2, plungerTexture.Height / 2 - 6 - (6 * fill)),
                Projectile.scale,
                SpriteEffects.None,
                0);
        }

        public override void AI()
        {
            if (maxRadianceContained > 0)
                Lighting.AddLight(Projectile.Center, CommonColors.RadianceColor1.ToVector3() * radianceContained / maxRadianceContained);
            if (isStickingToTarget)
            {
                if (maxRadianceContained > 0)
                {
                    radianceContained -= 0.1f * (isCrit ? 2 : 1);
                    Main.npc[targetWhoAmI].GetGlobalNPC<FleshCatalyzerNPC>().radianceContained += 0.1f * (isCrit ? 2 : 1);
                    Main.npc[targetWhoAmI].GetGlobalNPC<FleshCatalyzerNPC>().leakTimer = 300;
                }
                if ((targetWhoAmI < 0 || targetWhoAmI >= 200))
                    Projectile.Kill();
                else if (Main.npc[targetWhoAmI].active && !Main.npc[targetWhoAmI].dontTakeDamage)
                {
                    Projectile.Center = Main.npc[targetWhoAmI].Center - Projectile.velocity * 2f;
                    Projectile.gfxOffY = Main.npc[targetWhoAmI].gfxOffY;
                }
                else
                    Projectile.Kill();
            }
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                int goreType = Mod.Find<ModGore>("FleshCatalyzerSyringeGore").Type;
                int g = Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Vector2.UnitX.RotatedBy(Projectile.rotation - MathHelper.PiOver2) * -1 - Vector2.UnitY * 2, goreType);
                Main.gore[g].rotation = Projectile.rotation;
                Main.gore[g].sticky = false;
                Main.gore[g].timeLeft = 1;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft >= 174)
            {
                Vector2 drawOrigin = new(TextureAssets.Projectile[Projectile.type].Width() * 0.5f, Projectile.height * 0.5f);
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Main.spriteBatch.Draw(TextureAssets.Item[ModContent.ItemType<FleshCatalyzerSyringe>()].Value, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }
    }

    #endregion Syringe Projectile

    #region GlobalNPC

    public class FleshCatalyzerNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private static readonly SoundStyle sound = new("Radiance/Sounds/Goresplosion");

        public float radianceContained = 0;
        public float leakTimer = 0;
        public float explosionTimer = 0;
        public Item shotFC = new(0, 1);

        public override void PostAI(NPC npc)
        {
            leakTimer -= Math.Min(leakTimer, 1);
            if (leakTimer == 0 && explosionTimer == 0 && radianceContained > 0)
                radianceContained -= Math.Min(radianceContained, 0.1f);
            float size = npc.Hitbox.Width * 2 + npc.Hitbox.Height * 2;
            if (radianceContained >= size)
                explosionTimer++;
            if (explosionTimer >= 45)
            {
                Explode(npc);
                radianceContained = explosionTimer = 0;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            float size = npc.Hitbox.Width * 2 + npc.Hitbox.Height * 2;
            if (explosionTimer > 0)
            {
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 2; j++)
                        RadianceDrawing.DrawBeam(npc.Center, npc.Center + (Vector2.UnitX * 200).RotatedBy(MathHelper.PiOver2 * i), (j == 0 ? CommonColors.RadianceColor1 : new Color(255, 255, 255, 255)).ToVector4() * (explosionTimer / 45), 0, j == 0 ? 20 : 16, RadianceDrawing.SpriteBatchData.WorldDrawingData);
                
                Main.spriteBatch.End();
                RadianceDrawing.SpriteBatchData.WorldDrawingData.BeginSpriteBatchFromTemplate(BlendState.Additive);

                for (int i = 0; i < 2; i++)
                    RadianceDrawing.DrawSoftGlow(npc.Center, (i == 0 ? CommonColors.RadianceColor1 : new Color(255, 255, 255, 255)) * (explosionTimer / 45), RadianceUtils.EaseOutCirc(explosionTimer / 45) * (size / 100) / (i == 0 ? 2 : 3));
                
                Main.spriteBatch.End();
                RadianceDrawing.SpriteBatchData.WorldDrawingData.BeginSpriteBatchFromTemplate();
            }
            if (radianceContained > 0)
            {
                for (int i = 0; i < 2; i++)
                    RadianceDrawing.DrawSoftGlow(npc.Center, (i == 0 ? CommonColors.RadianceColor1 : new Color(255, 255, 255, 150)) * (Math.Clamp(radianceContained / size, 0, 1) / 2), (0.5f + Math.Clamp(radianceContained / size, 0, 1)) * (i == 0 ? 1 : 0.75f));
                for (int i = 0; i < 5 * (radianceContained / size); i++)
                {
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GoldCoin, 0, 0);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity.X *= 0f;
                    Main.dust[d].velocity.Y = -Math.Abs(Main.dust[d].velocity.Y);
                }
            }
        }

        public static void Explode(NPC npc)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int j = 0; j < 20; j++)
                {
                    int gore = Gore.NewGore(npc.GetSource_Misc("FleshCatalyzer"), new Vector2(npc.Center.X, npc.Center.Y), Main.rand.NextVector2Circular(10, 10), Main.rand.Next(61, 64));
                    Main.gore[gore].scale = Main.rand.NextFloat(0.8f, 1.7f);
                }
            }
            CameraSystem.Quake += 40;
            SoundEngine.PlaySound(sound, npc.Center);
            SoundEngine.PlaySound(SoundID.Item62, npc.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int damage = Main.LocalPlayer.GetWeaponDamage(npc.GetGlobalNPC<FleshCatalyzerNPC>().shotFC) * 10 * (int)Math.Max(npc.GetGlobalNPC<FleshCatalyzerNPC>().radianceContained / 200, 1);
                Projectile.NewProjectile(npc.GetSource_Misc("FleshCatalyzer"), npc.Center, Vector2.Zero, ModContent.ProjectileType<FleshCatalyzerExplosion>(), damage, 0, Main.myPlayer, npc.whoAmI);
                for (int i = 0; i < 6; i++)
                {
                    TempBeam proj = Main.projectile[Projectile.NewProjectile(npc.GetSource_Misc("FleshCatalyzer"), npc.Center, Vector2.Zero, ModContent.ProjectileType<TempBeam>(), 0, 0, Main.myPlayer)].ModProjectile as TempBeam;
                    proj.startPos = npc.Center;
                    proj.endPos = npc.Center - (Vector2.UnitX * Main.rand.Next(600, 900)).RotatedByRandom(MathHelper.Pi / Math.Max(1, 6)).RotatedBy(MathHelper.TwoPi / Math.Max(1, 4) * i);
                    proj.color = CommonColors.RadianceColor1;
                    proj.lifetime = proj.Projectile.timeLeft = Main.rand.Next(30, 60);
                    proj.innerWidth = Main.rand.Next(20, 35);
                    proj.outerWidth = proj.innerWidth * 2;
                    proj.spike = true;
                }
            }
            for (int i = 0; i < 300; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(25, 25);
                if (i % 2 == 0) vel /= 1.3f;
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GoldCoin, vel.X, vel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = Main.rand.NextFloat(1.2f, 2.1f);
                Main.dust[d].scale = Main.rand.NextFloat(2.5f, 2.9f);
            }
            for (int i = 0; i < 100; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(20, 20);
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GoldCoin, vel.X, vel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = Main.rand.NextFloat(1.2f, 1.9f);
                Main.dust[d].scale = Main.rand.NextFloat(1.7f, 2.5f);
                Main.dust[d].velocity *= Main.rand.NextFloat(1.5f, 3);
            }

            List<SoundStyle?> bloodList = new() { SoundID.NPCHit1, SoundID.NPCHit13, SoundID.NPCHit14, SoundID.NPCHit18, SoundID.NPCHit19, SoundID.NPCHit21, SoundID.NPCHit22, SoundID.NPCHit24, SoundID.NPCHit25, SoundID.NPCHit26 }; //ill do the rest later
            if (bloodList.Contains(npc.HitSound))
            {
                for (int i = 0; i < 100; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(10, 10);
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, vel.X, vel.Y);
                    Main.dust[d].scale = Main.rand.NextFloat(1.2f, 1.9f);
                    Main.dust[d].velocity *= Main.rand.NextFloat(1, 2);
                }
            }
        }
    }

    #endregion GlobalNPC

    #region Explosion Projectile

    public class FleshCatalyzerExplosion : ModProjectile
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Unshackled Radiance");
        }
        public override void SetDefaults()
        {
            Projectile.width = 400;
            Projectile.height = 400;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 3;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target == Main.npc[(int)Projectile.ai[0]])
                modifiers.SetCrit();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => RadianceUtils.AABBvCircle(targetHitbox, Projectile.Center, Projectile.width / 2f);
    }

    #endregion Explosion Projectile
}