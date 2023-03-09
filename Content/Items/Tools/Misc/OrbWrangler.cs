using Microsoft.Xna.Framework;
using Radiance.Core;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Utilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.BaseItems;
using System.Linq;
using static Terraria.Player;
using Terraria.Audio;

namespace Radiance.Content.Items.Tools.Misc
{
    public class OrbWrangler : BaseInstrument
    {
        public override float ConsumeAmount => 0.0005f;
        public const int maxDistance = 160;
        public Vector2 AttachedOrbPosition { get; set; }
        public float shakeTimer = 0;
        public OrbWranglerWrangledOrb Orb { get => Main.player[Item.playerIndexTheItemIsReservedFor].GetModPlayer<OrbWranglerPlayer>().Orb; set => Main.player[Item.playerIndexTheItemIsReservedFor].GetModPlayer<OrbWranglerPlayer>().Orb = value; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orb Wrangler");
            Tooltip.SetDefault("Placeholder Line\nHolds an orb that provides a great amount of light when held\nLeft click to launch the orb, or magnetize it back if already thrown out");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.holdStyle = 16;
        }
        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SyncPlayer>().mouseListener = true;
            if (!Main.projectile.Any(x => x.type == ModContent.ProjectileType<OrbWranglerWrangledOrb>() && x.active && x.owner == player.whoAmI) && player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand >= ConsumeAmount * player.GetRadianceDiscount())
                Orb = (OrbWranglerWrangledOrb)Main.projectile[Projectile.NewProjectile(Item.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<OrbWranglerWrangledOrb>(), 0, 0, player.whoAmI)].ModProjectile;
            if (player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand < ConsumeAmount * player.GetRadianceDiscount() && Orb != null)
            {
                Orb.Projectile.active = false;
                Orb = null;
            }
            if(Orb != null && Orb.attached)
                Orb.Projectile.timeLeft = 2;
        }
        public override void UpdateInventory(Player player)
        {
            if (shakeTimer > 0)
                shakeTimer--;
        }
        public void SetItemInHand(Player player)
        {
            SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
            if (sPlayer.mouseWorld.X > player.Center.X)
                player.ChangeDir(1);
            else
                player.ChangeDir(-1);
            
            Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -2f * player.gravDir);
            if(shakeTimer > 0)
                itemPosition += Main.rand.NextVector2Square(-shakeTimer / 2, shakeTimer / 2);
            float itemRotation = (sPlayer.mouseWorld - itemPosition).ToRotation();

