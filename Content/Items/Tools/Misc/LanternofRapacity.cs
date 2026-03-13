using Radiance.Content.Items.ProjectorLenses;
using Radiance.Core.Systems;
using static Terraria.Player;

namespace Radiance.Content.Items.Tools.Misc
{
    public class LanternofRapacity : ModItem, IInstrument
    {
        private const int RECALL_MAX_DISTANCE = 160;
        public static float RadianceConsumed => 0.0005f;
        public Vector2 HeldLanternPosition { get; set; }

        public LanternofRapacity_Thrown ThrownLantern
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
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.holdStyle = 16;
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SyncPlayer>().mouseListener = true;
            if (!Main.projectile.Any(x => x.type == ModContent.ProjectileType<LanternofRapacity_Thrown>() && x.active && x.owner == player.whoAmI) && player.HasRadiance(RadianceConsumed))
                ThrownLantern = (LanternofRapacity_Thrown)Main.projectile[Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<LanternofRapacity_Thrown>(), 0, 0, player.whoAmI)].ModProjectile;

            if (!player.HasRadiance(RadianceConsumed) && ThrownLantern != null)
            {
                ThrownLantern.Projectile.active = false;
                ThrownLantern = null;
            }
            if (ThrownLantern is not null && ThrownLantern.held)
                ThrownLantern.Projectile.timeLeft = 2;

            Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -2f * player.gravDir);
            Vector2 lanternPosition = itemPosition + new Vector2(10 * player.direction, 12 + player.gfxOffY).RotatedBy(player.itemRotation + player.fullRotation);
            int frame = player.bodyFrame.Y / player.bodyFrame.Height;
            if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                lanternPosition -= Vector2.UnitY * 2f;

            HeldLanternPosition = lanternPosition;
            if (ThrownLantern != null && ThrownLantern.held)
            {
                ThrownLantern.Projectile.Center = lanternPosition;
                ThrownLantern.Projectile.rotation = player.itemRotation + player.fullRotation;
                player.heldProj = ThrownLantern.Projectile.whoAmI;
            }

            player.itemLocation = lanternPosition;
        }

        public override void UpdateInventory(Player player)
        {
        }

        public override bool? UseItem(Player player)
        {
            if (player.ItemAnimationJustStarted)
            {
                if (ThrownLantern != null)
                {
                    if (ThrownLantern.held)
                    {
                        SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
                        Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -2f * player.gravDir);
                        float itemRotation = (sPlayer.mouseWorld - itemPosition).ToRotation();
                        for (int i = 0; i < 8; i++)
                        {
                            int d = Dust.NewDust(HeldLanternPosition - new Vector2(4, 4), 1, 1, DustID.GoldFlame);
                            Main.dust[d].velocity = itemRotation.ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(1, 8);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].scale = 1.3f;
                        }

                        if (Collision.SolidTiles(ThrownLantern.Projectile.position, ThrownLantern.Projectile.width, ThrownLantern.Projectile.height)) // if it would be thrown into tiles push it back towards the player
                            ThrownLantern.Projectile.Center = Vector2.Lerp(player.Center, ThrownLantern.Projectile.Center, 0.4f);

                        ThrownLantern.Projectile.velocity = itemRotation.ToRotationVector2() * 8;
                        ThrownLantern.held = false;
                        ThrownLantern.Projectile.timeLeft = 3600;
                        SoundEngine.PlaySound(SoundID.Item1, player.Center);
                    }
                    else
                    {
                        if (ThrownLantern.Projectile.Distance(HeldLanternPosition) < RECALL_MAX_DISTANCE)
                        {
                            ThrownLantern.returningTimer = 0;
                            ThrownLantern.returningTimeMax = (int)Vector2.Distance(ThrownLantern.Projectile.Center, HeldLanternPosition) / 12;

                            ThrownLantern.Projectile.velocity = Vector2.Zero;
                            ThrownLantern.returningStartPos = ThrownLantern.Projectile.Center;
                            ThrownLantern.returning = true;
                        }
                        else
                        {
                            //not close enough to recall
                        }
                    }
                }
            }
            return base.UseItem(player);
        }

        public void SetItemInHand(Player player)
        {
            SyncPlayer sPlayer = player.GetModPlayer<SyncPlayer>();
            if (sPlayer.mouseWorld.X > player.Center.X)
                player.ChangeDir(1);
            else
                player.ChangeDir(-1);

            Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -2f * player.gravDir);
            float itemRotation = (sPlayer.mouseWorld - itemPosition).ToRotation();

            if (ThrownLantern is not null && (ThrownLantern.held || ThrownLantern.returning))
                player.SetCompositeArmFront(true, CompositeArmStretchAmount.ThreeQuarters, itemRotation * player.gravDir - PiOver2);

            HoldStyleAdjustments(player, itemRotation, itemPosition, true);
        }

        public void HoldStyleAdjustments(Player player, float desiredRotation, Vector2 desiredPosition, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
        {
            if (noSandstorm)
                player.sandStorm = false;

            player.itemRotation = desiredRotation;

            if (flipAngle)
                player.itemRotation *= player.direction;
            else if (player.direction < 0)
                player.itemRotation += Pi;

            Vector2 lanternPosition = desiredPosition + new Vector2(10 * player.direction, 12 + player.gfxOffY).RotatedBy(player.itemRotation + player.fullRotation);
            if (stepDisplace)
            {
                int frame = player.bodyFrame.Y / player.bodyFrame.Height;
                if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                    lanternPosition -= Vector2.UnitY * 2f;
            }
            HeldLanternPosition = lanternPosition;
            if (ThrownLantern != null && ThrownLantern.held)
            {
                ThrownLantern.Projectile.Center = lanternPosition;
                ThrownLantern.Projectile.rotation = player.itemRotation + player.fullRotation;
                player.heldProj = ThrownLantern.Projectile.whoAmI;
            }

            player.itemLocation = lanternPosition;
        }

        public override void HoldStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player);

        public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player);

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ShimmeringGlass>(7)
                .AddTile(TileID.Anvils)
                .AddCondition(Condition.NearLava)
                .Register();
        }
    }

    public class LanternofRapacity_Thrown : ModProjectile
    {
        public bool held = true;
        public bool returning = false;
        public Vector2 returningStartPos = Vector2.Zero;
        public float returningStartRotation = 0f;

        internal int returningTimer = 0;
        internal int returningTimeMax = 0;

        public static SoundStyle popSound = new SoundStyle($"{nameof(Radiance)}/Sounds/LensPop") { Volume = 0.65f };
        public override string Texture => $"{nameof(Radiance)}/Content/Items/Tools/Misc/LanternofRapacity_Held";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lantern of Rapacity");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.netImportant = true;
            Projectile.scale = 0.9f;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.GetModPlayer<RadiancePlayer>().ConsumeRadiance(LanternofRapacity.RadianceConsumed) || player.dead || !player.active)
                Projectile.Kill();

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

            if (!held && !returning)
                AI_Detached();
            else
                AI_Attached();
        }

        private void AI_Attached()
        {
            Player player = Main.player[Projectile.owner];
            LanternofRapacity lanternItem = Main.LocalPlayer == player ? GetPlayerHeldItem().ModItem as LanternofRapacity : null;
            if (lanternItem is not null)
            {
                if (returning)
                {
                    int c = Dust.NewDust(Projectile.position, 24, 24, DustID.GoldFlame, Projectile.velocity.X, Projectile.velocity.Y);
                    Main.dust[c].velocity *= 0.5f;
                    Main.dust[c].noGravity = true;
                    Main.dust[c].scale = 1f;

                    float progress = returningTimer / (float)returningTimeMax;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.rotation += 0.3f;
                    Projectile.position = Vector2.Lerp(returningStartPos, lanternItem.HeldLanternPosition, progress);
                    if (returningTimer >= returningTimeMax)
                    {
                        SoundEngine.PlaySound(popSound, lanternItem.HeldLanternPosition);
                        held = true;
                        returning = false;
                        returningTimer = 0;
                        returningTimeMax = 0;
                    }
                    returningTimer++;
                }
                Projectile.tileCollide = false;
            }
            else
            {
                player.GetModPlayer<LanternofRapacity_Player>().ActiveLantern = null;
                Projectile.active = false;
            }
        }

        private void AI_Detached()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.lavaWet || player.Distance(Projectile.Center) > 2000)
                Projectile.Kill();

            Projectile.tileCollide = true;
            Projectile.rotation += Projectile.velocity.X / 10;

            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 16)
                Projectile.velocity.Y = 16;

            Projectile.velocity.X *= 0.97f;
            if (Math.Abs(Projectile.velocity.X) <= 0.1f)
            {
                Projectile.netUpdate = true;
                Projectile.velocity.X = 0;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            player.GetModPlayer<LanternofRapacity_Player>().ActiveLantern = null;
            for (int i = 0; i < 32; i++)
            {
                int d = Dust.NewDust(Projectile.position, 24, 24, DustID.GoldFlame);
                Main.dust[d].velocity.X = 0;
                Main.dust[d].velocity.Y = Main.rand.NextFloat(0, -3) * Main.rand.NextFloat(1, 2.5f);
                Main.dust[d].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!held)
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
            RadianceDrawing.DrawSoftGlow(Projectile.Center, CommonColors.RadianceColor1 * 0.5f, 0.4f);
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, ModContent.Request<Texture2D>(Texture).Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class LanternofRapacity_Player : ModPlayer
    {
        public LanternofRapacity_Thrown ActiveLantern { get; set; }
    }
}