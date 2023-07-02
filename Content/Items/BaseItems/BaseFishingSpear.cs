using Terraria.GameContent.Biomes;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseFishingSpear : BaseContainer
    {
        public BaseFishingSpear(int projectile, float maxRadiance, Texture2D radianceAdjustingTexture = null) : base(radianceAdjustingTexture, null, maxRadiance, ContainerMode.InputOnly, ContainerQuirk.CantAbsorb)
        {
            this.projectile = projectile;
        }
        public int projectile;
        public sealed override void SetDefaults()
        {
            Item.shoot = projectile;
            Item.maxStack = 1;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = false;
            Item.autoReuse = false; 
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shootSpeed = 12f;
            SetExtraDefaults();
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[projectile] == 0;
        public abstract void SetExtraDefaults();
    }

    public abstract class BaseFishingSpearProjectile : ModProjectile
    {
        public BaseFishingSpearProjectile(string texture, int item, bool lava = false)
        {
            this.texture = texture;
            this.item = item;
            this.lava = lava;
        }

        private string texture;
        public int item;
        public bool lava;
        private AIMode currentMode = AIMode.Held;
        public bool biting = false;
        public ref float Timer => ref Projectile.ai[0];
        public ref float Timer2 => ref Projectile.ai[1];
        public Player Owner { get => Main.player[Projectile.owner]; }

        private enum AIMode
        {
            Held,
            Thrown,
            Deployed,
            Retracting
        }
        public sealed override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.netImportant = true;
        }
        public abstract void SetExtraDefaults();
        public override bool? CanDamage() => false;
        public override bool ShouldUpdatePosition() => currentMode != AIMode.Held;
        public sealed override void AI()
        {
            Owner.itemTime = 2;
            if (!Owner.active || Owner.IsCCd() || (Owner.HeldItem.type != item))
                Projectile.Kill();

            switch(currentMode)
            {
                case AIMode.Held:
                    AI_Held();
                    break;
                case AIMode.Thrown:
                    AI_Thrown();
                    break;
                case AIMode.Deployed:
                    AI_Deployed();
                    break;
                case AIMode.Retracting:
                    AI_Retracting();
                    break;
            }
        }
        private void ShiftModeUp()
        {
            Timer = 0;
            currentMode = ++currentMode;
            Projectile.netUpdate = true;
        }
        private readonly int HELD_REEL_DURATION = 40;
        private readonly int HELD_SECOND_REEL_DURATION = 20;
        private readonly int HELD_THROW_DURATION = 15;
        private readonly float HELD_STARTING_ROTATION = 0.8f;
        private readonly float HELD_REELED_ROTATION = 3.5f;
        private readonly float HELD_ENDING_ROTATION = 3f;
        private void AI_Held()
        {
            Projectile.tileCollide = false;
            Owner.heldProj = Projectile.whoAmI;

            if (Timer == HELD_REEL_DURATION)
            {
                if (!Owner.channel)
                    Timer++;
            }
            else
                Timer++;

            float rotation = GetRotation(AIMode.Held);
            float angle = Owner.Center.AngleTo(Owner.GetModPlayer<SyncPlayer>().mouseWorld);
            Projectile.velocity = Vector2.UnitX.RotatedBy(angle) * Owner.HeldItem.shootSpeed;
            Owner.ChangeDir(Projectile.velocity.X.NonZeroSign());

            rotation -= angle * Owner.direction - (Owner.direction == -1 ? MathHelper.Pi : 0);
            rotation *= Owner.gravDir * -Owner.direction;

            float handRotation = rotation;
            Projectile.rotation = rotation - MathHelper.PiOver2;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, handRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -0.2f * Owner.direction);
            Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, handRotation) + Vector2.UnitY * Owner.gfxOffY;

            if (Timer >= HELD_REEL_DURATION + HELD_SECOND_REEL_DURATION + HELD_THROW_DURATION)
                ShiftModeUp();
        }
        private readonly int THROWN_FALL_REQUIREMENT = 10;
        private readonly float THROWN_OVERSHOOT_DURATION = 30f;
        private readonly float THROWN_OVERSHOOT_ROTATION = 1f;
        private void AI_Thrown()
        {
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Timer++;
            if (!Projectile.wet)
            {
                if (Timer > THROWN_FALL_REQUIREMENT)
                {
                    Projectile.velocity.X *= 1f - Timer / 60 / 60;
                    Projectile.velocity.Y += Timer / 60 / 10;
                }
                if (Projectile.velocity.Y > 12)
                    Projectile.velocity.Y = 12;
            }
            else
            {
                Timer2++;
                Projectile.velocity *= 1f - Timer2 / 60 / 30;
                if (Projectile.velocity.Length() <= 1f)
                    ShiftModeUp();
            }
            if (Timer < THROWN_OVERSHOOT_DURATION)
            {
                float rotation = MathHelper.Lerp(HELD_ENDING_ROTATION, THROWN_OVERSHOOT_ROTATION, EaseOutExponent(Timer / THROWN_OVERSHOOT_DURATION, 8)) * -Owner.direction - (Owner.direction == -1 ? MathHelper.Pi : 0);
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rotation + Projectile.velocity.ToRotation());
            }
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -0.2f * Owner.direction);
        }
        private void AI_Deployed()
        {
            Projectile.velocity *= 0.98f;
            Main.NewText('w');
        }
        private void AI_Retracting()
        {

        }
        private float GetRotation(AIMode mode)
        {
            float rotation = 0;
            switch(mode)
            {
                case AIMode.Held:
                    if (Timer <= HELD_REEL_DURATION)
                    {
                        rotation = MathHelper.Lerp(HELD_STARTING_ROTATION, HELD_REELED_ROTATION, EaseInOutCirc(Timer / HELD_REEL_DURATION));
                    }
                    else if (Timer - HELD_REEL_DURATION <= HELD_SECOND_REEL_DURATION)
                    {
                        float adjustedTimer = Timer - HELD_REEL_DURATION - 1f;
                        rotation = MathHelper.Lerp(HELD_REELED_ROTATION, HELD_REELED_ROTATION + 0.3f, EaseOutExponent(adjustedTimer / HELD_SECOND_REEL_DURATION, 2f));
                    }
                    else
                    {
                        float adjustedTimer = Timer - HELD_REEL_DURATION - HELD_SECOND_REEL_DURATION - 1f;
                        rotation = MathHelper.Lerp(HELD_REELED_ROTATION + 0.3f, HELD_ENDING_ROTATION, EaseInExponent(adjustedTimer / HELD_THROW_DURATION, 7f));
                    }
                    break;
            }
            return rotation;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, (Projectile.velocity.X < 0 && currentMode == AIMode.Held) ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D handleTex = ModContent.Request<Texture2D>(texture + "_Handle").Value;
            switch (currentMode)
            {
                case AIMode.Thrown:
                    Vector2 pos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Quarter, -0.2f * Owner.direction);
                    Main.EntitySpriteDraw(handleTex, pos - Main.screenPosition, null, lightColor, -0.2f * Owner.direction + MathHelper.PiOver2, handleTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

                    break;
            }
        }
    }
}