            Vector2 itemSize = new Vector2(56, 28);
            Vector2 itemOrigin = new Vector2(-32, -4);
            player.SetCompositeArmFront(true, CompositeArmStretchAmount.ThreeQuarters, itemRotation * player.gravDir - MathHelper.PiOver2);
            player.SetCompositeArmBack(true, CompositeArmStretchAmount.ThreeQuarters, itemRotation * player.gravDir - MathHelper.PiOver2 + player.direction * 0.5f);
            HoldStyleAdjustments(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
        }
        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation == Item.useAnimation)
            {
                if (Orb != null)
                {
                    if (Orb.attached)
                    {
                        SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
                        Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -2f * player.gravDir);
                        float itemRotation = (sPlayer.mouseWorld - itemPosition).ToRotation();
                        for (int i = 0; i < 8; i++)
                        {
                            int d = Dust.NewDust(AttachedOrbPosition - new Vector2(4, 4), 1, 1, DustID.GoldFlame);
                            Main.dust[d].velocity = itemRotation.ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(1, 8);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].scale = 1.7f;
                        }
                        Orb.Projectile.velocity = itemRotation.ToRotationVector2() * 8;
                        Orb.attached = false;
                        Orb.Projectile.timeLeft = 3600;
                        SoundEngine.PlaySound(SoundID.Item108, player.Center);
                    }
                    else
                    {
                        if (Orb.Projectile.Distance(AttachedOrbPosition) < maxDistance)
                        {
                            Orb.returningStartPos = Orb.Projectile.Center;
                            Orb.returning = true;
                            Orb.Projectile.velocity = Vector2.Zero;
                        }
                        else
                        {
                            shakeTimer = 16;
                            SoundEngine.PlaySound(SoundID.Item23, player.Center);
                        }
                    }
                }
            }
            return base.UseItem(player);
        }
        public void HoldStyleAdjustments(Player player, float desiredRotation, Vector2 desiredPosition, Vector2 spriteSize, Vector2? rotationOriginFromCenter = null, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
        {
            if (noSandstorm)
                player.sandStorm = false;

            if (rotationOriginFromCenter == null)
                rotationOriginFromCenter = new Vector2?(Vector2.Zero);

            Vector2 origin = rotationOriginFromCenter.Value;
            origin.X *= player.direction;
            origin.Y *= player.gravDir;
            player.itemRotation = desiredRotation;

            if (flipAngle)
                player.itemRotation *= player.direction;
            else if (player.direction < 0)
                player.itemRotation += MathHelper.Pi;

            Vector2 consistentAnchor = player.itemRotation.ToRotationVector2() * (spriteSize.X / -2f - 10f) * player.direction - origin.RotatedBy(player.itemRotation);
            Vector2 offsetAgain = spriteSize * -0.5f;
            Vector2 finalPosition = desiredPosition + offsetAgain + consistentAnchor;

            Vector2 orbPosition = desiredPosition + (Vector2.UnitX * (spriteSize.X + 4)).RotatedBy(player.itemRotation + player.fullRotation) * player.direction + consistentAnchor + Vector2.UnitY * player.gfxOffY;
            if (stepDisplace)
            {
                int frame = player.bodyFrame.Y / player.bodyFrame.Height;
                if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                {
                    finalPosition -= Vector2.UnitY * 2f;
                    orbPosition -= Vector2.UnitY * 2f;
                }
            }
            AttachedOrbPosition = orbPosition;
            if (Orb != null && Orb.attached)
            {
                Orb.Projectile.Center = orbPosition;
                Orb.Projectile.rotation = player.itemRotation + player.fullRotation;
            }
            
            player.itemLocation = finalPosition;
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player);
        public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player);
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ShimmeringGlass>(7)
                .AddTile(TileID.Anvils)
                .AddCondition(Recipe.Condition.NearLava)
                .Register();
        }
        public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick) //doesn't work yet :sob:
        {
            glowstick = true; 
        }
    }
    public class OrbWranglerWrangledOrb : ModProjectile
    {
        public bool attached = true;
        public bool returning = false;
        public Vector2 returningStartPos = Vector2.Zero;
        public const float consumeAmount = 0.0005f;
        public Player Owner { get => Main.player[Projectile.owner]; }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wrangled Orb");
        }
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }
        public override bool? CanDamage() => false;
        public override void AI()
        {
            if (Owner.GetModPlayer<RadiancePlayer>().currentRadianceOnHand >= consumeAmount * Owner.GetRadianceDiscount())
                Owner.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(consumeAmount * Owner.GetRadianceDiscount());
            else
                Projectile.Kill();
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 10)
            {
                Projectile.ai[0] = 0f;
                int tileDistance = 30;
                if ((Projectile.Center - Main.player[Main.myPlayer].Center).Length() < (float)(Main.screenWidth + tileDistance * 16))
                    Main.instance.SpelunkerProjectileHelper.AddSpotToCheck(Projectile.Center);
            }
            if (!attached && !returning)
            {
                if (Projectile.lavaWet || Owner.Distance(Projectile.Center) > 2000)
                    Projectile.Kill();

                Projectile.tileCollide = true;
                Projectile.rotation += Projectile.velocity.X / 10;
                if (Projectile.velocity.Y < 16)
                    Projectile.velocity.Y += 0.2f;
                if (Projectile.velocity.Y > 16)
                    Projectile.velocity.Y = 16;

                Projectile.velocity.X *= 0.99f;
                if (Math.Abs(Projectile.velocity.X) <= 0.1f)
                {
                    Projectile.netUpdate = true;
                    Projectile.velocity.X = 0;
                }
            }
            else
            {
                OrbWrangler orbWrangler = Main.player[Main.myPlayer] == Owner ? RadianceUtils.GetPlayerHeldItem().ModItem as OrbWrangler : null;
                if (orbWrangler != null)
                {
                    if (returning)
                    {
                        Projectile.rotation += 0.05f;
                        for (int i = 0; i < 2; i++)
                        {
                            int c = Dust.NewDust(Projectile.position, 24, 24, DustID.GoldFlame, Projectile.velocity.X, Projectile.velocity.Y);
                            Main.dust[c].velocity *= 0.5f;
                            Main.dust[c].noGravity = true;
                            Main.dust[c].scale = 1.5f;
                        }
                        Projectile.velocity = Vector2.Normalize(orbWrangler.AttachedOrbPosition - returningStartPos) * Vector2.Distance(orbWrangler.AttachedOrbPosition, returningStartPos) / 6;
                        if (Vector2.Distance(orbWrangler.AttachedOrbPosition, Projectile.Center) < 4)
                        {
                            for (int i = 0; i < 32; i++)
                            {
                                int d = Dust.NewDust(orbWrangler.AttachedOrbPosition, 24, 24, DustID.GoldFlame);
                                Main.dust[d].velocity *= 0f;
                                Main.dust[d].position = orbWrangler.AttachedOrbPosition + Main.rand.NextVector2Circular(12, 12);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].scale = 1.5f;
                            }
                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop"), orbWrangler.AttachedOrbPosition);
                            attached = true;
                            returning = false;
                            Projectile.velocity = Vector2.Zero;
                        }
                    }
                    Projectile.tileCollide = false;
                }

                if (orbWrangler == null || !Owner.active || Owner.dead)
                {
                    Owner.GetModPlayer<OrbWranglerPlayer>().Orb = null;
                    Projectile.active = false;
                }
            }
            float strength = 4;
            Lighting.AddLight(Projectile.Center, 1 * strength, 1 * strength, 0.5f * strength);
        }
        public override void Kill(int timeLeft)
        {
            Owner.GetModPlayer<OrbWranglerPlayer>().Orb = null;
            for (int i = 0; i < 32; i++)
            {
                int d = Dust.NewDust(Projectile.position, 24, 24, DustID.GoldFlame);
                Main.dust[d].velocity.X = 0;
                Main.dust[d].velocity.Y = Main.rand.NextFloat(0, -3) * Main.rand.NextFloat(1, 2.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.5f;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!attached)
            { 
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X / 2;
                    Projectile.velocity.Y *= 0.7f;
                }
                if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0.5f)
                {
                    Projectile.velocity.Y = -oldVelocity.Y / 2;
                    Projectile.velocity.X *= 0.7f;
                }
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            RadianceDrawing.DrawSoftGlow(Projectile.Center, CommonColors.RadianceColor1, 0.5f, RadianceDrawing.DrawingMode.Projectile);
            return true;
        }
    }
    public class OrbWranglerPlayer : ModPlayer
    {
        public OrbWranglerWrangledOrb Orb { get; set; }
    }
}