﻿using Radiance.Content.Items.ProjectorLenses;
using Radiance.Core.Systems;
using ReLogic.Utilities;

namespace Radiance.Content.Items.Tools.Misc
{
    public class ControlRod : ModItem
    {
        public bool focusedStartPoint = false;
        public bool focusedEndPoint = false;

        public float adjustedRotation;
        public const int sideBaubleSpeed = 60;
        public const int centerBaubleSpeed = 40;

        public static readonly SoundStyle HumSound = new("Radiance/Sounds/RodHumLoop");
        public static SlotId HumSlot;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Radiance Control Rod");
            Tooltip.SetDefault("Allows you to view Radiance inputs, outputs, and rays\nLeft click to draw new rays or grab existing ones\nRays without an input or output on either side will disappear");
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
                ref RadianceRay focusedRay = ref player.GetModPlayer<RadianceControlRodPlayer>().focusedRay;
                if (Main.mouseLeft && !player.IsCCd() && !player.mouseInterface && player.ItemAnimationActive && Main.hasFocus)
                {
                    Point16 mouseTile = Main.MouseWorld.ToTileCoordinates16();
                    if (focusedRay is null)
                    {
                        if (RadianceRay.FindRay(mouseTile, out focusedRay))
                        {
                            focusedStartPoint = focusedRay.startPos == mouseTile;
                            focusedEndPoint = !focusedStartPoint;

                            RadianceTransferSystem.shouldUpdateRays = true;
                        }
                        else
                        {
                            focusedRay = RadianceRay.NewRay(mouseTile, mouseTile);
                            RadianceTransferSystem.byPosition[focusedRay.startPos] = focusedRay;
                            focusedEndPoint = true;
                        }
                        focusedRay.focusedPlayerIndex = player.whoAmI;
                    }
                    focusedRay.outputTE = null;
                    focusedRay.inputTE = null;
                    focusedRay.pickedUpTimer = 5;
                    focusedRay.visualTimer = RadianceRay.VISUAL_TIMER_MAX;
                    focusedRay.disappearing = false;
                    if (!RadianceRay.FindRay(mouseTile, out _)) // if there's no ray at the attempted point to move to
                    {
                        if (focusedStartPoint)
                            MovePoint(focusedRay, ref focusedRay.startPos, ref focusedRay.endPos);
                        else
                            MovePoint(focusedRay, ref focusedRay.endPos, ref focusedRay.startPos);
                    }
                }
                else
                {
                    if (focusedRay is not null && focusedRay.PickedUp)
                        focusedRay.PlaceRay();

                    focusedRay = null;
                    focusedStartPoint = false;
                    focusedEndPoint = false;
                }
                player.GetModPlayer<RadianceInterfacePlayer>().canSeeRays = true;
            }
        }

        public static void MovePoint(RadianceRay ray, ref Point16 grabbed, ref Point16 anchor)
        {
            Point16 mouseCoords = Main.MouseWorld.ToTileCoordinates16();
            if(grabbed != mouseCoords)
                RadianceTransferSystem.byPosition.Remove(grabbed);

            grabbed = mouseCoords;
            if (Vector2.Distance(Main.MouseWorld, anchor.ToWorldCoordinates()) > RadianceRay.maxDistanceBetweenPoints)
            {
                Point16 v = (Vector2.Normalize(Main.MouseWorld - anchor.ToWorldCoordinates()) * RadianceRay.maxDistanceBetweenPoints).ToTileCoordinates16();
                grabbed = anchor + v;
            }
            RadianceTransferSystem.byPosition[anchor] = ray;
            RadianceTransferSystem.byPosition[grabbed] = ray;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            adjustedRotation = rotation + SineTiming(sideBaubleSpeed) / 5;
            Texture2D RodBaubleCenterTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodCenterBauble").Value;
            Texture2D RodBaubleLeftTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodLeftBauble").Value;
            Texture2D RodBaubleRightTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodRightBauble").Value;
            Texture2D RodTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodNaked").Value;

            Vector2 drawPos = Item.Center - Main.screenPosition + Vector2.UnitY * 2;
            Main.spriteBatch.Draw(RodBaubleCenterTex, drawPos + new Vector2(9, -9.5f).RotatedBy(rotation) * (1.6f + (SineTiming(centerBaubleSpeed) / 8)), null, lightColor, rotation, RodBaubleCenterTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleLeftTex, drawPos - new Vector2(9, 9).RotatedBy(adjustedRotation) - Vector2.UnitY.RotatedBy(PiOver4) * 5, null, lightColor, adjustedRotation, RodBaubleLeftTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleRightTex, drawPos + new Vector2(8, 8).RotatedBy(adjustedRotation) - Vector2.UnitY.RotatedBy(PiOver4) * 5, null, lightColor, adjustedRotation, RodBaubleRightTex.Size() / 2, 1, SpriteEffects.None, 0);
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

    public class RadianceControlRodPlayer : ModPlayer
    {
        public RadianceRay focusedRay;

        public override void UpdateDead()
        {
            focusedRay = null;
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
                if (Main.myPlayer == Projectile.owner && GetPlayerHeldItem().ModItem as ControlRod != null)
                    return Main.LocalPlayer.GetModPlayer<RadianceControlRodPlayer>().focusedRay;
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
            Player player = Main.player[Projectile.owner];
            rotation = Projectile.rotation + SineTiming(sideBaubleSpeed) / 5;
            Projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Size / 2f;
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver4;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
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

            Lighting.AddLight(Projectile.Center + Projectile.velocity / (1.5f + (SineTiming(centerBaubleSpeed) / 8)), 0.1f, 0.15f, 0.15f);
            Lighting.AddLight(Projectile.Center + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation) / 8, 0.075f, 0.10f, 0.10f);
            Lighting.AddLight(Projectile.Center + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation) / 8, 0.1f, 0.15f, 0.15f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D RodBaubleCenterTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodCenterBauble").Value;
            Texture2D RodBaubleLeftTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodLeftBauble").Value;
            Texture2D RodBaubleRightTex = ModContent.Request<Texture2D>("Radiance/Content/Items/Tools/Misc/ControlRodRightBauble").Value;

            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation), new Color(0, 255, 255, 20), 0.15f); //right bauble
            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation), new Color(0, 255, 255, 20), 0.15f); //left bauble
            RadianceDrawing.DrawSoftGlow(Projectile.Center + Projectile.velocity / (1.5f + (SineTiming(centerBaubleSpeed) / 8)), new Color(0, 255, 255, 20), 0.25f); //center bauble

            Main.spriteBatch.Draw(RodBaubleCenterTex, Projectile.Center + Projectile.velocity / (1.5f + (SineTiming(40) / 8)) - Main.screenPosition, null, lightColor, Projectile.rotation, RodBaubleCenterTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleLeftTex, Projectile.Center - Main.screenPosition + Projectile.velocity / 5 - new Vector2(8, 8).RotatedBy(rotation), null, lightColor, rotation, RodBaubleLeftTex.Size() / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(RodBaubleRightTex, Projectile.Center - Main.screenPosition + Projectile.velocity / 5 + new Vector2(8, 8).RotatedBy(rotation), null, lightColor, rotation, RodBaubleRightTex.Size() / 2, 1, SpriteEffects.None, 0);

            if (Main.LocalPlayer == Main.player[Projectile.owner] && ray != null) //beam to ray points
                for (int i = 0; i < 2; i++)
                    RadianceDrawing.DrawBeam(Projectile.Center + Projectile.velocity / (1.5f + (SineTiming(40) / 8)), i == 0 ? ray.visualEndPosition : ray.visualStartPosition, new Color(0, 255, 255, 4), 6);
            return true;
        }
    }
    public class ControlRodBuilderToggle : BuilderToggle
    {
        public override bool Active() => Main.LocalPlayer.HasItem(ModContent.ItemType<ControlRod>()) || Main.LocalPlayer.HasItem(ModContent.ItemType<MultifacetedLens>());
        public override string Texture => $"{nameof(Radiance)}/Content/Items/Tools/Misc/ControlRod_BuilderToggle";
        public override string HoverTexture => $"{nameof(Radiance)}/Content/Items/Tools/Misc/ControlRod_BuilderToggle_Outline";

        public override string DisplayValue()
        {
            string text = "Ray Direction Clarity: ";
            string[] textMessages = new[] { "Off", "On" };

            return text + textMessages[CurrentState];
        }

        public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
        {
            switch(CurrentState)
            {
                case 0:
                    drawParams.Color = Color.Gray;
                    break;
                case 1:
                    drawParams.Color = Color.White;
                    break;
            }
            return true;
        }
    }
}