using Radiance.Core.Visuals.Primitives;
using static Radiance.Content.Items.BaseItems.FishingSpearPart;

namespace Radiance.Content.Items.BaseItems
{
    public abstract class BaseFishingSpear : BaseContainer
    {
        public BaseFishingSpear(int projectile, float maxRadiance, Dictionary<BaseContainer_TextureType, string> extraTextures = null) : base(extraTextures, maxRadiance, false)
        {
            this.projectile = projectile;
        }

        public int projectile;

        public override sealed void SetDefaults()
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

        public Vector2 GetShaftDetails(out int width, out int height)
        {
            FishingSpearPart shaft = parts.First(x => x.type == FishingSpearPartType.Shaft);
            Texture2D shaftTexture = ModContent.Request<Texture2D>(texture + shaft.name).Value;
            width = shaftTexture.Width;
            height = shaftTexture.Height;
            Vector2 shaftTip = Vector2.UnitY * -shaftTexture.Height / 2 + shaft.position;
            return shaftTip.RotatedBy(Projectile.rotation) + Projectile.Center;
        }

        public override sealed void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.netImportant = true;
            Projectile.scale = 0.2f;

            parts = SetupParts();
            parts.ForEach(x => x.parent = this);
        }

        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.scale = 0.2f;
        }

        public abstract void SetExtraDefaults();

        public override bool? CanDamage() => false;

        public override bool ShouldUpdatePosition() => currentMode != AIMode.Held && currentMode != AIMode.RetractingHeld;

        public override sealed void AI()
        {
            Owner.itemTime = 2;
            if (!Owner.active || Owner.IsCCd() || (Owner.HeldItem.type != item))
                Projectile.Kill();

            switch (currentMode)
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
                    //case AIMode.RetractingHeld:
                    //    AI_RetractingHeld();
                    //    break;
            }
            foreach (FishingSpearPart part in parts)
            {
                part.UpdateFishingLine();
                if (part.type == FishingSpearPartType.Light)
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

        private readonly int HELD_REEL_DURATION = 10;
        private readonly int HELD_SECOND_REEL_DURATION = 5;
        private readonly int HELD_THROW_DURATION = 5;
        private readonly float HELD_STARTING_ROTATION = 0.9f;
        private readonly float HELD_REELED_ROTATION = -0.5f;
        private readonly float HELD_SECOND_REELED_ROTATION_INCREMENT = -0.3f;
        private readonly float HELD_ENDING_ROTATION = -0.8f;

        private void AI_Held()
        {
            Projectile.tileCollide = false;

            if (Projectile.scale < 1f)
                Projectile.scale += 0.075f;
            if (Projectile.scale > 1)
                Projectile.scale = 1;

            float rotation = GetRotation(AIMode.Held);
            float angle = Owner.Center.AngleTo(Owner.GetModPlayer<SyncPlayer>().mouseWorld);

            Projectile.velocity = Vector2.UnitX.RotatedBy(angle) * Owner.HeldItem.shootSpeed;
            Owner.ChangeDir(Projectile.velocity.X.NonZeroSign());

            rotation += angle * Owner.gravDir;
            rotation += SineTiming(120) / 10f;

            float handRotation = rotation;
            Projectile.rotation = rotation + PiOver2;

            if (Projectile.direction > 0)
                handRotation += Pi;

            if (Owner.gravDir == -1)
            {
                Projectile.rotation *= Owner.gravDir;
                Projectile.rotation += Pi;
            }
            Owner.GetModPlayer<FishingSpearPlayer>().heldFishingSpear = Projectile.whoAmI;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, handRotation);
            BackArmHandleHold();
            Projectile.Center = Owner.GetFrontHandPositionGravComplying(Player.CompositeArmStretchAmount.ThreeQuarters, handRotation) + Vector2.UnitY * Owner.gfxOffY + Vector2.UnitX.RotatedBy(handRotation * Owner.gravDir) * -holdOffset * -Owner.direction;

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
        private readonly float THROWN_OVERSHOOT_ROTATION = -0.2f;

        private void AI_Thrown()
        {
            Projectile.tileCollide = true;
            Projectile.rotation = AI_Thrown_ProperRotation();

            FishingSpearPart shaft = parts.First(x => x.type == FishingSpearPartType.Shaft);
            Texture2D shaftTexture = ModContent.Request<Texture2D>(texture + shaft.name).Value;

            Vector2 shaftTipWorldPos = GetShaftDetails(out int shaftWidth, out int shaftHeight);

            if (!Projectile.wet || !Collision.WetCollision(shaftTipWorldPos - Vector2.One * shaftWidth / 2, shaftWidth, shaftWidth))
            {
                Timer2 = 0;
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
                Projectile.velocity *= 1f - Timer2 / 60 / 10;
                if (Projectile.velocity.Length() <= 1.5f && Timer2 > 20)
                    ShiftModeUp();

                Timer2++;
            }
            if (Timer < THROWN_OVERSHOOT_DURATION)
            {
                float rotation = Lerp(HELD_ENDING_ROTATION, THROWN_OVERSHOOT_ROTATION, EaseOutExponent(Timer / THROWN_OVERSHOOT_DURATION, 3)) * Projectile.direction;
                rotation += Projectile.velocity.ToRotation() * Owner.gravDir;
                if (Projectile.direction == -1)
                    rotation += Pi;

                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rotation);
            }
            Timer++;
            BackArmHandleHold();
        }

        private float AI_Thrown_ProperRotation()
        {
            float rot = Vector2.Zero.AngleTo(Projectile.velocity);
            return (rot < 0 ? rot + TwoPi : rot) + PiOver2;
        }

        private void AI_Deployed()
        {
            Projectile.velocity *= 0.98f;
            Vector2 shaftTipWorldPos = GetShaftDetails(out _, out _);

            int index = 0;
            foreach (FishingSpearPart part in parts)
            {
                part.canCatch = false;
                part.offset = Vector2.Zero;
                if (part.type != FishingSpearPartType.Shaft)
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
                    if (part.worldPosition.Distance(shaftTipWorldPos) > part.maxDistance) //clamp position
                        part.worldPosition = shaftTipWorldPos + Vector2.Normalize(Vector2.UnitX.RotatedBy(part.worldPosition.AngleFrom(shaftTipWorldPos))) * part.maxDistance;

                    part.offset += Vector2.UnitX.RotatedBy(TwoPi * SineTiming(600, MathF.Pow(index, 9))) * MathF.Pow(Math.Min(Timer, 600), 0.3f) * (distance / 80f) * ease;
                    part.offset += Vector2.UnitX.RotatedBy(TwoPi * SineTiming(950, MathF.Pow(index + 1, 3))) * MathF.Pow(Math.Min(Timer, 600), 0.3f) * (distance / 80f) * ease;
                    part.rotation = SineTiming(90 + MathF.Pow(index, 3), MathF.Pow(index, 9)) * 0.3f;
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
                part.rotation = Lerp(0, part.rotation, 0.01f * speed);
                part.position = Vector2.Lerp(part.position, part.initialPosition, 0.01f * speed);
                part.offset = Vector2.Lerp(part.offset, Vector2.Zero, 0.01f * speed);
            }
            Projectile.rotation = Lerp(Projectile.rotation, Projectile.Center.AngleTo(Owner.Center) + Pi * 1.5f, 0.01f * speed);
            Projectile.Center += Vector2.UnitX.RotatedBy(Projectile.Center.AngleTo(Owner.Center)) * speed;
            if (Projectile.Hitbox.Intersects(Owner.Hitbox))
            {
                //ShiftModeUp();
                //Timer2 = Owner.Center.AngleTo(Projectile.Center);
                Projectile.Kill();
            }
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.Center.AngleTo(Projectile.Center) - PiOver2);
            BackArmHandleHold();
            Timer++;
        }

        private readonly int RETRACTINGHELD_REEL_DURATION = 60;
        private readonly float RETRACTINGHELD_ENDING_ROTATION = 3f;
        //private void AI_RetractingHeld()
        //{
        //    Projectile.tileCollide = false;
        //    Owner.heldProj = Projectile.whoAmI;

        //    float rotation = GetRotation(AIMode.RetractingHeld);
        //    Projectile.velocity = Vector2.UnitX.RotatedBy(Timer2) * Owner.HeldItem.shootSpeed;
        //    Main.NewText(Timer2);
        //    Owner.ChangeDir(Projectile.velocity.X.NonZeroSign());

        //    rotation -= Timer2 * Owner.direction;
        //    rotation *= Owner.gravDir * -Owner.direction;

        //    float handRotation = rotation - MathHelper.PiOver2;
        //    Projectile.rotation = rotation + MathHelper.Pi;

        //    Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, handRotation);
        //    BackArmHandleHold();

        //    Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, handRotation) + Vector2.UnitY * Owner.gfxOffY + Vector2.UnitX.RotatedBy(handRotation) * Projectile.direction * holdOffset;

        //    if (Timer >= RETRACTINGHELD_REEL_DURATION)
        //        Projectile.Kill();

        //    //if (Projectile.scale > 0.2f)
        //    //    Projectile.scale -= 0.025f;
        //    //if (Projectile.scale < 0.2f)
        //    //    Projectile.scale = 0.2f;

        //    Timer++;
        //}
        private void BackArmHandleHold()
        {
            HandlePos(out float rotation, Owner);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, rotation - PiOver2);
        }

        private float GetRotation(AIMode mode)
        {
            float rotation = 0;
            switch (mode)
            {
                case AIMode.Held:
                    if (Timer <= HELD_REEL_DURATION)
                    {
                        rotation = Lerp(HELD_STARTING_ROTATION, HELD_REELED_ROTATION, EaseOutExponent(Timer / HELD_REEL_DURATION, 4f));
                    }
                    else if (Timer - HELD_REEL_DURATION <= HELD_SECOND_REEL_DURATION)
                    {
                        float adjustedTimer = Timer - HELD_REEL_DURATION - 1f;
                        rotation = Lerp(HELD_REELED_ROTATION, HELD_REELED_ROTATION + HELD_SECOND_REELED_ROTATION_INCREMENT, EaseOutExponent(adjustedTimer / HELD_SECOND_REEL_DURATION, 2f));
                    }
                    else
                    {
                        float adjustedTimer = Timer - HELD_REEL_DURATION - HELD_SECOND_REEL_DURATION - 1f;
                        rotation = Lerp(HELD_REELED_ROTATION + HELD_SECOND_REELED_ROTATION_INCREMENT, HELD_ENDING_ROTATION, EaseInExponent(adjustedTimer / HELD_THROW_DURATION, 2f));
                    }
                    break;

                case AIMode.RetractingHeld:
                    rotation = Lerp(0, RETRACTINGHELD_ENDING_ROTATION, EaseOutExponent(Timer / RETRACTINGHELD_REEL_DURATION, 3f));
                    break;
            }
            rotation *= Projectile.direction;
            return rotation;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            //Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.FlipVertically, 0);

            if (Owner.GetModPlayer<FishingSpearPlayer>().heldFishingSpear == -1)
            {
                foreach (FishingSpearPart part in parts)
                {
                    part.DrawPart(Main.spriteBatch, lightColor);
                }
            }
            foreach (FishingSpearPart part in parts)
            {
                part.DrawFishingLine(Main.spriteBatch, lightColor);
            }
            return false;
        }

        public static Vector2 HandlePos(out float rotation, Player owner)
        {
            rotation = -0.2f * owner.direction * owner.gravDir + PiOver2;
            Vector2 position = owner.GetBackHandPositionGravComplying(Player.CompositeArmStretchAmount.Quarter, rotation - PiOver2);
            return position;
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

        public readonly int MaxControlPoints = 8;
        public BaseFishingSpearProjectile parent;
        public FishingSpearPartType type;
        public Vector2 initialPosition;
        public Vector2 position;
        public Vector2 offset;
        public Vector2 stringOffset;
        public float rotation;

        internal PrimitiveTrail fishingLine;
        public List<Vector2> fishingLineCurve;

        public bool canCatch;

        public Color? drawColor;
        public bool flipped;
        public string name;
        public float maxDistance = 0;

        public Vector2 worldPosition
        {
            get => parent.Projectile.Center + position.RotatedBy(parent.Projectile.rotation) * parent.Projectile.scale;
            set => position = (value - parent.Projectile.Center).RotatedBy(-parent.Projectile.rotation) * parent.Projectile.scale;
        }

        public Vector2 initialWorldPosition
        {
            get => parent.Projectile.Center + initialPosition.RotatedBy(parent.Projectile.rotation) * parent.Projectile.scale;
        }

        public FishingSpearPart(FishingSpearPartType type, Vector2 position, Vector2? stringOffset = null, bool flipped = false, Color? color = null, string name = "")
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
                    maxDistance = 170;
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

            if (!stringOffset.HasValue)
                stringOffset = Vector2.Zero;

            Vector2 stringOffsetReal = stringOffset.Value;
            if (flipped)
                stringOffsetReal.X = -stringOffsetReal.X;

            this.stringOffset = stringOffsetReal;
        }

        public void UpdateFishingLine()
        {
            Vector2[] controlPoints = new Vector2[MaxControlPoints];
            Vector2 adjustedWorldPos = worldPosition + offset + stringOffset.RotatedBy(parent.Projectile.rotation + rotation);
            if (type != FishingSpearPartType.Shaft)
            {
                FishingSpearPart shaft = parent.parts.First(x => x.type == FishingSpearPartType.Shaft);
                Texture2D shaftTexture = ModContent.Request<Texture2D>(parent.texture + shaft.name).Value;
                Vector2 shaftTip = Vector2.UnitY * -shaftTexture.Height / 2 + shaft.position;
                Vector2 shaftTipWorldPos = shaftTip.RotatedBy(parent.Projectile.rotation) + parent.Projectile.Center;
                controlPoints[0] = shaftTipWorldPos;
                controlPoints[MaxControlPoints - 1] = adjustedWorldPos;

                for (int i = 1; i < MaxControlPoints - 1; i++)
                {
                    Vector2 position = shaftTipWorldPos + (adjustedWorldPos - shaftTipWorldPos) * ((float)i / (MaxControlPoints - 1));
                    position += Vector2.UnitX.RotatedBy(parent.Projectile.rotation + rotation) * SineTiming(120, i * 40 + 75 * parent.parts.IndexOf(this)) * Vector2.Distance(shaftTipWorldPos, adjustedWorldPos) / 10;
                    controlPoints[i] = position;
                }
                //foreach (Vector2 point in controlPoints)
                //{
                //    point.SpawnDebugDust();
                //}
            }
            else
            {
                Texture2D handleTex = ModContent.Request<Texture2D>(parent.texture + "_Handle").Value;
                Vector2 handlePos = BaseFishingSpearProjectile.HandlePos(out float rotation, parent.Owner) + Vector2.UnitY.RotatedBy(rotation * -parent.Owner.direction) * handleTex.Height / 2;

                controlPoints[0] = handlePos;
                controlPoints[MaxControlPoints - 1] = adjustedWorldPos;

                for (int i = 1; i < MaxControlPoints - 1; i++)
                {
                    Vector2 position = handlePos + (adjustedWorldPos - handlePos) * ((float)i / (MaxControlPoints - 1));
                    position += Vector2.UnitX.RotatedBy(parent.Projectile.rotation + this.rotation) * SineTiming(120, i * 40 + 75 * parent.parts.IndexOf(this)) * Vector2.Distance(handlePos, adjustedWorldPos) / 60;
                    controlPoints[i] = position;
                }
                foreach (Vector2 point in controlPoints)
                {
                    //point.SpawnDebugDust();
                }
            }
            fishingLine ??= new PrimitiveTrail(30, PrimWidthFunction, PrimColorFunction);

            Vector2[] segmentPositions = controlPoints.ToArray();

            fishingLine.SetPositions(segmentPositions, SmoothBezierPointRetreivalFunction);
            fishingLine.NextPosition = worldPosition + offset;
        }

        public float PrimWidthFunction(float completionRatio)
        {
            return 0.8f;
        }

        public Color PrimColorFunction(float completionRatio)
        {
            if (type == FishingSpearPartType.Shaft)
            {
                return Color.Lerp(Lighting.GetColor((int)parent.Owner.Center.X / 16, (int)parent.Owner.Center.Y / 16), Lighting.GetColor((int)worldPosition.X / 16, (int)worldPosition.Y / 16), completionRatio);
            }
            return Color.Lerp(Lighting.GetColor((int)parent.GetShaftDetails(out var _, out var _).X / 16, (int)parent.GetShaftDetails(out var _, out var _).Y / 16), Lighting.GetColor((int)worldPosition.X / 16, (int)worldPosition.Y / 16), completionRatio);
        }

        public void DrawPart(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(parent.texture + name).Value;
            float rotation = parent.Projectile.rotation;
            SpriteEffects drawDirection = SpriteEffects.None;
            if (flipped)
                drawDirection = SpriteEffects.FlipHorizontally;

            Vector2 drawPos = worldPosition + this.offset - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, drawColor ?? lightColor, rotation + this.rotation, tex.Size() / 2, parent.Projectile.scale, drawDirection, 0f);
        }

        public void DrawPart(ref PlayerDrawSet drawInfo)
        {
            Texture2D tex = ModContent.Request<Texture2D>(parent.texture + name).Value;
            float rotation = parent.Projectile.rotation;
            SpriteEffects drawDirection = SpriteEffects.None;
            if (flipped)
                drawDirection = SpriteEffects.FlipHorizontally;

            Vector2 drawPos = worldPosition + this.offset - Main.screenPosition;
            drawInfo.DrawDataCache.Add(new DrawData(tex, drawPos, null, drawColor ?? Lighting.GetColor((int)parent.Projectile.Center.X / 16, (int)parent.Projectile.Center.Y / 16), rotation + this.rotation, tex.Size() / 2, parent.Projectile.scale, drawDirection, 0f));
        }

        public void DrawFishingLine(SpriteBatch spriteBatch, Color lightColor)
        {
            fishingLine?.Render(null, (Matrix?)null);
        }
    }

    public class FishingSpearPlayer : ModPlayer
    {
        public int heldFishingSpear = -1;
        public BaseFishingSpearProjectile heldSpear => Main.projectile[heldFishingSpear].ModProjectile as BaseFishingSpearProjectile;
        public BaseFishingSpearProjectile ownedSpear => Main.projectile.FirstOrDefault(x => x.ModProjectile is not null && x.ModProjectile is BaseFishingSpearProjectile && x.owner == Player.whoAmI && x.active)?.ModProjectile as BaseFishingSpearProjectile;

        public override void ResetEffects()
        {
            heldFishingSpear = -1;
        }

        public override void UpdateDead()
        {
            heldFishingSpear = -1;
        }
    }

    public class FishingSpearHandleLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Leggings);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<FishingSpearPlayer>().ownedSpear != null;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            BaseFishingSpearProjectile spear = drawInfo.drawPlayer.GetModPlayer<FishingSpearPlayer>().ownedSpear;
            Vector2 handlePos = BaseFishingSpearProjectile.HandlePos(out float rotation, drawInfo.drawPlayer);
            Texture2D handleTex = ModContent.Request<Texture2D>(spear.texture + "_Handle").Value;
            drawInfo.DrawDataCache.Add(new DrawData(handleTex, handlePos - Main.screenPosition, null, Lighting.GetColor((int)handlePos.X / 16, (int)handlePos.Y / 16), rotation, handleTex.Size() / 2, 1f, drawInfo.drawPlayer.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0));
        }
    }

    public class FishingSpearHeldSpearLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<FishingSpearPlayer>().heldFishingSpear > -1;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            BaseFishingSpearProjectile spear = drawInfo.drawPlayer.GetModPlayer<FishingSpearPlayer>().heldSpear;
            foreach (FishingSpearPart part in spear.parts)
            {
                part.DrawPart(ref drawInfo);
            }
        }
    }
}