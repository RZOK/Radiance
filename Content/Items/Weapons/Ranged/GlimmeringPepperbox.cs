using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using System;
using Radiance.Core.Systems;
using Radiance.Utilities;
using static Terraria.Player;

namespace Radiance.Content.Items.Weapons.Ranged
{
    #region Main Item
    public class GlimmeringPepperbox : BaseInstrument
    {
        public static readonly SoundStyle ShootSound = new("Radiance/Sounds/PepperboxFire");
        public static readonly SoundStyle PrimeSound = new("Radiance/Sounds/PepperboxCock");
        public static Vector2 offset = new Vector2(0, 0);
        public override float ConsumeAmount => 0.1f;
        public bool madeSound = false;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glimmering Pepperbox");
            Tooltip.SetDefault("Placeholder Line\nFires a short-ranged burst of sparkling Radiance");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 4;
            Item.width = 34;
            Item.ArmorPenetration = 8;
            Item.height = 22;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = false;
            Item.UseSound = ShootSound;
            Item.rare = ItemRarityID.Blue;
            Item.knockBack = 4;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 0, 10);
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 2;
            Item.useAmmo = AmmoID.Bullet;
        }
        public override bool CanUseItem(Player player) => player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand >= ConsumeAmount;
        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SyncPlayer>().mouseListener = true;
        }
        public override bool? UseItem(Player player)
        {
            SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
            player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(ConsumeAmount);
            madeSound = false;
            float rotation = (player.Center - sPlayer.mouseWorld).ToRotation();
            for (int i = 0; i < 50; i++)
            {
                Dust d = Dust.NewDustPerfect(player.Center - new Vector2(Item.width / 2, Item.height * -player.direction / 2).RotatedBy(rotation), DustID.GoldFlame);
                d.position += Main.rand.NextVector2Square(-5, 5);
                d.velocity = Vector2.UnitX.RotatedBy(rotation).RotatedByRandom(0.5f) * Main.rand.NextFloat(-15, -0.5f) * Main.rand.Next(1, 3);
                d.scale = ((i % 4) + 1) * 0.6f;
                d.noGravity = true;
                if(i % 3 == 0)
                {
                    Dust f = Dust.NewDustPerfect(player.Center - new Vector2(Item.width / 2, Item.height * -player.direction / 2).RotatedBy(rotation), DustID.Smoke);
                    f.velocity = Vector2.UnitX.RotatedBy(rotation).RotatedByRandom(1) * Main.rand.NextFloat(-5, -1) - Vector2.UnitY;
                    f.scale = ((i % 4) + 1) * 0.6f;
                    f.noGravity = true;
                }
            }
            if(Math.Abs(player.velocity.X) < 20 && !player.noKnockback) 
                player.velocity += Vector2.UnitX * -3 * Math.Sign(sPlayer.mouseWorld.X - player.Center.X);
            return null;
        }
        public void SetItemInHand(Player player)
        {
            SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
            if (sPlayer.mouseWorld.X > player.Center.X)
                player.ChangeDir(1);
            else
                player.ChangeDir(-1);

            Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -2f * player.gravDir);
            float ease = RadianceUtils.EaseInHex((float)player.itemAnimation / Item.useAnimation);

            float itemRotation = (sPlayer.mouseWorld - itemPosition).ToRotation() - MathHelper.Lerp(0, 1, ease * player.direction * player.gravDir);

            Vector2 itemSize = new Vector2(34, 22);
            Vector2 itemOrigin = new Vector2(-Item.width / 2 - 6, 0);
            player.SetCompositeArmFront(true, CompositeArmStretchAmount.ThreeQuarters, itemRotation * player.gravDir - MathHelper.PiOver2);
            HoldStyleAdjustments(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
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

             if (stepDisplace)
            {
                int frame = player.bodyFrame.Y / player.bodyFrame.Height;
                if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                {
                    finalPosition -= Vector2.UnitY * 2f;
                }
            }
            player.itemLocation = finalPosition;
        }
        public override void UseItemFrame(Player player)
        {
            float progress = 1 - player.itemTime / (float)player.itemTimeMax;
            if(progress >= 0.7f && !madeSound)
            {
                //SoundEngine.PlaySound(PrimeSound, player.Center);
                madeSound = true;
            }
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player);
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (CameraSystem.Quake <= 3)
                CameraSystem.Quake = 3;
            for (int i = 0; i < 3; i++)
            {
                int p = Projectile.NewProjectile(source, player.Center + new Vector2(Item.width / 2, Item.height * -player.direction / 2).RotatedBy(velocity.ToRotation()), velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.9f, 1.1f), ModContent.ProjectileType<GlimmeringPepperboxSpark>(), damage, knockback, Main.myPlayer);
                Main.projectile[p].timeLeft = (Main.projectile[p].ModProjectile as GlimmeringPepperboxSpark).time = (i + 2) * 25;
                (Main.projectile[p].ModProjectile as GlimmeringPepperboxSpark).shift = Main.rand.Next(60);
            }
            return false;
        }
    }
    #endregion

    #region Spark Projectile
    public class GlimmeringPepperboxSpark : ModProjectile
    {
        public int time = 60;
        public int shift = 0;
        public float sineTime = 0;
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spark");
        }
        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.extraUpdates = 4;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 60;
            
        }
        public override void AI()
        {
            sineTime++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.position += Vector2.UnitY.RotatedBy(Projectile.rotation) * (float)Math.Sin((shift + sineTime) * 4 / time * MathHelper.Pi) * 4;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame);
                d.velocity *= 0.1f;
                d.noGravity = true;
                d.scale = MathHelper.Lerp(1.7f, 0.6f, (float)Projectile.timeLeft / time);
                d.position += Main.rand.NextVector2Circular(-6, 6);
        }
        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame);
                d.velocity = Projectile.velocity + (i % 2 == 0 ? Main.rand.NextVector2CircularEdge(4, 4) : Main.rand.NextVector2Circular(4, 4));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.9f, 1.3f);
                d.fadeIn = 1.4f;
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Vector2 size = new Vector2(24, 24);
            hitbox = new Rectangle((int)(Projectile.position.X - size.X / 2), (int)(Projectile.position.Y - size.Y / 2), (int)size.X, (int)size.Y);
        }
    }
    #endregion
}
