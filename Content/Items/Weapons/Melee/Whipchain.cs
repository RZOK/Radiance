using Radiance.Core.Systems;

namespace Radiance.Content.Items.Weapons.Melee
{
    #region Main Item

    public class Whipchain : ModItem
    {
        public bool reversed = false;
        public NPC lassoedNPC => Main.npc.FirstOrDefault(n => n.TryGetGlobalNPC<WhipchainNPC>(out _) && n.GetGlobalNPC<WhipchainNPC>().lassoed && n.GetGlobalNPC<WhipchainNPC>().lassoedPlayer == Main.LocalPlayer && n.active);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bladewhip");
            Tooltip.SetDefault("Right Click to throw out a lasso that forms a rope of light between you and an enemy, or yank an attached enemy towards you");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.width = 62;
            Item.height = 32;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.autoReuse = false;
            Item.rare = ItemRarityID.Green;
            Item.knockBack = 7.5f;
            Item.UseSound = SoundID.Item39;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.sellPrice(0, 0, 10);
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<WhipchainKnife>();
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player) => !Main.projectile.Any(n => n.active && (n.type == ModContent.ProjectileType<WhipchainKnife>() || n.type == ModContent.ProjectileType<WhipchainLasso>()));

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SyncPlayer>().mouseListener = true;
            player.GetModPlayer<SyncPlayer>().rightClickListener = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse != 2)
                reversed = !reversed;
            else if (lassoedNPC != null)
                if (CameraSystem.Quake < 10)
                    CameraSystem.Quake += 10;
            return base.UseItem(player);
        }

        public override float UseAnimationMultiplier(Player player)
        {
            if (player.altFunctionUse == 2 && lassoedNPC != null)
                return 0.01f;
            return base.UseAnimationMultiplier(player);
        }

        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2 && lassoedNPC != null)
                return 100f;
            return base.UseSpeedMultiplier(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                if (lassoedNPC != null)
                {
                    if (lassoedNPC.knockBackResist != 0)
                        lassoedNPC.velocity = Vector2.Normalize(player.Center - (Vector2.UnitY * 200) - lassoedNPC.Center) * (float)Math.Sqrt(Vector2.Distance(player.Center - (Vector2.UnitY * 200), lassoedNPC.Center)) / 2;
                    lassoedNPC.AddBuff(ModContent.BuffType<WhipchainExposed>(), 120);
                    lassoedNPC.GetGlobalNPC<WhipchainNPC>().lassoedVisual = false;
                    lassoedNPC.GetGlobalNPC<WhipchainNPC>().beamTimer = 0;
                    float dist = Vector2.Distance(lassoedNPC.Center, player.Center) / 8;
                    for (int i = 0; i < dist; i++)
                    {
                        int d = Dust.NewDust(Vector2.Lerp(lassoedNPC.Center, player.Center, i / dist), 1, 1, DustID.GoldFlame);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].scale = Main.rand.NextFloat(1.4f, 1.9f);
                        Main.dust[d].velocity = (Vector2.Normalize(player.Center - lassoedNPC.Center) * Vector2.Distance(lassoedNPC.Center, player.Center) / 32).RotatedByRandom(0.1f) * Main.rand.NextFloat(0.8f, 1.2f);
                        Main.dust[d].position += Main.rand.NextVector2Circular(8, 8);
                    }
                    type = 0;
                    NPC.HitInfo info = new NPC.HitInfo()
                    {
                        SourceDamage = damage /= 3,
                        HitDirection = player.Center.X > lassoedNPC.Center.X ? -1 : 1,
                        Crit = Main.rand.Next(100) > player.GetWeaponCrit(player.HeldItem)
                    };
                    lassoedNPC.StrikeNPC(info);
                    if (lassoedNPC != null)
                        lassoedNPC.GetGlobalNPC<WhipchainNPC>().lassoed = false;
                }
                else
                {
                    SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
                    knockback = 0;
                    damage /= 3;
                    velocity = (sPlayer.mouseWorld - Vector2.UnitY * 32 - player.MountedCenter).SafeNormalize(Vector2.UnitY) * 16;
                    type = ModContent.ProjectileType<WhipchainLasso>();
                }
            }
        }

        public override void UseAnimation(Player player)
        {
            Item.UseSound = SoundID.Item39;
            if (player.altFunctionUse == 2)
                Item.UseSound = lassoedNPC == null ? SoundID.Item1 : SoundID.Item153;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (lassoedNPC != null && player.altFunctionUse == 2)
                return false;

            int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (Main.projectile[p].ModProjectile is WhipchainKnife proj)
            {
                SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
                proj.flipSprite = reversed;
                proj.direction = sPlayer.mouseWorld.X > player.Center.X ? -1 : 1;
                if (reversed)
                {
                    proj.startRotation = (sPlayer.mouseWorld - player.Center).ToRotation() - 3f;
                    proj.targetRotation = (sPlayer.mouseWorld - player.Center).ToRotation() + 3f;
                }
                else
                {
                    proj.startRotation = (sPlayer.mouseWorld - player.Center).ToRotation() + 3f;
                    proj.targetRotation = (sPlayer.mouseWorld - player.Center).ToRotation() - 3f;
                }
            }
            return false;
        }
    }

    #endregion Main Item

    #region Knife Projectile

    public class WhipchainKnife : ModProjectile
    {
        public int distance = 35;
        public float duration => 40 / Owner.GetAttackSpeed<MeleeDamageClass>() * (Projectile.extraUpdates + 1);
        public float distanceMult = 4f;
        public ref float timer => ref Projectile.ai[0];
        public float startRotation = 0;
        public float rotation = 0;
        public float targetRotation = 0;
        public bool flipSprite = false;
        public bool madeSound = false;
        public int direction = 1;
        public float Completion => timer / duration;
        public float DistanceProgress => EaseInExponent(Completion >= 0.5f ? 2 - Completion * 2 : Completion * 2, 8) * distanceMult * Owner.GetAttackSpeed<MeleeDamageClass>();
        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Whipchain");
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            if (Owner.IsCCd())
                Projectile.active = false;
            if (Completion > 0.33f && !madeSound)
            {
                SoundEngine.PlaySound(SoundID.Item152, Projectile.Center);
                madeSound = true;
            }
            Projectile.direction = direction;
            timer++;
            rotation = Lerp(startRotation, targetRotation, EaseInOutExponent(Completion, 5));
            Projectile.rotation = rotation + PiOver2;
            Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, (rotation - PiOver2)) + (Vector2.UnitX * 24 + Vector2.UnitX * distance * DistanceProgress).RotatedBy(rotation);
            Projectile.spriteDirection = Projectile.direction;
            Projectile.scale = Math.Max(1, DistanceProgress / distanceMult + 0.4f);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (rotation * Owner.gravDir - PiOver2));
            Owner.itemTime = Owner.itemAnimation = 5;
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
            Owner.heldProj = Projectile.whoAmI;
            if (timer >= duration)
                Projectile.Kill();
        }

        public override void Kill(int timeLeft)
        {
            Owner.itemTime = Owner.itemAnimation = 0;
        }

        public override bool? CanDamage() => timer > duration * 0.33f && timer < duration * 0.66f;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D knifeTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = knifeTex.Size() / 2;
            Vector2 handPos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, (rotation - PiOver2)) - Vector2.UnitY.RotatedBy(rotation) * 4 * Owner.direction;
            float distBetweenPlayer = Vector2.Distance(Projectile.Center - Vector2.UnitX.RotatedBy(rotation) * (knifeTex.Height / 2 - 2), handPos);
            Texture2D ropeTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Weapons/Melee/WhipchainRope").Value;
            Texture2D handleTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Weapons/Melee/WhipchainHandle").Value;

            //Dust dust = Dust.NewDustPerfect(Projectile.Center - Vector2.UnitX * knifeTex.Height / 2, DustID.RedTorch);
            //dust.velocity *= 0;
            //dust.noGravity = true;

            //Dust dust2 = Dust.NewDustPerfect(handPos, DustID.RedTorch);
            //dust2.velocity *= 0;
            //dust2.noGravity = true;

            Rectangle drawRect = new Rectangle(0, 0, (int)distBetweenPlayer, ropeTex.Height);
            Main.spriteBatch.Draw(ropeTex, handPos - Main.screenPosition, drawRect, lightColor, Projectile.rotation + PiOver2, new Vector2(drawRect.Width, drawRect.Height / 2), 1, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(knifeTex, Projectile.Center - Main.screenPosition - Vector2.UnitY.RotatedBy(rotation) * (2 + 4 * -direction), null, lightColor, Projectile.rotation, origin, Projectile.scale, flipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(handleTex, handPos - Main.screenPosition, null, lightColor, Projectile.rotation + PiOver2, handleTex.Size() / 2, 1, SpriteEffects.None, 0f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Completion > 0.33f && Completion < 0.66f)
                {
                    Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Main.spriteBatch.Draw(knifeTex, Projectile.oldPos[k] - Main.screenPosition - Vector2.UnitY.RotatedBy(rotation) * (2 + 4 * -direction), null, color * 0.5f, Projectile.oldRot[k], origin, Projectile.scale, flipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            Texture2D knifeTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 handPos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, (rotation - PiOver2)) - Vector2.UnitY.RotatedBy(rotation) * 4 * Owner.direction;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + knifeTex.Width / 2 * Vector2.UnitX.RotatedBy(Projectile.rotation), handPos, 28, ref collisionPoint);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 dirVelocity = (Vector2.UnitX * 5).RotatedBy(Projectile.rotation * (flipSprite ? 1 : -1));
            int loop = Main.rand.Next(9, 14);
            if (target.HasBuff<WhipchainExposed>())
            {
                for (int i = 0; i < 20; i++)
                {
                    int d = Dust.NewDust(target.position, target.width, target.height, DustID.GoldFlame);
                    Main.dust[d].scale = Main.rand.NextFloat(1.3f, 1.7f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity = Main.rand.NextVector2Circular(5, 5);
                }
                if (CameraSystem.Quake < 8)
                    CameraSystem.Quake = 8;
                target.RequestBuffRemoval(ModContent.BuffType<WhipchainExposed>());
                loop = Main.rand.Next(14, 32);
            }
            else if (CameraSystem.Quake < 5)
                CameraSystem.Quake = 5;
            for (int i = 0; i < loop; i++)
            {
                int d = Dust.NewDust(target.position, target.width, target.height, DustID.Blood, dirVelocity.X, dirVelocity.Y);
                Main.dust[d].scale = Main.rand.NextFloat(1.1f, 1.5f);
            }
            SoundEngine.PlaySound(SoundID.Item98, Projectile.Center);
        }
    }

    #endregion Knife Projectile

    #region Lasso Projectile

    public class WhipchainLasso : ModProjectile
    {
        public ref float retractTimer => ref Projectile.ai[0];
        public ref float lassoTimer => ref Projectile.ai[1];
        public float scaleTimer = 0;
        public float targetScale = 10;
        public Player Owner => Main.player[Projectile.owner];
        public NPC lassoedNPC;
        public bool retract = false;
        public bool lassoed = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Whipchain");
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
        }

        public override void AI()
        {
            if (Owner.IsCCd())
                Projectile.active = false;
            if (scaleTimer < targetScale)
                scaleTimer++;
            Projectile.scale = Lerp(0, 1, EaseInOutCirc(scaleTimer / targetScale));
            Projectile.spriteDirection = Projectile.direction;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Owner.Center - Projectile.Center).ToRotation() * Owner.gravDir + PiOver2);
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 5;
            if (!lassoed)
            {
                if (!retract)
                {
                    if (Vector2.Distance(Projectile.Center, Owner.Center) > 600)
                        retract = true;
                    Projectile.velocity.Y += 0.15f;
                    Projectile.velocity.X *= 0.99f;
                    Projectile.rotation = Projectile.velocity.X / 25;
                }
                else
                {
                    retractTimer++;
                    float rotation = (Owner.Center - Projectile.Center).ToRotation();
                    Vector2 handPos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, rotation - PiOver2) - new Vector2(20, 2 * Owner.direction).RotatedBy(rotation);
                    Projectile.Center += Projectile.DirectionTo(handPos) * Math.Min(retractTimer + 3, Vector2.Distance(handPos, Projectile.Center));
                    if (Projectile.Center == handPos)
                        Projectile.Kill();
                }
            }
            if (lassoedNPC != null)
            {
                Projectile.rotation = Lerp(Projectile.rotation, 0, lassoTimer / 30);
                lassoTimer++;
                Projectile.Center += Projectile.DirectionTo(lassoedNPC.Center) * Math.Min(retractTimer * 2 + 3, Vector2.Distance(lassoedNPC.Center, Projectile.Center));
                if (lassoTimer >= 30)
                {
                    Projectile.velocity = Vector2.Normalize(lassoedNPC.Center - (Owner.Center + Vector2.UnitY * 128)) * 16;
                    lassoedNPC.GetGlobalNPC<WhipchainNPC>().lassoedVisual = true;
                    NPC.HitInfo info = new NPC.HitInfo()
                    {
                        SourceDamage = Projectile.damage,
                        Knockback = 10,
                        HitDirection = Owner.Center.X > lassoedNPC.Center.X ? -1 : 1,
                        Crit = Main.rand.Next(100) > Owner.GetWeaponCrit(Owner.HeldItem)
                    };
                    retract = true;
                    lassoed = false;
                    lassoedNPC = null;
                    retractTimer = 20;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            retract = true;
            return false;
        }

        public override bool? CanDamage() => !retract && !lassoed;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D lassoTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = lassoTex.Size() / 2;
            if (Owner != null)
            {
                float rotation = (Owner.Center - Projectile.Center).ToRotation();
                Vector2 handPos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, rotation - PiOver2) - new Vector2(20, 2 * Owner.direction).RotatedBy(rotation);
                Texture2D ropeTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Weapons/Melee/WhipchainLassoRope").Value;
                Texture2D handleTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Weapons/Melee/WhipchainHandle").Value;
                float distBetweenPlayer = Vector2.Distance(Projectile.Center, handPos);

                Rectangle drawRect = new Rectangle(0, 0, (int)distBetweenPlayer, ropeTex.Height);

                Main.spriteBatch.Draw(lassoTex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, Projectile.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                Main.spriteBatch.Draw(ropeTex, handPos - Main.screenPosition, drawRect, lightColor, (handPos - Projectile.Center).ToRotation(), new Vector2(drawRect.Width, drawRect.Height / 2), 1, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(handleTex, handPos - Main.screenPosition, null, lightColor, (handPos - Projectile.Center).ToRotation(), handleTex.Size() / 2, 1, SpriteEffects.None, 0f);
            }
            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.3f;
                Main.spriteBatch.Draw(lassoTex, Projectile.oldPos[k] - Main.screenPosition, null, color, Projectile.oldRot[k], Vector2.Zero, Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            Owner.itemTime = Owner.itemAnimation = 5;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.CanBeChasedBy(this))
            {
                SoundEngine.PlaySound(SoundID.Item156, Projectile.Center);
                lassoed = true;
                lassoedNPC = target;
                target.GetGlobalNPC<WhipchainNPC>().lassoed = true;
                target.GetGlobalNPC<WhipchainNPC>().lassoedPlayer = Owner;
            }
            else
                retract = true;
            Projectile.velocity *= 0.1f;
        }
    }

    #endregion Lasso Projectile

    #region GlobalNPC

    public class WhipchainNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool lassoed = false;
        public bool lassoedVisual = false;
        public Player lassoedPlayer;
        public float beamTimer = 0;
        public float beamTimerMax = 10;

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.HasBuff<WhipchainExposed>())
            {
                Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/Items/Weapons/Melee/WhipchainExposed").Value;
                spriteBatch.Draw(tex, npc.Center - Main.screenPosition - Vector2.UnitY * (8 + tex.Height / 2 + npc.height / 2), null, CommonColors.RadianceColor1, 0, tex.Size() / 2, 0.8f, SpriteEffects.None, 0);
            }
            if (lassoedVisual && lassoedPlayer != null)
            {
                float beamLerp = EaseInCirc(beamTimer / beamTimerMax);

                RadianceDrawing.DrawSoftGlow(npc.Center, CommonColors.RadianceColor1 * beamLerp, 0.2f);
                RadianceDrawing.DrawSoftGlow(npc.Center, Color.White * beamLerp, 0.15f);

                RadianceDrawing.DrawBeam(npc.Center, lassoedPlayer.Center, CommonColors.RadianceColor1 * beamLerp, 8);
                RadianceDrawing.DrawBeam(npc.Center, lassoedPlayer.Center, Color.White * 0.5f * beamLerp, 6);

            }
        }

        public override bool PreAI(NPC npc)
        {
            if (lassoedPlayer != null)
            {
                if (lassoedVisual && beamTimer < beamTimerMax)
                    beamTimer++;
                if (Vector2.Distance(lassoedPlayer.Center, npc.Center) > 600)
                {
                    float dist = Vector2.Distance(npc.Center, lassoedPlayer.Center) / 8;
                    for (int i = 0; i < dist; i++)
                    {
                        int d = Dust.NewDust(Vector2.Lerp(npc.Center, lassoedPlayer.Center, i / dist), 1, 1, DustID.GoldFlame);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].scale = Main.rand.NextFloat(1.2f, 1.7f);
                        Main.dust[d].velocity = Main.rand.NextVector2Circular(3, 3);
                        Main.dust[d].position += Main.rand.NextVector2Circular(8, 8);
                    }
                    lassoedPlayer = null;
                    lassoed = false;
                    lassoedVisual = false;
                    beamTimer = 0;
                }
            }
            if (beamTimer == beamTimerMax - 1)
                SoundEngine.PlaySound(SoundID.Item20, npc.Center);
            return base.PreAI(npc);
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (lassoed && Main.rand.NextBool(4))
            {
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GoldFlame, 0, 0);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity.X *= 0f;
                Main.dust[d].velocity.Y = -Math.Abs(Main.dust[d].velocity.Y);
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            WhipchainKnife wck = projectile.ModProjectile as WhipchainKnife;
            if (wck != null)
            {
                modifiers.HitDirectionOverride = -wck.direction; //knockback fix
                if (npc.HasBuff<WhipchainExposed>())
                {
                    if (!Main.player[projectile.owner].kbGlove)
                        modifiers.Knockback *= 1.5f;
                    modifiers.SetCrit();
                }
            }
        }
    }

    #endregion GlobalNPC

    #region Exposed Buff

    public class WhipchainExposed : BaseBuff
    {
        public WhipchainExposed() : base("Exposed", "The next recieved Whipchain hit is a critical strike", true) { }
    }

    #endregion Exposed Buff
}