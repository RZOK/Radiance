using Radiance.Content.Items.ProjectorLenses;
using Radiance.Core.Systems;
using static Terraria.Player;

namespace Radiance.Content.Items.Tools.Misc
{
    public class LanternofRapacity : ModItem, IInstrument
    {
        internal const int RECALL_MAX_DISTANCE = 160;
        public static float RadianceConsumed => 0.0014f;

        public LanternofRapacity_Held ThrownLantern
        {
            get => Main.player[Item.playerIndexTheItemIsReservedFor].GetModPlayer<LanternofRapacity_Player>().ActiveLantern;
            set => Main.player[Item.playerIndexTheItemIsReservedFor].GetModPlayer<LanternofRapacity_Player>().ActiveLantern = value;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lantern of Rapacity");
            Tooltip.SetDefault("Provides a great amount of light and reveals treasures when held\nLeft click to toss the lantern, or recall it back if already thrown");
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.Glowsticks[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.noUseGraphic = true;
        }
        public override bool CanUseItem(Player player)
        {
            return player == Main.LocalPlayer && !player.sleeping.isSleeping;
        }
        public override void HoldItem(Player player)
        {
            SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
            sPlayer.mouseListener = true;
            if (!Main.projectile.Any(x => x.type == ModContent.ProjectileType<LanternofRapacity_Held>() && x.active && x.owner == player.whoAmI))
                ThrownLantern = (LanternofRapacity_Held)Main.projectile[Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<LanternofRapacity_Held>(), 0, 0, player.whoAmI)].ModProjectile;

            if (ThrownLantern is not null && ThrownLantern.currentState == LanternofRapacity_Held.AIState.Held)
                ThrownLantern.Projectile.timeLeft = 2;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Tools/Misc/LanternofRapacity_Glow").Value;
            Main.EntitySpriteDraw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }

