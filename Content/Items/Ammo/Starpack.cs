using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Core;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Items.ProjectorLenses;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Radiance.Utilities;

namespace Radiance.Content.Items.Ammo
{
    #region Main Item
    public class Starpack : BaseInstrument
    {
        public float consumeAmount = 20;
        public override float CosumeAmount 
        { 
            get => consumeAmount; 
            set => consumeAmount = value; 
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starpack");
            Tooltip.SetDefault("Consumes Radiance from cells within your inventory\nFunctions as ammo for weapons that use Fallen Stars\nExpends some Radiance on every shot");
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.maxStack = 1;
            Item.consumable = false;
            Item.value = Item.sellPrice(0, 0, 25, 0);
            Item.rare = ItemRarityID.Green;
            Item.ammo = AmmoID.FallenStar;
        }
        public override bool? CanBeChosenAsAmmo(Item weapon, Player player)
        {
            return weapon.useAmmo == AmmoID.FallenStar && player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand >= CosumeAmount;
        }
        public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
        {
            switch (weapon.type)
            {
                case ItemID.StarCannon:
                    type = ModContent.ProjectileType<StarpackRadiantStar>();
                    break;

                case ItemID.SuperStarCannon:
                    type = ModContent.ProjectileType<StarpackSuperRadiantStar>();
                    break;
            }

            speed *= 1.25f;

            if ((player.ammoCost75 && Main.rand.NextBool(4)) ||
               (player.ammoCost80 && Main.rand.NextBool(5)) ||
               (player.chloroAmmoCost80 && Main.rand.NextBool(5)) ||
               (player.huntressAmmoCost90 && Main.rand.NextBool(10)) ||
               (player.ammoPotion && Main.rand.NextBool(5)) ||
               (player.ammoBox && Main.rand.NextBool(5)))
                return;

            player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(CosumeAmount);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MeteoriteBar, 8)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddIngredient<ShimmeringGlass>(6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    #endregion

    #region Star
    public class StarpackRadiantStar : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star");
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 50;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0f && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                Projectile.ai[1] = 1f;
                Projectile.netUpdate = true;
            }
            if (Projectile.ai[1] != 0f)
                Projectile.tileCollide = true;
            if (Projectile.localAI[0] == 0f)
                Projectile.localAI[0] = 1f;
            Projectile.alpha += (int)(25f * Projectile.localAI[0]);
            if (Projectile.alpha > 200)
            {
                Projectile.alpha = 200;
                Projectile.localAI[0] = -1f;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
                Projectile.localAI[0] = 1f;
            }
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }
            Vector2 value4 = new(Main.screenWidth, Main.screenHeight);
            bool flag3 = Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value4 / 2f, value4 + new Vector2(400f)));
            if (flag3 && Main.rand.NextBool(15))
            {
                int goreID = Utils.SelectRandom(Main.rand, new int[]
                {
                    16,
                    17,
                    17,
                    17
                });
                if (Main.tenthAnniversaryWorld)
                {
                    goreID = Utils.SelectRandom(Main.rand, new int[]
                    {
                        16,
                        16,
                        16,
                        17
                    });
                }
                Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f, goreID, 1);
            }
            float strength = 0.5f;
            Lighting.AddLight(Projectile.Center, CommonColors.RadianceColor1.ToVector3() * strength);
            if (Main.rand.NextBool(5) || (Main.tenthAnniversaryWorld && Main.rand.NextBool(2)))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldCoin, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default(Color), 1.2f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D projectileTexture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle projectileRectangle = new(0, 0, projectileTexture.Width, projectileTexture.Height);
            Vector2 proejctileOrigin = projectileRectangle.Size() / 2f;
            Color alpha = Projectile.GetAlpha(lightColor);

            Texture2D extraTexture = TextureAssets.Extra[91].Value;
            Rectangle extraRectangle = extraTexture.Frame(1, 1, 0, 0, 0, 0);
            Vector2 extraOrigin = new(extraRectangle.Width / 2f, 10f);
            Vector2 value = new(0f, Projectile.gfxOffY);
            Vector2 spinningpoint = new(0f, -10f);

            float time = (float)Main.timeForVisualEffects / 45f;
            Vector2 nextPos = Projectile.Center + Projectile.velocity;

            Color color = new Color(255, 170, 0) * 0.2f;
            Color color2 = Color.White * 0.5f;
            color2.A = 0;
            float scale = 0f;

            if (Main.tenthAnniversaryWorld)
            {
                color = Color.HotPink * 0.3f;
                color2 = Color.White * 0.75f;
                color2.A = 0;
                scale = -0.1f;
            }
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            for (int i = 0; i < 3; i++)
            {
                float rotated = MathHelper.TwoPi * time + (i * (MathHelper.TwoPi / 3));
                Main.EntitySpriteDraw(extraTexture, nextPos - Main.screenPosition + value + spinningpoint.RotatedBy(rotated), new Rectangle?(extraRectangle), color, Projectile.velocity.ToRotation() + MathHelper.PiOver2, extraOrigin, 1.1f + (i * 0.15f) + scale, SpriteEffects.None, 0);
            }
            for (float num206 = 0f; num206 < 1f; num206 += 0.5f)
            {
                float num207 = time % 0.5f / 0.5f;
                num207 = (num207 + num206) % 1f;
                float num208 = num207 * 2f;
                if (num208 > 1f)
                {
                    num208 = 2f - num208;
                }
                Main.EntitySpriteDraw(extraTexture, Projectile.Center - Projectile.velocity * 0.5f - Main.screenPosition + value, new Microsoft.Xna.Framework.Rectangle?(extraRectangle), color2 * num208, Projectile.velocity.ToRotation() + MathHelper.PiOver2, extraOrigin, 0.3f + num207 * 0.5f, SpriteEffects.None, 0);
            }
            for (int j = 0; j < 3; j++)
            {
                float alpha2 = 0.2f * (j + 1);
                if (j == 2) alpha2 = 1;
                Main.EntitySpriteDraw(projectileTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(projectileRectangle), alpha * alpha2, Projectile.rotation - 0.5f * j, proejctileOrigin, Projectile.scale + 0.1f + ((2 - j) * 0.1f), spriteEffects, 0);
            }

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, lightColor.A - Projectile.alpha);
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Color color = CommonColors.RadianceColor1;
            if (Main.tenthAnniversaryWorld)
            {
                color = Color.HotPink;
                color.A /= 2;
            }
            for (int i = 0; i < 8; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldCoin, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, new Vector2?(Vector2.UnitX.RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat())) * (10 + Main.rand.NextFloat() * 4), 150, color, 1f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, new Vector2?(Vector2.UnitX.RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat()) * (6 + Main.rand.NextFloat() * 2)), 150);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            bool intersectsWithScreen = Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + screenSize / 2f, screenSize + new Vector2(400f)));
            if (intersectsWithScreen)
            {
                for (int i = 0; i < 7; i++)
                {
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * Projectile.velocity.Length(), Utils.SelectRandom(Main.rand, new int[]
                    {
                        16,
                        17,
                        17,
                        17,
                        17,
                        17,
                        17,
                        17
                    }), 1f);
                }
            }
        }
    }
    #endregion Star

    #region Super Star
    public class StarpackSuperRadiantStar : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Super Star");
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.alpha = 255;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Vector2 vector = Main.rand.NextVector2CircularEdge(200f, 200f);
            if (vector.Y < 0f)
            {
                vector.Y *= -1f;
            }
            vector.Y += 100f;
            Vector2 vector2 = vector.SafeNormalize(Vector2.UnitY) * 6f;
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center - vector2 * 20f, vector2, ModContent.ProjectileType<StarpackSuperRadiantStarSlash>(), (int)(Projectile.damage * 0.75), 0f, Projectile.owner, 0f, target.Center.Y);
        }

        public override void AI()
        {
            Projectile.alpha -= 10;
            int num = 100;
            if (Projectile.alpha < num)
            {
                Projectile.alpha = num;
            }
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.005f * Projectile.direction;
            Vector2 value = new(Main.screenWidth, Main.screenHeight);
            if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value / 2f, value + new Vector2(400f))) && Main.rand.NextBool(6))
            {
                Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f, Utils.SelectRandom(Main.rand, new int[]
                {
            16,
            17,
            17,
            17
                }), 1f);
            }
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextBool(8))
                {
                    int num2 = 228;
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, num2, 0f, 0f, 127, default(Color), 1f);
                    dust.velocity *= 0.25f;
                    dust.scale = 1.3f;
                    dust.noGravity = true;
                    dust.velocity += Projectile.velocity.RotatedBy((double)(0.3926991f * (1f - 2 * i)), default(Vector2)) * 0.2f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D projectileTexture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle projectileRectangle = new(0, 0, projectileTexture.Width, projectileTexture.Height);
            Vector2 proejctileOrigin = projectileRectangle.Size() / 2f;
            Color alpha = Projectile.GetAlpha(lightColor);

            Texture2D extraTexture = TextureAssets.Extra[91].Value;
            Rectangle extraRectangle = extraTexture.Frame(1, 1, 0, 0, 0, 0);
            Vector2 extraOrigin = new(extraRectangle.Width / 2f, 10f);
            Vector2 value = new(0f, Projectile.gfxOffY);
            Vector2 spinningpoint = new(0f, -10f);

            float time = (float)Main.timeForVisualEffects / 45f;
            Vector2 nextPos = Projectile.Center + Projectile.velocity;

            Color color = new Color(255, 170, 0) * 0.2f;
            Color color2 = Color.White * 0.5f;
            color2.A = 0;
            float scale = 0f;

            if (Main.tenthAnniversaryWorld)
            {
                color = Color.HotPink * 0.3f;
                color2 = Color.White * 0.75f;
                color2.A = 0;
                scale = -0.1f;
            }
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            for (int i = 0; i < 3; i++)
            {
                float rotated = MathHelper.TwoPi * time + (i * (MathHelper.TwoPi / 3));
                Main.EntitySpriteDraw(extraTexture, nextPos - Main.screenPosition + value + spinningpoint.RotatedBy(rotated), new Rectangle?(extraRectangle), color, Projectile.velocity.ToRotation() + MathHelper.PiOver2, extraOrigin, 1.1f + (i * 0.15f) + scale, SpriteEffects.None, 0);
            }
            for (float num206 = 0f; num206 < 1f; num206 += 0.5f)
            {
                float num207 = time % 0.5f / 0.5f;
                num207 = (num207 + num206) % 1f;
                float num208 = num207 * 2f;
                if (num208 > 1f)
                {
                    num208 = 2f - num208;
                }
                Main.EntitySpriteDraw(extraTexture, Projectile.Center - Projectile.velocity * 0.5f - Main.screenPosition + value, new Microsoft.Xna.Framework.Rectangle?(extraRectangle), color2 * num208, Projectile.velocity.ToRotation() + MathHelper.PiOver2, extraOrigin, 0.3f + num207 * 0.5f, SpriteEffects.None, 0);
            }
            for (int j = 0; j < 3; j++)
            {
                float alpha2 = 0.2f * (j + 1);
                if (j == 2) alpha2 = 1;
                Main.EntitySpriteDraw(projectileTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(projectileRectangle), alpha * alpha2, Projectile.rotation - 0.5f * j, proejctileOrigin, Projectile.scale + 0.3f + ((2 - j) * 0.3f), spriteEffects, 0);
            }

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, lightColor.A - Projectile.alpha);
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Color color = CommonColors.RadianceColor1;
            if (Main.tenthAnniversaryWorld)
            {
                color = Color.HotPink;
                color.A /= 2;
            }
            for (int i = 0; i < 15; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldCoin, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, new Vector2?(Vector2.UnitX.RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat())) * (10 + Main.rand.NextFloat() * 6), 150, color, 1f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, new Vector2?(Vector2.UnitX.RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat()) * (4 + Main.rand.NextFloat() * 8)), 150);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            bool intersectsWithScreen = Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + screenSize / 2f, screenSize + new Vector2(400f)));
            if (intersectsWithScreen)
            {
                for (int i = 0; i < 7; i++)
                {
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * Projectile.velocity.Length(), Utils.SelectRandom(Main.rand, new int[]
                    {
                        16,
                        17,
                        17,
                        17,
                        17,
                        17,
                        17,
                        17
                    }), 1f);
                }
            }
        }
    }
    #endregion Super Star

    #region Super Star Slash
    public class StarpackSuperRadiantStarSlash : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Super Star Slash");
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.scale = 1 + Main.rand.Next(30) * 0.01f;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 10 * Projectile.MaxUpdates;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void AI()
        {
            if (Projectile.alpha > 100)
                Projectile.alpha -= 10;
            if (Projectile.alpha < 100)
                Projectile.alpha = 100;

            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }
            if (Projectile.ai[0] != 0f)
            {
                Projectile.velocity = Projectile.velocity.RotatedBy((double)(Projectile.ai[0] / (10 * Projectile.MaxUpdates)));
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rectangle = new(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;
            Vector2 zero = Vector2.Zero;
            float v0 = 0f;

            int v1 = 18;
            int v2 = 0;
            int v3 = -2;
            float v4 = 1.3f;
            float v5 = 15f;
            float v6 = 0f;

            int v7 = v1;
            while ((v3 > 0 && v7 < v2) || (v3 < 0 && v7 > v2))
            {
                if (v7 < Projectile.oldPos.Length)
                {
                    Color alphaColor = Projectile.GetAlpha(lightColor);
                    float v8 = v2 - v7;
                    if (v3 < 0)
                    {
                        v8 = v1 - v7;
                    }
                    alphaColor *= v8 / (ProjectileID.Sets.TrailCacheLength[Projectile.type] * 1.5f);
                    Vector2 oldPosition = Projectile.oldPos[v7];
                    float rotation = Projectile.rotation;
                    SpriteEffects effects = spriteEffects;
                    if (oldPosition != Vector2.Zero)
                    {
                        Vector2 position3 = oldPosition + zero + Projectile.Size / 2f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                        Main.EntitySpriteDraw(texture, position3, new Microsoft.Xna.Framework.Rectangle?(rectangle), alphaColor, rotation + v0 + Projectile.rotation * v6 * (v7 - 1) * (float)(-(float)spriteEffects.HasFlag(SpriteEffects.FlipHorizontally).ToDirectionInt()), origin, MathHelper.Lerp(Projectile.scale, v4, v7 / v5), effects, 0);
                    }
                }
                v7 += v3;
            }
            Color color45 = Projectile.GetAlpha(lightColor);
            float scale = Projectile.scale;
            float rotation2 = Projectile.rotation + v0;
            Main.EntitySpriteDraw(texture, Projectile.Center + zero - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color45, rotation2, origin, scale, spriteEffects, 0);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            int num2;
            for (int num580 = 0; num580 < 10; num580 = num2 + 1)
            {
                Dust dust47 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SilverFlame, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default(Color), 1.2f);
                dust47.noGravity = true;
                ref float ptr = ref dust47.velocity.X;
                ptr *= 2f;
                num2 = num580;
            }
        }
    }
    #endregion Super Star Slash
}