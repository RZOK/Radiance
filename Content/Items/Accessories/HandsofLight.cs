using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Core;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Radiance.Content.Items.Accessories.HandsofLightHand;
using Radiance.Core.Systems;
using Radiance.Content.Particles;
using Terraria.Audio;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Radiance.Content.Items.Accessories
{
    public class HandsofLight : BaseAccessory
    {
        public override void Load()
        {
            RadiancePlayer.CanUseItemEvent += ResetBowUseTime;
            RadiancePlayer.PostUpdateEquipsEvent += UpdateTimer;
        }

        public override void Unload()
        {
            RadiancePlayer.CanUseItemEvent -= ResetBowUseTime;
            RadiancePlayer.PostUpdateEquipsEvent -= UpdateTimer;
        }

        public override string Texture => "Terraria/Images/Item_" + ItemID.ManaCrystal;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hands of Light");
            Tooltip.SetDefault("Creates apparitions of hands that will pull your bow back faster than you can");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = 0;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }
        public void UpdateTimer(Player player)
        {
            BaseAccessoryPlayer bAPlayer = player.GetModPlayer<BaseAccessoryPlayer>();
            if (player.Equipped<HandsofLight>())
            {
                List<Projectile> hands = Main.projectile.Where(x => x.active && x.type == ModContent.ProjectileType<HandsofLightHand>() && x.owner == player.whoAmI).ToList();
                if (hands.Count < 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        HandsofLightHand hand = Main.projectile[Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<HandsofLightHand>(), 0, 0, player.whoAmI, i)].ModProjectile as HandsofLightHand;
                        hand.firstLimbPosition = hand.secondLimbPosition = hand.Projectile.Center;
                    }
                }
                else
                    bAPlayer.timers[Name] = 0;

                Item item = player.GetPlayerHeldItem();
                hands.Where(x => (x.ModProjectile as HandsofLightHand).aiState == AIState.Focused).ToList().ForEach(x => (x.ModProjectile as HandsofLightHand).aiState = AIState.None); //kill me
                if ((item.useAmmo == AmmoID.Arrow || item.useAmmo == AmmoID.Stake) && player.ChooseAmmo(item) != null && hands.Any() && hands.Any(x => (x.ModProjectile as HandsofLightHand).hasArrow && (x.ModProjectile as HandsofLightHand).aiState == AIState.None))
                {
                    HandsofLightHand hand = hands.FirstOrDefault(x => (x.ModProjectile as HandsofLightHand).hasArrow && (x.ModProjectile as HandsofLightHand).aiState == AIState.None).ModProjectile as HandsofLightHand;
                    if (hand != null)
                        hand.aiState = AIState.Focused;
                }
            }
        }

        public bool ResetBowUseTime(Player player, Item item)
        {
            player.GetModPlayer<BaseAccessoryPlayer>().timers[Name] = 0;
            if ((item.useAmmo == AmmoID.Arrow || item.useAmmo == AmmoID.Stake) && player.HasAmmo(item) && Main.projectile.Any(x => x.ModProjectile is HandsofLightHand hand && hand.aiState == AIState.Focused && x.owner == player.whoAmI && x.active))
            {
                HandsofLightHand hand = Main.projectile.FirstOrDefault(x => x.ModProjectile is HandsofLightHand hand && hand.aiState == AIState.Focused && x.owner == player.whoAmI && x.active).ModProjectile as HandsofLightHand;
                if(hand != null)
                    hand.aiState = AIState.Pulling;
            }
            return true;
        }
    }

    public class HandsofLightHand : ModProjectile
    {
        public enum AIState
        {
            None,
            Focused,
            Pulling,
            Returning
        }
        public AIState aiState = AIState.None;
        public Vector2 idealPosition;
        public bool hasArrow = false;
        public float pullingTimer = 0;
        public float returnTimer = 0;
        public float arrowTimer = 0;
        public int arrowTimerMax = 180;
        public Player Owner => Main.player[Projectile.owner];
        public Vector2 firstLimbPosition;
        public float firstLimbRotation;

        public Vector2 secondLimbPosition;
        public float secondLimbRotation;
        readonly static SoundStyle bowPullSound = new SoundStyle("Radiance/Sounds/BowPull");
        public int Direction => Math.Sign((Owner.GetModPlayer<SyncPlayer>().mouseWorld - Owner.Center).X);
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }
        public override bool? CanDamage() => false;
        public override void AI()
        {
            Item bow = Owner.GetPlayerHeldItem();
            Item ammoItem = Owner.ChooseAmmo(bow);

            if (!hasArrow)
            {
                arrowTimer++;
                if (arrowTimer >= arrowTimerMax)
                {
                    for (int i = -10; i < 10; i += 2)
                    {
                        ParticleSystem.AddParticle(new Sparkle(Projectile.Center + Vector2.UnitY.RotatedBy(secondLimbRotation) * i + Main.rand.NextVector2Circular(-8, 8), Vector2.Zero, 60, 100, new Color(255, 236, 173), 0.5f + 0.2f * (1 - Math.Abs(i) / 10)));
                    }
                    SoundEngine.PlaySound(SoundID.Item156, Projectile.Center);
                    Projectile.Center += Main.rand.NextVector2CircularEdge(-32, 32);
                    hasArrow = true;
                    arrowTimer = 0;
                }
            }
            Owner.GetModPlayer<SyncPlayer>().mouseRotationListener = true;
            if (Owner.Equipped<HandsofLight>() && Owner.active && !Owner.dead && (bow.useAmmo == AmmoID.Arrow || bow.useAmmo == AmmoID.Stake))
                Projectile.timeLeft = 2;
            else
                Projectile.Kill();
            Vector2 defaultPosition = Owner.Center - Vector2.UnitY * 30 - new Vector2(64 * Direction, 0).RotatedBy((1.2f + Projectile.ai[0] * -0.5f) * Direction);
            float lerp = 0.1f;
            
            switch(aiState)
            {
                case AIState.None:
                    idealPosition = defaultPosition;
                    break;

                case AIState.Focused:
                    idealPosition = defaultPosition;
                    break;

                case AIState.Pulling:
                    idealPosition = Owner.Center + Vector2.Normalize(Owner.GetModPlayer<SyncPlayer>().mouseWorld - Owner.Center) * 128;
                    pullingTimer++;
                    if (pullingTimer > 10)
                    {
                        lerp = 0.5f;
                        idealPosition = Owner.Center - Vector2.UnitX.RotatedBy(Owner.itemRotation) * 300 * Direction;
                        hasArrow = false;
                        if (pullingTimer == 11)
                        {
                            float distance = Vector2.Distance(Projectile.Center, idealPosition) / 6;
                            for (int i = 0; i < distance - 24; i++)
                            {
                                ParticleSystem.AddParticle(new Sparkle(Vector2.Lerp(Owner.Center + Vector2.Normalize(Owner.GetModPlayer<SyncPlayer>().mouseWorld - Owner.Center) * 128, idealPosition, i / distance) + Main.rand.NextVector2Circular(-8, 8), Vector2.Normalize(idealPosition - Projectile.Center) * 12 * Main.rand.NextFloat(0.8f, 1.2f), 60, 100, new Color(255, 236, 173), 1 - i / distance));
                            }
                            Owner.itemAnimation = Owner.itemTime = 0;
                        }
                    }
                    if (pullingTimer == 15)
                    {  
                        SoundEngine.PlaySound(bowPullSound, Owner.Center);
                        aiState = AIState.Returning;
                        pullingTimer = 0;
                    }
                    break;
                case AIState.Returning:
                    returnTimer++;
                    if (returnTimer >= 90)
                    {
                        aiState = AIState.None;
                        returnTimer = 0;
                    }
                    lerp = 0.01f;
                    idealPosition = defaultPosition;
                    break;
            }

            Projectile.Center = Vector2.Lerp(Projectile.Center, idealPosition, lerp);
            Projectile.Center += new Vector2(RadianceUtils.SineTiming(50 - Projectile.ai[0] * 8), RadianceUtils.SineTiming(32 + Projectile.ai[0] * 4));
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D handTexture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D armTexture = ModContent.Request<Texture2D>("Radiance/Content/Items/Accessories/HandsofLightArm").Value;
            Vector2 firstLimbStartPosition = Owner.Center + new Vector2(16 * Direction, -16) + new Vector2(32 * Direction, 0).RotatedBy((3.5f + Projectile.ai[0] * -0.5f) * Direction);
            float A = armTexture.Width;
            float B = armTexture.Width;
            float C = Math.Min(Vector2.Distance(firstLimbStartPosition, Projectile.Center), armTexture.Width * 2);
            Vector2 diff = Projectile.Center - firstLimbStartPosition;
            float atan = (float)Math.Atan2(diff.Y, diff.X);
            float idealFirstLimbRotation = atan - RadianceUtils.AngleFromLawOfCosines(A, C, B) * Direction;

            float lerp = aiState == AIState.Pulling ? 0.8f : 0.5f;
            float rotationLerp = aiState == AIState.Pulling ? 0.8f : 0.1f;

            firstLimbPosition = Vector2.Lerp(firstLimbPosition, firstLimbStartPosition, lerp);
            firstLimbRotation = MathHelper.Lerp(firstLimbRotation, idealFirstLimbRotation, rotationLerp);

            Vector2 secondLimbStartPosition = firstLimbPosition + Vector2.UnitX.RotatedBy(firstLimbRotation) * armTexture.Width;
            secondLimbRotation = (Projectile.Center - secondLimbStartPosition).ToRotation();

            secondLimbPosition = Vector2.Lerp(secondLimbPosition, secondLimbStartPosition, lerp);

            Main.spriteBatch.Draw(armTexture, firstLimbPosition - Main.screenPosition, null, new Color(255, 255, 255, 175) * 0.7f, firstLimbRotation, Vector2.UnitY * armTexture.Height / 2, 1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(armTexture, secondLimbPosition - Main.screenPosition, null, new Color(255, 255, 255, 175) * 0.7f, secondLimbRotation, Vector2.UnitY * armTexture.Height / 2, 1, SpriteEffects.None, 0);
            
            Item bow = Owner.GetPlayerHeldItem();
            Item ammoItem = Owner.ChooseAmmo(bow);
            float handRotation = secondLimbRotation + Direction * MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0);
            if (ammoItem != null && (arrowTimer > 0 || hasArrow) && (bow.useAmmo == AmmoID.Arrow || bow.useAmmo == AmmoID.Stake))
            {
                int ammo = ammoItem.shoot;
                Main.instance.LoadProjectile(ammo);
                Main.spriteBatch.Draw(TextureAssets.Projectile[ammo].Value, Projectile.Center + Vector2.UnitX.RotatedBy(secondLimbRotation) * 4 - Main.screenPosition, null, (arrowTimer > 0 ? CommonColors.RadianceColor1 * (arrowTimer / arrowTimerMax - 0.7f) : Color.White), secondLimbRotation + (Direction == -1 ? MathHelper.Pi : 0), TextureAssets.Projectile[ammo].Size() / 2, 1, SpriteEffects.None, 0);
            }
            Rectangle frame = new Rectangle(0, 0, 30, 32);
            if (hasArrow)
                frame = new Rectangle(0, 34, 28, 28);
            Main.spriteBatch.Draw(handTexture, Projectile.Center - Main.screenPosition, frame, Color.White * 0.7f, handRotation, frame.Size() / 2, 0.9f, Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }
    }
}