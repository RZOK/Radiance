using Steamworks;
using Terraria.GameContent.Biomes;
using Terraria.Graphics.Renderers;

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
            Item.useTime = 20;
            Item.useAnimation = 20;
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
        public BaseFishingSpearProjectile(string texture, int item, float holdOffset, bool lava = false)
        {
            this.texture = texture;
            this.item = item;
            this.holdOffset = holdOffset;
            this.lava = lava;
        }

        internal string texture;
        public float holdOffset;
        public int item;
        public bool lava;
        private AIMode currentMode = AIMode.Held;
        public bool biting = false;
        public ref float Timer => ref Projectile.ai[0];
        public ref float Timer2 => ref Projectile.ai[1];
        public Player Owner { get => Main.player[Projectile.owner]; }

        public List<FishingSpearPart> parts;
        public abstract List<FishingSpearPart> SetupParts();
        private enum AIMode
        {
            Held,
            Thrown,
            Deployed,
            Retracting,
            RetractingHeld
        }
        public sealed override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.netImportant = true;
            
            parts = SetupParts();
            foreach (FishingSpearPart part in parts)
            {
                part.parent = this;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0.2f;
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
                case AIMode.RetractingHeld:
                    AI_RetractingHeld();
                    break;
            }
            foreach (FishingSpearPart part in parts)
            {
                if (part.type == FishingSpearPart.FishingSpearPartType.Light)
                {
                    float lightStrength = 1.2f;
                    Lighting.AddLight(part.worldPosition + part.offset, new Vector3(255f / 255f, 200f / 255f, 100f / 255f) * lightStrength);
                }
            }
        }
        private void ShiftModeUp()
        {
            Timer = Timer2 = 0;
            currentMode++;            
            Projectile.netUpdate = true;
        }
        private readonly int HELD_REEL_DURATION = 20;
        private readonly int HELD_SECOND_REEL_DURATION = 10;
        private readonly int HELD_THROW_DURATION = 10;
        private readonly float HELD_STARTING_ROTATION = 0.8f;
        private readonly float HELD_REELED_ROTATION = 3.5f;
        private readonly float HELD_ENDING_ROTATION = 3f;
        private void AI_Held()
        {
            Projectile.tileCollide = false;
            Owner.heldProj = Projectile.whoAmI;

            if (Projectile.scale < 1f)
                Projectile.scale += 0.1f;
            if (Projectile.scale > 1)
                Projectile.scale = 1;

            float rotation = GetRotation(AIMode.Held);
            float angle = Owner.Center.AngleTo(Owner.GetModPlayer<SyncPlayer>().mouseWorld);
            Projectile.velocity = Vector2.UnitX.RotatedBy(angle) * Owner.HeldItem.shootSpeed;
            Owner.ChangeDir(Projectile.velocity.X.NonZeroSign());

            rotation -= angle * Owner.direction - (Owner.direction == -1 ? MathHelper.Pi : 0);
            rotation *= Owner.gravDir * -Owner.direction;

            float handRotation = rotation;
            Projectile.rotation = rotation - MathHelper.PiOver2;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, handRotation);
            BackArmHandleHold();
            Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, handRotation) + Vector2.UnitY * Owner.gfxOffY + Vector2.UnitX.RotatedBy(handRotation) * Projectile.direction * holdOffset;

            if (Timer >= HELD_REEL_DURATION + HELD_SECOND_REEL_DURATION + HELD_THROW_DURATION)
                ShiftModeUp();

            if (Timer == HELD_REEL_DURATION)
            {
                if (!Owner.channel)
                    Timer++;
            }
            else
                Timer++;
        }
        private readonly int THROWN_FALL_REQUIREMENT = 10;
        private readonly float THROWN_OVERSHOOT_DURATION = 30f;
        private readonly float THROWN_OVERSHOOT_ROTATION = 1f;
        private void AI_Thrown()
        {
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
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
                Projectile.velocity *= 1f - Timer2 / 60 / 10;
                if (Projectile.velocity.Length() <= 1f)
                    ShiftModeUp();
            }
            if (Timer < THROWN_OVERSHOOT_DURATION)
            {
                float rotation = MathHelper.Lerp(HELD_ENDING_ROTATION, THROWN_OVERSHOOT_ROTATION, EaseOutExponent(Timer / THROWN_OVERSHOOT_DURATION, 8)) * -Owner.direction - (Owner.direction == -1 ? MathHelper.Pi : 0);
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rotation + Projectile.velocity.ToRotation());
            }
            BackArmHandleHold();
        }
        private void AI_Deployed()
        {
            Projectile.velocity *= 0.98f;
            FishingSpearPart shaft = parts.First(x => x.type == FishingSpearPart.FishingSpearPartType.Shaft);
            Texture2D shaftTexture = ModContent.Request<Texture2D>(texture + shaft.name).Value;
            Vector2 shaftTip = Vector2.UnitY * shaftTexture.Height / 2 - shaft.position;

            Vector2 shaftTipWorldPos = shaftTip.RotatedBy(Projectile.rotation) + Projectile.Center;
            //shaftTipWorldPos.SpawnDebugDust();
            int index = 0;
            foreach (FishingSpearPart part in parts)
            {
                part.canCatch = false;
                part.offset = Vector2.Zero;
                if (part.type != FishingSpearPart.FishingSpearPartType.Shaft)
                {
                    index++;
                    Vector2 away = Vector2.UnitX.RotatedBy(part.worldPosition.AngleFrom(shaftTipWorldPos));
                    float speed = 0.25f;
                    float easeSpeed = 1f;
                    float distance = Vector2.Distance(part.initialWorldPosition + away * part.maxDistance, part.worldPosition);
                    float divdDistance = distance / part.maxDistance * easeSpeed + 1f - easeSpeed;
                    float ease = EaseInExponent(divdDistance, 3f);
                    speed *= ease;
                    Vector2 idealPosition = part.worldPosition + away * speed;

                    Texture2D partTexture = ModContent.Request<Texture2D>(texture + part.name).Value;
                    if (Collision.WetCollision(part.worldPosition - partTexture.Size() / 2, partTexture.Width, partTexture.Height) && Collision.WetCollision(part.worldPosition - partTexture.Size() / 2 - Vector2.UnitY * 16, partTexture.Width, partTexture.Height) && !Collision.SolidCollision(part.worldPosition - partTexture.Size() / 2, partTexture.Width, partTexture.Height)) //dont update position if it would be above water
                    {
                        part.canCatch = true;
                        part.worldPosition = idealPosition;
                    }
                    if(part.worldPosition.Distance(shaftTipWorldPos) > part.maxDistance) //clamp position
                        part.worldPosition = shaftTipWorldPos + Vector2.Normalize(Vector2.UnitX.RotatedBy(part.worldPosition.AngleFrom(shaftTipWorldPos))) * part.maxDistance;

                    part.offset += Vector2.UnitX.RotatedBy(MathHelper.TwoPi * SineTiming(600, MathF.Pow(index, 9))) * MathF.Pow(Math.Min(Timer, 600), 0.3f) * (distance / 80f) * ease;
                    part.offset += Vector2.UnitX.RotatedBy(MathHelper.TwoPi * SineTiming(950, MathF.Pow(index + 1, 3))) * MathF.Pow(Math.Min(Timer, 600), 0.3f) * (distance / 80f) * ease;
                    part.rotation = SineTiming(90 + MathF.Pow(index, 3), MathF.Pow(index, 9)) * 0.3f;
                    //(part.worldPosition + part.offset).SpawnDebugDust();
                }
            }
            if (Owner == Main.LocalPlayer)
            {
                if (Main.mouseLeft && Main.mouseLeftRelease && !Owner.mouseInterface)
                    ShiftModeUp();
            }
            Timer++;
            BackArmHandleHold();
        }

        private void AI_Retracting()
        {
            Projectile.tileCollide = false;
            float speed = MathF.Pow(Timer, 2) / 100f;
            foreach (FishingSpearPart part in parts)
            {
                part.rotation = MathHelper.Lerp(0, part.rotation, 0.01f * speed);
                part.position = Vector2.Lerp(part.position, part.initialPosition, 0.01f * speed);
                part.offset = Vector2.Lerp(part.offset, Vector2.Zero, 0.01f * speed);
            }
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.Center.AngleTo(Owner.Center) + MathHelper.PiOver2, 0.01f * speed);
            Projectile.Center += Vector2.UnitX.RotatedBy(Projectile.Center.AngleTo(Owner.Center)) * speed;
            if(Projectile.Hitbox.Intersects(Owner.Hitbox))
            {
                ShiftModeUp();
                Timer2 = Owner.Center.AngleTo(Projectile.Center);
            }
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.Center.AngleTo(Projectile.Center) - MathHelper.PiOver2);
            BackArmHandleHold();
            Timer++;
        }

        private readonly int RETRACTINGHELD_REEL_DURATION = 120;
        private readonly float RETRACTINGHELD_ENDING_ROTATION = 3f;
        private void AI_RetractingHeld()
        {
            Projectile.tileCollide = false;
            Owner.heldProj = Projectile.whoAmI;

            float rotation = GetRotation(AIMode.RetractingHeld);
            Projectile.velocity = Vector2.UnitX.RotatedBy(Timer2) * Owner.HeldItem.shootSpeed;
            Main.NewText(Timer2);
            Owner.ChangeDir(Projectile.velocity.X.NonZeroSign());

            rotation -= Timer2 * Owner.direction - (Owner.direction == -1 ? MathHelper.Pi : 0);
            rotation *= Owner.gravDir * -Owner.direction;

            float handRotation = rotation - MathHelper.PiOver2;
            Projectile.rotation = rotation;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, handRotation);
            BackArmHandleHold();

            Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, handRotation) + Vector2.UnitY * Owner.gfxOffY + Vector2.UnitX.RotatedBy(handRotation) * Projectile.direction * holdOffset;

            if (Timer >= RETRACTINGHELD_REEL_DURATION)
                Projectile.Kill();

            //if (Projectile.scale > 0.2f)
            //    Projectile.scale -= 0.025f;
            //if (Projectile.scale < 0.2f)
            //    Projectile.scale = 0.2f;

            Timer++;
        }
        private void BackArmHandleHold()
        {
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, -0.2f * Owner.direction);
        }
        private float GetRotation(AIMode mode)
        {
            float rotation = 0;
            switch(mode)
            {
                case AIMode.Held:
                    if (Timer <= HELD_REEL_DURATION)
                    {
                        rotation = MathHelper.Lerp(HELD_STARTING_ROTATION, HELD_REELED_ROTATION, EaseOutExponent(Timer / (HELD_REEL_DURATION + 20), 7));
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
                case AIMode.RetractingHeld:
                    rotation = MathHelper.Lerp(0, RETRACTINGHELD_ENDING_ROTATION, EaseOutExponent(Timer / RETRACTINGHELD_REEL_DURATION, 7));
                    break;
            }
            return rotation;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            //Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.FlipVertically, 0);
            foreach (FishingSpearPart part in parts)
            {
                part.DrawPart(Main.spriteBatch, lightColor);
            }
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D handleTex = ModContent.Request<Texture2D>(texture + "_Handle").Value;
                    Vector2 pos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Quarter, -0.2f * Owner.direction);
                    Main.EntitySpriteDraw(handleTex, pos - Main.screenPosition, null, lightColor, -0.2f * Owner.direction + MathHelper.PiOver2, handleTex.Size() / 2, 1f, SpriteEffects.None, 0);

            
        }
    }
    public class FishingSpearPart
    {
        public enum FishingSpearPartType
        {
            Shaft,
            Hook,
            Light,
            Extra
        }
        public BaseFishingSpearProjectile parent;
        public FishingSpearPartType type;
        public Vector2 initialPosition;
        public Vector2 position;
        public Vector2 offset;
        public float rotation;

        public bool canCatch;

        public Color? drawColor;
        public bool flipped;
        public string name;
        public float maxDistance = 0;
        public Vector2 worldPosition 
        { 
            get => parent.Projectile.Center + position.RotatedBy(parent.Projectile.rotation + MathHelper.Pi) * parent.Projectile.scale;
            set => position = (value - parent.Projectile.Center).RotatedBy(-parent.Projectile.rotation + MathHelper.Pi) * parent.Projectile.scale;
        }
        public Vector2 initialWorldPosition
        {
            get => parent.Projectile.Center + initialPosition.RotatedBy(parent.Projectile.rotation + MathHelper.Pi) * parent.Projectile.scale;
        }
        public FishingSpearPart(FishingSpearPartType type, Vector2 position, bool flipped = false, Color? color = null, string name = "")
        {
            if (name == string.Empty)
            {
                switch (type)
                {
                    case FishingSpearPartType.Shaft:
                        name = "_Shaft";
                        break;
                    case FishingSpearPartType.Hook:
                        name = "_Hook";
                        break;
                    case FishingSpearPartType.Light:
                        name = "_Light";
                        break;
                    case FishingSpearPartType.Extra:
                        name = "_Extra";
                        break;
                }
            }
            switch (type)
            {
                case FishingSpearPartType.Hook:
                    maxDistance = 120;
                    break;
                case FishingSpearPartType.Light:
                    maxDistance = 20;
                    break;
                case FishingSpearPartType.Extra:
                    maxDistance = 40;
                    break;
            }
            if (color.HasValue)
                drawColor = color.Value;

            this.type = type;
            this.name = name;
            this.position = initialPosition = position;
            this.flipped = flipped;
        }
        public void DrawPart(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(parent.texture + name).Value;
            float rotation = parent.Projectile.rotation;
            SpriteEffects drawDirection = SpriteEffects.None;
            if(flipped)
                drawDirection = SpriteEffects.FlipHorizontally;

            Vector2 offset = position.RotatedBy(parent.Projectile.rotation) * parent.Projectile.direction * parent.Projectile.scale;
            if (parent.ShouldUpdatePosition() && parent.Projectile.direction == 1)
            {
                rotation += MathHelper.Pi;
                offset *= -1f;
            }
            if (parent.Projectile.direction == -1)
                rotation += MathHelper.Pi;

            Vector2 drawPos = parent.Projectile.Center + offset + this.offset - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, drawColor ?? lightColor, rotation + this.rotation, tex.Size() / 2, parent.Projectile.scale, drawDirection, 0f);
        }
    }
}