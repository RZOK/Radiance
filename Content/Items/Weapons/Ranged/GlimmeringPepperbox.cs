using Terraria.Graphics.Effects;
using static Terraria.Player;

namespace Radiance.Content.Items.Weapons.Ranged
{
    #region Main Item

    public class GlimmeringPepperbox : ModItem, IInstrument
    {
        public static readonly SoundStyle ShootSound = new("Radiance/Sounds/PepperboxFire");
        public static readonly SoundStyle PrimeSound = new("Radiance/Sounds/PepperboxCock");

        public float consumeAmount => 0.1f;

        public bool madeSound = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glimmering Pepperbox");
            Tooltip.SetDefault("Fires a short-ranged burst of sparkling Radiance");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 8;
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

        public override bool CanUseItem(Player player) => player.GetModPlayer<RadiancePlayer>().ConsumeRadiance(consumeAmount);

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SyncPlayer>().mouseListener = true;
        }

        public override bool? UseItem(Player player)
        {
            SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
            madeSound = false;
            float rotation = (player.Center - sPlayer.mouseWorld).ToRotation();
            for (int i = 0; i < 50; i++)
            {
                Dust d = Dust.NewDustPerfect(player.Center - new Vector2(Item.width / 2, Item.height * -player.direction / 2 - 2).RotatedBy(rotation), DustID.GoldFlame);
                d.position += Main.rand.NextVector2Square(-5, 5);
                d.velocity = Vector2.UnitX.RotatedBy(rotation).RotatedByRandom(0.5f) * Main.rand.NextFloat(-15, -0.5f) * Main.rand.Next(1, 3);
                d.scale = ((i % 4) + 1) * 0.6f;
                d.noGravity = true;
                if (i % 3 == 0)
                {
                    Dust f = Dust.NewDustPerfect(player.Center - new Vector2(Item.width / 2, Item.height * -player.direction / 2 - 2).RotatedBy(rotation), DustID.Smoke);
                    f.velocity = Vector2.UnitX.RotatedBy(rotation).RotatedByRandom(1) * Main.rand.NextFloat(-5, -1) - Vector2.UnitY;
                    f.scale = ((i % 4) + 1) * 0.6f;
                    f.noGravity = true;
                }
            }
            if (Math.Abs(player.velocity.X) < 20 && !player.noKnockback)
                player.velocity += Vector2.UnitX * -2 * Math.Sign(sPlayer.mouseWorld.X - player.Center.X);
            return null;
        }

