using Mono.Cecil.Cil;
using MonoMod.Cil;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using Steamworks;
using System.Runtime.InteropServices.Marshalling;

namespace Radiance.Content.Items.Accessories
{
    public class Pathfinders : BaseAccessory, IModPlayerTimer
    {
        private const int TICKS_TO_SUPER_SPRINT = 180;
        public int timerCount => 1;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pathfinders");
            Tooltip.SetDefault("The wearer can super-sprint and double jump if sprinting for long enough");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 34;
            Item.value = Item.sellPrice(0, 1, 10);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void SafeUpdateAccessory(Player player, bool hideVisual)
        {
            SuperSprintPlayer sprintPlayer = player.GetModPlayer<SuperSprintPlayer>();
            sprintPlayer.superSprintItemType = Type;

            if (sprintPlayer.isSprinting && sprintPlayer.CanSuperSprint)
            {
                player.IncrementTimer<Pathfinders>();
                if (player.GetTimer<Pathfinders>() >= TICKS_TO_SUPER_SPRINT)
                {
                    if (player.GetTimer<Pathfinders>() == TICKS_TO_SUPER_SPRINT)
                    {
                        // sprint begin effects
                    }

                    sprintPlayer.superSprint = true;
                    player.jumpSpeedBoost += 1f;
                    player.GetJumpState<Pathfinders_ExtraJump>().Enable();
                }
            }
            else
                player.SetTimer<Pathfinders>(0);

            float sprintChargeModifier;
            if (player.velocity.Y == 0)
            {
                sprintChargeModifier = 1f;
                if (player.velocity.X == 0)
                    sprintChargeModifier = 4f;
            }
            else if (!sprintPlayer.superSprint)
                sprintChargeModifier = 0f;
            else
                sprintChargeModifier = -1f;

            sprintPlayer.superSprintCharge += sprintChargeModifier;
            if (sprintPlayer.superSprintCharge >= SuperSprintPlayer.SUPER_SPRINT_CHARGE_MAX)
            {
                sprintPlayer.superSprintCharge = SuperSprintPlayer.SUPER_SPRINT_CHARGE_MAX;
                if(sprintPlayer.superSprintDisabledUntilFull)
                    sprintPlayer.superSprintDisabledUntilFull = false;
            }
            if (sprintPlayer.superSprintCharge <= 0)
            {
                sprintPlayer.superSprintCharge = 0;
                if (!sprintPlayer.superSprintDisabledUntilFull && sprintPlayer.superSprint)
                    sprintPlayer.superSprintDisabledUntilFull = true;
            }
                
        }
    }
    public class Pathfinders_ExtraJump : ExtraJump
    {
        public override Position GetDefaultPosition() => new Before(CloudInABottle);
        public override float GetDurationMultiplier(Player player) => 1f;
        public override void OnStarted(Player player, ref bool playSound)
        {
            int gravity = 0;
            if (player.gravDir == -1f)
                gravity -= player.height;

            ParticleSystem.AddParticle(new TestParticle(player.Bottom + Vector2.UnitY * gravity, new Vector2(player.velocity.X, player.velocity.Y * 0.5f), 30));
            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/DoubleJump"), player.Center);
            playSound = false;
        }
        public override void UpdateHorizontalSpeeds(Player player)
        {
            player.runAcceleration *= 3f;
            player.maxRunSpeed *= 2f;
        }
    }

    public class SuperSprintPlayer : ModPlayer
    {
        internal const float SUPER_SPRINT_CHARGE_MAX = 1200f;
        private const float SUPER_SPRINT_BASE_SPEED = 2f;
        private const float SUPER_SPRINT_BASE_ACCEL = 3f;

        public float speedMult = SUPER_SPRINT_BASE_SPEED;
        public float accelMult = SUPER_SPRINT_BASE_ACCEL;

        public bool isSprinting = false;
        public int sprintStopTimer = 0;

        public bool superSprint = false;
        public bool superSprintDisabledUntilFull = false;
        public int superSprintItemType = ItemID.None;
        public float superSprintCharge = 0;
        
        public bool CanSuperSprint => superSprintCharge >= 0 && !superSprintDisabledUntilFull;

        public override void Load()
        {
            MeterInfo.Register("SuperSprintCharge",
                (x) => x.GetModPlayer<SuperSprintPlayer>().superSprintItemType != ItemID.None && (x.GetModPlayer<SuperSprintPlayer>().superSprint || x.GetModPlayer<SuperSprintPlayer>().superSprintCharge < SUPER_SPRINT_CHARGE_MAX),
                SUPER_SPRINT_CHARGE_MAX,
                (x) => x.GetModPlayer<SuperSprintPlayer>().superSprintCharge,
                (x, progress) =>
                {
                    if (x.GetModPlayer<SuperSprintPlayer>().superSprintDisabledUntilFull)
                        return Color.Lerp(Color.Brown, Color.IndianRed, progress);
                    return Color.Lerp(Color.Khaki, Color.YellowGreen, progress);
                },
                $"{nameof(Radiance)}/Content/ExtraTextures/SuperSprintIcon");

            IL_Player.HorizontalMovement += MarkPlayerAsDashing;
            On_Player.SpawnFastRunParticles += SpawnSuperSprintParticles;
        }

