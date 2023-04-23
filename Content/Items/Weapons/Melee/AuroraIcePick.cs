using log4net.Filter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System;
using System.Composition.Convention;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Weapons.Melee
{
    #region Main Item

    public class AuroraIcePick : ModItem
    {
        public bool reversed = false;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aurora Ice Pick");
            Tooltip.SetDefault("Creates damaging afterimages upon colliding with a tile or critically striking an enemy");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.autoReuse = false;
            Item.rare = ItemRarityID.Green;
            Item.knockBack = 4;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.sellPrice(0, 1);
            Item.shootSpeed = 20f;
            Item.shoot = ModContent.ProjectileType<AuroraIcePickProjectile>();
        }
        public override bool CanUseItem(Player player) => !Main.projectile.Any(x => x.owner == player.whoAmI && x.ModProjectile is AuroraIcePickProjectile z && z.currentState != AuroraIcePickProjectile.AIState.ReturningHeld);
        //public override void HoldItem(Player player) => player.GetModPlayer<SyncPlayer>().mouseListener = true;
    }
    #endregion Main Item

    #region Projectile

    public class AuroraIcePickProjectile : ModProjectile
    {
        public override string Texture => "Radiance/Content/Items/Weapons/Melee/AuroraIcePick";
        public enum AIState
        {
            Held,
            Flying,
            Returning,
            ReturningHeld
        }
        private Player Owner => Main.player[Projectile.owner];
        private int heldDuration = 15;
        internal AIState currentState 
        { 
            get => (AIState)Projectile.ai[0]; 
            set => Projectile.ai[0] = (int)value; 
        }
        private float timer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aurora Ice Pick");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
        }
        public override bool ShouldUpdatePosition() => currentState == AIState.Flying || currentState == AIState.Returning;
        public override void AI()
        {
            switch(currentState)
            {
                case AIState.Held:
                    Projectile.tileCollide = false;
                    float rotation = MathHelper.Lerp(0.8f, 4f, RadianceUtils.EaseInExponent(timer / heldDuration, 2.5f)) * Projectile.direction;
                    Projectile.rotation = rotation + 0.5f - (Projectile.velocity.X > 0 ? MathHelper.PiOver2 : 0) + MathHelper.Pi;
                    Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, (rotation * Owner.gravDir));
                    Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, rotation) + Vector2.UnitY.RotatedBy(rotation) * (Projectile.width / 2 - 6);
                    Projectile.spriteDirection = Projectile.direction;
                    Owner.itemTime = Owner.itemAnimation = 2;
                    Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
                    Owner.heldProj = Projectile.whoAmI;
                    timer++;
                    if (timer >= heldDuration)
                    {
                        SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                        currentState++;
                        timer = 0;
                        goto case AIState.Flying;
                    }
                    break;
                case AIState.Flying:
                    Projectile.tileCollide = true;
                    if (Projectile.soundDelay <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.1f }, Projectile.Center);
                        Projectile.soundDelay = 7;
                    }
                    Projectile.rotation += MathHelper.PiOver4;
                    Projectile.velocity *= 0.99f;
                    if(timer > 15)
                    {
                        currentState++;
                        timer = 0;
                        goto case AIState.Returning;
                    }
                    timer++;
                    break;
                case AIState.Returning:
                    timer++;
                    Projectile.tileCollide = false;
                    if (Projectile.soundDelay <= 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.1f }, Projectile.Center);
                        Projectile.soundDelay = 7;
                    }
                    Projectile.rotation += MathHelper.PiOver4;
                    if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                    {
                        timer = 0;
                        currentState++;
                    }
                    Projectile.velocity += Vector2.Normalize(Owner.Center - Projectile.Center) * 1.5f;
                    if (Projectile.velocity.Length() > 20 || timer > 60)
                        Projectile.velocity = Vector2.Normalize(Owner.Center - Projectile.Center) * 20;
                    break;
                case AIState.ReturningHeld:
                    timer++;
                    if(timer > Projectile.oldPos.Length)
                        Projectile.Kill();

                    break;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (currentState == AIState.Flying || currentState == AIState.Returning)
            {
                Vector2 drawOrigin = new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width * 0.5f, Projectile.height * 0.5f);
                int amount = Projectile.oldPos.Length;
                if(currentState == AIState.Flying)
                    amount = (int)Math.Min(Projectile.oldPos.Length, timer);

                for (int k = 0; k < amount; k++)
                {
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length) * 0.5f;
                    Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, drawPos, null, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
                }
            }
            return currentState != AIState.ReturningHeld;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (currentState == AIState.Flying)
            {
                Projectile.velocity *= 0.8f;
                currentState = AIState.Returning;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(currentState == AIState.Flying)
            {
                AuroraIcePickAfterimage ipai = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Normalize(Projectile.oldVelocity) * 12, ModContent.ProjectileType<AuroraIcePickAfterimage>(), Projectile.damage, 0, Projectile.owner)].ModProjectile as AuroraIcePickAfterimage;
                ipai.initialRotation = Projectile.oldVelocity.ToRotation();
                ipai.initialPosition = Projectile.Center;
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                Projectile.velocity *= 0.3f;
                currentState = AIState.Returning;
            }
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 8;
            hitboxCenterFrac = new Vector2(width / Projectile.width, height / Projectile.height) / 2;
            return true;
        }
    }
    #endregion

    #region Afterimage Projectile

    public class AuroraIcePickAfterimage : ModProjectile
    {
        private float timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        internal float initialRotation;
        internal Vector2 initialPosition;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fading Aurora");
            //ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            //ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
        }
        public override void AI()
        { 
            timer++;
            float lerp1 = RadianceUtils.EaseInExponent(Math.Min(timer / 30, 1), 3);
            float lerp2 = RadianceUtils.EaseInExponent(Math.Min(timer / 30, 1), 10);
            Projectile.Center = Vector2.Lerp(initialPosition + -Projectile.velocity * 2, initialPosition + Projectile.velocity * 2, lerp1);
            Projectile.rotation = MathHelper.Lerp(initialRotation - 1f, initialRotation + 0.5f, lerp2);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Effect auroraEffect = Terraria.Graphics.Effects.Filters.Scene["Aurora"].GetShader().Shader;

            auroraEffect.Parameters["sampleTexture"].SetValue(tex);
            auroraEffect.Parameters["offset"].SetValue((float)(Main.GameUpdateCount + Projectile.whoAmI * 10) % 60 / 60);

            auroraEffect.Parameters["color1"].SetValue(new Color(77, 255, 139).ToVector4());
            auroraEffect.Parameters["color2"].SetValue(new Color(190, 98, 252).ToVector4());
            auroraEffect.Parameters["color3"].SetValue(new Color(98, 173, 252).ToVector4());


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, default, default, default, auroraEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, default, default, default, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }
    }
    #endregion
}