        public override bool? UseItem(Player player)
        {
            if (player.ItemAnimationJustStarted)
            {
                if (ThrownLantern != null)
                {
                    if (ThrownLantern.currentState == LanternofRapacity_Held.AIState.Held)
                    {
                        SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
                        Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -2f * player.gravDir);
                        float itemRotation = (sPlayer.mouseWorld - itemPosition).ToRotation();
                        for (int i = 0; i < 8; i++)
                        {
                            int d = Dust.NewDust(player.RotatedRelativePoint(player.MountedCenter) - new Vector2(4, 4), 1, 1, DustID.GoldFlame);
                            Main.dust[d].velocity = itemRotation.ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(1, 8);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].scale = 1.3f;
                        }

                        if (Collision.SolidTiles(ThrownLantern.Projectile.position, ThrownLantern.Projectile.width, ThrownLantern.Projectile.height)) // if it would be thrown into tiles push it back towards the player
                            ThrownLantern.Projectile.Center = Vector2.Lerp(player.Center, ThrownLantern.Projectile.Center, 0.4f);

                        ThrownLantern.Projectile.velocity = itemRotation.ToRotationVector2() * 8;
                        ThrownLantern.Projectile.timeLeft = 3600;
                        ThrownLantern.armThrowTimer = -2;
                        ThrownLantern.currentState = LanternofRapacity_Held.AIState.Thrown;
                        SoundEngine.PlaySound(SoundID.Item1, player.Center);
                    }
                    else if (ThrownLantern.currentState == LanternofRapacity_Held.AIState.Thrown)
                    {
                        if (ThrownLantern.Projectile.Distance(player.MountedCenter) < RECALL_MAX_DISTANCE)
                        {
                            ThrownLantern.returningTimer = 0;
                            ThrownLantern.returningTimeMax = (int)Vector2.Distance(ThrownLantern.Projectile.Center, player.MountedCenter) / 12;

                            ThrownLantern.Projectile.velocity = Vector2.Zero;
                            ThrownLantern.returningStartPos = ThrownLantern.Projectile.Center;
                            ThrownLantern.currentState = LanternofRapacity_Held.AIState.Returning;
                        }
                    }
                }
            }
            return base.UseItem(player);
        }
    }

    public class LanternofRapacity_Held : ModProjectile
    {
        internal bool lightActive = true;
        private const float ROTATION_OFFSET = 0.4f;

        internal int armThrowTimer = 0;
        internal const int ARM_THROW_TIMER_MAX = 8;

        internal Vector2 returningStartPos = Vector2.Zero;
        internal int returningTimer = 0;
        internal int returningTimeMax = 0;

        private Player player;
        private SyncPlayer syncPlayer;

        private static SoundStyle popSound = new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop");

        private const int PLAYER_MAX_TILE_DISTANCE = 128;

        public enum AIState
        {
            Held,
            Thrown,
            Returning
        }

        public AIState currentState = AIState.Held;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lantern of Rapacity");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.netImportant = true;
            Projectile.scale = 0.85f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            player = Main.player[Projectile.owner];
            syncPlayer = player.GetModPlayer<SyncPlayer>();
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            if (player.sleeping.isSleeping)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead || !player.active)
                Projectile.Kill();


            if (player.HasRadiance(LanternofRapacity.RadianceConsumed))
            {
                player.ConsumeRadiance(LanternofRapacity.RadianceConsumed);
                Projectile.ai[0] += 1f;
                if (Projectile.ai[0] >= 10)
                {
                    Projectile.ai[0] = 0f;
                    int tileDistance = 30;

                    if (Vector2.Distance(Projectile.Center, Main.LocalPlayer.Center) < Main.screenWidth + tileDistance * 16)
                        ChestSpelunkerHelper.Instance.AddSpotToCheck(Projectile.Center);
                }
                float strength = 3;
                Lighting.AddLight(Projectile.Center, 1 * strength, 1 * strength, 0.5f * strength);

                lightActive = true;
            }

            switch (currentState)
            {
                case AIState.Held:
                    AI_Held();
                    break;

                case AIState.Thrown:
                    AI_Thrown();
                    break;

                case AIState.Returning:
                    AI_Returning();
                    break;
            }
        }

        private void AI_Held()
        {
            SetPlayerArm();
            Projectile.tileCollide = false;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = player.itemRotation + player.fullRotation - ROTATION_OFFSET * -player.direction * player.gravDir;

            LanternofRapacity lanternItem = Main.LocalPlayer == player ? GetPlayerHeldItem().ModItem as LanternofRapacity : null;
            if (lanternItem is null || Main.LocalPlayer.sleeping.isSleeping)
            {
                player.GetModPlayer<LanternofRapacity_Player>().ActiveLantern = null;
                Projectile.active = false;
                return;
            }

            Projectile.Center = LanternPositionInHand();
        }

        private void AI_Thrown()
        {
            Projectile.tileCollide = true;
            Projectile.rotation += Projectile.velocity.X / 10;

            if (Projectile.lavaWet || player.Distance(Projectile.Center) > PLAYER_MAX_TILE_DISTANCE * 16)
                Projectile.Kill();

            Projectile.velocity.X *= 0.97f;
            if (Math.Abs(Projectile.velocity.X) <= 0.1f)
            {
                Projectile.netUpdate = true;
                Projectile.velocity.X = 0;
            }

            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 16)
                Projectile.velocity.Y = 16;

            if (armThrowTimer < ARM_THROW_TIMER_MAX)
            {
                LanternofRapacity lanternItem = Main.LocalPlayer == player ? GetPlayerHeldItem().ModItem as LanternofRapacity : null;
                if (lanternItem is not null)
                    SetPlayerArm();

                armThrowTimer++;
            }
        }

        private void AI_Returning()
        {
            armThrowTimer = 0;
            SetPlayerArm();
            Projectile.tileCollide = false;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation += 0.3f;

            LanternofRapacity lanternItem = Main.LocalPlayer == player ? GetPlayerHeldItem().ModItem as LanternofRapacity : null;

            float progress = returningTimer / (float)returningTimeMax;
            Vector2 lanternPosition = LanternPositionInHand();
            Projectile.position = Vector2.Lerp(returningStartPos, lanternPosition, progress);
            if (returningTimer >= returningTimeMax)
            {
                currentState = AIState.Held;
                SoundEngine.PlaySound(popSound with { Volume = 0.3f }, lanternPosition);
                returningTimer = 0;
                returningTimeMax = 0;
            }

            int c = Dust.NewDust(Projectile.position, 24, 24, DustID.GoldFlame, Projectile.velocity.X, Projectile.velocity.Y);
            Main.dust[c].velocity *= 0.5f;
            Main.dust[c].noGravity = true;
            Main.dust[c].fadeIn = 1.1f;

            returningTimer++;
        }

        internal Vector2 LanternPositionInHand()
        {
            float itemRotation = GetAndSetItemRotation();
            Vector2 initialOffset = new Vector2(-2f * player.direction, -2f * player.gravDir);
            if (player.gravDir == -1)
                initialOffset.X -= 2f * player.direction;

            Vector2 itemPosition = player.GetBackHandPositionGravComplying(CompositeArmStretchAmount.Full, itemRotation - PiOver2) + initialOffset;

            Vector2 lanternPosition = itemPosition + new Vector2(-4f * player.direction * player.gravDir, 16f + player.gfxOffY).RotatedBy(player.itemRotation + player.fullRotation);
            int frame = player.bodyFrame.Y / player.bodyFrame.Height;
            if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                lanternPosition -= Vector2.UnitY * 2f * player.gravDir;

            return lanternPosition;
        }

        private void SetPlayerArm()
        {
                if (syncPlayer.mouseWorld.X > player.Center.X)
                    player.ChangeDir(1);
                else
                    player.ChangeDir(-1);
            
            float itemRotation = GetAndSetItemRotation();

            player.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, itemRotation - PiOver2);
        }

        private float GetAndSetItemRotation()
        {
            float itemRotation = (syncPlayer.mouseWorld - player.RotatedRelativePoint(player.MountedCenter)).ToRotation() * player.gravDir - ROTATION_OFFSET * player.direction * Lerp(-4f, 1f, 1f - MathF.Pow(armThrowTimer / (float)ARM_THROW_TIMER_MAX, 0.3f));
            player.itemRotation = itemRotation * player.gravDir;
            if (player.direction < 0)
                player.itemRotation += Pi;
            if (player.gravDir == -1)
                player.itemRotation += Pi;

            return itemRotation;
        }

        public override void OnKill(int timeLeft)
        {
            player.GetModPlayer<LanternofRapacity_Player>().ActiveLantern = null;
            for (int i = 0; i < 16; i++)
            {
                int d = Dust.NewDust(Projectile.position, 24, 24, DustID.GoldFlame);
                Main.dust[d].velocity.X = 0;
                Main.dust[d].velocity.Y = Main.rand.NextFloat(0, -3) * Main.rand.NextFloat(1, 2.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = 1.2f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (currentState == AIState.Thrown)
            {
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X / 2;
                    Projectile.velocity.Y *= 0.2f;
                }
                if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0.5f)
                {
                    Projectile.velocity.Y = -oldVelocity.Y / 3;
                    Projectile.velocity.X *= 0.7f;
                }
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int yStart = 0;
            if (!lightActive)
                yStart = 36;
            else
                RadianceDrawing.DrawSoftGlow(Projectile.Center, CommonColors.RadianceColor1 * 0.5f, 0.4f);

            Rectangle frame = new Rectangle(0, yStart, 22, 34);
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            if (currentState == AIState.Thrown && Main.LocalPlayer.whoAmI == Projectile.owner && Main.LocalPlayer.PlayerHeldItem().type == ModContent.ItemType<LanternofRapacity>() && Main.LocalPlayer.MountedCenter.Distance(Projectile.Center) < LanternofRapacity.RECALL_MAX_DISTANCE)
            {
                Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Tools/Misc/LanternofRapacity_HeldOutline").Value;
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Main.OurFavoriteColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }

    public class LanternofRapacity_Player : ModPlayer
    {
        public LanternofRapacity_Held ActiveLantern { get; set; }
    }
}