        private void SpawnSuperSprintParticles(On_Player.orig_SpawnFastRunParticles orig, Player self)
        {
            if (self.TryGetModPlayer(out SuperSprintPlayer sprintPlayer))
            {
                if(sprintPlayer.superSprint)
                {
                    int gravity = 0;
                    if (self.gravDir == -1f)
                        gravity -= self.height;

                    if (self.runSoundDelay == 0 && self.velocity.Y == 0f)
                    {
                        SoundEngine.PlaySound(self.hermesStepSound.Style, self.position);
                        self.runSoundDelay = self.hermesStepSound.IntendedCooldown;
                    }
                    Vector2 feetPosition = self.Center + Main.rand.NextVector2FromRectangle(new Rectangle(-4, self.height / 2 + gravity, self.width + 8, 4));
                    if(Main.rand.NextBool(4))
                        ParticleSystem.AddParticle(new Sparkle(feetPosition, -self.velocity / 2f + Main.rand.NextVector2Circular(1f, 1f), 60, new Color(255, 244, 164), 0.5f));
                    else
                        ParticleSystem.AddParticle(new RadiantFire(feetPosition, Main.rand.Next(45, 60), 1f));

                    Vector2 bodyPosition = self.position + Main.rand.NextVector2FromRectangle(new Rectangle(0, 0, self.width, self.height));
                    Vector2 bodyVelocity = -self.velocity * 0.5f - Vector2.UnitY;
                    if (Main.GameUpdateCount % 6 == 0)
                        ParticleSystem.AddParticle(new SpeedLine(bodyPosition, bodyVelocity, Main.rand.Next(25, 40), new Color(255, 233, 122), (Vector2.UnitX * self.velocity.X).ToRotation(), MathF.Abs(self.velocity.X) * 8f));
                    return;
                }
            }
            orig(self);
        }

        private void MarkPlayerAsDashing(ILContext il)
        {
            static void DisableSprinting(Player x)
            {
                SuperSprintPlayer sprintPlayer = x.GetModPlayer<SuperSprintPlayer>();
                if (sprintPlayer.sprintStopTimer > 0)
                {
                    if(x.dashDelay == 0)
                        sprintPlayer.sprintStopTimer--;
                }
                else
                    sprintPlayer.isSprinting = false;
            }
            static void EnableSprinting(Player x)
            {
                if (!x.mount.Active)
                {
                    SuperSprintPlayer sprintPlayer = x.GetModPlayer<SuperSprintPlayer>();
                    sprintPlayer.sprintStopTimer = 10;
                    sprintPlayer.isSprinting = true;
                }
            }
            static float SetSprintSpeedForParticles(Player x) // without this, vanilla sprinting would stop as soon as supersprint starts since the player's max speed is being increased 
            {
                if (x.TryGetModPlayer(out SuperSprintPlayer sprintPlayer) && sprintPlayer.superSprint)
                    return (x.accRunSpeed / sprintPlayer.speedMult + x.maxRunSpeed / sprintPlayer.speedMult) / 2f;
                return (x.accRunSpeed + x.maxRunSpeed) / 2f;
            }

            ILCursor cursor = new ILCursor(il);

            cursor.Emit(OpCodes.Ldarg_0); // load player
            cursor.EmitDelegate(DisableSprinting);

            if (!cursor.TryGotoNext(MoveType.After,
               i => i.MatchLdcR4(2),
               i => i.MatchDiv(),
               i => i.MatchStloc2()))
            { 
                LogIlError("Radiance Player Is Sprinting", "Couldn't navigate to after min sprint speed check");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0); // load player
            cursor.EmitDelegate(SetSprintSpeedForParticles);
            cursor.Emit(OpCodes.Stloc_2); // set to 'num' (minimum sprint speed to create particles)

            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg0(),
                i => i.MatchLdflda(typeof(Entity), nameof(Entity.velocity)),
                i => i.MatchLdfld(typeof(Vector2), nameof(Vector2.X)),
                i => i.MatchLdcR4(0.0f),
                i => i.MatchLdloc2(),
                i => i.MatchSub(),
                i => i.MatchBgeUn(out _)))
            {
                LogIlError("Radiance Player Is Sprinting", "Couldn't navigate to after ground check 1");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0); // load player
            cursor.EmitDelegate(EnableSprinting);

            if (!cursor.TryGotoNext(MoveType.After,
               i => i.MatchLdarg0(),
               i => i.MatchLdflda(typeof(Entity), nameof(Entity.velocity)),
               i => i.MatchLdfld(typeof(Vector2), nameof(Vector2.X)),
               i => i.MatchLdloc2(),
               i => i.MatchBleUn(out _)))
            {
                LogIlError("Radiance Player Is Sprinting", "Couldn't navigate to after ground check 2");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0); // load player
            cursor.EmitDelegate(EnableSprinting);
        }
        public override void ResetEffects()
        {
            speedMult = SUPER_SPRINT_BASE_SPEED;
            accelMult = SUPER_SPRINT_BASE_ACCEL;
            superSprint = false;
            superSprintItemType = ItemID.None;
        }

        public override void UpdateDead()
        {
            superSprintDisabledUntilFull = false;
            ResetEffects();
        }

        public override void PostUpdateRunSpeeds()
        {
            if (CanSuperSprint && superSprint)
            {
                Player.accRunSpeed *= speedMult;
                Player.runAcceleration *= accelMult;
            }
        }
    }
}