        public void SetItemInHand(Player player)
        {
            float itemRotation = player.compositeFrontArm.rotation + PiOver2 * player.gravDir;
            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 7f;
            Vector2 itemSize = new Vector2(34, 22);
            Vector2 itemOrigin = new Vector2(-Item.width / 2 + 4, 2);
            player.SetCompositeArmFront(true, CompositeArmStretchAmount.ThreeQuarters, itemRotation * player.gravDir - PiOver2);
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
                player.itemRotation += Pi;

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
            SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
            if (sPlayer.mouseWorld.X > player.Center.X)
                player.ChangeDir(1);
            else
                player.ChangeDir(-1);
            float ease = EaseInExponent((float)player.itemAnimation / Item.useAnimation, 6);
            player.SetCompositeArmFront(true, CompositeArmStretchAmount.ThreeQuarters, (sPlayer.mouseWorld - player.Center).ToRotation() * player.gravDir - Lerp(0, 1, ease * player.direction * player.gravDir) * player.gravDir - PiOver2);

            float progress = 1 - player.itemTime / (float)player.itemTimeMax;
            if (progress >= 0.7f && !madeSound)
            {
                //SoundEngine.PlaySound(PrimeSound, player.Center);
                madeSound = true;
            }
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3; i++)
            {
                int p = Projectile.NewProjectile(source, player.Center + new Vector2(Item.width / 2, Item.height * player.gravDir * -player.direction / 2 - 2).RotatedBy(velocity.ToRotation()), velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.9f, 1.1f), ModContent.ProjectileType<GlimmeringPepperboxSpark>(), damage, knockback, Main.myPlayer);
                Main.projectile[p].timeLeft = (Main.projectile[p].ModProjectile as GlimmeringPepperboxSpark).time = (i + 2) * 15;
                (Main.projectile[p].ModProjectile as GlimmeringPepperboxSpark).shift = Main.rand.Next(60);
            }
            return false;
        }
    }

    #endregion Main Item

    #region Spark Projectile

    public class GlimmeringPepperboxSpark : ModProjectile
    {
        public int time = 60;
        public int shift = 0;
        public ref float sineTime => ref Projectile.ai[0];
        public bool canDamage = true;
        private bool disappearing = false;

        public bool Disappearing
        {
            get => disappearing;
            set
            {
                Projectile.tileCollide = false;
                Projectile.timeLeft = 255;
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
                disappearing = true;
                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                for (int i = 0; i < 6; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame);
                    d.velocity = Projectile.velocity + (i % 2 == 0 ? Main.rand.NextVector2CircularEdge(2, 2) : Main.rand.NextVector2Circular(2, 2));
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.7f, 1f);
                    d.fadeIn = 1.4f;
                }
            }
        }

        private float modifier => ((float)(255 - Projectile.alpha) / 255);

        internal PrimitiveTrail TrailDrawer;
        private List<Vector2> cache;
        public int trailLength = 30;
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
            Projectile.extraUpdates = 3;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 60;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            sineTime++;
            Projectile.position += Vector2.UnitY.RotatedBy(Projectile.rotation) * (float)Math.Sin((shift + sineTime) * 4 / time * Pi) * Projectile.velocity.Length() / 3;
            if (!disappearing)
            {
                if (Projectile.timeLeft == 1)
                    Disappearing = true;

                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            else
            {
                Projectile.velocity *= 0.92f;
                Projectile.alpha += 5;
                if (Projectile.alpha >= 255)
                    Projectile.Kill();
            }
            ManageCache();
            ManageTrail();
        }

        public void ManageCache()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < trailLength; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }
            cache.Add(Projectile.Center);
            while (cache.Count > trailLength)
            {
                cache.RemoveAt(0);
            }
        }

        public void ManageTrail()
        {
            TrailDrawer = TrailDrawer ?? new PrimitiveTrail(30, f =>
            {
                return 5f;
            }, factor =>
            {
                float trailOpacity = 0.75f * (float)Math.Pow(factor, 0.1f);
                Color trailColor;

                if (factor > 0.5f)
                    trailColor = Color.Lerp(CommonColors.RadianceColor1 * modifier, new Color(255, 200, 150) * modifier, 2f * (factor - 0.5f));
                else
                    trailColor = Color.Lerp(CommonColors.RadianceColor2 * modifier, CommonColors.RadianceColor1 * modifier, 2f * factor);

                return trailColor * trailOpacity;
            }, new TriangularTip(8f));
            TrailDrawer.SetPositionsSmart(cache, Projectile.Center, RigidPointRetreivalFunction);
            TrailDrawer.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Disappearing = true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!disappearing)
            {
                canDamage = false;
                Disappearing = true;
                return false;
            }
            return true;
        }

        public override bool? CanDamage() => canDamage;

        public override bool PreDraw(ref Color lightColor)
        {
            
            Effect effect = Filters.Scene["FadedUVMapStreak"].GetShader().Shader;
            effect.Parameters["time"].SetValue(0f);
            effect.Parameters["fadeDistance"].SetValue(0.3f);
            effect.Parameters["fadePower"].SetValue(1 / 16f);
            effect.Parameters["trailTexture"].SetValue(ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/BasicTrail").Value);

            TrailDrawer?.Render(effect, -Main.screenPosition);

            RadianceDrawing.DrawSoftGlow(Projectile.Center, CommonColors.RadianceColor1 * 0.5f * modifier, 0.6f);

            Texture2D star = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Star").Value;
            Main.spriteBatch.Draw(
                star,
                Projectile.Center - Main.screenPosition,
                null,
                new Color(CommonColors.RadianceColor1.R * modifier, CommonColors.RadianceColor1.G * modifier, CommonColors.RadianceColor1.B * modifier, 0) * modifier * 0.5f,
                (shift + sineTime) / 10,
                star.Size() / 2,
                0.2f,
                0,
                0
                );

            return false;
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Vector2 size = new Vector2(24, 24);
            hitbox = new Rectangle((int)(Projectile.position.X - size.X / 2), (int)(Projectile.position.Y - size.Y / 2), (int)size.X, (int)size.Y);
        }
    }

    #endregion Spark Projectile
}