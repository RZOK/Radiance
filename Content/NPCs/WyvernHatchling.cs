﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Tiles.CeremonialDish;
using Radiance.Core;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace Radiance.Content.NPCs
{
    public class WyvernHatchling : ModNPC
    {
        private const int length = 6;
        public float wibbleOffset = 0;
        public bool archWyvern = false;
        public CeremonialDishTileEntity home;
        public WyvernHatchlingSegment[] segments = new WyvernHatchlingSegment[length];
        public Vector2 HeadPosition => NPC.Center - new Vector2(segments[0].Width, segments[0].Height) / 2;
        public override void SetDefaults()
        {
            AIType = -1;
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            wibbleOffset = Main.rand.Next(120);
            segments = new WyvernHatchlingSegment[length];
            segments[0] = new WyvernHatchlingSegment(0, null, NPC.Center);
            archWyvern = Main.rand.NextBool(3);
            for (int i = 1; i < length; i++)
            {
                segments[i] = new WyvernHatchlingSegment((byte)i, segments[i - 1]);
            }
            NPC.width = segments[0].Width;
            NPC.height = segments[0].Height;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
                new FlavorTextBestiaryInfoElement("A newborn wyvern that has strayred from its nest in pursuit of food. Unlike the fully grown variants, the hatchlings are docile and quite playful.")
            });
        }
        public override bool CheckActive() => false;

        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if(!NPC.IsABestiaryIconDummy)
                npcHitbox = new Rectangle((int)HeadPosition.X, (int)HeadPosition.Y, segments[0].Height, segments[0].Height);
            return false;
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            if (!NPC.IsABestiaryIconDummy)
                boundingBox = new Rectangle((int)HeadPosition.X, (int)HeadPosition.Y, segments[0].Width, segments[0].Width);
        }

        #region AI
        public int timer = 0;
        public float rotation = 0;
        int meepTimer = 0;
        bool returning = false;
        int currentActionTimer = 0;
        int currentActionMax = 0;
        float currentActionCompletion => (float)currentActionTimer / (float)currentActionMax;
        public WyvernAction currentAction = WyvernAction.Nothing;
        public enum WyvernAction
        {
            Nothing,
            Twirl,
            SwoopAndTwirl,
            FeedingDash,
            ReturningLoop,
        }
        bool SetHome()
        {
            foreach (TileEntity entity in TileEntity.ByID.Values)
            {
                if (entity is CeremonialDishTileEntity dish)
                {
                    if (NPC.Distance(dish.TileEntityWorldCenter()) < 3000 && (home == null || NPC.Distance(dish.TileEntityWorldCenter()) < NPC.Distance(home.TileEntityWorldCenter())))
                        home = dish;
                }
            }
            return home != null;
        }
        bool TooFarFromHome(int x, int y, int width, int height)
        {
            if (home == null)
                return false;

            Point topLeft = home.TileEntityWorldCenter().ToPoint() - new Point(x, y);
            Rectangle flyBox = new Rectangle(topLeft.X, topLeft.Y, width, height);
            return !flyBox.Contains(NPC.Center.ToPoint());
        }
        public override void AI()
        {
            if (NPC.direction == 0)
                NPC.direction = 1;
            if (home == null || (Main.GameUpdateCount % 60 == 0 && !RadianceUtils.TryGetTileEntityAs<CeremonialDishTileEntity>(home.Position.X, home.Position.Y, out _)))
                SetHome();

            segments[0].position = NPC.Center + (Vector2.UnitX * NPC.width / 2).RotatedBy(rotation);
            segments[0].rotation = rotation;
            for (int i = 1; i < length; i++)
            {
                segments[i].position = segments[i].parent.position - new Vector2(segments[i].parent.Width, + (NPC.direction == -1 ? segments[i].frame.Y - segments[i].parent.frame.Y + segments[i].Height - segments[i].parent.Height : segments[i].parent.frame.Y - segments[i].frame.Y)).RotatedBy(segments[i].parent.rotation);
                segments[i].rotation = Utils.AngleLerp(segments[i].rotation, segments[i].parent.rotation, NPC.velocity.Length() / 30);
            }
            rotation = NPC.velocity.ToRotation();
            if (timer >= 3600)
            {
                if (Main.rand.NextBool(240))
                {
                    if (home != null && !home.CanSpawnWyverns && home.ReadyToFeed && !home.WyvernCurrentlyComingToFeed)
                        currentAction = WyvernAction.FeedingDash;
                    else
                        currentAction = (WyvernAction)Main.rand.Next(1, 3);
                    timer = 0;
                }
            }
            switch (currentAction)
            {
                case WyvernAction.Nothing:
                    returning = false;
                    timer++;
                    currentActionTimer = currentActionMax = 0;
                    Glide();
                    if (home != null && TooFarFromHome(1200, 900, 2400, 700))
                        currentAction = WyvernAction.ReturningLoop;
                    if (meepTimer == 0)
                    {
                        if (Main.rand.NextBool(600))
                        {
                            SoundStyle sound = new SoundStyle("Radiance/Sounds/WyvernSqueak");
                            sound.PitchVariance = 0.3f;
                            SoundEngine.PlaySound(sound, NPC.Center);
                            CombatText.NewText(new Rectangle((int)NPC.Center.X, (int)NPC.Center.Y, 1, 1), Color.WhiteSmoke, "Meep!", false, true);
                            meepTimer = 1200;
                        }
                    }
                    else
                        meepTimer -= 1;

                    break;
                case WyvernAction.Twirl:
                    SimpleTwirl();
                    break;
                case WyvernAction.SwoopAndTwirl:
                    SwoopAndTwirl();
                    break;
                case WyvernAction.ReturningLoop:
                    ReturningLoop();
                    break;
                case WyvernAction.FeedingDash:
                    FeedingDash();
                    break;
            }
        }
        #region Actions
        void SimpleTwirl()
        {
            currentActionMax = 195;
            Twirl(Math.Min(8, 4f + 16f * currentActionCompletion), Math.Min(0.12f, 0.08f + 0.32f * currentActionCompletion), Math.Max(0.7f, currentActionCompletion));
            currentActionTimer++;
            if (currentActionTimer >= currentActionMax)
                currentAction = WyvernAction.Nothing;
        }
        void SwoopAndTwirl()
        {
            currentActionMax = 300;
            if (currentActionCompletion < 0.15f)
            {
                float ease = RadianceUtils.EaseInExponent(currentActionCompletion >= 0.1f ? 2 - currentActionCompletion * 10 : currentActionCompletion * 10, 2);
                NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(NPC.direction * ease * 8f, -8f * RadianceUtils.EaseInExponent(ease, 2)), 0.1f);
            }
            else if (currentActionCompletion < 0.3f)
            {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * 16, 0.015f);
                NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 16, 0.015f);
            }
            else if (currentActionCompletion < 0.4f)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(RadianceUtils.EaseOutExponent(NPC.velocity.X.NonZeroSign(), 2) * 10, -16), 0.02f);
            }
            else if (currentActionCompletion < 1)
            {
                Twirl(8, 0.12f, Math.Max(0.7f, currentActionCompletion));
            }

            currentActionTimer++;
            if (currentActionTimer >= currentActionMax)
            {
                currentAction = WyvernAction.Nothing;
            }
        }
        void ReturningLoop()
        {
            if (!returning)
            {
                if (Math.Abs(NPC.Center.AngleTo(home.TileEntityWorldCenter()) - rotation) < 0.1f)
                {
                    returning = true;
                    NPC.direction = (home.TileEntityWorldCenter().X - NPC.Center.X).NonZeroSign();
                }
                else
                {
                    Twirl(3, 0.05f, Math.Min(1, (float)currentActionTimer / 60));
                    currentActionTimer++;
                }
            }
            else
            {
                Glide(NPC.AngleTo(home.TileEntityWorldCenter()));
            }

            if (home == null || (home != null && !TooFarFromHome(1000, 450, 2000, 550)))
                currentAction = WyvernAction.Nothing;
        }
        void FeedingDash()
        {
            Vector2 dishPosition = home.TileEntityWorldCenter() - Vector2.UnitY * home.Height * 8;
            float distanceToHome = NPC.Distance(dishPosition);
            if(distanceToHome < 16)
            {
                currentAction = WyvernAction.Nothing;
                meepTimer = 0;
                if(home.HasFood)
                    home.Feed(Main.rand.Next(home.GetSlotsWithItems()));
            }
            if(distanceToHome < 256)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Normalize(dishPosition - NPC.Center) * 12, 0.025f);
            }
            else
            {
                Glide(NPC.AngleTo(dishPosition));
            }
            currentActionTimer++;
        }
        #endregion

        #region Subactions
        void Glide(float? towards = null)
        {
            if(towards == null)
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.UnitX.RotatedBy(RadianceUtils.SineTiming(60 + wibbleOffset / 3, wibbleOffset) * 0.8f) * 2 * NPC.direction, 0.03f);
            else
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.UnitX.RotatedBy(towards.Value).RotatedBy(RadianceUtils.SineTiming(60 + wibbleOffset / 3, wibbleOffset) * 0.8f) * 2, 0.03f);
        }
        void Twirl(float speed, float angle, float ease)
        {
            NPC.velocity = Vector2.SmoothStep(NPC.velocity, Vector2.Normalize(NPC.velocity).RotatedBy(angle * -NPC.direction) * speed, ease);
        }
        #endregion
        #endregion
        public override bool? CanBeHitByProjectile(Projectile projectile) => false;
        public override bool CanBeHitByNPC(NPC attacker) => false;
        public override bool? CanBeHitByItem(Player player, Item item) => false;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                Texture2D tex = ModContent.Request<Texture2D>("Radiance/Content/NPCs/WyvernHatchlingSheet" + (archWyvern ? "Arch" : "")).Value;
                SpriteEffects flipped = NPC.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
                for (int i = 0; i < length; i++)
                {
                    Vector2 origin = new Vector2(segments[i].frame.Width, NPC.height / 2);
                    spriteBatch.Draw(tex, segments[i].position - screenPos, segments[i].frame, drawColor, segments[i].rotation, origin, NPC.scale, flipped, 0);
                }
                return false;
            }
            return true;
        }
    }
    public class WyvernHatchlingSegment
    {
        public Vector2 position;
        public float rotation;
        public Rectangle frame;
        public byte index;
        public int Width => frame.Width;
        public int Height => frame.Height;
        public WyvernHatchlingSegment parent;
        public WyvernHatchlingSegment(byte index, WyvernHatchlingSegment parent, Vector2? position = null)
        {
            this.index = index;
            this.parent = parent;
            switch (index)
            {
                case 0:
                    frame = new Rectangle(130, 0, 28, 18);
                    break;
                case 1:
                    frame = new Rectangle(110, 2, 18, 22);
                    break;
                case 2:
                    frame = new Rectangle(92, 2, 16, 16);
                    break;
                case 3:
                    frame = new Rectangle(72, 2, 18, 22);
                    break;
                case 4:
                    frame = new Rectangle(52, 2, 18, 16);
                    break;
                case 5:
                    frame = new Rectangle(0, 2, 50, 16);
                    break;
            }
            if (parent != null)
            {
                this.position = parent.position - Vector2.UnitX.RotatedBy(parent.rotation) * Width;
                rotation = parent.rotation;
            }
            else if (position != null)
            {
                this.position = position.Value;
                rotation = 0;
            }
        }
    }
}