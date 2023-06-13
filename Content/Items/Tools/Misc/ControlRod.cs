using Radiance.Content.Items.ProjectorLenses;
using Radiance.Content.Particles;
using Radiance.Core.Systems;
using ReLogic.Utilities;

namespace Radiance.Content.Items.Tools.Misc
{
    public class ControlRod : ModItem
    {
        public RadianceRay focusedRay;
        public bool focusedStartPoint = false;
        public bool focusedEndPoint = false;

        public float adjustedRotation;
        public const int sideBaubleSpeed = 60;
        public const int centerBaubleSpeed = 40;

        public static readonly SoundStyle HumSound = new("Radiance/Sounds/RodHumLoop");
        public static readonly SoundStyle RaySound = new("Radiance/Sounds/RayClick");
        public static SlotId HumSlot;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiance Control Rod");
            Tooltip.SetDefault("Allows you to view Radiance inputs, outputs, and rays\nLeft click to draw rays or grab existing ones\nRays without an input or output on either side will disappear");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 0, 5, 0);
            Item.rare = ItemRarityID.Blue;
            Item.useTurn = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.shootSpeed = 30;
            Item.shoot = ModContent.ProjectileType<ControlRodProjectile>();
            Item.useStyle = ItemUseStyleID.Shoot;
        }

        public override void HoldItem(Player player)
        {
            if (player == Main.LocalPlayer)
            {
                if (Main.mouseLeft && !player.IsCCd() && !player.mouseInterface && player.ItemAnimationActive)
                {
                    Vector2 mouseSnapped = new Vector2(Main.MouseWorld.X - Main.MouseWorld.X % 16 + 8, Main.MouseWorld.Y - Main.MouseWorld.Y % 16 + 8);
                    if (focusedRay == null)
                    {
                        if (RadianceRay.FindRay(mouseSnapped, out focusedRay))
                        {
                            focusedStartPoint = focusedRay.startPos == mouseSnapped;
                            focusedEndPoint = !focusedStartPoint;
                        }
                    }
                    if (focusedRay == null)
                    {
                        focusedRay = RadianceRay.NewRadianceRay(Main.MouseWorld, Main.MouseWorld);
                        focusedEndPoint = true;
                    }
                    if (focusedRay != null)
                    {
                        focusedRay.pickedUp = true;
                        focusedRay.pickedUpTimer = 5;
                        if (!RadianceRay.FindRay(mouseSnapped, out _))
                        {
                            if (focusedStartPoint)
                                MovePoint(ref focusedRay.startPos, ref focusedRay.endPos);
                            else
                                MovePoint(ref focusedRay.endPos, ref focusedRay.startPos);
                        }
                    }
                }
                else
                {
                    if (focusedRay != null && focusedRay.pickedUp)
                    {
                        focusedRay.TryGetIO(out _, out _, out bool startSuccess, out bool endSuccess);
                        if (startSuccess)
                            SpawnParticles(focusedRay.startPos);
                        if (endSuccess)
                            SpawnParticles(focusedRay.endPos);
                        focusedRay.pickedUp = false;
                    }
                    focusedRay = null;
                    focusedStartPoint = false;
                    focusedEndPoint = false;
                }
                player.GetModPlayer<RadiancePlayer>().canSeeRays = true;
            }
        }

        public static void MovePoint(ref Vector2 grabbed, ref Vector2 anchor)
        {
            Vector2 idealPosition = new Vector2(Main.MouseWorld.X - Main.MouseWorld.X % 16 + 8, Main.MouseWorld.Y - Main.MouseWorld.Y % 16 + 8);
            if (Vector2.Distance(idealPosition, anchor) > Radiance.maxDistanceBetweenPoints)
            {
                Vector2 v = Vector2.Normalize(idealPosition - anchor) * Radiance.maxDistanceBetweenPoints;
                idealPosition = anchor + v;
            }
            grabbed = Vector2.Lerp(grabbed, RadianceRay.SnapToCenterOfTile(idealPosition), 0.5f);
        }

        public static void SpawnParticles(Vector2 pos)
        {
            for (int i = 0; i < 5; i++)
            {
                SoundEngine.PlaySound(RaySound, pos);
                ParticleSystem.AddParticle(new Sparkle(pos, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2, 5), 60, 100, new Color(255, 236, 173), 0.7f));
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            adjustedRotation = rotation + RadianceUtils.SineTiming(sideBaubleSpeed) / 5;
            Texture2D RodBaubleCenterTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodCenterBauble").Value;
            Texture2D RodBaubleLeftTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodLeftBauble").Value;
            Texture2D RodBaubleRightTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodRightBauble").Value;
            Texture2D RodTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodNaked").Value;

            Vector2 drawPos = Item.Center - Main.screenPosition + Vector2.UnitY * 2;
            Main.spriteBatch.Draw(RodBaubleCenterTex, drawPos + new Vector2(9, -9.5f).RotatedBy(rotation) * (1.6f + (RadianceUtils.SineTiming(centerBaubleSpeed) / 8)), null, lightColor, rotation, RodBaubleCenterTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleLeftTex, drawPos - new Vector2(9, 9).RotatedBy(adjustedRotation) - Vector2.UnitY.RotatedBy(MathHelper.PiOver4) * 5, null, lightColor, adjustedRotation, RodBaubleLeftTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleRightTex, drawPos + new Vector2(8, 8).RotatedBy(adjustedRotation) - Vector2.UnitY.RotatedBy(MathHelper.PiOver4) * 5, null, lightColor, adjustedRotation, RodBaubleRightTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodTex, drawPos, null, lightColor, rotation, RodTex.Size() / 2, 1, SpriteEffects.None, 0);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ShimmeringGlass>(7)
                .AddTile(TileID.Anvils)
                .AddCondition(Condition.NearLava)
                .Register();
        }
    }

    public class ControlRodProjectile : ModProjectile
    {
        public float rotation;
        public const int sideBaubleSpeed = 60;
        public const int centerBaubleSpeed = 40;

        private RadianceRay ray
        {
            get
            {
                if (Main.myPlayer == Projectile.owner && RadianceUtils.GetPlayerHeldItem().ModItem as ControlRod != null)
                    return (RadianceUtils.GetPlayerHeldItem().ModItem as ControlRod).focusedRay;
                return null;
            }
        }

        public override string Texture => "Radiance/Content/Items/Tools/Misc/ControlRodNaked";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiance Control Rod");
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override bool? CanDamage() => false;
        public override void AI()
        {
            rotation = Projectile.rotation += RadianceUtils.SineTiming(sideBaubleSpeed) / 5;
            Player player = Main.player[Projectile.owner];
            Projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Size / 2f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter, true);
            if (Main.myPlayer == Projectile.owner)
            {
                bool flag2 = player.channel && !player.IsCCd();
                if (flag2)
                {
                    float scaleFactor = player.inventory[player.selectedItem].shootSpeed * Projectile.scale;
                    Vector2 vector3 = vector;
                    Vector2 value2 = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector3;
                    if (player.gravDir == -1f)
                        value2.Y = Main.screenHeight - Main.mouseY + Main.screenPosition.Y - vector3.Y;

                    Vector2 vector4 = Vector2.Normalize(value2);
                    if (float.IsNaN(vector4.X) || float.IsNaN(vector4.Y))
                        vector4 = -Vector2.UnitY;

                    vector4 *= scaleFactor;
                    if (vector4.X != Projectile.velocity.X || vector4.Y != Projectile.velocity.Y)
                        Projectile.netUpdate = true;
                    Projectile.velocity = vector4;
                }
                else
                    Projectile.Kill();
            }
            else
                Projectile.Kill();

            Lighting.AddLight(Projectile.Center + Projectile.velocity / (1.5f + (RadianceUtils.SineTiming(centerBaubleSpeed) / 8)), 0.1f, 0.15f, 0.15f);
            Lighting.AddLight(Projectile.Center + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation) / 8, 0.075f, 0.10f, 0.10f);
            Lighting.AddLight(Projectile.Center + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation) / 8, 0.1f, 0.15f, 0.15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D RodBaubleCenterTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodCenterBauble").Value;
            Texture2D RodBaubleLeftTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodLeftBauble").Value;
            Texture2D RodBaubleRightTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodRightBauble").Value;

            Main.spriteBatch.End();
            RadianceDrawing.SpriteBatchData.WorldDrawingData.BeginSpriteBatchFromTemplate(BlendState.Additive);

            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation), new Color(0, 255, 255, 20), 0.15f); //right bauble
            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation), new Color(0, 255, 255, 20), 0.15f); //left bauble
            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / (1.5f + (RadianceUtils.SineTiming(centerBaubleSpeed) / 8)), new Color(0, 255, 255, 20), 0.25f); //center bauble

            Main.spriteBatch.End();
            RadianceDrawing.SpriteBatchData.WorldDrawingData.BeginSpriteBatchFromTemplate(BlendState.AlphaBlend);

            Main.spriteBatch.Draw(RodBaubleCenterTex, Projectile.Center + Projectile.velocity / (1.5f + (RadianceUtils.SineTiming(40) / 8)) - Main.screenPosition, null, lightColor, Projectile.rotation, RodBaubleCenterTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleLeftTex, Projectile.Center - Main.screenPosition + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation), null, lightColor, rotation, RodBaubleLeftTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleRightTex, Projectile.Center - Main.screenPosition + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation), null, lightColor, rotation, RodBaubleRightTex.Size() / 2, 1, SpriteEffects.None, 0);
            // todo: player hand draws additive????
            if (Main.LocalPlayer == Main.player[Projectile.owner] && ray != null) //beam to ray points
                for (int i = 0; i < 2; i++)
                    RadianceDrawing.DrawBeam(Projectile.Center + Projectile.velocity / (1.5f + (RadianceUtils.SineTiming(40) / 8)), i == 0 ? ray.endPos : ray.startPos, new Color(0, 255, 255, 4).ToVector4(), 0.49f, 6, RadianceDrawing.SpriteBatchData.WorldDrawingData);
            return true;
        }
    }
}