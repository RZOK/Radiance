using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Radiance.Core.Systems.ParticleSystems;

namespace Radiance.Content.Items.Accessories
{
    public class StarslingHarness : BaseAccessory
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star-Sling Harness");
            Tooltip.SetDefault("Tap Down to quickly smash into the ground\nJumping immediately upon hitting the ground will launch you in the direction of your cursor");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(0, 1, 0);
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }
        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            StarslingHarnessPlayer starslingHarnessPlayer = player.GetModPlayer<StarslingHarnessPlayer>();
            if (player.velocity.Y != 0)
            {
                if(player.controlDown && !player.mount.Active && !starslingHarnessPlayer.groundPound)
                {
                    player.GetModPlayer<StarslingHarnessPlayer>().fallingTime = 0;
                    player.velocity.Y += 1f;
                    starslingHarnessPlayer.groundPound = true;
                    starslingHarnessPlayer.hasBeenHitInGroundPound = false;
                }
            }
            
        }
    }
    public class StarslingHarnessPlayer : ModPlayer
    {
        public bool groundPound = false;
        public bool hasBeenHitInGroundPound = false;
        public int fallingTime = 0;
        public int superJumpTimer = 0;
        public static readonly float STARSLINGHARNESS_SPEEDLINE_VELOCITY_THRESHOLD = 4;
        public static readonly float STARSLINGHARNESS_MINIMUM_FALLINGTIME_FOR_EFFECTS = 15;
        public static readonly float STARSLINGHARNESS_MINIMUM_FALLINGTIME_FOR_IMMUNITY_FRAMES = 15;
        public override void Load()
        {
            On_Player.JumpMovement += PreventJumping;
        }

        private void PreventJumping(On_Player.orig_JumpMovement orig, Player self)
        {
            if (self.GetModPlayer<StarslingHarnessPlayer>().groundPound)
                return;

            if (self.GetModPlayer<StarslingHarnessPlayer>().superJumpTimer > 0 && self.controlJump)
            {
                self.velocity = Vector2.Normalize(Main.MouseWorld - self.MountedCenter) * MathF.Min(self.GetModPlayer<StarslingHarnessPlayer>().fallingTime / 1.5f, 32f);
                self.GetModPlayer<StarslingHarnessPlayer>().superJumpTimer = 0;
                return;
            }
            orig(self);
        }

        public override void UpdateDead()
        {
            groundPound = false;
            hasBeenHitInGroundPound = false;
            fallingTime = 0;
            superJumpTimer = 0;
        }
        public override void PreUpdateMovement()
        {
            if (Player.Equipped<StarslingHarness>() && groundPound)
            {
                Player.velocity.Y += 0.35f;
                if (Player.velocity.Y > 0)
                    Player.velocity.Y += Player.velocity.Y / 20f;

                if (Player.velocity.Y > Player.maxFallSpeed)
                    Player.velocity.Y = Player.maxFallSpeed;

                if (Math.Abs(Player.velocity.X) > 1f)
                    Player.velocity.X *= 0.97f;
            }
        }

        public override void UpdateEquips()
        {
            if (Player.Equipped<StarslingHarness>() && !Player.mount.Active)
            {
                if (groundPound)
                {
                    Player.noFallDmg = true;
                    Player.maxFallSpeed = 24f;
                    fallingTime++;

                    if (Player.velocity.Y >= 0 && fallingTime > STARSLINGHARNESS_MINIMUM_FALLINGTIME_FOR_EFFECTS)
                    {
                        int amounts = (int)Player.velocity.Y / 7;
                        for (int i = 0; i < amounts; i++)
                        {
                            Vector2 dustPosition = Player.Bottom + new Vector2(Main.rand.NextFloat(Player.width / -2f, Player.width / 2f), Player.velocity.Y * 2f / amounts * i);
                            Vector2 dustVelocity = new Vector2(Main.rand.NextFloat(3, Math.Max(5, Player.velocity.Y) / 3f), -Player.velocity.Y / 2f);
                            if (Main.rand.NextBool())
                                dustVelocity.X *= -1f;

                            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.GoldFlame, dustVelocity);
                            dust.noGravity = true;
                            dust.scale = 1.3f;
                            dust.fadeIn = 1.3f;
                        }
                        if (Main.GameUpdateCount % 5 == 0 && Player.velocity.Y > STARSLINGHARNESS_SPEEDLINE_VELOCITY_THRESHOLD)
                            WorldParticleSystem.system.AddParticle(new SpeedLine(Player.position + new Vector2(Main.rand.Next(Player.width), Main.rand.Next(Player.height)) + Player.velocity / 2f, Vector2.UnitY * Player.velocity.Y, 15, new Color(255, 202, 122) * 0.8f, MathHelper.PiOver2, Player.velocity.Y * 14f, 0.9f));
                    }

                    if (Player.velocity.Y == 0)
                    {
                        if (fallingTime > STARSLINGHARNESS_MINIMUM_FALLINGTIME_FOR_EFFECTS)
                        {
                            int maxDistance = 240;
                            int distance = (int)Max(90, Min(fallingTime * 3f, maxDistance));
                            Vector2 smashOrigin = Player.Bottom;
                            SmashTilesInDirection(smashOrigin, distance, 1);
                            SmashTilesInDirection(smashOrigin, distance, -1);

                            CameraSystem.Quake += Max(0, distance - 90f) / 5f;

                            SoundStyle sound = SoundID.DD2_MonkStaffGroundImpact;
                            sound.Volume = 1.7f;
                            sound.Pitch = Lerp(0, -0.5f, fallingTime / (float)maxDistance);
                            SoundEngine.PlaySound(sound, Player.Center);

                            Item equippedHarness = Player.armor.Where(x => x.type == ModContent.ItemType<StarslingHarness>()).First();
                            Projectile projectile = Main.projectile[Projectile.NewProjectile(Player.GetSource_Accessory(equippedHarness), smashOrigin, Vector2.Zero, ModContent.ProjectileType<StarslingHarnessSmash>(), distance / 4, 0f, Player.whoAmI, distance)];
                            superJumpTimer = 10;
                        }
                        hasBeenHitInGroundPound = false;
                        groundPound = false;
                    }
                }
            }
            else
            {
                groundPound = false;
                superJumpTimer = 0;
            }
            if(superJumpTimer > 0)
                superJumpTimer--;
        }
        private static void SmashTilesInDirection(Vector2 origin, int amount, int direction)
        {
            float ratio = 6;
            int dustSpawnTileY = (int)origin.Y / 16;
            for (int h = 0; h < amount / ratio; h++)
            {
                float intensity = MathF.Pow(1f - h / (amount / (float)ratio), 0.5f);
                float dustX = origin.X + h * ratio * direction;
                Point tileCoords = new Point((int)dustX / 16, dustSpawnTileY);
                if (GetHighestTileYAtCoordinates(tileCoords.X, tileCoords.Y, out dustSpawnTileY))
                {
                    Tile spawnTile = Framing.GetTileSafely(tileCoords.X, dustSpawnTileY);
                    Vector2 dustPosition = new Vector2(dustX, dustSpawnTileY * 16f);

                    int tileDust = WorldGen.KillTile_MakeTileDust(tileCoords.X, dustSpawnTileY, spawnTile);
                    Dust dust = Main.dust[tileDust];
                    dust.active = false; //this is the only way i can get the dust type from a tile

                    float dustYModifier = 1f + amount / 120f;
                    float dustYVelocity = -5 * dustYModifier * intensity * MathF.Pow(Main.rand.NextFloat(), 1.5f) - 0.5f;
                    WorldParticleSystem.system.AddParticle(new FadeDust(dustPosition + Utils.RandomVector2(Main.rand, -4, 4), new Vector2(dust.velocity.X * 0.6f, dustYVelocity), Main.rand.Next(15, 45), dust.type, dust.frame, dust.color));
                    if(Main.rand.NextBool(2))
                    {
                        Dust smoke = Dust.NewDustPerfect(dustPosition, DustID.Smoke, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2.5f, -0.5f)));
                        smoke.fadeIn = Main.rand.NextFloat(1f, 1.5f);
                        smoke.scale = Main.rand.NextFloat(1f, 1.5f);
                    }
                }
            }
        }
        private static bool GetHighestTileYAtCoordinates(int i, int j, out int y)
        {
            y = j - 2;
            Tile tile = Framing.GetTileSafely(i, y);
            Tile aboveTile = Framing.GetTileSafely(i, y - 1);
            while (!tile.HasUnactuatedTile || !(Main.tileSolidTop[tile.TileType] || Main.tileSolid[tile.TileType]) || (Main.tileSolid[aboveTile.TileType] && aboveTile.HasUnactuatedTile))
            {
                y += 1;
                tile = Framing.GetTileSafely(i, y);
                aboveTile = Framing.GetTileSafely(i, y - 1);
                if (y > j + 3)
                    return false;
            }
            return true;
        }
        public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
        {
            if (Player.Equipped<StarslingHarness>() && groundPound && fallingTime > STARSLINGHARNESS_MINIMUM_FALLINGTIME_FOR_IMMUNITY_FRAMES && !hasBeenHitInGroundPound && dodgeable)
            {
                Player.immune = true;
                Player.immuneTime = 20;
                hasBeenHitInGroundPound = true;
                return true;
            }
            return false;
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.Equipped<StarslingHarness>() && groundPound)
                groundPound = false;
        }
    }
    public class StarslingHarnessSmash : ModProjectile
    {
        public override string Texture => "Radiance/Content/ExtraTextures/Blank";
        public override void SetStaticDefaults() => DisplayName.SetDefault("Starsling Harness");
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.knockBackResist != 0f)
                target.velocity.Y -= Projectile.ai[0] / 10f * target.knockBackResist;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => AABBvCircle(targetHitbox, Projectile.Center, Projectile.ai[0]);
    }
    public class StarslingHarnessFlareLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.JimsCloak);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<StarslingHarnessPlayer>().groundPound;
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Texture2D texture = TextureAssets.Extra[91].Value;
            float fallingTime = drawInfo.drawPlayer.GetModPlayer<StarslingHarnessPlayer>().fallingTime;
            float scale = 1.3f + Clamp(fallingTime / 300f, 0f, 0.5f);
            float alpha = Clamp(fallingTime / 240f, 0, 0.3f);
            Color color = Color.Lerp(new Color(255, 198, 112), new Color(255, 141, 112), Clamp(fallingTime / 240f, 0, 1)) * alpha;
            if (drawInfo.drawPlayer.velocity.Y < StarslingHarnessPlayer.STARSLINGHARNESS_SPEEDLINE_VELOCITY_THRESHOLD)
                alpha = 0;

            Main.spriteBatch.Draw(texture, drawInfo.Center - Main.screenPosition + Utils.RandomVector2(Main.rand, -4, 4), null, color, Pi, new Vector2(texture.Width / 2f, 32 / scale), scale, SpriteEffects.None, 0);
        }
    }
}