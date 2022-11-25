using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Common;
using Radiance.Content.Items.ProjectorLenses;
using Radiance.Core;
using Radiance.Utils;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Radiance.Content.Items.Tools.ControlRod
{
    public class ControlRod : ModItem
    {
        public RadianceRay focusedRay;
        public bool focusedStartPoint = false;
        public bool focusedEndPoint = false;

        public float adjustedRotation;
        public const int sideBaubleSpeed = 60;
        public const int centerBaubleSpeed = 40;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiance Control Rod");
            Tooltip.SetDefault("Allows you to view Radiance inputs, outputs, and rays\nLeft click to draw rays or grab existing ones\nRays without an input or output on either side will disappear");
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(0, 0, 5, 0);
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
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer);
            ControlRodProjectile crp = Main.projectile[p].ModProjectile as ControlRodProjectile;
            crp.ray = focusedRay;
            return false;
        }
        public override void HoldItem(Player player)
        {
            if (player == Main.LocalPlayer)
            {
                if (Main.mouseLeft && !player.CCed && !player.noItems && !player.mouseInterface)
                {
                    Vector2 mouseSnapped = new Vector2((int)(Math.Floor(Main.MouseWorld.X / 16) * 16), (int)(Math.Floor(Main.MouseWorld.Y / 16) * 16)) + new Vector2(8, 8);
                    for (int i = 0; i < Radiance.maxRays; i++)
                    {
                        if (Radiance.radianceRay[i] != null && Radiance.radianceRay[i].active && !focusedStartPoint && !focusedEndPoint) //grabs existing ray at mouse
                        {
                            RadianceRay ray = Radiance.radianceRay[i];

                            if (mouseSnapped == ray.endPos)
                            {
                                focusedRay = ray;
                                focusedEndPoint = true;
                            }
                            else if (mouseSnapped == ray.startPos)
                            {
                                focusedRay = ray;
                                focusedStartPoint = true;
                            }
                        }
                    }

                    if (focusedRay == null) //creates new ray
                    {
                        int r = RadianceRay.NewRadianceRay(Main.MouseWorld, Main.MouseWorld);
                        focusedRay = Radiance.radianceRay[r];
                        focusedEndPoint = true;
                    }
                    if (focusedRay != null) //moves focused ray
                    {
                        focusedRay.pickedUp = true;
                        focusedRay.pickedUpTimer = 5;
                        int maxDist = Radiance.maxDistanceBetweenPoints;
                        if (focusedEndPoint)
                        {
                            Vector2 end = Main.MouseWorld;
                            if (Vector2.Distance(end, focusedRay.startPos) > maxDist)
                            {
                                Vector2 v = end - focusedRay.startPos;
                                v = Vector2.Normalize(v) * maxDist;
                                end = focusedRay.startPos + v;
                            }
                            if (RadianceRay.FindRay(RadianceRay.SnapToCenterOfTile(end)) == null)
                            {
                                focusedRay.SnapToPosition(focusedRay.startPos, end);
                            }
                        }
                        if (focusedStartPoint)
                        {
                            Vector2 start = Main.MouseWorld;
                            if (Vector2.Distance(start, focusedRay.endPos) > maxDist)
                            {
                                Vector2 v = start - focusedRay.endPos;
                                v = Vector2.Normalize(v) * maxDist;
                                start = focusedRay.endPos + v;
                            }
                            if (RadianceRay.FindRay(RadianceRay.SnapToCenterOfTile(start)) == null)
                            {
                                focusedRay.SnapToPosition(start, focusedRay.endPos);
                            }
                        }
                    }
                }

                else
                {
                    if(focusedRay != null)
                    {
                        focusedRay.pickedUp = false;
                    }
                    focusedRay = default;
                    focusedStartPoint = false;
                    focusedEndPoint = false;
                }
                player.GetModPlayer<RadiancePlayer>().canSeeRays = true;
            }
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            adjustedRotation = rotation + (float)RadianceUtils.sineTiming(sideBaubleSpeed) / 5;
            Texture2D RodBaubleCenterTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/ControlRod/ControlRodCenterBauble").Value;
            Texture2D RodBaubleLeftTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/ControlRod/ControlRodLeftBauble").Value;
            Texture2D RodBaubleRightTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/ControlRod/ControlRodRightBauble").Value;
            Texture2D RodTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/ControlRod/ControlRodNaked").Value;

            Vector2 drawPos = Item.Center - Main.screenPosition + Vector2.UnitY * 2;
            Main.spriteBatch.Draw(RodBaubleCenterTex, drawPos + new Vector2(9, -9.5f).RotatedBy(rotation) * (1.6f + ((float)RadianceUtils.sineTiming(centerBaubleSpeed) / 8)), null, lightColor, rotation, RodBaubleCenterTex.Size() / 2, 1, SpriteEffects.None, 0);
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
                .AddCondition(Recipe.Condition.NearLava)
                .Register();
        }
    }
    public class ControlRodProjectile : ModProjectile
    {
        public float rotation;
        public const int sideBaubleSpeed = 60;
        public const int centerBaubleSpeed = 40;
        public RadianceRay ray;
        public override string Texture => "Radiance/Content/Items/Tools/ControlRod/ControlRodNaked";
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
        public override void AI()
        {
            rotation = Projectile.rotation += (float)RadianceUtils.sineTiming(sideBaubleSpeed) / 5;
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
                bool flag2 = player.channel && !player.noItems && !player.CCed && player.HasAmmo(player.inventory[player.selectedItem]);
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

            Lighting.AddLight(Projectile.Center + Projectile.velocity / (1.5f + ((float)RadianceUtils.sineTiming(centerBaubleSpeed) / 8)), 0.1f, 0.15f, 0.15f);
            Lighting.AddLight(Projectile.Center + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation) / 8, 0.075f, 0.10f, 0.10f);
            Lighting.AddLight(Projectile.Center + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation) / 8, 0.1f, 0.15f, 0.15f);
        }
    
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D RodBaubleCenterTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/ControlRod/ControlRodCenterBauble").Value;
            Texture2D RodBaubleLeftTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/ControlRod/ControlRodLeftBauble").Value;
            Texture2D RodBaubleRightTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/ControlRod/ControlRodRightBauble").Value;

            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation), new Color(0, 255, 255, 20), 0.15f, Main.GameViewMatrix.ZoomMatrix); //right bauble
            
            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation), new Color(0, 255, 255, 20), 0.15f, Main.GameViewMatrix.ZoomMatrix); //left bauble
            
            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / (1.5f + ((float)RadianceUtils.sineTiming(centerBaubleSpeed) / 8)), new Color(0, 255, 255, 20), 0.25f, Main.GameViewMatrix.ZoomMatrix); //center bauble
  
            Main.spriteBatch.Draw(RodBaubleCenterTex, Projectile.Center + Projectile.velocity / (1.5f + ((float)RadianceUtils.sineTiming(40) / 8)) - Main.screenPosition, null, lightColor, Projectile.rotation, RodBaubleCenterTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleLeftTex, Projectile.Center - Main.screenPosition + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation), null, lightColor, rotation, RodBaubleLeftTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleRightTex, Projectile.Center - Main.screenPosition + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation), null, lightColor, rotation, RodBaubleRightTex.Size() / 2, 1, SpriteEffects.None, 0);
            if (Main.LocalPlayer == Main.player[Projectile.owner] && ray != null) //beam to ray points
                for (int i = 0; i < 2; i++)
                    RadianceDrawing.DrawBeam(Projectile.Center + Projectile.velocity / (1.5f + ((float)RadianceUtils.sineTiming(40) / 8)), i == 0 ? ray.endPos : ray.startPos, new Color(0, 255, 255, 4).ToVector4(), 0.49f, 6, Main.GameViewMatrix.ZoomMatrix);
            return true;
        }
    }
}