//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.Audio;
//using Terraria.DataStructures;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Radiance.Content.Items.BaseItems;
//using Microsoft.Xna.Framework.Graphics;
//using Terraria.GameContent;
//using Radiance.Core;
//using System;
//using System.Collections.Generic;
//using Radiance.Core.Systems;
//using Radiance.Content.Projectiles;
//using Radiance.Utilities;

//namespace Radiance.Content.Items.Weapons.Melee
//{
//    #region Main Item
//    public class SunbracersBaton : BaseInstrument
//    {
//        public override float ConsumeAmount => 0.1f;
//        public override void SetStaticDefaults()
//        {
//            DisplayName.SetDefault("Sunbracer's Baton");
//            Tooltip.SetDefault("Placeholder Line\nFires syringes that inject enemies with Radiance until they explode\n25% chance to not consume ammo");
//            SacrificeTotal = 1;
//        }

//        public override void SetDefaults()
//        {
//            Item.damage = 22;
//            Item.width = 62;
//            Item.height = 32;
//            Item.useTime = 15;
//            Item.useAnimation = 15;
//            Item.DamageType = DamageClass.Ranged;
//            Item.useStyle = ItemUseStyleID.Shoot;
//            Item.autoReuse = true;
//            Item.UseSound = SoundID.Item108.WithPitchOffset(0.3f);
//            Item.rare = ItemRarityID.Lime;
//            Item.knockBack = 0.1f;
//            Item.noMelee = true;
//            Item.value = Item.sellPrice(0, 7, 50);
//            Item.shoot = ModContent.ProjectileType<FleshCatalyzerSyringeBullet>();
//            Item.shootSpeed = 16f;
//            Item.useAmmo = ModContent.ItemType<FleshCatalyzerSyringe>();
//        }
//        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//        {
//            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));
//            if (Collision.CanHit(position, 0, 0, position + velocity, 0, 0))
//                position += velocity * 2;
//            FleshCatalyzerSyringeBullet proj = Main.projectile[Projectile.NewProjectile(source, position, velocity, type, damage / 4, knockback, Main.myPlayer, 0, 0)].ModProjectile as FleshCatalyzerSyringeBullet;
//            proj.shotFC = Item;
//            if (player.GetModPlayer<RadiancePlayer>().currentRadianceOnHand >= ConsumeAmount)
//                proj.charged = true;
//            return false;
//        }
//        public override bool CanConsumeAmmo(Item ammo, Player player)
//        {
//            if (Main.rand.NextBool(4))
//                return false;
//            else
//                player.GetModPlayer<RadiancePlayer>().ConsumeRadianceOnHand(ConsumeAmount);
//            return true;
//        }
//        public override Vector2? HoldoutOffset()
//        {
//            return new Vector2(-9f, 0f);
//        }
//    }

//    #endregion
//}