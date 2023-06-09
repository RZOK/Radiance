using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.Items.BaseItems;
using Radiance.Content.Particles;
using Radiance.Core;
using Radiance.Core.Interfaces;
using Radiance.Core.Systems;
using Radiance.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Radiance.Content.Tiles
{
    public class CinderCrucible : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 18 };
            HitSound = SoundID.Item52;
            DustType = -1;

            LocalizedText name = CreateMapEntryName();
            name.SetDefault("Cinder Crucible");
            AddMapEntry(new Color(219, 33, 0), name);

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<CinderCrucibleTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);
        }

        public override void HitWire(int i, int j)
        {
            RadianceUtils.ToggleTileEntity(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
            {
                Tile tile = Main.tile[i, j];
                if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
                {
                    Texture2D backTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CinderCrucibleBack").Value;
                    Texture2D mainTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CinderCrucibleMain").Value;
                    Texture2D glowTexture = ModContent.Request<Texture2D>("Radiance/Content/Tiles/CinderCrucibleGlow").Value;
                    Color tileColor = Lighting.GetColor(i, j);
                    Color glowColor = Color.White;
                    float glowModifier = Math.Min(entity.boostTime / 120f, 1);
                    Vector2 mainPosition = new Vector2(i, j) * 16 + new Vector2(entity.Width * 8, entity.Height * 16) + RadianceUtils.tileDrawingZero - Main.screenPosition;
                    Vector2 origin = new Vector2(mainTexture.Width / 2, mainTexture.Height);
                    Main.spriteBatch.Draw
                    (
                        backTexture,
                        mainPosition,
                        null,
                        tileColor,
                        0,
                        origin,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    if (entity.boostTime > 0)
                    {
                        RadianceDrawing.DrawSoftGlow(mainPosition + Main.screenPosition - Vector2.UnitY * 20, new Color(255, 50, 0) * glowModifier * 0.7f * Math.Clamp(RadianceUtils.SineTiming(50), 0.7f, 1), 0.4f, RadianceDrawing.SpriteBatchData.WorldDrawingData);
                    }
                    Main.spriteBatch.Draw
                    (
                        mainTexture,
                        mainPosition,
                        null,
                        tileColor,
                        0,
                        origin,
                        1,
                        SpriteEffects.None,
                        0
                    );
                    if (entity.boostTime > 0)
                    {
                        Main.spriteBatch.Draw
                        (
                            glowTexture,
                            mainPosition,
                            null,
                            glowColor * glowModifier,
                            0,
                            origin,
                            1,
                            SpriteEffects.None,
                            0
                        );
                    }
                }
            }
            return false;
        }

        public override bool RightClick(int i, int j)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
            {
                Item item = RadianceUtils.GetPlayerHeldItem();
                bool success = false;
                if (entity.GetSlot(0).type != item.type || entity.GetSlot(0).stack == entity.GetSlot(0).maxStack)
                    entity.DropItem(0, new Vector2(i * 16, j * 16));

                if (item.type == ItemID.Hellstone || item.type == ItemID.HellstoneBar)
                    entity.SafeInsertItemIntoSlot(0, ref item, out success);

                if (success)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                return true;
            }
            return false;
        }
        public override bool AutoSelect(int i, int j, Item item)
        {
            List<int> validItems = new List<int>() { ItemID.Hellstone, ItemID.HellstoneBar };
            return validItems.Contains(item.type);
        }
        public override void MouseOver(int i, int j)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
            {
                Main.LocalPlayer.SetCursorItem(ItemID.Hellstone);
                entity.AddHoverUI();
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (RadianceUtils.TryGetTileEntityAs(i, j, out CinderCrucibleTileEntity entity))
                entity.DropAllItems(entity.TileEntityWorldCenter());

            Point origin = RadianceUtils.GetTileOrigin(i, j);
            ModContent.GetInstance<CinderCrucibleTileEntity>().Kill(origin.X, origin.Y);
        }
    }

    public class CinderCrucibleTileEntity : ImprovedTileEntity, IInventory
    {
        public CinderCrucibleTileEntity() : base(ModContent.TileType<CinderCrucible>(), 1, true)
        {
            idealStability = 50;
        }

        public int boostTime = 0;
        public int meltingTime = 0;
        public static readonly int effectRange = 22;
        private readonly int maxBoostTime = 54000;
        public Item[] inventory { get; set; }
        public byte[] inputtableSlots => new byte[1] { 0 };
        public byte[] outputtableSlots => Array.Empty<byte>();

        public override void OrderedUpdate()
        {
            this.ConstructInventory(1);
            if (enabled)
            {
                if (!this.GetSlot(0).IsAir)
                {
                    float amountGiven = (this.GetSlot(0).type == ItemID.HellstoneBar ? 4f : 1f) * (IsStabilized ? 1 : 0.05f);
                    if (boostTime <= maxBoostTime - amountGiven)
                    {
                        meltingTime++;
                        if (meltingTime > 300)
                        {
                            boostTime += (int)(3600 * amountGiven);

                            this.GetSlot(0).stack--;
                            if (this.GetSlot(0).stack <= 0)
                                this.GetSlot(0).TurnToAir();

                            SoundEngine.PlaySound(new SoundStyle($"{nameof(Radiance)}/Sounds/CinderMelt"), this.TileEntityWorldCenter());
                            for (int i = 0; i < 36; i++)
                            {
                                Dust dust = Dust.NewDustPerfect(this.TileEntityWorldCenter() - Vector2.UnitY * 8 + Main.rand.NextVector2Circular(6, 2), DustID.LavaMoss);
                                dust.color = Color.Yellow;
                                dust.velocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), (-i - 1) / 4f * RadianceUtils.EaseInCirc(i / 36f)) * Main.rand.NextFloat(0.9f, 1.1f);
                                dust.scale = Main.rand.NextFloat(1.5f, 1.8f);
                                dust.fadeIn = Main.rand.NextFloat(1.5f, 1.8f);
                                if (i % 4 == 0)
                                {
                                    Dust smoke = Dust.NewDustPerfect(this.TileEntityWorldCenter() - Vector2.UnitY * 8 + Main.rand.NextVector2Circular(6, 2), DustID.Smoke);
                                    smoke.velocity.Y = Main.rand.NextFloat(-1.5f, -0.3f);
                                    smoke.velocity.X *= 0.5f;
                                    smoke.scale = Main.rand.NextFloat(1.45f, 1.8f);
                                    smoke.noGravity = true;
                                }
                            }
                            meltingTime = 0;
                        }
                    }
                }
                else if (meltingTime > 0)
                    meltingTime--;

                if (meltingTime > 0)
                {
                    if (meltingTime % 15 == 0)
                    {
                        if (Main.rand.NextBool(4))
                            ParticleSystem.AddParticle(new Cinder(this.TileEntityWorldCenter() - Vector2.UnitY * 6 + Main.rand.NextVector2Circular(6, 2), new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-2, -1.5f)), 60, 0, new Color(219, 33, 0), new Color(100, 100, 100), 0.4f));
                    }
                }

                if (boostTime > 0)
                {
                    if (Main.GameUpdateCount % 60 == 0)
                        ParticleSystem.AddParticle(new TreasureSparkle(this.TileEntityWorldCenter() - Vector2.UnitY * 6 + Main.rand.NextVector2Circular(10, 2), Vector2.UnitY * Main.rand.NextFloat(-0.3f, -0.2f), 300, 0, 0.4f, new Color(219, 33, 0)));

                    foreach (PedestalTileEntity item in TileEntitySystem.TileEntitySearchHard(this, effectRange).Where(x => x is PedestalTileEntity))
                    {
                        item.AddCellBoost(nameof(CinderCrucible), 0.25f);
                    }
                    boostTime--;
                }
            }
        }
        protected override HoverUIData ManageHoverUI()
        {
            List<HoverUIElement> data = new List<HoverUIElement>()
            {
                new StabilityBarElement("StabilityBar", stability, idealStability, Vector2.One * -48)
            };

            if (enabled)
                data.Add(new SquareUIElement("AoESquare", effectRange * 16 + 4, new Color(219, 33, 0)));

            if (!this.GetSlot(0).IsAir)
                data.Add(new ItemUIElement("HellstoneCount", this.GetSlot(0).type, Vector2.UnitY * -32, this.GetSlot(0).stack));

            return new HoverUIData(this, this.TileEntityWorldCenter(), data.ToArray());
        }
        public override void SaveData(TagCompound tag)
        {
            this.SaveInventory(tag);
            if (boostTime > 0)
                tag["BoostTime"] = boostTime;
        }

        public override void LoadData(TagCompound tag)
        {
            this.LoadInventory(tag, 1);
            boostTime = tag.Get<int>("BoostTime");
            base.LoadData(tag);
        }
    }

    public class CinderCrucibleItem : BaseTileItem
    {
        public CinderCrucibleItem() : base("CinderCrucibleItem", "Cinder Crucible", "Consumes Hellstone to improve the efficiency of nearby Radiance Cells on Pedestals", "CinderCrucible", 1, Item.sellPrice(0, 0, 50, 0), ItemRarityID.LightRed) { }
    }
}